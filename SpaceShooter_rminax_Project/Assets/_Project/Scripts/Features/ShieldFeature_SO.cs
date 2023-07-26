using UnityEngine;

namespace _Project.Scripts.Features
{
    using Spaceship;
    
    [CreateAssetMenu(menuName = "Features/Shield Feature", order = 1)]
    public class ShieldFeature_SO : Feature_SO
    {
        private GameObject _shieldPrefab;

        private GameObject _spawnedShieldObj;

        public override void OnStart(SpaceshipController ownedController)
        {
            OwnedController = ownedController;
            
            _shieldPrefab = Resources.Load<GameObject>("SpawnablePrefabs/Miscs/Shield");
            
            _spawnedShieldObj = Instantiate(_shieldPrefab);

            OwnedController.Health.SetDamageable(false);
        }

        public override void OnUpdate()
        {
            if (_spawnedShieldObj == null) return;
            
            var targetPos = OwnedController.CachedTransform.position;
            
            _spawnedShieldObj.transform.position = targetPos;
        }

        public override void OnEnd()
        {
            OwnedController.Health.SetDamageable(true);
            
            if (_spawnedShieldObj == null) return;
            
            Destroy(_spawnedShieldObj);
        }
    }
}