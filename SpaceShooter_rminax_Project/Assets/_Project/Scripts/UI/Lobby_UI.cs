using TMPro;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.UI
{
    using Network.Managers.Room;
    
    public class Lobby_UI : NetworkBehaviour
    {
        [SerializeField] private RoomSearcher _roomSearcher;
        [SerializeField] private RoomCreator _roomCreator;
        
        [Header("Buttons")]
        [SerializeField] private Button _openWorldButton;
        [SerializeField] private Button _findRoomButton;
        [SerializeField] private Button _createRoomButton;

        [Header("Username Settings")]
        [SerializeField] private TMP_InputField usernameField;
        
        private void ConnectOpenWorld()
        {
            SpaceRoomManager.RequestJoinRoom("OpenWorld");
        }

        private void CreateRoom()
        {
            _roomCreator.gameObject.SetActive(true);
        }

        private void FindRoom()
        {
            _roomSearcher.gameObject.SetActive(true);
            _roomSearcher.UpdateUI();
        }

        private void OnUsernameFieldChanged(string value)
        {
            var isIncorrect = value.Length < 3 || string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value);

            _openWorldButton.interactable = !isIncorrect;
            _findRoomButton.interactable = !isIncorrect;
            _createRoomButton.interactable = !isIncorrect;
            
            if (isIncorrect) return;

            PlayerPrefs.SetString("username", value);
        }

        private void Awake()
        {
            _openWorldButton?.onClick.AddListener(ConnectOpenWorld);
            _findRoomButton?.onClick.AddListener(FindRoom);
            _createRoomButton?.onClick.AddListener(CreateRoom);
            
            usernameField?.onValueChanged.AddListener(OnUsernameFieldChanged);
        }

        private void Start()
        {
            if (!PlayerPrefs.HasKey("username")) return;

            usernameField.text = PlayerPrefs.GetString("username");
        }
    }
}