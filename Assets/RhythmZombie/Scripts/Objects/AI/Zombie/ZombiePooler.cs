using RhythmZombie.Scripts.Objects.Poolers;
using UnityEngine;

namespace RhythmZombie.Scripts.Objects.AI.Zombie
{
    [RequireComponent(typeof(ZombieMaterialsKeeper))]
    public class ZombiePooler : BasePooler<ZombieController>
    {
        [SerializeField] private DanceFloorConfig config;
        
        private ZombieMaterialsKeeper _materialKeeper;
        
        public static ZombiePooler Instance { get; private set; }

        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
            
            TryGetComponent(out _materialKeeper);

            initialAmount = config.lineCount * config.zombiesPerLine;
        }
    }
}