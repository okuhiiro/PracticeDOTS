using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(TransformSystemGroup))]
partial struct ManyCollisionSystem : ISystem
{
    private const float Radius = 0.1f;

    private float3 CellSize;
    
    NativeList<Entity> m_Entities;
    NativeList<LocalTransform> m_Transforms;
    NativeParallelMultiHashMap<int3, int> m_Map;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ManyCollisionData>();
        state.RequireForUpdate<ManyCollision>();

        CellSize = new float3(1.5f, 1.5f, 0f);
        
        m_Entities = new NativeList<Entity>(5000, Allocator.Persistent);
        m_Transforms = new NativeList<LocalTransform>(5000, Allocator.Persistent);
        m_Map = new NativeParallelMultiHashMap<int3, int>(math.ceilpow2(10000), Allocator.Persistent);
        
        state.EntityManager.AddComponentData(state.SystemHandle, new Singleton
        {
            m_Map = m_Map,
            m_Entities = m_Entities,
            m_Transforms = m_Transforms,
            
            CellSize = CellSize
        });
    }

    private struct Singleton : IComponentData
    {
        internal NativeParallelMultiHashMap<int3, int> m_Map;
        internal NativeList<Entity> m_Entities;
        internal NativeList<LocalTransform> m_Transforms;

        internal float3 CellSize;
        
        public void QueryCircleAll(ref MyCollision action)
        {
            var length = m_Entities.Length;
            for (int i = 0; i < length; i++)
            {
                action.Execute(m_Entities[i], m_Transforms[i]);
            }
        }

        public void QueryCircleMap(float3 center, ref MyCollision action)
        {
            int2 min = (int2)math.round((center.xy - Radius) / CellSize.xy);
            int2 max = (int2)math.round((center.xy + Radius) / CellSize.xy) + 1;
            // int k = (int)math.round(center.z / CellSize.z);
        
            for (int i = min.x; i < max.x; ++i)
            {
                for (int j = min.y; j < max.y; ++j)
                {
                    if (m_Map.TryGetFirstValue(new int3(i, j, 0), out int index, out var iterator))
                    {
                        do
                        {
                            action.Execute(m_Entities[index], m_Transforms[index]);
                        } while (m_Map.TryGetNextValue(out index, ref iterator));
                    }
                }
            }
        }
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var data = SystemAPI.GetSingleton<ManyCollisionData>();
        
        switch (data.Mode)
        {
            case DOTSMode.AllBustCompiler:
                for (int iteration = 0; iteration < 1; ++iteration)
                {
                    AllBustCompillerCollision(ref state);
                }
                break;
            
            case DOTSMode.AllBustCompilerJob:
                for (int iteration = 0; iteration < 1; ++iteration)
                {
                    AllBustCompillerJobCollision(ref state);
                }
                break;
            
            case DOTSMode.Spatial:
                SpatialJobCollision(ref state);
                break;
        }
    }
        
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        m_Entities.Dispose();
        m_Transforms.Dispose();
        m_Map.Dispose();
    }

    #region All_BustCompiller
    [BurstCompile]
    private void AllBustCompillerCollision(ref SystemState state)
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

                float penetration;
                if (distance < 0.0001f)
                {
                    penetration = 0.01f;
                }
                else
                {
                    penetration = radiusSum - distance;
                    penetration = (penetration / distance);
                }
                
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
    #endregion
    
    #region All_BustCompiller_Job
    [BurstCompile]
    private void AllBustCompillerJobCollision(ref SystemState state)
    {
        m_Entities.Clear();
        m_Transforms.Clear();
        m_Map.Clear();
        
        CopyJob job1 = new CopyJob
        {
            Entities = m_Entities,
            Transforms = m_Transforms,
        };
        JobHandle job1Handle = job1.Schedule(state.Dependency);
        job1Handle.Complete();
        
        var spatial = SystemAPI.GetSingletonRW<Singleton>();
        HashCollisionJob job2 = new HashCollisionJob
        {
            Spatial = spatial.ValueRO,
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
        [ReadOnly] public Singleton Spatial;
        
        void Execute(in Entity entity, ref LocalTransform transform)
        {
            var action = new MyCollision
            {
                Entity = entity,
                Transform = transform,
            };
            
            Spatial.QueryCircleAll(ref action);
            
            if (action.Weight > 0)
            {
                action.Displacement /= action.Weight;
                transform.Position += new float3(action.Displacement, 0);
            }
        }
    }
    #endregion
    
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
            float penetration;
            if (distance < 0.0001f)
            {
                penetration = 0.01f;
            }
            else
            {
                penetration = radiusSum - distance;
                penetration = (penetration / distance);
            }
            
            Displacement += towards * penetration;
            Weight++;
        }
    }

    #region Spatial
    [BurstCompile]
    private void SpatialJobCollision(ref SystemState state)
    {
        m_Entities.Clear();
        m_Transforms.Clear();
        m_Map.Clear();
        
        CopyJob job1 = new CopyJob
        {
            Entities = m_Entities,
            Transforms = m_Transforms,
        };
        JobHandle job1Handle = job1.Schedule(state.Dependency);
        
        CreateMap job2 = new CreateMap
        {
            Map = m_Map.AsParallelWriter(),
            CellSize = CellSize,
        };
        var job2Handle = job2.ScheduleParallel(state.Dependency);
        
        var combineJob = JobHandle.CombineDependencies(job1Handle, job2Handle);
        combineJob.Complete();
        
        var spatial = SystemAPI.GetSingletonRW<Singleton>();
        var job3 = new MapCollisionJob
        {
            Spatial = spatial.ValueRO,
        };
        state.Dependency = job3.ScheduleParallel(state.Dependency);
    }
    
    [BurstCompile]
    partial struct CreateMap : IJobEntity
    {
        [WriteOnly]
        public NativeParallelMultiHashMap<int3, int>.ParallelWriter Map;
        public float3 CellSize;
    
        void Execute([EntityIndexInQuery] int entityInQueryIndex, in LocalTransform transform)
        {
            // int3 cell = (int3)math.round((transform.Position) / CellSize);
            // Map.Add(cell, entityInQueryIndex);
            
            float3 center = transform.Position;
            int2 min = (int2) math.round((center.xy - Radius) / CellSize.xy);
            int2 max = (int2) math.round((center.xy + Radius) / CellSize.xy) + 1;
            //int k = (int) math.round(center.z / CellSize.z);
            for (int i = min.x; i < max.x; ++i)
            {
                for (int j = min.y; j < max.y; ++j)
                {
                    Map.Add(new int3(i, j, 0), entityInQueryIndex);
                }
            }
        }
    }
    
    [BurstCompile]
    partial struct MapCollisionJob : IJobEntity
    {
        [ReadOnly] public Singleton Spatial;
        
        void Execute(Entity entity, ref LocalTransform transform)
        {
            var action = new MyCollision
            {
                Entity = entity,
                Transform = transform,
            };
            
            Spatial.QueryCircleMap(transform.Position, ref action);
            
            if (action.Weight > 0)
            {
                action.Displacement /= action.Weight;
                transform.Position += new float3(action.Displacement, 0);
            }
        }
    }
    #endregion
}