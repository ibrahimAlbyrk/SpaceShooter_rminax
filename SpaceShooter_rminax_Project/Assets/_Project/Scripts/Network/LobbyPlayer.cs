namespace _Project.Scripts.Network
{
    public class LobbyPlayer : NetIdentity
    {
        public static LobbyPlayer LocalLobbyPlayer;
        
        private void Start()
        {
            if (isLocalPlayer)
                LocalLobbyPlayer = this;
        }
    }
}