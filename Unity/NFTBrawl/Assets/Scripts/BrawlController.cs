using Solana.Unity.Wallet;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

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

        BrawlerData winnerData = null;

        foreach (var brawler in allBrawlers)
        {
            if (brawler.publicKey != winner)
            {
                brawlerLineup.Enqueue(brawler);
            }
            else
            {
                winnerData = brawler;
            }
        }

        brawlerLineup.Enqueue(winnerData);

        if (playoutFightsRoutine != null)
        {
            StopCoroutine(playoutFightsRoutine);
        }

        playoutFightsRoutine = StartCoroutine(c_PlayoutFights());
    }

    private bool readyForFight = false;

    IEnumerator c_PlayoutFights()
    {
        InitCrowd(brawlerLineup.ToList());

        yield return new WaitForSeconds(1f);

        while(brawlerLineup.Count > 1)
        {
            readyForFight = false;

            BrawlerData fighter1 = brawlerLineup.Dequeue();
            BrawlerData fighter2 = brawlerLineup.Dequeue();

            RemoveFromCrowd(fighter1);
            RemoveFromCrowd(fighter2);

            InitFightSequence(fighter1, fighter2);

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
    }

    IEnumerator autoPlayFight()
    {
        bool isLeftWinner = false;

        if (this.winner.ToString() == this.firstBrawler.publicKey.ToString())
        {
            isLeftWinner = true;
        }
        else if (this.winner.ToString() == this.secondBrawler.publicKey.ToString())
        {
            isLeftWinner = false;
        }
        else
        {
            // Random winner
            isLeftWinner = UnityEngine.Random.value >= 0.5f;
        }

        yield return new WaitForSeconds(2f);

        yield return StartCoroutine(DoAttackVisuals(true, isLeftWinner));

        if (!isLeftWinner) 
        {
            yield return new WaitForSeconds(1f);

            yield return StartCoroutine(DoAttackVisuals(false, !isLeftWinner));
        }

        yield return new WaitForSeconds(2f);       
    }

    public void RemoveFromCrowd(BrawlerData data)
    {
        GameObject brawler = crowd.FirstOrDefault(b => b.GetComponent<BrawlerCharacter>().MyBrawlerData == data);

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

    public void InitFightSequence(BrawlerData firstBrawler, BrawlerData secondBrawler)
    {
        this.firstBrawler = firstBrawler;
        this.secondBrawler = secondBrawler;

        brawlerLeft.transform.position = leftOutside.position;
        brawlerRight.transform.position = rightOutside.position;

        brawlerLeft.GetComponent<BrawlerCharacter>().SetBrawlerData(this.firstBrawler);
        brawlerRight.GetComponent<BrawlerCharacter>().SetBrawlerData(this.secondBrawler);

        brawlerLeft.GetComponent<BrawlerCharacter>().SetDeath(false);
        brawlerRight.GetComponent<BrawlerCharacter>().SetDeath(false);

        if (fightSequence != null)
        {
            StopCoroutine(fightSequence);
        }

        fightSequence = StartCoroutine(FightSequence());
    }

    IEnumerator FightSequence()
    {
        MoveBothIntoBattlePositions();

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

    private void MoveBothIntoBattlePositions()
    {
        if (leftPlayerRoutine != null)
        {
            StopCoroutine(leftPlayerRoutine);
        }

        if (rightPlayerRoutine != null)
        {
            StopCoroutine(rightPlayerRoutine);
        }

        leftPlayerRoutine = StartCoroutine(MoveIntoBattleVisuals(0f, true));
        rightPlayerRoutine = StartCoroutine(MoveIntoBattleVisuals(0.3f, false));
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
