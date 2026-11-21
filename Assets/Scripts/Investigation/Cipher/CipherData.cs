using System;
using System.Collections.Generic;

[Serializable]
public class CipherChapterData
{
    public int chapterId;
    public List<CipherBranchData> branches;
}

[Serializable]
public class CipherBranchData
{
    public string branchId;
    public List<CipherEntryData> ciphers;
}

[Serializable]
public class CipherEntryData
{
    public string cipherId;
    // 0 = nhận ở Intro; 1 = nhận sau MainStory Case 1; 2 = sau Case 2; 3 = sau Case 3.
    public int caseNumber;
    public string title;
    public string description;
    public string targetCharacterId;
    // Đường dẫn Resources, không gồm "Resources/" và không có phần mở rộng.
    public string imageResourcePath;
    public string answer;
    public List<string> acceptedAnswers;
}
