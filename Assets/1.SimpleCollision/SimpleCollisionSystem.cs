using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(TransformSystemGroup))]
partial struct SimpleCollisionSystem : ISystem
{
    private const int Diameter = 2;
    
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimpleCollision>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        NoJobCollision(ref state);
        //JobCollision(ref state);

        //state.Enabled = false;
    }

    private void NoJobCollision(ref SystemState state)
    {
        foreach (var (transform1, entity1) in
                 SystemAPI.Query<RefRW<LocalTransform>>().WithEntityAccess())
        {
            foreach (var (transform2, entity2) in
                     SystemAPI.Query<RefRW<LocalTransform>>().WithEntityAccess())
            {
                if (entity2.Index == entity1.Index)
                    continue;
                
                var vec = transform2.ValueRO.Position - transform1.ValueRO.Position;
                
                float3 dir;
                if (vec.Equals(float3.zero))
                {
                    dir = new float3(0, 1, 0);
                }
                else
                {
                    dir = math.normalizesafe(vec);
                }
                var distance = math.sqrt(math.lengthsq(vec));
                
                if (distance < Diameter)
                {
                    transform1.ValueRW.Position -= (Diameter - distance) * dir / 2;
                    transform2.ValueRW.Position += (Diameter - distance ) * dir / 2;
                }
            }
        }
    }

    private void JobCollision(ref SystemState state)
    {
        var world = state.WorldUnmanaged;
        var count = state.GetEntityQuery(ComponentType.ReadOnly<LocalTransform>()).CalculateEntityCount();
        
        var entityPositions= CollectionHelper.CreateNativeArray<float3, RewindableAllocator>(count, ref world.UpdateAllocator);
        
        new CreatePositionsJob()
        {
            entityPositions = entityPositions
        }.ScheduleParallel();
        
        new CollisionJob()
        {
            entityPositions = entityPositions
        }.ScheduleParallel();
    }
    
    [BurstCompile]
    public partial struct CreatePositionsJob : IJobEntity
    {
        public NativeArray<float3> entityPositions;
        
        void Execute([EntityIndexInQuery] int entityInQueryIndex, in LocalTransform transform)
        {
            entityPositions[entityInQueryIndex] = transform.Position;
        }
    }
    
    [BurstCompile]
    public partial struct CollisionJob : IJobEntity
    {
        public NativeArray<float3> entityPositions;
        
        void Execute([EntityIndexInQuery] int entityInQueryIndex, ref LocalTransform transform)
        {
            for (int i = 0; i < entityPositions.Length; i++)
            {
                if (i == entityInQueryIndex)
                    continue;

                var target = entityPositions[i];
                var vec = target - transform.Position;
                
                float3 dir;
                if (vec.Equals(float3.zero))
                {
                    dir = new float3(0, 1, 0);
                }
                else
                {
                    dir = math.normalizesafe(vec);
                }
                var distance = math.sqrt(math.lengthsq(vec));
                
                if (distance < Diameter)
                {
                    transform.Position -= (Diameter - distance) * dir / 2;
                    //target += (Diameter - distance ) * dir / 2;
                }
            }
        }
    }
}
