using Mirror;

namespace _Project.Scripts.Network.Managers.Room
{
    public struct RoomMessage : NetworkMessage
    {
        public readonly RoomState RoomState;

        public RoomMessage(RoomState roomState)
        {
            RoomState = roomState;
        }
    }
}