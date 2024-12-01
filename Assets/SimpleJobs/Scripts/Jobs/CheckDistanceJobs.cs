using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

namespace SimpleJobs {

  #region Spheres, return index list
  /// <summary>
  /// Creates a list of indecies (int), where each index corresponds to a given sphere. 
  /// Brute Force : Used for performance comparisons.
  /// </summary>
  [BurstCompile]
  public struct CheckSpheresDistanceJobIndexList : IJob {
    
    [ReadOnly] public NativeArray<float3> Positions;
    [ReadOnly] public NativeArray<float> Radii;

    [ReadOnly] public float3 CheckCenter;
    [ReadOnly] public float CheckRadius;

    [WriteOnly] public NativeList<int> FoundPointers;

    public void Execute() {

      FoundPointers = new NativeList<int>(Positions.Length, Allocator.TempJob);

      if(Positions.Length != Radii.Length) { return; }

      for (int i = 0; i < Positions.Length; i++) {

        if(math.distance(Positions[i], CheckCenter) > Radii[i] + CheckRadius) { continue; }

        FoundPointers.Add(i);

      }


    }
  }


  /// <summary>
  /// Creates a list of indecies (int), where each index corresponds to a given sphere. 
  /// Faster than non-uniform, uses square magnitudes and does not need square roots.
  /// Brute Force : Used for performance comparisons.
  /// </summary>
  [BurstCompile]
  public struct CheckUniformSpheresDistanceJobIndexList : IJob {

    [ReadOnly] public NativeArray<float3> Positions;
    //[ReadOnly] public float SpheresRadius;

    [ReadOnly] public float3 CheckCenter;

    /// <summary>
    /// Radius of the check sphere + Radius of the spheres.
    /// Uniform: given spheres are all the same radius.
    /// </summary>
    [ReadOnly] public float CheckRadius;

    [WriteOnly] public NativeList<int> FoundPointers;

    public void Execute() {

      FoundPointers = new NativeList<int>(Positions.Length, Allocator.TempJob);

      //float sqrCheckDist = (SpheresRadius * SpheresRadius) + (CheckRadius * CheckRadius);
      float sqrCheckDist = CheckRadius * CheckRadius;

      for (int i = 0; i < Positions.Length; i++) {

        if (math.distancesq(Positions[i], CheckCenter) > sqrCheckDist) { continue; }

        FoundPointers.Add(i);

      }


    }
  }

  #endregion

  #region Spheres, trim array
  /// <summary>
  /// Takes the given arrays, and creates new ones were all of the spheres are within the checked distance. 
  /// Brute Force : Used for performance comparisons.
  /// </summary>
  [BurstCompile]
  public struct CheckSpheresDistanceJobTrim : IJob {

    [ReadOnly] public NativeArray<float3> Positions;
    [ReadOnly] public NativeArray<float> Radii;

    [ReadOnly] public float3 CheckCenter;
    [ReadOnly] public float CheckRadius;

    [WriteOnly] public NativeArray<float3> TrimmedPositions;
    [WriteOnly] public NativeArray<float> TrimmedRadii;

    public void Execute() {

      TrimmedPositions = new NativeArray<float3>(Positions.Length, Allocator.TempJob);
      TrimmedRadii = new NativeArray<float>(Radii, Allocator.TempJob);

      if (Positions.Length != Radii.Length) { return; }

      int j = 0;

      for (int i = 0; i < Positions.Length; i++) {

        if (math.distance(Positions[i], CheckCenter) > Radii[i] + CheckRadius) { continue; }
        TrimmedPositions[j] = TrimmedPositions[i];
        TrimmedRadii[j] = TrimmedRadii[i];

      }

      TrimmedPositions.Slice(0, j);
      TrimmedRadii.Slice(0, j);


    }
  }

  /// <summary>
  /// Takes the given arrays, and creates new ones were all of the spheres are within the checked distance. 
  /// Faster than non-uniform, uses square magnitudes and does not need square roots.
  /// Brute Force : Used for performance comparisons.
  /// </summary>
  [BurstCompile]
  public struct CheckUniformSpheresDistanceJobTrim : IJob {

    [ReadOnly] public NativeArray<float3> Positions;
    //[ReadOnly] public float SpheresRadius;

    [ReadOnly] public float3 CheckCenter;

