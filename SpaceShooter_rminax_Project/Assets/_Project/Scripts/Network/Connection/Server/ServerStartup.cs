using System;
using Mirror;
using UnityEngine;
using Unity.Services.Core;
using Sirenix.OdinInspector;
using System.Threading.Tasks;
using Unity.Services.Multiplay;

namespace _Project.Scripts.Network.Connection
{
    public class ServerStartup : MonoBehaviour
    {
        [Title("Connection Settings")] [SerializeField]
        private TelepathyTransport _transport;

        [SerializeField] private string _internalServerIP = "0.0.0.0";

        private ushort _serverPort;

        private async void Start()
        {
            var _currentServerFPS = 60;

            var args = Environment.GetCommandLineArgs();

            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];

                if (arg == "-port" && i + 1 < args.Length)
                {
                    _serverPort = ushort.Parse(args[i + 1]);
                }

                if (arg == "-fps" && i + 1 < arg.Length)
                {
                    var fps = int.Parse(args[i + 1]);

                    _currentServerFPS = fps;
                }
            }
            
            if (Application.isBatchMode) // it's Server
            {
                SetTargetFps(_currentServerFPS);
                await StartServerServices();
                return;
            }
            // it's Client
        }

        private void StartServer(ushort port)
        {
            SetConnection(_internalServerIP, port);
            
            NetworkManager.singleton.StartServer();
        }

        private async Task StartServerServices()
        {
            await UnityServices.InitializeAsync();

            try
            {
                await MultiplayService.Instance.StartServerQueryHandlerAsync((ushort)NetworkManager.singleton.maxConnections,
                    "n/a", "n/a", "0", "n/a");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Something went wrong trying to set up the SQP Services:\n{e}");
            }

            var serverConfig = MultiplayService.Instance.ServerConfig;
            
            if (!string.IsNullOrEmpty(serverConfig.AllocationId)) return;
            
            Multiplay_Allocate();
        }

        private void Multiplay_Allocate()
        {
            var config = MultiplayService.Instance.ServerConfig;
            Debug.Log($"Awaiting Allocation. Server Config is:\n" +
                      $"-ServerID: {config.ServerId}\n" +
                      $"-AllocationID: {config.AllocationId}\n" +
                      $"-Port: {config.Port}\n" +
                      $"-QPort: {config.QueryPort}\n" +
                      $"-logs: {config.ServerLogDirectory}");

            var port = config.Port;

            StartServer(port);
        }

        private void SetConnection(string ipV4, ushort port)
        {
            NetworkManager.singleton.networkAddress = ipV4;
            _transport.Port = port;
        }

        private void SetTargetFps(int value)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = value;
        }
    }
}