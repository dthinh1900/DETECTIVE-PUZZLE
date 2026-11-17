
using System;
using System.Collections.Generic;

[Serializable]
public class MainStoryData
{
    public int chapterId;
    public string chapterName;

    // Nội dung dùng chung cho cả 3 Branch: Vụ án 1, 2 và hội thoại cơ hội cuối số 5.
    public List<MainStoryCaseData> commonCases;

    // Nội dung riêng của từng Branch.
    public List<MainStoryBranchData> branches;
}

[Serializable]
public class MainStoryBranchData
{
    public string branchId;
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
