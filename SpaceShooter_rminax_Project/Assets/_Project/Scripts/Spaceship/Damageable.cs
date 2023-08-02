using Mirror;
using MoreMountains.Feedbacks;
using Sirenix.Utilities;
using UnityEngine;

namespace _Project.Scripts.Spaceship
{
    public class Damageable : NetworkBehaviour
    {
        [SerializeField] private Health _health;

        [SerializeField] private MMShaker[] _shakers;

        public float GetHealth() => _health.GetHealth();
        
        [ServerCallback]
        public void DealDamage(float dealToDamage)
        {
            var controller = GetComponent<SpaceshipController>();

            if (controller != null)
            {
                controller.RPC_Shake(); //TODO: It's was CMD
                RPC_PlayShakers();
            }
            
            _health.Remove(dealToDamage);
        }

        [TargetRpc]
        private void RPC_PlayShakers() => _shakers.ForEach(shaker => shaker?.Play());
    }
}