using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrawlerData
{
    public CharacterType characterType;
    public BrawlerType brawlerType;
    public string username;

    public enum CharacterType
    {
        Default = 0,
        Male1 = 1,
        Female1 = 2,
        Bonki = 3,
        SolBlaze = 4,
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
