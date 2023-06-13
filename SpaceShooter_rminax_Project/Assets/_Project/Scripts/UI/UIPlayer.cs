using _Project.Scripts.Network;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.UI
{
    public class UIPlayer : MonoBehaviour
    {
        [SerializeField] private TMP_Text _nameText;

        private Player_NETWORK _player;

        public void SetPlayer(Player_NETWORK player)
        {
            _player = player;
            _nameText.text = _player.Username;
        }
    }
}