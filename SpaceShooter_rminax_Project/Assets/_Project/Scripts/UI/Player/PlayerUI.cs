using TMPro;
using UnityEngine;

namespace _Project.Scripts.UI.Player
{
    using Spaceship;
    
    public class PlayerUI : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private Health _health;
        [SerializeField] private TMP_Text _healthText;
        
        [Header("Kills Settings")]
        [SerializeField] private TMP_Text _killsText;

        private void Awake()
        {
            _health.OnHealthChanged += HealthOnOnHealthChanged;
        }

        private void HealthOnOnHealthChanged(object sender, HealthChangedEventArgs args)
        {
            _healthText.text = $"{args.Health}";
        }
    }
}