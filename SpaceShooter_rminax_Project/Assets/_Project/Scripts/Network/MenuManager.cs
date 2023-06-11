using _Project.Scripts.Network.Managers;
using UnityEngine;

namespace _Project.Scripts.Network
{
    public class MenuManager : MonoBehaviour
    {
        public void ConnectOpenWorld()
        {
            ConnectToNetwork(); // TODO: like this for now
            
            SpaceNetworkManager.singleton.ConnectOpenWorld();
        }

        public void FindMatch()
        {
            
        }
        
        private void ConnectToNetwork()
        {
            //SpaceNetworkManager.singleton.StartClient();
            SpaceNetworkManager.singleton.StartHost();
        }
    }
}