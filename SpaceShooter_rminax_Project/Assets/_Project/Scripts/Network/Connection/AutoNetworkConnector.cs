using Mirror;
using UnityEngine;

namespace _Project.Scripts.Network.Connection
{
    using Web;
    using Managers;

    public class AutoNetworkConnector : MonoBehaviour
    {
        [SerializeField] private bool _autoConnect;

        [SerializeField] private string _serverConnectionAddress;
        [SerializeField] private string _clientConnectionAddress;

        private void Start()
        {
            if (!_autoConnect) return;

            var onServer = Application.isBatchMode;

            NetworkManager.singleton.networkAddress = onServer
                ? _serverConnectionAddress
                : _clientConnectionAddress;
            
            if(onServer)
                NetworkManager.singleton.StartServer();
            else
                NetworkManager.singleton.StartClient();
        }
    }
}