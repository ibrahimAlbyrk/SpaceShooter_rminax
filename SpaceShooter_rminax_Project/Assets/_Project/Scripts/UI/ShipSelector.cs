using _Project.Scripts.Network;
using UnityEngine;

namespace _Project.Scripts.UI
{
    public class ShipSelector : MonoBehaviour
    {
        public void SelectShip(string shipName)
        {
            PlayerPrefs.SetString("ShipName", shipName);
            LobbyPlayer.LocalLobbyPlayer?.CMD_SetShip(shipName);
        }
    }
}