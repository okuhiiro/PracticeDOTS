namespace ECS_Spatial_Partitioning
{
    using Tags;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Mathematics;
    using Unity.Entities;
    using Unity.Jobs;
    using UnityEngine;
    using FrustumPlanes = Unity.Rendering.FrustumPlanes;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [BurstCompile]
    public partial struct DetectOffScreenSystem : ISystem
    {
        private NativeArray<float4> _frustumPlanesArr;
        private NativeArray<float2> _gridCellCornerA;
        private NativeArray<float2> _gridCellCornerB;
        private NativeArray<float2> _gridCellCornerC;
        private NativeArray<float2> _gridCellCornerD;
        private NativeArray<bool> _gridCellOffScreen;
        private Vector3 _oldCamPos;
        private ComponentType _gridEntityType;
        private float _cellSize;

        public void OnCreate(ref SystemState state)
        {
            _frustumPlanesArr = new NativeArray<float4>(6, Allocator.Persistent);
            state.RequireForUpdate<Grid>();
            _gridEntityType = typeof(GridEntity);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            _frustumPlanesArr.Dispose();
            _gridCellCornerA.Dispose();
            _gridCellCornerB.Dispose();
            _gridCellCornerC.Dispose();
            _gridCellCornerD.Dispose();
            _gridCellOffScreen.Dispose();
        }


        public void OnUpdate(ref SystemState state)
        {
            if (_gridCellOffScreen.Length == 0) InitializeGridCorners(ref state);

            var camPos = Camera.main.transform.position;
            FrustumPlanes.FromCamera(Camera.main, _frustumPlanesArr);
            if (camPos != _oldCamPos) SetOffScreenTagsForCells(ref state);

            SetOffScreenTagsForCellCrossingEntities(ref state);

            _oldCamPos = camPos;
        }


        [BurstCompile]
        private void InitializeGridCorners(ref SystemState state)
        {
            var gridEntity = SystemAPI.GetSingletonEntity<Grid>();
            var grid = SystemAPI.GetComponentRW<Grid>(gridEntity);

            // Cache the grid dimensions locally
            _cellSize = grid.ValueRO.CellSize;
            var gridColumns = grid.ValueRO.Columns;
            var gridRows = grid.ValueRO.Rows;

            //Create four arrays, one for each corner of all cells
            _gridCellCornerA = new NativeArray<float2>(gridColumns * gridRows, Allocator.Persistent);
            _gridCellCornerB = new NativeArray<float2>(gridColumns * gridRows, Allocator.Persistent);
            _gridCellCornerC = new NativeArray<float2>(gridColumns * gridRows, Allocator.Persistent);
            _gridCellCornerD = new NativeArray<float2>(gridColumns * gridRows, Allocator.Persistent);
            _gridCellOffScreen = new NativeArray<bool>(gridColumns * gridRows, Allocator.Persistent);

            // Calculate the offset based on the grid's origin
            var offset = new float2(gridColumns * _cellSize * 0.5f, gridRows * _cellSize * 0.5f);

            for (var x = 0; x < gridColumns; x++)
            for (var y = 0; y < gridRows; y++)
            {
                var index = x * gridRows + y;
                _gridCellCornerA[index] = new float2(x * _cellSize - offset.x, y * _cellSize - offset.y); //bottom left
                _gridCellCornerB[index] =
                    new float2(x * _cellSize + _cellSize - offset.x, y * _cellSize - offset.y); //bottom right
                _gridCellCornerC[index] =
                    new float2(x * _cellSize - offset.x, y * _cellSize + _cellSize - offset.y); //top left
                _gridCellCornerD[index] =
                    new float2(x * _cellSize + _cellSize - offset.x, y * _cellSize + _cellSize - offset.y); //top right
                _gridCellOffScreen[index] = true; //All grids are considered off screen in the beginning
            }
        }

        [BurstCompile]
        private void SetOffScreenTagsForCells(ref SystemState state)
        {
            var previousState = new NativeArray<bool>(_gridCellOffScreen.Length, Allocator.Temp);
            previousState.CopyFrom(_gridCellOffScreen);

            var dependency = state.Dependency;
            new UpdateArrayJob
            {
                FrustumPlanesArr = _frustumPlanesArr,
                GridCellCornerA = _gridCellCornerA,
                GridCellCornerB = _gridCellCornerB,
                GridCellCornerC = _gridCellCornerC,
                GridCellCornerD = _gridCellCornerD,
                GridCellOffScreen = _gridCellOffScreen,
                CellSize = _cellSize
            }.ScheduleParallel(_gridCellOffScreen.Length, 64, dependency).Complete();
            
            for (var cellIndex = 0; cellIndex < _gridCellOffScreen.Length; cellIndex++)
            {
                if (previousState[cellIndex] == _gridCellOffScreen[cellIndex])
                    continue;
                var query = SystemAPI.QueryBuilder().WithAll<OffScreen, GridEntity>()
                    .WithOptions(EntityQueryOptions.IgnoreComponentEnabledState).Build();
                query.SetSharedComponentFilter(new GridEntity { CellIndex = cellIndex });
                var jobHandle = new SetOffScreenTagsForCellJob
                {
                    NewStatus = _gridCellOffScreen[cellIndex]
                }.ScheduleParallel(query, dependency);
                dependency = jobHandle;
            }

            state.Dependency = dependency;
            previousState.Dispose();
        }

        private struct UpdateArrayJob : IJobFor
        {
            [ReadOnly] public NativeArray<float4> FrustumPlanesArr;
            [ReadOnly] public NativeArray<float2> GridCellCornerA;
            [ReadOnly] public NativeArray<float2> GridCellCornerB;
            [ReadOnly] public NativeArray<float2> GridCellCornerC;
            [ReadOnly] public NativeArray<float2> GridCellCornerD;
            [ReadOnly] public float CellSize;

            public NativeArray<bool> GridCellOffScreen;

            public void Execute(int cellIndex)
            {
                if (IsInsideFrustum(ref FrustumPlanesArr, GridCellCornerA[cellIndex].x, GridCellCornerA[cellIndex].y, CellSize)
                    || IsInsideFrustum(ref FrustumPlanesArr, GridCellCornerB[cellIndex].x, GridCellCornerB[cellIndex].y, CellSize)
                    || IsInsideFrustum(ref FrustumPlanesArr, GridCellCornerC[cellIndex].x, GridCellCornerC[cellIndex].y, CellSize)
                    || IsInsideFrustum(ref FrustumPlanesArr, GridCellCornerD[cellIndex].x, GridCellCornerD[cellIndex].y, CellSize))
                    GridCellOffScreen[cellIndex] = false;
                else
                    GridCellOffScreen[cellIndex] = true;
            }
        }

        [BurstCompile]
        private partial struct SetOffScreenTagsForCellJob : IJobEntity
        {
            [ReadOnly] public bool NewStatus;

            private void Execute(EnabledRefRW<OffScreen> offScreenDetection)
            {
                offScreenDetection.ValueRW = NewStatus;
            }
        }


        [BurstCompile]
        private void SetOffScreenTagsForCellCrossingEntities(ref SystemState state)
        {
            var query = SystemAPI.QueryBuilder()
                .WithAll<GridEntity, OffScreen>()
                .WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)
                .Build();

            query.SetChangedVersionFilter(_gridEntityType);

            var job = new SetTagForEntitiesJob
            {
                GridCellOffScreen = _gridCellOffScreen
            };

            var jobHandle = job.ScheduleParallel(query, state.Dependency);
            state.Dependency = jobHandle;
        }

        [BurstCompile]
        private partial struct SetTagForEntitiesJob : IJobEntity
        {
            [ReadOnly] public NativeArray<bool> GridCellOffScreen;

            private void Execute(GridEntity gridEntity, EnabledRefRW<OffScreen> offScreenTag)
            {
                if (gridEntity.CellIndex < 0 || gridEntity.CellIndex >= GridCellOffScreen.Length)
                    return;

                offScreenTag.ValueRW = GridCellOffScreen[gridEntity.CellIndex];
            }
        }

        [BurstCompile]
        private static bool IsInsideFrustum(ref NativeArray<float4> frustumPlanes, float positionX, float positionY, float cellSize)
        {
            // a little offset so entities don't visibly pop in at the screen corners (large entities or small frustums)
            // This also solves the issue of zooming so far in that the frustum planes are inside the grid 
            var leeway = cellSize * -1;

            //left, right, bottom, top, [ignored: near, far]
             for (var i = 0; i < 4; i++)
             {
                 var plane = frustumPlanes[i];
                 var distanceToPlane = math.dot(plane.xy, new float2(positionX, positionY)) + plane.w;
            
                 // If the point is behind any of the frustum planes, it is outside the frustum
                 if (distanceToPlane < leeway)
                     return false;
             }

            return true;
        }
    }
}