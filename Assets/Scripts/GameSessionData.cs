using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CollectedEvidenceInfo
{
    public int caseNumber;
    public string evidenceId;
    public string evidenceName;
    public string description;
    public Sprite evidenceImage;
}

[Serializable]
public class StatementInfo
{
    public int chapterId;
    public string characterId;
    public string question;
    public string answer;
}

public static class GameSessionData
{
    private static readonly List<StatementInfo> collectedStatements = new List<StatementInfo>();
    private static readonly HashSet<string> completedStoryKeys = new HashSet<string>();
    private static readonly Dictionary<string, CollectedEvidenceInfo> collectedEvidence = new Dictionary<string, CollectedEvidenceInfo>();
    private static readonly Dictionary<int, List<SpawnedEvidenceSaveData>> spawnedByCase = new Dictionary<int, List<SpawnedEvidenceSaveData>>();

    public static string PendingSpecialEvidenceId { get; private set; }

    private static string EvidenceKey(int caseNumber, string id) => caseNumber + "_" + id;

    // ---------------- Statements ----------------
    public static void AddStatement(int chapterId, string characterId, string question, string answer)
    {
        if (chapterId <= 0 || string.IsNullOrWhiteSpace(characterId) || string.IsNullOrWhiteSpace(question) || string.IsNullOrWhiteSpace(answer)) return;
        if (collectedStatements.Exists(s => s.chapterId == chapterId && s.characterId == characterId && s.question == question)) return;
        collectedStatements.Add(new StatementInfo { chapterId = chapterId, characterId = characterId, question = question, answer = answer });
        AutoSave();
    }

    public static void AddStatement(string characterId, string question, string answer)
    {
        if (GameManager.Instance != null) AddStatement(GameManager.Instance.SelectedChapter, characterId, question, answer);
    }

    public static void UpsertStoryStatement(int chapterId, string characterId, string question, string answer)
    {
        if (chapterId <= 0 || string.IsNullOrWhiteSpace(characterId) || string.IsNullOrWhiteSpace(question) || string.IsNullOrWhiteSpace(answer)) return;
        StatementInfo old = collectedStatements.Find(s => s.chapterId == chapterId && string.Equals(s.characterId, characterId, StringComparison.OrdinalIgnoreCase) && string.Equals(s.question, question, StringComparison.OrdinalIgnoreCase));
        if (old != null) old.answer = answer;
        else collectedStatements.Add(new StatementInfo { chapterId = chapterId, characterId = characterId, question = question, answer = answer });
        AutoSave();
    }

    public static List<StatementInfo> GetStatementsByCharacter(int chapterId, string characterId) => collectedStatements.FindAll(s => s.chapterId == chapterId && s.characterId == characterId);
    public static List<StatementInfo> GetStatementsByCharacter(string characterId) => collectedStatements.FindAll(s => s.characterId == characterId);
    public static void ClearStatements() => collectedStatements.Clear();

    // ---------------- Story progress ----------------
    public static bool TryMarkStoryCompleted(string storyKey)
    {
        if (string.IsNullOrWhiteSpace(storyKey)) return false;
        bool added = completedStoryKeys.Add(storyKey);
        if (added) AutoSave();
        return added;
    }
    public static List<string> GetCompletedStoryKeys() => new List<string>(completedStoryKeys);

    // ---------------- Collected evidence ----------------
    public static bool IsEvidenceCollected(string evidenceId) => IsEvidenceCollected(CaseSessionData.CurrentCaseNumber, evidenceId);
    public static bool IsEvidenceCollected(int caseNumber, string evidenceId)
    {
        if (caseNumber < 1 || string.IsNullOrWhiteSpace(evidenceId)) return false;
        return collectedEvidence.ContainsKey(EvidenceKey(caseNumber, evidenceId));
    }

    // Dùng cho điều kiện Story cấp Chapter: vật chứng có thể đến từ bất kỳ Case đã chơi.
    public static bool IsEvidenceCollectedInAnyCase(string evidenceId)
    {
        if (string.IsNullOrWhiteSpace(evidenceId)) return false;

        foreach (CollectedEvidenceInfo evidence in collectedEvidence.Values)
        {
            if (evidence != null && evidence.evidenceId == evidenceId)
                return true;
        }

        return false;
    }

