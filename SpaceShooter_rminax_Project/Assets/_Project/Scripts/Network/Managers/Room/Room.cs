using Mirror;
using System.Collections.Generic;
using System.Linq;

namespace _Project.Scripts.Network.Managers.Room
{
    [System.Serializable]
    public class Room
    {
        public bool IsServer;
        
        public string RoomName;
        public string SceneName;
        
        public int MaxPlayers;
        public int CurrentPlayers;

        public readonly List<NetworkConnection> _connections;

        public Room(string roomName, string sceneName, int maxPlayers, bool isServer)
        {
            IsServer = isServer;
            
            RoomName = roomName;
            SceneName = sceneName;
            MaxPlayers = maxPlayers;
            _connections = new List<NetworkConnection>();
        }

        public bool AddConnection(NetworkConnection conn)
        {
            if (_connections.Contains(conn)) return false;

            _connections.Add(conn);

            CurrentPlayers++;
            
            return true;
        }

        public List<NetworkConnection> RemoveAllConnections()
        {
            var connections = _connections.ToList();
            
            _connections.ForEach(connection => RemoveConnection(connection));

            return connections;
        }
        
        public bool RemoveConnection(NetworkConnection conn)
        {
            var isRemoved = _connections.Remove(conn);

            if (isRemoved) CurrentPlayers--;

            return isRemoved;
        }
    }
}