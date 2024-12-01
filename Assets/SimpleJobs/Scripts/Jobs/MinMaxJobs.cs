using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

//New in 1.1.0: MinMaxJobs


namespace SimpleJobs {


  /// <summary>
  /// Finds the minimum value in the given array
  /// </summary>
  [BurstCompile]
  public struct MinJobFloat : IJob {

    [ReadOnly] public NativeArray<float> Given;

    public float Result;

    public void Execute() {
      Result = Given[0];

      for (int i = 1; i < Given.Length; i++) {
        if (Result > Given[i]) { Result = Given[i]; }
      }
    }

  }

  /// <summary>
  /// Finds the minimum value in the given array
  /// </summary>
  [BurstCompile]
  public struct MaxJobFloat : IJob {

    [ReadOnly] public NativeArray<float> Given;

    public float Result;

    public void Execute() {
      Result = Given[0];

      for (int i = 1; i < Given.Length; i++) {
        if (Result < Given[i]) { Result = Given[i]; }
      }
    }
  }


  /// <summary>
  /// Finds the minimum value in the given array
  /// </summary>
  [BurstCompile]
  public struct MinJobInt : IJob {

    [ReadOnly] public NativeArray<int> Given;

    public int Result;

    public void Execute() {
      Result = Given[0];

      for (int i = 1; i < Given.Length; i++) {
        if (Result > Given[i]) { Result = Given[i]; }
      }
    }

  }

  /// <summary>
  /// Finds the minimum value in the given array
  /// </summary>
  [BurstCompile]
  public struct MaxJobInt : IJob {

    [ReadOnly] public NativeArray<int> Given;

    public int Result;

    public void Execute() {
      Result = Given[0];

      for (int i = 1; i < Given.Length; i++) {
        if (Result < Given[i]) { Result = Given[i]; }
      }
    }
  }


}