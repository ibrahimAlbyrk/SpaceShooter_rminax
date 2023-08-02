using System;
using Mirror;
using UnityEngine;
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

        [ClientCallback]
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
                    BulletDamage = m_shooting.bulletSettings.BulletDamage,
                    BulletSpeed = m_shooting.bulletSettings.BulletSpeed + _controller.CurrentSpeed,
                    BulletLifetime = m_shooting.bulletSettings.BulletLifetime,
                    BulletCount = m_shooting.bulletSettings.BulletCount
                };
                
                //var dir = _controller.CachedTransform.position * (_controller.CurrentSpeed * Time.fixedDeltaTime);
                //var dir = _controller.CachedTransform.localPosition + _controller.CachedTransform.forward * (_controller.SpeedFactor * Time.fixedDeltaTime);
                
                CMD_BulletShooting(gameObject, mousePos, bulletInfo);

                _shakers.ForEach(shaker => shaker?.Play());
            }
        }

        [ClientCallback]
        private Vector3 ScreenMousePosition()
        {
            if (m_camera.TargetCamera.targetTexture == null)
            {
                return m_camera.TargetCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,
                    Input.mousePosition.y, m_shooting.bulletSettings.TargetDistance));
            }

            //EVERYTHING MUST BE A FLOAT
            return m_camera.TargetCamera.ScreenToWorldPoint(new Vector3(
                Input.mousePosition.x / (Screen.height / (float)m_camera.TargetCamera.pixelHeight),
                Input.mousePosition.y / (Screen.height / (float)m_camera.TargetCamera.pixelHeight),
                m_shooting.bulletSettings.TargetDistance));
        }

        [Command]
        private void CMD_BulletShooting(GameObject owner, Vector3 mousePos, BulletInfo bulletInfo)
        {
            for (var x = 0; x < bulletInfo.BulletCount; x++)
            {
                for (var i = b; i < m_shooting.bulletSettings.BulletBarrels.Count; i++)
                {
                    var barrelTransform = m_shooting.bulletSettings.BulletBarrels[i].transform;

                    var pos = barrelTransform.position;

                    var bullet = Instantiate(m_shooting.bulletSettings.Bullet,
                        pos + barrelTransform.forward * (x * 50),
                        Quaternion.LookRotation(transform.forward, transform.up));

                    SceneManager.MoveGameObjectToScene(bullet, gameObject.scene);

                    bullet.GetComponent<BulletScript>().Init(owner, isEnemy: false, 
                        bulletInfo.BulletDamage,
                        bulletInfo.BulletLifetime,
                        bulletInfo.BulletSpeed);
                    
                    bullet.transform.LookAt(mousePos);

                    NetworkServer.Spawn(bullet, gameObject);

                    bullet.transform.LookAt(mousePos);
                    
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

        //[ClientRpc]
        //private void RPC_SetBulletConfiguration(GameObject owner, GameObject bullet, Vector3 mousePos,
        //    float bulletLifeTime, float bulletSpeed)
        //{
        //    bullet.GetComponent<BulletScript>().Init(owner, isEnemy: false, bulletLifeTime, bulletSpeed);
        //    
        //    bullet.transform.LookAt(mousePos);
        //}

        [Serializable]
        public struct BulletInfo
        {
            public float BulletCount;
            public float BulletSpeed;
            public float BulletLifetime;
            public float BulletDamage;
        }
    }
}