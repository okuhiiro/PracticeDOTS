using Unity.Entities;
using UnityEngine;

public class ExecuteAuthoring : MonoBehaviour
{
    public bool SimpleCollision;
    public bool ManyCollision;
    public bool UseManaged;

    class Baker : Baker<ExecuteAuthoring>
    {
        public override void Bake(ExecuteAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            if (authoring.SimpleCollision) AddComponent<SimpleCollision>(entity);
            if (authoring.ManyCollision) AddComponent<ManyCollision>(entity);
            if (authoring.UseManaged) AddComponent<UseManaged>(entity);
        }
    }
}

public struct SimpleCollision : IComponentData
{
}

public struct ManyCollision : IComponentData
{
}

public struct UseManaged : IComponentData
{
}