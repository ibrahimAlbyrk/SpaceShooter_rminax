using System;
using Mirror;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using _Project.Scripts.Utilities;
using Random = UnityEngine.Random;

namespace _Project.Scripts.AI
{
    using Network;
    using Spaceship;
    
    public class BasicAI : NetIdentity
    {
        #region Serialize Var

        [Header("Setup Settings")] public Transform origin;
        public Transform barrel;

        public GameObject bullet;

        [Header("General Settings")] public float timeInThreatenedMode = 20f;
        public float aggresiveSpeed = 2.75f;
        public float aggresiveTurnSpeed = 0.65f;
        public float normalSpeed = 1f;
        public float normalTurnSpeed = 10f;

        [SerializeField] private float _fireCountDown = .5f;

        [Header("Patrol Settings")] public float patrolRange = 1000f;
        public int pointCount = 50;

        public bool aggresive;

        [Header("Detection Settings")] [SerializeField]
        private LayerMask _detectionLayer;

        [SerializeField] public float DetectionRange = 150f;
        [SerializeField] private float _stopRange = 20f;

        [SerializeField] private float _bulletLifeTime = 5f;
        [SerializeField] private float _bulletSpeed = 150f;
        
        #endregion

        #region Private Vars

        private readonly List<Vector3> _points = new();

        private Transform _targetPlayer;

        private Rigidbody _rigid;

        private int _pointIndex;

        private float _fireTimer;
        
        private float prevspeed;
        
        private float prevturn;

        private CancellationTokenSource _destroyCancellationTokenSource;

        #endregion

        #region Base Methods

        private Vector3 _firstPosition;

        [ServerCallback]
        private void Start()
        {
            for (var i = 0; i < pointCount; i++)
            {
                var point = origin.position + Random.insideUnitSphere * patrolRange;

                _points.Add(point);
            }

            _pointIndex = 0;

            _fireTimer = _fireCountDown;

            _firstPosition = origin.position;
        }

        [ServerCallback]
        private void FixedUpdate()
        {
            _targetPlayer = FindPlayer();

            if (_targetPlayer != null && _targetPlayer.gameObject.activeSelf) Chase();
            else Go();
        }

        private void OnDestroy()
        {
            _destroyCancellationTokenSource?.Cancel();
        }
        
        #endregion

        #region Threat Methods

        public void Threat()
        {
            if (!aggresive)
                Threatened();
        }

        [Command(requiresAuthority = false)]
        private void Threatened()
        {
            StartCoroutine(Cor_Threatened());
        }

        [Server]
        private IEnumerator Cor_Threatened()
        {
            prevturn = normalTurnSpeed;
            prevspeed = normalSpeed;
            normalSpeed = aggresiveSpeed;
            normalTurnSpeed = aggresiveTurnSpeed;

            yield return new WaitForSeconds(timeInThreatenedMode);

            normalSpeed = prevspeed;
            normalTurnSpeed = prevturn;
        }

        #endregion

        #region Chase Methods

        private Transform FindPlayer()
        {
            var colls = Physics.OverlapSphere(origin.position, DetectionRange, _detectionLayer);

            return (from coll in colls
                    where coll.CompareTag("Spaceship")
                    select coll.transform)
                .FirstOrDefault(ship => !ship.GetComponent<Health>().IsDead);
        }

        private void Chase()
        {
            var distance = (transform.position - _targetPlayer.position).sqrMagnitude;

            var dir = transform.forward * aggresiveSpeed;

            if (distance <= _stopRange * _stopRange)
                dir = Vector3.zero;

            transform.Translate(dir, Space.World);
            transform.rotation = Quaternion.Lerp(transform.rotation,
                Quaternion.LookRotation(_targetPlayer.position - transform.position, transform.up),
                Time.deltaTime / aggresiveTurnSpeed);

            if (_fireTimer >= _fireCountDown)
            {
                Fire();
                _fireTimer = 0f;
            }
            else _fireTimer += Time.fixedDeltaTime;
        }

        private void Go()
        {
            var _currentMovePosition = _points[_pointIndex];

            var outDistance = MathUtilities.OutDistance(transform.position, _currentMovePosition, 15);
            
            if (outDistance)
            {
                transform.Translate(transform.forward * normalSpeed, Space.World);
                transform.rotation = Quaternion.Lerp(transform.rotation,
                    Quaternion.LookRotation(_currentMovePosition - transform.position, transform.up),
                    Time.fixedDeltaTime);
                return;
            }

            _pointIndex++;

            if (_pointIndex > _points.Count - 1)
                _pointIndex = 0;
        }

        #endregion

        #region Fire Methods

        private async void Fire()
        {
            var bulletObj = Instantiate(bullet, barrel.position,
                Quaternion.LookRotation(transform.forward, transform.up));

            NetworkServer.Spawn(bulletObj);

            _destroyCancellationTokenSource = new CancellationTokenSource();
            
            try
            {
                await Task.Delay(100, _destroyCancellationTokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                // GameObject has been destroyed, so we stop here
                return;
            }

            RPC_SetBulletSettings(gameObject, bulletObj, _bulletLifeTime, _bulletSpeed);
        }

        [Command(requiresAuthority = false)]
        private void CMD_SetBulletSettings(GameObject owner, GameObject bulletObj, float bulletLifeTime, float bulletSpeed) =>
            RPC_SetBulletSettings(owner, bulletObj, bulletLifeTime, bulletSpeed);

        [ClientRpc]
        private void RPC_SetBulletSettings(GameObject owner, GameObject bulletObj, float bulletLifeTime, float bulletSpeed)
        {
            if (owner == null || bulletObj == null) return;
            
            bulletObj.GetComponent<BulletScript>().Init(owner, isEnemy: true, bulletLifeTime, bulletSpeed);
        }

        #endregion

        private void OnDrawGizmosSelected()
        {
            if (origin == null) return;
            
            Gizmos.color = Color.red;

            Gizmos.DrawWireSphere(origin.position, DetectionRange);

            if (!Application.isPlaying)
            {
                Gizmos.color = Color.yellow;

                Gizmos.DrawWireSphere(origin.position, _stopRange);   
            }
            else
            {
                for (var i = 0; i < pointCount; i++)
                {
                    var pos = _points[i];
                    
                    Gizmos.color = new Color(0.61f, 0.4f, 1f);
                    
                    Gizmos.DrawSphere(pos, 1f);
                    
                    if(i + 1 >= pointCount) break;
                    
                    var nextPos = _points[i + 1];
                    
                    Gizmos.color = Color.blue;
                    
                    Gizmos.DrawLine(pos, nextPos);
                }
            }

            Gizmos.color = Color.green;

            Gizmos.DrawWireSphere(!Application.isPlaying ? origin.position : _firstPosition, patrolRange);
        }
    }
}