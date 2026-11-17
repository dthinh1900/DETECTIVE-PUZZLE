using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameSaveData
{
    public int saveVersion = 3;
    public int chapterId;
    public string selectedBranchId;
    public int currentCaseNumber; // Case cao nhất đã mở trong tiến trình cốt truyện.
    public int viewingCaseNumber; // Case đang xem trong Explore, có thể là Case cũ.
    public string currentSceneName;
    public float remainingTime;
    public bool hasSaveData;
    public string pendingSpecialEvidenceId;
    public int storyDialogueIndex;
    public bool finalChancePending; // Đã hoàn thành 4 vụ án và đang ở lượt điều tra cuối cùng.
    public List<CaseSaveData> cases = new List<CaseSaveData>();
    public List<CollectedEvidenceSaveData> collectedEvidence = new List<CollectedEvidenceSaveData>();
    public List<StatementSaveData> statements = new List<StatementSaveData>();
    public List<string> completedStoryKeys = new List<string>();
}

[Serializable]
public class CaseSaveData
{
    public int caseNumber;
    // Một Case chỉ có một hiện trường, chứa 12 vật chứng.
    public List<SpawnedEvidenceSaveData> spawnedEvidence = new List<SpawnedEvidenceSaveData>();
}

[Serializable]
public class SpawnedEvidenceSaveData
{
    public string evidenceId;
    public int spawnPointIndex;
    public Vector2 anchoredPosition;
    public Vector2 anchorMin;
    public Vector2 anchorMax;
    public Vector2 pivot;
    public Vector2 sizeDelta;
    public Vector3 localScale;
}

[Serializable]
public class CollectedEvidenceSaveData
{
    public int caseNumber;
    public string evidenceId;
    public string evidenceName;
    public string description;
}

[Serializable]
public class StatementSaveData
{
    public int chapterId;
    public string characterId;
    public string question;
    public string answer;
}
