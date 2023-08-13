using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace _Project.Scripts.Network.Managers.Room
{
    public class RoomCreator : MonoBehaviour
    {
        [Title("Fields")]
        [SerializeField] private TMP_InputField _roomNameField;
        [SerializeField] private TMP_InputField _maxPlayerField;
        
        [Title("Buttons")]
        [SerializeField] private Button _createRoomButton;
        [SerializeField] private Button _exitButton;
        
        private void CreateRoom()
        {
            var roomInfo = new RoomInfo
            {
                Name = _roomNameField.text,
                MaxPlayers = int.Parse(_maxPlayerField.text),
                SceneName = "OpenWorld_Scene"
            };
            
            SpaceRoomManager.RequestCreateRoom(roomInfo);
        }

        private void ButtonEnableHandler()
        {
            var isInteractible = !string.IsNullOrEmpty(_roomNameField.text) && !string.IsNullOrEmpty(_maxPlayerField.text);
            
            _createRoomButton.interactable = isInteractible;
        }

        private void OnRoomNameValueChanged(string _) => ButtonEnableHandler();

        private void OnMaxPlayerValueChanged(string value)
        {
            if (int.TryParse(value, out var intValue))
            {
                if (intValue > 30)
                    _maxPlayerField.text = "30";
                if (intValue < 2)
                    _maxPlayerField.text = "2";
            }
            
            ButtonEnableHandler();
        }

        private void Awake()
        {
            _createRoomButton.interactable = false;
            
            _exitButton?.onClick.AddListener(() => gameObject.SetActive(false));
            _createRoomButton?.onClick.AddListener(CreateRoom);
            
            _roomNameField?.onValueChanged.AddListener(OnRoomNameValueChanged);
            _maxPlayerField?.onValueChanged.AddListener(OnMaxPlayerValueChanged);
        }
    }
}