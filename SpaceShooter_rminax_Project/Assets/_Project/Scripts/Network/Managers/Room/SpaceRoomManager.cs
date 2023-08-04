using System;
using Mirror;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
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
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = FindObjectOfType<SpaceRoomManager>();

                        if (instance == null)
                        {
                            var obj = new GameObject(nameof(SpaceRoomManager));
                            instance = obj.AddComponent<SpaceRoomManager>();
                            DontDestroyOnLoad(obj);
                        }
                    }
                }

                return instance;
            }
        }

        #endregion
        
        public static event Action<NetworkConnectionToClient> OnServerJoinedClient;
        
        public static event Action OnClientCreatedRoom;

        private readonly List<Room> _rooms = new();
        
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

            var room = new Room(roomName, maxPlayers, onServer);
            
            _rooms.Add(room);

            //If it is a client, add in to the room
            if (!onServer)
            {
                SendRoomMessage(conn, ClientRoomState.Created);
                
                SpaceSceneManager.Instance.LoadScene(roomInfo.SceneName, LoadSceneMode.Additive,
                    scene =>
                    {
                        room.Scene = scene;
                        JoinRoom(conn.identity.connectionToClient, roomName); 
                    });
                
                return;
            }
            
            SpaceSceneManager.Instance.LoadScene(roomInfo.SceneName, LoadSceneMode.Additive,
                scene => room.Scene = scene);
        }

        [ServerCallback]
        public void JoinRoom(NetworkConnectionToClient conn, string roomName)
        {
            var room = _rooms.FirstOrDefault(r => r.RoomName == roomName);

            if (room == null) // Handle room not found.
            {
                SendRoomMessage(conn, ClientRoomState.Fail);
                return;
            }

            if (room.MaxPlayers <= room.CurrentPlayers) // Handle room is full.
            {
                SendRoomMessage(conn, ClientRoomState.Fail);
                return;
            }

            room.AddConnection(conn);
            
            OnServerJoinedClient?.Invoke(conn);
            
            SendRoomMessage(conn, ClientRoomState.Joined);
        }

        [ServerCallback]
        public void RemoveAllRoom()
        {
            foreach (var room in _rooms)
            {
                RemoveRoom(room.RoomName);
            }
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

            removedConnections.ForEach(connection => SendRoomMessage(connection, ClientRoomState.Removed));
        }

        [ServerCallback]
        public void ExitRoom(NetworkConnection conn)
        {
            if (!_rooms.Any(room => room.RemoveConnection(conn)))
            {
                // Handle exit failed (user not in any room).
                return;
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
                case ServerRoomState.Exit:
                    ExitRoom(conn);
                    break;
                default:
                    return;
            }
        }

        [ClientCallback]
        private void OnReceivedRoomMessageViaClient(ClientRoomMessage msg)
        {
            switch (msg.ClientRoomState)
            {
                case ClientRoomState.Created:
                    print("created room");
                    break;
                case ClientRoomState.Joined:
                    print("joined room");
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
            print("started client");
            NetworkClient.RegisterHandler<ClientRoomMessage>(OnReceivedRoomMessageViaClient);
        }

        #endregion

        #region Utilities

        [ServerCallback]
        private void SendRoomMessage(NetworkConnection conn, ClientRoomState state)
        {
            var roomMessage = new ClientRoomMessage(state, conn.connectionId);

            conn.Send(roomMessage);
        }

        #endregion
    }
}