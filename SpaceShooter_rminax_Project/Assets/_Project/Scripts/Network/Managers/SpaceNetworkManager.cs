using Mirror;
using UnityEngine;

namespace _Project.Scripts.Network.Managers
{
    using Spaceship;
    
    public class SpaceNetworkManager : NetworkManager
    {
        public new static SpaceNetworkManager singleton;

        [SerializeField] private int _maxConnection;
        
        [Scene] [SerializeField] private string _menuScene;
        [Scene] [SerializeField] private string _OpenWorldScene;
        
        #region Server Callbacks

        public override void OnStartServer()
        {
            base.OnStartServer();
            
            NetworkServer.RegisterHandler<CreateSpaceshipMessage>(OnCreatePlayer);
        }

        public override void OnClientSceneChanged()
        {
            base.OnClientSceneChanged();

            if (networkSceneName != _OpenWorldScene) return;
            
            var spaceshipMessage = new CreateSpaceshipMessage
            {
                SpaceshipPrefab = playerPrefab
            };
            
            NetworkClient.Send(spaceshipMessage);
        }

        #endregion

        public void ConnectOpenWorld()
        {
            ServerChangeScene(_OpenWorldScene);
        }

        private void OnCreatePlayer(NetworkConnectionToClient conn, CreateSpaceshipMessage createSpaceshipMessage)
        {
            var spaceshipObj = Instantiate(playerPrefab);
            
            NetworkServer.Spawn(spaceshipObj, conn);

            NetworkServer.AddPlayerForConnection(conn, spaceshipObj);
            
            if (!spaceshipObj.TryGetComponent(out SpaceshipManager manager)) return;
            
            manager.CreateSpaceship(Vector3.zero, Quaternion.identity);
        }

        public override void Awake()
        {
            base.Awake();

            singleton = this;

            maxConnections = _maxConnection;
        }
    }

    public struct CreateSpaceshipMessage : NetworkMessage
    {
        public GameObject SpaceshipPrefab;
    }
}