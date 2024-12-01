namespace ECS_Spatial_Partitioning
{
    using Unity.Entities;
    
    public struct GridEntity : ISharedComponentData
    {
        public int CellIndex; // Index of the GridCell this entity is currently inside of
    }
}