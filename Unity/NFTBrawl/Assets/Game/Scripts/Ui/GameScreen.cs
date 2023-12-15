using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Frictionless;
using Lumberjack.Accounts;
using Solana.Unity.SDK;
using Services;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using Deathbattle.Accounts;

/// <summary>
/// This is the screen which handles the interaction with the anchor program.
/// It checks if there is a game account already and has a button to call a function in the program.
/// </summary>
public class GameScreen : MonoBehaviour
{
    public static GameScreen instance;

    [Header("Screens")]
    public GraveyardController graveyardScreen;
    public ProfileController profileScreen;
    public BrawlController brawlScreen;

    [Header("Misc")]
    public Button ChuckWoodSessionButton;
    public Button NftsButton;
    public Button InitGameDataButton;

    public TextMeshProUGUI EnergyAmountText;
    public TextMeshProUGUI WoodAmountText;
    public TextMeshProUGUI NextEnergyInText;
    public TextMeshProUGUI TotalLogAvailableText;
    public TextMeshProUGUI PubKeyText;
    public TMP_InputField usernameInput;

    public GameObject NotInitializedRoot;
    public GameObject InitializedRoot;
    public GameObject ActionFx;
    public GameObject ActionFxPosition;
    public GameObject Tree;

    [Header("Prefabs")]
    public GameObject brawlerPfb;
    
    private Vector3 CharacterStartPosition;
    private PlayerData currentPlayerData;
    private CloneLab currentCloneLab;

    public Action<int> FalledBrawlersUpdated;
    public Action<BrawlerData> BrawlerRetrieved;

    private List<BrawlerData> myBrawlers = new();

    public List<BrawlerData> MyBrawlers { get => myBrawlers; set => myBrawlers = value; }

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        DisableAllScreens();

        profileScreen.Toggle(true);
        //ChuckWoodSessionButton.onClick.AddListener(OnChuckWoodSessionButtonClicked);
        //NftsButton.onClick.AddListener(OnNftsButtonClicked);
        //InitGameDataButton.onClick.AddListener(OnInitGameDataButtonClicked);
        //CharacterStartPosition = ChuckWoodSessionButton.transform.localPosition;
        // In case we are not logged in yet load the LoginScene
        if (Web3.Account == null)
        {
            //SceneManager.LoadScene("LoginScene");
            return;
        }
        //StartCoroutine(UpdateNextEnergy());
        
        //BrawlAnchorService.OnPlayerDataChanged += OnPlayerDataChanged;
        //BrawlAnchorService.OnInitialDataLoaded += UpdateContent;
        BrawlAnchorService.OnCloneLabChanged += OnGameDataChanged;
    }

    public void OpenLab()
    {
        DisableAllScreens();
        graveyardScreen.Toggle(true);
    }

    private void DisableAllScreens()
    {
        profileScreen.Toggle(false);
        graveyardScreen.Toggle(false);
        brawlScreen.Toggle(false);
    }

    private void OnDestroy()
    {
        //BrawlAnchorService.OnPlayerDataChanged -= OnPlayerDataChanged;
        //BrawlAnchorService.OnInitialDataLoaded -= UpdateContent;
        BrawlAnchorService.OnCloneLabChanged -= OnGameDataChanged;
    }

    private void OnEnable()
    {
        StartCoroutine(UpdateNextEnergy());
    }

    private async void OnInitGameDataButtonClicked()
    {
        // On local host we probably dont have the session key progeam, but can just sign with the in game wallet instead. 
        await BrawlAnchorService.Instance.InitAccounts(!Web3.Rpc.NodeAddress.AbsoluteUri.Contains("localhost"), usernameInput.text);
    }

    private void OnNftsButtonClicked()
    {
        ServiceFactory.Resolve<UiService>().OpenPopup(UiService.ScreenType.NftListPopup, new NftListPopupUiData(false, Web3.Wallet));
    }

    private IEnumerator UpdateNextEnergy()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            UpdateContent();
        }
    }

    private void OnPlayerDataChanged(PlayerData playerData)
    {
        if (currentPlayerData != null && currentPlayerData.Wood < playerData.Wood)
        {
            ChuckWoodSessionButton.transform.DOLocalMove(CharacterStartPosition, 0.2f);
        }

        currentPlayerData = playerData;
        UpdateContent();
    }

    private void OnGameDataChanged(CloneLab cloneLab)
    {
        /*
        if (currentGameData != null && currentGameData.TotalWoodCollected != cloneLab.TotalWoodCollected)
        {
            Tree.transform.DOKill();
            Tree.transform.localScale = Vector3.one;
            Tree.transform.DOPunchScale(Vector3.one * 0.1f, 0.1f);
            Instantiate(ActionFx, ActionFxPosition.transform.position, Quaternion.identity);
        }

        var totalLogAvailable = BrawlAnchorService.MAX_WOOD_PER_TREE - cloneLab.TotalWoodCollected;
        TotalLogAvailableText.text = totalLogAvailable + " Wood available.";
        currentGameData = cloneLab;
        */
        currentCloneLab = cloneLab;
    }

    private void UpdateContent()
    {
        return;

        var isInitialized = BrawlAnchorService.Instance.IsInitialized();
        NotInitializedRoot.SetActive(!isInitialized);
        InitGameDataButton.gameObject.SetActive(!isInitialized && BrawlAnchorService.Instance.CurrentPlayerData == null);
        InitializedRoot.SetActive(isInitialized);

        if (BrawlAnchorService.Instance.CurrentPlayerData == null)
        {
            return;
        }
        
        var lastLoginTime = BrawlAnchorService.Instance.CurrentPlayerData.LastLogin;
        var timePassed = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - lastLoginTime;
        
        while (
            timePassed >= BrawlAnchorService.TIME_TO_REFILL_ENERGY &&
            BrawlAnchorService.Instance.CurrentPlayerData.Energy < BrawlAnchorService.MAX_ENERGY
        ) {
            BrawlAnchorService.Instance.CurrentPlayerData.Energy += 1;
            BrawlAnchorService.Instance.CurrentPlayerData.LastLogin += BrawlAnchorService.TIME_TO_REFILL_ENERGY;
            timePassed -= BrawlAnchorService.TIME_TO_REFILL_ENERGY;
        }

        var timeUntilNextRefill = BrawlAnchorService.TIME_TO_REFILL_ENERGY - timePassed;

        if (timeUntilNextRefill > 0)
        {
            NextEnergyInText.text = timeUntilNextRefill.ToString();
        }
        else
        {
            NextEnergyInText.text = "";
        }
        
        EnergyAmountText.text = BrawlAnchorService.Instance.CurrentPlayerData.Energy.ToString();
        WoodAmountText.text = BrawlAnchorService.Instance.CurrentPlayerData.Wood.ToString();
    }

    private void OnChuckWoodSessionButtonClicked()
    {
        ChuckWoodSessionButton.transform.localPosition = CharacterStartPosition;
        ChuckWoodSessionButton.transform.DOLocalMove(CharacterStartPosition + Vector3.up * 10, 0.3f);
        BrawlAnchorService.Instance.ChopTree(!Web3.Rpc.NodeAddress.AbsoluteUri.Contains("localhost"), () =>
        {
            // Do something with the result. The websocket update in onPlayerDataChanged will come a bit earlier
        });
    }

    public void AttemptCreateBrawler()
    {

    }

    public void AttemptReviveBrawler()
    {
        GraveyardController.Instance.SummonEffect();
    }
}
