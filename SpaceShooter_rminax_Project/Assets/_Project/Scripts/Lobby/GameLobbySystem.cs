using TMPro;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
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

        private Room _currentRoom;

        private Coroutine _startCoroutine;
        
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

        private IEnumerator Start_Cor()
        {
            var timer = 3f;
            while (timer >= 0)
            {
                _countDownText.text = $"{timer:0}";

                timer += Time.fixedDeltaTime;
                
                yield return new WaitForFixedUpdate();
            }

            var gameManager = gameObject.GameContainer().Get<GameManager>();
            var mod = gameManager.GetMod();
            
            if (mod is not ShrinkingAreaMod shrinkingAreaMod) yield break;
            
            shrinkingAreaMod.Start();
        }

        [Server]
        private void OnJoinedClient(NetworkConnectionToClient conn)
        {
            var username = conn?.identity?.GetComponent<SpaceshipController>()?.Username;

            if (string.IsNullOrEmpty(username)) return;

            RPC_AddPlayerToList(username);

            //TODO sayaç işlemi client da oyunu başlatma işlemi sunucuda çalışmalı
            if (_currentRoom.CurrentPlayers >= _currentRoom.MaxPlayers)
            {
                _countDownText.gameObject.SetActive(true);
                _startCoroutine = StartCoroutine(Start_Cor());
            }
        }

        [Server]
        private void OnExitedClient(NetworkConnectionToClient conn)
        {
            var username = conn?.identity?.GetComponent<SpaceshipController>()?.Username;

            if (string.IsNullOrEmpty(username)) return;
            
            RPC_RemovePlayerFromList(username);
            
            //TODO sayaç işlemi client da oyunu başlatma işlemi sunucuda çalışmalı
            if (_currentRoom.CurrentPlayers < _currentRoom.MaxPlayers)
            {
                if (_startCoroutine == null) return;
                
                _countDownText.gameObject.SetActive(false);
                StopCoroutine(_startCoroutine);
            }
        }
        
        [ClientRpc]
        private void RPC_AddPlayerToList(string username)
        {
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

    public class GameLobby_PlayerUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _usernameText;
        
        public void Init(string username)
        {
            _usernameText.text = username;
        }
    }
}