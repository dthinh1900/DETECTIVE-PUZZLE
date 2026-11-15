
using System;
using System.Collections.Generic;

[Serializable]
public class ChapterData
{
    public string chapterId;
    public List<CaseItemData> items;
    public List<CaseBranchData> branches;
}

[Serializable]
public class CaseItemData
{
    public string itemId;
    public string itemName;
    public string description;
    public string decoyDetail;
}

[Serializable]
public class CaseBranchData
{
    public string branchId;
    public List<CorrectEvidenceData> correctEvidence;
}

[Serializable]
public class CorrectEvidenceData
{
    public string itemId;
    public string killerInfo;
}
