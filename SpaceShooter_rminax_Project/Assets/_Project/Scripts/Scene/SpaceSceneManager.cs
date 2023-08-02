using System;
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

        private readonly SceneLoader _loader = new();
        
        public static event Action<Scene> OnSceneLoaded;

        #region SceneHandler Methods

        public void LoadScene(string sceneName, LoadSceneMode loadSceneMode = LoadSceneMode.Single, Action<Scene> onCompleted = null)
        {
            _loader.LoadScene(sceneName, loadSceneMode,
                scene =>
                {
                    onCompleted?.Invoke(scene);
                    OnSceneLoaded?.Invoke(scene);
                });
        }

        public void UnLoadScene(Scene scene)
        {
            _loader.UnloadScene(scene);
        }

        #endregion
    }
}