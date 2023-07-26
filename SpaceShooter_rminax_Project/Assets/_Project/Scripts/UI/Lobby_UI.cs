using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.UI
{
    using Network.Managers;
    
    public class Lobby_UI : NetworkBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button _openWorldButton;
        [SerializeField] private Button _findRoomButton;
        [SerializeField] private Button _createRoomButton;

        [Header("Username Settings")]
        [SerializeField] private TMP_InputField usernameField;
        
        [Command(requiresAuthority = false)]
        private void ConnectOpenWorld()
        {
            SpaceNetworkManager.singleton.ConnectOpenWorld();
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
            _openWorldButton.onClick.AddListener(ConnectOpenWorld);

            usernameField.onValueChanged.AddListener(OnUsernameFieldChanged);
        }

        private void Start()
        {
            if (!PlayerPrefs.HasKey("username")) return;

            usernameField.text = PlayerPrefs.GetString("username");
        }
    }
}