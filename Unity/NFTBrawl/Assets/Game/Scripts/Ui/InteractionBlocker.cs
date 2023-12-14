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
        if (BrawlAnchorService.Instance == null)
        {
            return;
        }
        BlockingSpinner.gameObject.SetActive(BrawlAnchorService.Instance.IsAnyBlockingTransactionInProgress);
        NonBlocking.gameObject.SetActive(BrawlAnchorService.Instance.IsAnyNonBlockingTransactionInProgress);
        CurrentTransactionsInProgress.text = (BrawlAnchorService.Instance.BlockingTransactionsInProgress +
                                             BrawlAnchorService.Instance.NonBlockingTransactionsInProgress).ToString();
        LastTransactionTimeText.text = $"Last took: {BrawlAnchorService.Instance.LastTransactionTimeInMs}ms";
        LastError.text = BrawlAnchorService.Instance.LastError;
        canvasGroup.alpha = BrawlAnchorService.Instance.IsAnyBlockingTransactionInProgress || BrawlAnchorService.Instance.IsAnyNonBlockingTransactionInProgress ? 1f : 0f;
    }
}
