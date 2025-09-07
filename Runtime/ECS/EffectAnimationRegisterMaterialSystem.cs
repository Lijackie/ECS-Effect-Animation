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

            // ���o�T��Material�AAlpha�BPremulitiply�BAdditive
            // ������n�T�ӡA�]�������MaterialProperty��BlendMode�A�H�ᦳ���ѨM��צA�ק�
            var materials = entityManager.GetComponentObject<EffectAnimationMaterial>(materialTag);
            
            // �n���U���S���ɮסA���o���z��
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