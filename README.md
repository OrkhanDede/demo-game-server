# Experimental Distributed Game Server Architecture

This project is an experimental distributed game server architecture designed to create a robust multiplayer game server system where players can interact in real-time. 

## Project Goal

The primary objective of this project is to build a distributed multiplayer game server system. We aim to provide a scalable and reliable infrastructure for online multiplayer games, enabling real-time interactions between players.

## Key Features

- Real-time distributed multiplayer gameplay with the ability for players to shoot at each other.
- In-game chat functionality, allowing players to communicate with each other in real-time.
- Player health management: If a player is low on health, they can increase their health by drinking elixir.
- Elixir System: Elixirs have a limited expiration time. If a player doesn't drink an elixir in time, the elixir will be destroyed.

## Technologies Used

- [Proto.Actor](https://github.com/asynkron/protoactor-dotnet): Proto.Actor is a powerful framework for building highly concurrent, distributed, and fault-tolerant systems. It's a fundamental part of the architecture, providing the scalability and resilience required for multiplayer gaming.

- [SignalR](https://github.com/dotnet/aspnetcore/tree/main/src/SignalR): SignalR is used to facilitate real-time communication between the game server and players' clients. It ensures that chat messages and game updates are delivered instantly.

**Build and Run**:
- requirements
   - .NET 6.
   - Visual studio 2019 +
   - To run a client application, see [Client repo](https://github.com/OrkhanDede/demo-game.client)

## Getting Started

To get started with this project, please refer to the project's documentation and installation instructions. You can find more information on how to set up the server and start playing the game.

## Client side of project

 [Client repo](https://github.com/OrkhanDede/demo-game.client)




