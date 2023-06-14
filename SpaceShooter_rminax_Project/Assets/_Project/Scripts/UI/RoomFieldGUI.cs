using System;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.UI
{
    using Network.Managers;
    using Network.Messages;
    
    public class RoomFieldGUI : MonoBehaviour
    {
        private Guid _roomID;

        [Header("GUI Elements")]
        [SerializeField] private Image _image;
        [SerializeField] private Toggle _toggleButton;
        [SerializeField] private TMP_Text _matchName;
        [SerializeField] private TMP_Text _playerCount;

        public void Awake()
        {
            _toggleButton.onValueChanged.AddListener(delegate { OnToggleClicked(); });
        }

        [ClientCallback]
        public void OnToggleClicked()
        {
            SpaceRoomManager.Singleton.SelectRoom(_toggleButton.isOn ? _roomID : Guid.Empty);
            _image.color = _toggleButton.isOn ? new Color(0f, 1f, 0f, 0.5f) : new Color(1f, 1f, 1f, 0.2f);
        }

        [ClientCallback]
        public Guid GetRoomID() => _roomID;

        [ClientCallback]
        public void SetRoomInfo(RoomInfo infos)
        {
            _roomID = infos.RoomID;
            _matchName.text = $"Match {infos.RoomID.ToString()[..8]}";
            _playerCount.text = $"{infos.Players} / {infos.MaxPlayers}";
        }
    }
}