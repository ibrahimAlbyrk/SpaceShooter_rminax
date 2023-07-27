﻿using Mirror;
using System;
using System.Linq;
using System.Threading.Tasks;
using _Project.Scripts.Network.Managers.Room;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Network.Managers
{
    public class SpaceNetworkManager : NetworkManager
    {
        public static event Action OnServerConnected;
        public static event Action<NetworkConnection> OnServerRedied;
        public static event Action OnServerDisconnected;
        public static event Action OnClientConnected;
        public static event Action OnClientDisconnected;

        [Scene] [SerializeField] private string _hubScene;
        [Scene] [SerializeField] private string _gameScene;

        [SerializeField] private GameObject _lobbyPlayerPrefab;
        [SerializeField] private GameObject _gamePlayerPrefab;

        [SerializeField] private int _maxConnection;

        public new static SpaceNetworkManager singleton { get; private set; }

        #region Base methods

        public override void Awake()
        {
            base.Awake();
            singleton = this;

            maxConnections = _maxConnection;
        }

        #endregion

        #region Server System Callbacks

        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            if (numPlayers >= maxConnections)
            {
                conn.Disconnect();
                return;
            }

            base.OnServerConnect(conn);

            OnServerConnected?.Invoke();
        }

        public override void OnServerReady(NetworkConnectionToClient conn)
        {
            base.OnServerReady(conn);
            
            OnServerRedied?.Invoke(conn);
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            base.OnServerDisconnect(conn);

            OnServerDisconnected?.Invoke();
        }

        // public override void ServerChangeScene(string newSceneName)
        // {
        //     //If from the hubScene to the gameScene
        //     if (SceneManager.GetActiveScene().name == _hubScene && newSceneName.Equals(_gameScene))
        //     {
        //         //Change lobby players to game players
        //         foreach (var conn in NetworkServer.connections.Values)
        //         {
        //             var newPlayer = ReplaceGamePlayer(conn);
        //             newPlayer.name = $"{_gamePlayerPrefab.name} [connId={conn.connectionId}]";
        //         }
        //     }
        //     
        //     //If from the gameScene to the hubScene
        //     if (SceneManager.GetActiveScene().name == _gameScene && newSceneName.Equals(_hubScene))
        //     {
        //         //Change lobby players to game players
        //         foreach (var conn in NetworkServer.connections.Values)
        //         {
        //             var newPlayer = ReplaceLobbyPlayer(conn);
        //             newPlayer.name = $"{_lobbyPlayerPrefab.name} [connId={conn.connectionId}]";
        //         }
        //     }
        //
        //     base.ServerChangeScene(newSceneName);
        // }

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            var activeSceneName = SceneManager.GetActiveScene().path;

            var prefab = activeSceneName == _hubScene
                ? _lobbyPlayerPrefab
                : _gamePlayerPrefab;

            var player = Instantiate(prefab);
            player.name = $"{prefab.name} [connId={conn.connectionId}]";

            NetworkServer.AddPlayerForConnection(conn, player);
        }

        #endregion

        #region Client System Callbacks

        public override void OnClientConnect()
        {
            base.OnClientConnect();

            OnClientConnected?.Invoke();
        }

        public override void OnClientDisconnect()
        {
            OnClientDisconnected?.Invoke();
        }

        #endregion

        [ServerCallback]
        private void OnClientJoinedRoom(NetworkConnectionToClient conn)
        {
            var newPlayer = ReplaceGamePlayer(conn);
            newPlayer.name = $"{_gamePlayerPrefab.name} [connId={conn.connectionId}]";

            var playersRoom = SpaceRoomManager.Instance.GetPlayersRoom(conn);
            
            SceneManager.MoveGameObjectToScene(newPlayer, playersRoom.Scene);
        }
        
        #region Start & Stop Callbacks

        public override void OnStartServer()
        {
            spawnPrefabs.Clear();
            
            spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList();
            
            SpaceRoomManager.OnStartedServer();
            
            var roomInfo = new RoomInfo("OpenWorld", "OpenWorld_Scene", 50);
            
            SpaceRoomManager.CreateRoom(roomInfo);

            SpaceRoomManager.OnClientJoinedRoom += OnClientJoinedRoom;
        }

        public override void OnStartClient()
        {
            spawnPrefabs.Clear();
            spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList();

            NetworkClient.ClearSpawners();

            foreach (var spawnablePrefab in spawnPrefabs)
            {
                NetworkClient.RegisterPrefab(spawnablePrefab);
            }
            
            SpaceRoomManager.OnStartedClient();
        }

        public override void OnStopServer()
        {
        }

        public override void OnStopClient()
        {
            if(!SceneManager.GetActiveScene().path.Equals(_hubScene))
                ServerChangeScene(_hubScene);
        }

        #endregion

        public void ConnectOpenWorld()
        {
            ServerChangeScene(_gameScene);
        }

        #region Player Methods

        public GameObject ReplaceGamePlayer(NetworkConnectionToClient conn, GameObject playerObj = null)
        {
            var oldPlayer = conn.identity.gameObject;

            var newPlayer = Instantiate(playerObj == null ? _gamePlayerPrefab : playerObj);

            NetworkServer.Destroy(oldPlayer);

            NetworkServer.ReplacePlayerForConnection(conn, newPlayer, true);

            return newPlayer;
        }
        
        public GameObject ReplaceLobbyPlayer(NetworkConnectionToClient conn, GameObject playerObj = null)
        {
            var oldPlayer = conn.identity.gameObject;

            var newPlayer = Instantiate(playerObj == null ? _lobbyPlayerPrefab : playerObj);

            NetworkServer.Destroy(oldPlayer);

            NetworkServer.ReplacePlayerForConnection(conn, newPlayer, true);

            return newPlayer;
        }
        
        #endregion
    }
}