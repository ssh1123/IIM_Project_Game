using UnityEngine;

public class AnswerTrigger : MonoBehaviour
{
    [SerializeField] private LaneType laneType;
    [SerializeField] private RunnerQuestionManager questionManager;

    private bool used = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (used) return;
        if (!other.CompareTag("Player")) return;

        used = true;
        questionManager.OnPlayerChooseLane(laneType);
    }
}