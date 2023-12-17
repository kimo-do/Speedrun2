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
using Solana.Unity.Rpc.Models;
using Deathbattle.Program;
using Solana.Unity.Programs;
using Solana.Unity.Wallet;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Linq;

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
    public RectTransform initScreen;

    [Header("Misc")]
    public Button ChuckWoodSessionButton;
    public Button NftsButton;
    public Button InitGameDataButton;
    public Button initProfileButton;
    public Button copyWalletAddy;

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

    public TextMeshProUGUI errorMessage;
    public Animation errorMessageAnim;

    [Header("Prefabs")]
    public GameObject brawlerPfb;
    
    private Vector3 CharacterStartPosition;
    private PlayerData currentPlayerData;
    private CloneLab currentCloneLab;

    public Action<int> FalledBrawlersUpdated;
    public Action BrawlersRetrieved;
    //public Action<LobbyData> PendingLobbiesRetrieved;
    public Action<Solana.Unity.Wallet.PublicKey> PendingLobbyRetrieved;
    public Action<Solana.Unity.Wallet.PublicKey> ActiveLobbyRetrieved;
    public Action<Solana.Unity.Wallet.PublicKey> EndedLobbyRetrieved;

    private List<BrawlerData> myBrawlers = new();
    private List<PublicKey> myBrawlersPubKeys = new();

    private List<BrawlerData> activeGameBrawlers = new();
    private PublicKey activeGameWinner;
    private PublicKey activePlayingBrawl;

    private List<PublicKey> pendingJoinableBrawls = new();
    private List<PublicKey> readyToStartBrawls = new();
    private List<PublicKey> endedBrawls = new();
    private List<PublicKey> attemptedToStartBrawls = new();


    private bool initialSubcribed;
    private Coroutine errorRoutine;

    public List<BrawlerData> MyBrawlers { get => myBrawlers; set => myBrawlers = value; }
    public List<BrawlerData> ActiveGameBrawlers { get => activeGameBrawlers; set => activeGameBrawlers = value; }
    public PublicKey ActiveGameWinner { get => activeGameWinner; set => activeGameWinner = value; }

    public bool IsPlayingOutBattle { get; set; }
    public bool HoldWalletUpdates { get; set; }
    public List<PublicKey> PendingJoinableBrawls { get => pendingJoinableBrawls; set => pendingJoinableBrawls = value; }
    public List<PublicKey> ReadyToStartBrawls { get => readyToStartBrawls; set => readyToStartBrawls = value; }
    public List<PublicKey> EndedBrawls { get => endedBrawls; set => endedBrawls = value; }
    public List<PublicKey> MyBrawlersPubKeys { get => myBrawlersPubKeys; set => myBrawlersPubKeys = value; }
    public PublicKey ActivePlayingBrawl { get => activePlayingBrawl; set => activePlayingBrawl = value; }
    public List<PublicKey> AttemptedToStartBrawls { get => attemptedToStartBrawls; set => attemptedToStartBrawls = value; }

    // The PDAs
    public PublicKey BrawlerPDA;

    private void Awake()
    {
        instance = this;
    }

    public void CopyWallet()
    {
        if (Web3.Account != null)
        {
            GUIUtility.systemCopyBuffer = Web3.Account.PublicKey.ToString();
            ShowError("Wallet copied!", 2f);
        }
        else
        {
            ShowError("Something went wrong, no account found.", 2f);
        }
    }

    public void ShowError(string message, float duration)
    {
        if (errorRoutine != null)
        {
            StopCoroutine(errorRoutine);
        }

        errorRoutine = StartCoroutine(ShowErrorRoutine(message, duration));
    }

    IEnumerator ShowErrorRoutine(string message, float duration)
    {
        errorMessage.text = message;
        errorMessageAnim["errorpo"].time = 0;
        errorMessageAnim["errorpo"].speed = 1f;

        errorMessageAnim.Play("errorpo");

        yield return new WaitForSeconds(duration);

        errorMessageAnim["errorpo"].time = errorMessageAnim["errorpo"].length;
        errorMessageAnim["errorpo"].speed = -1f;

        errorMessageAnim.Play("errorpo");
    }

    void Start()
    {
        DisableAllScreens();

        profileScreen.Toggle(true);
        //ChuckWoodSessionButton.onClick.AddListener(OnChuckWoodSessionButtonClicked);
        //NftsButton.onClick.AddListener(OnNftsButtonClicked);
        InitGameDataButton.onClick.AddListener(OnInitGameDataButtonClicked);
        initProfileButton.onClick.AddListener(OnInitGameDataButtonClicked);
        copyWalletAddy.onClick.AddListener(CopyWallet);
        //CharacterStartPosition = ChuckWoodSessionButton.transform.localPosition;
        // In case we are not logged in yet load the LoginScene
        if (Web3.Account == null)
        {
            //SceneManager.LoadScene("LoginScene");
            return;
        }

        PubKeyText.text = Web3.Account.PublicKey.ToString();

        StartCoroutine(MonitorSession());
        
        //BrawlAnchorService.OnPlayerDataChanged += OnPlayerDataChanged;
        //BrawlAnchorService.OnInitialDataLoaded += UpdateContent;
        BrawlAnchorService.OnCloneLabChanged += OnCloneLabChanged;
        BrawlAnchorService.OnGraveyardChanged += OnGraveyardChanged;
        BrawlAnchorService.OnColosseumChanged += OnColosseumChanged;
        //BrawlAnchorService.OnCloneLabChanged += OnCloneLabChanged;
    }

    private IEnumerator MonitorSession()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            VerifySession();
        }
    }

    private void VerifySession()
    {
        var isInitialized = BrawlAnchorService.Instance.IsInitialized();

        if (!isInitialized)
        {
            if (!initScreen.gameObject.activeInHierarchy)
            {
                DisableAllScreens();
                initScreen.gameObject.SetActive(true);
            }
        }
        else if (initScreen.gameObject.activeInHierarchy)
        {
            DisableAllScreens();
            profileScreen.Toggle(true);
        }

        if (BrawlAnchorService.Instance.CurrentProfile == null)
        {
            Debug.Log("No profile retrieved, profile is null..");
        }
        else if (isInitialized)
        {
           // Debug.Log("We have an active profile!");

            if (!initialSubcribed)
            {
                initialSubcribed = true;
                Debug.Log("Subscribing to game updates..");
                BrawlAnchorService.Instance.SubscribeToUpdates();
            }
        }
    }

    private void OnColosseumChanged(Colosseum colosseum)
    {
        if (colosseum != null)
        {
            Debug.Log("Received colosseum update pending: " + colosseum.PendingBrawls.Length);
            Debug.Log("Received colosseum update active: " + colosseum.ActiveBrawls.Length);
            Debug.Log("Received colosseum update ended: " + colosseum.EndedBrawls.Length);

            if (colosseum.PendingBrawls.Length > 0)
            {
                pendingJoinableBrawls = new(colosseum.PendingBrawls);
                PendingLobbyRetrieved?.Invoke(pendingJoinableBrawls[0]);
            }
            else
            {
                profileScreen.createNewBrawl.gameObject.SetActive(true);
            }

            if (colosseum.ActiveBrawls.Length > 0)
            {
                readyToStartBrawls = new(colosseum.ActiveBrawls);
                ActiveLobbyRetrieved?.Invoke(readyToStartBrawls[0]);
            }

            if (colosseum.EndedBrawls.Length > 0)
            {
                endedBrawls = new(colosseum.EndedBrawls);
                EndedLobbyRetrieved?.Invoke(endedBrawls[0]);
            }
        }
    }

    //private async void FetchAllPendingBrawls(Colosseum colosseum)
    //{
    //    List<Brawl> pendingBrawls = await BrawlAnchorService.Instance.FetchAllPendingBrawls(colosseum);

    //    List<PublicKey> readyToStartBrawls = new();
    //    List<PublicKey> readyToJoinBrawls = new();

    //    if (pendingBrawls != null)
    //    {
    //        if (pendingBrawls.Count > 0)
    //        {
    //            foreach (var pendingBrawl in pendingBrawls)
    //            {
    //                if (pendingBrawl.Queue != null)
    //                {
    //                    if (pendingBrawl.Queue.Length == 8)
    //                    {
    //                        readyToStartBrawls.Add(pendingBrawl);
    //                    }
    //                    else if (pendingBrawl.Queue.Length < 8)
    //                    {
    //                        readyToJoinBrawls.Add(pendingBrawl);
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    foreach (var brawl in readyToStartBrawls)
    //    {
    //        // Start full pending brawls
    //    }

    //    pendingJoinableBrawls = new();

    //    Debug.Log($"Finished fetching all pending brawls: {pendingBrawls.Count}");
    //    Debug.Log($"Ready to join brawls: {readyToJoinBrawls.Count}");
    //    Debug.Log($"Ready to start brawls: {readyToStartBrawls.Count}");

    //    if (pendingJoinableBrawls.Count > 0)
    //    {
    //        PendingLobby = pendingJoinableBrawls[0];
    //        PendingLobbyRetrieved?.Invoke(colosseum.PendingBrawls[0]);
    //    }

    //    //if (MyBrawlers.Count > 0)
    //    //{
    //    //    Debug.Log("MY BRAWLER:");
    //    //    Debug.Log($"-- {MyBrawlers[0].characterType}");
    //    //    Debug.Log($"-- {MyBrawlers[0].brawlerType}");
    //    //    Debug.Log($"-- {MyBrawlers[0].username}");
    //    //}

    //    BrawlersRetrieved?.Invoke();
    //}

    private void OnGraveyardChanged(Graveyard graveyard)
    {
        if (graveyard != null && graveyard.Brawlers != null)
        {
            Debug.Log("Received graveyard update: " + graveyard.Brawlers.Length);
            FalledBrawlersUpdated?.Invoke(graveyard.Brawlers.Length);
        }
    }

    void Update()
    {
        //if (Input.GetKeyUp(KeyCode.L))
        //{
        //    AudioManager.instance.PlayBattleMusic();
        //    DisableAllScreens();
        //    brawlScreen.Toggle(true);

        //    MyBrawlers[0].username = "DaveR";
        //    MyBrawlers[1].username = "Kimosabe";
        //    MyBrawlers[2].username = "Acammm";
        //    MyBrawlers[3].username = "Blockiosaurus";
        //    MyBrawlers[4].username = "Alfie";
        //    MyBrawlers[5].username = "Eidlucie";
        //    MyBrawlers[6].username = "Anatoly";
        //    MyBrawlers[7].username = "Raj";

        //    brawlScreen.PlayOutFights(MyBrawlers.Take(8).ToList(), MyBrawlers[UnityEngine.Random.Range(0, MyBrawlers.Count)].brawlerKey, MyBrawlers[0].brawlerKey);
        //}

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 rayOrigin = SuperCamera.instance.Camera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.zero);

            if (hit.collider != null)
            {
                if (hit.transform.TryGetComponent(out BrawlerCharacter character))
                {
                    if (MyBrawlers.Contains(character.MyBrawlerData))
                    {
                        if (profileScreen.isShowingProfile)
                        {
                            if (profileScreen.AttemptedJoinLobby == null)
                            {
                                ClickedBrawler(character);
                            }
                        }
                    }
                }
            }
        }
    }

    public BrawlerCharacter selectedCharacter;

    private void ClickedBrawler(BrawlerCharacter character)
    {
        foreach (var brawl in profileScreen.gameObjects)
        {
            brawl.GetComponent<BrawlerCharacter>().ToggleSelected(false);
        }

        character.ToggleSelected(true);

        selectedCharacter = character;
    }

    public void OpenLab()
    {
        IsPlayingOutBattle = false;
        DisableAllScreens();
        graveyardScreen.Toggle(true);
    }

    public void OpenProfile()
    {
        HoldWalletUpdates = false;
        IsPlayingOutBattle = false;
        DisableAllScreens();
        profileScreen.Toggle(true);
    }

    public void OpenBrawl()
    {
        if (ActiveGameBrawlers.Count > 0)
        {
            BrawlerData myBrawlerEntry = ActiveGameBrawlers.First(b => b.ownerKey == Web3.Account.PublicKey);

            if (myBrawlerEntry != null)
            {
                AudioManager.instance.PlayBattleMusic();
                DisableAllScreens();
                brawlScreen.Toggle(true);
                brawlScreen.PlayOutFights(ActiveGameBrawlers.Take(8).ToList(), ActiveGameWinner, myBrawlerEntry.brawlerKey);
            }
            else
            {
                ShowError("Trying view a brawl but you don't own any of the participants.", 4f);
            }
        }
        else
        {
            ShowError("Trying to start all brawl with no participants", 4f);
        }
    }

    private void DisableAllScreens()
    {
        profileScreen.Toggle(false);
        graveyardScreen.Toggle(false);
        brawlScreen.Toggle(false);
        initScreen.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        //BrawlAnchorService.OnPlayerDataChanged -= OnPlayerDataChanged;
        //BrawlAnchorService.OnInitialDataLoaded -= UpdateContent;
        BrawlAnchorService.OnCloneLabChanged -= OnCloneLabChanged;
        BrawlAnchorService.OnGraveyardChanged -= OnGraveyardChanged;
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

    private void OnCloneLabChanged(CloneLab cloneLab)
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

        if (currentCloneLab != null)
        {
            Debug.Log($"Received clonelab update, brawlers: {currentCloneLab.Brawlers.Length}, number: {currentCloneLab.NumBrawlers}");

            if (currentCloneLab.Brawlers.Length > 0)
            {
                Debug.Log($"Attempting to fetch all brawlers..");
                FetchAllNewBrawlers(currentCloneLab.Brawlers);
            }
        }
    }

    private async void FetchAllNewBrawlers(PublicKey[] newBrawlers)
    {
        foreach (var brawl in currentCloneLab.Brawlers)
        {
            if (!myBrawlersPubKeys.Contains(brawl))
            {
                myBrawlersPubKeys.Add(brawl);

                Brawler fetchedBrawler = await BrawlAnchorService.Instance.FetchBrawler(brawl);

                if (fetchedBrawler != null)
                {
                    int brawlIntType = (int)fetchedBrawler.BrawlerType;
                    int charIntType = (int)fetchedBrawler.CharacterType;
                    string brawlName = fetchedBrawler.Name;

                    BrawlerData brawlerData = new BrawlerData()
                    {
                        brawlerType = (BrawlerData.BrawlerType)brawlIntType,
                        characterType = (BrawlerData.CharacterType)charIntType,
                        username = fetchedBrawler.Name,
                        ownerKey = fetchedBrawler.Owner,
                        brawlerKey = brawl,
                    };

                    myBrawlers.Add(brawlerData);
                }
            }
        }

        Debug.Log($"Finished fetching all brawlers: {MyBrawlers.Count}");

        //if (MyBrawlers.Count > 0)
        //{
        //    Debug.Log("MY BRAWLER:");
        //    Debug.Log($"-- {MyBrawlers[0].characterType}");
        //    Debug.Log($"-- {MyBrawlers[0].brawlerType}");
        //    Debug.Log($"-- {MyBrawlers[0].username}");
        //}

        BrawlersRetrieved?.Invoke();
    }

    //private async void FetchAllPendingBrawls(Colosseum colosseum)
    //{
    //    if (colosseum == null) return;
    //    if (colosseum.PendingBrawls.Length < 1) return;

    //    List<Brawl> pendingBrawls = await BrawlAnchorService.Instance.FetchAllPendingBrawls(colosseum);

    //    if (pendingBrawls != null)
    //    {
    //        Debug.Log($"Finished fetching all pending brawls: {pendingBrawls.Count}");
    //        if (pendingBrawls.Count > 0)
    //        {
    //            Brawl firstLobby = pendingBrawls[0];

    //            LobbyData lobbyData = new LobbyData()
    //            {
    //                firstLobby.
    //            };
    //            //PendingLobbiesRetrieved?.Invoke()

    //        }
    //    }

    //    foreach (var brawl in currentCloneLab.Brawlers)
    //    {
    //        if (!myBrawlersPubKeys.Contains(brawl))
    //        {
    //            myBrawlersPubKeys.Add(brawl);

    //            Brawler fetchedBrawler = await BrawlAnchorService.Instance.FetchBrawler(brawl);

    //            if (fetchedBrawler != null)
    //            {
    //                int brawlIntType = (int)fetchedBrawler.BrawlerType;
    //                int charIntType = (int)fetchedBrawler.CharacterType;
    //                string brawlName = fetchedBrawler.Name;

    //                BrawlerData brawlerData = new BrawlerData()
    //                {
    //                    brawlerType = (BrawlerData.BrawlerType)brawlIntType,
    //                    characterType = (BrawlerData.CharacterType)charIntType,
    //                    username = fetchedBrawler.Name,
    //                    publicKey = fetchedBrawler.Owner,
    //                };

    //                myBrawlers.Add(brawlerData);
    //            }
    //        }
    //    }

    //    Debug.Log($"Finished fetching all brawlers: {MyBrawlers.Count}");

    //    //if (MyBrawlers.Count > 0)
    //    //{
    //    //    Debug.Log("MY BRAWLER:");
    //    //    Debug.Log($"-- {MyBrawlers[0].characterType}");
    //    //    Debug.Log($"-- {MyBrawlers[0].brawlerType}");
    //    //    Debug.Log($"-- {MyBrawlers[0].username}");
    //    //}

    //    BrawlersRetrieved?.Invoke();
    //}

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
        AttemptCreateAsync();
    }

    public void AttemptReviveBrawler()
    {
        AttemptReviveAsync();
    }

    private double solBalance = 0;

    private async void AttemptCreateAsync()
    {
        solBalance = await Web3.Instance.WalletBase.GetBalance();

        MainThreadDispatcher.Instance().Enqueue(ContinueCreateBrawler);
    }

    private async void AttemptReviveAsync()
    {
        solBalance = await Web3.Instance.WalletBase.GetBalance();

        MainThreadDispatcher.Instance().Enqueue(ContinueReviveBrawler);
    }

    private void ContinueCreateBrawler()
    {
        if (BrawlAnchorService.Instance.CurrentProfile != null)
        {
            if (solBalance >= 0.1)
            {
                BrawlAnchorService.Instance.CreateBrawler(!Web3.Rpc.NodeAddress.AbsoluteUri.Contains("localhost"), () =>
                {
                    // Do something with the result. The websocket update in onPlayerDataChanged will come a bit earlier
                    GraveyardController.Instance.SummonEffect();
                    Debug.Log("Created a brawler!");
                });
            }
            else
            {
                ShowError("Insufficient Sol Balance!", 2f);
            }
        }
        else
        {
            ShowError("No user profile found!", 2f);
        }
    }

    private void ContinueReviveBrawler()
    {
        if (BrawlAnchorService.Instance.CurrentProfile != null)
        {
            if (solBalance >= 0.05)
            {
                BrawlAnchorService.Instance.ReviveBrawler(!Web3.Rpc.NodeAddress.AbsoluteUri.Contains("localhost"), () =>
                {
                    // Do something with the result. The websocket update in onPlayerDataChanged will come a bit earlier
                    GraveyardController.Instance.SummonEffect();
                    Debug.Log("Revived a brawler!");
                });
            }
            else
            {
                ShowError("Insufficient Sol Balance!", 2f);
            }
        }
        else
        {
            ShowError("No user profile found!", 2f);
        }
    }
}
