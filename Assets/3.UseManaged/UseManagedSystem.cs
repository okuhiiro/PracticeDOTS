using Unity.Burst;
using Unity.Entities;
using UnityEngine;

partial struct UseManagedSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<UseManaged>();
        state.RequireForUpdate<UseManagedData>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // var spinningCubesQuery = SystemAPI.QueryBuilder().WithAll<UseManagedData>().Build();
        // Debug.Log(spinningCubesQuery.IsEmpty);
        //
        var useManagedData = SystemAPI.GetSingleton<UseManagedData>();
        var entity = SystemAPI.GetSingletonEntity<UseManagedData>();
        //
        Debug.Log($"{useManagedData} {entity}");
        
        state.Enabled = false;
    }
}
