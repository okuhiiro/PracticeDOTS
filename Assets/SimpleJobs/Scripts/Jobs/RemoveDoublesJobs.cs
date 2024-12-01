using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

namespace SimpleJobs {

  /// <summary>
  /// Takes a sorted array of int, and removes duplicate values by replacing them with an invalid value.
  /// </summary>
  [BurstCompile]
  public struct ReplaceDoublesJob : IJob {

    /// <summary>
    /// Must be sorted first.
    /// </summary>
    public NativeArray<int> GivenSorted;

    /// <summary>
    /// Value used to replace duplicates
    /// </summary>
    public int InvalidValue;

    public void Execute() {

      for (int i = 1; i < GivenSorted.Length; i++) {

        if(GivenSorted[i] == GivenSorted[i - 1]) {
          GivenSorted[i - 1] = InvalidValue;
        }

      }
    }
  }

  /// <summary>
  /// Takes a sorted array of int, and creates a new list without duplicate values.
  /// </summary>
  [BurstCompile]
  public struct RemoveDoublesJob : IJob {

    /// <summary>
    /// Must be sorted first.
    /// </summary>
   [ReadOnly]  public NativeArray<int> GivenSorted;

   [WriteOnly] public NativeArray<int> Reduced;


    public void Execute() {

      int j = 0;

      Reduced = new NativeArray<int>(GivenSorted.Length, Allocator.TempJob);

      Reduced[0] = GivenSorted[0];

      for (int i = 1; i < GivenSorted.Length; i++) {

        if (GivenSorted[i] != GivenSorted[i - 1]) {
          Reduced[j] = GivenSorted[i];
          j++;
        }

      }

       Reduced.Slice(0, j);

    }
  }


  /// <summary>
  /// Takes an array of int, and creates a new list without duplicate values. Uses a NativeHashSet<>.
  /// </summary>
  [BurstCompile]
  public struct RemoveUnsortedDoublesJob : IJob {

    [ReadOnly] public NativeArray<int> Given;
    [WriteOnly] public NativeArray<int> Reduced;


    public void Execute() {

      int j = 0;

      NativeHashSet<int> hashSet = new NativeHashSet<int>();
      Reduced = new NativeArray<int>(Given.Length, Allocator.TempJob);

      for (int i = 0; i < Given.Length; i++) {
        
        if (!hashSet.Contains(Given[i])) {
          Reduced[j] = Given[i];
          j++;
        }

      }

      Reduced.Slice(0, j);
      hashSet.Dispose();
    }
  }


}