using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GraveyardController : MonoBehaviour
{
    public Button cloneFallenButton;
    public Button createNewButton;
    public Button closeButton;
    public TMP_Text fallenBrawlersCountText;

    public GameObject cloneVFX;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Start()
    {
        cloneFallenButton.interactable = false;
        cloneFallenButton.onClick.AddListener(OnClickedCloneFallen);
        createNewButton.onClick.AddListener(OnClickedCreateNew);
        closeButton.onClick.AddListener(OnClickedClose);

        GameScreen.instance.FalledBrawlersUpdated += OnFallenBrawlersUpdated;
    }

    private void OnFallenBrawlersUpdated(int brawlers)
    {
        fallenBrawlersCountText.text = $"Fallen Brawlers: {brawlers}";

        if (brawlers > 0)
        {
            cloneFallenButton.interactable = true;
        }
    }

    public void Toggle(bool toggle)
    {
        if (toggle)
        {
            cloneVFX.SetActive(true);
            fallenBrawlersCountText.gameObject.SetActive(true);
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        else
        {
            cloneVFX.SetActive(false);
            fallenBrawlersCountText.gameObject.SetActive(false);
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }


    private void OnClickedClose()
    {
        Toggle(false);
    }

    private void OnClickedCreateNew()
    {
        GameScreen.instance.AttemptCreateBrawler();
    }

    private void OnClickedCloneFallen()
    {
        GameScreen.instance.AttemptReviveBrawler();
    }
}
