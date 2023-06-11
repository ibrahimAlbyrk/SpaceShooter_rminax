using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.Network
{
    public class Player_NETWORK : NetworkBehaviour
    {
        public string Username;

        [FormerlySerializedAs("singleObjects")] [SerializeField] private GameObject[] networkObjects;

        private void Start()
        {
            foreach (var singleObject in networkObjects)
            {
                singleObject.gameObject.SetActive(isOwned);
            }
        }
    }
}