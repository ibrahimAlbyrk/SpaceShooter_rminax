using Mirror;
using UnityEngine;

namespace _Project.Scripts.Game.Simulator
{
    public class PhysicsSimulator : MonoBehaviour
    {
        private PhysicsScene physicsScene;

        private bool simulatePhysicsScene;

        private void Awake()
        {
            if (NetworkServer.active)
            {
                physicsScene = gameObject.scene.GetPhysicsScene();
                simulatePhysicsScene = physicsScene.IsValid() && physicsScene != Physics.defaultPhysicsScene;

                return;
            }
            
            enabled = false;
        }
        
        private void FixedUpdate()
        {
            if (!simulatePhysicsScene) return;
            
            physicsScene.Simulate(Time.fixedDeltaTime);
        }
    }
}
