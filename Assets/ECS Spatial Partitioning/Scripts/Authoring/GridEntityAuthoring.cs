namespace ECS_Spatial_Partitioning.Authoring
{
    using Tags;
    using Unity.Entities;
    using UnityEngine;
    
    public class GridEntityAuthoring : MonoBehaviour
    {
        public class GridCellEntityBaker : Baker<GridEntityAuthoring>
        {
            public override void Bake(GridEntityAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var sharedComponent = new GridEntity
                {
                    CellIndex = -1
                };
                AddSharedComponent(entity, sharedComponent);

                //Every GridEntity needs this tags 
                AddComponent(entity, typeof(OffScreen));
            }
        }
    }
}