using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace EffectAnimation
{
    [RequireMatchingQueriesForUpdate]
    partial class EffectAnimationRegisterMaterialSystem : SystemBase
    {
        private NativeHashMap<FixedString32Bytes, BatchMaterialID> materialHashMap;

        protected override void OnStartRunning()
        {
            var entityManager = EntityManager;
            var hybridRenderer = World.GetOrCreateSystemManaged<EntitiesGraphicsSystem>();

            var materialTag = SystemAPI.GetSingletonEntity<EffectAnimationMaterialTag>();

            // 取得三個Material，Alpha、Premulitiply、Additive
            // 為什麼要三個，因為不能用MaterialProperty改BlendMode，以後有找到解決方案再修改
            var materials = entityManager.GetComponentObject<EffectAnimationMaterial>(materialTag);
            
            // 要註冊的特效檔案，取得紋理用
            var files = entityManager.GetComponentObject<EffectAnimationFiles>(materialTag);
            
            materialHashMap = new NativeHashMap<FixedString32Bytes, BatchMaterialID>(files.AnimationFiles.Count, Allocator.Persistent);
            foreach (var file in files.AnimationFiles)
            {
                var code = new FixedString32Bytes((file.name + "alpha"));
                var code2 = new FixedString32Bytes((file.name + "premulitiply"));
                var code3 = new FixedString32Bytes((file.name + "additive"));

                Material alpha = new Material(materials.AlphaMaterial);
                alpha.SetTexture("_Texture2D", file.Atlas);
                materialHashMap[code] = hybridRenderer.RegisterMaterial(alpha);

                Material premulitiply = new Material(materials.PremulitiplyMaterial);
                premulitiply.SetTexture("_Texture2D", file.Atlas);
                materialHashMap[code2] = hybridRenderer.RegisterMaterial(premulitiply);

                Material additive = new Material(materials.AdditiveMaterial);
                additive.SetTexture("_Texture2D", file.Atlas);
                materialHashMap[code3] = hybridRenderer.RegisterMaterial(additive);
            }

            entityManager.AddComponentData(entityManager.CreateEntity(), new EffectAnimationMaterialHashMap
            {
                Value = materialHashMap
            });
        }

        protected override void OnUpdate()
        {
        
        }

        protected override void OnDestroy()
        {
            materialHashMap.Dispose();
        }
    }
}