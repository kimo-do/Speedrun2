using System;
using Deathbattle.Accounts;
using Lumberjack.Accounts;
using Solana.Unity.SDK;
using Solana.Unity.Wallet.Bip39;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

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
    public Button audioToggle;

    public Volume globalVolume;

    // Profile
    public Button initProfileButton;
    public Button profilePictureButton;
    public Image profilePictureImage;
    public TMP_InputField usernameInput;
    public TextMeshProUGUI pubKeyText;
    public TextMeshProUGUI errorText;

    private bool creatingProfile = false;
    private bool audioOn = true;
    private float loginTime;

    private ChromaticAberration chromaticAberration;
    private float pingPongDuration = 3f;

    void Start()
    {
        if (!Application.isEditor)
        {
            editorLoginButton.gameObject.SetActive(false);
        }

        globalVolume.profile.TryGet(out chromaticAberration);
       
        connectWalletScreen.gameObject.SetActive(true);
        createProfileScreen.gameObject.SetActive(false);

        editorLoginButton.onClick.AddListener(OnEditorLoginClicked);
        loginWalletAdapterButton.onClick.AddListener(OnLoginWalletAdapterButtonClicked);
        initProfileButton.onClick.AddListener(OnInitGameDataButtonClicked);
        audioToggle.onClick.AddListener(ToggleAudio);

        //BrawlAnchorService.OnPlayerDataChanged += OnPlayerDataChanged;
        BrawlAnchorService.OnProfileChanged += OnProfileChanged;
        BrawlAnchorService.OnInitialDataLoaded += OnInitialDataLoaded;

        StartCoroutine(ChromaticLoop());

        AudioManager.instance.menuMusic.Play();
    }

    private void ToggleAudio()
    {
        this.audioOn = !audioOn;
        AudioManager.instance.ToggleMute(this.audioOn);

        if (audioOn)
        {
            audioToggle.GetComponent<Image>().sprite = AudioManager.instance.unMutedSprite;
        }
        else
        {
            audioToggle.GetComponent<Image>().sprite = AudioManager.instance.mutedSprite;
        }
    }

    IEnumerator ChromaticLoop()
    {
        while (true)
        {
            float pingPong = Mathf.PingPong(Time.time / pingPongDuration, 0.5f); // Oscillates between 0 and 0.2
            chromaticAberration.intensity.value = 0.4f + pingPong; // Adjusts range to 0.4 to 0.6
            yield return null; // Wait for the next frame
        }
    }

    private void OnProfileChanged(Profile profile)
    {
        Debug.Log("Profile data callback received");

        if (Web3.Account != null)
        {
            Debug.Log("Web3 account available");

            var isInitialized = BrawlAnchorService.Instance.IsInitialized();
            string isInit = isInitialized ? "was" : "not";

            Debug.Log($"BrawlAnchorservice {isInit} initialized at this time.");

            /*
            if (isInitialized)
            {
                AudioManager.instance.menuMusic.Stop();
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
            */
        }
    }

    private void OnInitialDataLoaded()
    {
        if (Web3.Account != null)
        {
            Debug.Log("Initial data load complete");

            SceneManager.LoadScene("LobbyScene");
            //var isInitialized = BrawlAnchorService.Instance.IsInitialized();

            //if (!isInitialized)
            //{
            //    loginTime = Time.time;
            //}
        }
        else
        {
            connectWalletScreen.gameObject.SetActive(true);
            createProfileScreen.gameObject.SetActive(false);
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
        //BrawlAnchorService.Instance.IsAnyBlockingProgress = true;

        var newMnemonic = new Mnemonic(WordList.English, WordCount.Twelve);

        // Dont use this one for production. Its only ment for editor login
        var account = await Web3.Instance.LoginInGameWallet("123456") ??
                      await Web3.Instance.CreateAccount(newMnemonic.ToString(), "123456");
    }
}
