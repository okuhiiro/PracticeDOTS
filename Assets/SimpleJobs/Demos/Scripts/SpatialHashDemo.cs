using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SimpleJobs;
using Unity.Collections;
using Unity.Mathematics;

using UR = UnityEngine.Random;
using UnityEngine.UI;
using Unity.Jobs;

namespace SimpleJobs.Demos {

  // Great reference on spatial hashes:
  // Teschner, M., Heidelberger, B., Müller, M., Pomerantes, D., & Gross, M. H. (2003, November).
  // Optimized spatial hashing for collision detection of deformable objects. In Vmv (Vol. 3, pp. 47-54).

  /// <summary>
  /// Demonstration of a Spatial Hash algorithm, which makes use of some the jobs in SimpleJobs.
  /// </summary>
  public class SpatialHashDemo : MonoBehaviour {

    [Header("Particles")]
    public bool NewParticlesBttn;
    public int NewParticlesCount;
    public Slider NewParticlesSlider;
    
    public Bounds ParticleBounds;

    public Particle[] Particles;

    public GameObject ParticlePrefab;
    float maxRadius = 1.5f;



    [Header("Hash Performance")]
    public bool ShowHashCountsBttn;
    public int[] HashCounts;


    [Header("Performance Test Controls")]
    public int FindOverlapsType;
    public bool EnableFindOverlaps, UpdateParticles;
    public Toggle CheckToggle, UpdateToggle;

    public Slider CheckSizeSlider, HashCellSizeSlider, UpdateTypeSlider;
    public float CheckRadius = 1f;
    public Transform CheckSphere;

    [Range(.01f, 100f, order = -1)]
    public float HashCellSize = 1f;
    public Transform HashCellMarkers;


    [Header("Performance Test Results\n (10000 Ticks = 1 Millisecond)")]
    [TextArea(3, 10)]
    public string PerformanceResults;
    public Text PerformanceText;

    public Button ClearPerformanceData;

    public List<long> queryData;
    public int qdPC, qdHS, qdUT;
    public float qdCS, qdCR;

    public float queryDataMean, queryDataVariance;

    private void Start() {
      NewParticlesSlider.minValue = 1;
      NewParticlesSlider.maxValue = 100;
      NewParticlesSlider.value = Mathf.Sqrt( NewParticlesCount);
      NewParticlesSlider.onValueChanged.AddListener((float f) => {
        int i = Mathf.RoundToInt(f * f);
        if (i != NewParticlesCount) { NewParticlesBttn = true; }
        NewParticlesCount = i;
      });


      CheckToggle.isOn = EnableFindOverlaps;
      CheckToggle.onValueChanged.AddListener((bool b) => { EnableFindOverlaps = b; });

      UpdateToggle.isOn = UpdateParticles;
      UpdateToggle.onValueChanged.AddListener((bool b) => { UpdateParticles = b; });

      HashCellSizeSlider.minValue = 0.5f;
      HashCellSizeSlider.maxValue = 50f;
      HashCellSizeSlider.value = HashCellSize;
      HashCellSizeSlider.onValueChanged.AddListener((float f) => { HashCellSize = f; HashCellMarkers.transform.localScale = new Vector3(HashCellSize, 1, 1); });


      CheckSizeSlider.minValue = 0.1f;
      CheckSizeSlider.maxValue = 100f;
      CheckSizeSlider.value = CheckRadius;
      CheckSizeSlider.onValueChanged.AddListener((float f) => { CheckRadius = f; CheckSphere.transform.localScale = CheckRadius * Vector3.one; });


      UpdateTypeSlider.minValue = 1;
      UpdateTypeSlider.maxValue = 4;
      UpdateTypeSlider.value = FindOverlapsType;
      UpdateTypeSlider.onValueChanged.AddListener((float f) => { FindOverlapsType = Mathf.RoundToInt(f); });

      ClearPerformanceData.onClick.AddListener(() => { queryData = new List<long>(); PerformanceResults = ""; });
    }



