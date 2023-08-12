﻿using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Game.Mod
{
    using ShrinkingArea;

    [System.Serializable]
    public class ShrinkingAreaMod : OpenWorldMod
    {
        [Header("Shrinking Settings")]
        [SerializeField] private ShrinkingAreaSystem _shrinkingAreaSystem;

        public override void StartOnServer()
        {
            base.StartOnServer();

            SpawnShrinkingAreaSystem();
        }

        private void SpawnShrinkingAreaSystem()
        {
            var system = Object.Instantiate(_shrinkingAreaSystem.gameObject);
            
            SceneManager.MoveGameObjectToScene(system, _manager.gameObject.scene);
            
            NetworkServer.Spawn(system);
        }
    }
}