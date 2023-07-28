using Mirror;
using System.Linq;
using UnityEngine;
using System.Collections;

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

        private PhysicsScene _physicsScene;

        public void Init(GameObject owner, bool isEnemy = false, float bulletLifeTime = 3f, float bulletSpeed = 100f)
        {
            _owner = owner;

            _bulletLifeTime = bulletLifeTime;
            _bulletSpeed = bulletSpeed;

            Invoke(nameof(CMD_Lifetime), _bulletLifeTime);

            _init = true;

            _isEnemy = isEnemy;

            _physicsScene = gameObject.scene.GetPhysicsScene();
        }
        
        [ClientCallback]
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

            StartCoroutine(DestroySequence());
        }
        
        private void TakeDamageToObstacle(GameObject owner, GameObject obstacle)
        {
            if (obstacle.gameObject == owner) return;
            
            var _settings = SpaceshipController.instance.m_shooting.bulletSettings;
            
            StartCoroutine(DestroySequence());

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
                    Damageable.DealDamage(_settings.BulletDamage);   
                }
                return;
            }

            var destructionScript = obstacle.GetComponent<DestructionScript>();
            if (destructionScript != null)
            {
                if (destructionScript.HP <= 0) return;
                
                destructionScript.HP -= _settings.BulletDamage;
                
                var basicAI = destructionScript.GetComponent<BasicAI>();

                var isDead = destructionScript.HP <= 0;

                if (basicAI != null)
                {
                    basicAI.Threat();
                    
                    if (isDead)
                    {
                        AddScore(username, 30);
                    }
                }
                else
                {
                    if (isDead)
                    {
                        AddScore(username, 1);
                    }
                }
            }
        }

        private void AddScore(string username, int value)
        {
            if (_isEnemy) return;
            
            LeaderboardManager.Instance.CMD_AddScore(username, value);
        }

        private IEnumerator DestroySequence()
        {
            _isMove = false;

            if (Trail != null)
                Trail.gameObject.SetActive(false);

            if (HitEffect != null)
            {
                var firework = Instantiate(HitEffect, transform.position, Quaternion.identity);
                firework.transform.parent = gameObject.transform;

                yield return new WaitForSeconds(1f);

                Destroy(firework);
            }

            if (gameObject == null) yield break;

            NetworkServer.Destroy(gameObject);
        }

        [Command(requiresAuthority = false)]
        private void CMD_Lifetime() => NetworkServer.Destroy(gameObject);
    }
}