    private void Update() {

      if (NewParticlesBttn) {
        NewParticlesBttn = false;

        Particles = new Particle[NewParticlesCount];

        for (int i = 0; i < NewParticlesCount; i++) {
          Vector3 rPos = UR.insideUnitSphere;
          rPos.x *= ParticleBounds.size.x;
          rPos.y *= ParticleBounds.size.y;
          rPos.z *= ParticleBounds.size.z;
          rPos += ParticleBounds.min;

          Particles[i] = new Particle { Position = rPos, Radius = UR.Range(0.5f, maxRadius), Velocity = UR.onUnitSphere * (UR.value + 2f) };
        }

      }

      if (transform.childCount < Particles.Length) {
        for (int i = transform.childCount; i < Particles.Length; i++) {
          Instantiate(ParticlePrefab, transform);
        }
      }

      if (UpdateParticles) {
        for (int i = 0; i < Particles.Length; i++) {
          Vector3 newPos = Particles[i].Position + (Particles[i].Velocity * Time.deltaTime);
          if (!ParticleBounds.Contains(newPos)) {
            if (newPos.x > ParticleBounds.max.x) { newPos.x -= ParticleBounds.size.x; }
            if (newPos.y > ParticleBounds.max.y) { newPos.y -= ParticleBounds.size.y; }
            if (newPos.z > ParticleBounds.max.z) { newPos.z -= ParticleBounds.size.z; }

            if (newPos.x < ParticleBounds.min.x) { newPos.x += ParticleBounds.size.x; }
            if (newPos.y < ParticleBounds.min.y) { newPos.y += ParticleBounds.size.y; }
            if (newPos.z < ParticleBounds.min.z) { newPos.z += ParticleBounds.size.z; }
          }
          Particles[i].Position = newPos;
          Particles[i].CurrentCell = Hash.Cell((float3)newPos, HashCellSize);
        }
      }

      if (EnableFindOverlaps) {
        switch (FindOverlapsType) {
          default:
          case 1: FindOverlaps1(); break;
          case 2: FindOverlaps2(); break;
          case 3: FindOverlaps3(); break;
          case 4: FindOverlapsBruteForce(); break;
        }

      }

      PerformanceText.text = PerformanceResults;
    }


    void SavePerformanceData(string results, long queryTicks, int pCount, int hSize, int updateType, float cellSize, float cRadius) {
      PerformanceResults = results;

      if (qdPC != pCount
        || qdHS != hSize
        || qdUT != updateType
        || qdCS != cellSize
        || qdCR != cRadius) {
        queryData = new List<long>();
        queryData.Add(queryTicks);
        qdPC = pCount;
        qdHS = hSize;
        qdUT = updateType;
        qdCS = cellSize;
        qdCR = cRadius;
      }
      queryData.Add(queryTicks);

      NativeArray<float> queryDataArray = new NativeArray<float>(queryData.Count, Allocator.TempJob);

      for (int i = 0; i < queryDataArray.Length; i++) {
        queryDataArray[i] = queryData[i];
      }

      NativeArray<float> diffs = new NativeArray<float>(queryDataArray.Length, Allocator.TempJob);
      NativeArray<float> sqrs = new NativeArray<float>(queryDataArray.Length, Allocator.TempJob);

      NativeArray<float> meanResult = new NativeArray<float>(1, Allocator.TempJob);
      NativeArray<float> varianceResult = new NativeArray<float>(1, Allocator.TempJob);

      MeanJobFloat meanJob = new MeanJobFloat { Given = queryDataArray, Result = meanResult };
      JobHandle meanHandle = meanJob.Schedule();
      meanHandle.Complete();
      queryDataMean = meanResult[0];

      meanResult[0] = -queryDataMean;

      AddUniformJobFloat diffJob = new AddUniformJobFloat { Given = queryDataArray, Value = meanResult, Results = diffs };

      JobHandle diffsHandle = diffJob.Schedule(queryDataArray.Length, 64, meanHandle);

      SquaresJobFloat squaresJob = new SquaresJobFloat { Given = diffs, Squares = sqrs };
      JobHandle sqrsHandle = squaresJob.Schedule(queryDataArray.Length, 64, diffsHandle);

      VarianceJobFloat varainceJob = new VarianceJobFloat { MeanDiffSquares = sqrs, SampleOrPopulation = true, Result = varianceResult };
      JobHandle varianceHandle = varainceJob.Schedule(sqrsHandle);

      varianceHandle.Complete();
      queryDataVariance = varainceJob.Result[0];


      queryDataArray.Dispose();
      diffs.Dispose();
      sqrs.Dispose();
      meanResult.Dispose();
      varianceResult.Dispose();

      PerformanceResults += $"\nQueryData: N={queryDataArray.Length}\nMean={(int)queryDataMean}\nStd Deviation={(int)Mathf.Sqrt(queryDataVariance)}";
    }


