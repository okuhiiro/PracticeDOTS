namespace ECS_Spatial_Partitioning
{
    using Unity.Entities;
    
    public struct Grid : IComponentData
    {
        public float CellSize; // The size of each grid cell
        public int Columns; //amount of cells x
        public int Rows; //amount of cells y
        
        // Good values depend heavily on your use-case.
        // In general it's a good idea to don't have too many cells, as the overhead of having many cells can be significant.
        // If your unit density is high, more cells can be worth it.
        // If your unit density is low, go for fewer cells and make them larger.
    }
}