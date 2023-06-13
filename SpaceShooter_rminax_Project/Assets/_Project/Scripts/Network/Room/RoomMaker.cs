using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Project.Scripts.Network.Room
{
    [Serializable]
    public class Room
    {
        public string RoomID;

        public List<GameObject> Players = new();

        public Room()
        {
        }

        public Room(string roomID, GameObject player)
        {
            RoomID = roomID;
            Players.Add(player);
        }
    }

    public class RoomMaker : NetworkBehaviour
    {
        public static RoomMaker instance;

        public readonly SyncList<Room> Rooms = new();

        public readonly SyncList<string> roomIDs = new();

        public bool HostGame(string roomID, GameObject player)
        {
            if (!roomIDs.Contains(roomID))
            {
                roomIDs.Add(roomID);
                Rooms.Add(new Room(roomID, player));

                Debug.Log("Room Generated");
                
                return true;
            }
            
            Debug.Log("Room ID already exists");
            
            return false;
        }
        
        public bool JoinGame(string roomID, GameObject player)
        {
            if (!roomIDs.Contains(roomID))
            {
                foreach (var room in Rooms.Where(room => room.RoomID == roomID))
                {
                    room.Players.Add(player);
                    break;
                }

                Debug.Log("Room joined");
                
                return true;
            }
            
            Debug.Log("Room ID does not exist");
            
            return false;
        }

        public static string GetRandomRoomID()
        {
            var ID = string.Empty;

            for (var i = 0; i < 5; i++)
            {
                var random = Random.Range(0, 36);
                if (random < 26)
                    ID += (char)(random + 65);
                else
                    ID += (random - 26).ToString();
            }

            Debug.Log($"<color=orange>Random Room ID: {ID}</color>");

            return ID;
        }

        private void Start()
        {
            instance = this;
        }
    }

    public static class RoomExtensions
    {
        public static Guid ToGuid(this string id)
        {
            var provider = new MD5CryptoServiceProvider();
            var inputBytes = Encoding.Default.GetBytes(id);
            var hashBytes = provider.ComputeHash(inputBytes);

            return new Guid(hashBytes);
        }
    }
}