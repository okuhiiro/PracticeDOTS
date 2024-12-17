using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum MonoMode
{
    Physics,
    All,
}

public class ManyCollisionMono : MonoBehaviour
{
  
    
    private const float Radius = 0.1f;
    
    [SerializeField]
    public GameObject prefab;
    [SerializeField]
    public GameObject prefabPhysics;
    [SerializeField]
    private int spawnCount;
    [SerializeField]
    private MonoMode mode;
    
    private List<Tuple<Transform, GameObject>> objects = new ();
    
    void Start()
    {
        if (SceneParameter.Param1 != -1)
        {
            mode = (MonoMode)SceneParameter.Param1;
        }
        
        GameObject prefab = this.prefab;
        if (mode == MonoMode.Physics)
        {
            prefab = prefabPhysics;
        }
        for (int i = 0; i < spawnCount; i++)
        {
            float x = Random.Range(-2.0f, 2.0f);
            float y = Random.Range(-2.0f, 2.0f);
            var go = Instantiate(prefab, new Vector3(x, y, 0.0f), Quaternion.identity);
            objects.Add(Tuple.Create(go.transform, go));
        }
    }

    void Update()
    {
        switch (mode)
        {
            case MonoMode.Physics:
                Physics2D.Simulate(Time.fixedDeltaTime);
                break;
            
            case MonoMode.All:
                var count = objects.Count;
                for (int i = 0; i < count; i++)
                {
                    Vector3 Displacement = Vector3.zero;
                    uint Weight = 0;
                    
                    var transform = objects[i].Item1;
                    for (int j = 0; j < count; j++)
                    {
                        var otherTransform = objects[j].Item1;
                        Vector3 towards = transform.localPosition - otherTransform.localPosition;

                        float distancesq = towards.sqrMagnitude;
                        float radiusSum = Radius + Radius;
                        if (distancesq > radiusSum * radiusSum || objects[i].Item2 == objects[j].Item2)
                            continue;
                        
                        float distance = towards.magnitude;
                        float penetration;
                        if (distance < 0.0001f)
                        {
                            penetration = 0.01f;
                        }
                        else
                        {
                            penetration = radiusSum - distance;
                            penetration = (penetration / distance);
                        }
                        
                        Displacement += towards * penetration;
                        Weight++;
                    }
                    
                    if (Weight > 0)
                    {
                        Displacement /= Weight;
                        transform.localPosition += Displacement;
                    }
                }
                break;
        }
    }
}
