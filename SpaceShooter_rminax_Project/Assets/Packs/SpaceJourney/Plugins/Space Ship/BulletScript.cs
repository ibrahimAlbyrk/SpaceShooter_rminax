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

        public void Init()
        {
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

            if (colls.Any(coll => coll.gameObject == SpaceshipController.instance.gameObject)) return;

            _isHit = true;

            TakeDamageToObstacle(colls.FirstOrDefault());

            StartCoroutine(DestroySequence());
        }

        private void TakeDamageToObstacle(Component coll)
        {
            var spaceship = SpaceshipController.instance;
            
            if (coll.transform.root == ship) return;

            if (coll.TryGetComponent(out Damageable damageable))
            {
                damageable.DealDamage(_settings.BulletDamage);
            }

            if (coll.TryGetComponent(out DestructionScript destructionScript))
            {
                if (spaceship != null)
                    destructionScript.HP -= _settings.BulletDamage;
            }

            if (coll.TryGetComponent(out BasicAI ai))
                ai.threat();
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

        private void CMD_Lifetime() => Destroy(gameObject);
    }
}