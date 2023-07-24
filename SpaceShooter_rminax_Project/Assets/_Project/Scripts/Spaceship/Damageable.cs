using Mirror;
using UnityEngine;

namespace _Project.Scripts.Spaceship
{
    public class Damageable : NetworkBehaviour
    {
        [SerializeField] private Health _health;

        public float GetHealth() => _health.GetHealth();
        
        public void DealDamage(float dealToDamage)
        {
            var controller = GetComponent<SpaceshipController>();
            
            if(controller != null) controller.CMD_Shake();
            
            _health.Remove(dealToDamage);
        }
    }
}