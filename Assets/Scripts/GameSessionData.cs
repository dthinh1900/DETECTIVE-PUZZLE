
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
    public string characterId;
    public string question;
    public string answer;
}


public static class GameSessionData
{
    // Danh sách lời khai đã thu thập.
    private static List<StatementInfo> collectedStatements = new List<StatementInfo>();

    // Thêm lời khai mới.
    public static void AddStatement(
        string characterId,
        string question,
        string answer)
    {
        if (string.IsNullOrWhiteSpace(characterId) ||
            string.IsNullOrWhiteSpace(question) ||
            string.IsNullOrWhiteSpace(answer))
        {
            Debug.LogWarning("Lời khai không đầy đủ, không thể lưu!");
            return;
        }

        // Tránh lưu trùng cùng một câu hỏi của cùng nhân vật.
        bool alreadyExists = collectedStatements.Exists(
            s => s.characterId == characterId &&
                 s.question == question
        );

        if (alreadyExists)
            return;

        collectedStatements.Add(new StatementInfo
        {
            characterId = characterId,
            question = question,
            answer = answer
        });

        Debug.Log("Đã lưu lời khai của: " + characterId);
    }

    // Lấy lời khai theo nhân vật.
    public static List<StatementInfo> GetStatementsByCharacter(
        string characterId)
    {
        return collectedStatements.FindAll(
            s => s.characterId == characterId
        );
    }

    // Xóa toàn bộ lời khai.
    public static void ClearStatements()
    {
        collectedStatements.Clear();
    }


    // Lưu vật chứng theo ID để không bị trùng.
    private static Dictionary<string, CollectedEvidenceInfo> collectedEvidence =
        new Dictionary<string, CollectedEvidenceInfo>();

    public static bool IsEvidenceCollected(string evidenceId)
    {
        if (string.IsNullOrWhiteSpace(evidenceId))
            return false;

        return collectedEvidence.ContainsKey(evidenceId);
    }

    // Lưu đầy đủ thông tin vật chứng khi người chơi thu thập.
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
        return new List<CollectedEvidenceInfo>(collectedEvidence.Values);
    }

    public static List<string> GetCollectedEvidenceIds()
    {
        return new List<string>(collectedEvidence.Keys);
    }

    public static void ClearCollectedEvidence()
    {
        collectedEvidence.Clear();
    }
}
