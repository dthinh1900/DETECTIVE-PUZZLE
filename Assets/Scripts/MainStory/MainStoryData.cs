using System;
using System.Collections.Generic;

[Serializable]
public class MainStoryData
{
    public int chapterId;
    public string chapterName;
    public List<MainStoryCaseData> commonCases;
    public List<MainStoryBranchData> branches;
}

[Serializable]
public class MainStoryBranchData
{
    public string branchId;

    // Kết quả cố định của từng Branch trong Chapter 1.
    public string killerCharacterId;
    public string deadAfterCase1CharacterId;
    public string deadAfterCase2CharacterId;
    public string deadAfterCase3CharacterId;
    public string deadAfterCase4CharacterId;

    public List<MainStoryCaseData> cases;
}

[Serializable]
public class MainStoryCaseData
{
    public int caseNumber;
    public string caseId;
    public string title;
    public string summary;
    public List<MainStoryDialogueData> dialogues;
}

[Serializable]
public class MainStoryDialogueData
{
    public string speaker;
    public string text;
}
