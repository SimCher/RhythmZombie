using UnityEngine;

namespace RhythmZombie.Scripts.Objects.Poolers
{
    public interface IObjectPooler
    {
        GameObject NextPooled { get; }
        void ReturnObject(GameObject obj);
        
    }
}