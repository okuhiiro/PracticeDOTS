using UnityEngine.Serialization;

namespace ECS_Spatial_Partitioning.Authoring
{
    using Unity.Entities;
    using UnityEngine;

    public class GridAuthoring : MonoBehaviour
    {
        public float CellSize = 100f;
        [FormerlySerializedAs("GridWidth")] public int GridColumns = 20;
        [FormerlySerializedAs("GridHeight")] public int GridRows = 20;

        public class GridBaker : Baker<GridAuthoring>
        {
            public override void Bake(GridAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity,
                    new ECS_Spatial_Partitioning.Grid
                    {
                        CellSize = authoring.CellSize,
                        Columns = authoring.GridColumns,
                        Rows = authoring.GridRows
                    });
            }
        }
    }
}