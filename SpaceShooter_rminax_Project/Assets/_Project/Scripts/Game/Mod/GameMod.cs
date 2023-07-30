using UnityEngine;

namespace _Project.Scripts.Game.Mod
{
    using Data;
    
    [System.Serializable]
    public abstract class GameMod
    {
        protected ModManager _manager;

        [Header("General Settings"), SerializeField]
        protected MapGeneratorData _mapGeneratorData;

        public MapGeneratorData GetMapData() => _mapGeneratorData;
        
        public virtual void Init(ModManager manager)
        {
            _manager = manager;
        }
        
        public virtual void Start(){}

        public virtual void Run(){}
        
        public virtual void FixedRun(){}
    }
}