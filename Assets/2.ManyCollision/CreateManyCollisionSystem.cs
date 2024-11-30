using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(InitializationSystemGroup))]
partial struct CreateManyCollisionSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ManyCollisionData>();
        state.RequireForUpdate<ManyCollision>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var data = SystemAPI.GetSingleton<ManyCollisionData>();
        
        var entities = state.EntityManager.Instantiate(data.Prefab, data.SpawnCount, Allocator.Temp);
        foreach (var entity in entities)
        {
            var localTransform = LocalTransform.FromPosition(new float3(0f, 0f, 0f));
            SystemAPI.SetComponent(entity, localTransform);
        }

        state.Enabled = false;
    }
}
