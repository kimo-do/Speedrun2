using Deathbattle.Accounts;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ProfileController : Window
{
    public RectTransform noBrawlers;
    public RectTransform openLobby;
    public GameObject yourBrawlers;
    public GameObject yourBrawlersContainer;
    public TMP_Text infoLabel;

    public List<GameObject> gameObjects; // List of GameObjects to layout
    public int columns = 5; // Number of columns in the grid
    public float horizontalSpacing = 1.5f; // Horizontal spacing between items
    public float verticalSpacing = 1.5f; // Vertical spacing between items
    public float startYPosition = 1.5f; // Vertical spacing between items
    public int maxItems = 9;
    public bool refresh;
    public Transform brawlerContainer;
    public Button labButton1;
    public Button labButton2;
    public Button joinOpenLobbyButton;
    public Button createNewBrawl;
    public bool isShowingProfile;

    public PublicKey AttemptedJoinLobby;

    public override void Awake()
    {
        base.Awake();
    }

    public override void Toggle(bool toggle)
    {
        base.Toggle(toggle);

        if (toggle)
        {
            UpdateProfileView();
        }
        else
        {
            yourBrawlers.gameObject.SetActive(false);
        }

        isShowingProfile = toggle;
    }

    private void UpdateProfileView()
    {
        yourBrawlers.gameObject.SetActive(true);
        openLobby.gameObject.SetActive(false);
        joinOpenLobbyButton.gameObject.SetActive(false);
        joinOpenLobbyButton.interactable = false;
        createNewBrawl.gameObject.SetActive(false);
        createNewBrawl.interactable = false;
        labButton2.interactable = false;


        if (GameScreen.instance.MyBrawlers.Count > 0)
        {
            if (AttemptedJoinLobby == null)
            {
                joinOpenLobbyButton.interactable = true;
                labButton2.interactable = true;
            }
            createNewBrawl.interactable = true;
        }

        if (AttemptedJoinLobby != null)
        {
            FetchLobbyCount();
            infoLabel.text = "Waiting for other players..";
        }
        else if (GameScreen.instance.PendingJoinableBrawls.Count > 0)
        {
            openLobby.gameObject.SetActive(true);
            joinOpenLobbyButton.gameObject.SetActive(true);
            infoLabel.text = "Pending brawl found:";
            labButton2.interactable = true;

        }
        else
        {
            labButton2.interactable = true;
            createNewBrawl.gameObject.SetActive(true);
            infoLabel.text = "There is no pending brawl.";
        }
    }

    private async void FetchLobbyCount()
    {
        if (AttemptedJoinLobby == null) return;

        Brawl awaitingBrawl = await BrawlAnchorService.Instance.FetchBrawl(AttemptedJoinLobby);

        if (awaitingBrawl != null)
        {
            if (AttemptedJoinLobby != null)
            {
                infoLabel.text = $"Waiting for other players.. {awaitingBrawl.Queue.Length}/8";
            }
        }
    }

    void Start()
    {
        GameScreen.instance.BrawlersRetrieved += OnBrawlersUpdated;
        GameScreen.instance.PendingLobbyRetrieved += OnPendingLobbyFound;
        GameScreen.instance.ActiveLobbyRetrieved += OnActiveLobbyFound;
        GameScreen.instance.EndedLobbyRetrieved += OnEndedLobbyFound;


        labButton1.onClick.AddListener(ClickedOpenLab);
        labButton2.onClick.AddListener(ClickedOpenLab);
        joinOpenLobbyButton.onClick.AddListener(ClickedJoinLobby);
        createNewBrawl.onClick.AddListener(ClickedCreateBrawl);

        //PositionGameObjects();
        noBrawlers.gameObject.SetActive(true);
        yourBrawlers.gameObject.SetActive(true);
    }



    private void ClickedCreateBrawl()
    {
        BrawlAnchorService.Instance.StartBrawl(OnBrawlCreated);
    }

    private void OnBrawlCreated()
    {

    }

    private void ClickedJoinLobby()
    {
        if (GameScreen.instance.PendingJoinableBrawls.Count > 0)
        {
            PublicKey myBrawler = null;

            if (GameScreen.instance.selectedCharacter != null)
            {
                myBrawler = GameScreen.instance.selectedCharacter.MyBrawlerData.brawlerKey;
            }
            else
            {
                myBrawler = GameScreen.instance.MyBrawlers[0].brawlerKey;
            }

            if (myBrawler != null)
            {
                GameScreen.instance.HoldWalletUpdates = true;
                AttemptedJoinLobby = GameScreen.instance.PendingJoinableBrawls[0];
                BrawlAnchorService.Instance.JoinBrawl(AttemptedJoinLobby, myBrawler);
                infoLabel.text = "Waiting for other players..";
                joinOpenLobbyButton.interactable = false;
                labButton2.interactable = false;
            }
            else
            {
                GameScreen.instance.ShowError("No brawlers available!", 2f);
            }
        }
        else
        {
            GameScreen.instance.ShowError("No pending lobby found!", 2f);
        }
    }

    private void OnPendingLobbyFound(PublicKey lobbyPubkey)
    {
        UpdateProfileView();
    }


    private void OnActiveLobbyFound(PublicKey lobbyPubkey)
    {
        // attempt to start all active lobbies found
        if (GameScreen.instance.ReadyToStartBrawls.Count > 0)
        {
            Debug.Log($"Attempting to start all ({GameScreen.instance.ReadyToStartBrawls.Count}) brawls.");

            foreach (var readyBrawl in GameScreen.instance.ReadyToStartBrawls)
            {
                if (AttemptedJoinLobby != null)
                {
                    if (AttemptedJoinLobby.ToString() == readyBrawl.ToString())
                    {
                        AttemptedJoinLobby = null;
                    }
                }

                if (!GameScreen.instance.AttemptedToStartBrawls.Contains(readyBrawl))
                {
                    GameScreen.instance.AttemptedToStartBrawls.Add(readyBrawl);
                    BrawlAnchorService.Instance.RunMatch(true, OnRunMatch, readyBrawl);
                }
            }
        }
        else
        {
            Debug.Log($"No active brawls found..");
        }
    }

    private void OnRunMatch()
    {

    }

    private void OnEndedLobbyFound(PublicKey endedLobby)
    {
        Debug.Log($"Found new ended lobbies, checking if I was a part of it..");

        if (GameScreen.instance.EndedBrawls.Count > 0)
        {
            FetchEndedBrawls(GameScreen.instance.EndedBrawls);
        }
    }

    private async void FetchEndedBrawls(List<PublicKey> endedBrawls)
    {
        foreach (var brawl in endedBrawls)
        {
            Brawl endedBrawl = await BrawlAnchorService.Instance.FetchBrawl(brawl);

            if (endedBrawl != null)
            {
                if (endedBrawl.Queue != null)
                {
                    foreach (var queuedBrawler in endedBrawl.Queue)
                    {
                        if (GameScreen.instance.MyBrawlersPubKeys.Any(pb => pb.ToString() == queuedBrawler.ToString()))
                        {
                            // I was in this ended match.
                            if (!GameScreen.instance.IsPlayingOutBattle)
                            {
                                GameScreen.instance.IsPlayingOutBattle = true;
                                FetchAllParticipatingBrawlers(brawl);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    private async void FetchAllParticipatingBrawlers(PublicKey brawl)
    {
        Brawl activeBrawl = await BrawlAnchorService.Instance.FetchBrawl(brawl);

        if (activeBrawl != null)
        {
            Debug.Log($"Fetched active brawl: {brawl.ToString()}, Players: {activeBrawl.Queue.Length}");
            Debug.Log($"Fetching all brawlers..");

            GameScreen.instance.ActiveGameWinner = activeBrawl.Winner;
            GameScreen.instance.ActivePlayingBrawl = brawl;

            List<Brawler> brawlers = await BrawlAnchorService.Instance.FetchAllBrawlersFromBrawl(activeBrawl);

            if (brawlers != null)
            {
                Debug.Log($"Fetched all brawlers from active brawl: {brawlers.Count}");

                GameScreen.instance.ActiveGameBrawlers = new();

                foreach (var br in brawlers)
                {
                    if (br != null)
                    {
                        int brawlIntType = (int)br.BrawlerType;
                        int charIntType = (int)br.CharacterType;
                        string brawlName = br.Name;

                        BrawlerData brawlerData = new BrawlerData()
                        {
                            brawlerType = (BrawlerData.BrawlerType)brawlIntType,
                            characterType = (BrawlerData.CharacterType)charIntType,
                            username = br.Name,
                            ownerKey = br.Owner,
                            brawlerKey = brawl,
                        };

                        GameScreen.instance.ActiveGameBrawlers.Add(brawlerData);

                        MainThreadDispatcher.Instance().Enqueue(GameScreen.instance.OpenBrawl);
                    }
                }
            }
        }
    }

    private void ClickedOpenLab()
    {
        GameScreen.instance.OpenLab();
    }

    private void OnBrawlersUpdated()
    {
        if (GameScreen.instance.MyBrawlers.Count > 0)
        {
            foreach (GameObject brawlerShown in gameObjects)
            {
                Destroy(brawlerShown);
            }

            gameObjects.Clear();

            foreach (BrawlerData myBrawler in GameScreen.instance.MyBrawlers)
            {
                if (gameObjects.Count < 9)
                {
                    GameObject newBrawler = Instantiate(GameScreen.instance.brawlerPfb, brawlerContainer);
                    if (myBrawler != null)
                    {
                        newBrawler.GetComponent<BrawlerCharacter>().SetBrawlerData(myBrawler);
                    }
                    gameObjects.Add(newBrawler);
                }
            }

            PositionGameObjects();

            if (isShowingProfile)
            {
                noBrawlers.gameObject.SetActive(false);
                yourBrawlers.gameObject.SetActive(true);

                if (AttemptedJoinLobby == null)
                {
                    joinOpenLobbyButton.interactable = true;
                    labButton2.interactable = true;
                }
                createNewBrawl.interactable = true;
            }
        }
    }

    private void Update()
    {
        if (refresh)
        {
            refresh = false;

            PositionGameObjects();
        }
    }

    void PositionGameObjects()
    {
        if (gameObjects.Count == 1)
        {
            // Center the single GameObject horizontally
            gameObjects[0].transform.position = new Vector3(0, startYPosition, gameObjects[0].transform.position.z);
        }
        else
        {
            int rowCount = Mathf.CeilToInt((float)gameObjects.Count / columns);
            float totalWidth = (columns - 1) * horizontalSpacing;
            Vector2 gridStart = new Vector2(-totalWidth / 2, startYPosition);

            for (int i = 0; i < gameObjects.Count; i++)
            {
                int row = i / columns;
                int column = i % columns;

                if (gameObjects[i] != null)
                {
                    // Calculate position for each GameObject
                    Vector2 position;
                    if (gameObjects.Count == 2)
                    {
                        // Special case for 2 GameObjects
                        position = new Vector2((column - 0.5f) * horizontalSpacing, startYPosition);
                    }
                    else
                    {
                        // Standard grid positioning, only horizontal centering
                        position = new Vector2(gridStart.x + column * horizontalSpacing, startYPosition - row * horizontalSpacing);
                    }

                    gameObjects[i].transform.position = new Vector3(position.x, position.y, gameObjects[i].transform.position.z);
                }
            }
        }
    }


}
