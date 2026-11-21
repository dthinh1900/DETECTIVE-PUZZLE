using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameSaveData
{
    public int saveVersion = 5;
    public int chapterId;
    public string selectedBranchId;
    public string selectedKillerCharacterId;
    public List<CharacterDeathSaveData> deadCharacters = new List<CharacterDeathSaveData>();
    public int currentCaseNumber; // Case cao nhất đã mở trong tiến trình cốt truyện.
    public int viewingCaseNumber; // Case đang xem trong Explore, có thể là Case cũ.
    public string currentSceneName;
    public float remainingTime;
    // Mức thời gian tối đa cho mỗi giai đoạn; giữ nguyên qua Continue.
    public float maxTimerPerStageSeconds;
    public bool hasSaveData;
    public string pendingSpecialEvidenceId;
    public int storyDialogueIndex;
    public bool finalChancePending; // Đã hoàn thành 4 vụ án và đang ở lượt điều tra cuối cùng.
    public List<CaseSaveData> cases = new List<CaseSaveData>();
    public List<CollectedEvidenceSaveData> collectedEvidence = new List<CollectedEvidenceSaveData>();
    // Lời khai người chơi đã thu thập khi thẩm vấn (không sửa JSON gốc).
    public List<StatementSaveData> statements = new List<StatementSaveData>();
    // Câu trả lời được Story đặc biệt cập nhật, lưu riêng và ưu tiên khi hiển thị.
    public List<StatementSaveData> storyStatementOverrides = new List<StatementSaveData>();
    // Các câu hỏi đã hỏi, dùng để khôi phục trạng thái và số lượt hỏi.
    public List<string> askedQuestionKeys = new List<string>();
    public List<string> completedStoryKeys = new List<string>();
    // Tiến trình thu thập và giải mật mã Chapter 1.
    public List<string> collectedCipherIds = new List<string>();
    public List<string> solvedCipherIds = new List<string>();
    public string activeCipherId;
}

[Serializable]
public class CharacterDeathSaveData
{
    public int chapterId;
    public string characterId;
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
