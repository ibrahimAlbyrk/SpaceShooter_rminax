using TMPro;
using Mirror;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.Utilities;
using System.Collections;
using Sirenix.OdinInspector;

namespace _Project.Scripts.Lobby
{
    using Game;
    using Network;
    using Game.Mod;
    using Utilities;
    using Spaceship;
    using Extensions;
    using Network.Managers.Room;
    
    public class GameLobbySystem : NetIdentity
    {
        [Title("Setup")]
        [SerializeField] private Transform _playerContent;
        [SerializeField] private GameObject _playerUIPrefab;
        
        [Title("UI")]
        [SerializeField] private TMP_Text _countDownText;
        [SerializeField] private Button _exitButton;
        private Room _currentRoom;

        private Coroutine _startCoroutine;

        private bool _isGameStarted;

        [Command(requiresAuthority = false)]
        private void CMD_ListPlayers()
        {
            _currentRoom = SpaceRoomManager.Instance.GetRoomOfScene(gameObject.scene);
            
            RPC_ListPlayers(_currentRoom?.Connections?.Select(conn => conn?.identity?.GetComponent<SpaceshipController>()?.Username).ToArray());
        }
        
        [ClientRpc]
        private void RPC_ListPlayers(string[] usernames)
        {
            usernames.ForEach(AddPlayerToList);
        }

        public override void OnStartClient() => CMD_ListPlayers();

        [ClientCallback]
        private void Awake()
        {
            _exitButton.onClick.AddListener(() =>
            {
                SpaceRoomManager.RequestExitRoom();
            });
        }
        
        [ServerCallback]
        private void Start()
        {
            _currentRoom = SpaceRoomManager.Instance.GetRoomOfScene(gameObject.scene);
            
            SpaceRoomManager.OnServerJoinedClient += OnJoinedClient;
            SpaceRoomManager.OnServerExitedClient += OnExitedClient;
        }

        [ServerCallback]
        private void OnDestroy()
        {
            SpaceRoomManager.OnServerJoinedClient -= OnJoinedClient;
            SpaceRoomManager.OnServerExitedClient -= OnExitedClient;
        }

        [ClientRpc]
        private void RPC_StartCountDown()
        {
            _countDownText.gameObject.SetActive(true);
            _startCoroutine = StartCoroutine(Start_Cor());
        }
        
        [ClientRpc]
        private void RPC_StopCountDown()
        {
            if (_startCoroutine == null) return;
                
            _countDownText.gameObject.SetActive(false);
            StopCoroutine(_startCoroutine);
        }
        
        [Command(requiresAuthority = false)]
        private void CMD_StartGame()
        {
            if (_isGameStarted) return;
            
            var gameManager = gameObject.GameContainer().Get<GameManager>();
            var mod = gameManager.GetMod();
            
            if (mod is not ShrinkingAreaMod shrinkingAreaMod) return;
            
            shrinkingAreaMod.Start();

            NetworkServer.Destroy(gameObject);
            
            _isGameStarted = true;
        }
        
        private IEnumerator Start_Cor()
        {
            var timer = 3f;
            while (timer >= 0)
            {
                _countDownText.text = $"{timer:0}";

                timer -= Time.fixedDeltaTime;
                
                yield return new WaitForFixedUpdate();
            }

            CMD_StartGame();
        }
        
        [Server]
        private void OnJoinedClient(NetworkConnectionToClient conn)
        {
            if (_currentRoom.CurrentPlayers >= _currentRoom.MaxPlayers)
            {
                RPC_StartCountDown();
            }
            
            var username = conn?.identity?.GetComponent<SpaceshipController>()?.Username;

            if (string.IsNullOrEmpty(username)) return;

            RPC_AddPlayerToList(username);
        }

        [Server]
        private void OnExitedClient(NetworkConnectionToClient conn)
        {
            if (_currentRoom.CurrentPlayers < _currentRoom.MaxPlayers)
            {
                RPC_StopCountDown();
            }
            
            var username = conn?.identity?.GetComponent<SpaceshipController>()?.Username;

            if (string.IsNullOrEmpty(username)) return;
            
            RPC_RemovePlayerFromList(username);
        }
        
        [ClientRpc]
        private void RPC_AddPlayerToList(string username)
        {
            AddPlayerToList(username);
        }
        
        private void AddPlayerToList(string username)
        {
            if (string.IsNullOrEmpty(username)) return;

            if (_playerContent.Cast<Transform>().Any(player => player?.GetComponent<GameLobby_PlayerUI>()?.GetUsername() == username))
            {
                return;
            }
            
            var playerField = Instantiate(_playerUIPrefab, _playerContent);
            var ui = playerField.GetComponent<GameLobby_PlayerUI>();
            ui.Init(username);
        }
        
        [ClientRpc]
        private void RPC_RemovePlayerFromList(string username)
        {
            foreach (Transform child in _playerContent)
            {
                var ship = child.GetComponent<SpaceshipController>();
                
                if(ship == null || ship.Username != username) continue;
                
                child.gameObject.Destroy();
            }
        }
    }
}