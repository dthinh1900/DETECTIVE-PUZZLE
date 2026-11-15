using System;
using UnityEngine;

[Serializable]
public class CaseCharacterInfo
{
    public string characterId;
    public string characterName;

    [TextArea(2, 5)]
    public string characterDescription;

    public bool isDead;
}

[Serializable]
public class CaseCharacterListData
{
    public CaseCharacterInfo[] characters;
}