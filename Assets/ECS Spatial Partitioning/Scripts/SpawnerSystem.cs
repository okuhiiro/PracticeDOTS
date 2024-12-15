using UnityEngine;

namespace ECS_Spatial_Partitioning
{
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Transforms;
    using Unity.Burst;
    using Unity.Mathematics;
    using Random = Unity.Mathematics.Random;
    
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct SpawnerSystem : ISystem
    {
        private Random _random;


        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Spawner>();
            _random = new Random(1234567890);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false;
            Spawn(ref state);
        }

        [BurstCompile]
        private void Spawn(ref SystemState state)
        {
            var spawnerEntity = SystemAPI.GetSingletonEntity<Spawner>();
            var spawner = SystemAPI.GetComponentRO<Spawner>(spawnerEntity);
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            for (var i = 0; i < spawner.ValueRO.SpawnAmount; i++)
            {
                var newAgent = ecb.Instantiate(spawner.ValueRO.Prefab);
                const float areaSizeValue = 15000f; //assumes a square playable area with the center at 0,0
                var randomPosition = new float3(
                    _random.NextFloat(-areaSizeValue, areaSizeValue),
                    _random.NextFloat(-areaSizeValue, areaSizeValue),
                    -1f);
                ecb.SetComponent(newAgent, LocalTransform.FromPosition(randomPosition));
            }

            ecb.Playback(state.EntityManager);
        }
    }
}