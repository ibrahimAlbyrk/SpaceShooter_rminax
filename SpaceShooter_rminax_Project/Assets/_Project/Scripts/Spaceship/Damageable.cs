using Mirror;
using UnityEngine;

namespace _Project.Scripts.Spaceship
{
    public class Damageable : NetworkBehaviour
    {
        [SerializeField] private Health _health;

        public void DealDamage(float dealToDamage)
        {
            if(TryGetComponent(out SpaceshipController controller))
                controller.CMD_Shake();
            
            _health.Remove(dealToDamage);
        }
    }
}