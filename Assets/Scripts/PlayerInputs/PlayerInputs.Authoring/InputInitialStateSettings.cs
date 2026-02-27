using PlayerInputs.PlayerInputs.Data;
using UnityEngine;

namespace PlayerInputs.PlayerInputs.Authoring
{
    [CreateAssetMenu(fileName = nameof(InputInitialStateSettings),
        menuName = "Input/" + nameof(InputInitialStateSettings), order = 0)]
    public class InputInitialStateSettings : ScriptableObject
    {
        public InputInitialState initialState;
    }
}