    public static bool CollectEvidence(EvidenceItemData evidence) => CollectEvidence(evidence, CaseSessionData.CurrentCaseNumber);
    public static bool CollectEvidence(EvidenceItemData evidence, int caseNumber)
    {
        if (evidence == null || caseNumber < 1 || string.IsNullOrWhiteSpace(evidence.evidenceId)) return false;
        string key = EvidenceKey(caseNumber, evidence.evidenceId);
        if (collectedEvidence.ContainsKey(key)) return false;
        collectedEvidence.Add(key, new CollectedEvidenceInfo
        {
            caseNumber = caseNumber,
            evidenceId = evidence.evidenceId,
            evidenceName = evidence.evidenceName,
            description = evidence.description,
            evidenceImage = evidence.evidenceImage
        });
        AutoSave();
        return true;
    }

    public static List<CollectedEvidenceInfo> GetCollectedEvidence() => new List<CollectedEvidenceInfo>(collectedEvidence.Values);
    public static List<CollectedEvidenceInfo> GetCollectedEvidence(int caseNumber)
    {
        List<CollectedEvidenceInfo> result = new List<CollectedEvidenceInfo>();
        foreach (CollectedEvidenceInfo evidence in collectedEvidence.Values)
            if (evidence.caseNumber == caseNumber) result.Add(evidence);
        return result;
    }
    public static List<string> GetCollectedEvidenceIds() => GetCollectedEvidenceIds(CaseSessionData.CurrentCaseNumber);
    public static List<string> GetCollectedEvidenceIds(int caseNumber)
    {
        List<string> result = new List<string>();
        foreach (CollectedEvidenceInfo e in collectedEvidence.Values) if (e.caseNumber == caseNumber) result.Add(e.evidenceId);
        return result;
    }
    public static int GetCollectedEvidenceCount(int caseNumber)
    {
        int count = 0;
        foreach (CollectedEvidenceInfo e in collectedEvidence.Values) if (e.caseNumber == caseNumber) count++;
        return count;
    }
    public static void ClearCollectedEvidence() { collectedEvidence.Clear(); PendingSpecialEvidenceId = null; }

    // ---------------- Spawned evidence and exact layout ----------------
    public static bool HasSavedSpawnedEvidence(int caseNumber) => spawnedByCase.ContainsKey(caseNumber) && spawnedByCase[caseNumber] != null && spawnedByCase[caseNumber].Count == 12;
    public static void SaveSpawnedEvidence(int caseNumber, List<SpawnedEvidenceSaveData> entries)
    {
        if (caseNumber < 1 || entries == null || entries.Count != 12) { Debug.LogError("[SAVE] Danh sách vật chứng phải có 12 phần tử."); return; }
        if (HasSavedSpawnedEvidence(caseNumber)) return;
        spawnedByCase[caseNumber] = CloneSpawnData(entries);
        AutoSave();
    }
    public static List<SpawnedEvidenceSaveData> GetSavedSpawnedEvidence(int caseNumber)
    {
        return HasSavedSpawnedEvidence(caseNumber) ? CloneSpawnData(spawnedByCase[caseNumber]) : null;
    }
    // Giữ tương thích với code cũ.
    public static void SaveSpawnedEvidenceIds(int caseNumber, List<string> ids)
    {
        if (ids == null || ids.Count != 12) return;
        List<SpawnedEvidenceSaveData> entries = new List<SpawnedEvidenceSaveData>();
        for (int i = 0; i < ids.Count; i++) entries.Add(new SpawnedEvidenceSaveData { evidenceId = ids[i], spawnPointIndex = i, localScale = Vector3.one });
        SaveSpawnedEvidence(caseNumber, entries);
    }
    public static List<string> GetSavedSpawnedEvidenceIds(int caseNumber)
    {
        List<SpawnedEvidenceSaveData> entries = GetSavedSpawnedEvidence(caseNumber);
        if (entries == null) return null;
        List<string> ids = new List<string>();
        foreach (SpawnedEvidenceSaveData e in entries) ids.Add(e.evidenceId);
        return ids;
    }
    public static void ClearSavedSpawnedEvidence() => spawnedByCase.Clear();

