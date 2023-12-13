using TMPro;
using UnityEngine;

public class InteractionBlocker : MonoBehaviour
{
    public GameObject BlockingSpinner;
    public GameObject NonBlocking;
    public TextMeshProUGUI CurrentTransactionsInProgress;
    public TextMeshProUGUI LastTransactionTimeText;
    public TextMeshProUGUI LastError;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Update()
    {
        if (AnchorService.Instance == null)
        {
            return;
        }
        BlockingSpinner.gameObject.SetActive(AnchorService.Instance.IsAnyBlockingTransactionInProgress);
        NonBlocking.gameObject.SetActive(AnchorService.Instance.IsAnyNonBlockingTransactionInProgress);
        CurrentTransactionsInProgress.text = (AnchorService.Instance.BlockingTransactionsInProgress +
                                             AnchorService.Instance.NonBlockingTransactionsInProgress).ToString();
        LastTransactionTimeText.text = $"Last took: {AnchorService.Instance.LastTransactionTimeInMs}ms";
        LastError.text = AnchorService.Instance.LastError;
        canvasGroup.alpha = AnchorService.Instance.IsAnyBlockingTransactionInProgress ? 1f : 0f;
    }
}
