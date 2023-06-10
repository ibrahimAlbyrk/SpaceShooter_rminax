using _Project.Scripts.Spaceship;
using Mirror;
using UnityEngine;

namespace _Project.Scripts.Network.Managers
{
    public class SpaceNetworkManager : NetworkManager
    {
        #region Server Callbacks

        public override void OnStartServer()
        {
            base.OnStartServer();
            
            NetworkServer.RegisterHandler<CreateSpaceshipMessage>(OnCreatePlayer);
        }

        #endregion

        #region Client Callbacks

        public override void OnClientConnect()
        {
            base.OnClientConnect();

            var spaceshipMessage = new CreateSpaceshipMessage
            {
                SpaceshipPrefab = playerPrefab
            };
            
            NetworkClient.Send(spaceshipMessage);
        }

        #endregion

        private void OnCreatePlayer(NetworkConnectionToClient conn, CreateSpaceshipMessage createSpaceshipMessage)
        {
            var spaceshipObj = Instantiate(playerPrefab);

            if (!spaceshipObj.TryGetComponent(out SpaceshipManager manager)) return;
            
            manager.CreateSpaceship(Vector3.zero, Quaternion.identity);

            NetworkServer.AddPlayerForConnection(conn, spaceshipObj);
        }
    }

    public struct CreateSpaceshipMessage : NetworkMessage
    {
        public GameObject SpaceshipPrefab;
    }
}