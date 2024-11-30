using Unity.Burst;
using Unity.Entities;

partial struct TemplateSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        // state.RequireForUpdate<HogeHoge>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
    }
}
