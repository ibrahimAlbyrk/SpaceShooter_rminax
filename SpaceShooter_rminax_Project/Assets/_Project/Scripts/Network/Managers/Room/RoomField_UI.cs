using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Network.Managers.Room
{
    public class RoomField_UI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _countText;
        [SerializeField] private TMP_Text _nameText;

        [SerializeField] private Button _connectButton;

        public void Init(string roomName, int currentPlayerCount, int maxPlayerCount)
        {
            _countText.text = $"{currentPlayerCount}/{maxPlayerCount}";
            _nameText.text = roomName;
        }

        private void OnConnect()
        {
            var roomName = _nameText.text;
            
            if(string.IsNullOrEmpty(roomName)) return;
            
            SpaceRoomManager.RequestJoinRoom(roomName);
        }

        private void Awake()
        {
            _connectButton?.onClick.AddListener(OnConnect);
        }
    }
}