    /// <summary>
    /// Radius of the check sphere + Radius of the spheres.
    /// Uniform: given spheres are all the same radius.
    /// </summary>
    [ReadOnly] public float CheckRadius;

    [WriteOnly] public NativeArray<float3> TrimmedPositions;

    public void Execute() {

      TrimmedPositions = new NativeArray<float3>(Positions.Length, Allocator.TempJob);

      //float sqrCheckDist = (SpheresRadius * SpheresRadius) + (CheckRadius * CheckRadius);
      float sqrCheckDist = CheckRadius * CheckRadius;

      int j = 0;

      for (int i = 0; i < Positions.Length; i++) {

        if (math.distancesq(Positions[i], CheckCenter) > sqrCheckDist) { continue; }
        TrimmedPositions[j] = TrimmedPositions[i];
        j++;
      }

      TrimmedPositions.Slice(0, j);


    }
  }

  #endregion


  #region AABBs, return index list
  /// <summary>
  /// Creates a list of indecies (int), where each index corresponds to a given axis-aligned bounding box. 
  /// Brute Force : Used for performance comparisons.
  /// </summary>
  [BurstCompile]
  public struct CheckAABBsDistanceJobIndexList : IJob {

    [ReadOnly] public NativeArray<float3> Positions;
    [ReadOnly] public NativeArray<float3> Sizes;

    [ReadOnly] public float3 CheckCenter;
    [ReadOnly] public float3 CheckSize;

    [WriteOnly] public NativeList<int> FoundPointers;

    public void Execute() {

      FoundPointers = new NativeList<int>(Positions.Length, Allocator.TempJob);

      for (int i = 0; i < Positions.Length; i++) {

        float3 delta = Positions[i] - CheckCenter;

        if (math.all(math.abs(delta) > (CheckSize + Sizes[i]))) { continue; }

        FoundPointers.Add(i);

      }
    }

  }


  /// <summary>
  /// Creates a list of indecies (int), where each index corresponds to a given axis-aligned bounding box. 
  /// Faster than non-uniform.
  /// Brute Force : Used for performance comparisons.
  /// </summary>
  [BurstCompile]
  public struct CheckUniformAABBsDistanceJobIndexList : IJob {

    [ReadOnly] public NativeArray<float3> Positions;
    //[ReadOnly] public float3 AABBSize;

    [ReadOnly] public float3 CheckCenter;

    /// <summary>
    /// Size of the check box + Size of the boxes.
    /// Uniform: given boxes are all the same size.
    /// </summary>
    [ReadOnly] public float3 CheckSize;

    [WriteOnly] public NativeList<int> FoundPointers;

    public void Execute() {
      
      FoundPointers = new NativeList<int>(Positions.Length, Allocator.TempJob);

      for (int i = 0; i < Positions.Length; i++) {

        float3 delta = Positions[i] - CheckCenter;

        if ( math.all( math.abs(delta) > CheckSize  ) ) { continue; }

        FoundPointers.Add(i);

      }
    }

  }

  #endregion


  #region AABBs, trim array

  /// <summary>
  /// Takes the given arrays, and creates new ones were all of the AABB are within the checked box. 
  /// Brute Force : Used for performance comparisons.
  /// </summary>
  [BurstCompile]
  public struct CheckAABBsDistanceJobTrim : IJob {

    [ReadOnly] public NativeArray<float3> Positions;
    [ReadOnly] public NativeArray<float3> Sizes;

    [ReadOnly] public float3 CheckCenter;
    [ReadOnly] public float3 CheckSize;

    [WriteOnly] public NativeArray<float3> TrimmedPositions;
    [WriteOnly] public NativeArray<float3> TrimmedSizes;

    public void Execute() {

      int j = 0;

      TrimmedPositions = new NativeArray<float3>(Positions.Length, Allocator.TempJob);
      TrimmedSizes = new NativeArray<float3>(Sizes.Length, Allocator.TempJob);

      for (int i = 0; i < Positions.Length; i++) {

        float3 delta = Positions[i] - CheckCenter;

        if (math.all(math.abs(delta) > (CheckSize + Sizes[i]))) { continue; }

        TrimmedPositions[j] = Positions[i];
        TrimmedSizes[j] = Sizes[i];
        j++; 

      }

      TrimmedPositions.Slice(0, j);
      TrimmedSizes.Slice(0, j);

    }



  }


