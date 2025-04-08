using RhythmZombie.Scripts.Objects.Arrows;
using RhythmZombie.Scripts.Rhythm.NoteSystem;
using UnityEngine;

namespace RhythmZombie.Scripts.Rhythm.Core
{
    public class InputHandler : MonoBehaviour
    {
        [SerializeField] private Transform targetCircle;
        [SerializeField] private float masterRadius = 10f;
        [SerializeField] private float greatRadius = 25f;
        [SerializeField] private float goodRadius = 40f;

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.W)) CheckHit(ArrowDirection.Up);
            if(Input.GetKeyDown(KeyCode.A)) CheckHit(ArrowDirection.Left);
            if(Input.GetKeyDown(KeyCode.S)) CheckHit(ArrowDirection.Down);
            if(Input.GetKeyDown(KeyCode.D)) CheckHit(ArrowDirection.Right);
        }

        private HitResult EvaluateHit(float distance)
        {
            if (distance > goodRadius) return HitResult.Miss;
            if (distance > greatRadius) return HitResult.Good;
            if (distance > masterRadius) return HitResult.Great;
            return HitResult.Master;
        }

        private void CheckHit(ArrowDirection inputDirection)
        {
            var arrows = GameObject.FindGameObjectsWithTag("Arrow");
            GameObject closestArrow = null;
            var minDistance = float.MaxValue;

            foreach (var arrow in arrows)
            {
                if(!arrow.activeInHierarchy) continue;

                var distance = Vector2.Distance(arrow.GetComponent<RectTransform>().localPosition,
                    targetCircle.localPosition);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestArrow = arrow;
                }
            }

            if (!closestArrow)
            {
                Debug.Log("Промах: Стрелок не найдено");
                return;
            }

            var arrowMovement = closestArrow.GetComponent<ArrowMovement>();
            if (arrowMovement.Direction != inputDirection)
            {
                Debug.Log("Неверно: Некорректное направление");
                return;
            }

            var result = EvaluateHit(minDistance);
            Debug.Log($"Хит: {result}, Дистанция: {minDistance}");

            // closestArrow.GetComponent<ArrowMovement>().Pooler.ReturnObject(closestArrow);
        }
    }
}