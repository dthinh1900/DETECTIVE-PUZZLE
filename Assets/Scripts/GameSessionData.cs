using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CollectedEvidenceInfo
{
    public string evidenceId;
    public string evidenceName;
    public string description;
    public Sprite evidenceImage;
}

[System.Serializable]
public class StatementInfo
{
    public int chapterId;
    public string characterId;
    public string question;
    public string answer;
}

public static class GameSessionData
{
    // =========================
    // STATEMENT
    // =========================

    private static List<StatementInfo> collectedStatements =
        new List<StatementInfo>();

    // Luu cac Story da ap dung cap nhat Statement.
    private static HashSet<string> completedStoryKeys =
        new HashSet<string>();

    public static void AddStatement(
        int chapterId,
        string characterId,
        string question,
        string answer)
    {
        if (chapterId <= 0 ||
            string.IsNullOrWhiteSpace(characterId) ||
            string.IsNullOrWhiteSpace(question) ||
            string.IsNullOrWhiteSpace(answer))
        {
            Debug.LogWarning(
                "[LỜI KHAI] Dữ liệu không đầy đủ!"
            );
            return;
        }

        bool alreadyExists = collectedStatements.Exists(s =>
            s.chapterId == chapterId &&
            s.characterId == characterId &&
            s.question == question
        );

        if (alreadyExists)
        {
            Debug.Log(
                "[LỜI KHAI] Lời khai đã tồn tại!"
            );
            return;
        }

        collectedStatements.Add(new StatementInfo
        {
            chapterId = chapterId,
            characterId = characterId,
            question = question,
            answer = answer
        });

        Debug.Log(
            "[LỜI KHAI] Đã lưu chương " + chapterId +
            " | Nhân vật: " + characterId
        );
    }

    // Giữ hàm cũ để tương thích với hệ thống thẩm vấn.
    public static void AddStatement(
        string characterId,
        string question,
        string answer)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError(
                "[LỜI KHAI] Không tìm thấy GameManager!"
            );
            return;
        }

        AddStatement(
            GameManager.Instance.SelectedChapter,
            characterId,
            question,
            answer
        );
    }

    // Story dung ham nay:
    // Neu trung chapter + characterId + question thi cap nhat answer.
    // Neu chua co thi them Statement moi.
    public static void UpsertStoryStatement(
        int chapterId,
        string characterId,
        string question,
        string answer)
    {
        if (chapterId <= 0 ||
            string.IsNullOrWhiteSpace(characterId) ||
            string.IsNullOrWhiteSpace(question) ||
            string.IsNullOrWhiteSpace(answer))
        {
            Debug.LogWarning(
                "[STORY STATEMENT] Du lieu cap nhat khong day du."
            );
            return;
        }

        string normalizedCharacterId = characterId.Trim();
        string normalizedQuestion = question.Trim();

        StatementInfo existing = collectedStatements.Find(s =>
            s.chapterId == chapterId &&
            string.Equals(
                s.characterId?.Trim(),
                normalizedCharacterId,
                StringComparison.OrdinalIgnoreCase
            ) &&
            string.Equals(
                s.question?.Trim(),
                normalizedQuestion,
                StringComparison.OrdinalIgnoreCase
            )
        );

        if (existing != null)
        {
            existing.answer = answer;

            Debug.Log(
                "[STORY STATEMENT] Da cap nhat cau tra loi: " +
                normalizedCharacterId
            );

            return;
        }

        collectedStatements.Add(new StatementInfo
        {
            chapterId = chapterId,
            characterId = normalizedCharacterId,
            question = normalizedQuestion,
            answer = answer
        });

        Debug.Log(
            "[STORY STATEMENT] Da them Statement moi: " +
            normalizedCharacterId
        );
    }

    public static List<StatementInfo> GetStatementsByCharacter(
        int chapterId,
        string characterId)
    {
        return collectedStatements.FindAll(s =>
            s.chapterId == chapterId &&
            s.characterId == characterId
        );
    }

    // Giữ hàm cũ để tương thích.
    public static List<StatementInfo> GetStatementsByCharacter(
        string characterId)
    {
        return collectedStatements.FindAll(s =>
            s.characterId == characterId
        );
    }

    public static void ClearStatements()
    {
        collectedStatements.Clear();

        Debug.Log(
            "[LỜI KHAI] Đã xóa toàn bộ lời khai."
        );
    }

    // =========================
    // STORY COMPLETION
    // =========================

    // Moi Branch + Story chi duoc ap dung cap nhat mot lan.
    public static bool TryMarkStoryCompleted(string storyKey)
    {
        if (string.IsNullOrWhiteSpace(storyKey))
            return false;

        return completedStoryKeys.Add(storyKey);
    }

    // =========================
    // EVIDENCE
    // =========================

    private static Dictionary<string, CollectedEvidenceInfo>
        collectedEvidence =
            new Dictionary<string, CollectedEvidenceInfo>();

    public static bool IsEvidenceCollected(string evidenceId)
    {
        if (string.IsNullOrWhiteSpace(evidenceId))
            return false;

        return collectedEvidence.ContainsKey(evidenceId);
    }

    public static bool CollectEvidence(EvidenceItemData evidence)
    {
        if (evidence == null ||
            string.IsNullOrWhiteSpace(evidence.evidenceId))
        {
            return false;
        }

        if (collectedEvidence.ContainsKey(evidence.evidenceId))
            return false;

        CollectedEvidenceInfo info = new CollectedEvidenceInfo
        {
            evidenceId = evidence.evidenceId,
            evidenceName = evidence.evidenceName,
            description = evidence.description,
            evidenceImage = evidence.evidenceImage
        };

        collectedEvidence.Add(
            evidence.evidenceId,
            info
        );

        return true;
    }

    public static List<CollectedEvidenceInfo> GetCollectedEvidence()
    {
        return new List<CollectedEvidenceInfo>(
            collectedEvidence.Values
        );
    }

    public static List<string> GetCollectedEvidenceIds()
    {
        return new List<string>(
            collectedEvidence.Keys
        );
    }

    public static void ClearCollectedEvidence()
    {
        collectedEvidence.Clear();
        ClearPendingSpecialEvidenceId();

        Debug.Log(
            "[VẬT CHỨNG] Đã xóa toàn bộ vật chứng."
        );
    }

    // =========================
    // SPECIAL STORY REQUEST
    // =========================

    // ExploreController goi ham nay sau khi thu thap
    // thanh cong Evidence dac biet.
    public static string PendingSpecialEvidenceId
    {
        get;
        private set;
    }

    public static void SetPendingSpecialEvidenceId(
        string evidenceId)
    {
        if (string.IsNullOrWhiteSpace(evidenceId))
            return;

        PendingSpecialEvidenceId = evidenceId;

        Debug.Log(
            "[STORY] Dang cho mo Story dac biet cua Evidence: " +
            evidenceId
        );
    }

    public static void ClearPendingSpecialEvidenceId()
    {
        PendingSpecialEvidenceId = null;
    }

    // =========================
    // RESET SESSION
    // =========================

    public static void ClearAllSessionData()
    {
        ClearStatements();
        ClearCollectedEvidence();

        completedStoryKeys.Clear();
        ClearPendingSpecialEvidenceId();

        Debug.Log(
            "[GAME SESSION] Da reset toan bo du lieu phien choi moi."
        );
    }
}