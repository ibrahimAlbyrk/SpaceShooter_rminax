using Mirror;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using _Project.Scripts.Utilities;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Game.Mod
{
    using Spaceship;
    using ShrinkingArea;
    using Network.Managers.Room;

    [System.Serializable]
    public class ShrinkingAreaMod : OpenWorldMod
    {
        [Header("Shrinking Settings")]
        [SerializeField] private ShrinkingAreaSystem _shrinkingAreaSystem;

        private bool _isGameEnded;
        
        public override void StartOnServer()
        {
            //base.StartOnServer();

            //SpawnShrinkingAreaSystem();
        }

        public override void FixedRun()
        {
            if (!_isSpawned && !_isGameEnded) return;
            
            base.FixedRun();
            
            var livingShips = CalculateLivingShips();

            if (livingShips.Count() == 1)
            {
                var ship = livingShips.First();
                ship.OpenWinPanel();
                _isGameEnded = true;
            }
        }

        private IEnumerable<SpaceshipController> CalculateLivingShips()
        {
            var room = SpaceRoomManager.Instance.GetRoomOfScene(_manager.gameObject.scene);
            var connections = room.Connections;

            return from conn in connections
                select conn.identity.GetComponent<SpaceshipController>()
                into ship
                where ship != null
                where !ship.Health.IsDead
                select ship;
        }

        private void SpawnShrinkingAreaSystem()
        {
            var system = Object.Instantiate(_shrinkingAreaSystem.gameObject);
            
            SceneManager.MoveGameObjectToScene(system, _manager.gameObject.scene);
            
            NetworkServer.Spawn(system);
        }
    }
}