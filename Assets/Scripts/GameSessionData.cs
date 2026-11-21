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
    private static readonly List<StatementInfo> storyStatementOverrides = new List<StatementInfo>();
    private static readonly HashSet<string> askedQuestionKeys = new HashSet<string>();
    private static readonly HashSet<string> completedStoryKeys = new HashSet<string>();
    private static readonly Dictionary<string, CollectedEvidenceInfo> collectedEvidence = new Dictionary<string, CollectedEvidenceInfo>();
    private static readonly Dictionary<int, List<SpawnedEvidenceSaveData>> spawnedByCase = new Dictionary<int, List<SpawnedEvidenceSaveData>>();
    private static readonly List<CharacterDeathSaveData> deadCharacters = new List<CharacterDeathSaveData>();
    private static readonly HashSet<string> collectedCipherIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> solvedCipherIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public static string PendingSpecialEvidenceId { get; private set; }
    public static string ActiveCipherId { get; private set; }

    // ---------------- Cipher progress ----------------
    public static bool IsCipherCollected(string cipherId)
    {
        return !string.IsNullOrWhiteSpace(cipherId) && collectedCipherIds.Contains(cipherId.Trim());
    }

    public static bool IsCipherSolved(string cipherId)
    {
        return !string.IsNullOrWhiteSpace(cipherId) && solvedCipherIds.Contains(cipherId.Trim());
    }

    // Mật mã mới nhất trở thành mật mã hiện tại; các mật mã trước đó chỉ còn xem lại.
    public static bool CollectCipher(string cipherId)
    {
        if (string.IsNullOrWhiteSpace(cipherId))
            return false;

        string id = cipherId.Trim();
        if (!collectedCipherIds.Add(id))
            return false;

        ActiveCipherId = id;
        Debug.Log("[CIPHER] Đã thu thập mật mã: " + id);
        AutoSave();
        return true;
    }

    public static bool MarkCipherSolved(string cipherId)
    {
        if (string.IsNullOrWhiteSpace(cipherId) ||
            !IsCipherCollected(cipherId) ||
            !string.Equals(ActiveCipherId, cipherId.Trim(), StringComparison.OrdinalIgnoreCase))
            return false;

        bool added = solvedCipherIds.Add(cipherId.Trim());
        if (added)
        {
            Debug.Log("[CIPHER] Đã giải đúng mật mã: " + cipherId);
            AutoSave();
        }
        return added;
    }

    public static List<string> GetCollectedCipherIds()
    {
        return new List<string>(collectedCipherIds);
    }

    public static List<string> GetSolvedCipherIds()
    {
        return new List<string>(solvedCipherIds);
    }

    private static void ClearCipherProgress()
    {
        collectedCipherIds.Clear();
        solvedCipherIds.Clear();
        ActiveCipherId = null;
    }

    private static string EvidenceKey(int caseNumber, string id) => caseNumber + "_" + id;

    // ---------------- Character fate / death state ----------------
    public static bool IsCharacterDead(int chapterId, string characterId)
    {
        if (chapterId <= 0 || string.IsNullOrWhiteSpace(characterId))
            return false;

        return deadCharacters.Exists(d => d != null && d.chapterId == chapterId &&
            string.Equals(d.characterId, characterId, StringComparison.OrdinalIgnoreCase));
    }

    public static bool MarkCharacterDead(int chapterId, string characterId)
    {
        if (chapterId <= 0 || string.IsNullOrWhiteSpace(characterId) ||
            IsCharacterDead(chapterId, characterId))
            return false;

        deadCharacters.Add(new CharacterDeathSaveData
        {
            chapterId = chapterId,
            characterId = characterId.Trim()
        });

        Debug.Log("[CHARACTER FATE] Đã ghi nhận nhân vật bị sát hại: " + characterId);
        AutoSave();
        return true;
    }

    public static List<CharacterDeathSaveData> GetDeadCharacters()
    {
        List<CharacterDeathSaveData> result = new List<CharacterDeathSaveData>();
        foreach (CharacterDeathSaveData death in deadCharacters)
        {
            if (death == null) continue;
            result.Add(new CharacterDeathSaveData
            {
                chapterId = death.chapterId,
                characterId = death.characterId
            });
        }
        return result;
    }

    public static void ClearCharacterDeaths()
    {
        deadCharacters.Clear();
    }

    private static void AddCharacterDeathWithoutSave(int chapterId, string characterId)
    {
        if (chapterId <= 0 || string.IsNullOrWhiteSpace(characterId) || IsCharacterDead(chapterId, characterId))
            return;

        deadCharacters.Add(new CharacterDeathSaveData
        {
            chapterId = chapterId,
            characterId = characterId.Trim()
        });
    }

    // ---------------- Statements ----------------
    private static string QuestionKey(int chapterId, string characterId, string question)
    {
        return chapterId + "|" + (characterId ?? "").Trim().ToUpperInvariant() + "|" + (question ?? "").Trim().ToUpperInvariant();
    }

    public static bool IsQuestionAsked(int chapterId, string characterId, string question)
    {
        return askedQuestionKeys.Contains(QuestionKey(chapterId, characterId, question));
    }

    public static void MarkQuestionAsked(int chapterId, string characterId, string question)
    {
        if (chapterId <= 0 || string.IsNullOrWhiteSpace(characterId) || string.IsNullOrWhiteSpace(question)) return;
        if (askedQuestionKeys.Add(QuestionKey(chapterId, characterId, question))) AutoSave();
    }

    public static List<string> GetAskedQuestionKeys() => new List<string>(askedQuestionKeys);

    // Chỉ lưu câu trả lời người chơi thực sự đã nghe; không sửa dữ liệu JSON gốc.
    public static void AddStatement(int chapterId, string characterId, string question, string answer)
    {
        if (chapterId <= 0 || string.IsNullOrWhiteSpace(characterId) || string.IsNullOrWhiteSpace(question) || string.IsNullOrWhiteSpace(answer)) return;
        StatementInfo old = collectedStatements.Find(s => s.chapterId == chapterId && string.Equals(s.characterId, characterId, StringComparison.OrdinalIgnoreCase) && string.Equals(s.question, question, StringComparison.OrdinalIgnoreCase));
        if (old == null)
            collectedStatements.Add(new StatementInfo { chapterId = chapterId, characterId = characterId, question = question, answer = answer });
        else
            old.answer = answer;
        MarkQuestionAsked(chapterId, characterId, question);
        AutoSave();
    }

    public static void AddStatement(string characterId, string question, string answer)
    {
        if (GameManager.Instance != null) AddStatement(GameManager.Instance.SelectedChapter, characterId, question, answer);
    }

    // Lưu lớp ghi đè riêng. Không thay đổi collectedStatements hay JSON gốc.
    public static void UpsertStoryStatement(int chapterId, string characterId, string question, string answer)
    {
        if (chapterId <= 0 || string.IsNullOrWhiteSpace(characterId) || string.IsNullOrWhiteSpace(question) || string.IsNullOrWhiteSpace(answer)) return;
        StatementInfo old = storyStatementOverrides.Find(s => s.chapterId == chapterId && string.Equals(s.characterId, characterId, StringComparison.OrdinalIgnoreCase) && string.Equals(s.question, question, StringComparison.OrdinalIgnoreCase));
        if (old != null) old.answer = answer;
        else storyStatementOverrides.Add(new StatementInfo { chapterId = chapterId, characterId = characterId, question = question, answer = answer });
        AutoSave();
    }

    // Thứ tự ưu tiên: đáp án Story cập nhật > lời khai đã hỏi > JSON gốc.
    public static string GetEffectiveStatementAnswer(int chapterId, string characterId, string question, string originalAnswer)
    {
        StatementInfo update = storyStatementOverrides.Find(s => s.chapterId == chapterId && string.Equals(s.characterId, characterId, StringComparison.OrdinalIgnoreCase) && string.Equals(s.question, question, StringComparison.OrdinalIgnoreCase));
        if (update != null) return update.answer;
        StatementInfo asked = collectedStatements.Find(s => s.chapterId == chapterId && string.Equals(s.characterId, characterId, StringComparison.OrdinalIgnoreCase) && string.Equals(s.question, question, StringComparison.OrdinalIgnoreCase));
        return asked != null ? asked.answer : originalAnswer;
    }

    public static List<StatementInfo> GetStoryStatementOverrides(int chapterId, string characterId)
    {
        return storyStatementOverrides.FindAll(s => s.chapterId == chapterId && string.Equals(s.characterId, characterId, StringComparison.OrdinalIgnoreCase));
    }

    // Hồ sơ lời khai hiển thị hợp nhất: câu đã hỏi và câu được Story cập nhật.
    public static List<StatementInfo> GetStatementsByCharacter(int chapterId, string characterId)
    {
        List<StatementInfo> result = collectedStatements.FindAll(s => s.chapterId == chapterId && string.Equals(s.characterId, characterId, StringComparison.OrdinalIgnoreCase));
        foreach (StatementInfo update in storyStatementOverrides)
        {
            if (update.chapterId != chapterId || !string.Equals(update.characterId, characterId, StringComparison.OrdinalIgnoreCase)) continue;
            StatementInfo existing = result.Find(s => string.Equals(s.question, update.question, StringComparison.OrdinalIgnoreCase));
            if (existing != null) existing.answer = update.answer;
            else result.Add(new StatementInfo { chapterId = update.chapterId, characterId = update.characterId, question = update.question, answer = update.answer });
        }
        return result;
    }

    public static List<StatementInfo> GetStatementsByCharacter(string characterId)
    {
        List<StatementInfo> result = collectedStatements.FindAll(s => string.Equals(s.characterId, characterId, StringComparison.OrdinalIgnoreCase));
        foreach (StatementInfo update in storyStatementOverrides)
        {
            if (!string.Equals(update.characterId, characterId, StringComparison.OrdinalIgnoreCase)) continue;
            StatementInfo existing = result.Find(s => s.chapterId == update.chapterId && string.Equals(s.question, update.question, StringComparison.OrdinalIgnoreCase));
            if (existing != null) existing.answer = update.answer;
            else result.Add(new StatementInfo { chapterId = update.chapterId, characterId = update.characterId, question = update.question, answer = update.answer });
        }
        return result;
    }

    public static void ClearStatements() { collectedStatements.Clear(); storyStatementOverrides.Clear(); askedQuestionKeys.Clear(); }

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
        data.storyStatementOverrides.Clear();
        foreach (StatementInfo s in storyStatementOverrides)
            data.storyStatementOverrides.Add(new StatementSaveData { chapterId = s.chapterId, characterId = s.characterId, question = s.question, answer = s.answer });
        data.askedQuestionKeys = GetAskedQuestionKeys();
        data.completedStoryKeys = GetCompletedStoryKeys();
        data.pendingSpecialEvidenceId = PendingSpecialEvidenceId;
        data.deadCharacters = GetDeadCharacters();
        data.collectedCipherIds = GetCollectedCipherIds();
        data.solvedCipherIds = GetSolvedCipherIds();
        data.activeCipherId = ActiveCipherId;
    }

    public static void RestoreFromSave(GameSaveData data)
    {
        collectedStatements.Clear(); storyStatementOverrides.Clear(); askedQuestionKeys.Clear(); completedStoryKeys.Clear(); collectedEvidence.Clear(); spawnedByCase.Clear(); deadCharacters.Clear();
        ClearCipherProgress();
        PendingSpecialEvidenceId = data.pendingSpecialEvidenceId;

        if (data.chapterId == 1 && CaseSessionData.IsSessionStarted)
        {
            // Chapter 1 có thứ tự sát hại khác nhau theo Branch.
            // Dùng số người đã chết trong Save làm tiến độ, rồi ánh xạ lại ID theo Branch hiện tại.
            int savedDeathCount = 0;
            if (data.deadCharacters != null)
            {
                foreach (CharacterDeathSaveData death in data.deadCharacters)
                {
                    if (death != null && death.chapterId == 1)
                        savedDeathCount++;
                }
            }

            // CurrentCaseNumber chỉ tăng sau khi người chơi rời EndPanel của Case trước.
            int progressDeathCount = 0;
            if (data.currentCaseNumber >= 2) progressDeathCount = 1;
            if (data.currentCaseNumber >= 3) progressDeathCount = 2;
            if (data.currentCaseNumber >= 4) progressDeathCount = 3;
            if (data.finalChancePending) progressDeathCount = 4;

            int deathsToRestore = Mathf.Clamp(Math.Max(savedDeathCount, progressDeathCount), 0, 4);
            for (int completedCase = 1; completedCase <= deathsToRestore; completedCase++)
            {
                string characterId = CaseSessionData.GetDeadCharacterAfterCase(completedCase);
                AddCharacterDeathWithoutSave(1, characterId);
            }
        }
        else if (data.deadCharacters != null)
        {
            foreach (CharacterDeathSaveData death in data.deadCharacters)
            {
                if (death != null)
                    AddCharacterDeathWithoutSave(death.chapterId, death.characterId);
            }
        }
        if (data.statements != null) foreach (StatementSaveData s in data.statements) if (s != null) collectedStatements.Add(new StatementInfo { chapterId = s.chapterId, characterId = s.characterId, question = s.question, answer = s.answer });
        if (data.storyStatementOverrides != null) foreach (StatementSaveData s in data.storyStatementOverrides) if (s != null) storyStatementOverrides.Add(new StatementInfo { chapterId = s.chapterId, characterId = s.characterId, question = s.question, answer = s.answer });
        if (data.askedQuestionKeys != null) foreach (string key in data.askedQuestionKeys) if (!string.IsNullOrEmpty(key)) askedQuestionKeys.Add(key);
        // Tương thích Save cũ: suy ra các câu đã hỏi từ danh sách lời khai đã lưu.
        foreach (StatementInfo s in collectedStatements) askedQuestionKeys.Add(QuestionKey(s.chapterId, s.characterId, s.question));
        if (data.completedStoryKeys != null) foreach (string key in data.completedStoryKeys) if (!string.IsNullOrEmpty(key)) completedStoryKeys.Add(key);
        if (data.collectedCipherIds != null)
            foreach (string id in data.collectedCipherIds)
                if (!string.IsNullOrWhiteSpace(id)) collectedCipherIds.Add(id.Trim());
        if (data.solvedCipherIds != null)
            foreach (string id in data.solvedCipherIds)
                if (!string.IsNullOrWhiteSpace(id) && collectedCipherIds.Contains(id.Trim())) solvedCipherIds.Add(id.Trim());
        if (!string.IsNullOrWhiteSpace(data.activeCipherId) && collectedCipherIds.Contains(data.activeCipherId.Trim()))
            ActiveCipherId = data.activeCipherId.Trim();
        else if (data.collectedCipherIds != null && data.collectedCipherIds.Count > 0)
        {
            // Tương thích Save cũ hoặc Save thiếu activeCipherId: lấy ID hợp lệ cuối cùng trong danh sách lưu.
            for (int i = data.collectedCipherIds.Count - 1; i >= 0; i--)
            {
                string id = data.collectedCipherIds[i];
                if (!string.IsNullOrWhiteSpace(id) && collectedCipherIds.Contains(id.Trim()))
                {
                    ActiveCipherId = id.Trim();
                    break;
                }
            }
        }
        if (data.cases != null) foreach (CaseSaveData c in data.cases) if (c != null && c.spawnedEvidence != null && c.spawnedEvidence.Count == 12) spawnedByCase[c.caseNumber] = CloneSpawnData(c.spawnedEvidence);
        if (data.collectedEvidence != null) foreach (CollectedEvidenceSaveData e in data.collectedEvidence) if (e != null && !string.IsNullOrEmpty(e.evidenceId))
                    collectedEvidence[EvidenceKey(e.caseNumber, e.evidenceId)] = new CollectedEvidenceInfo { caseNumber = e.caseNumber, evidenceId = e.evidenceId, evidenceName = e.evidenceName, description = e.description, evidenceImage = null };
    }

    public static void ClearAllSessionData()
    {
        ClearStatements(); ClearCollectedEvidence(); ClearSavedSpawnedEvidence(); completedStoryKeys.Clear(); ClearCharacterDeaths(); ClearCipherProgress(); PendingSpecialEvidenceId = null;
    }

    private static void AutoSave()
    {
        if (GameManager.Instance != null && CaseSessionData.IsSessionStarted) GameManager.Instance.SaveGame();
    }
}

