using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace RhythmZombie.Scripts.Objects.Poolers
{
    public class ObjectPooler : MonoBehaviour, IObjectPooler
    {
        [SerializeField] protected int initialPoolSize = 50;
        [SerializeField] protected GameObject prefab;
        [SerializeField] protected Transform spawnPoint;
        [SerializeField] protected Transform parent;

        protected Queue<GameObject> pooledObjects;

        private bool _isInitialized;

        [CanBeNull]
        public GameObject NextPooled
        {
            get
            {
                if (!_isInitialized)
                {
                    InitializePool();
                }
                
                if (pooledObjects.Count == 0)
                {
                    Debug.LogWarning("Пул объектов переполнен. Увеличь размер пула.");
                    return null;
                }

                var obj = pooledObjects.Dequeue();
                obj.SetActive(true);
                return obj;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if(!prefab) Debug.LogWarning("prefab не назначен в ObjectPooler");
            if(!spawnPoint) Debug.LogWarning("spawnpoint не назначен в ObjectPooler");
        }
#endif

        protected void InitializePool()
        {
            pooledObjects = new Queue<GameObject>(initialPoolSize);
            for (int i = 0; i < initialPoolSize; i++)
            {
                var obj = Instantiate(prefab, spawnPoint.localPosition, Quaternion.identity, parent);
                obj.SetActive(false);
                pooledObjects.Enqueue(obj);
            }

            _isInitialized = true;
        }

        public void ReturnObject(GameObject obj)
        {
            obj.SetActive(false);
            obj.transform.SetPositionAndRotation(spawnPoint.position, Quaternion.identity);
            pooledObjects.Enqueue(obj);
        }
    }
}