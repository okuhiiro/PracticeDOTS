using UnityEngine;
using Random = UnityEngine.Random;

public class ManyCollisionMono : MonoBehaviour
{
    private enum Mode
    {
        Physics,
        All,
        Spatial
    }
    
    [SerializeField]
    private GameObject spritePrefab;
    [SerializeField]
    private int spawnCount;
    [SerializeField]
    private Mode mode;
    
    void Start()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            float x = Random.Range(-2.0f, 2.0f);
            float y = Random.Range(-2.0f, 2.0f);
            Instantiate(spritePrefab, new Vector3(x, y, 0.0f), Quaternion.identity);
        }
    }

    void Update()
    {
        switch (mode)
        {
            case Mode.Physics:
                Physics2D.Simulate(Time.fixedDeltaTime);
                break;
        }
    }
}
