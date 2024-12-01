using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;


namespace SimpleJobs {

  /* To find the Median:
   *  Use the built-in NativeArray.Sort() method.
   *  Find the Median with array[array.length/2]
   * 
   */

  #region Mean
  [BurstCompile]
  public struct MeanJobFloat : IJob {

    [ReadOnly] public NativeArray<float> Given;

    [WriteOnly] public NativeArray<float> Result;

    public void Execute() {
      float r = 0;
      for (int i = 0; i < Given.Length; i++) {
        r += Given[i];
      }

      Result[0] = r / Given.Length;

    }

  }

  [BurstCompile]
  public struct MeanJobFloat2 : IJob {

    [ReadOnly] public NativeArray<float2> Given;

    [WriteOnly] public NativeArray<float2> Result;

    public void Execute() {
      float2 r = 0;
      for (int i = 0; i < Given.Length; i++) {
        r += Given[i];
      }

      Result[0] = r / Given.Length;

    }

  }
  [BurstCompile]
  public struct MeanJobFloat3 : IJob {

    [ReadOnly] public NativeArray<float3> Given;

    [WriteOnly] public NativeArray<float3> Result;

    public void Execute() {
      float3 r = 0;
      for (int i = 0; i < Given.Length; i++) {
        r += Given[i];
      }

      Result[0] = r / Given.Length;

    }

  }
  [BurstCompile]
  public struct MeanJobFloat4 : IJob {

    [ReadOnly] public NativeArray<float4> Given;

    [WriteOnly] public NativeArray<float4> Result;

    public void Execute() {
      float4 r = 0;
      for (int i = 0; i < Given.Length; i++) {
        r += Given[i];
      }

      Result[0] = r / Given.Length;

    }

  }


  #endregion


  #region Variance

  /// <summary>
  /// Finds the variance of a set data. To set up given data: use MeanJobFloat, AddUniformFloat, and SquaresJobFloat.
  /// </summary>
  [BurstCompile]
  public struct VarianceJobFloat : IJob {

    /// <summary>
    /// To create this array: Subtract the mean from each value, and then square each value.
    /// </summary>
    [ReadOnly] public NativeArray<float> MeanDiffSquares;

    [WriteOnly] public NativeArray<float> Result;

    [WriteOnly] public bool SampleOrPopulation;
    public bool IsSample => SampleOrPopulation;
    public bool IsPopulation => !SampleOrPopulation;

    public void Execute() {
      float sumOfSquares = 0;

      for (int i = 0; i < MeanDiffSquares.Length; i++) {
        sumOfSquares += MeanDiffSquares[i];
      }

      if (SampleOrPopulation) {
        if (MeanDiffSquares.Length > 1) {
          Result[0] = sumOfSquares / (MeanDiffSquares.Length - 1f);
        }
        else {
          Result[0] = float.NaN; 
        }
      }
      else{
        Result[0] = sumOfSquares / MeanDiffSquares.Length;
      }

    }
  }

  /// <summary>
  /// Finds the variance of a set data. To set up given data: use MeanJobFloat2, AddUniformFloat2, and SquaresJobFloat2.
  /// </summary>
  [BurstCompile]
  public struct VarianceJobFloat2 : IJob {

    /// <summary>
    /// To create this array: Subtract the mean from each value, and then square each value.
    /// </summary>
    [ReadOnly] public NativeArray<float2> MeanDiffSquares;

    [WriteOnly] public NativeArray<float2> Result;

    [WriteOnly] public bool SampleOrPopulation;
    public bool IsSample => SampleOrPopulation;
    public bool IsPopulation => !SampleOrPopulation;

    public void Execute() {
      float2 sumOfSquares = 0;

      for (int i = 0; i < MeanDiffSquares.Length; i++) {
        sumOfSquares += MeanDiffSquares[i];
      }

      if (SampleOrPopulation) {
        if (MeanDiffSquares.Length > 1) {
          Result[0] = sumOfSquares / (MeanDiffSquares.Length - 1f);
        }
        else {
          Result[0] = float.NaN;
        }
      }
      else {
        Result[0] = sumOfSquares / MeanDiffSquares.Length;
      }

    }
  }

