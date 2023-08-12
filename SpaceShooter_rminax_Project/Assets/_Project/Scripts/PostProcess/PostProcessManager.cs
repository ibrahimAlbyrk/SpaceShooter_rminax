using Mirror;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace _Project.Scripts.PostProcess
{
    using Network;
    
    public class PostProcessManager : NetIdentity
    {
        [SerializeField] private PostProcessVolume _volume;
        
        [SerializeField] private PostProcessProfile GlobalProcess;
        [SerializeField] private PostProcessProfile ZoneArenaProcess;

        [ClientRpc]
        public void RPC_SetPostProcess(PostProcessMode mode)
        {
            _volume.profile = mode switch
            {
                PostProcessMode.Global => GlobalProcess,
                PostProcessMode.ZoneArea => ZoneArenaProcess,
                _ => _volume.profile
            };
        }
    }

    public enum PostProcessMode
    {
        Global,
        ZoneArea
    }
}