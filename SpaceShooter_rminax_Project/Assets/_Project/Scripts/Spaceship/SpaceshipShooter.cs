﻿using Mirror;
using UnityEngine;

namespace _Project.Scripts.Spaceship
{
    public class SpaceshipShooter : NetworkBehaviour
    {
        private int b = 0;

        private float _fireDelayTimer;

        [field: SerializeField] public SpaceshipController.ShootingSettings m_shooting;
        [SerializeField] private SpaceshipController.CameraSettings m_camera;

        private SpaceshipController _controller;

        private bool _isFiring;

        [Client]
        private void Start()
        {
            _fireDelayTimer = m_shooting.bulletSettings.BulletFireDelay;

            _controller = GetComponent<SpaceshipController>();
        }

        [Client]
        private void Update()
        {
            if (_controller.Health.IsDead) return;

            if (!_controller.IsRunningMotor || !_controller.IsEnableControl) return;

            _isFiring = Input.GetMouseButton(0);
        }

        [Client]
        private void FixedUpdate()
        {
            if (!isOwned) return;
            if (!_isFiring) return;

            if (_controller.Health.IsDead) return;

            if (!_controller.IsRunningMotor || !_controller.IsEnableControl) return;

            var delay = m_shooting.bulletSettings.BulletFireDelay;
            if (_fireDelayTimer >= delay)
            {
                _fireDelayTimer = 0f;
                var mousePos = ScreenMousePosition();

                CMD_BulletShooting(gameObject, mousePos);
            }
            else
                _fireDelayTimer += Time.fixedDeltaTime;
        }

        [Client]
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
        private void CMD_BulletShooting(GameObject owner, Vector3 mousePos)
        {
            for (var x = 0; x < m_shooting.bulletSettings.BulletCount; x++)
            {
                for (var i = b; i < m_shooting.bulletSettings.BulletBarrels.Count; i++)
                {
                    var barrelTransform = m_shooting.bulletSettings.BulletBarrels[i].transform;

                    var bullet = Instantiate(m_shooting.bulletSettings.Bullet,
                        barrelTransform.position + barrelTransform.forward * (x * 50),
                        Quaternion.LookRotation(transform.forward, transform.up));

                    NetworkServer.Spawn(bullet, gameObject);

                    RPC_SetBulletConfiguration(owner, bullet, mousePos,
                        m_shooting.bulletSettings.BulletLifetime,
                        m_shooting.bulletSettings.BulletSpeed);

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
        private void RPC_SetBulletConfiguration(GameObject owner, GameObject bullet, Vector3 mousePos,
            float bulletLifeTime, float bulletSpeed)
        {
            bullet.GetComponent<BulletScript>().Init(owner, isEnemy: false, bulletLifeTime, bulletSpeed);

            bullet.transform.LookAt(mousePos);
        }
    }
}