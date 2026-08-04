using UnityEngine;

public class QuestionTrigger : MonoBehaviour
{
    [SerializeField] private RunnerGameManager gameManager;
    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        gameManager.ShowFirstQuestion();
    }
}