    private static List<SpawnedEvidenceSaveData> CloneSpawnData(List<SpawnedEvidenceSaveData> source)
    {
        List<SpawnedEvidenceSaveData> copy = new List<SpawnedEvidenceSaveData>();
        foreach (SpawnedEvidenceSaveData e in source)
        {
            if (e == null) continue;
            copy.Add(new SpawnedEvidenceSaveData { evidenceId = e.evidenceId, spawnPointIndex = e.spawnPointIndex, anchoredPosition = e.anchoredPosition, anchorMin = e.anchorMin, anchorMax = e.anchorMax, pivot = e.pivot, sizeDelta = e.sizeDelta, localScale = e.localScale });
        }
        return copy;
    }

    // ---------------- Special evidence ----------------
    public static void SetPendingSpecialEvidenceId(string evidenceId)
    {
        if (!string.IsNullOrWhiteSpace(evidenceId)) { PendingSpecialEvidenceId = evidenceId; AutoSave(); }
    }
    public static void ClearPendingSpecialEvidenceId() { PendingSpecialEvidenceId = null; AutoSave(); }

    // ---------------- Save / Restore ----------------
    public static void WriteToSave(GameSaveData data)
    {
        data.cases.Clear();
        foreach (KeyValuePair<int, List<SpawnedEvidenceSaveData>> pair in spawnedByCase)
            data.cases.Add(new CaseSaveData { caseNumber = pair.Key, spawnedEvidence = CloneSpawnData(pair.Value) });
        data.collectedEvidence.Clear();
        foreach (CollectedEvidenceInfo e in collectedEvidence.Values)
            data.collectedEvidence.Add(new CollectedEvidenceSaveData { caseNumber = e.caseNumber, evidenceId = e.evidenceId, evidenceName = e.evidenceName, description = e.description });
        data.statements.Clear();
        foreach (StatementInfo s in collectedStatements)
            data.statements.Add(new StatementSaveData { chapterId = s.chapterId, characterId = s.characterId, question = s.question, answer = s.answer });
        data.completedStoryKeys = GetCompletedStoryKeys();
        data.pendingSpecialEvidenceId = PendingSpecialEvidenceId;
    }

    public static void RestoreFromSave(GameSaveData data)
    {
        collectedStatements.Clear(); completedStoryKeys.Clear(); collectedEvidence.Clear(); spawnedByCase.Clear();
        PendingSpecialEvidenceId = data.pendingSpecialEvidenceId;
        if (data.statements != null) foreach (StatementSaveData s in data.statements) if (s != null) collectedStatements.Add(new StatementInfo { chapterId = s.chapterId, characterId = s.characterId, question = s.question, answer = s.answer });
        if (data.completedStoryKeys != null) foreach (string key in data.completedStoryKeys) if (!string.IsNullOrEmpty(key)) completedStoryKeys.Add(key);
        if (data.cases != null) foreach (CaseSaveData c in data.cases) if (c != null && c.spawnedEvidence != null && c.spawnedEvidence.Count == 12) spawnedByCase[c.caseNumber] = CloneSpawnData(c.spawnedEvidence);
        if (data.collectedEvidence != null) foreach (CollectedEvidenceSaveData e in data.collectedEvidence) if (e != null && !string.IsNullOrEmpty(e.evidenceId))
                    collectedEvidence[EvidenceKey(e.caseNumber, e.evidenceId)] = new CollectedEvidenceInfo { caseNumber = e.caseNumber, evidenceId = e.evidenceId, evidenceName = e.evidenceName, description = e.description, evidenceImage = null };
    }

    public static void ClearAllSessionData()
    {
        ClearStatements(); ClearCollectedEvidence(); ClearSavedSpawnedEvidence(); completedStoryKeys.Clear(); PendingSpecialEvidenceId = null;
    }

    private static void AutoSave()
    {
        if (GameManager.Instance != null && CaseSessionData.IsSessionStarted) GameManager.Instance.SaveGame();
    }
}

