using UnityEngine;

namespace RhythmZombie.Scripts.Objects.Poolers
{
    public class ArrowPooler : ObjectPooler
    {
        [field: SerializeField]
        public RectTransform Lane
        {
            get;
             set;
        }

        public float ArrowWidth
        {
            get
            {
                if (!prefab)
                {
                    Debug.LogError($"{nameof(prefab)} не привязан в {nameof(ArrowPooler)}");
                    return 100f;
                }

                var rectTransform = prefab.GetComponent<RectTransform>();
                if (!rectTransform)
                {
                    Debug.LogError($"У {nameof(prefab)} отсутствует {nameof(RectTransform)}");
                    return 100f;
                }

                return rectTransform.sizeDelta.x;
            }
        }
    }
}