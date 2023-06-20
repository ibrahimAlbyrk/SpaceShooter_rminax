using System.Collections;
using System.Linq;
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

        private Transform _player;

        //float Damage;

        //float lifetime;

        private bool _isHit;
        private bool _isMove = true;

        private SpaceshipController.BulletSettings _settings;

        private GameObject _ownerPlayer;

        public void Init(GameObject ownerPlayer)
        {
            print("worked");
            
            _ownerPlayer = ownerPlayer;
            
            _settings = SpaceshipController.instance.m_shooting.bulletSettings;
            
            Invoke(nameof(CMD_Lifetime),
                _settings.BulletLifetime);

            if (SpaceshipController.instance == null) return;
            _player = SpaceshipController.instance.transform;
            ship = SpaceshipController.instance.transform.root;
        }
        
        private void Update()
        {
            if (!_isMove) return;

            transform.position += transform.forward * (_settings.BulletSpeed * Time.fixedDeltaTime);

            var colls = Physics.OverlapBox(transform.position, transform.localScale, transform.rotation,
                _obstacleLayer);

            if (colls.Length < 1 || _isHit) return;

            if (colls.Any(coll => coll.gameObject == _ownerPlayer)) return;

            print($"owner: __* {_ownerPlayer.name} *__");
            
            _isHit = true;

            var obstacle = colls.FirstOrDefault()?.gameObject;
            
            CMD_TakeDamageToObstacle(_ownerPlayer, obstacle);

            StartCoroutine(DestroySequence());
        }
        
        [Command(requiresAuthority = false)]
        private void CMD_TakeDamageToObstacle(GameObject ownerPlayer, GameObject obstacle)
        {
            var spaceship = SpaceshipController.instance;
            
            if (obstacle == ownerPlayer) return;

            var damageable = obstacle.GetComponent<Damageable>();
            if (damageable != null)
            {
                damageable.DealDamage(_settings.BulletDamage);
            }

            var destructionScript = obstacle.GetComponent<DestructionScript>();
            if (destructionScript != null)
            {
                if (spaceship != null)
                    destructionScript.HP -= _settings.BulletDamage;
            }

            var ai = obstacle.GetComponent<BasicAI>();
            if (ai != null)
            {
                ai.threat();
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

            Destroy(gameObject);
        }

        [Command(requiresAuthority = false)]
        private void CMD_Lifetime() => NetworkServer.Destroy(gameObject);
    }
}