  /// <summary>
  /// Finds the variance of a set data. To set up given data: use MeanJobFloat3, AddUniformFloat3, and SquaresJobFloat3.
  /// </summary>
  [BurstCompile]
  public struct VarianceJobFloat3 : IJob {

    /// <summary>
    /// To create this array: Subtract the mean from each value, and then square each value.
    /// </summary>
    [ReadOnly] public NativeArray<float3> MeanDiffSquares;

    [WriteOnly] public NativeArray<float3> Result;

    [WriteOnly] public bool SampleOrPopulation;
    public bool IsSample => SampleOrPopulation;
    public bool IsPopulation => !SampleOrPopulation;

    public void Execute() {
      float3 sumOfSquares = 0;

      for (int i = 0; i < MeanDiffSquares.Length; i++) {
        sumOfSquares += MeanDiffSquares[i];
      }

      if (SampleOrPopulation) {
        if (MeanDiffSquares.Length > 1) {
          Result[0] = sumOfSquares / (MeanDiffSquares.Length - 1f);
        }
        else {
          Result[0] = float.NaN;
        }
      }
      else {
        Result[0] = sumOfSquares / MeanDiffSquares.Length;
      }

    }
  }

  /// <summary>
  /// Finds the variance of a set data. To set up given data: use MeanJobFloat4, AddUniformFloat4, and SquaresJobFloat4.
  /// </summary>
  [BurstCompile]
  public struct VarianceJobFloat4 : IJob {

    /// <summary>
    /// To create this array: Subtract the mean from each value, and then square each value.
    /// </summary>
    [ReadOnly] public NativeArray<float4> MeanDiffSquares;

    [WriteOnly] public NativeArray<float4> Result;

    [WriteOnly] public bool SampleOrPopulation;
    public bool IsSample => SampleOrPopulation;
    public bool IsPopulation => !SampleOrPopulation;

    public void Execute() {
      float4 sumOfSquares = 0;

      for (int i = 0; i < MeanDiffSquares.Length; i++) {
        sumOfSquares += MeanDiffSquares[i];
      }

      if (SampleOrPopulation) {
        if (MeanDiffSquares.Length > 1) {
          Result[0] = sumOfSquares / (MeanDiffSquares.Length - 1f);
        }
        else {
          Result[0] = float.NaN;
        }
      }
      else {
        Result[0] = sumOfSquares / MeanDiffSquares.Length;
      }

    }
  }

  #endregion


  #region Covariance

  /// <summary>
  /// Finds the covariance of a set data. To set up given data sets: use MeanJobFloat, and AddUniformFloat.
  /// </summary>
  [BurstCompile]
  public struct CovarianceJobFloat : IJob {

    /// <summary>
    /// To create these arrays: Subtract the mean from each value.
    /// </summary>
    [ReadOnly] public NativeArray<float> MeanDiffA, MeanDiffB;

    [WriteOnly] public NativeArray<float> Result;

    [WriteOnly] public bool SampleOrPopulation;
    public bool IsSample => SampleOrPopulation;
    public bool IsPopulation => !SampleOrPopulation;

    public void Execute() {
      float sumOfSquares = 0;

      for (int i = 0; i < MeanDiffA.Length; i++) {
        sumOfSquares += MeanDiffA[i] * MeanDiffB[i];
      }

      if (SampleOrPopulation) {
        if (MeanDiffA.Length > 1) {
          Result[0] = sumOfSquares / (MeanDiffA.Length - 1f);
        }
        else {
          Result[0] = float.NaN;
        }
      }
      else {
        Result[0] = sumOfSquares / MeanDiffA.Length;
      }

    }
  }





  #endregion




}