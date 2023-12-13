using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ProfileController : MonoBehaviour
{
    public RectTransform noBrawlers;
    public GameObject yourBrawlers;

    public List<GameObject> gameObjects; // List of GameObjects to layout
    public int columns = 5; // Number of columns in the grid
    public float horizontalSpacing = 1.5f; // Horizontal spacing between items
    public float verticalSpacing = 1.5f; // Vertical spacing between items
    public float startYPosition = 1.5f; // Vertical spacing between items
    public int maxItems = 9;
    public bool refresh;
    public Transform brawlerContainer;

    void Start()
    {
        GameScreen.instance.BrawlerRetrieved += OnBrawlersUpdated;

        //PositionGameObjects();
        noBrawlers.gameObject.SetActive(true);
        //yourBrawlers.gameObject.SetActive(false);
    }

    private void OnBrawlersUpdated(Brawler brawler)
    {
        if (GameScreen.instance.MyBrawlers.Count > 0)
        {
            foreach (GameObject brawlerShown in gameObjects)
            {
                Destroy(brawlerShown);
            }

            foreach (Brawler myBrawler in GameScreen.instance.MyBrawlers)
            {
                GameObject newBrawler = Instantiate(GameScreen.instance.brawlerPfb, brawlerContainer);
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

    public class Brawler
    {
        public CharacterType characterType;

        public enum CharacterType
        {
            Default = 0,
        }
    }
}
