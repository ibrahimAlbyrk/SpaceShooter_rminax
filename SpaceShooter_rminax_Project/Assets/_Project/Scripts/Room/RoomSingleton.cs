﻿using Mirror;
using UnityEngine;

namespace _Project.Scripts.Room
{
    using Game;
    
    [RequireComponent(typeof(NetworkIdentity))]
    public class RoomSingleton<T> : NetworkBehaviour where T : class
    {
        protected virtual void Awake()
        {
            GameContainer.Add(gameObject.scene, this as T);
        }

        protected virtual void OnDestroy()
        {
            GameContainer.Remove<T>(gameObject.scene);
        }
    }
}