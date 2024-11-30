using Unity.Entities;
using UnityEngine;

public class ManyCollisionAuthoring : MonoBehaviour
{
    public GameObject Prefab;
    public int SpawnCount;
    
    class Baker : Baker<ManyCollisionAuthoring>
    {
        public override void Bake(ManyCollisionAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new ManyCollisionData()
            {
                Prefab = GetEntity(authoring.Prefab, TransformUsageFlags.None),
                SpawnCount = authoring.SpawnCount,
            });
        }
    }
   
}

public struct ManyCollisionData : IComponentData
{
    public Entity Prefab;
    public int SpawnCount;
}