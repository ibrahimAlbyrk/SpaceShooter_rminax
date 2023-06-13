using _Project.Scripts.Network.Managers;
using UnityEngine;

namespace _Project.Scripts.Network.Connection
{
    public class AutoHostClient : MonoBehaviour
    {
        public void JoinLocal()
        {
            SpaceNetworkManager.singleton.networkAddress = "localhost";
            SpaceNetworkManager.singleton.StartClient();
        }
        
        private void Start()
        {
            if (!Application.isBatchMode) //Headless build
            {
                Debug.Log("<color=green>=====Client Connected===</color>");
                SpaceNetworkManager.singleton.StartClient();
                
                return;
            }
            
            Debug.Log("<color=green>=====Server Starting===</color>");
        }
    }
}