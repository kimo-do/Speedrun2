using System;
using Lumberjack.Accounts;
using Solana.Unity.SDK;
using Solana.Unity.Wallet.Bip39;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Handles the connection to the players wallet.
/// </summary>
public class LoginScreen : MonoBehaviour
{
    // Screens
    public RectTransform connectWalletScreen;
    public RectTransform createProfileScreen;

    public Button editorLoginButton;
    public Button loginWalletAdapterButton;

    // Profile
    public Button initProfileButton;
    public Button profilePictureButton;
    public Image profilePictureImage;
    public TMP_InputField usernameInput;
    public TextMeshProUGUI pubKeyText;
    public TextMeshProUGUI errorText;

    private bool creatingProfile = false;
    private float loginTime;

    void Start()
    {
        if (!Application.isEditor)
        {
            editorLoginButton.gameObject.SetActive(false);
        }

        connectWalletScreen.gameObject.SetActive(true);
        createProfileScreen.gameObject.SetActive(false);

        editorLoginButton.onClick.AddListener(OnEditorLoginClicked);
        loginWalletAdapterButton.onClick.AddListener(OnLoginWalletAdapterButtonClicked);
        initProfileButton.onClick.AddListener(OnInitGameDataButtonClicked);

        BrawlAnchorService.OnPlayerDataChanged += OnPlayerDataChanged;
        BrawlAnchorService.OnInitialDataLoaded += UpdateContent;
        BrawlAnchorService.OnInitialDataLoaded += OnInitialDataLoaded;
    }

    private void OnInitialDataLoaded()
    {
        if (Web3.Account != null)
        {
            var isInitialized = BrawlAnchorService.Instance.IsInitialized();

            if (isInitialized)
            {
                SceneManager.LoadScene("LobbyScene");
            }
            else if (!creatingProfile)
            {
                creatingProfile = true;
                connectWalletScreen.gameObject.SetActive(false);
                createProfileScreen.gameObject.SetActive(true);
                pubKeyText.text = Web3.Account.PublicKey;
                usernameInput.text = "";
            }
        }
    }

    private void OnDestroy()
    {
        BrawlAnchorService.OnPlayerDataChanged -= OnPlayerDataChanged;
        BrawlAnchorService.OnInitialDataLoaded -= UpdateContent;
    }

    private async void OnLoginWalletAdapterButtonClicked()
    {
        await Web3.Instance.LoginWalletAdapter();
    }

    private async void OnInitGameDataButtonClicked()
    {
        errorText.text = "";

        if (usernameInput.text.Length < 3)
        {
            errorText.text = "Username must be at least 3 characters.";
            return;
        }

        // On local host we probably dont have the session key progeam, but can just sign with the in game wallet instead. 
        await BrawlAnchorService.Instance.InitAccounts(!Web3.Rpc.NodeAddress.AbsoluteUri.Contains("localhost"), usernameInput.text);
    }

    private void OnPlayerDataChanged(PlayerData playerData)
    {
        UpdateContent();
    }

    private void UpdateContent()
    {
        if (Web3.Account != null)
        {
            var isInitialized = BrawlAnchorService.Instance.IsInitialized();

            if (!isInitialized)
            {
                loginTime = Time.time;
            }
        }
        else
        {
            connectWalletScreen.gameObject.SetActive(true);
            createProfileScreen.gameObject.SetActive(false);
        }
    }

    private async void OnEditorLoginClicked()
    {
        var newMnemonic = new Mnemonic(WordList.English, WordCount.Twelve);

        // Dont use this one for production. Its only ment for editor login
        var account = await Web3.Instance.LoginInGameWallet("1234") ??
                      await Web3.Instance.CreateAccount(newMnemonic.ToString(), "1234");
    }
}
