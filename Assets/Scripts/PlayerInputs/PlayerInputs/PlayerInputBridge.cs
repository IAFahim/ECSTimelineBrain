using PlayerInputs.PlayerInputs.Data;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerInputs.PlayerInputs
{
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputBridge : MonoBehaviour
    {
        private Entity _playerEntity;
        private EntityManager _entityManager;
        private ECSPlayerInput _ecsInputData;

        private void Start()
        {
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            _playerEntity = _entityManager.CreateEntity(typeof(ECSPlayerInput), typeof(ECSPlayerInputUpdateThisFrame));
            var unityPlayerInput = GetComponent<PlayerInput>();
            _ecsInputData = new ECSPlayerInput
            {
                ID = (byte)unityPlayerInput.playerIndex
            };

            PushToECS();
        }

        private void OnDestroy()
        {
            if (_entityManager != default && _entityManager.Exists(_playerEntity))
            {
                _entityManager.DestroyEntity(_playerEntity);
            }
        }

        private void PushToECS()
        {
            if (_entityManager != default && _entityManager.Exists(_playerEntity))
            {
                _entityManager.SetComponentData(_playerEntity, _ecsInputData);
                _entityManager.SetComponentEnabled<ECSPlayerInputUpdateThisFrame>(_playerEntity, true);
            }
        }

        public void OnMove(InputValue value)
        {
            _ecsInputData.Move = value.Get<Vector2>();
            PushToECS();
        }

        public void OnLook(InputValue value)
        {
            _ecsInputData.Look = value.Get<Vector2>();
            PushToECS();
        }

        public void OnAttack(InputValue value)
        {
            _ecsInputData.SetButton(ECSPlayerButton.Attack, value.isPressed);
            PushToECS();
        }

        public void OnInteract(InputValue value)
        {
            _ecsInputData.SetButton(ECSPlayerButton.Interact, value.isPressed);
            PushToECS();
        }

        public void OnCrouch(InputValue value)
        {
            _ecsInputData.SetButton(ECSPlayerButton.Crouch, value.isPressed);
            PushToECS();
        }

        public void OnJump(InputValue value)
        {
            _ecsInputData.SetButton(ECSPlayerButton.Jump, value.isPressed);
            PushToECS();
        }

        public void OnPrevious(InputValue value)
        {
            _ecsInputData.SetButton(ECSPlayerButton.Previous, value.isPressed);
            PushToECS();
        }

        public void OnNext(InputValue value)
        {
            _ecsInputData.SetButton(ECSPlayerButton.Next, value.isPressed);
            PushToECS();
        }

        public void OnSprint(InputValue value)
        {
            _ecsInputData.SetButton(ECSPlayerButton.Sprint, value.isPressed);
            PushToECS();
        }
    }
}