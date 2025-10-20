using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Rendering;

namespace EffectAnimation
{
    public class EffectAnimationMaterialAuthoring : MonoBehaviour
    {
        public Material AlphaMaterial;
        public Material PremulitiplyMaterial;
        public Material AdditiveMaterial;
        public List<AnimationFile> effects;

        public class Baker : Baker<EffectAnimationMaterialAuthoring>
        {
            public override void Bake(EffectAnimationMaterialAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);

                AddComponent<EffectAnimationMaterialTag>(entity);
                AddComponentObject(entity, new EffectAnimationMaterial
                {
                    AlphaMaterial = authoring.AlphaMaterial,
                    PremulitiplyMaterial = authoring.PremulitiplyMaterial,
                    AdditiveMaterial = authoring.AdditiveMaterial,
                });
                AddComponentObject(entity, new EffectAnimationFiles
                {
                    AnimationFiles = authoring.effects
                });
            }
        }
    }

    public struct EffectAnimationMaterialTag : IComponentData { }

    public class EffectAnimationMaterial : IComponentData 
    {
        public Material AlphaMaterial;
        public Material PremulitiplyMaterial;
        public Material AdditiveMaterial;
    }

    public class EffectAnimationFiles : IComponentData
    {
        public List<AnimationFile> AnimationFiles;
    }

    public struct EffectAnimationMaterialHashMap : IComponentData
    {
        public NativeHashMap<FixedString32Bytes, BatchMaterialID> Value;
    }
}
