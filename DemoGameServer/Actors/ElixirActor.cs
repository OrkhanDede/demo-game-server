using Proto;
using Proto.Cluster;
using Proto.Timers;

namespace DemoGameServer.Actors
{
    public class ElixirActor : IActor
    {
        private readonly string _name;
        private CancellationTokenSource? _cancellationTokenSource = null;
        public ElixirActor(string playerName)
        {
            _name = playerName;
            Console.WriteLine($"Elixir created:" + playerName);
        }
        public async Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case Started:
                    _cancellationTokenSource = context.Scheduler().SendOnce(TimeSpan.FromMilliseconds(11000),
                        context.Self, new SetFalseCanDrinkMessage());
                    break;
                case Stopped:
                    if (_cancellationTokenSource != null)
                    {
                        _cancellationTokenSource.Cancel();
                        _cancellationTokenSource = null;
                    }
                    break;
                case SetFalseCanDrinkMessage setFalseCanDrinkMessage:
                    await SetFalseCanDrinkElixir(context);
                    break;
                default:
                    break;
            }
        }
        private async Task SetFalseCanDrinkElixir(IContext context)
        {
                var player = context.Cluster().GetPlayerGrain(_name);
                await player.SetFalseCanDrinkElixir(CancellationToken.None);
        }

        private class SetFalseCanDrinkMessage
        {
        }
    }
}
