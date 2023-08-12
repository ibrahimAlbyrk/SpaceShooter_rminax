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
        
        public static PostProcessManager Instance;
        
        public void SetPostProcess(PostProcessMode mode) => CMD_SetPostProcess(mode);

        [Command(requiresAuthority = false)]
        private void CMD_SetPostProcess(PostProcessMode mode) => RPC_SetPostProcess(mode);

        [ClientRpc]
        private void RPC_SetPostProcess(PostProcessMode mode)
        {
            _volume.profile = mode switch
            {
                PostProcessMode.Global => GlobalProcess,
                PostProcessMode.ZoneArea => ZoneArenaProcess,
                _ => _volume.profile
            };
        }
        
        private void Awake()
        {
            Instance = this;
        }
    }

    public enum PostProcessMode
    {
        Global,
        ZoneArea
    }
}