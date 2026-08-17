using TMPro;
using UnityEngine;

public class IndexTextUI : MonoBehaviour
{
    [Header("TMP Text")]
    [SerializeField] private TMP_Text fundingText;
    [SerializeField] private TMP_Text interestText;
    [SerializeField] private TMP_Text sustainabilityText;

    private void OnEnable()
    {
        if (GameState.Instance == null)
            return;

        GameState.Instance.OnIndexChanged += Refresh;
        Refresh(
            GameState.Instance.Funding,
            GameState.Instance.Interest,
            GameState.Instance.Sustainability
        );
    }

    private void OnDisable()
    {
        if (GameState.Instance == null)
            return;

        GameState.Instance.OnIndexChanged -= Refresh;
    }

    private void Refresh(int funding, int interest, int sustainability)
    {
        if (fundingText != null)
        {
            fundingText.text ="資金：" + funding.ToString();
        }

        if (interestText != null)
        {
            interestText.text = "好感度：" + interest.ToString();
        }

        if (sustainabilityText != null)
        {
            sustainabilityText.text = "永續值：" + sustainability.ToString();
        }
    }
}