namespace ECS_Spatial_Partitioning
{
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Transforms;
    using Tags;
    
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [BurstCompile]
    public partial struct GridSystem : ISystem
    {
        private float _cellSize;
        private int _gridColumns;
        private int _gridRows;
        private bool _isInitialized;

        private ComponentType _localToWorldType;


        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Grid>();
            _localToWorldType = typeof(LocalToWorld);
        }

        [BurstCompile]
        private void InitializeGrid(ref SystemState state)
        {
            var gridEntity = SystemAPI.GetSingletonEntity<Grid>();
            var grid = SystemAPI.GetComponentRW<Grid>(gridEntity);

            // Cache the grid dimensions locally
            _cellSize = grid.ValueRO.CellSize;
            _gridColumns = grid.ValueRO.Columns;
            _gridRows = grid.ValueRO.Rows;
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!_isInitialized)
            {
                InitializeGrid(ref state);
                _isInitialized = true;
            }

            UpdateEntityCellRelation(ref state);
        }


        [BurstCompile]
        private void UpdateEntityCellRelation(ref SystemState state)
        {
            // Loop through all relevant entities and update their assigned grid cell if their position changed to a new cell
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            var query = SystemAPI.QueryBuilder()
                .WithAll<GridEntity, LocalToWorld>()
                .WithNone<NeverChangesGridTag>()
                .Build();
            query.SetChangedVersionFilter(_localToWorldType);
            var job = new UpdateEntityCellRelationJob
            {
                Ecb = ecb.AsParallelWriter(),
                CellSize = _cellSize,
                GridColumns = _gridColumns,
                GridRows = _gridRows
            };
            var jobHandle = job.ScheduleParallel(query, state.Dependency);
            jobHandle.Complete();
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
            state.Dependency = jobHandle;
        }


        [BurstCompile]
        private partial struct UpdateEntityCellRelationJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter Ecb;
            [ReadOnly] public float CellSize;
            [ReadOnly] public int GridColumns;
            [ReadOnly] public int GridRows;

            private void Execute(Entity entity, [EntityIndexInQuery] int entityIndex, GridEntity gridCellEntity,
                RefRO<LocalToWorld> localToWorld)
            {
                var pos = localToWorld.ValueRO.Position;
                var cellX = (int)((pos.x + GridColumns * CellSize * 0.5f) / CellSize);
                var cellY = (int)((pos.y + GridRows * CellSize * 0.5f) / CellSize);
                var currentCellIndex = cellX * GridRows + cellY;

                if (gridCellEntity.CellIndex == currentCellIndex)
                    return;

                Ecb.SetSharedComponent(entityIndex, entity, new GridEntity { CellIndex = currentCellIndex });
            }
        }
    }
    
}