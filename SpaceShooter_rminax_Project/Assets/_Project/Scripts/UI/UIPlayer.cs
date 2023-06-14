using _Project.Scripts.Network;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.UI
{
    public class UIPlayer : MonoBehaviour
    {
        [SerializeField] private TMP_Text _nameText;

        private LobbyPlayer _lobbyPlayer;

        public void SetPlayer(LobbyPlayer lobbyPlayer)
        {
            _lobbyPlayer = lobbyPlayer;
            _nameText.text = _lobbyPlayer.Username;
        }
    }
}