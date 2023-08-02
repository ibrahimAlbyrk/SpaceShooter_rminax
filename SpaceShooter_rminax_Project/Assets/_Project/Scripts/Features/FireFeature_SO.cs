using _Project.Scripts.Utilities;
using UnityEngine;

namespace _Project.Scripts.Features
{
    using Spaceship;
    
    [CreateAssetMenu(menuName = "Features/Fire Feature", order = 0)]
    public class FireFeature_SO : Feature_SO
    {
        [Header("Fire Settings")]
        [SerializeField] private Vector2 _increaseBulletCountRange = new (2, 4);
        [SerializeField] private Vector2 _increaseTargetDistanceRange = new (50, 300);
        [SerializeField] private Vector2 _increaseSpeedRange = new (200, 500);

        private float _bulletCount;
        private float _targetDistance;
        private float _speed;

        public override void OnStart(SpaceshipController ownedController)
        {
            OwnedController = ownedController;
            
            SetBulletSettings();
            
            if(_bulletCount != 0)
                OwnedController.Shooter.m_shooting.bulletSettings.BulletCount += _bulletCount;
            
            if(_targetDistance != 0)
                OwnedController.Shooter.m_shooting.bulletSettings.TargetDistance += _targetDistance;
            
            if(_speed != 0)
                OwnedController.Shooter.m_shooting.bulletSettings.BulletSpeed += _speed;
        }

        public override void OnUpdate()
        {
        }

        public override void OnEnd()
        {
            if(_bulletCount != 0)
                OwnedController.Shooter.m_shooting.bulletSettings.BulletCount -= _bulletCount;
            
            if(_targetDistance != 0)
                OwnedController.Shooter.m_shooting.bulletSettings.TargetDistance -= _targetDistance;
            
            if(_speed != 0)
                OwnedController.Shooter.m_shooting.bulletSettings.BulletSpeed -= _speed;
        }

        private void SetBulletSettings()
        {
            _bulletCount = _increaseBulletCountRange.GetRandomValue();
            _targetDistance = _increaseTargetDistanceRange.GetRandomValue();
            _speed = _increaseSpeedRange.GetRandomValue();
        }
    }
}