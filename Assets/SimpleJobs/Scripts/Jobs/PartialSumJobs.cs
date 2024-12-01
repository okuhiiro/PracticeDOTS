using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

namespace SimpleJobs {

  #region int

  /// <summary>
  /// Computes partial sums of a given list.
  /// </summary>
  [BurstCompile]
  public struct PartialSumJobInt : IJob {

    [ReadOnly] public NativeArray<int> Given;
    [WriteOnly] public NativeArray<int> Sums;

    public void Execute() {

      int sum = 0;

      for (int i = 0; i < Given.Length; i++) {
        sum += Given[i];
        Sums[i] = sum;
      }

    }
  }

  /// <summary>
  /// Computes partial sums of a given list.
  /// </summary>
  [BurstCompile]
  public struct PartialSumJobInt2 : IJob {

    [ReadOnly] public NativeArray<int2> Given;
    [WriteOnly] public NativeArray<int2> Sums;

    public void Execute() {

      int2 sum = 0;

      for (int i = 0; i < Given.Length; i++) {
        sum += Given[i];
        Sums[i] = sum;
      }

    }
  }

  /// <summary>
  /// Computes partial sums of a given list.
  /// </summary>
  [BurstCompile]
  public struct PartialSumJobInt3 : IJob {

    [ReadOnly] public NativeArray<int3> Given;
    [WriteOnly] public NativeArray<int3> Sums;

    public void Execute() {

      int3 sum = 0;

      for (int i = 0; i < Given.Length; i++) {
        sum += Given[i];
        Sums[i] = sum;
      }

    }
  }

  /// <summary>
  /// Computes partial sums of a given list.
  /// </summary>
  [BurstCompile]
  public struct PartialSumJobInt4 : IJob {

    [ReadOnly] public NativeArray<int4> Given;
    [WriteOnly] public NativeArray<int4> Sums;

    public void Execute() {

      int4 sum = 0;

      for (int i = 0; i < Given.Length; i++) {
        sum += Given[i];
        Sums[i] = sum;
      }

    }
  }

  #endregion


  #region float

  /// <summary>
  /// Computes partial sums of a given list.
  /// </summary>
  [BurstCompile]
  public struct PartialSumJobFloat : IJob {

    [ReadOnly] public NativeArray<float> Given;
    [WriteOnly] public NativeArray<float> Sums;

    public void Execute() {

      float sum = 0;

      for (int i = 0; i < Given.Length; i++) {
        sum += Given[i];
        Sums[i] = sum;
      }

    }
  }

  /// <summary>
  /// Computes partial sums of a given list.
  /// </summary>
  [BurstCompile]
  public struct PartialSumJobFloat2 : IJob {

    [ReadOnly] public NativeArray<float2> Given;
    [WriteOnly] public NativeArray<float2> Sums;

    public void Execute() {

      float2 sum = 0;

      for (int i = 0; i < Given.Length; i++) {
        sum += Given[i];
        Sums[i] = sum;
      }

    }
  }

  /// <summary>
  /// Computes partial sums of a given list.
  /// </summary>
  [BurstCompile]
  public struct PartialSumJobFloat3 : IJob {

    [ReadOnly] public NativeArray<float3> Given;
    [WriteOnly] public NativeArray<float3> Sums;

    public void Execute() {

      float3 sum = 0;

      for (int i = 0; i < Given.Length; i++) {
        sum += Given[i];
        Sums[i] = sum;
      }

    }
  }

  /// <summary>
  /// Computes partial sums of a given list.
  /// </summary>
  [BurstCompile]
  public struct PartialSumJobFloat4 : IJob {

    [ReadOnly] public NativeArray<float4> Given;
    [WriteOnly] public NativeArray<float4> Sums;

    public void Execute() {

      float4 sum = 0;

      for (int i = 0; i < Given.Length; i++) {
        sum += Given[i];
        Sums[i] = sum;
      }

    }
  }

  #endregion


  #region bool

  /// <summary>
  /// Computes partial counts of a given list.
  /// </summary>
  [BurstCompile]
  public struct PartialCountJobBool : IJob {

    [ReadOnly] public NativeArray<bool> Given;
    [WriteOnly] public NativeArray<int> Counts;

    public void Execute() {

      int count = 0;

      for (int i = 0; i < Given.Length; i++) {
        if (Given[i]) { count++; }
        Counts[i] = count;
      }

    }
  }

  /// <summary>
  /// Computes partial counts of a given list.
  /// </summary>
  [BurstCompile]
  public struct PartialSumJobBool2 : IJob {

    [ReadOnly] public NativeArray<bool2> Given;
    [WriteOnly] public NativeArray<int2> Counts;

    public void Execute() {

      int2 count = 0;

      for (int i = 0; i < Given.Length; i++) {
        count += new int2(Given[i]);
        Counts[i] = count;
      }

    }
  }

  /// <summary>
  /// Computes partial counts of a given list.
  /// </summary>
  [BurstCompile]
  public struct PartialCountJobBool3 : IJob {

    [ReadOnly] public NativeArray<bool3> Given;
    [WriteOnly] public NativeArray<int3> Counts;

    public void Execute() {

      int3 count = 0;

      for (int i = 0; i < Given.Length; i++) {
        count += new int3(Given[i]);
        Counts[i] = count;
      }

    }
  }

  /// <summary>
  /// Computes partial counts of a given list.
  /// </summary>
  [BurstCompile]
  public struct PartialCountJobBool4 : IJob {

    [ReadOnly] public NativeArray<bool4> Given;
    [WriteOnly] public NativeArray<int4> Counts;

    public void Execute() {

      int4 count = 0;

      for (int i = 0; i < Given.Length; i++) {
        count += new int4(Given[i]);
        Counts[i] = count;
      }

    }
  }

  #endregion





}