    /// <summary>
    /// In this variation, the spatial hash job modifies the array of particles.
    /// </summary>
    void FindOverlaps1() {

      System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch(); 
      System.Text.StringBuilder sb = new System.Text.StringBuilder();

      long initializeTicks = 0;
      long queryTicks = 0;

      //int hashSize = math.max(20, Mathf.RoundToInt( Mathf.Sqrt(20 * Particles.Length)) );
      //int hashSize = math.max(20,Particles.Length);
      int3 maxCellSize = (int3)math.ceil(((float3)ParticleBounds.size) / HashCellSize);
      int hashSize = math.min(Particles.Length, maxCellSize.x * maxCellSize.y * maxCellSize.z);

      sb.AppendLine($"FindOverlaps :: Spatial Hash (1)\nParticleCount = {Particles.Length}, CheckRadius={CheckRadius}, CellSize={HashCellSize}, HashSize={hashSize}");

      sw.Start();
      NativeArray<int3> particleCells = new NativeArray<int3>(Particles.Length, Allocator.TempJob);
      NativeArray<float3> particlePositions = new NativeArray<float3>(Particles.Length, Allocator.TempJob);
      for (int i = 0; i < Particles.Length; i++) {
        particleCells[i] = Particles[i].CurrentCell;
        particlePositions[i] = Particles[i].Position;
      }
      sw.Stop();
      sb.AppendLine($"Create particle array:\t Ticks ={sw.ElapsedTicks}");
      initializeTicks += sw.ElapsedTicks;

      sw.Restart();
      NativeArray<int> counts = new NativeArray<int>(hashSize, Allocator.TempJob);
      HashCountJob hashCountJob = new HashCountJob{HashSize = hashSize, CellPositions = particleCells, Counts = counts };
      JobHandle hashCountHandle =  hashCountJob.Schedule();
      hashCountHandle.Complete();
      sw.Stop();
      sb.AppendLine($"HashCountJob:\t\t Ticks ={sw.ElapsedTicks}");
      initializeTicks += sw.ElapsedTicks;

      if (ShowHashCountsBttn) {
        ShowHashCountsBttn = false;
        HashCounts = counts.ToArray();
      }


      sw.Restart();

      NativeArray<int> sums = new NativeArray<int>(hashSize, Allocator.TempJob);
      PartialSumJobInt partialSumJob = new PartialSumJobInt { Given = counts , Sums = sums};
      //partialSumJob.Execute();
      JobHandle partialSumHandle = partialSumJob.Schedule();
      partialSumHandle.Complete();

      sw.Stop();
      sb.AppendLine($"PartialSumJob:\t\t Ticks ={sw.ElapsedTicks}");
      initializeTicks += sw.ElapsedTicks;

      sw.Restart();
      NativeArray<int> sortedPointers = new NativeArray<int>(Particles.Length, Allocator.TempJob);
      SpatialHashSortJob sortJob = new SpatialHashSortJob { Sums = sums, CellPositions = particleCells, SortedPointers =sortedPointers };
      //sortJob.Execute();
      JobHandle sortHandle = sortJob.Schedule();
      sortHandle.Complete();

      sw.Stop();
      sb.AppendLine($"SpatialHashSortJob:\t Ticks ={sw.ElapsedTicks}");
      initializeTicks += sw.ElapsedTicks;
      sb.AppendLine($"Total Initialize Jobs:\t Ticks ={initializeTicks}");

      //example query
      sw.Restart();
      NativeArray<Particle> particles = new NativeArray<Particle>(Particles, Allocator.TempJob);
      NativeHashSet<int> checkedIndicies = new NativeHashSet<int>(sums.Length, Allocator.TempJob);
      SpatialHashQueryParticlesSphereJob queryJob = new SpatialHashQueryParticlesSphereJob {
        CellSize = HashCellSize,
        CheckCenter = transform.position,
        CheckRadius = CheckRadius,
        MaxParticleRadius = maxRadius,
        Particles = particles,
        SortedPointers = sortedPointers,
        Sums = sums,
        CheckedIndicies = checkedIndicies,
        Time = Time.time
        
      };
      //queryJob.Execute();
      JobHandle queryHandle = queryJob.Schedule();
      queryHandle.Complete();

      Particles = particles.ToArray();

      sw.Stop();
      sb.AppendLine($"SpatialHashQueryJob:\t Ticks ={sw.ElapsedTicks}");

      queryTicks += sw.ElapsedTicks;
      sb.AppendLine($"Total QueryTicks:\t Ticks ={queryTicks}");


      sw.Restart();

      particlePositions.Dispose();
      //particleRadii.Dispose();
      particleCells.Dispose();
      counts.Dispose();
      sums.Dispose();
      sortedPointers.Dispose();
      //queriedCells.Dispose();
      //foundPointers.Dispose();
      particles.Dispose();
      checkedIndicies.Dispose();

      sw.Stop();
      sb.AppendLine($"Dispose:\t\t Ticks ={sw.ElapsedTicks}");

      //Debug.Log(sb);
      SavePerformanceData(sb.ToString(), queryTicks, particles.Length, hashSize, 1, HashCellSize, CheckRadius);


    }

