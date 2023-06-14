using Mirror;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.UI
{
    using Network.Managers;
    using Network.Messages;
    
    public class RoomGUI : MonoBehaviour
    {
        [SerializeField] private GameObject _playerList;
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private Button _leaveButton;
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _readyButton;
        [SerializeField] private bool _isOwner;

        [ClientCallback]
        public void RefreshRoomPlayers(PlayerInfo[] playerInfos)
        {
            foreach (Transform child in _playerList.transform)
                Destroy(child.gameObject);

            _startButton.interactable = false;
            var everyoneReady = true;

            foreach (var playerInfo in playerInfos)
            {
                var newPlayer = Instantiate(_playerPrefab, Vector3.zero, Quaternion.identity);
                newPlayer.transform.SetParent(_playerList.transform, false);
                newPlayer.GetComponent<PlayerGUI>().SetPlayerInfo(playerInfo);

                if (!playerInfo.IsReady)
                    everyoneReady = false;
            }

            _startButton.interactable = everyoneReady && _isOwner && (playerInfos.Length > 0);
        }

        [ClientCallback]
        public void SetOwner(bool isOwner)
        {
            _isOwner = isOwner;
            _cancelButton.gameObject.SetActive(isOwner);
            _leaveButton.gameObject.SetActive(!isOwner);
        }
        
        private void Start()
        {
            _cancelButton.onClick.AddListener(SpaceRoomManager.Singleton.RequestCancelRoom);
            _leaveButton.onClick.AddListener(SpaceRoomManager.Singleton.RequestLeaveRoom);
            _readyButton.onClick.AddListener(SpaceRoomManager.Singleton.RequestReadyChange);
            _startButton.onClick.AddListener(SpaceRoomManager.Singleton.RequestStartRoom);
        }
    }
}