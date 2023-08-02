using Unity.Jobs;
using Unity.Burst;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

namespace _Project.Scripts.Game.Mod
{
    [BurstCompile]
    internal struct MeteorPositionJob : IJobParallelFor
    {
        public float3 Scale;

        public NativeArray<float3> Positions;
        public NativeArray<float3> Rotations;
        public NativeArray<Matrix4x4> Matrices;

        
        public void Execute(int index)
        {
            Matrices[index] = Matrix4x4.TRS(Positions[index],
                quaternion.Euler(Rotations[index]),
                Scale);
        }
    }
}