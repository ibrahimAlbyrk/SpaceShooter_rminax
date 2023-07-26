using Mirror;
using UnityEngine;

namespace _Project.Scripts.Game
{
    using Network;
    
    public class SpawnSystem : NetIdentity
    {
        public override void OnStartClient()
        {
            if (!isOwned) return;
            
            //CMD_SpawnPlayer();
        }
        
        [Command(requiresAuthority = false)]
        public void CMD_SpawnPlayer()
        {
            var spawnRange = GameManager.Instance.GetData().GameAreaRadius;

            var spawnPosition = SpawnUtilities.GetSpawnPosition(spawnRange);

            var spawnRotation = SpawnUtilities.GetSpawnRotation();
            
            RPC_SpawnPlayer(spawnPosition, spawnRotation);
        }

        [TargetRpc]
        private void RPC_SpawnPlayer(Vector3 pos, Quaternion rot)
        {
            var playerTransform = transform;

            playerTransform.position = pos;
            playerTransform.rotation = rot;
        }
    }
}