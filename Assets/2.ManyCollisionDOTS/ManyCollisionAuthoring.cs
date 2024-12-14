using System;
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
    public GameObject PrefabPhysics;
    public int SpawnCount;
    public Mode Mode;

    class Baker : Baker<ManyCollisionAuthoring>
    {
        public override void Bake(ManyCollisionAuthoring authoring)
        {
            SelectParameter(authoring);
            
            var prefab = authoring.Prefab;
            if (authoring.Mode == Mode.Physics)
            {
                prefab = authoring.PrefabPhysics;
            }
            
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new ManyCollisionData()
            {
                Prefab = GetEntity(prefab, TransformUsageFlags.None),
                SpawnCount = authoring.SpawnCount,
                Mode = authoring.Mode,
            });
        }

        private void SelectParameter(ManyCollisionAuthoring authoring)
        {
            if (SceneParameter.Param1 != -1)
            {
                authoring.Mode = (Mode)SceneParameter.Param1;
            }
        }
    }
}

public struct ManyCollisionData : IComponentData
{
    public Entity Prefab;
    public int SpawnCount;
    public Mode Mode;
}