namespace ECS_Spatial_Partitioning.Authoring
{
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine;
    
    [BurstCompile]
    public class SpawnerAuthoring : MonoBehaviour
    {
        public GameObject prefab;
        public int spawnAmount = 10000;

        public class SpawnerBaker : Baker<SpawnerAuthoring>
        {
            public override void Bake(SpawnerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity,
                    new Spawner
                    {
                        Prefab = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic),
                        SpawnAmount = authoring.spawnAmount
                    });
            }
        }
    }
}