    /// <summary>
    /// In this variation, the job is divided up into many jobs to be multi-threaded. Each hash-cell is given its own job.
    /// </summary>
    void FindOverlaps2() {

      System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
      System.Text.StringBuilder sb = new System.Text.StringBuilder();

      long initializeTicks = 0;
      long queryTicks = 0;

      //int hashSize = math.max(20, Mathf.RoundToInt( Mathf.Sqrt(20 * Particles.Length)) );
      //int hashSize = math.max(20,Particles.Length);
      int3 maxCellSize = (int3)math.ceil(((float3)ParticleBounds.size) / HashCellSize);
      int hashSize = math.min(Particles.Length, maxCellSize.x * maxCellSize.y * maxCellSize.z);

      sb.AppendLine($"FindOverlaps :: Spatial Hash (2)\nParticleCount = {Particles.Length}, CheckRadius={CheckRadius}, CellSize={HashCellSize}, HashSize={hashSize}");

      sw.Start();
      NativeArray<int3> particleCells = new NativeArray<int3>(Particles.Length, Allocator.TempJob);
      NativeArray<float3> particlePositions = new NativeArray<float3>(Particles.Length, Allocator.TempJob);
      for (int i = 0; i < Particles.Length; i++) {
        particleCells[i] = Particles[i].CurrentCell;
        particlePositions[i] = Particles[i].Position;
      }
      sw.Stop();
      sb.AppendLine($"Create particle array:\t Ticks ={sw.ElapsedTicks}");
      initializeTicks += sw.ElapsedTicks;

      sw.Restart();
      NativeArray<int> counts = new NativeArray<int>(hashSize, Allocator.TempJob);
      HashCountJob hashCountJob = new HashCountJob { HashSize = hashSize, CellPositions = particleCells, Counts = counts };
      JobHandle hashCountHandle = hashCountJob.Schedule();
      hashCountHandle.Complete();
      sw.Stop();
      sb.AppendLine($"HashCountJob:\t\t Ticks ={sw.ElapsedTicks}");
      initializeTicks += sw.ElapsedTicks;

      if (ShowHashCountsBttn) {
        ShowHashCountsBttn = false;
        HashCounts = counts.ToArray();
      }


      sw.Restart();

      NativeArray<int> sums = new NativeArray<int>(hashSize, Allocator.TempJob);
      PartialSumJobInt partialSumJob = new PartialSumJobInt { Given = counts, Sums = sums };
      //partialSumJob.Execute();
      JobHandle partialSumHandle = partialSumJob.Schedule();
      partialSumHandle.Complete();

      sw.Stop();
      sb.AppendLine($"PartialSumJob:\t\t Ticks ={sw.ElapsedTicks}");
      initializeTicks += sw.ElapsedTicks;

      sw.Restart();
      NativeArray<int> sortedPointers = new NativeArray<int>(Particles.Length, Allocator.TempJob);
      SpatialHashSortJob sortJob = new SpatialHashSortJob { Sums = sums, CellPositions = particleCells, SortedPointers = sortedPointers };
      //sortJob.Execute();
      JobHandle sortHandle = sortJob.Schedule();
      sortHandle.Complete();

      sw.Stop();
      sb.AppendLine($"SpatialHashSortJob:\t Ticks ={sw.ElapsedTicks}");
      initializeTicks += sw.ElapsedTicks;
      sb.AppendLine($"Total Initialize Jobs:\t Ticks ={initializeTicks}");

      //example query

      sw.Restart();
      int3 cellMin = Hash.Cell((float3)transform.position - CheckRadius - maxRadius, HashCellSize);
      int3 cellMax = Hash.Cell((float3)transform.position + CheckRadius + maxRadius, HashCellSize);

      NativeArray<Particle> particles = new NativeArray<Particle>(Particles, Allocator.TempJob);

      NativeHashSet<int> checkedIndicies = new NativeHashSet<int>(sums.Length, Allocator.TempJob);
      NativeList<JobHandle> queryHandles = new NativeList<JobHandle>(sums.Length, Allocator.TempJob);

      for (int x = cellMin.x; x <= cellMax.x; x++) {
        for (int y = cellMin.y; y <= cellMax.y; y++) {
          for (int z = cellMin.z; z <= cellMax.z; z++) {

            int3 cell = new int3(x, y, z);

            int hashIndex = Hash.Index(cell, hashSize);

            if (checkedIndicies.Contains(hashIndex)) { continue; }
            checkedIndicies.Add(hashIndex);

            SpatialHashQueryParticlesCellJob job = new SpatialHashQueryParticlesCellJob {
              HashIndex = hashIndex,
              CheckCenter = transform.position,
              CheckRadius = CheckRadius,
              Particles = particles,
              SortedPointers = sortedPointers,
              Sums = sums,
              Time = Time.time
            };

            queryHandles.Add(job.Schedule());
          }
        }
      }

      for (int i = 0; i < queryHandles.Length; i++) {
        queryHandles[i].Complete();
      }

      Particles = particles.ToArray();

      sw.Stop();
      sb.AppendLine($"SpatialHashQueryJob:\t Ticks ={sw.ElapsedTicks}");

      queryTicks += sw.ElapsedTicks;
      sb.AppendLine($"Total QueryTicks:\t Ticks ={queryTicks}");


      sw.Restart();

      queryHandles.Dispose();
      particlePositions.Dispose();
      particles.Dispose();
      particleCells.Dispose();
      counts.Dispose();
      sums.Dispose();
      sortedPointers.Dispose();
      checkedIndicies.Dispose();

      sw.Stop();
      sb.AppendLine($"Dispose:\t\t Ticks ={sw.ElapsedTicks}");

      //Debug.Log(sb);
      SavePerformanceData(sb.ToString(), queryTicks, particles.Length, hashSize, 2, HashCellSize, CheckRadius);


    }

