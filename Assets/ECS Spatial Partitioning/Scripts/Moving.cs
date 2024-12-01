namespace ECS_Spatial_Partitioning
{
    using Unity.Entities;
    using Unity.Mathematics;
    
    public struct Moving : IComponentData
    {
        public float Speed;
        public float3 Target;

        // Wait time in seconds after reaching the target. 
        public float WaitTime;

        // Used by MovingSystem to move off screen entities only every X frames instead of every frame.
        public int LastMovedInFrame;
    }
}