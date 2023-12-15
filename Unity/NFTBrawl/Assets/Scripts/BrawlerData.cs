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
    }

    public enum BrawlerType
    {
        Saber = 0,
        Pistol = 1,
        Hack = 2,
    }
}
