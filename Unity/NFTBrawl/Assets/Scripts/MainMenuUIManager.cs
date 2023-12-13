using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuUIManager : MonoBehaviour
{
    public RectTransform lobbies_screen;
    public RectTransform main_menu_screen;

    // Lobbies
    public LobbyListing lobbyListingTemplate;

    private List<LobbyListing> listedLobbies = new();

    public void AddLobby(LobbyItem lobby)
    {
        LobbyListing listing = Instantiate(lobbyListingTemplate, lobbyListingTemplate.transform.parent);
        listing.lobbyTitle.text = lobby.title;
        listing.lobbyHost.text = $"Host: {lobby.host}";
        listing.playerCount.text = $"{lobby.currentPlayers}/{lobby.totalPlayers}";
        listing.priceValue.text = lobby.prizeRewardValue.ToString("0.##");
        listing.gameObject.SetActive(true);
        listedLobbies.Add(listing);
    }

    public void RefreshLobbies(List<LobbyItem> lobbies)
    {
        foreach (var listedLobby in listedLobbies)
        {
            Destroy(listedLobby.gameObject);
        }

        listedLobbies.Clear();

        foreach (var lobby in lobbies)
        {
            AddLobby(lobby);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public class LobbyItem
    {
        public string title;
        public string host;
        public int currentPlayers;
        public int totalPlayers;
        public Sprite prizeCurrencyIcon;
        public double prizeRewardValue;
    }
}
