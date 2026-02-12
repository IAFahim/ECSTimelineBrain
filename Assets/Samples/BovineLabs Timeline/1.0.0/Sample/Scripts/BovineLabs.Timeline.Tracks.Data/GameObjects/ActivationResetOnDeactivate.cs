namespace BovineLabs.Timeline.Tracks.Data.GameObjects
{
    using Unity.Entities;
    
    public struct ActivationResetOnDeactivate : IComponentData
    {
        public bool WasDisabled;
    }
}