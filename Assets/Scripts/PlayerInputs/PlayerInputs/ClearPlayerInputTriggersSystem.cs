using PlayerInputs.PlayerInputs.Data;
using Unity.Entities;

namespace PlayerInputs.PlayerInputs
{
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    public partial struct ClearPlayerInputTriggersSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (ecsPlayerInput, entity) in SystemAPI.Query<RefRW<ECSPlayerInput>>().WithAll<ECSPlayerInputUpdateThisFrame>().WithEntityAccess())
            {
                ecsPlayerInput.ValueRW.Buttons.ClearMarked();
                SystemAPI.SetComponentEnabled<ECSPlayerInputUpdateThisFrame>(entity, false);
            }

        }
    }
}