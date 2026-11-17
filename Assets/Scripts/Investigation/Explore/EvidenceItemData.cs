using UnityEngine;
using UnityEngine.UI;

public class EvidenceItemData : MonoBehaviour
{
    [Header("Thông tin vật chứng")]
    public string evidenceId;
    public string evidenceName;
    [TextArea(3, 6)] public string description;
    [Header("Hình ảnh vật chứng")]
    public Sprite evidenceImage;
    [SerializeField] private Image displayImage;
    [Header("Trạng thái")]
    public bool isCollected;
    public bool isSpecialEvidence;

    private void Awake() { SetupVisual(); }

    public void SetupVisual()
    {
        if (displayImage == null)
        {
            Transform imageTransform = transform.Find("Image_Evidence");
            if (imageTransform != null) displayImage = imageTransform.GetComponent<Image>();
        }
        if (displayImage == null) displayImage = GetComponentInChildren<Image>(true);
        if (displayImage != null)
        {
            displayImage.sprite = evidenceImage;
            displayImage.preserveAspect = true;
            displayImage.enabled = evidenceImage != null;
        }
    }

    public bool IsCollected() => GameSessionData.IsEvidenceCollected(
        CaseSessionData.ViewingCaseNumber > 0
            ? CaseSessionData.ViewingCaseNumber
            : CaseSessionData.CurrentCaseNumber,
        evidenceId
    );
}
