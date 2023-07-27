using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Scene
{
    using Network;

    public class SceneManager : NetIdentity
    {
        public static SceneManager Instance;

        private static readonly List<UnityEngine.SceneManagement.Scene> _loadedScenes = new();

        private static int _sceneCount;

        public static int GetSceneCount() => _sceneCount;
        
        public static List<UnityEngine.SceneManagement.Scene> GetLoadedScenes() => _loadedScenes;

        public static event Action<UnityEngine.SceneManagement.Scene> OnSceneLoaded;
        
        public void LoadScene(string sceneName, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
        {
            StartCoroutine(LoadScene_Cor(sceneName, loadSceneMode));
        }

        public SceneHandler LoadAdditiveScene(string sceneName)
        {
            StartCoroutine(LoadScene_Cor(sceneName, LoadSceneMode.Additive));

            return new SceneHandler();
        }
        
        public void UnLoadScene(UnityEngine.SceneManagement.Scene scene)
        {
            StartCoroutine(UnLoadScene_Cor(scene));
        }

        public void UnLoadScene(int index)
        {
            StartCoroutine(UnLoadScene_Cor(index));
        }
        
        private static IEnumerator LoadScene_Cor(string sceneName, LoadSceneMode loadSceneMode)
        {
            _sceneCount++;
            
            var loadSceneParameters = new LoadSceneParameters(loadSceneMode, LocalPhysicsMode.Physics3D);

            yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, loadSceneParameters);

            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(_sceneCount);

            _loadedScenes.Add(scene);
            
            OnSceneLoaded?.Invoke(scene);
        }
        
        private IEnumerator UnLoadScene_Cor(UnityEngine.SceneManagement.Scene scene)
        {
            yield return UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);

            var unloadedScene = _loadedScenes[_sceneCount];

            _loadedScenes.Remove(unloadedScene);

            _sceneCount--;
        }
        
        private IEnumerator UnLoadScene_Cor(int index)
        {
            yield return UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(index, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);

            var unloadedScene = _loadedScenes[_sceneCount];

            _loadedScenes.Remove(unloadedScene);

            _sceneCount--;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }
    }

    public class SceneHandler{}

    public static class SceneExtension
    {
        public static void OnCompleted(this SceneHandler _, Action<UnityEngine.SceneManagement.Scene> action)
        {
            SceneManager.OnSceneLoaded += action.Invoke;
        }
    }
}