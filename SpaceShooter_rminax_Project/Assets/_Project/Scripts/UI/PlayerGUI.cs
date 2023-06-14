using Mirror;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.UI
{
    using Network.Messages;
    
    public class PlayerGUI : MonoBehaviour
    {
        public TMP_Text _playerNameText;

        [ClientCallback]
        public void SetPlayerInfo(PlayerInfo info)
        {
            _playerNameText.text = info.Username;
            _playerNameText.color = info.IsReady ? Color.green : Color.red;
        }
    }
}