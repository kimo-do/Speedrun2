using Deathbattle.Accounts;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class GraveyardController : Window
{
    public static GraveyardController Instance;

    public Button cloneFallenButton;
    public Button createNewButton;
    public Button closeButton;
    public TMP_Text fallenBrawlersCountText;

    public GameObject cloneVFX;
    public Light2D capsuleLight;
    public Animator cloneCapsuleAnimator;
    public float brightnessOnSummon;
    public ParticleSystem idlePS;
    public ParticleSystem boomPS;
    public AudioSource summonAudio;
    public Animator revealBrawlerAnimator;

    private Coroutine glowLight;

    public override void Awake()
    {
        Instance = this;
        base.Awake();
    }

    void Start()
    {
        cloneFallenButton.interactable = false;
        cloneFallenButton.onClick.AddListener(OnClickedCloneFallen);
        createNewButton.onClick.AddListener(OnClickedCreateNew);
        closeButton.onClick.AddListener(OnClickedClose);

        GameScreen.instance.FalledBrawlersUpdated += OnFallenBrawlersUpdated;

        IdleGlowEffect();
    }

    private void OnFallenBrawlersUpdated(int brawlers)
    {
        fallenBrawlersCountText.text = $"Fallen Brawlers: {brawlers}";
        UpdateGraveyardView();
    }

    public override void Toggle(bool toggle)
    {
        base.Toggle(toggle);

        cloneCapsuleAnimator.SetBool("Clone", false);
        cloneCapsuleAnimator.SetTrigger("Reset");
        revealBrawlerAnimator.SetTrigger("Reset");

        if (toggle)
        {
            cloneVFX.SetActive(true);
            fallenBrawlersCountText.gameObject.SetActive(true);
            IdleGlowEffect();
            UpdateGraveyardView();
        }
        else
        {
            cloneVFX.SetActive(false);
            fallenBrawlersCountText.gameObject.SetActive(false);
        }
    }

    private void UpdateGraveyardView()
    {
        cloneFallenButton.interactable = false;

        if (BrawlAnchorService.Instance.CurrentGraveyard != null)
        {
            int fallenBrawlers = BrawlAnchorService.Instance.CurrentGraveyard.Brawlers.Length;

            fallenBrawlersCountText.text = $"Fallen brawlers: {fallenBrawlers}";

            if (fallenBrawlers > 0)
            {
                cloneFallenButton.interactable = true;
            }
        }
    }


    private void OnClickedClose()
    {
        GameScreen.instance.OpenProfile();
    }

    private void OnClickedCreateNew()
    {
        GameScreen.instance.AttemptCreateBrawler();
    }

    private void OnClickedCloneFallen()
    {
        GameScreen.instance.AttemptReviveBrawler();
    }

    public void SummonEffect()
    {
        cloneCapsuleAnimator.SetBool("Clone", true);
        cloneCapsuleAnimator.SetTrigger("Reset");
        revealBrawlerAnimator.SetTrigger("Reset");
        summonAudio.Play();
        capsuleLight.GetComponent<Animation>().Stop();
        idlePS.Stop();

        if (glowLight != null)
        {
            StopCoroutine(glowLight);
        }

        glowLight = StartCoroutine(GlowLight());
        StartCoroutine(DoAfterWhile(3.3f));
    }

    IEnumerator DoAfterWhile(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        revealBrawlerAnimator.SetTrigger("Reveal");
    }

    public void IdleGlowEffect()
    {
        if (glowLight != null)
        {
            StopCoroutine(glowLight);
        }

        capsuleLight.GetComponent<Animation>().Play("idle_light");
        idlePS.Play();
    }

    IEnumerator GlowLight()
    {
        while (capsuleLight.intensity < brightnessOnSummon)
        {
            capsuleLight.intensity = Mathf.MoveTowards(capsuleLight.intensity, brightnessOnSummon, Time.deltaTime * 1f);
            yield return new WaitForEndOfFrame();
        }
    }
}
