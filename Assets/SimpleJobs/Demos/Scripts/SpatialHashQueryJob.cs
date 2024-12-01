using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

using Unity.Collections.LowLevel.Unsafe;

namespace SimpleJobs.Demos {

  /// <summary>
  /// Finds a list of pointers (int index) to objects that are in the queried cells.
  /// </summary>
  public struct SpatialHashQueryIndiciesJob : IJob {

    [ReadOnly] public NativeArray<int> QueriedIndicies;
    public int InvalidCellValue;
    [ReadOnly] public NativeArray<int> SortedPointers;
    [ReadOnly] public NativeArray<int> Sums;


    [WriteOnly] public NativeList<int> FoundPointers;

    public void Execute() {

      FoundPointers = new NativeList<int>(SortedPointers.Length, Allocator.TempJob);

      for (int i = 0; i < QueriedIndicies.Length; i++) {
        int hashIndex = QueriedIndicies[i];
        if(hashIndex == InvalidCellValue) { continue; }
        int start = Sums[hashIndex];
        int end = (hashIndex == Sums.Length - 1) ? SortedPointers.Length : Sums[hashIndex + 1];

        for (int j = start; j < end; j++) {
          FoundPointers.Add(SortedPointers[j]);
        }

      }

    }


  }


  /// <summary>
  /// Finds a list of pointers (int index) to objects that are in the queried cells.
  /// </summary>
  public struct SpatialHashQuerySphereJob : IJob {

    //[ReadOnly] public float3 AABBMin, AABBMax;
    [ReadOnly] public float3 CheckCenter;
    [ReadOnly] public float CheckRadius, CellSize;

    [ReadOnly] public NativeArray<float3> Positions;
    [ReadOnly] public NativeArray<float> Radii;
    [ReadOnly] public NativeArray<int> SortedPointers;
    [ReadOnly] public NativeArray<int> Sums;

    /// <summary>
    /// Each value in this list is:
    ///   the index of an object found by this query,
    ///   that object can be found in the Positions array.
    /// </summary>
    [WriteOnly] public NativeList<int> FoundPointers;

    public NativeHashSet<int> CheckedIndicies;

    public void Execute() {

      //FoundPointers = new NativeList<int>(SortedPointers.Length, Allocator.TempJob);

      //NativeHashSet<int> checkedIndicies = new NativeHashSet<int>(Sums.Length, Allocator.TempJob);

      int3 cellMin = Hash.Cell(CheckCenter - CheckRadius, CellSize);
      int3 cellMax = Hash.Cell(CheckCenter + CheckRadius, CellSize);

      for (int x = cellMin.x; x <= cellMax.x; x++) {
        for (int y = cellMin.y; y <= cellMax.y; y++) {
          for (int z = cellMin.z; z <= cellMax.z; z++) {
            int3 cell = new int3(x, y, z);

            int hashIndex = Hash.Index(cell, Sums.Length);

            if (CheckedIndicies.Contains(hashIndex)) { continue; }
            CheckedIndicies.Add(hashIndex);

            int start = Sums[hashIndex];
            int end = (hashIndex == Sums.Length - 1) ? SortedPointers.Length : Sums[hashIndex + 1];

            for (int j = start; j < end; j++) {
              int objIndex = SortedPointers[j];
              if (math.distance(Positions[objIndex], CheckCenter) > Radii[objIndex] + CheckRadius) { continue; }

              FoundPointers.Add(objIndex);
            }
          }
        }
      }


    }


  }




  /// <summary>
  /// Finds a list of pointers (int index) to objects that are in the queried cells.
  /// </summary>
  [BurstCompile]
  public struct SpatialHashQueryParticlesSphereJob : IJob {

    //[ReadOnly] public float3 AABBMin, AABBMax;
    [ReadOnly] public float3 CheckCenter;
    [ReadOnly] public float CheckRadius, CellSize, MaxParticleRadius;

    [ReadOnly] public NativeArray<int> SortedPointers;
    [ReadOnly] public NativeArray<int> Sums;
    [ReadOnly] public float Time;

    public NativeArray<SpatialHashDemo.Particle> Particles;
    public NativeHashSet<int> CheckedIndicies;

