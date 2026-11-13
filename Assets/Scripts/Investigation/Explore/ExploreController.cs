
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ExploreController : MonoBehaviour
{
    [System.Serializable]
    public class LocationData
    {
        [Header("Thông tin địa điểm")]
        public string locationName;

        [Header("Nút chọn địa điểm")]
        public Button locationButton;

        [Header("Background của địa điểm")]
        public Sprite background;

        [Header("Nhóm vật chứng của địa điểm")]
        public GameObject evidenceGroup;
    }

    [Header("Danh sách địa điểm")]
    public LocationData[] locations;

    [Header("Background")]
    public Image backgroundImage;

    [Header("Tên địa điểm hiện tại")]
    public TMP_Text currentLocationText;

    [Header("Danh sách địa điểm")]
    public GameObject locationListPanel;
    public Button toggleLocationListButton;

    [Header("Thông tin vật chứng")]
    public GameObject evidenceDetailPanel;
    public Image evidenceImage;
    public TMP_Text evidenceNameText;
    public TMP_Text evidenceDescriptionText;
    public Button collectButton;
    public Button closeDetailButton;

    [Header("Trạng thái")]
    public int currentLocationIndex = 0;

    private EvidenceItemData currentEvidence;

    private void Start()
    {
        // Đăng ký sự kiện cho các nút địa điểm.
        for (int i = 0; i < locations.Length; i++)
        {
            int index = i;

            if (locations[i].locationButton != null)
            {
                locations[i].locationButton.onClick.RemoveAllListeners();

                locations[i].locationButton.onClick.AddListener(
                    () => SelectLocation(index)
                );
            }
        }

        // Nút ẩn/hiện danh sách địa điểm.
        if (toggleLocationListButton != null)
        {
            toggleLocationListButton.onClick.RemoveAllListeners();
            toggleLocationListButton.onClick.AddListener(ToggleLocationList);
        }

        // Nút thu thập.
        if (collectButton != null)
        {
            collectButton.onClick.RemoveAllListeners();
            collectButton.onClick.AddListener(CollectCurrentEvidence);
        }

        // Nút đóng bảng thông tin.
        if (closeDetailButton != null)
        {
            closeDetailButton.onClick.RemoveAllListeners();
            closeDetailButton.onClick.AddListener(CloseEvidenceDetail);
        }

        // Ẩn bảng thông tin lúc bắt đầu.
        if (evidenceDetailPanel != null)
        {
            evidenceDetailPanel.SetActive(false);
        }

        // Hiển thị địa điểm đầu tiên.
        if (locations != null && locations.Length > 0)
        {
            SelectLocation(currentLocationIndex);
        }


        // Ẩn những vật chứng đã thu thập từ trước.
        EvidenceItemData[] allEvidence =
            FindObjectsByType<EvidenceItemData>(FindObjectsSortMode.None);

        foreach (EvidenceItemData evidence in allEvidence)
        {
            if (evidence.IsCollected())
            {
                evidence.gameObject.SetActive(false);
            }
        }

    }

    // Chọn địa điểm.
    public void SelectLocation(int index)
    {
        if (locations == null || index < 0 || index >= locations.Length)
            return;

        currentLocationIndex = index;

        UpdateCurrentLocationText();

        // Đổi Background.
        if (backgroundImage != null)
        {
            backgroundImage.sprite = locations[index].background;
        }

        // Chỉ hiện nhóm vật chứng thuộc địa điểm được chọn.
        for (int i = 0; i < locations.Length; i++)
        {
            if (locations[i].evidenceGroup != null)
            {
                locations[i].evidenceGroup.SetActive(i == index);
            }
        }

        CloseEvidenceDetail();
    }

    // Ẩn hoặc hiện danh sách địa điểm.
    public void ToggleLocationList()
    {
        if (locationListPanel == null)
            return;

        locationListPanel.SetActive(!locationListPanel.activeSelf);
    }

    // Cập nhật tên địa điểm.
    private void UpdateCurrentLocationText()
    {
        if (currentLocationText == null)
            return;

        currentLocationText.text =
            locations[currentLocationIndex].locationName;
    }

    // Hiển thị thông tin vật chứng.
    public void ShowEvidence(EvidenceItemData evidence)
    {
        if (evidence == null)
            return;

        if (evidenceDetailPanel == null)
            return;

        currentEvidence = evidence;

        evidenceDetailPanel.SetActive(true);

        // Hiển thị tên.
        if (evidenceNameText != null)
        {
            evidenceNameText.text = evidence.evidenceName;
        }

        // Hiển thị mô tả.
        if (evidenceDescriptionText != null)
        {
            evidenceDescriptionText.text = evidence.description;
        }

        // Hiển thị ảnh vật chứng.
        if (evidenceImage != null)
        {
            evidenceImage.sprite = evidence.evidenceImage;

            evidenceImage.gameObject.SetActive(
                evidence.evidenceImage != null
            );
        }

        // Kiểm tra trạng thái từ dữ liệu chung.
        bool alreadyCollected = evidence.IsCollected();

        evidence.isCollected = alreadyCollected;

        if (collectButton != null)
        {
            collectButton.gameObject.SetActive(!alreadyCollected);
        }
    }

    // Thu thập vật chứng.


    public void CollectCurrentEvidence()
    {
        if (currentEvidence == null)
            return;

        EvidenceItemData collectedItem = currentEvidence;

        string id = collectedItem.evidenceId;

        if (string.IsNullOrWhiteSpace(id))
        {
            Debug.LogWarning("Vật chứng chưa được đặt evidenceId!");
            return;
        }

        bool collected = GameSessionData.CollectEvidence(collectedItem);

        if (!collected)
        {
            Debug.Log("Vật chứng đã được thu thập: " + id);
            collectedItem.gameObject.SetActive(false);
            CloseEvidenceDetail();
            return;
        }

        collectedItem.isCollected = true;

        Debug.Log("Đã thu thập vật chứng: " +
                  collectedItem.evidenceName);

        // Ẩn vật chứng khỏi màn hình Explore.
        collectedItem.gameObject.SetActive(false);

        // Đóng bảng thông tin vật chứng.
        CloseEvidenceDetail();
    }


    // Đóng bảng thông tin.
    public void CloseEvidenceDetail()
    {
        if (evidenceDetailPanel != null)
        {
            evidenceDetailPanel.SetActive(false);
        }

        currentEvidence = null;
    }

    // Quay lại Investigation.
    public void OnBack()
    {
        SceneManager.LoadScene("Investigation");
    }
}
