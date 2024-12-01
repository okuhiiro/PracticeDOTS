using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace SimpleJobs.Demos {

  /// <summary>
  /// Demonstration of how to use Mean, Variance, and Covariance jobs.
  /// </summary>
  [ExecuteInEditMode]
  public class StatisticsDemo : MonoBehaviour {

    [Header("Use the 'Bttn' buttons to calculate the desired values")]
    public float[] ValuesA;
    public float[] ValuesB;

    public float MeanA, VarianceA, MeanB, VarianceB, Covariance;

    [Tooltip("Sample Variance? or if not, Population Variance")]
    public bool SampleVarianceOrPop;

    public bool FindVarianceBttn, FindCovarianceBttn;

    // New in 1.1.0: Min & Max
    public float MinA, MaxA, MinB, MaxB;
    public bool FindMinMaxBttn;

    private void Update() {

      if (FindVarianceBttn) {
        FindVarianceBttn = false;
        FindMeanAndVariance(ValuesA, ref MeanA, ref VarianceA, SampleVarianceOrPop);
        FindMeanAndVariance(ValuesB, ref MeanB, ref VarianceB, SampleVarianceOrPop);
      }
      if (FindCovarianceBttn) {
        FindCovarianceBttn = false;
        FindMeansAndCovariance(ValuesA, ValuesB, ref MeanA, ref MeanB, ref Covariance, SampleVarianceOrPop);
      }
      if (FindMinMaxBttn) {
        FindMinMaxBttn = false;
        FindMinMax(ValuesA, ref MinA, ref MaxA);
        FindMinMax(ValuesB, ref MinB, ref MaxB);
      }
    }

    public static void FindMeanAndVariance(float[] values, ref float mean, ref float variance, bool sampleVariance) {
      if (values.Length < 1) { return; }

      NativeArray<float> vs = new NativeArray<float>(values, Allocator.TempJob);
      NativeArray<float> diffs = new NativeArray<float>(values.Length, Allocator.TempJob);
      NativeArray<float> sqrs = new NativeArray<float>(values.Length, Allocator.TempJob);

      NativeArray<float> meanResult = new NativeArray<float>(1, Allocator.TempJob);
      NativeArray<float> varianceResult = new NativeArray<float>(1, Allocator.TempJob);

      MeanJobFloat meanJob = new MeanJobFloat { Given = vs, Result = meanResult };
      JobHandle meanHandle = meanJob.Schedule();
      meanHandle.Complete();
      mean = meanResult[0];

      meanResult[0] = -mean;

      AddUniformJobFloat diffJob = new AddUniformJobFloat { Given = vs, Value = meanResult, Results = diffs };
      JobHandle diffsHandle = diffJob.Schedule(vs.Length, 64, meanHandle);

      SquaresJobFloat squaresJob = new SquaresJobFloat { Given = diffs, Squares = sqrs };
      JobHandle sqrsHandle = squaresJob.Schedule(vs.Length, 64, diffsHandle);

      VarianceJobFloat varainceJob = new VarianceJobFloat { MeanDiffSquares = sqrs, SampleOrPopulation = sampleVariance, Result = varianceResult };
      JobHandle varianceHandle = varainceJob.Schedule(sqrsHandle);

      varianceHandle.Complete();
      variance = varainceJob.Result[0];

      vs.Dispose();
      diffs.Dispose();
      sqrs.Dispose();
      meanResult.Dispose();
      varianceResult.Dispose();

    }

    public static void FindMeansAndCovariance(float[] valuesA, float[] valuesB, ref float meanA, ref float meanB, ref float covariance, bool sampleCovariance) {
      if (valuesA.Length < 1) { return; }
      if(valuesA.Length != valuesB.Length) { Debug.LogError("Arrays must be the same length."); return; }

      NativeArray<float> vsA = new NativeArray<float>(valuesA, Allocator.TempJob);
      NativeArray<float> vsB = new NativeArray<float>(valuesB, Allocator.TempJob);
      NativeArray<float> diffsA = new NativeArray<float>(valuesA.Length, Allocator.TempJob);
      NativeArray<float> diffsB = new NativeArray<float>(valuesB.Length, Allocator.TempJob);

      NativeArray<float> meanAResult = new NativeArray<float>(1, Allocator.TempJob);
      NativeArray<float> meanBResult = new NativeArray<float>(1, Allocator.TempJob);
      NativeArray<float> covarianceResult = new NativeArray<float>(1, Allocator.TempJob);

      MeanJobFloat meanAJob = new MeanJobFloat { Given = vsA, Result = meanAResult };
      JobHandle meanAHandle = meanAJob.Schedule();

      MeanJobFloat meanBJob = new MeanJobFloat { Given = vsB, Result = meanBResult };
      JobHandle meanBHandle = meanBJob.Schedule();

      meanAHandle.Complete();
      meanBHandle.Complete();

      meanA = meanAResult[0];
      meanB = meanBResult[0];

      meanAResult[0] = -meanA;
      meanBResult[0] = -meanB;

      AddUniformJobFloat diffAJob = new AddUniformJobFloat { Given = vsA, Value = meanAResult, Results = diffsA };
      JobHandle diffsAHandle = diffAJob.Schedule(vsA.Length, 64, meanAHandle);

      AddUniformJobFloat diffBJob = new AddUniformJobFloat { Given = vsB, Value = meanBResult, Results = diffsB };
      JobHandle diffsBHandle = diffBJob.Schedule(vsB.Length, 64, meanBHandle);

      CovarianceJobFloat covarianceJob = new CovarianceJobFloat { MeanDiffA = diffsA, MeanDiffB = diffsB, Result = covarianceResult , SampleOrPopulation = sampleCovariance};
      JobHandle covarianceHandle = covarianceJob.Schedule(JobHandle.CombineDependencies(diffsAHandle, diffsBHandle));

      covarianceHandle.Complete();
      covariance = covarianceJob.Result[0];

      vsA.Dispose();
      vsB.Dispose();
      diffsA.Dispose();
      diffsB.Dispose();
      meanAResult.Dispose();
      meanBResult.Dispose();
      covarianceResult.Dispose();

    }

    //New in 1.1.0: Find MinMax
    public static void FindMinMax(float[] values, ref float min, ref float max) {
      if (values.Length < 1) { return; }

      NativeArray<float> vs = new NativeArray<float>(values, Allocator.TempJob);

      MinJobFloat minJob = new MinJobFloat { Given = vs };
      MaxJobFloat maxJob = new MaxJobFloat { Given = vs };

      JobHandle minHandle = minJob.Schedule();
      JobHandle maxHandle = maxJob.Schedule();

      minHandle.Complete();
      maxHandle.Complete();

      min = minJob.Result;
      max = maxJob.Result;

      vs.Dispose();
    }

    public static void FindMinMax(int[] values, ref int min, ref int max) {
      if (values.Length < 1) { return; }

      NativeArray<int> vs = new NativeArray<int>(values, Allocator.TempJob);

      MinJobInt minJob = new MinJobInt { Given = vs };
      MaxJobInt maxJob = new MaxJobInt { Given = vs };

      JobHandle minHandle = minJob.Schedule();
      JobHandle maxHandle = maxJob.Schedule();

      minHandle.Complete();
      maxHandle.Complete();

      min = minJob.Result;
      max = maxJob.Result;

      vs.Dispose();
    }



    // New in 1.1.0: OnDrawGizmos()
    private void OnDrawGizmos() {

      float z = Mathf.Sqrt(VarianceA + VarianceB);

      float s = Mathf.Sqrt(VarianceA) / 10f;
      Gizmos.color = Color.red;
      foreach (float f in ValuesA) {
        Gizmos.DrawSphere(new Vector3(0, f, 0), s);
      }
      Gizmos.color = new Color(1, .4f, .4f);
      Gizmos.DrawSphere(new Vector3(z / 2, MeanA, 0), s);

      s = Mathf.Sqrt(VarianceB) / 10f;
      Gizmos.color = Color.blue;
      foreach (float f in ValuesB) {
        Gizmos.DrawSphere(new Vector3(0, f, z), s);
      }
      Gizmos.color = new Color(.4f, .4f, 1f);
      Gizmos.DrawSphere(new Vector3(z / 2, MeanB, z), s);




      Gizmos.color = Color.white;
      Vector3 border = new Vector3(0, Mathf.Max(MaxB, MaxA) - Mathf.Min(MinA, MinB), z);
      Gizmos.DrawWireCube(border/2, border);
    
    }

  }

}