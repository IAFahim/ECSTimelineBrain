using Unity.Entities;
using UnityEngine;
using UnityEngine.Timeline;
using Hash128 = Unity.Entities.Hash128;

namespace BovineLabs.Timeline.Authoring
{
    // Data attached to the clip entity
    public struct RukhankaSingleClipData : IComponentData
    {
        public Hash128 ClipHash;
    }
    
}