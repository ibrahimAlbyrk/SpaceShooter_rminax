using System;

namespace _Project.Scripts.Network.Managers.Room
{
    [Serializable]
    public struct RoomListInfo
    {
        public string Name;
        public int MaxPlayer;
        public int CurrentPlayer;

        public RoomListInfo(string name, int maxPlayer, int currentPlayer)
        {
            Name = name;
            MaxPlayer = maxPlayer;
            CurrentPlayer = currentPlayer;
        }
    }
}