  /// <summary>
  /// Takes the given arrays, and creates new ones were all of the AABB are within the checked box. 
  /// Faster than non-uniform.
  /// Brute Force : Used for performance comparisons.
  /// </summary>
  [BurstCompile]
  public struct CheckUniformAABBsDistanceTrim : IJob {

    [ReadOnly] public NativeArray<float3> Positions;
    //[ReadOnly] public float3 AABBSize;

    [ReadOnly] public float3 CheckCenter;

    /// <summary>
    /// Size of the check box + Size of the boxes.
    /// Uniform: given boxes are all the same size.
    /// </summary>
    [ReadOnly] public float3 CheckSize;

    [WriteOnly] public NativeArray<float3> TrimmedPositions;

    public void Execute() {

      TrimmedPositions = new NativeArray<float3>(Positions.Length, Allocator.TempJob);

      int j = 0;

      for (int i = 0; i < Positions.Length; i++) {

        float3 delta = Positions[i] - CheckCenter;

        if (math.all(math.abs(delta) > CheckSize)) { continue; }

        TrimmedPositions[j] = Positions[i];
        j++;

      }

      TrimmedPositions.Slice(0, j);

    }

  }



  #endregion

  #region AABBs, ParallelFor


  /// <summary>
  /// Creates a new bool array, where each value is true if the corresponding box is within the check box.
  /// Faster than non-uniform.
  /// Brute Force : Used for performance comparisons.
  /// </summary>
  [BurstCompile]
  public struct CheckAABBsDistanceJobBool : IJobParallelFor {

    [ReadOnly] public NativeArray<float3> Positions;
    [ReadOnly] public NativeArray<float3> Sizes;

    [ReadOnly] public float3 CheckCenter;
    [ReadOnly] public float3 CheckSize;

    [WriteOnly] public NativeArray<bool> Results;

    public void Execute(int i) {
      float3 delta = Positions[i] - CheckCenter;

      Results[i] = math.all(math.abs(delta) > (Sizes[i] + CheckSize));
    }

  }

  /// <summary>
  /// Creates a new bool array, where each value is true if the corresponding box is within the check box.
  /// Faster than non-uniform.
  /// Brute Force : Used for performance comparisons.
  /// </summary>
  [BurstCompile]
  public struct CheckAABBsDistanceJobBool3 : IJobParallelFor {

    [ReadOnly] public NativeArray<float3> Positions;
    [ReadOnly] public NativeArray<float3> Sizes;

    [ReadOnly] public float3 CheckCenter;
    [ReadOnly] public float3 CheckSize;

    [WriteOnly] public NativeArray<bool3> Results;

    public void Execute(int i) {
      Results[i] = math.abs(Positions[i] - CheckCenter) > (Sizes[i] + CheckSize);
    }

  }


  /// <summary>
  /// Creates a new bool array, where each value is true if the corresponding box is within the check box.
  /// Faster than non-uniform.
  /// Brute Force : Used for performance comparisons.
  /// </summary>
  [BurstCompile]
  public struct CheckUniformAABBsDistanceJobBool : IJobParallelFor {

    [ReadOnly] public NativeArray<float3> Positions;

    [ReadOnly] public float3 CheckCenter;
    [ReadOnly] public float3 CheckSize;

    [WriteOnly] public NativeArray<bool> Results;

    public void Execute(int i) {
      float3 delta = Positions[i] - CheckCenter;

      Results[i] = math.all(math.abs(delta) > CheckSize);
    }

  }

  /// <summary>
  /// Creates a new bool array, where each value is true if the corresponding box is within the check box.
  /// Faster than non-uniform.
  /// Brute Force : Used for performance comparisons.
  /// </summary>
  [BurstCompile]
  public struct CheckUniformAABBsDistanceJobBool3 : IJobParallelFor {

    [ReadOnly] public NativeArray<float3> Positions;

    [ReadOnly] public float3 CheckCenter;
    [ReadOnly] public float3 CheckSize;

    [WriteOnly] public NativeArray<bool3> Results;

    public void Execute(int i) {
      Results[i] = math.abs(Positions[i] - CheckCenter) > CheckSize;
    }

  }
  #endregion

}