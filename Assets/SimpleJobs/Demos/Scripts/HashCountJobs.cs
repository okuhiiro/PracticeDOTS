using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

namespace SimpleJobs {

  /// <summary>
  /// Part of the Spatial Hash process. Counts the number of objects in each hash-cell.
  /// </summary>
  [BurstCompile]
  public struct HashCountJob : IJob {

    [ReadOnly] public NativeArray<int3> CellPositions;

    [ReadOnly] public int HashSize;
    
    public NativeArray<int> Counts;

    public void Execute() {

      //Counts = new NativeArray<int>(HashSize, Allocator.TempJob);

      for (int i = 0; i < CellPositions.Length; i++) {
        int hash = Hash.Index(CellPositions[i], HashSize);
        Counts[hash] = Counts[hash] + 1;
      }

    }
  }
}
