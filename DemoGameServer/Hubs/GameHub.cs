using Microsoft.AspNetCore.SignalR;
using Proto;
using DemoGameServer.Actors;

namespace DemoGameServer.Hubs
{
    public class GameHub: Hub
    {
        private readonly IHubContext<GameHub> _eventsHubContext;
        private readonly ActorSystem _actorSystem;

        public GameHub(ActorSystem actorSystem,IHubContext<GameHub> eventsHubContext)
        {
            _eventsHubContext = eventsHubContext;
            _actorSystem = actorSystem;
        }
        private PID? ConnectionPID
        {
            get => Context.Items["connection-pid"] as PID;
            set => Context.Items["connection-pid"] = value;
        }
        public override async Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            ConnectionPID = _actorSystem.Root.Spawn(Props.FromProducer(() => new ConnectionActor(connectionId, _eventsHubContext)));
        }
        public void Join(string username)
        {
            if (ConnectionPID != null)
            {
               _actorSystem.Root.Send(ConnectionPID, new PlayerJoinGameRequest()
               {
                   Username = username
               });
            }
        }
        public void Left()
        {
            if (ConnectionPID != null)
            {
                _actorSystem.Root.Send(ConnectionPID, new PlayerLeftGameRequest());
            }
        }
        public void Rejoin()
        {
            if (ConnectionPID != null)
            {
                _actorSystem.Root.Send(ConnectionPID, new PlayerRejoinRequest());
            }
        }
        public void DrinkElixir()
        {
            if (ConnectionPID != null)
            {
                _actorSystem.Root.Send(ConnectionPID, new PlayerDrinkElixirRequest());
            }
        }
        public void Shut(string username) {

            if (String.IsNullOrEmpty(username)) return;
            if (ConnectionPID != null)
            {
                _actorSystem.Root.Send(ConnectionPID, new PlayerShutRequest()
                {
                    Username=username
                });
            }
        }
        public void GetGameState()
        {
            if (ConnectionPID == null) return;
            
            _actorSystem.Root.Send(ConnectionPID, new GetGameStateRequest());
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (ConnectionPID != null)
                await _actorSystem.Root.StopAsync(ConnectionPID);
        }
    }
}
