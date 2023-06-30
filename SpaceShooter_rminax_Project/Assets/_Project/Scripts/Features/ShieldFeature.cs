using System.Collections;
using Mirror;
using UnityEngine;

namespace _Project.Scripts.Features
{
    using Spaceship;
    
    public class ShieldFeature : SpaceshipFeature
    {
        [Header("Shield Settings")]
        [SerializeField] private GameObject _shieldPrefab;

        private GameObject _spawnedShieldObj;
        
        public override void AddFeature()
        {
            CMD_SpawnShield();

            SpaceshipController.instance.Health.isDamageable = false;

            _featureCoroutine = StartCoroutine(RemoveFeatureToDuration());
        }
        
        public override void RemoveFeature()
        {
            SpaceshipController.instance.Health.isDamageable = true;
            
            CMD_DestroyShield();
        }

        [Command(requiresAuthority = false)]
        private void CMD_SpawnShield()
        {
            _spawnedShieldObj = Instantiate(_shieldPrefab);
            NetworkServer.Spawn(_spawnedShieldObj);
        }

        [Command(requiresAuthority = false)]
        private void CMD_DestroyShield()
        {
            if (_spawnedShieldObj == null) return;
            
            NetworkServer.Destroy(_spawnedShieldObj);
        }

        private void Update()
        {
            if (_spawnedShieldObj == null) return;

            var targetPos = SpaceshipController.instance.CachedTransform.position;
            
            _spawnedShieldObj.transform.position = targetPos;
        }
    }
}