using _Project.Scripts.Extensions;
using _Project.Scripts.Scenes;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Room
{
    public class RoomSingletonManager
    {
        [RuntimeInitializeOnLoadMethod]
        private static void RuntimeInitializeOnLoad()
        {
            SpaceSceneManager.OnSceneLoaded += OnSceneLoaded;
            SpaceSceneManager.OnSceneUnloading += OnSceneUnloading;
        }
    
        private static void OnSceneLoaded(Scene scene)
        {
            foreach (var obj in scene.GetRootGameObjects())
            {
                foreach (var mono in obj.GetComponents<MonoBehaviour>())
                {
                    if (mono.GetType().IsDefined(typeof(RoomSingletonAttribute), false))
                    {
                        mono.gameObject.GameContainer().Add(mono);
                    }
                }
            }
        }
    
        private static void OnSceneUnloading(Scene scene)
        {
            foreach (var obj in scene.GetRootGameObjects())
            {
                foreach (var mono in obj.GetComponents<MonoBehaviour>())
                {
                    if (mono.GetType().IsDefined(typeof(RoomSingletonAttribute), false))
                    {
                        mono.gameObject.GameContainer().Remove(mono);
                    }
                }
            }
        }
    }
}