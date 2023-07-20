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

        //float Damage;

        //float lifetime;

        private bool _isHit;
        private bool _isMove = true;

        private GameObject _owner;

        private bool _init;

        private bool _isEnemy;

        public void Init(GameObject owner, bool isEnemy = false)
        {
            _owner = owner;
            
            var _settings = SpaceshipController.instance.m_shooting.bulletSettings;
            
            Invoke(nameof(CMD_Lifetime),
                _settings.BulletLifetime);

            _init = true;

            _isEnemy = isEnemy;
        }
        
        [Client]
        private void Update()
        {
            if (!_init) return;
            if (!_isMove) return;
            
            var _settings = SpaceshipController.instance.m_shooting.bulletSettings;

            transform.position += transform.forward * (_settings.BulletSpeed * Time.fixedDeltaTime);

            var colls = Physics.OverlapBox(transform.position, transform.localScale, transform.rotation,
                _obstacleLayer);

            if (colls.Length < 1 || _isHit) return;
            if (colls.Any(coll => coll.gameObject == _owner)) return;
            
            _isHit = true;

            var obstacle = colls.FirstOrDefault()?.gameObject;
            
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
                AddScore(username, 30);
                
                Damageable.DealDamage(_settings.BulletDamage);
                return;
            }
            
            var basicAI = obstacle.GetComponent<BasicAI>();
            if (basicAI != null)
            {
                AddScore(username, 30);
                
                basicAI.Threat();
                return;
            }

            var destructionScript = obstacle.GetComponent<DestructionScript>();
            if (destructionScript != null)
            {
                destructionScript.HP -= _settings.BulletDamage;
                if (destructionScript.HP <= 0)
                {
                    AddScore(username, 1);
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

            NetworkServer.Destroy(gameObject);
        }

        [Command(requiresAuthority = false)]
        private void CMD_Lifetime() => NetworkServer.Destroy(gameObject);
    }
}