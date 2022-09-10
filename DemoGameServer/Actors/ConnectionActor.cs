using Proto;
using Microsoft.AspNetCore.SignalR;
using DemoGameServer.Hubs;
using Proto.Cluster;

namespace DemoGameServer.Actors
{
    public class ConnectionActor : IActor
    {
        private readonly string _connectionId;
        private readonly IHubContext<GameHub> _hubContext;
        private string _name = String.Empty;
        public ConnectionActor(string connectionId, IHubContext<GameHub> hubContext)
        {
            _connectionId = connectionId;
            _hubContext = hubContext;
        }

        public async Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case Started:
                    await WhenActorStartAsync(context);
                    break;
                case Stopping:
                    await WhenActorStopAsync(context);
                    break;
                case PlayerJoinGameRequest playerJoinGameRequest:
                    await PlayerJoinGameRequestHandlerAsync(playerJoinGameRequest,context);
                    break;
                case PlayerLeaveGameRequest playerLeaveGameRequest:
                    await PlayerLeaveGameRequestHandlerAsync(context);
                    break;
                case GetGameStateRequest getGameStateRequest:
                    await GetGameStateRequestHandlerAsync(context);
                    break;
                case PlayerJoinedGame joined:
                    await PlayerJoinedGameAsync(joined);
                    break;
                case PlayerLeftGame left:
                    await PlayerLeftGameAsync(left);
                    break;
                case PlayerShutRequest playerShutRequest:
                    await PlayerShutRequest(playerShutRequest,context);
                    break;
                case PlayerStateChanged playerStateChanged:
                    await ChangedPlayerState(playerStateChanged);
                    break;
                case PlayerRejoinRequest playerRejoinRequest:
                    await PlayerRejoinRequestHandler(context);
                    break;
              
                case PlayerDrinkElixirRequest playerEatElixir:
                    await PlayerDrinkElixirRequestHandler(context);
                    break;
                default:
                    break;
            }
        }


        private async Task WhenActorStartAsync(IContext context)
        {
           
        }
        private async Task WhenActorStopAsync(IContext context)
        {
            if (String.IsNullOrEmpty(_name)) return;
            var playerGrain = context.Cluster().GetPlayerGrain(_name);
            await playerGrain.LeaveGame(CancellationToken.None);
        }

        private async Task PlayerJoinGameRequestHandlerAsync(PlayerJoinGameRequest playerJoinGameRequest,IContext context)
        {
            _name = playerJoinGameRequest.Username;
            var playerGrain = context.Cluster().GetPlayerGrain(playerJoinGameRequest.Username);
            await playerGrain.JoinGame(new PlayerJoinGameRequest()
            {
                Username = _name,
                Address = context.Self.Address,
                Id = context.Self.Id
            },CancellationToken.None);
        }
        private async Task PlayerLeaveGameRequestHandlerAsync(IContext context)
        {
            if (String.IsNullOrEmpty(_name)) return;
            var playerGrain = context.Cluster().GetPlayerGrain(_name);
            await playerGrain.LeaveGame(CancellationToken.None);
        }
        private async Task PlayerShutRequest(PlayerShutRequest request,IContext context)
        {
            var playerGrain = context.Cluster().GetPlayerGrain(_name);
         
            await playerGrain.Shut(new PlayerShutRequest()
            {
                Username=request.Username
            },CancellationToken.None);
        }
        private async Task ChangedPlayerState(PlayerStateChanged playerState)
        {
            await _hubContext.Clients.Client(_connectionId).SendAsync("ChangedPlayerState", playerState);
        }
        private async Task PlayerRejoinRequestHandler(IContext context)
        {
            var playerGrain = context.Cluster().GetPlayerGrain(_name);
            await playerGrain.ReJoinGame(CancellationToken.None);
        }
       
        private async Task PlayerDrinkElixirRequestHandler(IContext context)
        {
            if (String.IsNullOrEmpty(_name)) return;
            var playerGrain = context.Cluster().GetPlayerGrain(_name);
            await playerGrain.DrinkElixir(CancellationToken.None);
        }
      
        private async Task GetGameStateRequestHandlerAsync(IContext context)
        {
            if (String.IsNullOrEmpty(_name)) return;
            var playerGrain = context.Cluster().GetPlayerGrain(_name);
            var states=await playerGrain.RequestGameState(CancellationToken.None);
            await _hubContext.Clients.Client(_connectionId).SendAsync("States", states);
        }
        private async Task PlayerJoinedGameAsync(PlayerJoinedGame joined)
        {
            await _hubContext.Clients.Client(_connectionId).SendAsync("PlayerJoined", joined);
        }
        private async Task PlayerLeftGameAsync(PlayerLeftGame left)
        {
            await _hubContext.Clients.Client(_connectionId).SendAsync("PlayerLeft", left.Username);
        }
        
    }
}