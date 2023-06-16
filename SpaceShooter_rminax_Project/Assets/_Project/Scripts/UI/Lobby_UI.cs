using Mirror;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.UI
{
    using Network.Managers;
    
    public class Lobby_UI : NetworkBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button _openWorldButton; 
        
        [Command(requiresAuthority = false)]
        private void ConnectOpenWorld()
        {
            SpaceNetworkManager.singleton.ConnectOpenWorld();
        }

        private void Awake()
        {
            _openWorldButton.onClick.AddListener(ConnectOpenWorld);
        }
    }
}