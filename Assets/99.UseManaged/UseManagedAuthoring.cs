using Unity.Entities;
using UnityEngine;

public class UseManagedAuthoring : MonoBehaviour
{
    class Baker : Baker<UseManagedAuthoring>
    {
        public override void Bake(UseManagedAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new UseManagedData()
            {
                //aaa = 1,
            });
        }
    }
   
}

public struct UseManagedData : IComponentData
{
    //public int aaa;
}