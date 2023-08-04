using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Network.Managers.Room
{
    public class RoomSearcher : MonoBehaviour
    {
        [SerializeField] private Transform _roomsContent;
        [SerializeField] private GameObject _roomFieldPrefab;

        private readonly List<GameObject> _roomList = new();

        public void UpdateUI()
        {
            var rooms = SpaceRoomManager.Instance.GetRooms();

            _roomList.RemoveAll(_ => true);

            foreach (var room in rooms)
            {
                var roomField = Instantiate(_roomFieldPrefab, _roomsContent).GetComponent<RoomField_UI>();

                if (roomField == null) continue;

                roomField.Init(room.RoomName, room.CurrentPlayers, room.MaxPlayers);
            }
        }
    }
}