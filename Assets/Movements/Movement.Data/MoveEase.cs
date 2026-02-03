using AV.Eases.Runtime;
using Unity.Entities;

namespace Movements.Movement.Data
{
    public struct MoveEase : IComponentData
    {
        public EaseConfig EaseConfig;
    }
}