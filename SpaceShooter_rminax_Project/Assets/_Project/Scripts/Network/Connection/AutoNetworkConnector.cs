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
                    Server sendedServer = default;
                    
                    var listServers = JsonUtility.FromJson<ListServers>("{\"ServerList\":" + json + "}");
                    foreach (var server in listServers.ServerList)
                    {
                        if (server.status != ServerStatus.ONLINE.ToString() && //Server is offline!
                            server.status != ServerStatus.ALLOCATED.ToString())
                        {
                            sendedServer = server;
                            break;
                        }
                        
                        //Server is online!
                        ConnectToServer(server.ip, (ushort)server.port);
                        return;
                    }
                    
                    CreateAllocationToServer(sendedServer);
                });
        }

        private static void CreateAllocationToServer(Server server)
        {
            var jsonRequestBody = JsonUtility.ToJson(new TokenExchangeRequest
            {
                scopes = new[]{"multiplay.allocations.create", "multiplay.allocations.list"}
            });
            
            WebRequests.PostJson(ServerSecrets.EXCHANGE_URL, request =>
                {
                    request.SetRequestHeader("Authorization", "Basic " + ServerSecrets.KEY_BASE_64);
                },
                jsonRequestBody,
                error => { print($"Error: {error}");},
                json =>
                {
                    PostAllocationHandler(server, json);
                });
        }
        
        private static void PostAllocationHandler(Server server, string json)
        {
            print(json);
            var tokenExchangeResponse = JsonUtility.FromJson<TokenExchangeResponse>(json);

            WebRequests.PostJson(ServerSecrets.ALLOCATIONS_URL, request =>
                {
                    request.SetRequestHeader("Authorization", "Bearer " + tokenExchangeResponse.accessToken);
                },
                JsonUtility.ToJson(new QueueAllocationRequest
                {
                    allocationId = ServerSecrets.ALLOCATION_ID,
                    buildConfigurationId = server.buildConfigurationID,
                    regionId = ServerSecrets.REGION_ID
                }),
                error =>
                {
                    print($"Error: {error}");
                },
                _ =>
                {
                    ConnectToServer(server.ip, (ushort)server.port);
                });
        }
    }
}