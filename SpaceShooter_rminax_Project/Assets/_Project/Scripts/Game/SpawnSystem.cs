namespace _Project.Scripts.Game
{
    using Network;
    
    public class SpawnSystem : NetIdentity
    {
        public override void OnStartClient()
        {
            if (!isOwned) return;
            
            SpawnPlayer();
        }

        public void SpawnPlayer()
        {
            var playerTransform = transform;

            var spawnRange = GameManager.Instance.GetData().GameAreaRadius;

            var spawnPosition = SpawnUtilities.GetSpawnPosition(spawnRange);

            var spawnRotation = SpawnUtilities.GetSpawnRotation();

            playerTransform.position = spawnPosition;
            playerTransform.rotation = spawnRotation;
        }
    }
}