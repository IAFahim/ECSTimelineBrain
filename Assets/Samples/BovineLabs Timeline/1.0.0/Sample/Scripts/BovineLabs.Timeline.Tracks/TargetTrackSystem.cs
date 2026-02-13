using BovineLabs.Core.Jobs;
using BovineLabs.Reaction.Data.Core;
using BovineLabs.Timeline.Data;
using Samples.BovineLabs_Timeline._1._0._0.Sample.Scripts.BovineLabs.Timeline.Tracks.Data.Targets;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace BovineLabs.Timeline.Tracks
{
    [UpdateInGroup(typeof(TimelineComponentAnimationGroup))]
    public partial struct TargetTrackSystem : ISystem
    {
        private TrackBlendImpl<Target, TargetAnimationComponent> _impl;

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
            var blendData = _impl.Update(ref state);
            var targetSelectLookup = SystemAPI.GetComponentLookup<TargetSelect>();

            state.Dependency = new ApplyTargetJob
            {
                BlendData = blendData,
                TargetSelectLookup = targetSelectLookup
            }.ScheduleParallel(blendData, 64, state.Dependency);
        }

        [BurstCompile]
        private struct ApplyTargetJob : IJobParallelHashMapDefer
        {
            [ReadOnly] public NativeParallelHashMap<Entity, MixData<Target>>.ReadOnly BlendData;
            [NativeDisableParallelForRestriction] public ComponentLookup<TargetSelect> TargetSelectLookup;

            public void ExecuteNext(int entryIndex, int jobIndex)
            {
                this.Read(BlendData, entryIndex, out var entity, out var mixData);

                if (!TargetSelectLookup.TryGetComponent(entity, out var component))
                    return;

                // Discrete blending: Select the target with the highest weight
                var bestWeight = -1.0f;
                var result = component.Target;

                if (mixData.Weights.x > bestWeight)
                {
                    bestWeight = mixData.Weights.x;
                    result = mixData.Value1;
                }
                if (mixData.Weights.y > bestWeight)
                {
                    bestWeight = mixData.Weights.y;
                    result = mixData.Value2;
                }
                if (mixData.Weights.z > bestWeight)
                {
                    bestWeight = mixData.Weights.z;
                    result = mixData.Value3;
                }
                if (mixData.Weights.w > bestWeight)
                {
                    bestWeight = mixData.Weights.w;
                    result = mixData.Value4;
                }

                // Only write if we have a valid weight (active clip)
                if (bestWeight > math.EPSILON)
                {
                    component.Target = result;
                    TargetSelectLookup[entity] = component;
                }
            }
        }
    }
}