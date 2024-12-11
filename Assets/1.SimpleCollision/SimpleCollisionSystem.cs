using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(TransformSystemGroup))]
partial struct SimpleCollisionSystem : ISystem
{
    private const int Diameter = 2;
    private const int Radius = 1;
    
    NativeList<Entity> m_entities;
    NativeList<LocalTransform> m_Transforms;
    
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimpleCollision>();
        
        m_entities = new NativeList<Entity>(256, Allocator.Persistent);
        m_Transforms = new NativeList<LocalTransform>(256, Allocator.Persistent);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        NoJobCollision(ref state);
        //JobCollision(ref state);
    }
    
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        m_entities.Dispose();
        m_Transforms.Dispose();
    }
    
    [BurstCompile]
    private void NoJobCollision(ref SystemState state)
    {
        foreach (var (transform, entity) in
                 SystemAPI.Query<RefRW<LocalTransform>>().WithEntityAccess())
        {
            float2 Displacement = float2.zero;
            uint Weight = 0;
            
            foreach (var (otherTransform, otherEntity) in
                     SystemAPI.Query<RefRW<LocalTransform>>().WithEntityAccess())
            {
                float2 towards = transform.ValueRO.Position.xy - otherTransform.ValueRO.Position.xy;
                
                float distancesq = math.lengthsq(towards);
                float radiusSum = Radius + Radius;
                if (distancesq > radiusSum * radiusSum || entity == otherEntity)
                    continue;
                
                float distance = math.sqrt(distancesq);
                float penetration = radiusSum - distance;
                penetration = (penetration / distance);
                
                Displacement += towards * penetration;
                Weight++;
            }
            
            if (Weight > 0)
            {
                Displacement = Displacement / Weight;
                transform.ValueRW.Position += new float3(Displacement, 0);
            }
        }
    }
    
    [BurstCompile]
    private void JobCollision(ref SystemState state)
    {
        m_entities.Clear();
        m_Transforms.Clear();
        
        CopyJob job1 = new CopyJob
        {
            Entities = m_entities,
            Transforms = m_Transforms,
        };
        JobHandle job1Handle = job1.Schedule(state.Dependency);
        job1Handle.Complete();
        
        HashCollisionJob job2 = new HashCollisionJob
        {
            Entities = m_entities,
            Transforms = m_Transforms.AsArray()
        };
        state.Dependency = job2.ScheduleParallel(state.Dependency);
    }
    
    [BurstCompile]
    partial struct CopyJob : IJobEntity
    {
        public NativeList<Entity> Entities;
        public NativeList<LocalTransform> Transforms;

        void Execute(in Entity entity, in LocalTransform transform)
        {
            Entities.Add(entity);
            Transforms.Add(transform);
        }
    }

    [BurstCompile]
    partial struct HashCollisionJob : IJobEntity
    {
        [ReadOnly]
        public NativeList<Entity> Entities;
        [ReadOnly]
        public NativeArray<LocalTransform> Transforms;
        
        void Execute(in Entity entity, ref LocalTransform transform)
        {
            var action = new MyCollision
            {
                Entity = entity,
                Transform = transform,
            };
            var length = Entities.Length;
            for (int i = 0; i < length; i++)
            {
                action.Execute(Entities[i], Transforms[i]);
            }
            if (action.Weight > 0)
            {
                action.Displacement = action.Displacement / action.Weight;
                transform.Position += new float3(action.Displacement, 0);
            }
        }
    }

    struct MyCollision
    {
        [ReadOnly]
        public Entity Entity;
        [ReadOnly]
        public LocalTransform Transform;
        
        public float2 Displacement;
        public uint Weight;

        public void Execute(in Entity otherEntity, in LocalTransform otherTransform)
        {
            float2 towards = Transform.Position.xy - otherTransform.Position.xy;
            
            float distancesq = math.lengthsq(towards);
            float radiusSum = Radius + Radius;
            if (distancesq > radiusSum * radiusSum || Entity == otherEntity)
                return;
            
            float distance = math.sqrt(distancesq);
            float penetration = radiusSum - distance;
            penetration = (penetration / distance);
            
            Displacement += towards * penetration;
            Weight++;
        }
    }
}
