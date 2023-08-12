using Mirror;

namespace _Project.Scripts.Network.Managers.Room
{
    [System.Serializable]
    public struct ServerRoomMessage : NetworkMessage
    {
        public readonly ServerRoomState ServerRoomState;
        public readonly RoomInfo RoomInfo;

        public readonly bool IsDisconnected;

        public ServerRoomMessage(ServerRoomState serverRoomState, RoomInfo roomInfo, bool isDisconnected = false)
        {
            ServerRoomState = serverRoomState;
            RoomInfo = roomInfo;
            IsDisconnected = isDisconnected;
        }
    }
    
    [System.Serializable]
    public struct ClientRoomMessage : NetworkMessage
    {
        public int ConnectionId;
        
        public string SceneName;
        
        public readonly ClientRoomState ClientRoomState;

        public ClientRoomMessage(string sceneName, ClientRoomState clientRoomState, int connectionId)
        {
            SceneName = sceneName;
            ClientRoomState = clientRoomState;
            ConnectionId = connectionId;
        }
        
        public ClientRoomMessage(ClientRoomState clientRoomState, int connectionId)
        {
            SceneName = null;
            ClientRoomState = clientRoomState;
            ConnectionId = connectionId;
        }
    }
}