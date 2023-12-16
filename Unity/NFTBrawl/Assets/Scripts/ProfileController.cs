using Solana.Unity.Wallet;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfileController : Window
{
    public RectTransform noBrawlers;
    public RectTransform openLobby;
    public GameObject yourBrawlers;

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
    }

    private void UpdateProfileView()
    {
        yourBrawlers.gameObject.SetActive(true);
        openLobby.gameObject.SetActive(false);
        joinOpenLobbyButton.gameObject.SetActive(false);
        joinOpenLobbyButton.interactable = false;

        if (gameObjects.Count > 0)
        {
            joinOpenLobbyButton.interactable = true;
        }

        if (GameScreen.instance.PendingLobby != null)
        {
            openLobby.gameObject.SetActive(true);
            joinOpenLobbyButton.gameObject.SetActive(true);
        }
    }

    void Start()
    {
        GameScreen.instance.BrawlersRetrieved += OnBrawlersUpdated;
        GameScreen.instance.PendingLobbyRetrieved += OnPendingLobbyFound;

        labButton1.onClick.AddListener(ClickedOpenLab);
        labButton2.onClick.AddListener(ClickedOpenLab);
        joinOpenLobbyButton.onClick.AddListener(ClickedJoinLobby);

        //PositionGameObjects();
        noBrawlers.gameObject.SetActive(true);
        yourBrawlers.gameObject.SetActive(true);
    }

    private void ClickedJoinLobby()
    {
        if (GameScreen.instance.PendingLobby != null)
        {

        }
    }

    private void OnPendingLobbyFound(PublicKey lobbyPubkey)
    {
        UpdateProfileView();
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

            foreach (BrawlerData myBrawler in GameScreen.instance.MyBrawlers)
            {
                GameObject newBrawler = Instantiate(GameScreen.instance.brawlerPfb, brawlerContainer);
                if (myBrawler != null)
                {
                    newBrawler.GetComponent<BrawlerCharacter>().SetBrawlerData(myBrawler);
                }
                gameObjects.Add(newBrawler);
            }

            PositionGameObjects();

            noBrawlers.gameObject.SetActive(false);
            yourBrawlers.gameObject.SetActive(true);
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
