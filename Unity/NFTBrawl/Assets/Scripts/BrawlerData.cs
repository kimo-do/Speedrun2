using Solana.Unity.Wallet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrawlerData
{
    public CharacterType characterType;
    public BrawlerType brawlerType;
    public string username;
    public PublicKey ownerKey;
    public PublicKey brawlerKey;

    public enum CharacterType
    {
        Default = 0,
        Male1 = 1,
        Female1 = 2,
        Bonki = 3,
        SolBlaze = 4,
        Male2 = 5,
        Female2 = 6,
        Cop = 7,
        Gangster = 8,
    }

    public enum BrawlerType
    {
        Saber = 0,
        Pistol = 1,
        Hack = 2,
        Katana = 3,
        Virus = 4,
        Laser = 5,
    }
}
