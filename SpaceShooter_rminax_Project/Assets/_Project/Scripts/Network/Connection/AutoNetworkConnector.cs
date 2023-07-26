using Mirror;
using UnityEngine;

namespace _Project.Scripts.Network.Connection
{
    using Managers;
    
    public class AutoNetworkConnector : MonoBehaviour
    {
        [SerializeField] private bool isLocal;
        [SerializeField] private bool isHost;
        [SerializeField] private string networkAddress = "13.50.196.147";
        [SerializeField] private GameObject _connectingPanel;
        
        public void HostLocal()
        {
            SpaceNetworkManager.singleton.networkAddress = "localhost";
            SpaceNetworkManager.singleton.StartHost();
        }
    
        public void JoinLocal()
        {
            SpaceNetworkManager.singleton.networkAddress = "localhost";
            SpaceNetworkManager.singleton.StartClient();
        }
        
        private void Start()
        {
            OpenConnectionPanel();

            if (NetworkServer.active)
            {
                CloseConnectionPanel();
                return;
            }
            
            SpaceNetworkManager.OnClientConnected += CloseConnectionPanel;
            
            if (isHost)
            {
                SpaceNetworkManager.singleton.networkAddress = isLocal ? "localhost" : networkAddress;
                SpaceNetworkManager.singleton.StartHost();
                
                Debug.Log("<color=green>=====Client Connected And Starting as host===</color>");
                
                return;
            }
            
            if (!Application.isBatchMode) //Headless build
            {
                SpaceNetworkManager.singleton.networkAddress = isLocal ? "localhost" : networkAddress;
                SpaceNetworkManager.singleton.StartClient();
                
                Debug.Log("<color=green>=====Client Connected===</color>");
                return;
            }
            
            SpaceNetworkManager.singleton.networkAddress = networkAddress;
            
            Debug.Log("<color=green>=====Server Starting===</color>");
        }

        private void OpenConnectionPanel() => _connectingPanel.SetActive(true);
        private void CloseConnectionPanel() => _connectingPanel.SetActive(false);
    }
}