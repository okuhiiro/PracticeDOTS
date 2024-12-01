namespace ECS_Spatial_Partitioning
{
    using Tags;
    using Unity.Burst;
    using Unity.Burst.Intrinsics;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [BurstCompile]
    public partial struct MovingSystem : ISystem
    {
        private const float TargetReachedDistance = 0.1f;

        // The higher the value of OffScreenUpdateFrameInterval, the less often entities off screen are moved.
        // A value of 60 means that entities off screen are moved only every 60th frame (once per second at 60 fps).
        // Higher values lead to better performance, at the cost of simulation precision.
        private const int OffScreenUpdateFrameInterval = 60;

        private int _gridColumns;
        private int _gridRows;
        private int _frameCounter;
        private bool _isInitialized;

        private ComponentTypeHandle<Moving> _movingHandle;
        private ComponentTypeHandle<LocalTransform> _transformHandle;
        private ComponentTypeHandle<TargetReached> _targetReachedTagHandle;


        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Grid>();
            _movingHandle = SystemAPI.GetComponentTypeHandle<Moving>();
            _transformHandle = SystemAPI.GetComponentTypeHandle<LocalTransform>();
            _targetReachedTagHandle = SystemAPI.GetComponentTypeHandle<TargetReached>();
        }

        [BurstCompile]
        private void InitializeGrid(ref SystemState state)
        {
            var gridEntity = SystemAPI.GetSingletonEntity<Grid>();
            var grid = SystemAPI.GetComponentRW<Grid>(gridEntity);

            // Cache the grid dimensions locally
            _gridColumns = grid.ValueRO.Columns;
            _gridRows = grid.ValueRO.Rows;
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!_isInitialized)
            {
                InitializeGrid(ref state);
                _isInitialized = true;
            }

            _frameCounter++;
            _movingHandle.Update(ref state);
            _transformHandle.Update(ref state);
            _targetReachedTagHandle.Update(ref state);


            var dependency = state.Dependency;

            // First: Update all entities that are inside of the screen
            var queryInScreen = SystemAPI.QueryBuilder()
                .WithAll<Moving, LocalTransform>()
                .WithNone<OffScreen, TargetReached>()
                .Build();
            var inScreenJob = new MoveJob
            {
                FixedDeltaTime = SystemAPI.Time.DeltaTime,
                CurrentFrameCount = _frameCounter,
                Steps = 1,
                MovingType = _movingHandle,
                LocalTransformType = _transformHandle,
                TargetReachedTagType = _targetReachedTagHandle
            };
            var inScreenJobHandle = inScreenJob.ScheduleParallel(queryInScreen, dependency);
            dependency = inScreenJobHandle;

            // Secondly: Update entities that are off screen only every X frames.
            // Distribute this process over a corresponding fraction of cells per frame for performance reasons.
            var queryOffScreen = SystemAPI.QueryBuilder()
                .WithAll<GridEntity, Moving, LocalTransform, OffScreen>()
                .WithNone<TargetReached>()
                .Build();
            var offScreenJob = new MoveJob()
            {
                FixedDeltaTime = SystemAPI.Time.DeltaTime,
                CurrentFrameCount = _frameCounter,
                Steps = OffScreenUpdateFrameInterval,
                MovingType = _movingHandle,
                LocalTransformType = _transformHandle,
                TargetReachedTagType = _targetReachedTagHandle
            };

            var maxCells = _gridColumns * _gridRows;
            var cellsPerFrame = (int)math.ceil((float)maxCells / OffScreenUpdateFrameInterval);
            var start = cellsPerFrame * (_frameCounter % OffScreenUpdateFrameInterval);
            var end = start + cellsPerFrame;
            
            for (var i = start; i < (int)math.ceil(end); i++)
            {
                queryOffScreen.SetSharedComponentFilter(new GridEntity { CellIndex = i });
                var offScreenJobHandle = offScreenJob.ScheduleParallel(queryOffScreen, dependency);
                dependency = offScreenJobHandle;
            }

            state.Dependency = dependency;
        }


        [BurstCompile]
        private struct MoveJob : IJobChunk
        {
            [ReadOnly] public float FixedDeltaTime;
            [ReadOnly] public int CurrentFrameCount;
            [ReadOnly] public int Steps;

            public ComponentTypeHandle<Moving> MovingType;
            public ComponentTypeHandle<LocalTransform> LocalTransformType;
            public ComponentTypeHandle<TargetReached> TargetReachedTagType;


            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask,
                in v128 chunkEnabledMask)
            {
                var transformArray = chunk.GetNativeArray(ref LocalTransformType);
                var movingArray = chunk.GetNativeArray(ref MovingType);

                for (var i = 0; i < chunk.Count; i++)
                {
                    var movingComp = movingArray[i];
                    var currentPos = transformArray[i].Position;
                    var direction = math.normalize(movingComp.Target - currentPos);

                    //if this is not a new movement, add the frame difference since the last movement to the steps
                    var steps = Steps;
                    if (movingComp.LastMovedInFrame > 0)
                        steps = Steps + CurrentFrameCount - movingComp.LastMovedInFrame;

                    var newPosition = currentPos;
                    var targetReached = false;

                    var targetDistance = math.distance(newPosition, movingComp.Target);
                    if (targetDistance < TargetReachedDistance)
                        targetReached = true;

                    if (targetReached)
                    {
                        chunk.SetComponentEnabled(ref TargetReachedTagType, i, true);
                        movingComp.LastMovedInFrame = -1;
                    }
                    else
                    {
                        // Do not overshoot the target (this is easy to happen because we calculate multiple steps at once if off screen)
                        var moveDistance = math.min(movingComp.Speed * steps * FixedDeltaTime, targetDistance);
                        newPosition += direction * moveDistance;
                        movingComp.LastMovedInFrame = CurrentFrameCount;
                    }

                    // Update moving component changes
                    movingArray[i] = movingComp;

                    // Apply the new position
                    transformArray[i] = transformArray[i].Translate(newPosition - currentPos);
                }

                transformArray.Dispose();
                movingArray.Dispose();
            }
        }
    }
}