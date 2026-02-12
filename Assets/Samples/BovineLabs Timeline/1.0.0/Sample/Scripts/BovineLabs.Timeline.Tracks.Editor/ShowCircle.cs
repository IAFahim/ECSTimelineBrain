using BovineLabs.Core.Jobs;
using BovineLabs.Quill;
using BovineLabs.Timeline;
using BovineLabs.Timeline.Data;
using BovineLabs.Timeline.Tracks.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Drawer = BovineLabs.Quill.Drawer;

namespace Samples.BovineLabs_Timeline._1._0._0.Sample.Scripts.BovineLabs.Timeline.Tracks.Editor
{
    public partial struct PhysicsVelocityTrackSystemEditor : ISystem
    {
        private TrackBlendImpl<PhysicsVelocity, PhysicsVelocityAnimated> _impl;

        public const int InnerloopBatchCount = 64;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _impl.OnCreate(ref state);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            _impl.OnDestroy(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var blendData = this._impl.Update(ref state);
            state.Dependency = new ViewVelocityJob
            {
                BlendData = blendData,
                VelocityLookup = SystemAPI.GetComponentLookup<PhysicsVelocity>(true),
                LocalTransform = SystemAPI.GetComponentLookup<LocalTransform>(true),
                Drawer = SystemAPI.GetSingleton<DrawSystem.Singleton>().CreateDrawer()
            }.ScheduleParallel(blendData, InnerloopBatchCount, state.Dependency);
        }

        [BurstCompile]
        private partial struct ViewVelocityJob : IJobParallelHashMapDefer
        {
            [ReadOnly] public NativeParallelHashMap<Entity, MixData<PhysicsVelocity>>.ReadOnly BlendData;
            [NativeDisableParallelForRestriction] [ReadOnly] public ComponentLookup<PhysicsVelocity> VelocityLookup;
            [NativeDisableParallelForRestriction] [ReadOnly] public ComponentLookup<LocalTransform> LocalTransform;
            public Drawer Drawer;

            public void ExecuteNext(int entryIndex, int jobIndex)
            {
                this.Read(BlendData, entryIndex, out Entity entity, out var mixData );
                var physicsVelocity = this.VelocityLookup.GetRefRO(entity).ValueRO;
                var transform = this.LocalTransform.GetRefRO(entity).ValueRO;
                Drawer.Line(transform.Position, physicsVelocity.Linear, Color.aliceBlue);
            }
        }
    }
}