using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BrawlerCharacter : MonoBehaviour
{
    public List<VisualBrawlerData> visualBrawlers;

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

    [Serializable]
    public class VisualBrawlerData
    {
        public BrawlerData.CharacterType characterType;
        public RuntimeAnimatorController controller;
    }
}
