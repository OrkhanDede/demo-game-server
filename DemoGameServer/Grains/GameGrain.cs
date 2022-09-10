using Proto;
using Proto.Cluster;
using Proto.Cluster.Seed;

namespace DemoGameServer.Grains
{
    public class GameGrain : GameGrainBase
    {
        public const string Default = "DefaultGameInstance";
        private readonly ClusterIdentity _clusterIdentity;
        private  Dictionary<string,PlayerInfo>_players=new ();
        public GameGrain(ClusterIdentity clusterIdentity, IContext context) : base(context)
        {
            _clusterIdentity = clusterIdentity;
            Console.WriteLine($"{_clusterIdentity.Identity}: created");
        }

        public override Task ChangePlayerState(PlayerStateChange request)
        {
            if (_players.ContainsKey(request.State.Username))
            {
                _players.TryGetValue(request.State.Username, out PlayerInfo playerInfo);

                _players.Remove(request.State.Username);

                playerInfo!.State = request.State;
                _players.Add(request.State.Username, playerInfo);

             _players.Values.ToList().ForEach(p => Context.
              Send(new PID(p.Identity.Address, p.Identity.Id), new PlayerStateChanged()
              {
                  State = playerInfo.State
              }));
            }
            return Task.CompletedTask;
        }

        public override Task<GetGameStateResponse> GetState()
        {
            var playersState = _players.Values.Select(x => x.State).ToList();

            return Task.FromResult(new GetGameStateResponse
            {
                Players = { playersState }
            });
        }

        public override Task JoinGame(PlayerJoinGame request)
        {
            if (!_players.ContainsKey(request.Identity.Username))
            {
                var playerInfo = new PlayerInfo()
                {
                    Identity = request.Identity,
                    State = request.State,
                };
                _players.Add(request.Identity.Username, playerInfo);
               
               _players.Values.ToList().ForEach(p => Context.
               Send(new PID(p.Identity.Address, p.Identity.Id), new PlayerJoinedGame()
               {
                   Player = playerInfo.State,

               }));
            }
            return Task.FromResult(new JoinResponse());
        }

        public override Task LeaveGame(PlayerLeaveGame request)
        {
            if (_players.ContainsKey(request.Identity.Username))
            {
                _players.Remove(request.Identity.Username);
                _players.Values.ToList().ForEach(p => Context.
               Send(new PID(p.Identity.Address, p.Identity.Id), new PlayerLeftGame()
               {
                   Username = request.Identity.Username,
               }));
            }
            return Task.FromResult(new JoinResponse());
        }
    }
   
}