    /// <summary>
    /// In this variation, the spatial hash job returns a list of indicies. Each index in the list points to a particle.
    /// </summary>
    void FindOverlaps3() {

      System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch(); 
      System.Text.StringBuilder sb = new System.Text.StringBuilder();

      long initializeTicks = 0;
      long queryTicks = 0;

      //int hashSize = math.max(20, Mathf.RoundToInt( Mathf.Sqrt(20 * Particles.Length)) );
      //int hashSize = math.max(20,Particles.Length);
      int3 maxCellSize = (int3)math.ceil(((float3)ParticleBounds.size) / HashCellSize);
      int hashSize = math.min(Particles.Length, maxCellSize.x * maxCellSize.y * maxCellSize.z);

      sb.AppendLine($"FindOverlaps :: Spatial Hash (3)\nParticleCount = {Particles.Length}, CheckRadius={CheckRadius}, CellSize={HashCellSize}, HashSize={hashSize}");

      sw.Start();
      NativeArray<int3> particleCells = new NativeArray<int3>(Particles.Length, Allocator.TempJob);
      NativeArray<float> particleRadii = new NativeArray<float>(Particles.Length, Allocator.TempJob);
      NativeArray<float3> particlePositions = new NativeArray<float3>(Particles.Length, Allocator.TempJob);
      for (int i = 0; i < Particles.Length; i++) {
        particleCells[i] = Particles[i].CurrentCell;
      particleRadii[i] = Particles[i].Radius;
        particlePositions[i] = Particles[i].Position;
      }
      sw.Stop();
      sb.AppendLine($"Create particle array:\t Ticks ={sw.ElapsedTicks}");
      initializeTicks += sw.ElapsedTicks;

      sw.Restart();
      NativeArray<int> counts = new NativeArray<int>(hashSize, Allocator.TempJob);
      HashCountJob hashCountJob = new HashCountJob { HashSize = hashSize, CellPositions = particleCells, Counts = counts };
      JobHandle hashCountHandle = hashCountJob.Schedule();
      hashCountHandle.Complete();
      sw.Stop();
      sb.AppendLine($"HashCountJob:\t\t Ticks ={sw.ElapsedTicks}");
      initializeTicks += sw.ElapsedTicks;

      if (ShowHashCountsBttn) {
        ShowHashCountsBttn = false;
        HashCounts = counts.ToArray();
      }


      sw.Restart();

      NativeArray<int> sums = new NativeArray<int>(hashSize, Allocator.TempJob);
      PartialSumJobInt partialSumJob = new PartialSumJobInt { Given = counts, Sums = sums };
      //partialSumJob.Execute();
      JobHandle partialSumHandle = partialSumJob.Schedule();
      partialSumHandle.Complete();

      sw.Stop();
      sb.AppendLine($"PartialSumJob:\t\t Ticks ={sw.ElapsedTicks}");
      initializeTicks += sw.ElapsedTicks;

      sw.Restart();
      NativeArray<int> sortedPointers = new NativeArray<int>(Particles.Length, Allocator.TempJob);
      SpatialHashSortJob sortJob = new SpatialHashSortJob { Sums = sums, CellPositions = particleCells, SortedPointers = sortedPointers };
      //sortJob.Execute();
      JobHandle sortHandle = sortJob.Schedule();
      sortHandle.Complete();

      sw.Stop();
      sb.AppendLine($"SpatialHashSortJob:\t Ticks ={sw.ElapsedTicks}");
      initializeTicks += sw.ElapsedTicks;
      sb.AppendLine($"Total Initialize Jobs:\t Ticks ={initializeTicks}");


      NativeList<int> foundPointers = new NativeList<int>(Particles.Length, Allocator.TempJob);
      NativeHashSet<int> checkedIndicies = new NativeHashSet<int>(hashSize, Allocator.TempJob);

      //example query
      sw.Restart();
      SpatialHashQuerySphereJob queryJob = new SpatialHashQuerySphereJob {
        CheckCenter = transform.position,
        CheckRadius = CheckRadius,
        Positions = particlePositions,
        Radii = particleRadii,
        CellSize = HashCellSize,
        SortedPointers = sortedPointers,
        Sums = sums,
        FoundPointers = foundPointers,
        CheckedIndicies = checkedIndicies
      };
      //queryJob.Execute();
      JobHandle queryHandle = queryJob.Schedule();
      queryHandle.Complete();


      sw.Stop();
      sb.AppendLine($"SpatialHashQueryJob:\t Ticks ={sw.ElapsedTicks}");
      queryTicks += sw.ElapsedTicks;

      sw.Restart();
      for (int i = 0; i < queryJob.FoundPointers.Length; i++) {
        int pointer = foundPointers[i];
        Particles[pointer].LastCollision = Time.time;
      }

      sw.Stop();
      sb.AppendLine($"Apply Overlaps:\t\t Ticks ={sw.ElapsedTicks}");
      queryTicks += sw.ElapsedTicks;
      sb.AppendLine($"Total QueryTicks:\t Ticks ={queryTicks}");

      sw.Restart();

      particlePositions.Dispose();
      particleRadii.Dispose();
      particleCells.Dispose();
      counts.Dispose();
      sums.Dispose();
      sortedPointers.Dispose();
      foundPointers.Dispose();
      checkedIndicies.Dispose();

      sw.Stop();
      sb.AppendLine($"Dispose:\t\t Ticks ={sw.ElapsedTicks}");

      SavePerformanceData(sb.ToString(), queryTicks, particlePositions.Length, hashSize, 3, HashCellSize, CheckRadius);
      //Debug.Log(sb);
    }

