using System;
using Mirror;
using UnityEngine;

namespace _Project.Scripts.Spaceship
{
    public class SpaceshipManager : NetworkBehaviour
    {
        [SerializeField] private GameObject _spaceshipPrefab;

        private SpaceshipController _spaceshipController;

        private GameObject _ctx;

        private NetworkMatch _networkMatch;

        public void Init(Guid matchID)
        {
            _networkMatch.matchId = matchID;
        }
        
        public void CreateSpaceship(Vector3 position, Quaternion rotation)
        {
            var spaceshipObj = Instantiate(_spaceshipPrefab, position, rotation);
            
            spaceshipObj.GetComponent<NetworkMatch>().matchId = _networkMatch.matchId;

            var controller = spaceshipObj.GetComponentInChildren<SpaceshipController>();

            if (controller == null) return;
            
            _spaceshipController = controller;
            
            _spaceshipController.Init();

            _ctx = _spaceshipController.transform.parent.gameObject;

            NetworkServer.Spawn(spaceshipObj, connectionToClient);
        }

        public void RemoveSpaceship()
        {
            NetworkServer.Destroy(_ctx);
            Destroy(_ctx);
        }

        public override void OnStopClient()
        {
            base.OnStopClient();

            RemoveSpaceship();
        }

        private void Awake()
        {
            _networkMatch = GetComponent<NetworkMatch>();
        }
        
        [Client]
        private void Update()
        {
            if (!isLocalPlayer) return;
            if (_spaceshipController == null) return;
            
            _spaceshipController.OnUpdate();
        }

        [Client]
        private void FixedUpdate()
        {
            if (!isLocalPlayer) return;
            if (_spaceshipController == null) return;
            
            _spaceshipController.OnFixedUpdate();
        }
        
        [Client]
        private void LateUpdate()
        {
            if (!isLocalPlayer) return;
            if (_spaceshipController == null) return;
            
            _spaceshipController.OnLateUpdate();
        }
    }
}