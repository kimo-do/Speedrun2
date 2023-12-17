using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BrawlerCharacter : MonoBehaviour
{
    public List<VisualBrawlerData> visualBrawlers;
    public RuntimeAnimatorController defaultAnims;
    public Animator abilityAnimator;
    public GameObject selection;

    private VisualBrawlerData myVisualBrawler;
    private Animator animator;
    private BrawlerData myBrawlerData;
    private AudioSource audioSource;

    public AudioClip hackClip;
    public AudioClip saberClip;
    public AudioClip pistolClip;
    public AudioClip katanaClip;
    public AudioClip laserClip;
    public AudioClip virusClip;

    public BrawlerData MyBrawlerData { get => myBrawlerData; set => myBrawlerData = value; }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        animator.SetFloat("offset", UnityEngine.Random.Range(0, 1f));
    }

    private void OnEnable()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        animator.SetFloat("offset", UnityEngine.Random.Range(0, 1f));
    }

    public void ToggleSelected(bool selected)
    {
        if (selected)
        {
            selection.SetActive(true);
        }
        else
        {
            selection.SetActive(false);
        }
    }

    public void SetBrawlerData(BrawlerData brawlerData)
    {
        animator = GetComponent<Animator>();

        myBrawlerData = brawlerData;
        myVisualBrawler = visualBrawlers.FirstOrDefault(b => b.characterType == brawlerData.characterType);

        if (myVisualBrawler != null)
        {
            if (myVisualBrawler.controller != null)
            {
                animator.runtimeAnimatorController = myVisualBrawler.controller;
            }
            else
            {
                animator.runtimeAnimatorController = defaultAnims;
            }
        }
        else
        {
            Debug.Log($"NO BRAWLER FOR TYPE : {brawlerData.characterType}, {brawlerData.brawlerType}");
        }

        animator.SetFloat("idlespeed", myVisualBrawler.customIdleSpeed);
    }

    public void SetDeath(bool death)
    {
        animator.SetBool("death", death);
    }

    public void DoAttack()
    {
        switch (myBrawlerData.brawlerType)
        {
            case BrawlerData.BrawlerType.Hack:
                abilityAnimator.SetTrigger("hack");
                audioSource.clip = hackClip;
                break;
            case BrawlerData.BrawlerType.Saber:
                abilityAnimator.SetTrigger("saber");
                audioSource.clip = saberClip;
                break;
            case BrawlerData.BrawlerType.Pistol:
                abilityAnimator.SetTrigger("pistol");
                audioSource.clip = pistolClip;
                break;
            case BrawlerData.BrawlerType.Katana:
                abilityAnimator.SetTrigger("katana");
                audioSource.clip = katanaClip;
                break;
            case BrawlerData.BrawlerType.Laser:
                abilityAnimator.SetTrigger("laser");
                audioSource.clip = laserClip;
                break;
            case BrawlerData.BrawlerType.Virus:
                abilityAnimator.SetTrigger("virus");
                audioSource.clip = virusClip;
                break;
        }

        audioSource.Play();
    }

    [Serializable]
    public class VisualBrawlerData
    {
        public BrawlerData.CharacterType characterType;
        public AnimatorOverrideController controller;
        public float customIdleSpeed = 1f;
    }
}
