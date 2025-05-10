using System;
using RhythmZombie.Scripts.Objects.Arrows;
using UnityEngine;
using UnityEngine.Events;

namespace RhythmZombie.Scripts.GameCore
{
    public class InputHandler : MonoBehaviour
    {
        public UnityEvent<ArrowType> KeyPressed;

        private void Update()
        {
            foreach (var kvp in ArrowConfig.ArrowKeyMap)
            {
                if(!Input.GetKeyDown(kvp.Value))
                    continue;
                
                KeyPressed?.Invoke(kvp.Key);
            }
        }
    }
}