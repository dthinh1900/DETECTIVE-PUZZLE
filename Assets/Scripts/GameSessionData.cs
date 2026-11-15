
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
    // ==================== LỜI KHAI ====================

    private static List<StatementInfo> collectedStatements =
        new List<StatementInfo>();

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
            Debug.LogWarning("[LỜI KHAI] Dữ liệu không đầy đủ!");
            return;
        }

        bool alreadyExists = collectedStatements.Exists(s =>
            s.chapterId == chapterId &&
            s.characterId == characterId &&
            s.question == question
        );

        if (alreadyExists)
        {
            Debug.Log("[LỜI KHAI] Lời khai đã tồn tại!");
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

    // Giữ hàm cũ để tương thích
    public static void AddStatement(
        string characterId,
        string question,
        string answer)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("[LỜI KHAI] Không tìm thấy GameManager!");
            return;
        }

        AddStatement(
            GameManager.Instance.SelectedChapter,
            characterId,
            question,
            answer
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

    // Giữ hàm cũ để tương thích
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

        Debug.Log("[LỜI KHAI] Đã xóa toàn bộ lời khai.");
    }

    // ==================== VẬT CHỨNG ====================

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

        collectedEvidence.Add(evidence.evidenceId, info);

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
        return new List<string>(collectedEvidence.Keys);
    }

    public static void ClearCollectedEvidence()
    {
        collectedEvidence.Clear();

        Debug.Log("[VẬT CHỨNG] Đã xóa toàn bộ vật chứng.");
    }

    // ==================== RESET PHIÊN CHƠI ====================

    public static void ClearAllSessionData()
    {
        ClearStatements();
        ClearCollectedEvidence();

        Debug.Log(
            "[GAME SESSION] Đã reset toàn bộ dữ liệu phiên chơi mới."
        );
    }
}
