﻿using System;
using System.Collections;
using System.Linq;
using Mirror;
using UnityEngine;

namespace _Project.Scripts.Network.Managers
{
    using Spaceship;
    
    public class SpaceNetworkManager : NetworkManager
    {
        public static event Action OnConnectedServer;
        public static event Action OnServerRedied;
        public static event Action OnServerDisconnected;
        public static event Action OnConnectedClient;
        public static event Action OnDisconnectedClient;
        
        [SerializeField] private GameObject _gamePlayerPrefab;
        
        [SerializeField] private int _maxConnection;
        
        [Scene] [SerializeField] private string[] _gameScenes;
        
        public new static SpaceNetworkManager singleton { get; private set; }

        public string GetGameScene(string sceneName)
        {
            var gameScene = _gameScenes.FirstOrDefault(gameSceneName => gameSceneName == sceneName);

            return gameScene ?? string.Empty;
        }

        /// <summary>
        /// Runs on both Server and Client
        /// Networking is NOT initialized when this fires
        /// </summary>
        public override void Awake()
        {
            base.Awake();
            singleton = this;
            
            maxConnections = _maxConnection;
        }
        
        #region Server System Callbacks

        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            base.OnServerConnect(conn);
            
            OnConnectedServer?.Invoke();
        }

        public override void OnServerReady(NetworkConnectionToClient conn)
        {
            base.OnServerReady(conn);
            SpaceRoomManager.Singleton.OnServerReady(conn);
            
            OnServerRedied?.Invoke();
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            StartCoroutine(DoServerDisconnect(conn));
        }

        private IEnumerator DoServerDisconnect(NetworkConnectionToClient conn)
        {
            yield return SpaceRoomManager.Singleton.OnServerDisconnect(conn);
            base.OnServerDisconnect(conn);
            
            OnServerDisconnected?.Invoke();
        }

        #endregion

        #region Client System Callbacks

        public override void OnClientConnect()
        {
            base.OnClientConnect();

            //var playerNetwork = NetworkClient.localPlayer.GetComponent<Player_NETWORK>();
            SpaceRoomManager.Singleton.OnClientConnect();
            
            OnConnectedClient?.Invoke();
        }

        public override void OnClientDisconnect()
        {
            SpaceRoomManager.Singleton.OnClientDisconnect();
            
            OnDisconnectedClient?.Invoke();
        }

        #endregion

        #region Start & Stop Callbacks

        public override void OnStartServer()
        {
            SpaceRoomManager.Singleton.OnStartServer();
        }

        public override void OnStartClient()
        {
            SpaceRoomManager.Singleton.OnStartClient();
        }

        public override void OnStopServer()
        {
            SpaceRoomManager.Singleton.OnStopServer();
        }

        public override void OnStopClient()
        {
            SpaceRoomManager.Singleton.OnStartClient();   
        }

        #endregion
        
        public void ConnectOpenWorld()
        {
            var scene = GetGameScene("OpenWorld_Scene");
            
            ServerChangeScene(scene);
        }

        public void ReplacePlayer(NetworkConnectionToClient conn)
        {
            var oldPlayer = conn.identity.gameObject;
            
            var spaceshipObj = Instantiate(_gamePlayerPrefab);
            
            NetworkServer.Spawn(spaceshipObj, conn);
            NetworkServer.ReplacePlayerForConnection(conn, spaceshipObj);
            
            if (!spaceshipObj.TryGetComponent(out SpaceshipManager manager)) return;

            var networkMatch = conn.identity.GetComponent<NetworkMatch>();
            
            manager.Init(networkMatch.matchId);
            manager.CreateSpaceship(Vector3.zero, Quaternion.identity);
            
            Destroy(oldPlayer);
        }
        
        public void CreatePlayer(NetworkConnectionToClient conn)
        {
            var spaceshipObj = Instantiate(playerPrefab);
            
            NetworkServer.Spawn(spaceshipObj, conn);

            NetworkServer.AddPlayerForConnection(conn, spaceshipObj);
            
            if (!spaceshipObj.TryGetComponent(out SpaceshipManager manager)) return;
            
            manager.CreateSpaceship(Vector3.zero, Quaternion.identity);
        }
    }

    public struct CreateSpaceshipMessage : NetworkMessage
    {
        public GameObject SpaceshipPrefab;
    }
}