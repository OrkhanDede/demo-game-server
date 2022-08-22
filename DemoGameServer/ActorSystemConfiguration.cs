using Proto;
using Proto.Cluster;
using Proto.Cluster.Partition;
using Proto.Cluster.Testing;
using Proto.DependencyInjection;
using Proto.Remote;
using Proto.Remote.GrpcNet;
using DemoGameServer.Grains;

namespace DemoGameServer
{
    public static class ActorSystemConfiguration
    {
        public static void AddActorSystem(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton(provider =>
            {
                var actorSystemConfig = ActorSystemConfig.Setup();
                var remoteConfig = GrpcNetRemoteConfig.
                                    BindToLocalhost().
                                    WithProtoMessages(MessagesReflection.Descriptor);


                var clusterConfig = ClusterConfig.Setup(clusterName: "ProtoClusterTutorial",
                    clusterProvider: new TestProvider(new TestProviderOptions(), new InMemAgent()),
                    identityLookup: new PartitionIdentityLookup())

                     .WithClusterKind(
                        kind: PlayerGrainActor.Kind,
                        prop: Props.FromProducer(() =>
                            new PlayerGrainActor(
                                (context, clusterIdentity) => new PlayerGrain(context, clusterIdentity)
                            )
                        )
                    )
                     .WithClusterKind(
                        kind: GameGrainActor.Kind,
                        prop: Props.FromProducer(() =>
                            new GameGrainActor(
                                (context, clusterIdentity) => new GameGrain(clusterIdentity, context)
                            )
                        )
                    );


                return new ActorSystem(actorSystemConfig)
                .WithServiceProvider(provider)
                .WithRemote(remoteConfig)
                .WithCluster(clusterConfig);

            });
        }
    }
}
