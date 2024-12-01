using Unity.Entities;

namespace ECS_Spatial_Partitioning.Tags
{
    // This Tag can be used for entities that are GridEntities but should never have their GridEntity.CellIndex updated
    // This is useful for Entities that are generated elsewhere with GridEntity.CellIndex set to the correct value
    // e.g. Tile Entities that never move
    public class NeverChangesGridTag : IComponentData { }
    
}
