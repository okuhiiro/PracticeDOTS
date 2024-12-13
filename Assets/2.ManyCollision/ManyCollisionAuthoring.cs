using Unity.Entities;
using UnityEngine;

public enum Mode
{
    Physics,
    NoJob,
    Job,
    Spatial
}

public class ManyCollisionAuthoring : MonoBehaviour
{
    public GameObject Prefab;
    public int SpawnCount;
    public Mode Mode;
    
    class Baker : Baker<ManyCollisionAuthoring>
    {
        public override void Bake(ManyCollisionAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new ManyCollisionData()
            {
                Prefab = GetEntity(authoring.Prefab, TransformUsageFlags.None),
                SpawnCount = authoring.SpawnCount,
                Mode = authoring.Mode,
            });
        }
    }
   
}

public struct ManyCollisionData : IComponentData
{
    public Entity Prefab;
    public int SpawnCount;
    public Mode Mode;
}