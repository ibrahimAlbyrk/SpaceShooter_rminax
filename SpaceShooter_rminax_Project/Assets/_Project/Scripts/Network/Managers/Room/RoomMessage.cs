using Mirror;

namespace _Project.Scripts.Network.Managers.Room
{
    [System.Serializable]
    public struct ServerRoomMessage : NetworkMessage
    {
        public readonly ServerRoomState ServerRoomState;
        public readonly RoomInfo RoomInfo;

        public ServerRoomMessage(ServerRoomState serverRoomState, RoomInfo roomInfo)
        {
            ServerRoomState = serverRoomState;
            RoomInfo = roomInfo;
        }
    }
    
    [System.Serializable]
    public struct ClientRoomMessage : NetworkMessage
    {
        public readonly ClientRoomState ClientRoomState;

        public ClientRoomMessage(ClientRoomState clientRoomState)
        {
            ClientRoomState = clientRoomState;
        }
    }
}