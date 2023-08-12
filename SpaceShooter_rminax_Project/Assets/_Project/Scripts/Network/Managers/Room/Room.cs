using Mirror;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Network.Managers.Room
{
    [System.Serializable]
    public class Room
    {
        public bool IsServer;
        
        public string RoomName;

        public Scene Scene;
        
        public int MaxPlayers;
        public int CurrentPlayers;

        public readonly List<NetworkConnection> Connections;

        public Room(string roomName, int maxPlayers, bool isServer)
        {
            IsServer = isServer;
            RoomName = roomName;
            MaxPlayers = maxPlayers;
            Connections = new List<NetworkConnection>();
        }

        public bool AddConnection(NetworkConnection conn)
        {
            if (Connections.Contains(conn)) return false;

            Connections.Add(conn);

            CurrentPlayers++;
            
            return true;
        }

        public List<NetworkConnection> RemoveAllConnections()
        {
            var connections = Connections.ToList();
            
            connections.ForEach(connection => RemoveConnection(connection));

            return connections;
        }
        
        public bool RemoveConnection(NetworkConnection conn)
        {
            var isRemoved = Connections.Remove(conn);

            if (isRemoved) CurrentPlayers--;

            return isRemoved;
        }
    }
}