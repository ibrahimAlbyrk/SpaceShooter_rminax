using UnityEngine;

namespace _Project.Scripts.Features
{
    using Spaceship;
    
    [CreateAssetMenu(menuName = "Features/Fire Feature", order = 0)]
    public class FireFeature_SO : Feature_SO
    {
        [Header("Fire Settings")]
        [SerializeField]private float _increaseBulletCount = 2;
        [SerializeField] private float _increaseSpeed = 50f;

        public override void OnStart(SpaceshipController ownedController)
        {
            OwnedController = ownedController;
            
            OwnedController.Shooter.m_shooting.bulletSettings.BulletCount += _increaseBulletCount;
            OwnedController.Shooter.m_shooting.bulletSettings.TargetDistance += _increaseSpeed;
        }

        public override void OnUpdate()
        {
        }

        public override void OnEnd()
        {
            OwnedController.Shooter.m_shooting.bulletSettings.BulletCount -= _increaseBulletCount;
            OwnedController.Shooter.m_shooting.bulletSettings.TargetDistance -= _increaseSpeed;
        }
    }
}