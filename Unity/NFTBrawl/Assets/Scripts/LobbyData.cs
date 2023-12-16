using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyData
{
    public LobbyType lobbyType;
    public string hostName;
    public int currentPlayers;
    public int maxPlayers;
    public double prizeAmount;
    public string prizeTicker;

    public enum LobbyType
    {
        Public = 0,
        Private = 1,
        Gated = 2,
    }
}
