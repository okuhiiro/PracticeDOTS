using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

// Check ParallelOperationJobs.cs for notes about IJobParallelFor 

namespace SimpleJobs {

  /// <summary>
  /// Writes the Value to each index where the given Checks are true.
  /// </summary>
  [BurstCompile]
  public struct IfSetJobFloat : IJobParallelFor {

    [ReadOnly] NativeArray<bool> Checks;

    /// <summary> Must be length 1. </summary>
    [ReadOnly] NativeArray<float> Value;

    /// <summary> Should be same length as Checks.</summary>
    [WriteOnly] NativeArray<float> Results;

    public void Execute(int index) {
      if (Checks[index]) { Results[index] = Value[0]; }
    }

  }

  [BurstCompile]
  public struct IfSetJobFloat2 : IJobParallelFor {

    [ReadOnly] NativeArray<bool> Checks;
    /// <summary> Must be length 1. </summary>
    [ReadOnly] NativeArray<float2> Value;

    /// <summary> Should be same length as Checks.</summary>
    [WriteOnly] NativeArray<float2> Results;

    public void Execute(int index) {
      if (Checks[index]) { Results[index] = Value[0]; }
    }

  }
  [BurstCompile]
  public struct IfSetJobFloat3 : IJobParallelFor {

    [ReadOnly] NativeArray<bool> Checks;
    /// <summary> Must be length 1. </summary>
    [ReadOnly] NativeArray<float3> Value;

    /// <summary> Should be same length as Checks.</summary>
    [WriteOnly] NativeArray<float3> Results;

    public void Execute(int index) {
      if (Checks[index]) { Results[index] = Value[0]; }
    }

  }
  [BurstCompile]
  public struct IfSetJobFloat4 : IJobParallelFor {

    [ReadOnly] NativeArray<bool> Checks;
    /// <summary> Must be length 1. </summary>
    [ReadOnly] NativeArray<float4> Value;

    /// <summary> Should be same length as Checks.</summary>
    [WriteOnly] NativeArray<float4> Results;

    public void Execute(int index) {
      if (Checks[index]) { Results[index] = Value[0]; }
    }

  }

  [BurstCompile]
  public struct IfSetJobInt : IJobParallelFor {

    [ReadOnly] NativeArray<bool> Checks;
    /// <summary> Must be length 1. </summary>
    [ReadOnly] NativeArray<int> Value;

    /// <summary> Should be same length as Checks.</summary>
    [WriteOnly] NativeArray<int> Results;

    public void Execute(int index) {
      if (Checks[index]) { Results[index] = Value[0]; }
    }

  }


  [BurstCompile]
  public struct IfSetJobInt2 : IJobParallelFor {

    [ReadOnly] NativeArray<bool> Checks;
    /// <summary> Must be length 1. </summary>
    [ReadOnly] NativeArray<int2> Value;

    /// <summary> Should be same length as Checks.</summary>
    [WriteOnly] NativeArray<int2> Results;

    public void Execute(int index) {
      if (Checks[index]) { Results[index] = Value[0]; }
    }

  }
  [BurstCompile]
  public struct IfSetJobInt3 : IJobParallelFor {

    [ReadOnly] NativeArray<bool> Checks;
    /// <summary> Must be length 1. </summary>
    [ReadOnly] NativeArray<int3> Value;

    /// <summary> Should be same length as Checks.</summary>
    [WriteOnly] NativeArray<int3> Results;

    public void Execute(int index) {
      if (Checks[index]) { Results[index] = Value[0]; }
    }

  }
  [BurstCompile]
  public struct IfSetJobInt4 : IJobParallelFor {

    [ReadOnly] NativeArray<bool> Checks;
    /// <summary> Must be length 1. </summary>
    [ReadOnly] NativeArray<int4> Value;

    /// <summary> Should be same length as Checks.</summary>
    [WriteOnly] NativeArray<int4> Results;

    public void Execute(int index) {
      if (Checks[index]) { Results[index] = Value[0]; }
    }

  }


}