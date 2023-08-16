using Mirror;
using UnityEngine;

namespace _Project.Scripts.Network.Connection
{
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

            if (onServer)
            {
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 120;
                
                if(!NetworkServer.active)
                    NetworkManager.singleton.StartServer();
            }
            else
                NetworkManager.singleton.StartClient();
        }
    }
}