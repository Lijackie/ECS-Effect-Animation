using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine.Rendering;

namespace EffectAnimation
{
    partial struct EffectAnimationSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<EffectAnimationMaterialHashMap>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var effectAnimationMaterialEntity = SystemAPI.GetSingletonEntity<EffectAnimationMaterialHashMap>();
            var materialHashMap = SystemAPI.GetComponent<EffectAnimationMaterialHashMap>(effectAnimationMaterialEntity);

            EffectAnimationJob effectAnimationJob = new EffectAnimationJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                ECB = SystemAPI.GetSingletonRW<BeginSimulationEntityCommandBufferSystem.Singleton>().ValueRW.CreateCommandBuffer(state.WorldUnmanaged),
                MaterialLookup = SystemAPI.GetComponentLookup<MaterialMeshInfo>(),
                Materials = materialHashMap.Value,
            };
            state.Dependency = effectAnimationJob.Schedule(state.Dependency);
        }

        [BurstCompile]
        public partial struct EffectAnimationJob : IJobEntity
        {
            public float DeltaTime;
            public EntityCommandBuffer ECB;
            public ComponentLookup<MaterialMeshInfo> MaterialLookup;
            public NativeHashMap<FixedString32Bytes, BatchMaterialID> Materials;

            public void Execute(Entity entity, ref EffectAnimation data, ref DynamicBuffer<EffectChildrenBuffer> layers)
            {
                // animation frame data
                ref EffectAnimationData animationData = ref data.Data.Value;

                // 目前frame
                data.Time += DeltaTime;
                var newFrame = (int)math.floor(data.Time * animationData.FrameRate);
                if (data.Frame == newFrame)
                    return;
                data.Frame = newFrame;

                // 是否循環
                if (data.Frame > animationData.MaxKey)
                {
                    if (data.Loop)
                    {
                        data.Time = 1f / animationData.FrameRate;
                        data.Frame = (int)math.floor(data.Time * animationData.FrameRate);
                    }
                    else
                    {
                        // 刪除
                        ECB.SetComponent(entity, new EffectAnimationFinish
                        {
                            Value = true
                        });
                    }
                }

                for (int i = 0; i < layers.Length; i++)
                {
                    // 如果未使用就跳過，如第0層
                    if (animationData.Layers[i].Animations.Length == 0)
                        continue;

                    Entity layerEntity = layers[i].Value;
                    ref var layer = ref animationData.Layers[i];
                    var lastFrame = 0;
                    var lastSource = 0;
                    var startAnim = -1;
                    var nextAnim = -1;
                    var lastAnim = -1;

                    for (int j = 0; j < layer.Animations.Length; j++)
                    {
                        ref var a = ref layer.Animations[j];
                        if (a.Frame < data.Frame)
                        {
                            if (a.Type == 0)
                                startAnim = j;
                            if (a.Type == 1)
                                nextAnim = j;
                        }

                        lastFrame = math.max(lastFrame, a.Frame);
                        if (a.Type == 0)
                        {
                            lastSource = math.max(lastSource, a.Frame);
                            lastAnim = j;
                        }
                    }

                    //RenderLayerFixedFrame，最後
                    if (startAnim < 0 || (nextAnim < 0 && lastFrame < data.Frame))
                    {
                        /*if (lastAnim != 0 && lastFrame == lastSource)
                        {
                            ref var lastAnimationData = ref layer.Animations[lastAnim];
                            var lastTexture = layer.Textures[(int)lastAnimationData.Aniframe];

                            ECB.SetComponent(layerEntity, new EffectAnimationMeshDown
                            {
                                Value = UpdateMesh(ref lastAnimationData.XY, 2, 3, lastAnimationData.Angle)
                            });
                            ECB.SetComponent(layerEntity, new EffectAnimationMeshUp
                            {
                                Value = UpdateMesh(ref lastAnimationData.XY, 0, 1, lastAnimationData.Angle)
                            });
                            ECB.SetComponent(layerEntity, new EffectAnimationTilingAndOffset
                            {
                                Value = UpdateUV(ref animationData, lastTexture)
                            });
                            ECB.SetComponent(layerEntity, new EffectAnimationColor
                            {
                                Value = lastAnimationData.Color
                            });
                            ECB.SetComponent(layerEntity, LocalTransform.FromPosition(UpdatePosition(lastAnimationData.Position)));

                            var materialID = UpdateMaterial(ref lastAnimationData, data, in Materials);
                            if (materialID != BatchMaterialID.Null)
                            {
                                var mat = MaterialLookup[layerEntity];
                                mat.MaterialID = materialID;
                                MaterialLookup[layerEntity] = mat;
                            }

                            ECB.SetComponentEnabled<MaterialMeshInfo>(layerEntity, true);
                            continue;
                        }
                        */
                        ECB.SetComponentEnabled<MaterialMeshInfo>(layerEntity, false);
                        continue;
                    }

                    ref var from = ref layer.Animations[startAnim];

                    EffectAnimationData.AnimationEntry? to = null;

                    if (nextAnim >= 0)
                    {
                        to = layer.Animations[nextAnim];
                    }

                    var delta = data.Frame - from.Frame;
                    var materialId = UpdateMaterial(ref from, data, in Materials);
                    if (materialId != BatchMaterialID.Null)
                    {
                        var mat = MaterialLookup[layerEntity];
                        mat.MaterialID = materialId;
                        MaterialLookup[layerEntity] = mat;
                    }

                    if (nextAnim != startAnim + 1 || to?.Frame != from.Frame)
                    {
                        if (to != null && lastSource <= from.Frame)
                        {
                            ECB.SetComponentEnabled<MaterialMeshInfo>(layerEntity, false);
                            continue;
                        }

                        var fixedFrame = layer.Textures[(int)from.Aniframe];
                        ECB.SetComponent(layerEntity, new EffectAnimationMeshDown
                        {
                            Value = UpdateMesh(ref from.XY, 2, 3, from.Angle)
                        });
                        ECB.SetComponent(layerEntity, new EffectAnimationMeshUp
                        {
                            Value = UpdateMesh(ref from.XY, 0, 1, from.Angle)
                        });
                        ECB.SetComponent(layerEntity, new EffectAnimationTilingAndOffset
                        {
                            Value = UpdateUV(ref animationData, fixedFrame)
                        });
                        ECB.SetComponent(layerEntity, new EffectAnimationColor
                        {
                            Value = from.Color
                        });
                        ECB.SetComponent(layerEntity, LocalTransform.FromPosition(UpdatePosition(from.Position)));


                        ECB.SetComponentEnabled<MaterialMeshInfo>(layerEntity, true);
                        continue;
                    }

                    ref var to2 = ref layer.Animations[nextAnim];
                    NativeArray<float2> tempPositions2 = new NativeArray<float2>(4, Allocator.Temp);
                    for (var ii = 0; ii < 4; ii++)
                    {
                        tempPositions2[ii] = from.XY[ii] + to2.XY[ii] * delta;
                    }

                    var pos = from.Position + to2.Position * delta;
                    var angle = from.Angle + to2.Angle * delta;
                    var color = from.Color + to2.Color * delta;

                    var frameId = 0;

                    switch (to2.Anitype)
                    {
                        case 1:
                            frameId = (int)math.floor(from.Aniframe + to2.Aniframe * delta);
                            break;
                        case 2:
                            frameId = (int)math.floor(math.min(from.Aniframe + to2.Delay * delta, layer.Textures.Length - 1));
                            break;
                        case 3:
                            frameId = (int)math.floor((from.Aniframe + to2.Delay * delta) % layer.Textures.Length);
                            break;
                        case 4:
                            frameId = (int)math.floor((from.Aniframe - to2.Delay * delta) % layer.Textures.Length);
                            break;
                    }

                    var texIndex = layer.Textures[frameId];
                    ECB.SetComponent(layerEntity, new EffectAnimationMeshDown
                    {
                        Value = UpdateMesh(tempPositions2, 2, 3, angle)
                    });
                    ECB.SetComponent(layerEntity, new EffectAnimationMeshUp
                    {
                        Value = UpdateMesh(tempPositions2, 0, 1, angle)
                    });
                    ECB.SetComponent(layerEntity, new EffectAnimationTilingAndOffset
                    {
                        Value = UpdateUV(ref animationData, texIndex)
                    });
                    ECB.SetComponent(layerEntity, new EffectAnimationColor
                    {
                        Value = color
                    });
                    ECB.SetComponent(layerEntity, LocalTransform.FromPosition(UpdatePosition(pos)));

                    ECB.SetComponentEnabled<MaterialMeshInfo>(layerEntity, true);
                }
            }

            [BurstCompile]
            private float4 UpdateMesh(ref BlobArray<float2> pos, int index1, int index2, float angle)
            {
                var xy1 = Rotate(pos[index1], -angle * math.TORADIANS);
                var xy2 = Rotate(pos[index2], -angle * math.TORADIANS);

                return new float4(xy1, xy2) / 50f;
            }

            [BurstCompile]
            private float4 UpdateMesh(NativeArray<float2> pos, int index1, int index2, float angle)
            {
                var xy1 = Rotate(pos[index1], -angle * math.TORADIANS);
                var xy2 = Rotate(pos[index2], -angle * math.TORADIANS);
                return new float4(xy1, xy2) / 50f;
            }

            [BurstCompile]
            private float4 UpdateUV(ref EffectAnimationData data, int index)
            {
                var uv = data.AtlasRects[index];

                return uv;
            }

            [BurstCompile]
            private float3 UpdatePosition(float2 pos)
            {
                return new float3((pos.x - 320f) / 50f, -(pos.y - 320f) / 50f, 0);
            }

            [BurstCompile]
            private float2 Rotate(float2 v, float delta)
            {
                return new float2(
                            v.x * math.cos(delta) - v.y * math.sin(delta),
                            v.x * math.sin(delta) + v.y * math.cos(delta)
                );
            }

            [BurstCompile]
            private BatchMaterialID UpdateMaterial(ref EffectAnimationData.AnimationEntry entry, EffectAnimation data, in NativeHashMap<FixedString32Bytes, BatchMaterialID> map)
            {
                float srcBlend = entry.SrcAlpha;
                float destBlend = entry.DstAlpha;

                if (srcBlend == 2 && destBlend == 1)
                {
                    return map[data.Alpha];
                }

                if (srcBlend == 5 && destBlend == 6)
                {
                    return map[data.Premulitiply];
                }

                if (srcBlend == 5 && destBlend == 7)
                {
                    return map[data.Additive];
                }

                return BatchMaterialID.Null;
            }
        }
    }
}
