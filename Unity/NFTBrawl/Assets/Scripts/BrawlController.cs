using Game.Scripts.Ui;
using Solana.Unity.Wallet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BrawlController : Window
{

    public GameObject brawlerLeft;
    public GameObject brawlerRight;

    public Transform leftOutside;
    public Transform leftBattle;

    public Transform rightOutside;
    public Transform rightBattle;

    public Transform leftMeleeAttack;
    public Transform rightMeleeAttack;

    public List<GameObject> crowd;
    public GameObject brawlContainer;

    [Header("Settings")]
    public float moveInSpeed = 1f;
    public float moveAttackSpeed = 1f;
    public float attackDuration = 1f;

    [Header("UI")]
    public Button closeBrawl;
    public Animation versusAnim;
    public Animation resultAnim;

    public TMP_Text playerLeftText;
    public TMP_Text playerRightText;

    private Coroutine fightSequence;
    private Coroutine leftPlayerRoutine;
    private Coroutine rightPlayerRoutine;

    private bool shownVictory = false;
    private BrawlerData firstBrawler;
    private BrawlerData secondBrawler;
    private List<BrawlerData> crowdDataList;
    private Coroutine playoutFightsRoutine;

    private Queue<BrawlerData> brawlerLineup = new();
    private PublicKey winner;
    private PublicKey myBrawlerKey;

    public override void Awake()
    {
        base.Awake();

        closeBrawl.onClick.AddListener(CloseBrawlClicked);
    }

    private void CloseBrawlClicked()
    {
        GameScreen.instance.OpenProfile();
        HideResultInstant();
    }

    public override void Toggle(bool toggle)
    {
        base.Toggle(toggle);

        brawlContainer.SetActive(toggle);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    //void Update()
    //{
    //    if (Input.GetKeyUp(KeyCode.B))
    //    {
    //        firstBrawler = new BrawlerData()
    //        {
    //            username = "Longnameguy",
    //            characterType = BrawlerData.CharacterType.Female1,
    //            brawlerType = BrawlerData.BrawlerType.Pistol,
    //        };

    //        secondBrawler = new BrawlerData()
    //        {
    //            username = "DaveR",
    //            characterType = BrawlerData.CharacterType.Bonki,
    //            brawlerType = BrawlerData.BrawlerType.Katana,
    //        };

    //        InitFightSequence(firstBrawler, secondBrawler);
    //    }

    //    if (Input.GetKeyUp(KeyCode.H))
    //    {
    //        DoAttack(true);
    //    }

    //    if (Input.GetKeyUp(KeyCode.J))
    //    {
    //        DoAttack(false);
    //    }

    //    if (Input.GetKeyUp(KeyCode.K))
    //    {
    //        brawlerRight.GetComponent<BrawlerCharacter>().SetDeath(true);
    //    }

    //    if (Input.GetKeyUp(KeyCode.L))
    //    {
    //        ShowBattleResult(false);
    //    }
    //}

    public void PlayOutFights(List<BrawlerData> allBrawlers, PublicKey winner, PublicKey myBrawler)
    {
        this.winner = winner;
        this.myBrawlerKey = myBrawler;

        brawlerLineup = new();

        //rawlerData winnerData = null;

        ListShuffler.Shuffle(allBrawlers);

        foreach (var brawler in allBrawlers)
        {
            brawlerLineup.Enqueue(brawler);
        }

        //brawlerLineup.Enqueue(winnerData);

        if (playoutFightsRoutine != null)
        {
            StopCoroutine(playoutFightsRoutine);
        }

        playoutFightsRoutine = StartCoroutine(c_PlayoutFights());
    }

    private bool readyForFight = false;
    private bool leftFighterDied = false;

    IEnumerator c_PlayoutFights()
    {
        InitCrowd(brawlerLineup.ToList());

        yield return new WaitForSeconds(1f);

        bool isFirstFight = true;
        leftFighterDied = false;
        readyForFight = false;

        Debug.Log(" ---- Brawler Lineup");
        foreach (var br in brawlerLineup.ToList())
        {
            Debug.Log(" - " + br.brawlerKey);
        }
        Debug.Log(" ----");

        while (brawlerLineup.Count > 0)
        {
            readyForFight = false;

            if (isFirstFight)
            {
                BrawlerData fighter1 = brawlerLineup.Dequeue();
                BrawlerData fighter2 = brawlerLineup.Dequeue();

                RemoveFromCrowd(fighter1);
                RemoveFromCrowd(fighter2);

                InitFightSequence(fighter1, fighter2, true, true);

                Debug.Log(" # removed and starting to fight: " + fighter1.brawlerKey);
                Debug.Log(" # removed and starting to fight: " + fighter2.brawlerKey);

                isFirstFight = false;
            }
            else
            {
                BrawlerData newFighter = brawlerLineup.Dequeue();

                RemoveFromCrowd(newFighter);

                if (leftFighterDied)
                {
                    InitFightSequence(newFighter, this.secondBrawler, true, false);
                }
                else
                {
                    InitFightSequence(this.firstBrawler, newFighter, false, true);
                }

                Debug.Log(" # removed and starting to fight: " + newFighter.brawlerKey);
            }

            yield return new WaitUntil(() => readyForFight == true);

            yield return StartCoroutine(autoPlayFight());
        }

        // All fights finished
        if (winner.ToString() == myBrawlerKey.ToString())
        {
            ShowBattleResult(true);
        }
        else
        {
            ShowBattleResult(false);
        }

        if (GameScreen.instance.ActivePlayingBrawl != null)
        {
            BrawlAnchorService.Instance.ClearEndedBrawl(true, OnEndBrawl, GameScreen.instance.ActivePlayingBrawl, GameScreen.instance.ActiveGameWinner);
        }

        GameScreen.instance.HoldWalletUpdates = false;
        SolBalanceWidget.instance.ShowPendingChanges();
    }

    private void OnEndBrawl()
    {

    }

    IEnumerator autoPlayFight()
    {
        bool isLeftWinner = false;

        if (this.winner.ToString() == this.firstBrawler.ownerKey.ToString())
        {
            isLeftWinner = true;
        }
        else if (this.winner.ToString() == this.secondBrawler.ownerKey.ToString())
        {
            isLeftWinner = false;
        }
        else
        {
            // Random winner
            isLeftWinner = UnityEngine.Random.value >= 0.5f;
        }

        yield return new WaitForSeconds(1.5f);

        if (isLeftWinner)
        {
            yield return StartCoroutine(DoAttackVisuals(true, isLeftWinner));
        }
        else
        {
            yield return StartCoroutine(DoAttackVisuals(false, !isLeftWinner));
        }

        leftFighterDied = isLeftWinner == false;

        Debug.Log(" $ elimninated: " + (isLeftWinner ? this.secondBrawler.brawlerKey.ToString() : this.firstBrawler.brawlerKey.ToString()));
        Debug.Log(" --------");


        yield return new WaitForSeconds(1.7f);       
    }

    public void RemoveFromCrowd(BrawlerData data)
    {
        GameObject brawler = crowd.FirstOrDefault(b => b.GetComponent<BrawlerCharacter>().MyBrawlerData.brawlerKey.ToString() == data.brawlerKey.ToString());

        if (brawler != null)
        {
            brawler.SetActive(false);
        }
    }

    public void InitCrowd(List<BrawlerData> crowdData)
    {
        this.crowdDataList = crowdData;

        foreach (var crowdParticipant in crowd)
        {
            crowdParticipant.SetActive(false);
        }

        for (int i = 0; i < crowd.Count; i++)
        {
            if (i < this.crowdDataList.Count)
            {
                if (this.crowdDataList[i] != null)
                {
                    crowd[i].GetComponent<BrawlerCharacter>().SetBrawlerData(this.crowdDataList[i]);
                    crowd[i].SetActive(true);
                }
            }
        }
    }

    public void InitFightSequence(BrawlerData firstBrawler, BrawlerData secondBrawler, bool moveLeft, bool moveRight)
    {
        this.firstBrawler = firstBrawler;
        this.secondBrawler = secondBrawler;

        if (moveLeft)
        {
            brawlerLeft.transform.position = leftOutside.position;
        }

        if (moveRight)
        {
            brawlerRight.transform.position = rightOutside.position;
        }

        brawlerLeft.GetComponent<BrawlerCharacter>().SetBrawlerData(this.firstBrawler);
        brawlerRight.GetComponent<BrawlerCharacter>().SetBrawlerData(this.secondBrawler);

        brawlerLeft.GetComponent<BrawlerCharacter>().SetDeath(false);
        brawlerRight.GetComponent<BrawlerCharacter>().SetDeath(false);

        if (fightSequence != null)
        {
            StopCoroutine(fightSequence);
        }

        fightSequence = StartCoroutine(FightSequence(moveLeft, moveRight));
    }

    IEnumerator FightSequence(bool moveLeft, bool moveRight)
    {
        MoveBothIntoBattlePositions(moveLeft, moveRight);

        yield return new WaitForSeconds(0.8f);

        PlayVersus(firstBrawler, secondBrawler);

        readyForFight = true;
    }

    private void PlayVersus(BrawlerData leftBrawler, BrawlerData rightBrawler)
    {
        playerLeftText.text = leftBrawler.username;
        playerRightText.text = rightBrawler.username;
        versusAnim.Play("versus_ui");
    }

    private void MoveBothIntoBattlePositions(bool moveLeft, bool moveRight)
    {
        if (leftPlayerRoutine != null)
        {
            StopCoroutine(leftPlayerRoutine);
        }

        if (rightPlayerRoutine != null)
        {
            StopCoroutine(rightPlayerRoutine);
        }

        if (moveLeft)
        {
            leftPlayerRoutine = StartCoroutine(MoveIntoBattleVisuals(0f, true));
        }

        if (moveRight)
        {
            rightPlayerRoutine = StartCoroutine(MoveIntoBattleVisuals(0.3f, false));
        }
    }

    public void DoAttack(bool forLeftPlayer)
    {
        if (forLeftPlayer)
        {
            if (leftPlayerRoutine != null)
            {
                StopCoroutine(leftPlayerRoutine);
            }

            leftPlayerRoutine = StartCoroutine(DoAttackVisuals(true, false));
        }
        else
        {
            if (rightPlayerRoutine != null)
            {
                StopCoroutine(rightPlayerRoutine);
            }

            rightPlayerRoutine = StartCoroutine(DoAttackVisuals(false, false));
        }
    }

    public void ShowBattleResult(bool wonBattle)
    {
        shownVictory = wonBattle;

        if (wonBattle)
        {
            resultAnim["victory_ui"].time = 0;
            resultAnim["victory_ui"].speed = 1f;
            resultAnim.Play("victory_ui");
        }
        else
        {
            resultAnim["eliminated_ui"].time = 0;
            resultAnim["eliminated_ui"].speed = 1f;
            resultAnim.Play("eliminated_ui");
        }
    }

    public void HideBattleResult()
    {
        if (shownVictory)
        {
            resultAnim["victory_ui"].time = resultAnim["victory_ui"].length;
            resultAnim["victory_ui"].speed = -1f;
            resultAnim.Play("victory_ui");
        }
        else
        {
            resultAnim["eliminated_ui"].time = resultAnim["eliminated_ui"].length;
            resultAnim["eliminated_ui"].speed = -1f;
            resultAnim.Play("eliminated_ui");
        }
    }

    public void HideResultInstant()
    {
        if (shownVictory)
        {
            resultAnim["victory_ui"].time = resultAnim["victory_ui"].length;
            resultAnim["victory_ui"].speed = -100f;
            resultAnim.Play("victory_ui");
        }
        else
        {
            resultAnim["eliminated_ui"].time = resultAnim["eliminated_ui"].length;
            resultAnim["eliminated_ui"].speed = -100f;
            resultAnim.Play("eliminated_ui");
        }
    }

    IEnumerator MoveIntoBattleVisuals(float delay, bool isLeftPlayer)
    {
        yield return new WaitForSeconds(delay);

        float elapsedTime = 0;

        Transform brawler = isLeftPlayer ? brawlerLeft.transform : brawlerRight.transform;
        Vector3 start = isLeftPlayer ? leftOutside.position : rightOutside.position;
        Vector3 end = isLeftPlayer ? leftBattle.position : rightBattle.position;

        while (elapsedTime < moveInSpeed)
        {
            brawler.position = Vector3.Lerp(start, end, (elapsedTime / moveInSpeed));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        brawler.position = end;
    }

    IEnumerator DoAttackVisuals(bool isLeftPlayer, bool targetDies)
    {
        float elapsedTime = 0;
        bool shouldMove = false;

        Transform brawler = isLeftPlayer ? brawlerLeft.transform : brawlerRight.transform;
        Transform opponentBrawler = isLeftPlayer ? brawlerRight.transform : brawlerLeft.transform;

        BrawlerCharacter brawlerCharacter = brawler.GetComponent<BrawlerCharacter>();

        if (brawlerCharacter.MyBrawlerData.brawlerType == BrawlerData.BrawlerType.Saber || brawlerCharacter.MyBrawlerData.brawlerType == BrawlerData.BrawlerType.Katana)
        {
            shouldMove = true;
        }

        Vector3 start = isLeftPlayer ? leftBattle.position : rightBattle.position;
        Vector3 end = isLeftPlayer ? leftMeleeAttack.position : rightMeleeAttack.position;

        if (shouldMove)
        {
            while (elapsedTime < moveInSpeed)
            {
                brawler.position = Vector3.Lerp(start, end, (elapsedTime / moveAttackSpeed));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            brawler.position = end;
        }

        brawlerCharacter.DoAttack();

        yield return new WaitForSeconds(attackDuration);

        if (targetDies)
        {
            opponentBrawler.GetComponent<BrawlerCharacter>().SetDeath(true);
        }

        elapsedTime = 0;

        if (shouldMove)
        {
            while (elapsedTime < moveInSpeed)
            {
                brawler.position = Vector3.Lerp(end, start, (elapsedTime / moveAttackSpeed));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            brawler.position = start;
        }
    }
}
