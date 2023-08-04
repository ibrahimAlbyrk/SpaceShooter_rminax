using System;
using Mirror;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Network.Managers.Room
{
    using Scenes;
    
    public class SpaceRoomManager : NetIdentity
    {
        #region Singleton

        private static readonly object padlock = new();
        
        private static SpaceRoomManager instance;

        public static SpaceRoomManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<SpaceRoomManager>();
                }

                return instance;
            }
        }
        
        #endregion
        
        public static event Action<NetworkConnectionToClient> OnServerJoinedClient;
        
        public static event Action OnClientCreatedRoom;

        private readonly List<Room> _rooms = new();

        [SyncVar(hook = nameof(OnChangedRoomCount))]
        private int _roomCount;

        public void OnChangedRoomCount(int _, int newCount)
        {
            
        }
        
        public IEnumerable<Room> GetRooms() => _rooms.ToList();

        public Room GetPlayersRoom(NetworkConnection conn)
        {
            return _rooms.FirstOrDefault(room => room._connections.Any(connection => connection == conn));
        }

        #region Request Methods

        [ClientCallback]
        public static void RequestCreateRoom(RoomInfo roomInfo)
        {
            var serverRoomMessage = new ServerRoomMessage(ServerRoomState.Create, roomInfo);

            NetworkClient.Send(serverRoomMessage);
        }

        [ClientCallback]
        public static void RequestJoinRoom(string roomName)
        {
            var roomInfo = new RoomInfo { Name = roomName };

            var serverRoomMessage = new ServerRoomMessage(ServerRoomState.Join, roomInfo);

            NetworkClient.Send(serverRoomMessage);
        }

        [ClientCallback]
        public static void RequestExitRoom()
        {
            var serverRoomMessage = new ServerRoomMessage(ServerRoomState.Exit, default);

            NetworkClient.Send(serverRoomMessage);
        }

        #endregion

        #region Room Methods

        [ServerCallback]
        public void CreateRoom(RoomInfo roomInfo) => CreateRoom(null, roomInfo);

        [ServerCallback]
        public void CreateRoom(NetworkConnection conn, RoomInfo roomInfo = default)
        {
            var roomName = roomInfo.Name;
            var maxPlayers = roomInfo.MaxPlayers;

            if (_rooms.Any(room => room.RoomName == roomName)) return;

            var onServer = conn is null;

            var room = new Room(roomName, maxPlayers,onServer);

            _rooms.Add(room);
            
            //If it is a client, add in to the room
            if (!onServer)
            {
                SpaceSceneManager.Instance.LoadScene(roomInfo.SceneName, LoadSceneMode.Additive,
                    scene =>
                    {
                        room.Scene = scene;
                        
                        room.AddConnection(conn);

                        SendRoomMessage(conn, ClientRoomState.Created, conn.connectionId, roomInfo.SceneName);
                    });
                return;
            }
            
            SpaceSceneManager.Instance.LoadScene(roomInfo.SceneName, LoadSceneMode.Additive,
                scene => room.Scene = scene);
        }

        [ServerCallback]
        public void RemoveRoom(string roomName)
        {
            var room = _rooms.FirstOrDefault(room => room.RoomName == roomName);

            if (room == null) return;

            var removedConnections = room.RemoveAllConnections();

            _rooms.Remove(room);

            var roomScene = room.Scene;
            
            SpaceSceneManager.Instance.UnLoadScene(roomScene);

            removedConnections.ForEach(connection => SendRoomMessage(connection, ClientRoomState.Removed, connection.connectionId, roomName));
        }

        [ServerCallback]
        public void JoinRoom(NetworkConnectionToClient conn, string roomName)
        {
            var room = _rooms.FirstOrDefault(r => r.RoomName == roomName);
            
            print(room?.Scene.name);

            if (room == null) // Handle room not found.
            {
                SendRoomMessage(conn, ClientRoomState.Fail, conn.connectionId);
                return;
            }

            if (room.MaxPlayers <= room.CurrentPlayers) // Handle room is full.
            {
                SendRoomMessage(conn, ClientRoomState.Fail, conn.connectionId);
                return;
            }

            room.AddConnection(conn);

            OnServerJoinedClient?.Invoke(conn);
            
            SendRoomMessage(conn, ClientRoomState.Joined, conn.connectionId, roomName);
        }

        [ServerCallback]
        public void ExitRoom(NetworkConnection conn)
        {
            var exitedRoom = _rooms.FirstOrDefault(room => room.RemoveConnection(conn));

            if (exitedRoom != null && exitedRoom._connections.Count < 1)
            {
                var serverMessage = new ServerRoomMessage(
                    ServerRoomState.Remove,
                    new RoomInfo{Name = exitedRoom.RoomName});
                
                conn.Send(serverMessage);
            }

            var roomMessage = new ClientRoomMessage(ClientRoomState.Exited, conn.connectionId);

            conn.Send(roomMessage);
        }

        #endregion

        #region Recieve Message Methods

        [ServerCallback]
        private void OnReceivedRoomMessageViaServer(NetworkConnectionToClient conn, ServerRoomMessage msg)
        {
            switch (msg.ServerRoomState)
            {
                case ServerRoomState.Create:
                    CreateRoom(conn, msg.RoomInfo);
                    break;
                case ServerRoomState.Join:
                    JoinRoom(conn, msg.RoomInfo.Name);
                    break;
                case ServerRoomState.Remove:
                    RemoveRoom(msg.RoomInfo.Name);
                    break;
                case ServerRoomState.Exit:
                    ExitRoom(conn);
                    break;
                default:
                    return;
            }
        }

        [Command(requiresAuthority = false)]
        private void CMD_OnServerJoined(int connectionId)
        {
            SpaceNetworkManager.singleton.OnServerJoinedClient(connectionId);
        }

        [ClientCallback]
        private void OnReceivedRoomMessageViaClient(ClientRoomMessage msg)
        {
            switch (msg.ClientRoomState)
            {
                case ClientRoomState.Created:
                    print("created room");
                    OnClientCreatedRoom?.Invoke();
                    CMD_OnServerJoined(msg.ConnectionId);
                    break;
                case ClientRoomState.Joined:
                    CMD_OnServerJoined(msg.ConnectionId);
                    break;
                case ClientRoomState.Removed:
                    break;
                case ClientRoomState.Exited:
                    break;
                case ClientRoomState.Fail:
                    break;
                default:
                    return;
            }
        }

        #endregion

        #region Base Server Methods
        
        [ServerCallback]
        internal void OnStartedServer()
        {
            NetworkServer.RegisterHandler<ServerRoomMessage>(OnReceivedRoomMessageViaServer);
        }
        
        [ClientCallback]
        internal void OnStartedClient()
        {
            NetworkClient.RegisterHandler<ClientRoomMessage>(OnReceivedRoomMessageViaClient);
        }

        #endregion

        #region Utilities

        [ServerCallback]
        private void SendRoomMessage(NetworkConnection conn, ClientRoomState state, int connectionId, string roomName = null)
        {
            var roomMessage = new ClientRoomMessage(roomName, state, connectionId);

            conn.Send(roomMessage);
        }

        #endregion
    }
}