using Mirror;
using UnityEngine;

namespace _Project.Scripts.Game
{
    using Mod;
    using Data;
    using Network;
    
    public class ModManager : NetIdentity
    {
        [SerializeField] private OpenWorldMod _openWorldMod;
        [SerializeField] private ShrinkingAreaMod _shrinkingAreaMod;

        private GameMod _selectedMod;

        public MapGeneratorData GetMapData() => _selectedMod?.GetMapData();
        
        public void Init(ModType _modType)
        {
            _selectedMod = _modType switch
            {
                ModType.OpenWorld => _openWorldMod,
                ModType.ShrinkingArea => _shrinkingAreaMod,
                _ => _selectedMod
            };
            _selectedMod.Init(this);
        }
        
        [Server]
        public void StartGameMod()
        {
            _selectedMod.Start();
        }
        
        public void RunGameMod()
        {
            _selectedMod.Run();
        }
        
        [Server]
        public void FixedRunGameMod()
        {
            _selectedMod.FixedRun();
        }
    }
}