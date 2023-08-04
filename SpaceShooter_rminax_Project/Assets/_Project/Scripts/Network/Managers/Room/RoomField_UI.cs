using TMPro;
using UnityEngine;

namespace _Project.Scripts.Network.Managers.Room
{
    public class RoomField_UI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _countText;
        [SerializeField] private TMP_Text _nameText;

        public void Init(string roomName, int currentPlayerCount, int maxPlayerCount)
        {
            _countText.text = $"{currentPlayerCount}/{maxPlayerCount}";
            _nameText.text = roomName;
        }
    }
}