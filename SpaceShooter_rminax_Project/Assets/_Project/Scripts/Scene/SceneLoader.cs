using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Scenes
{
    public enum LoadOperation
    {
        Load,
        UnLoad
    }
    
    public class SceneLoader
    {
        private readonly SpaceSceneManager _sceneManager;
        
        public SceneLoader(SpaceSceneManager sceneManager)
        {
            _sceneManager = sceneManager;
        }
        
        private class SceneLoaderHandler : MonoBehaviour
        {
        }

        private static SceneLoaderHandler _sceneLoaderHandler;

        private readonly Queue<SceneLoadingTask> _scenes = new();

        private bool _isCurrentlyLoading;

        private class SceneLoadingTask
        {
            public readonly string SceneName;
            public readonly Action<Scene> OnTaskCompletedAction;

            public readonly LoadOperation LoadOperation;
            public readonly LoadSceneMode LoadMode;
            
            public readonly Scene Scene;

            public SceneLoadingTask(string sceneName, LoadOperation loadOperation, LoadSceneMode loadMode, Action<Scene> onTaskCompleted)
            {
                SceneName = sceneName;
                OnTaskCompletedAction = onTaskCompleted;
                LoadOperation = loadOperation;
                LoadMode = loadMode;
            }
            
            public SceneLoadingTask(Scene scene, LoadOperation loadOperation, Action<Scene> onTaskCompleted)
            {
                Scene = scene;
                OnTaskCompletedAction = onTaskCompleted;
                LoadOperation = loadOperation;
            }
        }

        public void LoadScene(string sceneName, LoadSceneMode loadMode, Action<Scene> onCompleted = null)
        {
            Init();

            _scenes.Enqueue(new SceneLoadingTask(sceneName, LoadOperation.Load, loadMode, onCompleted));

            if (_isCurrentlyLoading) return;

            _sceneLoaderHandler.StartCoroutine(SceneCoroutine());
        }

        public void UnloadScene(Scene scene, Action<Scene> onCompleted = null)
        {
            Init();

            _scenes.Enqueue(new SceneLoadingTask(scene, LoadOperation.UnLoad, onCompleted));

            if (_isCurrentlyLoading) return;

            _sceneLoaderHandler.StartCoroutine(SceneCoroutine());
        }

        private IEnumerator SceneCoroutine()
        {
            while (_scenes.Count > 0)
            {
                _isCurrentlyLoading = true;

                //The reason I add one is that the first scene is the menu scene and it will stay open all the time.
                var sceneIndex = _sceneManager.GetLoadedSceneCount() + 1;
                
                var task = _scenes.Dequeue();

                Scene scene = default;
                
                //If the scene is unloading, we need to get the scene info without deleting it
                if (task.LoadOperation == LoadOperation.UnLoad)
                    scene = task.Scene;

                yield return task.LoadOperation switch
                {
                    LoadOperation.Load => StartAsyncSceneLoad(task),
                    LoadOperation.UnLoad => StartAsyncSceneUnload(task),
                    _ => throw new ArgumentOutOfRangeException($"SceneLoader", "Load Operation is undefined")
                };
                
                //If the scene is loading, the scene information comes after loaded
                if(task.LoadOperation == LoadOperation.Load)
                    scene = SceneManager.GetSceneAt(sceneIndex);

                task.OnTaskCompletedAction?.Invoke(scene);
            }

            _isCurrentlyLoading = false;
        }

        private AsyncOperation StartAsyncSceneLoad(SceneLoadingTask task)
        {
            var sceneParameters = new LoadSceneParameters
            {
                loadSceneMode = task.LoadMode,
                localPhysicsMode = LocalPhysicsMode.Physics3D
            };
            
            var asyncOperation = SceneManager.LoadSceneAsync(task.SceneName, sceneParameters);

            return asyncOperation;
        }

        private AsyncOperation StartAsyncSceneUnload(SceneLoadingTask task)
        {
            var asyncOperation =
                SceneManager.UnloadSceneAsync(task.Scene, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);

            return asyncOperation;
        }

        private static void Init()
        {
            if (_sceneLoaderHandler != null) return;

            var gameObject = new GameObject("SceneLoader Handler");
            _sceneLoaderHandler = gameObject.AddComponent<SceneLoaderHandler>();
        }
    }
}