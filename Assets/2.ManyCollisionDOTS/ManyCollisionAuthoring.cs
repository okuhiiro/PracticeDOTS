using Unity.Entities;
using UnityEngine;

public enum DOTSMode
{
    Physics,
    AllBustCompiler,
    AllBustCompilerJob,
    Spatial
}

public class ManyCollisionAuthoring : MonoBehaviour
{
    public GameObject Prefab;
    public GameObject PrefabPhysics;
    [Range(0, 5000)]
    public int SpawnCount;
    public DOTSMode Mode;

    class Baker : Baker<ManyCollisionAuthoring>
    {
        public override void Bake(ManyCollisionAuthoring authoring)
        {
            if (SceneParameter.Param1 != -1)
            {
                authoring.Mode = (DOTSMode)SceneParameter.Param1;
            }
            
            var prefab = authoring.Prefab;
            if (authoring.Mode == DOTSMode.Physics)
            {
                prefab = authoring.PrefabPhysics;
            }
            
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new ManyCollisionData()
            {
                Prefab = GetEntity(prefab, TransformUsageFlags.Dynamic),
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
    public DOTSMode Mode;
}