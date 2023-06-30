using UnityEngine;

namespace _Project.Scripts.Features
{
    using Spaceship;
    
    public class FireFeature : SpaceshipFeature
    {
        [Header("Fire Settings")]
        [SerializeField] private float _increaseFireDelay = .1f;
        [SerializeField] private float _increaseSpeed = 50f;
        
        public override void AddFeature()
        {
            SpaceshipController.instance.Shooter.m_shooting.bulletSettings.BulletFireDelay -= _increaseFireDelay;
            SpaceshipController.instance.Shooter.m_shooting.bulletSettings.TargetDistance += _increaseSpeed;
            
            _featureCoroutine = StartCoroutine(RemoveFeatureToDuration());
        }

        public override void RemoveFeature()
        {
            SpaceshipController.instance.Shooter.m_shooting.bulletSettings.BulletFireDelay += _increaseFireDelay;
            SpaceshipController.instance.Shooter.m_shooting.bulletSettings.TargetDistance -= _increaseSpeed;
        }
    }
}