
using UnityEngine;

public class EvidenceItemData : MonoBehaviour
{
    [Header("Thông tin vật chứng")]
    public string evidenceId;
    public string evidenceName;

    [TextArea(3, 6)]
    public string description;

    [Header("Hình ảnh vật chứng")]
    public Sprite evidenceImage;

    [Header("Trạng thái")]
    public bool isCollected;

    // Kiểm tra vật chứng đã được thu thập chưa.
    public bool IsCollected()
    {
        return GameSessionData.IsEvidenceCollected(evidenceId);
    }
}
