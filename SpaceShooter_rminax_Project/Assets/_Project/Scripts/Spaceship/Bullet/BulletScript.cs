using Mirror;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using _Project.Scripts.Utilities;

namespace _Project.Scripts.Spaceship
{
    using AI;
    using Game;
    
    public class BulletScript : NetworkBehaviour
    {
        [SerializeField] private LayerMask _obstacleLayer;

        [Tooltip("The particle effect of hitting something.")]
        public GameObject HitEffect;

        public ParticleSystem Trail;

        private bool _isHit;
        private bool _isMove = true;

        private GameObject _owner;

        private bool _init;

        private bool _isEnemy;

        private float _bulletLifeTime;
        private float _bulletSpeed;
        private float _bulletDamage;

        private PhysicsScene _physicsScene;

        public void Init(GameObject owner, bool isEnemy = false, float bulletDamage = 1f, float bulletLifeTime = 3f, float bulletSpeed = 100f)
        {
            _owner = owner;

            _bulletLifeTime = bulletLifeTime;
            _bulletSpeed = bulletSpeed;
            _bulletDamage = bulletDamage;

            Invoke(nameof(Lifetime), _bulletLifeTime);

            _init = true;

            _isEnemy = isEnemy;

            _physicsScene = gameObject.scene.GetPhysicsScene();
        }
        
        [ServerCallback]
        private void Update()
        {
            if (!_init) return;
            if (!_isMove) return;

            transform.position += transform.forward * (_bulletSpeed * Time.fixedDeltaTime);

            var detectedColls = new Collider[3];

            var detectCount = _physicsScene.OverlapBox(transform.position, transform.localScale, detectedColls, transform.rotation,
                _obstacleLayer);

            if (detectCount < 1 || _isHit) return;
            if (detectedColls
                .Where(coll => coll != null)
                .Any(coll => coll.gameObject == _owner)) return;
            
            _isHit = true;

            var obstacle = detectedColls.FirstOrDefault()?.gameObject;
            
            TakeDamageToObstacle(_owner, obstacle);
        }
        
        [ServerCallback]
        private void TakeDamageToObstacle(GameObject owner, GameObject obstacle)
        {
            if (obstacle.gameObject == owner) return;

            var username = "";
            
            if(!_isEnemy)
                username = owner.GetComponent<SpaceshipController>().Username;

            var Damageable = obstacle.GetComponent<Damageable>();

            if (Damageable != null)
            {
                if (Damageable.GetHealth() <= 0)
                {
                    AddScore(username, 30);
                }
                else
                {
                    Damageable.DealDamage(_bulletDamage);   
                }
                return;
            }

            var destructionScript = obstacle.GetComponent<DestructionScript>();
            if (destructionScript != null)
            {
                if (destructionScript.HP <= 0) return;
                
                destructionScript.HP -= _bulletDamage;
                
                var basicAI = destructionScript.GetComponent<BasicAI>();

                var isDead = destructionScript.HP <= 0;

                if (basicAI != null)
                {
                    basicAI.Threat();
                    
                    if (isDead) AddScore(username, 30);
                }
                else if
                    (isDead) AddScore(username, 1);
            }
            
            StartCoroutine(DestroySequence());
        }

        [ServerCallback]
        private void AddScore(string username, int value)
        {
            if (_isEnemy) return;
            
            LeaderboardManager.Instance?.AddScore(username, value);
        }

        [ServerCallback]
        private IEnumerator DestroySequence()
        {
            _isMove = false;
            
            RPC_CloseVisual();

            RPC_SpawnFireworks(transform.position, Quaternion.identity, 1);

            yield return new WaitForSeconds(1.1f);
            
            NetworkServer.Destroy(gameObject);
        }

        private void Lifetime() => NetworkServer.Destroy(gameObject);

        [ClientRpc]
        private void RPC_CloseVisual()
        {
            if (Trail == null) return;
            
            Trail.gameObject.SetActive(false);
        }
        
        [ClientRpc]
        private async void RPC_SpawnFireworks(Vector3 pos, Quaternion rot, int destroyCountdown)
        {
            if (HitEffect == null) return;
        
            var firework = Instantiate(HitEffect, pos, rot);
            
            if (firework == null) return;

            firework.GetComponentInChildren<ParticleSystem>().Play();

            await Task.Delay(destroyCountdown * 1000);

            firework.Destroy();
        }
    }
}