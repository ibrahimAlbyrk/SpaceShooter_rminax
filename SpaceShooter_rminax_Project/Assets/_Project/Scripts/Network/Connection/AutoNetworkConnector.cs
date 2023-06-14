using UnityEngine;

namespace _Project.Scripts.Network.Connection
{
    using Managers;
    
    public class AutoNetworkConnector : MonoBehaviour
    {
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
            
            SpaceNetworkManager.OnConnectedClient += CloseConnectionPanel;
            
            if (!Application.isBatchMode) //Headless build
            {
                SpaceNetworkManager.singleton.networkAddress = "13.50.196.147";
                SpaceNetworkManager.singleton.StartClient();
                
                Debug.Log("<color=green>=====Client Connected===</color>");
                return;
            }
            
            Debug.Log("<color=green>=====Server Starting===</color>");
        }

        private void OpenConnectionPanel() => _connectingPanel.SetActive(true);
        private void CloseConnectionPanel() => _connectingPanel.SetActive(false);
    }
}