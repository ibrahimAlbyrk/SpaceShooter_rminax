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
        
        public void DealDamage(float dealToDamage)
        {
            var controller = GetComponent<SpaceshipController>();

            if (controller != null)
            {
                controller.CMD_Shake();
                _shakers.ForEach(shaker => shaker?.Play());
            }
            
            _health.Remove(dealToDamage);
        }
    }
}