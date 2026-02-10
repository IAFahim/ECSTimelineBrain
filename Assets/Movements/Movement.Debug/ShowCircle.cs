using BovineLabs.Quill;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Drawer = BovineLabs.Quill.Drawer;

public partial struct DrawSphereSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var drawer = SystemAPI.GetSingleton<DrawSystem.Singleton>().CreateDrawer();
        state.Dependency = new DrawJob 
        { 
            Drawer = drawer 
        }.Schedule(state.Dependency);
    }

    [BurstCompile]
    private struct DrawJob : IJob
    {
        public Drawer Drawer;

        public void Execute()
        {
            this.Drawer.Sphere(float3.zero, 1.0f, 16, Color.red);
        }
    }
}