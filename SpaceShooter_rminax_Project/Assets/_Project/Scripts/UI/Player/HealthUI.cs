using TMPro;
using UnityEngine;

namespace _Project.Scripts.UI.Player
{
    using Spaceship;
    
    public class HealthUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _healthText;
        
        private void Start()
        {
            SpaceshipController.instance.Health.OnHealthChanged += OnHealthUpdated;
        }

        private void OnHealthUpdated(object sender, HealthChangedEventArgs args)
        {
            _healthText.text = $"{args.Health}";
        }
    }
}