namespace ECS_Spatial_Partitioning
{
    using Tags;
    using Unity.Burst;
    using Unity.Burst.Intrinsics;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Rendering;
    
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [BurstCompile]
    public partial struct SwitchRendererWhenOffScreenSystem : ISystem
    {
        private ComponentType _disableRenderingCompType;
        private ComponentType _offScreenDetectionCompType;
        private ComponentLookup<DisableRendering> _disableRenderingLookup;
        private EntityTypeHandle _entityHandle;

        public void OnCreate(ref SystemState state)
        {
            _disableRenderingCompType = typeof(DisableRendering);
            _offScreenDetectionCompType = typeof(OffScreen);
            _disableRenderingLookup = state.GetComponentLookup<DisableRendering>();
            _entityHandle = state.GetEntityTypeHandle();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var dependency = state.Dependency;

            _disableRenderingLookup.Update(ref state);
            _entityHandle.Update(ref state);

            // We use two queries here because the build-in DisableRendering component is not an
            // IEnableableComponent and because we don't want to perform a HasComponent lookup within the job

            var queryDisableThis = SystemAPI.QueryBuilder()
                .WithAll<RendererCanBeDisabled, OffScreen>()
                .WithNone<DisableRendering>()
                .Build();
            queryDisableThis.SetChangedVersionFilter(_offScreenDetectionCompType);

            var queryEnableThis = SystemAPI.QueryBuilder()
                .WithAll<RendererCanBeDisabled, DisableRendering>()
                .WithNone<OffScreen>()
                .Build();

            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            var enableJob = new SetDisableRenderingJob
            {
                DisableRenderingCompType = _disableRenderingCompType,
                DisableRenderer = false,
                EntityHandle = _entityHandle,
                Ecb = ecb
            }.Schedule(queryDisableThis, dependency);
            dependency = enableJob;

            var disableJob = new SetDisableRenderingJob
            {
                DisableRenderingCompType = _disableRenderingCompType,
                DisableRenderer = true,
                EntityHandle = _entityHandle,
                Ecb = ecb
            }.Schedule(queryEnableThis, dependency);

            disableJob.Complete();
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
            state.Dependency = dependency;
        }

        [BurstCompile]
        private struct SetDisableRenderingJob : IJobChunk
        {
            [ReadOnly] public ComponentType DisableRenderingCompType;
            [ReadOnly] public bool DisableRenderer;
            [ReadOnly] public EntityTypeHandle EntityHandle;
            public EntityCommandBuffer Ecb;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask,
                in v128 chunkEnabledMask)
            {
                var entities = chunk.GetNativeArray(EntityHandle);

                for (var i = 0; i < chunk.Count; i++)
                    if (DisableRenderer)
                        Ecb.RemoveComponent(entities[i], DisableRenderingCompType);
                    else
                        Ecb.AddComponent(entities[i], DisableRenderingCompType);
            }
        }
    }
}