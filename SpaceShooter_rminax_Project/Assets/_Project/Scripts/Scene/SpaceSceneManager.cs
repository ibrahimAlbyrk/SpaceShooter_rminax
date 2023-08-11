using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Scenes
{
    public class SpaceSceneManager : MonoBehaviour
    {
        #region Singleton

        private static readonly object padlock = new();
        
        private static SpaceSceneManager instance;

        public static SpaceSceneManager Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = FindObjectOfType<SpaceSceneManager>();

                        if (instance == null)
                        {
                            var obj = new GameObject(nameof(SpaceSceneManager));
                            instance = obj.AddComponent<SpaceSceneManager>();
                            DontDestroyOnLoad(obj);
                        }
                    }
                }

                return instance;
            }
        }

        #endregion

        public static event Action<Scene> OnSceneLoaded;
        public static event Action<Scene> OnSceneUnloading;
        public static event Action<Scene> OnSceneUnloaded;

        private readonly List<Scene> LoadedScenes = new();
        
        private SceneLoader _loader;

        public int GetLoadedSceneCount() => LoadedScenes.Count;

        #region SceneHandler Methods

        public void LoadScene(string sceneName, LoadSceneMode loadSceneMode = LoadSceneMode.Single, Action<Scene> onCompleted = null)
        {
            _loader.LoadScene(sceneName, loadSceneMode,
                loadScene =>
                {
                    onCompleted?.Invoke(loadScene);
                    OnSceneLoaded?.Invoke(loadScene);
                });
        }

        public void UnLoadScene(Scene scene, Action<Scene> onCompleted = null)
        {
            _loader.UnloadScene(scene, unloadScene =>
            {
                onCompleted?.Invoke(unloadScene);
                OnSceneUnloaded?.Invoke(unloadScene);
            }, OnSceneUnloading);
        }

        #endregion
        
        private void KeepLoadedScene(Scene scene) => LoadedScenes.Add(scene);
        
        private void DiscardLoadedScene(Scene scene) => LoadedScenes.Remove(scene);

        private void Awake()
        {
            OnSceneLoaded += KeepLoadedScene;
            OnSceneUnloaded += DiscardLoadedScene;

            _loader = new SceneLoader(this);
        }
    }
}