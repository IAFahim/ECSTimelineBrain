using System;
using BovineLabs.Timeline.Data;
using Samples.BovineLabs_Timeline._1._0._0.Sample.Scripts.BovineLabs.Timeline.Tracks.Data;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class StartUI : MonoBehaviour
{
    private void Start()
    {
        TriggerTimeline();
    }

    public void TriggerTimeline()
    {
        if (World.DefaultGameObjectInjectionWorld == null)
        {
            return;
        }

        var em = World.DefaultGameObjectInjectionWorld.EntityManager;

        using var query = new EntityQueryBuilder(Allocator.Temp).WithAll<TimelineReference>().WithDisabled<TimelineActive>().Build(em);

        foreach(var e in query.ToEntityArray(Allocator.Temp))
        {
            em.SetComponentEnabled<TimelineActive>(e, true);
        }
    }
}
