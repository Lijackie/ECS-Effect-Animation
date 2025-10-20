using Unity.Entities;
using UnityEngine;

namespace EffectAnimation
{
    public class BillboardObjectAuthoring : MonoBehaviour
    {
        public class Baker : Baker<BillboardObjectAuthoring>
        {
            public override void Bake(BillboardObjectAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<BillboardObject>(entity);
            }
        }
    }

    public struct BillboardObject : IComponentData { }
}
