using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Collections.Generic;

namespace _Project.Scripts.Network.Managers.Room
{
    using Utilities;
    
    public class RoomSearcher : MonoBehaviour
    {
        [SerializeField] private Transform _roomsContent;
        [SerializeField] private GameObject _roomFieldPrefab;

        [SerializeField] private Button _exitButton;

        private readonly List<GameObject> _roomList = new();

        public void UpdateUI()
        {
            var rooms = SpaceRoomManager.Instance?.GetRooms();

            if (rooms == null) return;

            foreach (var room in _roomList.ToList())
            {
                _roomList.Remove(room);
                room.Destroy();
            }

            foreach (var room in rooms)
            {
                var roomField = Instantiate(_roomFieldPrefab, _roomsContent).GetComponent<RoomField_UI>();

                if (roomField == null) continue;

                roomField.Init(room.Name, room.CurrentPlayer, room.MaxPlayer);

                _roomList.Add(roomField.gameObject);
            }
        }

        private void Awake()
        {
            _exitButton.onClick.AddListener(() => gameObject.SetActive(false));
        }
    }
}