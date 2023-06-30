using System.Collections;
using System.Linq;
using _Project.Scripts.Game;
using Mirror;
using UnityEngine;

namespace _Project.Scripts.Spaceship
{
    public class BulletScript : NetworkBehaviour
    {
        [SerializeField] private LayerMask _obstacleLayer;

        [Tooltip("The particle effect of hitting something.")]
        public GameObject HitEffect;

        public ParticleSystem Trail;
        public Transform ship;

        //float Damage;

        //float lifetime;

        private bool _isHit;
        private bool _isMove = true;

        private GameObject _owner;

        private bool _init;

        public void Init(GameObject owner)
        {
            _owner = owner;
            
            var _settings = SpaceshipController.instance.m_shooting.bulletSettings;
            
            Invoke(nameof(CMD_Lifetime),
                _settings.BulletLifetime);

            if (SpaceshipController.instance == null) return;
            ship = SpaceshipController.instance.transform.root;

            _init = true;
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
            
            var username = owner.GetComponent<SpaceshipController>().Username;

            var Damageable = obstacle.GetComponent<Damageable>();

            if (Damageable != null)
            {
                LeaderboardManager.Instance.CMD_AddScore(username, 30);
                
                Damageable.DealDamage(_settings.BulletDamage);
                return;
            }
            
            var basicAI = obstacle.GetComponent<BasicAI>();
            if (basicAI != null)
            {
                LeaderboardManager.Instance.CMD_AddScore(username, 30);
                
                basicAI.threat();
                return;
            }

            var destructionScript = obstacle.GetComponent<DestructionScript>();
            if (destructionScript != null)
            {
                destructionScript.HP -= _settings.BulletDamage;
                if (destructionScript.HP <= 0)
                {
                    LeaderboardManager.Instance.CMD_AddScore(username, 1);
                }
            }
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