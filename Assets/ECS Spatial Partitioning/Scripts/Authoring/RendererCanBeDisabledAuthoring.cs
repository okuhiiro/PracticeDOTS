namespace ECS_Spatial_Partitioning.Authoring
{
    using Tags;
    using Unity.Entities;
    using UnityEngine;
    
    public class CanBeDisabledAuthoring : MonoBehaviour
    {
        public class RendererCanBeDisabledBaker : Baker<CanBeDisabledAuthoring>
        {
            public override void Bake(CanBeDisabledAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new RendererCanBeDisabled());
            }
        }
    }
}