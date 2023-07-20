using Mirror;
using UnityEngine;

namespace _Project.Scripts.Game.Mod
{
    using ShrinkingArea;
    
    [System.Serializable]
    public class ShrinkingAreaMod : OpenWorldMod
    {
        [Header("Shrinking Settings")]
        [SerializeField] private ShrinkingAreaSystem _shrinkingAreaSystem;
        
        public override void Start()
        {
            base.Start();
            
            SpawnShrinkingAreaSystem();
        }

        private void SpawnShrinkingAreaSystem()
        {
            var system = Object.Instantiate(_shrinkingAreaSystem.gameObject);
            NetworkServer.Spawn(system);
        }
    }
}