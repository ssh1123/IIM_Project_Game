using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuInputGuard : MonoBehaviour
{
    [SerializeField] private Button[] menuButtons;

    private IEnumerator Start()
    {
        SetButtonsInteractable(false);

        // 等至少一個 frame，
        // 再等到滑鼠左鍵已放開
        yield return null;

        while (Input.GetMouseButton(0))
        {
            yield return null;
        }

        SetButtonsInteractable(true);
    }

    private void SetButtonsInteractable(bool canInteract)
    {
        foreach (Button button in menuButtons)
        {
            if (button != null)
            {
                button.interactable = canInteract;
            }
        }
    }
}