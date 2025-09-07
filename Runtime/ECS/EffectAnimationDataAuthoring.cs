using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

namespace EffectAnimation
{
    public class EffectAnimationDataAuthoring : MonoBehaviour
    {
        public class Baker : Baker<EffectAnimationDataAuthoring>
        {
            public override void Bake(EffectAnimationDataAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent<EffectAnimationMeshDown>(entity);
                AddComponent<EffectAnimationMeshUp>(entity);
                AddComponent<EffectAnimationTilingAndOffset>(entity);
                AddComponent<EffectAnimationColor>(entity);
            }
        }
    }

    [MaterialProperty("_MeshDown")]
    public struct EffectAnimationMeshDown : IComponentData
    {
        public float4 Value;
    }

    [MaterialProperty("_MeshUp")]
    public struct EffectAnimationMeshUp : IComponentData
    {
        public float4 Value;
    }

    [MaterialProperty("_TilingAndOffset")]
    public struct EffectAnimationTilingAndOffset : IComponentData
    {
        public float4 Value;
    }

    [MaterialProperty("_Color")]
    public struct EffectAnimationColor : IComponentData
    {
        public float4 Value;
    }
}