using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

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
        var rand = Random.CreateFromIndex(1);
        foreach (var entity in entities)
        {
            var f3 = rand.NextFloat3();
            f3.z = 0;
            var localTransform = LocalTransform.FromPosition(f3);
            SystemAPI.SetComponent(entity, localTransform);
        }
        
        entities.Dispose();

        state.Enabled = false;
    }
}
