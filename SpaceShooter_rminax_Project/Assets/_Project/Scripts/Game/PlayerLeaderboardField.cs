using TMPro;
using UnityEngine;

namespace _Project.Scripts.Game
{
    public class PlayerLeaderboardField : MonoBehaviour
    {
        [SerializeField] private TMP_Text _usernameText;
        [SerializeField] private TMP_Text _scoreText;
        
        public string Username { get; private set; }
        public int Score { get; private set; }
        
        public void Init(string username)
        {
            Username = username;
            Score = 0;
        }

        public void UpdateUI()
        {
            _usernameText.text = Username;
            _scoreText.text = $"{Score}";
        }

        public void SetScore(int score) => Score = score;
    }
}