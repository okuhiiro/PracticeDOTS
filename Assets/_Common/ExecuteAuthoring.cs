using Unity.Entities;
using UnityEngine;

public class ExecuteAuthoring : MonoBehaviour
{
    public bool SimpleCollision;

    class Baker : Baker<ExecuteAuthoring>
    {
        public override void Bake(ExecuteAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            if (authoring.SimpleCollision) AddComponent<SimpleCollision>(entity);
        }
    }
}

public struct SimpleCollision : IComponentData
{
}