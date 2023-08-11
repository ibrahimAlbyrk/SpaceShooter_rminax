using System;
using Mirror;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Project.Scripts.Game.Mod
{
    using ShrinkingArea;

    [Serializable]
    public class ShrinkingAreaMod : OpenWorldMod
    {
        [Header("Shrinking Settings")] [SerializeField]
        private ShrinkingAreaSystem _shrinkingAreaSystem;

        public override void StartOnServer()
        {
            base.StartOnServer();

            SpawnShrinkingAreaSystem();
        }

        public override void StartOnClient()
        {
        }

        private void SpawnShrinkingAreaSystem()
        {
            var system = Object.Instantiate(_shrinkingAreaSystem.gameObject);
            NetworkServer.Spawn(system);
        }
    }

    public class ShrinkingArea_UI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _titleText;

        public void StartCountDownHandler(float time)
        {
        }

        public void StartShrinkingHandler(float time)
        {
        }
    }
}