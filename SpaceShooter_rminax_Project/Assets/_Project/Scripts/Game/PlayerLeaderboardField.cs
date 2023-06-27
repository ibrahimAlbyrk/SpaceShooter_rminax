using TMPro;
using UnityEngine;

namespace _Project.Scripts.Game
{
    public class PlayerLeaderboardField : MonoBehaviour
    {
        [SerializeField] private TMP_Text _usernameText;
        [SerializeField] private TMP_Text _scoreText;

        private string Username;
        private int Score;
        
        public void Init(string username, int score)
        {
            Username = username;
            Score = score;

            UpdateUI();
        }

        private void UpdateUI()
        {
            _usernameText.text = Username;
            _scoreText.text = $"{Score}";
        }
    }
}