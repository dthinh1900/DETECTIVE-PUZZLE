
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

public static class GameSessionData
{
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
