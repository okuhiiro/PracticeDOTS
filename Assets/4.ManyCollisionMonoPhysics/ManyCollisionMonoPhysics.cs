using UnityEngine;

public class ManyCollisionMonoPhysics : MonoBehaviour
{
    [SerializeField]
    GameObject spritePrefab;
    [SerializeField]
    private int spawnCount;
    
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
        Physics2D.Simulate(Time.fixedDeltaTime);
    }
}
