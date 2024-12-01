namespace ECS_Spatial_Partitioning
{
    using Unity.Entities;
    
    public struct Spawner : IComponentData
    {
        public Entity Prefab;
        public int SpawnAmount;
    }
}