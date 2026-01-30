using AV.Day02.Data;
using Unity.Entities;
using UnityEngine;

namespace AV.Day02.Authoring
{
    public class GameStateAuthoring : MonoBehaviour
    {
        public GameStates InitialState;

        class GameStateBaker : Baker<GameStateAuthoring>
        {
            public override void Bake(GameStateAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                
                AddComponent(entity, new GameState
                {
                    Value = (byte)authoring.InitialState
                });
                
                AddComponent(entity, new GameStatePrevious
                {
                    Value = 0
                });

                // Add tag components for states if they need to be pre-added (usually StateModel handles adding/removing, 
                // but the Register call requires the *type* to exist. 
                // Actually StateModel adds/removes components based on configuration.
                // The components used in Register<TState, TComponent> MUST be IEnableableComponent or standard components.
                // If they are standard components, StateModel adds/removes them.
                // If they are IEnableableComponent, StateModel enables/disables them.
                // Our LobbyState/GameplayState are IEnableableComponent, so we should add them here disabled.
                
                AddComponent(entity, new LobbyState());
                SetComponentEnabled<LobbyState>(entity, false);
                
                AddComponent(entity, new GameplayState());
                SetComponentEnabled<GameplayState>(entity, false);
            }
        }
    }
}
