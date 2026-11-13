
using System;

[Serializable]
public class CaseCharacterInfo
{
    public string characterId;
    public string characterName;
}

[Serializable]
public class CaseCharacterListData
{
    public CaseCharacterInfo[] characters;
}
