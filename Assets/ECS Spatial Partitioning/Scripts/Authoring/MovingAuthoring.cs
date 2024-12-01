namespace ECS_Spatial_Partitioning.Authoring
{
    using Tags;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine;
    
    [BurstCompile]
    public class MovingAuthoring : MonoBehaviour
    {
        public float speed = 1;
        public float3 target;

        public class MovingBaker : Baker<MovingAuthoring>
        {
            public override void Bake(MovingAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Moving
                {
                    Speed = authoring.speed,
                    Target = authoring.target
                });

                //Every Moving component needs this tag: 
                AddComponent(entity, typeof(TargetReached));
                AddComponent(entity, typeof(ReadyToMove));
            }
        }
    }
}