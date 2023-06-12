using System;
using System.Collections;
using System.Linq;
using Mirror;
using UnityEngine;

namespace _Project.Scripts.Network.Managers
{
    using Spaceship;
    
    public class SpaceNetworkManager : NetworkManager
    {
        [SerializeField] private SpaceRoomManager _roomManager;
        
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

        public override void OnServerReady(NetworkConnectionToClient conn)
        {
            base.OnServerReady(conn);
            _roomManager.OnServerReady(conn);
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            StartCoroutine(DoServerDisconnect(conn));
        }

        private IEnumerator DoServerDisconnect(NetworkConnectionToClient conn)
        {
            yield return _roomManager.OnServerDisconnect(conn);
            base.OnServerDisconnect(conn);
        }

        #endregion

        #region Client System Callbacks

        public override void OnClientConnect()
        {
            base.OnClientConnect();

            //var playerNetwork = NetworkClient.localPlayer.GetComponent<Player_NETWORK>();
            _roomManager.OnClientConnect();
        }

        public override void OnClientDisconnect()
        { ;
            _roomManager.OnClientDisconnect();
        }

        #endregion

        #region Start & Stop Callbacks

        public override void OnStartServer()
        {
            _roomManager.OnStartServer();
        }

        public override void OnStartClient()
        {
            _roomManager.OnStartClient();
        }

        public override void OnStopServer()
        {
            _roomManager.OnStopServer();
        }

        public override void OnStopClient()
        {
            _roomManager.OnStartClient();   
        }

        #endregion
        
        public void ConnectOpenWorld()
        {
            var scene = GetGameScene("OpenWorld_Scene");
            
            ServerChangeScene(scene);
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