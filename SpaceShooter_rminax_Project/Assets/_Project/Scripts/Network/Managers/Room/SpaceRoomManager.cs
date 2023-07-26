using Mirror;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace _Project.Scripts.Network.Managers.Room
{
    public class SpaceRoomManager : NetIdentity
    {
        public static SpaceRoomManager Instance;
        
        [SerializeField] private List<Room> _rooms = new();

        #region Command Methods

        [Command(requiresAuthority = false)]
        public void CreateRoomRequest(NetworkConnectionToClient conn, RoomInfo roomInfo) => CreateRoom(conn, roomInfo);

        [Command(requiresAuthority = false)]
        public void JoinRoomRequest(NetworkConnectionToClient conn, string roomName) => JoinRoom(conn, roomName);

        [Command(requiresAuthority = false)]
        public void ExitRoomRequest(NetworkConnectionToClient conn) => ExitRoom(conn);

        #endregion

        #region Server Methods

        [Server]
        public bool CreateRoom(RoomInfo roomInfo) => CreateRoom(null, roomInfo);

        [Server]
        public bool CreateRoom(NetworkConnection conn, RoomInfo roomInfo = default)
        {
            var roomName = roomInfo.Name;
            var sceneName = roomInfo.SceneName;
            var maxPlayers = roomInfo.MaxPlayers;

            if (_rooms.Any(room => room.RoomName == roomName)) return false;

            var onServer = conn is null;
            
            var room = new Room(roomName, sceneName, maxPlayers, onServer);
            
            //If it is a client, add in to the room
            if (!onServer)
            {
                room.AddConnection(conn);
                
                SendRoomMessage(conn, RoomState.Created);
            }
            
            _rooms.Add(room);

            return true;
        }

        [Server]
        public bool RemoveRoom(string roomName)
        {
            var room = _rooms.FirstOrDefault(room => room.RoomName == roomName);

            if (room == null) return false;

            var removedConnections = room.RemoveAllConnections();
            
            _rooms.Remove(room);
            
            removedConnections.ForEach(connection => SendRoomMessage(connection, RoomState.Removed));
            
            return true;
        }

        [Server]
        public bool JoinRoom(NetworkConnection conn, string roomName)
        {
            var room = _rooms.FirstOrDefault(r => r.RoomName == roomName);

            if (room == null) return false; // Handle room not found.
            
            if (room.MaxPlayers <= room.CurrentPlayers)
            {
                // Handle room is full.
                
                SendRoomMessage(conn, RoomState.Fail);
                
                return false;
            }

            room.AddConnection(conn);

            SendRoomMessage(conn, RoomState.Joined);

            return true;
        }

        [Server]
        public bool ExitRoom(NetworkConnection conn)
        {
            if (!_rooms.Any(room => room.RemoveConnection(conn)))
            {
                // Handle exit failed (user not in any room).
                return false;
            }
            
            var roomMessage = new RoomMessage(RoomState.Exited);
            
            conn.Send(roomMessage);

            return true;
        }

        #endregion

        #region Recieve Message Methods

        private void OnReceivedRoomMessageViaServer(NetworkConnection conn, RoomMessage msg)
        {
            switch (msg.RoomState)
            {
                case RoomState.Created:
                    break;
                case RoomState.Joined:
                    break;
                case RoomState.Removed:
                    break;
                case RoomState.Exited:
                    break;
                case RoomState.Fail:
                    break;
                default:
                    return;
            }
        }
        
        private void OnReceivedRoomMessageViaClient(RoomMessage msg)
        {
            switch (msg.RoomState)
            {
                case RoomState.Created:
                    break;
                case RoomState.Joined:
                    break;
                case RoomState.Removed:
                    break;
                case RoomState.Exited:
                    break;
                case RoomState.Fail:
                    break;
                default:
                    return;
            }
        }

        #endregion

        #region Override Methods

        public override void OnStartServer()
        {
            NetworkServer.RegisterHandler<RoomMessage>(OnReceivedRoomMessageViaServer);
        }

        public override void OnStartClient()
        {
            NetworkClient.RegisterHandler<RoomMessage>(OnReceivedRoomMessageViaClient);
        }

        #endregion

        #region Base Methods

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        #endregion

        #region Utilities

        private void SendRoomMessage(NetworkConnection conn, RoomState state)
        {
            var roomMessage = new RoomMessage(state);
                
            conn.Send(roomMessage);
        }

        #endregion
    }
}