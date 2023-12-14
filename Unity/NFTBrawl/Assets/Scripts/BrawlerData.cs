using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrawlerData
{
    public CharacterType characterType;
    public string username;

    public enum CharacterType
    {
        Default = 0,
        Male1 = 1,
        Female1 = 2,
    }
}
