using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _Project.Scripts.Network.Managers
{
    using UI;
    using Messages;

    public class SpaceRoomManager : MonoBehaviour
    {
        [Header("Room")]
        [SerializeField] private GameObject _roomList;
        [SerializeField] private GameObject _roomPrefab;
        [SerializeField] private RoomGUI _roomGUI;
        
        [Header("Views")]
        [SerializeField] private GameObject _lobbyView;
        [SerializeField] private GameObject _roomView;
        
        [Header("Buttons")]
        [SerializeField] private Button _createButton;
        [SerializeField] private Button _joinButton;
        [SerializeField] private ToggleGroup _toggleGroup;
        
        public static SpaceRoomManager Singleton;

        /// <summary>
        /// Room Controllers listen for this to terminate their room and clean up
        /// </summary>
        public event Action<NetworkConnectionToClient> OnPlayerDisconnected;

        /// <summary>
        /// Cross-reference of client that created the corresponding room in openRooms below
        /// </summary>
        internal static readonly Dictionary<NetworkConnectionToClient, Guid> playerRooms = new();

        /// <summary>
        /// Open rooms that are available for joining
        /// </summary>
        internal static readonly Dictionary<Guid, RoomInfo> openRooms = new();

        /// <summary>
        /// Network Connections of all players in a room
        /// </summary>
        internal static readonly Dictionary<Guid, HashSet<NetworkConnectionToClient>> roomConnections = new();

        /// <summary>
        /// Player informations by Network Connection
        /// </summary>
        internal static readonly Dictionary<NetworkConnection, PlayerInfo> playerInfos = new();

        /// <summary>
        /// Network Connections that have neither started not joined a room yet
        /// </summary>
        internal static readonly List<NetworkConnectionToClient> waitingConnections = new();

        /// <summary>
        /// GUID of a room the local player has selected in the Toggle Group room list
        /// </summary>
        internal Guid selectedRoom = Guid.Empty;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void ResetStatics()
        {
            openRooms.Clear();
            playerRooms.Clear();
            roomConnections.Clear();
            playerInfos.Clear();
            waitingConnections.Clear();
        }

        // Called from several places to ensure a clean reset
        //  - SpaceNetworkManager.Awake
        //  - OnStartServer
        //  - OnStartClient
        //  - OnClientDisconnect
        //  - ResetCanvas
        internal void InitializeData()
        {
            openRooms.Clear();
            playerRooms.Clear();
            roomConnections.Clear();
            waitingConnections.Clear();
            playerInfos.Clear();
            
            if(LobbyPlayer.localLobbyPlayer != null)
                LobbyPlayer.localLobbyPlayer.CMD_SetRoomID(Guid.Empty);
        }

        private void ResetCanvas()
        {
            InitializeData();
            _lobbyView.SetActive(false);
            _roomView.SetActive(false);
        }

        #region Button Calls
        
        [ClientCallback]
        public void SelectRoom(Guid roomID)
        {
            //TODO
            
            if (roomID == Guid.Empty)
            {
                selectedRoom = Guid.Empty;
                //joinButton.interactable = false;
            }
            else
            {
                if (!openRooms.Keys.Contains(roomID))
                {
                    _joinButton.interactable = false;
                    return;
                }

                selectedRoom = roomID;
                var infos = openRooms[roomID];
                _joinButton.interactable = infos.Players < infos.MaxPlayers;
            }
        }

        /// <summary>
        /// Assigned in inspector to Create button
        /// </summary>
        [ClientCallback]
        public void RequestCreateRoom()
        {
            NetworkClient.Send(new ServerRoomMessage { ServerRoomOperation = ServerRoomOperation.Create });
        }

        /// <summary>
        /// Assigned in inspector to Join button
        /// </summary>
        [ClientCallback]
        public void RequestJoinRoom()
        {
            if (selectedRoom == Guid.Empty) return;

            NetworkClient.Send(new ServerRoomMessage { ServerRoomOperation = ServerRoomOperation.Join, RoomID = selectedRoom });
        }

        /// <summary>
        /// Assigned in inspector to Leave button
        /// </summary>
        [ClientCallback]
        public void RequestLeaveRoom()
        {
            if (LobbyPlayer.localLobbyPlayer.RoomID == Guid.Empty) return;

            NetworkClient.Send(new ServerRoomMessage { ServerRoomOperation = ServerRoomOperation.Leave, RoomID = LobbyPlayer.localLobbyPlayer.RoomID });
        }

        /// <summary>
        /// Assigned in inspector to Cancel button
        /// </summary>
        [ClientCallback]
        public void RequestCancelRoom()
        {
            if (LobbyPlayer.localLobbyPlayer.RoomID == Guid.Empty) return;

            NetworkClient.Send(new ServerRoomMessage { ServerRoomOperation = ServerRoomOperation.Cancel });
        }

        /// <summary>
        /// Assigned in inspector to Ready button
        /// </summary>
        [ClientCallback]
        public void RequestReadyChange()
        {
            if (LobbyPlayer.localLobbyPlayer.RoomID == Guid.Empty) return;

            NetworkClient.Send(new ServerRoomMessage { ServerRoomOperation = ServerRoomOperation.Ready, RoomID = LobbyPlayer.localLobbyPlayer.RoomID });
        }

        /// <summary>
        /// Assigned in inspector to Start button
        /// </summary>
        [ClientCallback]
        public void RequestStartRoom()
        {
            if (LobbyPlayer.localLobbyPlayer.RoomID == Guid.Empty) return;

            NetworkClient.Send(new ServerRoomMessage { ServerRoomOperation = ServerRoomOperation.Start });
        }
        [ClientCallback]
        public void OnRoomEnded()
        {
            LobbyPlayer.localLobbyPlayer.CMD_SetRoomID(Guid.Empty);
            ShowLobbyView();
        }

        #endregion

        #region Server Callbacks

        [ServerCallback]
        internal void OnStartServer()
        {
            InitializeData();
            NetworkServer.RegisterHandler<ServerRoomMessage>(OnServerRoomMessage);
        }

        [ServerCallback]
        internal void OnServerReady(NetworkConnectionToClient conn)
        {
            waitingConnections.Add(conn);

            //var playerNetwork = conn.identity.GetComponent<Player_NETWORK>();

            var playerInfo = new PlayerInfo
            {
                Username = "testUser",
                IsReady = false
            };

            playerInfos.Add(conn, playerInfo);

            SendRoomList();
        }

        [ServerCallback]
        internal IEnumerator OnServerDisconnect(NetworkConnectionToClient conn)
        {
            OnPlayerDisconnected?.Invoke(conn);

            PlayerInfo playerInfo;
            if (playerRooms.TryGetValue(conn, out var roomID))
            {
                playerRooms.Remove(conn);
                openRooms.Remove(roomID);

                foreach (var playerConn in roomConnections[roomID])
                {
                    playerInfo = playerInfos[playerConn];
                    playerInfo.IsReady = false;
                    playerInfo.RoomID = Guid.Empty;

                    playerInfos[playerConn] = playerInfo;

                    var clientRoomMessage = new ClientRoomMessage
                    {
                        ClientRoomOperation = ClientRoomOperation.Departed
                    };

                    playerConn.Send(clientRoomMessage);
                }
            }

            foreach (var kvp in roomConnections)
                kvp.Value.Remove(conn);

            playerInfo = playerInfos[conn];
            if (playerInfo.RoomID != Guid.Empty)
            {
                if (openRooms.TryGetValue(playerInfo.RoomID, out var roomInfo))
                {
                    roomInfo.Players--;
                    openRooms[playerInfo.RoomID] = roomInfo;
                }

                if (roomConnections.TryGetValue(playerInfo.RoomID, out var connections))
                {
                    var infos = connections.Select(playerConn => playerInfos[playerConn]).ToArray();

                    foreach (var playerConn in roomConnections[playerInfo.RoomID])
                    {
                        if (playerConn != conn)
                        {
                            var clientRoomMessage = new ClientRoomMessage
                            {
                                ClientRoomOperation = ClientRoomOperation.UpdateRoom,
                                PlayerInfos = infos
                            };

                            playerConn.Send(clientRoomMessage);
                        }
                    }
                }
            }

            waitingConnections.Remove(conn);
            playerInfos.Remove(conn);
            
            SendRoomList();

            yield return null;
        }

        #endregion

        #region Client Callbacks

        [ServerCallback]
        internal void OnStopServer()
        {
            ResetCanvas();
        }

        [ClientCallback]
        internal void OnClientConnect()
        {
            var playerInfo = new PlayerInfo
            {
                Username = "testPlayer",
                IsReady = false
            };
            playerInfos.Add(NetworkClient.connection, playerInfo);
        }

        [ClientCallback]
        internal void OnStartClient()
        {
            InitializeData();
            ShowLobbyView();
            _createButton.gameObject.SetActive(true);
            _joinButton.gameObject.SetActive(true);
            NetworkClient.RegisterHandler<ClientRoomMessage>(OnClientRoomMessage);
        }

        [ClientCallback]
        internal void OnClientDisconnect() => InitializeData();

        [ClientCallback]
        internal void OnStopClient()
        {
            ResetCanvas();
        }

        #endregion

        #region Server Room Message Handlers

        [ServerCallback]
        private void OnServerRoomMessage(NetworkConnectionToClient conn, ServerRoomMessage msg)
        {
            switch (msg.ServerRoomOperation)
            {
                case ServerRoomOperation.None:
                {
                    Debug.LogWarning("Missing ServerRoomOperation");
                    break;
                }
                case ServerRoomOperation.Create:
                {
                    OnServerCreateRoom(conn, 100);
                    break;
                }
                case ServerRoomOperation.Cancel:
                {
                    OnServerCancelRoom(conn);
                    break;
                }
                case ServerRoomOperation.Start:
                {
                    OnServerStartRoom(conn);
                    break;
                }
                case ServerRoomOperation.Join:
                {
                    OnServerJoinRoom(conn, msg.RoomID);
                    break;
                }
                case ServerRoomOperation.Leave:
                {
                    OnServerLeaveRoom(conn, msg.RoomID);
                    break;
                }
                case ServerRoomOperation.Ready:
                {
                    OnServerPlayerReady(conn, msg.RoomID);
                    break;
                }
            }
        }

        [ServerCallback]
        private void OnServerPlayerReady(NetworkConnectionToClient conn, Guid roomID)
        {
            var playerInfo = playerInfos[conn];
            playerInfo.IsReady = !playerInfo.IsReady;
            playerInfos[conn] = playerInfo;

            var connections = roomConnections[roomID];
            var infos = connections.Select(playerConn => playerInfos[playerConn]).ToArray();

            foreach (var playerConn in roomConnections[roomID])
                playerConn.Send(new ClientRoomMessage { ClientRoomOperation = ClientRoomOperation.UpdateRoom, PlayerInfos = infos });
        }
        
        [ServerCallback]
        private void OnServerLeaveRoom(NetworkConnectionToClient conn, Guid roomID)
        {
            var roomInfo = openRooms[roomID];
            roomInfo.Players--;
            openRooms[roomID] = roomInfo;

            var playerInfo = playerInfos[conn];
            playerInfo.IsReady = false;
            playerInfo.RoomID = Guid.Empty;
            playerInfos[conn] = playerInfo;

            foreach (var kvp in roomConnections)
                kvp.Value.Remove(conn);

            var connections = roomConnections[roomID];
            var infos = connections.Select(playerConn => playerInfos[playerConn]).ToArray();

            foreach (var playerConn in roomConnections[roomID])
                playerConn.Send(new ClientRoomMessage { ClientRoomOperation = ClientRoomOperation.UpdateRoom, PlayerInfos = infos });

            SendRoomList();

            conn.Send(new ClientRoomMessage { ClientRoomOperation = ClientRoomOperation.Departed });
        }

        [Server]
        public void OnServerCreateRoomViaServer(byte maxPlayers)
        {
            var newRoomID = Guid.NewGuid();
            roomConnections.Add(newRoomID, new HashSet<NetworkConnectionToClient>());

            var roomInfo = new RoomInfo
            {
                RoomID = newRoomID,
                MaxPlayers = maxPlayers,
                Players = 0,
                IsServer = true
            };

            openRooms.Add(newRoomID, roomInfo);

            SendRoomList();
        }
        
        [ServerCallback]
        private void OnServerCreateRoom(NetworkConnectionToClient conn, byte maxPlayers)
        {
            if (playerRooms.ContainsKey(conn)) return;

            var newRoomID = Guid.NewGuid();
            roomConnections.Add(newRoomID, new HashSet<NetworkConnectionToClient>());
            roomConnections[newRoomID].Add(conn);
            playerRooms.Add(conn, newRoomID);

            var roomInfo = new RoomInfo
            {
                RoomID = newRoomID,
                MaxPlayers = maxPlayers,
                Players = 1,
                IsServer = false
            };

            openRooms.Add(newRoomID, roomInfo);

            var playerInfo = playerInfos[conn];
            playerInfo.IsReady = false;
            playerInfo.RoomID = newRoomID;
            playerInfos[conn] = playerInfo;

            var infos = roomConnections[newRoomID].Select(playerConn => playerInfos[playerConn]).ToArray();

            var clientRoomMessage = new ClientRoomMessage
            {
                ClientRoomOperation = ClientRoomOperation.Created,
                RoomID = newRoomID,
                PlayerInfos = infos
            };

            conn.Send(clientRoomMessage);

            SendRoomList();
        }

        [ServerCallback]
        private void OnServerCancelRoom(NetworkConnectionToClient conn)
        {
            if (!playerRooms.ContainsKey(conn)) return;

            conn.Send(new ClientRoomMessage { ClientRoomOperation = ClientRoomOperation.Cancelled });

            if (playerRooms.TryGetValue(conn, out var roomID))
            {
                playerRooms.Remove(conn);
                openRooms.Remove(roomID);

                foreach (var playerConn in roomConnections[roomID])
                {
                    var playerInfo = playerInfos[playerConn];
                    playerConn.isReady = false;
                    playerInfo.RoomID = Guid.Empty;
                    playerInfos[playerConn] = playerInfo;

                    playerConn.Send(new ClientRoomMessage { ClientRoomOperation = ClientRoomOperation.Departed });
                }
                
                SendRoomList();
            }
        }

        [ServerCallback]
        private void OnServerStartRoom(NetworkConnectionToClient conn)
        {
            if (!playerRooms.ContainsKey(conn)) return;

            if (!playerRooms.TryGetValue(conn, out var roomID)) return;

            foreach (var playerConn in roomConnections[roomID])
            {
                playerConn.Send(new ClientRoomMessage{ClientRoomOperation = ClientRoomOperation.Started});
                
                //Reset ready state for after the room
                var playerInfo = playerInfos[playerConn];
                playerInfo.IsReady = false;
                playerInfos[playerConn] = playerInfo;
            }

            playerRooms.Remove(conn);
            openRooms.Remove(roomID);
            
            SendRoomList();
        }

        [ServerCallback]
        private void OnServerJoinRoom(NetworkConnectionToClient conn, Guid roomID)
        {
            if (!roomConnections.ContainsKey(roomID) || !openRooms.ContainsKey(roomID)) return;
            
            var roomInfo = openRooms[roomID];
            roomInfo.Players++;
            openRooms[roomID] = roomInfo;
            roomConnections[roomID].Add(conn);

            var playerInfo = playerInfos[conn];
            playerInfo.IsReady = false;
            playerInfo.RoomID = roomID;
            playerInfos[conn] = playerInfo;

            var infos = roomConnections[roomID].Select(playerConn => playerInfos[playerConn]).ToArray();
            
            SendRoomList();

            var clientRoomMessage = new ClientRoomMessage
            {
                ClientRoomOperation = ClientRoomOperation.Joined,
                RoomID = roomID,
                PlayerInfos = infos
            };
            
            conn.Send(clientRoomMessage);

            foreach (var playerConn in roomConnections[roomID])
            {
                playerConn.Send(new ClientRoomMessage
                {
                    ClientRoomOperation = ClientRoomOperation.UpdateRoom,
                    PlayerInfos = infos
                });
            }
        }

        [ServerCallback]
        internal void SendRoomList(NetworkConnectionToClient conn = null)
        {
            if (conn != null)
            {
                var clientRoomMessage = new ClientRoomMessage
                {
                    ClientRoomOperation = ClientRoomOperation.List,
                    RoomInfos = openRooms.Values.ToArray()
                };

                conn.Send(clientRoomMessage);

                return;
            }

            foreach (var waiter in waitingConnections)
            {
                var clientRoomOperation = new ClientRoomMessage
                {
                    ClientRoomOperation = ClientRoomOperation.List, RoomInfos = openRooms.Values.ToArray()
                };

                waiter.Send(clientRoomOperation);
            }
        }

        #endregion

        #region Client Room Message Handlers

        [ClientCallback]
        private void OnClientRoomMessage(ClientRoomMessage msg)
        {
            switch (msg.ClientRoomOperation)
            {
                case ClientRoomOperation.None:
                {
                    Debug.LogWarning("Missing ClientRoomOperation");
                    break;
                }
                case ClientRoomOperation.List:
                {
                    openRooms.Clear();
                    foreach (var roomInfo in msg.RoomInfos)
                    {
                        openRooms.Add(roomInfo.RoomID, roomInfo);
                    }

                    RefreshRoomList();
                    break;
                }
                case ClientRoomOperation.Created:
                {
                    LobbyPlayer.localLobbyPlayer.CMD_SetRoomID(msg.RoomID);
                    ShowRoomView();
                    _roomGUI.RefreshRoomPlayers(msg.PlayerInfos);
                    _roomGUI.SetOwner(true);
                    break;
                }
                case ClientRoomOperation.Cancelled:
                {
                    LobbyPlayer.localLobbyPlayer.CMD_SetRoomID(Guid.Empty);
                    ShowLobbyView();
                    break;
                }
                case ClientRoomOperation.Joined:
                {
                    LobbyPlayer.localLobbyPlayer.CMD_SetRoomID(msg.RoomID);
                    ShowRoomView();
                    _roomGUI.RefreshRoomPlayers(msg.PlayerInfos);
                    _roomGUI.SetOwner(false);
                    break;
                }
                case ClientRoomOperation.Departed:
                {
                    LobbyPlayer.localLobbyPlayer.CMD_SetRoomID(Guid.Empty);
                    ShowLobbyView();
                    break;
                }
                case ClientRoomOperation.UpdateRoom:
                {
                    _roomGUI.RefreshRoomPlayers(msg.PlayerInfos);
                    break;
                }
                case ClientRoomOperation.Started:
                {
                    _lobbyView.SetActive(false);
                    _roomView.SetActive(false);
                    
                    //Create Player

                    SceneManager.LoadSceneAsync("OpenWorld_Scene", LoadSceneMode.Additive).completed += delegate
                    {
                        var player = SpaceNetworkManager.singleton.ReplaceGamePlayer(LobbyPlayer.localLobbyPlayer.connectionToClient);
                        SceneManager.MoveGameObjectToScene(player, SceneManager.GetSceneByName("OpenWorld_Scene"));
                    };
                    
                    break;
                }
            }
        }

        [ClientCallback]
        private void ShowLobbyView()
        {
            _lobbyView.SetActive(true);
            _roomView.SetActive(false);

            foreach (Transform child in _roomList.transform)
            {
                if (child.gameObject.GetComponent<RoomFieldGUI>().GetRoomID() == selectedRoom)
                {
                    var toggle = child.gameObject.GetComponent<Toggle>();
                    toggle.isOn = true;
                }
            }
        }

        [ClientCallback]
        private void ShowRoomView()
        {
            _lobbyView.SetActive(false);
            _roomView.SetActive(true);
        }

        [ClientCallback]
        private void RefreshRoomList()
        {
            foreach (Transform child in _roomList.transform)
                Destroy(child.gameObject);

            _joinButton.interactable = false;

            foreach (var roomInfo in openRooms.Values)
            {
                var newMatch = Instantiate(_roomPrefab, Vector3.zero, Quaternion.identity);
                newMatch.transform.SetParent(_roomList.transform, false);
                newMatch.GetComponent<RoomFieldGUI>().SetRoomInfo(roomInfo);

                var toggle = newMatch.GetComponent<Toggle>();
                toggle.group = _toggleGroup;
                if (roomInfo.RoomID == selectedRoom)
                    toggle.isOn = true;
            }
        }

        #endregion

        #region Base Methods

        private void Awake() => Init();

        private void Start()
        {
            _createButton.onClick.AddListener(RequestCreateRoom);
            _joinButton.onClick.AddListener(RequestJoinRoom);
        }
        
        private void Init()
        {
            if (Singleton == null)
            {
                Singleton = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Singleton != null && Singleton != this)
                Destroy(Singleton.gameObject);
        }

        #endregion
    }
}