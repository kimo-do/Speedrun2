using System.Collections;
using System.Collections.Generic;
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
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.B))
        {
            firstBrawler = new BrawlerData()
            {
                username = "X",
                characterType = BrawlerData.CharacterType.Female1,
                brawlerType = BrawlerData.BrawlerType.Hack,
            };

            secondBrawler = new BrawlerData()
            {
                username = "DaveR",
                characterType = BrawlerData.CharacterType.Male1,
                brawlerType = BrawlerData.BrawlerType.Saber,
            };

            InitFightSequence(firstBrawler, secondBrawler);
        }

        if (Input.GetKeyUp(KeyCode.H))
        {
            DoAttack(true);
        }

        if (Input.GetKeyUp(KeyCode.J))
        {
            DoAttack(false);
        }

        if (Input.GetKeyUp(KeyCode.K))
        {
            HideBattleResult();
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
                crowd[i].GetComponent<BrawlerCharacter>().SetBrawlerData(this.crowdDataList[i]);
                crowd[i].SetActive(true);
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

            leftPlayerRoutine = StartCoroutine(DoMeleeAttackVisuals(true));
        }
        else
        {
            if (rightPlayerRoutine != null)
            {
                StopCoroutine(rightPlayerRoutine);
            }

            rightPlayerRoutine = StartCoroutine(DoMeleeAttackVisuals(false));
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

    IEnumerator DoMeleeAttackVisuals(bool isLeftPlayer)
    {
        float elapsedTime = 0;
        bool shouldMove = false;

        Transform brawler = isLeftPlayer ? brawlerLeft.transform : brawlerRight.transform;

        BrawlerCharacter brawlerCharacter = brawler.GetComponent<BrawlerCharacter>();

        if (brawlerCharacter.MyBrawlerData.brawlerType == BrawlerData.BrawlerType.Saber)
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
