using Unity.Services.Multiplay;
using UnityEngine;

namespace _Project.Scripts.Network.Connection
{
    using Web;
    using Managers;

    public class AutoNetworkConnector : MonoBehaviour
    {
        private void Start()
        {
            FindOnlineServer();
        }

        private static void ConnectToServer(string ipv4, ushort port)
        {
            SpaceNetworkManager.singleton.ConnectClient(ipv4, port);
        }

        private static void FindOnlineServer()
        {
            WebRequests.Get(ServerSecrets.SERVER_URL,
                request => { request.SetRequestHeader("Authorization", $"Basic {ServerSecrets.KEY_BASE_64}"); },
                errorMessage => { Debug.Log($"Error: {errorMessage}"); },
                json =>
                {
                    var listServers = JsonUtility.FromJson<ListServers>("{\"ServerList\":" + json + "}");
                    foreach (var server in listServers.ServerList)
                    {
                        print(server.ip);
                        if (server.status == ServerStatus.ONLINE.ToString() ||
                            server.status == ServerStatus.ALLOCATED.ToString())
                        {
                            //Server is online!
                            print($"Online server was found: {server.ip}:{server.port}");
                            ConnectToServer(server.ip, (ushort)server.port);
                            return;
                        }
                        else
                            CreateAllocationToServer(server);
                    }
                });
        }

        private static void CreateAllocationToServer(Server server)
        {
            var jsonRequestBody = JsonUtility.ToJson(new TokenExchangeRequest
            {
                scopes = new[]{"multiplay.allocations.create", "multiplay.allocations.list"}
            });

            var url = ServerSecrets.GetExchangeURL(server.fleetID);
            
            WebRequests.PostJson(url, request => { },
                jsonRequestBody,
                error => { print($"Error: {error}");},
                json =>
                {
                    var tokenExchangeResponse = JsonUtility.FromJson<TokenExchangeResponse>(json);

                    WebRequests.PostJson(url, request =>
                    {
                        request.SetRequestHeader("Authorization", "Bearer " + tokenExchangeResponse.accessToken);
                    },
                        JsonUtility.ToJson(new QueueAllocationRequest
                        {
                            allocationId = "",
                            buildConfigurationId = server.buildConfigurationID,
                            regionId = ""
                        }),
                        error =>
                        {
                            print($"Error: {error}");
                        },
                        json =>
                        {
                            print($"Success: {json}");
                        });
                });
        }
    }
    
    [System.Serializable]
    public class TokenExchangeResponse {
        public string accessToken;
    }


    [System.Serializable]
    public class TokenExchangeRequest {
        public string[] scopes;
    }
    
    [System.Serializable]
    public class QueueAllocationRequest {
        public string allocationId;
        public int buildConfigurationId;
        public string payload;
        public string regionId;
        public bool restart;
    }
}