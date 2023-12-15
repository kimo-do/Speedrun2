using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BrawlerCharacter : MonoBehaviour
{
    public List<VisualBrawlerData> visualBrawlers;
    public Animator abilityAnimator;

    private VisualBrawlerData myVisualBrawler;
    private Animator animator;
    private BrawlerData myBrawlerData;

    public BrawlerData MyBrawlerData { get => myBrawlerData; set => myBrawlerData = value; }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.SetFloat("offset", UnityEngine.Random.Range(0, 1f));
    }

    public void SetBrawlerData(BrawlerData brawlerData)
    {
        myBrawlerData = brawlerData;
        myVisualBrawler = visualBrawlers.FirstOrDefault(b => b.characterType == brawlerData.characterType);
        animator.runtimeAnimatorController = myVisualBrawler.controller;
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
                break;
            case BrawlerData.BrawlerType.Saber:
                abilityAnimator.SetTrigger("saber");
                break;
            case BrawlerData.BrawlerType.Pistol:
                abilityAnimator.SetTrigger("pistol");
                break;
            case BrawlerData.BrawlerType.Katana:
                abilityAnimator.SetTrigger("katana");
                break;
            case BrawlerData.BrawlerType.Laser:
                abilityAnimator.SetTrigger("laser");
                break;
            case BrawlerData.BrawlerType.Virus:
                abilityAnimator.SetTrigger("virus");
                break;
        }
    }

    [Serializable]
    public class VisualBrawlerData
    {
        public BrawlerData.CharacterType characterType;
        public RuntimeAnimatorController controller;
    }
}
