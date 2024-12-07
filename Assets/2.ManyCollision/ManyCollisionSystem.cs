using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(TransformSystemGroup))]
partial struct ManyCollisionSystem : ISystem
{
    private const float Diameter = 0.2f;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ManyCollisionData>();
        state.RequireForUpdate<ManyCollision>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        NoJobCollision(ref state);
    }

    [BurstCompile]
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

                float3 vec = transform2.ValueRO.Position - transform1.ValueRO.Position;

                float3 dir;
                if (vec.Equals(float3.zero))
                {
                    dir = new float3(0, 1, 0);
                }
                else
                {
                    dir = math.normalizesafe(vec);
                }
                float distance = math.sqrt(math.lengthsq(vec));

                if (distance < Diameter)
                {
                    transform1.ValueRW.Position -= (Diameter - distance) * dir / 2;
                    transform2.ValueRW.Position += (Diameter - distance) * dir / 2;
                }
            }
        }
    }
}