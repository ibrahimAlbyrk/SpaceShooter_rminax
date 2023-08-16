using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Project.Scripts
{
    using Game;
    using Network;
    using Spaceship;
    
    public class Collectible : NetIdentity
    {
        [SerializeField] private float _detectionRange;
        [SerializeField] private LayerMask _detectionLayer;

        [SerializeField] private GameObject _visual;
        
        [SerializeField] private GameObject HitEffect;
        
        private PhysicsScene _physicsScene;

        private bool _isDetected;

        private Vector3 _dir;
        
        //Destroy Variables
        private float _destroyTimer;
        private bool _isStartDestroy;
        
        //Firework Variables
        private float _fireworkTimer;
        private bool _isStartFireworks;
        private GameObject _firework;
        
        [ServerCallback]
        private void Start()
        {
            _physicsScene = gameObject.scene.GetPhysicsScene();

            _dir = new Vector3(Random.value, Random.value, Random.value);
        }

        [ServerCallback]
        private void Update()
        {
            transform.Rotate(_dir);
        }
        
        private void FixedUpdate()
        {
            if (_isStartFireworks)
            {
                if (_fireworkTimer > 1)
                {
                    Destroy(_firework);
            
                    _fireworkTimer = 0;
                    _isStartFireworks = false;
                }
                else
                    _fireworkTimer += Time.fixedDeltaTime;
            }
            
            if (!isServer) return;
            
            if (_isStartDestroy)
            {
                if (_destroyTimer > 1.5f)
                {
                    NetworkServer.Destroy(gameObject);
            
                    _destroyTimer = 0;
                    _isStartFireworks = false;
                }
                else
                    _destroyTimer += Time.fixedDeltaTime;
            }
            
            if (_isDetected) return;
            
            var colls = new Collider[5];
            _physicsScene.OverlapSphere(transform.position, _detectionRange, colls, _detectionLayer,
                QueryTriggerInteraction.UseGlobal);

            foreach (var coll in colls)
            {
                if (coll == null) continue;

                var ship = coll.GetComponent<SpaceshipController>();
                
                if (ship != null)
                {
                    var leaderboardManager = GameContainer.Get<LeaderboardManager>(gameObject.scene);
                    
                    leaderboardManager.AddScore(ship.Username, 5);
                    ObjectDestroyer();
                    _isDetected = true;
                    RPC_SpawnEffect();
                }
            }
        }

        private void ObjectDestroyer()
        {
            RPC_CloseVisual();
            _isStartDestroy = true;
        }

        [ClientRpc]
        private void RPC_CloseVisual()
        {
            _visual.SetActive(false);
        }

        [ClientRpc]
        private void RPC_SpawnEffect()
        {
            if (HitEffect == null) return;

            _firework = Instantiate(HitEffect, transform.position, Quaternion.identity);

            if (_firework == null) return;

            _firework.GetComponentInChildren<ParticleSystem>().Play();

            _isStartFireworks = true;
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            
            Gizmos.DrawWireSphere(transform.position, _detectionRange);
        }
    }
}