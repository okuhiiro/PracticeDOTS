using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

namespace SimpleJobs {


  /// <summary>
  /// Part of the Spatial Hash process. Takes a list of objects, creates and sorts their pointers (int index), such that all the objects in each hash-cell are together in the list.
  /// </summary>
  [BurstCompile]
  public struct SpatialHashSortJob : IJob {

    [ReadOnly] public NativeArray<int3> CellPositions;
    public NativeArray<int> Sums;

    [WriteOnly] public NativeArray<int> SortedPointers;

    public void Execute() {

      //SortedPointers = new NativeArray<int>(Positions.Length, Allocator.TempJob);

      for (int i = 0; i < CellPositions.Length; i++) {
        int hash = Hash.Index(CellPositions[i], Sums.Length);
        Sums[hash] = Sums[hash] - 1;
        int sortIndex = Sums[hash];
        SortedPointers[sortIndex] = i;
      }

    }
  }
}