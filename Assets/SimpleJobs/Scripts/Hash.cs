using UnityEngine;
using Unity.Mathematics;

namespace SimpleJobs {
  public static class Hash {

    #region UnityEngine

    /// <summary>Find the cell the given transform is in.</summary>
    /// <param name="cellSize">Size of each cell.</param>
    public static Vector3Int Cell(Transform transform, float cellSize) {
      return Cell(transform.position, cellSize);
    }

    /// <summary>Find the cell the given transform is in.</summary>
    /// <param name="cellSize">Size of each cell.</param>
    public static Vector3Int Cell(Vector3 position, float cellSize) {
      position /= cellSize;
      return new Vector3Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y), Mathf.RoundToInt(position.z));
    }

    /// <summary>Finds the index of the cell the given transform is in.</summary>
    /// <param name="hashLength">Length of the hash arrays.</param>
    /// <param name="cellSize">Size of each cell.</param>
    public static int Index(Transform transform, int hashLength, float cellSize) {
      return Index(transform.position, hashLength, cellSize);
    }

    /// <summary>Finds the index of the cell the given position is in.</summary>
    /// <param name="hashLength">Length of the hash arrays.</param>
    /// <param name="cellSize">Size of each cell.</param>
    public static int Index(Vector3 position, int hashLength, float cellSize) {
      return Index(Cell(position, cellSize), hashLength);
    }

    /// <summary>Finds the hash-index of the given cell.</summary>
    /// <param name="hashLength">Length of the hash arrays.</param>
    public static int Index(Vector3Int cell, int hashLength) {
      // Multiply each coord by a large prime number. 
      int i = (cell.x * 9997007) ^ (cell.y * 9997997) ^ (cell.z * 9998977);

      // The C# remainder operator '%' doesn't do anything to negative numbers,
      // use a loop instead of absolute value
      // ( -1 -> int.MaxValue, rather than 1 )
      i &= int.MaxValue;

      return i % hashLength;
    }

    #endregion UnityEngine

    #region Unity.Mathematics

    /// <summary>Find the cell the given transform is in.</summary>
    /// <param name="cellSize">Size of each cell.</param>
    public static int3 Cell(float3 position, float cellSize) {
      position /= cellSize;
      return (int3)math.round(position);
    }

    /// <summary>Finds the index of the cell the given position is in.</summary>
    /// <param name="hashLength">Length of the hash arrays.</param>
    /// <param name="cellSize">Size of each cell.</param>
    public static int Index(float3 position, int hashLength, float cellSize) {
      return Index(Cell(position, cellSize), hashLength);
    }

    /// <summary>Finds the hash-index of the given cell.</summary>
    /// <param name="hashLength">Length of the hash arrays.</param>
    public static int Index(int3 cell, int hashLength) {
      // Multiply each coord by a large prime number. 
      cell *= new int3(9997007, 9997997, 9998977);
      int i = cell.x ^ cell.y ^ cell.z;

      // The C# remainder operator '%' doesn't do anything to negative numbers,
      // use a loop instead of absolute value
      // ( -1 -> int.MaxValue, rather than 1 )
      i &= int.MaxValue;

      return i % hashLength;
    }


    #endregion Unity.Mathematics

  }
}