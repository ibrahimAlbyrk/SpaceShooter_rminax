namespace _Project.Scripts.Network.Managers.Room
{
    [System.Serializable]
    public struct RoomInfo
    {
        public string Name;
        public string SceneName;
        
        public int MaxPlayers;

        public RoomInfo(string name, string sceneName, int maxPlayers)
        {
            Name = name;
            SceneName = sceneName;
            MaxPlayers = maxPlayers;
        }
    }
}