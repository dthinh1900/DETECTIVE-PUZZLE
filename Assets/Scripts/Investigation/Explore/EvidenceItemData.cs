using UnityEngine;
using UnityEngine.UI;

public class EvidenceItemData : MonoBehaviour
{
    [Header("Thông tin vật chứng")]
    public string evidenceId;
    public string evidenceName;

    [TextArea(3, 6)]
    public string description;

    [Header("Hình ảnh vật chứng")]
    public Sprite evidenceImage;

    [Header("Image hiển thị tại hiện trường")]
    [SerializeField] private Image displayImage;

    [Header("Trạng thái")]
    public bool isCollected;

    private void Awake()
    {
        SetupVisual();
    }

    public void SetupVisual()
    {
        if (displayImage == null)
        {
            Transform imageTransform =
                transform.Find("Image_Evidence");

            if (imageTransform != null)
                displayImage = imageTransform.GetComponent<Image>();
        }

        if (displayImage == null)
            displayImage = GetComponentInChildren<Image>(true);

        if (displayImage != null)
        {
            displayImage.sprite = evidenceImage;
            displayImage.preserveAspect = true;
            displayImage.enabled = evidenceImage != null;
        }
        else
        {
            Debug.LogWarning(
                "Không tìm thấy Image hiển thị cho vật phẩm: " +
                evidenceId
            );
        }
    }

    public bool IsCollected()
    {
        return GameSessionData.IsEvidenceCollected(evidenceId);
    }
}