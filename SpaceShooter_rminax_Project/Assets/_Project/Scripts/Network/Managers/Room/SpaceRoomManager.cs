﻿using System;
using Mirror;
using System.Linq;
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
                    }
                }

                return instance;
            }
        }

        #endregion

        public static event Action<NetworkConnectionToClient> OnServerJoinedClient;

        private readonly List<Room> _rooms = new();

        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private SyncList<RoomListInfo> _roomInfos = new();

        public List<RoomListInfo> GetRooms() => _roomInfos.ToList();

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
            
            AddToList(room);

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
            
            UpdateRoomInfo(room);

            OnServerJoinedClient?.Invoke(conn);

            SendRoomMessage(conn, ClientRoomState.Joined);
        }

        [ServerCallback]
        public void RemoveAllRoom()
        {
            foreach (var room in _rooms.ToList())
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

            RemoveToList(room);

            var roomScene = room.Scene;

            SpaceSceneManager.Instance.UnLoadScene(roomScene);

            removedConnections.ForEach(connection => SendRoomMessage(connection, ClientRoomState.Removed));
        }

        [ServerCallback]
        public void ExitRoom(NetworkConnection conn)
        {
            var exitedRoom = _rooms.FirstOrDefault(room => room.RemoveConnection(conn));

            if (exitedRoom == default)
            {
                // Handle exit failed (user not in any room).
                return;
            }

            if (exitedRoom.CurrentPlayers < 1)
            {
                RemoveRoom(exitedRoom.RoomName);
            }
            else
            {
                UpdateRoomInfo(exitedRoom);
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
            NetworkClient.RegisterHandler<ClientRoomMessage>(OnReceivedRoomMessageViaClient);
        }

        #endregion

        #region Utilities

        [ServerCallback]
        private void UpdateRoomInfo(Room room)
        {
            var index = _roomInfos.FindIndex(info => info.Name == room.RoomName);

            var roomInfo = new RoomListInfo
                (
                    room.RoomName,
                    room.MaxPlayers,
                    room.CurrentPlayers
                );
            
            _roomInfos[index] = roomInfo;
        }
        
        [ServerCallback]
        private void AddToList(Room room)
        {
            _rooms.Add(room);

            var roomListInfo = new RoomListInfo
            (
                room.RoomName,
                room.MaxPlayers,
                room.CurrentPlayers
            );
            
            _roomInfos.Add(roomListInfo);
        }

        [ServerCallback]
        private void RemoveToList(Room room)
        {
            _rooms.Remove(room);
            _roomInfos.RemoveAll(info => info.Name == room.RoomName);
        }

        [ServerCallback]
        private void SendRoomMessage(NetworkConnection conn, ClientRoomState state)
        {
            var roomMessage = new ClientRoomMessage(state, conn.connectionId);

            conn.Send(roomMessage);
        }

        #endregion
    }
}