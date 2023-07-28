using System;
using Mirror;
using System.Linq;
using System.Collections.Generic;

namespace _Project.Scripts.Network.Managers.Room
{
    using Scene;
    
    public class SpaceRoomManager : NetIdentity
    {
        public static event Action<NetworkConnectionToClient> OnClientJoinedRoom; 
        
        

        private static readonly List<Room> _rooms = new();

        public List<Room> GetRooms() => _rooms;

        public static Room GetPlayersRoom(NetworkConnection conn)
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
        public static void CreateRoom(RoomInfo roomInfo) => CreateRoom(null, roomInfo);

        [ServerCallback]
        public static void CreateRoom(NetworkConnection conn, RoomInfo roomInfo = default)
        {
            var roomName = roomInfo.Name;
            var maxPlayers = roomInfo.MaxPlayers;

            if (_rooms.Any(room => room.RoomName == roomName)) return;

            var onServer = conn is null;

            var room = new Room(roomName, maxPlayers,onServer);

            //If it is a client, add in to the room
            if (!onServer)
            {
                room.AddConnection(conn);

                SendRoomMessage(conn, ClientRoomState.Created);
            }
            
            _rooms.Add(room);
            
            SceneManager.Instance.LoadAdditiveScene(1).OnCompleted(scene =>
            {
                room.Scene = scene;
            });
        }

        [ServerCallback]
        public static void RemoveRoom(string roomName)
        {
            var room = _rooms.FirstOrDefault(room => room.RoomName == roomName);

            if (room == null) return;

            var removedConnections = room.RemoveAllConnections();

            _rooms.Remove(room);

            var roomScene = room.Scene;
            
            SceneManager.Instance.UnLoadScene(roomScene);

            removedConnections.ForEach(connection => SendRoomMessage(connection, ClientRoomState.Removed));
        }

        [ServerCallback]
        public static void JoinRoom(NetworkConnectionToClient conn, string roomName)
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

            OnClientJoinedRoom?.Invoke(conn);
            
            SendRoomMessage(conn, ClientRoomState.Joined);
        }

        [ServerCallback]
        public static void ExitRoom(NetworkConnection conn)
        {
            if (!_rooms.Any(room => room.RemoveConnection(conn)))
            {
                // Handle exit failed (user not in any room).
                return;
            }

            var roomMessage = new ClientRoomMessage(ClientRoomState.Exited);

            conn.Send(roomMessage);
        }

        #endregion

        #region Recieve Message Methods

        [ServerCallback]
        private static void OnReceivedRoomMessageViaServer(NetworkConnectionToClient conn, ServerRoomMessage msg)
        {
            switch (msg.ServerRoomState)
            {
                case ServerRoomState.Create:
                    print("Create room, <color=green>server</color>");
                    CreateRoom(conn, msg.RoomInfo);
                    break;
                case ServerRoomState.Join:
                    print("Join room, <color=green>server</color>");
                    JoinRoom(conn, msg.RoomInfo.Name);
                    break;
                case ServerRoomState.Exit:
                    print("Exit room, <color=green>server</color>");
                    ExitRoom(conn);
                    break;
                default:
                    return;
            }
        }

        [ClientCallback]
        private static void OnReceivedRoomMessageViaClient(ClientRoomMessage msg)
        {
            switch (msg.ClientRoomState)
            {
                case ClientRoomState.Created:
                    print("Created room, <color=green>client</color>");
                    break;
                case ClientRoomState.Joined:
                    print("Joined room, <color=green>client</color>");
                    break;
                case ClientRoomState.Removed:
                    print("Removed room, <color=green>client</color>");
                    break;
                case ClientRoomState.Exited:
                    print("Exited room, <color=green>client</color>");
                    break;
                case ClientRoomState.Fail:
                    print("Failed room, <color=green>client</color>");
                    break;
                default:
                    return;
            }
        }

        #endregion

        #region Base Server Methods
        
        [ServerCallback]
        internal static void OnStartedServer()
        {
            NetworkServer.RegisterHandler<ServerRoomMessage>(OnReceivedRoomMessageViaServer);
        }
        
        [ClientCallback]
        internal static void OnStartedClient()
        {
            NetworkClient.RegisterHandler<ClientRoomMessage>(OnReceivedRoomMessageViaClient);
        }

        #endregion

        #region Utilities

        [ServerCallback]
        private static void SendRoomMessage(NetworkConnection conn, ClientRoomState state)
        {
            var roomMessage = new ClientRoomMessage(state);

            conn.Send(roomMessage);
        }

        #endregion
    }
}