    public void Execute() {

      //NativeHashSet<int> checkedIndicies = new NativeHashSet<int>(Sums.Length, Allocator.TempJob);

      int3 cellMin = Hash.Cell(CheckCenter - CheckRadius - MaxParticleRadius, CellSize);
      int3 cellMax = Hash.Cell(CheckCenter + CheckRadius + MaxParticleRadius, CellSize);

      for (int x = cellMin.x; x <= cellMax.x; x++) {
        for (int y = cellMin.y; y <= cellMax.y; y++) {
          for (int z = cellMin.z; z <= cellMax.z; z++) {
            int3 cell = new int3(x, y, z);

            int hashIndex = Hash.Index(cell, Sums.Length);

            if (CheckedIndicies.Contains(hashIndex)) { continue; }
            CheckedIndicies.Add(hashIndex);

            int start = Sums[hashIndex];
            int end = (hashIndex == Sums.Length - 1) ? SortedPointers.Length : Sums[hashIndex + 1];

            for (int j = start; j < end; j++) {
              int objIndex = SortedPointers[j];
              SpatialHashDemo.Particle particle = Particles[objIndex];
              if (math.distance(particle.Position, CheckCenter) > particle.Radius + CheckRadius) { continue; }
              particle.LastCollision = Time;
              Particles[objIndex] = particle;
            }
          }
        }
      }

      //CheckedIndicies.Dispose();


    }


  }



  /// <summary>
  /// Finds a list of pointers (int index) to objects that are in the queried cells.
  /// </summary>
  [BurstCompile]
  public struct SpatialHashQueryParticlesCellJob : IJob {

    //[ReadOnly] public float3 AABBMin, AABBMax;
    [ReadOnly] public float3 CheckCenter;
    [ReadOnly] public float CheckRadius;

    [ReadOnly] public int HashIndex;
    [ReadOnly] public NativeArray<int> SortedPointers;
    [ReadOnly] public NativeArray<int> Sums;
    [ReadOnly] public float Time;


    // This attribute is 'Unsafe', but allows for multi-threading
    //  it allows multiple jobs to be scheduled write to the array at the same time.
    //  'Unsafe' means 1 job may overwrite another job, in an unpredictable way.
    //  However, the spatial hash ensures each job works with a unique set of Particles in the array.
    [NativeDisableContainerSafetyRestriction]
    public NativeArray<SpatialHashDemo.Particle> Particles;


    public void Execute() {
      int start = Sums[HashIndex];
      int end = (HashIndex == Sums.Length - 1) ? SortedPointers.Length : Sums[HashIndex + 1];

      for (int j = start; j < end; j++) {
        int objIndex = SortedPointers[j];
        SpatialHashDemo.Particle particle = Particles[objIndex];
        if (math.distance(particle.Position, CheckCenter) > particle.Radius + CheckRadius) { continue; }
        particle.LastCollision = Time;
        Particles[objIndex] = particle;
      }


    }


  }


  /// <summary>
  /// Finds a list of pointers (int index) to objects that are in the queried cells.
  /// </summary>
  [BurstCompile]
  public struct SpatialHashQueryBoolCellJob : IJob {

    [ReadOnly] public float3 CheckCenter;
    [ReadOnly] public float CheckRadius, CellSize, MaxParticleRadius;

    [ReadOnly] public int3 Cell;
    [ReadOnly] public NativeArray<int> SortedPointers;
    [ReadOnly] public NativeArray<int> Sums;
    [ReadOnly] public float Time;

    [ReadOnly] public NativeArray<float3> Positions;
    [ReadOnly] public NativeArray<float> Radii;

    public NativeArray<bool> IsFoundArray;

    public void Execute() {

      int hashIndex = Hash.Index(Cell, Sums.Length);

      int start = Sums[hashIndex];
      int end = (hashIndex == Sums.Length - 1) ? SortedPointers.Length : Sums[hashIndex + 1];

      for (int j = start; j < end; j++) {
        int objIndex = SortedPointers[j];
        IsFoundArray[objIndex] = math.distance(Positions[objIndex], CheckCenter) < Radii[objIndex] + CheckRadius;
      }


    }


  }
}