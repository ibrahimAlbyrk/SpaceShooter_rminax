using System;
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
        
        private PhysicsScene _physicsScene;

        private bool _isDetected;

        private Vector3 _dir;
        
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
        
        [ServerCallback]
        private void FixedUpdate()
        {
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
                    NetworkServer.Destroy(gameObject);
                    _isDetected = true;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            
            Gizmos.DrawWireSphere(transform.position, _detectionRange);
        }
    }
}