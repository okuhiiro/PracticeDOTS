using Unity.Entities;
using UnityEngine;

public class TemplateAuthoring : MonoBehaviour
{
    public GameObject Prefab;
    
    class Baker : Baker<TemplateAuthoring>
    {
        public override void Bake(TemplateAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new TemplateData()
            {
                Prefab = GetEntity(authoring.Prefab, TransformUsageFlags.None),
            });
        }
    }
}

public struct TemplateData : IComponentData
{
    public Entity Prefab;
}