
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CaseFileEvidenceController : MonoBehaviour
{
    [Header("Danh sách vật chứng")]
    [SerializeField] private Transform content;

    [SerializeField] private GameObject evidenceCardPrefab;

    [Header("Thông báo")]
    [SerializeField] private TMP_Text noEvidenceText;

    private void OnEnable()
    {
        RefreshEvidenceList();
    }

    public void RefreshEvidenceList()
    {
        if (content == null || evidenceCardPrefab == null)
        {
            Debug.LogError(
                "Chưa gán Content hoặc Evidence Card Prefab!"
            );
            return;
        }

        // Xóa danh sách cũ trước khi tạo lại.
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        List<CollectedEvidenceInfo> evidenceList =
            GameSessionData.GetCollectedEvidence();

        bool hasEvidence = evidenceList.Count > 0;

        if (noEvidenceText != null)
        {
            noEvidenceText.gameObject.SetActive(!hasEvidence);
        }

        foreach (CollectedEvidenceInfo evidence in evidenceList)
        {
            GameObject card = Instantiate(
                evidenceCardPrefab,
                content
            );

            TMP_Text[] texts =
                card.GetComponentsInChildren<TMP_Text>(true);

            Image[] images =
                card.GetComponentsInChildren<Image>(true);

            TMP_Text nameText = null;
            TMP_Text descriptionText = null;
            Image evidenceImage = null;

            foreach (TMP_Text text in texts)
            {
                if (text.name == "Text_EvidenceName")
                    nameText = text;

                else if (text.name == "Text_EvidenceDescription")
                    descriptionText = text;
            }

            foreach (Image image in images)
            {
                if (image.name == "Image_Evidence")
                {
                    evidenceImage = image;
                    break;
                }
            }

            if (nameText != null)
                nameText.text = evidence.evidenceName;

            if (descriptionText != null)
                descriptionText.text = evidence.description;

            if (evidenceImage != null)
            {
                evidenceImage.sprite = evidence.evidenceImage;
                evidenceImage.gameObject.SetActive(
                    evidence.evidenceImage != null
                );
            }
        }
    }
}
