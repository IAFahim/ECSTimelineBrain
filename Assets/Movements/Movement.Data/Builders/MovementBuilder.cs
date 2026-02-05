using BovineLabs.Core.EntityCommands;
using Movements.Movement.Data.Parameters.Motion;
using Movements.Movement.Data.Parameters.Timing;
using Movements.Movement.Data.Tags.Modifiers;
using Movements.Movement.Data.Tags.MovementTypes;
using Movements.Movement.Data.Transforms;
using Movements.Movement.Data.Transforms.StartEnd;
using Unity.Mathematics;

namespace Movements.Movement.Data.Builders
{
    
    public struct MovementBuilder
    {
        private float3 _startPosition;
        private float3 _endPosition;
        private float _speed;
        private float _progress;
        private float _range;

        private bool _hasRotation;
        private quaternion _startRotation;
        private quaternion _endRotation;
        
        private bool _isLinner;

        
        public void WithPositions(float3 start, float3 end)
        {
            _startPosition = start;
            _endPosition = end;
        }

        public void WithStartPosition(float3 start) => _startPosition = start;

        public void WithEndPosition(float3 end) => _endPosition = end;

        public void WithSpeed(float value) => _speed = value;

        public void WithProgress(float value) => _progress = value;

        /// <summary>
        ///     Sets the range scaler for movement distance.
        /// </summary>
        /// <param name="value">The range scaler (1 = full distance, 0.5 = half, 2 = double).</param>
        public void WithRange(float value)
        {
            _range = value;
        }
        
        public void WithRotation(quaternion start, quaternion end)
        {
            _hasRotation = true;
            _startRotation = start;
            _endRotation = end;
        }

        public void WithLinner() => _isLinner = true;

        public void ApplyTo<T>(ref T builder) where T : struct, IEntityCommands
        {
            builder.AddComponent<LinearMovementTag>();

            builder.AddComponent(new StartPositionComponent { value = _startPosition });
            builder.AddComponent(new PositionAnimated { Value = _endPosition });
            builder.AddComponent(new SpeedComponent { value = _speed });
            builder.AddComponent(new NormalizedProgress { value = _progress });
            builder.AddComponent(new RangeComponent { value = _range });

            if (_hasRotation)
            {
                builder.AddComponent(new WithRotationTag());
                builder.AddComponent(new StartQuaternionComponent { value = _startRotation });
                builder.AddComponent(new QuaternionAnimated() { Value = _endRotation });
            }
        }
    }
}