    void FindOverlapsBruteForce() {

      System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
      System.Text.StringBuilder sb = new System.Text.StringBuilder();

      long queryTicks = 0;

      sb.AppendLine($"FindOverlaps :: Brute Force\nParticleCount = {Particles.Length}, CheckRadius={CheckRadius}");

      sw.Start();

      NativeArray<float3> particlePositions = new NativeArray<float3>(Particles.Length, Allocator.TempJob);
      NativeArray<float> particleRadii = new NativeArray<float>(Particles.Length, Allocator.TempJob);

      for (int i = 0; i < Particles.Length; i++) {
        particlePositions[i] = Particles[i].Position;
        particleRadii[i] = Particles[i].Radius;
      }

      sw.Stop();
      sb.AppendLine($"Create particle arrays:\t Ticks ={sw.ElapsedTicks}");

      sw.Restart();
      var checkDistanceJob = new CheckSpheresDistanceJobIndexList { CheckCenter = transform.position, CheckRadius = CheckRadius, Positions = particlePositions, Radii = particleRadii };
      checkDistanceJob.Execute();
      NativeList<int> foundPointers = checkDistanceJob.FoundPointers;
      sw.Stop();
      sb.AppendLine($"CheckDistanceJob:\t Ticks ={sw.ElapsedTicks}");
      queryTicks += sw.ElapsedTicks;

      sw.Restart();
      for (int i = 0; i < foundPointers.Length; i++) {
        int pointer = foundPointers[i];
        Particles[pointer].LastCollision = Time.time;
      }

      sw.Stop();
      sb.AppendLine($"Apply Overlaps:\t\t Ticks ={sw.ElapsedTicks}");
      queryTicks += sw.ElapsedTicks;
      sb.AppendLine($"Total Query Ticks:\t Ticks ={queryTicks}");

      sw.Restart();
      particlePositions.Dispose();
      particleRadii.Dispose();
      foundPointers.Dispose();

      sw.Stop();
      sb.AppendLine($"Dispose:\t\t Ticks ={sw.ElapsedTicks}");

      //Debug.Log(sb);
      SavePerformanceData(sb.ToString(), queryTicks, particlePositions.Length, 0, -1, 0, CheckRadius);
    }




    private void OnDrawGizmosSelected() {
      //if (Particles == null) { return; }
      //for (int i = 0; i < Particles.Length; i++) {
      //  Gizmos.color = (Particles[i].LastCollision > Time.time - .1f) ? new Color(.95f, .25f, .2f) : new Color(.25f, .75f, .8f);
      //  Gizmos.DrawLine(Particles[i].Position, new Vector3(Particles[i].Position.x, 0, Particles[i].Position.z));
      //  Gizmos.DrawSphere(Particles[i].Position, Particles[i].Radius);
      //}
      Gizmos.color = Color.white;
      Gizmos.DrawWireSphere(transform.position, CheckRadius);
    }

    [System.Serializable]
    public struct Particle {
      public Vector3 Position;
      public int3 CurrentCell;
      public Vector3 Velocity;
      public float Radius;
      public float LastCollision;
      public float CollisionTimer => Time.time - LastCollision;
    }

  }
}