using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace EffectAnimation
{
    partial struct BillboardObjectSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BillboardObject>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var transform in SystemAPI.Query<RefRW<LocalTransform>>().WithAll<BillboardObject>()) 
            {
                transform.ValueRW.Rotation = Camera.main.transform.rotation;
            }
        }
    }
}
