using Proto;
using Proto.Cluster;
using DemoGameServer.Actors;

namespace DemoGameServer.Grains
{
  
    public class PlayerGrain : PlayerGrainBase
    {
        private readonly ClusterIdentity _clusterIdentity;
        private PlayerIdentityInfo? _playerIdentityInfo;
        private int _life = 10;
        private int _point = 0;
        private PlayerStatus _status = PlayerStatus.Default;
        private bool _canDrinkElixir = false;
        private PID? _elixirActorPid=null;
        public PlayerGrain(IContext context, ClusterIdentity clusterIdentity) : base(context)
        {
            _clusterIdentity = clusterIdentity;
            Console.WriteLine($"{_clusterIdentity.Identity}: created");
        }

        public override async Task JoinGame(PlayerJoinGameRequest request)
        {
            _playerIdentityInfo = new() { Address = request.Address, Id = request.Id, Username = request.Username };

            if(_status== PlayerStatus.GameOver||_status==PlayerStatus.Default)
            {
                _life= 10;
                _point = 0;
                _canDrinkElixir= false;
            }
            _status = PlayerStatus.OnGame;

            var gameGrain = Context.Cluster().GetGameGrain(GameGrain.Default);
                await gameGrain.JoinGame(new ()
                {
                    Identity=_playerIdentityInfo,
                    State= new PlayerStateInfo()
                    {
                        Life = _life,
                        Point = _point,
                        Username = _playerIdentityInfo?.Username,
                        Status = _status,
                        CanDrinkElixir=_canDrinkElixir
                    }
                },CancellationToken.None);
        }

        public override async Task LeaveGame()
        {
                var gameGrain = Context.Cluster().GetGameGrain(GameGrain.Default);
                await gameGrain.LeaveGame(new ()
                {
                    Identity = _playerIdentityInfo,
                }, CancellationToken.None);
            _status = PlayerStatus.Default;
        }

        public override async Task ReJoinGame()
        {
            _status = PlayerStatus.OnGame;
            _life = 10;
            _point = 0;
            _canDrinkElixir = false;
            await SendMessageToGamePlayerState();
        }

        public override async Task<GetGameStateResponse> RequestGameState()
        {
            var gameGrain = Context.Cluster().GetGameGrain(GameGrain.Default);
            var state=await gameGrain.GetState(CancellationToken.None);
            return state!;
        }

        public override async Task Shut(PlayerShutRequest request)
        {
            if (_status != PlayerStatus.OnGame)
            {
                return;
            }
            var shutedPlayerGrain = Context.Cluster().GetPlayerGrain(request.Username);
            await shutedPlayerGrain.Shuted(CancellationToken.None);
            
            if (_point >= 0) _point++;

            SetCanDrinkElixir();

            await SendMessageToGamePlayerState();
        }

        public override async Task Shuted()
        {
            if (_status == PlayerStatus.GameOver) return;

            if (_life > 0) _life--;
            if (_life == 0)
            {
                _status = PlayerStatus.GameOver;
                _canDrinkElixir = false;
            }
            else
                SetCanDrinkElixir();

            await SendMessageToGamePlayerState();
        }
        public override async Task DrinkElixir()
        {
            if (_canDrinkElixir)
            {
                _life = 10;
                _point -= 5;
                if (_point < 0) _point = 0;

                if (_elixirActorPid != null)
                {
                    await Context.StopAsync(_elixirActorPid);
                    _elixirActorPid = null;
                    _canDrinkElixir = false;
                }
                await SendMessageToGamePlayerState();
            }

        }
        private void SetCanDrinkElixir()
        {
            if (_point != 0 &&
                _point % 5 == 0 &&
                _life < 10 &&
                _status != PlayerStatus.GameOver&&
                !_canDrinkElixir)
            {
             
                if(_elixirActorPid == null)
                {
                    _canDrinkElixir = true;
                    var props = Props.FromProducer(() => new ElixirActor(_clusterIdentity.Identity));
                    _elixirActorPid = Context.Spawn(props);
                }
            }
        }

        private async Task SendMessageToGamePlayerState()
        {
            var gameGrain = Context.Cluster().GetGameGrain(GameGrain.Default);

            await gameGrain.ChangePlayerState(new PlayerStateChange()
            {
                State = new PlayerStateInfo()
                {
                    Username = _playerIdentityInfo?.Username,
                    Life = _life,
                    Point = _point,
                    Status= _status,
                    CanDrinkElixir=_canDrinkElixir
                }
            }, CancellationToken.None);
        }

        public override async Task SetFalseCanDrinkElixir()
        {
            if (_elixirActorPid != null)
            {
                Context.Stop(_elixirActorPid);
                _elixirActorPid = null;
                _canDrinkElixir = false;
            }
            await SendMessageToGamePlayerState();
        }
    }
}
