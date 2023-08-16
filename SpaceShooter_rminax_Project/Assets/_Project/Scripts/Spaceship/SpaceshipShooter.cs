using System;
using Mirror;
using UnityEngine;
using System.Linq;
using Sirenix.Utilities;
using MoreMountains.Feedbacks;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Spaceship
{
    public class SpaceshipShooter : NetworkBehaviour
    {
        private int b;

        [SerializeField] public SpaceshipController.ShootingSettings m_shooting;
        [SerializeField] private SpaceshipController.CameraSettings m_camera;

        [SerializeField] private MMShaker[] _shakers;

        private SpaceshipController _controller;

        private void Start()
        {
            _controller = GetComponent<SpaceshipController>();
        }

        [ClientCallback]
        private void Update()
        {
            if (!isOwned) return;

            if (_controller.Health.IsDead) return;

            if (!_controller.IsRunningMotor || !_controller.IsEnableControl) return;

            if (Input.GetMouseButtonDown(0))
            {
                var mousePos = ScreenMousePosition();

                var bulletInfo = new BulletInfo
                {
                    BulletDamage = (byte)m_shooting.bulletSettings.BulletDamage,
                    BulletSpeed = (short)(m_shooting.bulletSettings.BulletSpeed + _controller.CurrentSpeed),
                    BulletLifetime = (byte)m_shooting.bulletSettings.BulletLifetime,
                    BulletCount = (byte)m_shooting.bulletSettings.BulletCount
                };

                var barrelPredictionData = new BarrelPredictionData
                {
                    LastBarrelPositions = m_shooting.bulletSettings.BulletBarrels
                        .Select(barrel => barrel.transform.position).ToArray(),
                };

                BulletShooting(gameObject, mousePos, bulletInfo, barrelPredictionData);

                _shakers.ForEach(shaker => shaker?.Play());
            }
        }

        [ClientCallback]
        private Vector3 ScreenMousePosition()
        {
            return m_camera.TargetCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,
                Input.mousePosition.y + 10, m_shooting.bulletSettings.TargetDistance));
        }
        
        public struct BarrelPredictionData
        {
            public Vector3[] LastBarrelPositions;
        }
        
        private Vector3 CalculatePredictedPosition(Vector3 originalPosition, float speed, float predictionFactor)
        {
            var direction = transform.forward;

            var futurePosition = originalPosition + direction * (speed * predictionFactor * Time.fixedDeltaTime);
            
            var predictedPosition = Vector3.Lerp(originalPosition, futurePosition, Time.fixedDeltaTime);

            return predictedPosition;
        }

        [Command]
        private void BulletShooting(GameObject owner, Vector3 mousePos, BulletInfo bulletInfo, BarrelPredictionData barrelPredictionData)
        {
            for (var x = 0; x < bulletInfo.BulletCount; x++)
            {
                for (var i = b; i < m_shooting.bulletSettings.BulletBarrels.Count; i++)
                {
                    var forward = m_shooting.bulletSettings.BulletBarrels[i].transform.forward;

                    var predictedPosition = CalculatePredictedPosition(barrelPredictionData.LastBarrelPositions[i],
                        _controller.CurrentSpeed, 200);
                    
                    var pos = predictedPosition + forward;

                    var bullet = Instantiate(m_shooting.bulletSettings.Bullet,
                        pos + forward * (x * 50),
                        Quaternion.LookRotation(transform.forward, transform.up));
                    
                    SceneManager.MoveGameObjectToScene(bullet, gameObject.scene);
                    
                    SetBulletConfiguration(owner, bullet,
                        mousePos,
                        bulletInfo.BulletDamage,
                        bulletInfo.BulletLifetime,
                        bulletInfo.BulletSpeed);
                    
                    NetworkServer.Spawn(bullet);
                    
                    RPC_SyncBullet(owner, bullet,
                        mousePos,
                        bulletInfo.BulletDamage,
                        bulletInfo.BulletLifetime,
                        bulletInfo.BulletSpeed,i, x);
                    
                    if (m_shooting.bulletSettings.BulletBarrels.Count > 1)
                    {
                        b = b == 0 ? 1 : 0;
                    }

                    var bulletParticle = m_shooting.bulletSettings.BulletBarrels[i].GetComponent<ParticleSystem>();

                    if (bulletParticle != null)
                    {
                        bulletParticle.Play();
                    }
                }
            }
        }

        [ClientRpc]
        private void RPC_SyncBullet(GameObject owner, GameObject bullet, Vector3 mousePos,
            float bulletDamage, float bulletLifeTime, float bulletSpeed, int index, int x)
        {
            var forward = m_shooting.bulletSettings.BulletBarrels[index].transform.forward;
            var pos = m_shooting.bulletSettings.BulletBarrels[index].transform.position;

            bullet.transform.position = pos + forward * (x * 50);
            
            SetBulletConfiguration(owner, bullet, mousePos, bulletDamage, bulletLifeTime, bulletSpeed);
        }
        
        private void SetBulletConfiguration(GameObject owner, GameObject bullet, Vector3 mousePos,
            float bulletDamage, float bulletLifeTime, float bulletSpeed)
        {
            bullet.GetComponent<BulletScript>().Init(owner, isEnemy: false, bulletDamage, bulletLifeTime, bulletSpeed);
            
            bullet.transform.LookAt(mousePos);
        }

        [Serializable]
        public struct BulletInfo
        {
            public byte BulletCount;
            public short BulletSpeed;
            public byte BulletLifetime;
            public byte BulletDamage;
        }
    }
}