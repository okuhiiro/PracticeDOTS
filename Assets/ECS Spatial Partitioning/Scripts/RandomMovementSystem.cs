namespace ECS_Spatial_Partitioning
{
    using Tags;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;
    using Random = Unity.Mathematics.Random;
    
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    [BurstCompile]
    public partial struct RandomMovementSystem : ISystem
    {
        private int _frameCounter;

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _frameCounter++;

            var queryWait = SystemAPI.QueryBuilder().WithAll<Moving, TargetReached>().WithDisabled<ReadyToMove>()
                .Build();
            var jobWait = new WaitJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime
            };
            var jobHandleWait = jobWait.ScheduleParallel(queryWait, state.Dependency);

            var queryMove = SystemAPI.QueryBuilder().WithAll<Moving, LocalTransform, ReadyToMove>().Build();
            var jobMove = new RandomMovementJob
            {
                RandomGen = new Random((uint)_frameCounter)
            };
            var jobHandleMove = jobMove.ScheduleParallel(queryMove, jobHandleWait);
            state.Dependency = jobHandleMove;
        }


        [BurstCompile]
        public partial struct WaitJob : IJobEntity
        {
            [ReadOnly] public float DeltaTime;

            private void Execute(Entity entity, RefRW<Moving> moving, EnabledRefRW<TargetReached> targetReachedTag,
                EnabledRefRW<ReadyToMove> waitFinishedTag)
            {
                // Entities should wait for some time at target position before being assigned a new target
                moving.ValueRW.WaitTime -= DeltaTime;

                if (moving.ValueRO.WaitTime <= 0)
                {
                    targetReachedTag.ValueRW = false;
                    waitFinishedTag.ValueRW = true;
                }
            }
        }

        [BurstCompile]
        public partial struct RandomMovementJob : IJobEntity
        {
            [ReadOnly] public Random RandomGen;

            private void Execute(Entity entity, RefRW<Moving> moving, RefRO<LocalTransform> transform,
                EnabledRefRW<ReadyToMove> waitFinishedTag)
            {
                waitFinishedTag.ValueRW = false;

                // Setup new random target nearby but within the playable area
                const float range = 200f;
                const float areaSizeValue = 15000f; //assumes a square playable area with the center at 0,0
                var newPosition = transform.ValueRO.Position + RandomGen.NextFloat3(-range, range);
                //Move away from the edges (pure random would lead to clustering at the edges which is not a realistic use case)
                if (newPosition.x < -areaSizeValue || newPosition.x > areaSizeValue
                    || newPosition.y < -areaSizeValue || newPosition.y > areaSizeValue)
                    newPosition = RandomGen.NextFloat3(-areaSizeValue, areaSizeValue);

                newPosition.x = math.clamp(newPosition.x, -areaSizeValue, areaSizeValue);
                newPosition.y = math.clamp(newPosition.y, -areaSizeValue, areaSizeValue);
                newPosition.z = -1f;

                // Also change the speed for demo purposes - why not
                var newSpeed = RandomGen.NextFloat(1f, 3f);

                // Apply new values and activate the component
                moving.ValueRW.Target = newPosition;
                moving.ValueRW.Speed = newSpeed;
                moving.ValueRW.WaitTime = RandomGen.NextFloat(0, 3); //time in seconds
            }
        }
    }
}