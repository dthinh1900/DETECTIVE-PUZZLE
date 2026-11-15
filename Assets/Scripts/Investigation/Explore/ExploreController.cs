using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ExploreController : MonoBehaviour
{
    [Header("Sinh vật phẩm")]
    [SerializeField] private EvidenceSpawner evidenceSpawner;

    private const int MAX_EVIDENCE = 4;
    private const string STORY_SCENE_NAME = "Story";

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

    [Header("Số lượt thu thập vật chứng")]
    public TMP_Text evidenceCountText;

    [Header("Trạng thái")]
    public int currentLocationIndex = 0;

    private EvidenceItemData currentEvidence;

    // Danh sách toàn bộ vật chứng trong Scene hiện tại.
    private EvidenceItemData[] sceneEvidenceItems;

    private void Start()
    {
        if (evidenceSpawner != null)
        {
            evidenceSpawner.SpawnEvidence(this);
        }
        else
        {
            Debug.LogError(
                "Chưa gán EvidenceSpawner vào ExploreController!"
            );
        }

        // Lấy cả vật chứng đang bị ẩn.
        sceneEvidenceItems =
            FindObjectsByType<EvidenceItemData>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        // Đăng ký sự kiện cho các nút địa điểm.
        if (locations != null)
        {
            for (int i = 0; i < locations.Length; i++)
            {
                int index = i;

                if (locations[i] != null &&
                    locations[i].locationButton != null)
                {
                    locations[i].locationButton.onClick.RemoveAllListeners();

                    locations[i].locationButton.onClick.AddListener(
                        () => SelectLocation(index)
                    );
                }
            }
        }

        // Nút ẩn/hiện danh sách địa điểm.
        if (toggleLocationListButton != null)
        {
            toggleLocationListButton.onClick.RemoveAllListeners();
            toggleLocationListButton.onClick.AddListener(
                ToggleLocationList
            );
        }

        // Nút thu thập.
        if (collectButton != null)
        {
            collectButton.onClick.RemoveAllListeners();
            collectButton.onClick.AddListener(
                CollectCurrentEvidence
            );
        }

        // Nút đóng bảng thông tin.
        if (closeDetailButton != null)
        {
            closeDetailButton.onClick.RemoveAllListeners();
            closeDetailButton.onClick.AddListener(
                CloseEvidenceDetail
            );
        }

        // Ẩn bảng thông tin lúc bắt đầu.
        if (evidenceDetailPanel != null)
            evidenceDetailPanel.SetActive(false);

        // Ẩn bộ đếm lúc bắt đầu.
        if (evidenceCountText != null)
            evidenceCountText.gameObject.SetActive(false);

        // Ẩn những vật chứng đã thu thập từ trước.
        if (sceneEvidenceItems != null)
        {
            foreach (EvidenceItemData evidence in sceneEvidenceItems)
            {
                if (evidence != null && evidence.IsCollected())
                {
                    evidence.isCollected = true;
                    evidence.gameObject.SetActive(false);
                }
            }
        }

        // Hiển thị địa điểm đầu tiên.
        if (locations != null && locations.Length > 0)
        {
            SelectLocation(currentLocationIndex);
        }
    }

    // =========================================
    // CHỌN ĐỊA ĐIỂM
    // =========================================

    public void SelectLocation(int index)
    {
        if (locations == null ||
            index < 0 ||
            index >= locations.Length ||
            locations[index] == null)
        {
            return;
        }

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
            if (locations[i] != null &&
                locations[i].evidenceGroup != null)
            {
                locations[i].evidenceGroup.SetActive(i == index);
            }
        }

        CloseEvidenceDetail();

        // Cập nhật bộ đếm theo địa điểm.
        UpdateEvidenceCount();
    }

    // =========================================
    // ĐẾM VẬT CHỨNG
    // =========================================

    private int GetCollectedEvidenceCount()
    {
        int count = 0;

        if (sceneEvidenceItems == null)
            return count;

        foreach (EvidenceItemData evidence in sceneEvidenceItems)
        {
            if (evidence != null && evidence.IsCollected())
            {
                count++;
            }
        }

        return count;
    }

    private bool HasEvidenceAtCurrentLocation()
    {
        if (locations == null ||
            currentLocationIndex < 0 ||
            currentLocationIndex >= locations.Length ||
            locations[currentLocationIndex] == null)
        {
            return false;
        }

        GameObject group =
            locations[currentLocationIndex].evidenceGroup;

        if (group == null)
            return false;

        EvidenceItemData[] evidenceItems =
            group.GetComponentsInChildren<EvidenceItemData>(true);

        return evidenceItems.Length > 0;
    }

    private void UpdateEvidenceCount()
    {
        if (evidenceCountText == null)
            return;

        bool hasEvidence = HasEvidenceAtCurrentLocation();

        evidenceCountText.gameObject.SetActive(hasEvidence);

        if (!hasEvidence)
            return;

        int collectedCount = GetCollectedEvidenceCount();

        evidenceCountText.text =
            $"Đã thu thập: {collectedCount}/{MAX_EVIDENCE}";
    }

    // =========================================
    // DANH SÁCH ĐỊA ĐIỂM
    // =========================================

    public void ToggleLocationList()
    {
        if (locationListPanel == null)
            return;

        locationListPanel.SetActive(
            !locationListPanel.activeSelf
        );
    }

    private void UpdateCurrentLocationText()
    {
        if (currentLocationText == null ||
            locations == null ||
            currentLocationIndex < 0 ||
            currentLocationIndex >= locations.Length ||
            locations[currentLocationIndex] == null)
        {
            return;
        }

        currentLocationText.text =
            locations[currentLocationIndex].locationName;
    }

    // =========================================
    // HIỂN THỊ THÔNG TIN VẬT CHỨNG
    // =========================================

    public void ShowEvidence(EvidenceItemData evidence)
    {
        if (evidence == null ||
            evidenceDetailPanel == null)
        {
            return;
        }

        currentEvidence = evidence;

        evidenceDetailPanel.SetActive(true);

        if (evidenceNameText != null)
        {
            evidenceNameText.text = evidence.evidenceName;
        }

        if (evidenceDescriptionText != null)
        {
            evidenceDescriptionText.text = evidence.description;
        }

        if (evidenceImage != null)
        {
            evidenceImage.sprite = evidence.evidenceImage;

            evidenceImage.gameObject.SetActive(
                evidence.evidenceImage != null
            );
        }

        bool alreadyCollected = evidence.IsCollected();

        evidence.isCollected = alreadyCollected;

        bool canCollect =
            !alreadyCollected &&
            GetCollectedEvidenceCount() < MAX_EVIDENCE;

        if (collectButton != null)
        {
            collectButton.gameObject.SetActive(canCollect);
        }
    }

    // =========================================
    // KIỂM TRA EVIDENCE ĐẶC BIỆT CỦA BRANCH
    // =========================================

    private bool IsCurrentBranchSpecialEvidence(
        EvidenceItemData evidence
    )
    {
        if (evidence == null ||
            CaseSessionData.CurrentBranch == null)
        {
            return false;
        }

        // Dùng specialEvidenceId của Branch hiện tại
        // làm điều kiện xác định chính xác.
        return CaseSessionData.CurrentBranch.specialEvidenceId
               == evidence.evidenceId;
    }

    // =========================================
    // THU THẬP VẬT CHỨNG
    // =========================================

    public void CollectCurrentEvidence()
    {
        if (currentEvidence == null)
            return;

        // Không cho thu thập quá 4 vật chứng.
        if (GetCollectedEvidenceCount() >= MAX_EVIDENCE)
        {
            Debug.Log("Đã đạt giới hạn 4 vật chứng!");

            UpdateEvidenceCount();
            CloseEvidenceDetail();

            return;
        }

        EvidenceItemData collectedItem = currentEvidence;

        string id = collectedItem.evidenceId;

        if (string.IsNullOrWhiteSpace(id))
        {
            Debug.LogWarning(
                "Vật chứng chưa được đặt evidenceId!"
            );
            return;
        }

        // Kiểm tra đây có phải Evidence đặc biệt
        // của Branch đang chơi hay không.
        bool isSpecialEvidence =
            IsCurrentBranchSpecialEvidence(collectedItem);

        // Lưu Evidence vào dữ liệu phiên chơi.
        bool collected =
            GameSessionData.CollectEvidence(collectedItem);

        if (!collected)
        {
            Debug.Log(
                "Vật chứng đã được thu thập: " + id
            );

            collectedItem.gameObject.SetActive(false);

            CloseEvidenceDetail();
            UpdateEvidenceCount();

            return;
        }

        collectedItem.isCollected = true;

        Debug.Log(
            "Đã thu thập vật chứng: " +
            collectedItem.evidenceName
        );

        // Ẩn vật chứng khỏi màn hình Explore.
        collectedItem.gameObject.SetActive(false);

        // Cập nhật số lượt.
        UpdateEvidenceCount();

        // Đóng bảng thông tin vật chứng.
        CloseEvidenceDetail();

        // Nếu là Evidence đặc biệt:
        // lưu ID đang chờ và chuyển sang Scene Story.
        if (isSpecialEvidence)
        {
            GameSessionData.SetPendingSpecialEvidenceId(id);

            Debug.Log(
                "Đã thu thập Evidence đặc biệt: " +
                id +
                ". Đang mở Story..."
            );

            SceneManager.LoadScene(STORY_SCENE_NAME);

            return;
        }
    }

    // =========================================
    // ĐÓNG BẢNG THÔNG TIN
    // =========================================

    public void CloseEvidenceDetail()
    {
        if (evidenceDetailPanel != null)
        {
            evidenceDetailPanel.SetActive(false);
        }

        currentEvidence = null;
    }

    // =========================================
    // QUAY LẠI INVESTIGATION
    // =========================================

    public void OnBack()
    {
        SceneManager.LoadScene("Investigation");
    }
}