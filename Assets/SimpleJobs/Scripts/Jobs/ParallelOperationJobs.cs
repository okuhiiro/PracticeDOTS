using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;


// IJobParallelFor: Allows for easy multi-threading by automatically splitting up a job on one Array.
// You can think of IJobParallelFor as spreadsheet operation, like Excel or Google-Sheets: 
// It's 1 operation applied to an array of data, with another new array as the output.

// To use multi-threading with just IJob, there would need to be multiple arrays, where each job is given a different array.
// Many kinds of jobs cannot use Multi-threading.
// If the job has a single result or any object that is both read from and written to, multi-threading becomes much harder.
// Mean and Varaince calculations are examples of jobs that are much harder to multi-thread. (See BasicStatisticsJobs.cs)


namespace SimpleJobs {

  #region Add Uniform

  // Burst perfers NativeContainers over simple values
  // So, the uniform 'Value' uses a NativeArray with length=1


  [BurstCompile]
  public struct AddUniformJobFloat : IJobParallelFor {

    [ReadOnly] public NativeArray<float> Given;

    /// <summary>Only reads value at index = 0. With burst, this is faster than a a simple float</summary>
    [ReadOnly] public NativeArray<float> Value;

    [WriteOnly] public NativeArray<float> Results;

    public void Execute(int index) {
      Results[index] = Given[index] + Value[0];
    }
  }

  [BurstCompile]
  public struct AddUniformJobFloat2 : IJobParallelFor {

    [ReadOnly] public NativeArray<float2> Given;

    /// <summary>Only reads value at index = 0. With burst, this is faster than a a simple float</summary>
    [ReadOnly] public NativeArray<float2> Value;

    [WriteOnly] public NativeArray<float2> Results;

    public void Execute(int index) {
      Results[index] = Given[index] + Value[0];
    }
  }

  [BurstCompile]
  public struct AddUniformJobFloat3 : IJobParallelFor {

    [ReadOnly] public NativeArray<float3> Given;

    /// <summary>Only reads value at index = 0. With burst, this is faster than a a simple float</summary>
    [ReadOnly] public NativeArray<float3> Value;

    [WriteOnly] public NativeArray<float3> Results;

    public void Execute(int index) {
      Results[index] = Given[index] + Value[0];
    }
  }

  [BurstCompile]
  public struct AddUniformJobFloat4 : IJobParallelFor {

    [ReadOnly] public NativeArray<float4> Given;

    /// <summary>Only reads value at index = 0. With burst, this is faster than a a simple float</summary>
    [ReadOnly] public NativeArray<float4> Value;

    [WriteOnly] public NativeArray<float4> Results;

    public void Execute(int index) {
      Results[index] = Given[index] + Value[0];
    }
  }

  #endregion

  #region Add Pair
  [BurstCompile]
  public struct AddPairJobFloat : IJobParallelFor {

    /// <summary> Must be same length </summary>
    [ReadOnly] public NativeArray<float> GivenA, GivenB;

    [WriteOnly] public NativeArray<float> Results;

    public void Execute(int index) {
      Results[index] = GivenA[index] + GivenB[index];
    }
  }

  [BurstCompile]
  public struct AddPairJobFloat2 : IJobParallelFor {

    /// <summary> Must be same length </summary>
    [ReadOnly] public NativeArray<float2> GivenA, GivenB;

    [WriteOnly] public NativeArray<float2> Results;

    public void Execute(int index) {
      Results[index] = GivenA[index] + GivenB[index];
    }
  }

  [BurstCompile]
  public struct AddPairJobFloat3 : IJobParallelFor {

    /// <summary> Must be same length </summary>
    [ReadOnly] public NativeArray<float3> GivenA, GivenB;

    [WriteOnly] public NativeArray<float3> Results;

    public void Execute(int index) {
      Results[index] = GivenA[index] + GivenB[index];
    }
  }
  [BurstCompile]
  public struct AddPairJobFloat4 : IJobParallelFor {

    /// <summary> Must be same length </summary>
    [ReadOnly] public NativeArray<float4> GivenA, GivenB;

    [WriteOnly] public NativeArray<float4> Results;

    public void Execute(int index) {
      Results[index] = GivenA[index] + GivenB[index];
    }
  }
  #endregion

  #region Squares

  [BurstCompile]
  public struct SquaresJobFloat : IJobParallelFor {

    /// <summary> Must be same length </summary>
    [ReadOnly] public NativeArray<float> Given;
    [WriteOnly] public NativeArray<float> Squares;

    public void Execute(int i) {
      Squares[i] = Given[i] * Given[i];
    }

  }

  [BurstCompile]
  public struct SquaresJobFloat2 : IJobParallelFor {

    /// <summary> Must be same length </summary>
    [ReadOnly] public NativeArray<float2> Given;
    [WriteOnly] public NativeArray<float2> Squares;

    public void Execute(int i) {
      Squares[i] = Given[i] * Given[i];
    }

  }

  [BurstCompile]
  public struct SquaresJobFloat3 : IJobParallelFor {

    /// <summary> Must be same length </summary>
    [ReadOnly] public NativeArray<float3> Given;
    [WriteOnly] public NativeArray<float3> Squares;

    public void Execute(int i) {
      Squares[i] = Given[i] * Given[i];
    }

  }

  [BurstCompile]
  public struct SquaresJobFloat4 : IJobParallelFor {

    /// <summary> Must be same length </summary>
    [ReadOnly] public NativeArray<float4> Given;
    [WriteOnly] public NativeArray<float4> Squares;

    public void Execute(int i) {
      Squares[i] = Given[i] * Given[i];
    }

  }
  #endregion

}