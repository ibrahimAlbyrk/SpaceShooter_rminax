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

        private readonly List<RoomField_UI> _roomList = new();

        public void UpdateUI()
        {
            var rooms = SpaceRoomManager.Instance?.GetRooms();

            if (rooms == null) return;

            foreach (var room in _roomList.ToList())
            {
                _roomList.Remove(room);
                room.gameObject.Destroy();
            }

            foreach (var room in rooms.Where(room => room.Name != "OpenWorld"))
            {
                CreateRoomField(room);
            }
        }

        private void CreateRoomField(RoomListInfo roomListInfo)
        {
            var roomField = Instantiate(_roomFieldPrefab, _roomsContent).GetComponent<RoomField_UI>();

            if (roomField == null) return;

            roomField.Init(roomListInfo.Name, roomListInfo.CurrentPlayer, roomListInfo.MaxPlayer);

            _roomList.Add(roomField);
        }

        private void RemoveRoomField(RoomField_UI roomField)
        {
            _roomList.Remove(roomField);
            roomField.gameObject.Destroy();
        }

        private void UpdateRoomField(RoomListInfo oldRoomList, RoomListInfo newRoomList)
        {
            if (!gameObject.activeSelf) return;

            var roomName = newRoomList.CurrentPlayer < 1 ? oldRoomList.Name : newRoomList.Name;
            
            var roomFieldUI = _roomList.FirstOrDefault(field => field.GetRoomName() == roomName);

            if (roomFieldUI == null)
            {
                CreateRoomField(newRoomList);
                return;
            }
            
            if (newRoomList.CurrentPlayer < 1)
            {
                RemoveRoomField(roomFieldUI);
                return;
            }
            
            roomFieldUI.Init(newRoomList.Name, newRoomList.CurrentPlayer, newRoomList.MaxPlayer);
        }

        private void Awake()
        {
            _exitButton.onClick.AddListener(() => gameObject.SetActive(false));
            SpaceRoomManager.OnServerCreatedRoom += UpdateRoomField;
        }
    }
}