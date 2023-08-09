using Mirror;
using System;
using UnityEngine;
using System.Linq;
using System.Collections;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Network.Managers
{
    using Room;
    
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
            
            CreateLobbyPlayer(conn);
            
            OnServerRedied?.Invoke(conn);
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            base.OnServerDisconnect(conn);

            OnServerDisconnected?.Invoke();
        }
        
        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            var activeSceneName = SceneManager.GetActiveScene().path;

            if (activeSceneName != _hubScene) return;

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
        public void OnServerJoinedClient(NetworkConnectionToClient conn)
        {
            StartCoroutine(OnClientJoinedRoom_Cor(conn));
        }
        
        [ServerCallback]
        public void OnServerJoinedClient(int connectionId)
        {
            if (!NetworkServer.connections.ContainsKey(connectionId)) return;
            
            var conn = NetworkServer.connections[connectionId];

            if (conn == null) return;

            StartCoroutine(OnClientJoinedRoom_Cor(conn));
        }

        private IEnumerator OnClientJoinedRoom_Cor(NetworkConnectionToClient conn)
        {
            conn.Send(new SceneMessage { sceneName = _gameScene, sceneOperation = SceneOperation.Normal });

            yield return new WaitForEndOfFrame();
            
            var newPlayer = ReplaceGamePlayer(conn);
            newPlayer.name = $"{_gamePlayerPrefab.name} [connId={conn.connectionId}]";

            var playersRoom = SpaceRoomManager.Instance.GetPlayersRoom(conn);
            
            SceneManager.MoveGameObjectToScene(newPlayer, playersRoom.Scene);
        }
        
        #region Start & Stop Callbacks

        private IEnumerator RoomManagerStartedServer()
        {
            while(SpaceRoomManager.Instance == null)
            {
                yield return null;
            }
            
            SpaceRoomManager.Instance.OnStartedServer();
            
            var roomInfo = new RoomInfo
            {
                Name = "OpenWorld",
                MaxPlayers = 100,
                SceneName = "OpenWorld_Scene"
            };
            
            SpaceRoomManager.Instance.CreateRoom(roomInfo);

            SpaceRoomManager.OnServerJoinedClient += OnServerJoinedClient;
        }
        
        private IEnumerator RoomManagerStartedClient()
        {
            while(SpaceRoomManager.Instance == null)
            {
                yield return null;
            }
            
            SpaceRoomManager.Instance.OnStartedClient();
        }
        
        public override void OnStartServer()
        {
            spawnPrefabs.Clear();
            
            spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList();

            StartCoroutine(RoomManagerStartedServer());
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
            
            StartCoroutine(RoomManagerStartedClient());
        }

        public override void OnStopServer()
        {
            SpaceRoomManager.Instance.RemoveAllRoom();
        }

        public override void OnStopClient()
        {
            if(!SceneManager.GetActiveScene().path.Equals(_hubScene))
                ServerChangeScene(_hubScene);
        }

        #endregion

        #region Connection Methods

        public void ConnectClient(string ipv4, ushort port)
        {
            SetConnectionConfig(ipv4, port);
            
            StartClient();
        }

        public void ConnectServer(string ipv4, ushort port)
        {
            SetConnectionConfig(ipv4, port);
            
            StartServer();
        }

        public void SetConnectionConfig(string ipv4, ushort port)
        {
            networkAddress = ipv4;
            
            if (transport is TelepathyTransport telepathyTransport)
            {
                telepathyTransport.port = port;
            }
        }

        #endregion

        #region Player Methods

        public GameObject CreateLobbyPlayer(NetworkConnectionToClient conn, GameObject lobbyPlayerObj = null)
        {
            var player = Instantiate(lobbyPlayerObj == null ? _lobbyPlayerPrefab : lobbyPlayerObj);

            NetworkServer.AddPlayerForConnection(conn, player);

            return player;
        }
        
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