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

        if (brawlers > 0)
        {
            cloneFallenButton.interactable = true;
        }
    }

    public override void Toggle(bool toggle)
    {
        base.Toggle(toggle);

        if (toggle)
        {
            cloneVFX.SetActive(true);
            fallenBrawlersCountText.gameObject.SetActive(true);
            IdleGlowEffect();
        }
        else
        {
            cloneVFX.SetActive(false);
            fallenBrawlersCountText.gameObject.SetActive(false);
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

    public void SummonEffect()
    {
        cloneCapsuleAnimator.SetTrigger("Clone");
        capsuleLight.GetComponent<Animation>().Stop();
        idlePS.Stop();

        if (glowLight != null)
        {
            StopCoroutine(glowLight);
        }

        glowLight = StartCoroutine(GlowLight());
        //StartCoroutine(DoAfterWhile(2f));
    }

    IEnumerator DoAfterWhile(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        boomPS.Emit(25);
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
