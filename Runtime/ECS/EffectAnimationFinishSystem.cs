using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Entities;

namespace EffectAnimation
{
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    [UpdateBefore(typeof(EndSimulationEntityCommandBufferSystem))]
    partial struct EffectAnimationFinishSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EffectAnimationFinishJob effectAnimationFinishJob = new EffectAnimationFinishJob
            {
                ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().
                CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
            };
            state.Dependency = effectAnimationFinishJob.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        public partial struct EffectAnimationFinishJob : IJobEntity, IJobEntityChunkBeginEnd
        {
            public EntityCommandBuffer.ParallelWriter ECB;

            private int _chunkIndex;

            public void Execute(Entity entity, in EffectAnimationFinish effect)
            {
                if (effect.Value)
                {
                    ECB.DestroyEntity(_chunkIndex, entity);
                }
            }

            public bool OnChunkBegin(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                _chunkIndex = unfilteredChunkIndex;
                return true;
            }

            public void OnChunkEnd(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask, bool chunkWasExecuted)
            {
            }
        }
    }
}
