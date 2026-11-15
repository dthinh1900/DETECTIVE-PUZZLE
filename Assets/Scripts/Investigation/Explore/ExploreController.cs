
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ExploreController : MonoBehaviour
{
    [Header("Sinh vật phẩm")]
    [SerializeField] private EvidenceSpawner evidenceSpawner;

    private const int MAX_EVIDENCE = 4;

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
            Debug.LogError("Chưa gán EvidenceSpawner vào ExploreController!");
        }
        // Lấy cả vật chứng đang bị ẩn.
        sceneEvidenceItems =
            FindObjectsByType<EvidenceItemData>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

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

        // Ẩn bộ đếm lúc bắt đầu.
        if (evidenceCountText != null)
        {
            evidenceCountText.gameObject.SetActive(false);
        }

        // Ẩn những vật chứng đã thu thập từ trước.
        foreach (EvidenceItemData evidence in sceneEvidenceItems)
        {
            if (evidence != null && evidence.IsCollected())
            {
                evidence.isCollected = true;
                evidence.gameObject.SetActive(false);
            }
        }

        // Hiển thị địa điểm đầu tiên.
        if (locations != null && locations.Length > 0)
        {
            SelectLocation(currentLocationIndex);
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

        // Cập nhật bộ đếm theo địa điểm.
        UpdateEvidenceCount();
    }

    // Đếm số vật chứng đã thu thập trong Scene hiện tại.
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

    // Kiểm tra địa điểm hiện tại có vật chứng hay không.
    private bool HasEvidenceAtCurrentLocation()
    {
        if (locations == null ||
            currentLocationIndex < 0 ||
            currentLocationIndex >= locations.Length)
        {
            return false;
        }

        GameObject group = locations[currentLocationIndex].evidenceGroup;

        if (group == null)
            return false;

        EvidenceItemData[] evidenceItems =
            group.GetComponentsInChildren<EvidenceItemData>(true);

        return evidenceItems.Length > 0;
    }

    // Cập nhật số lượt thu thập.
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

        bool canCollect =
            !alreadyCollected &&
            GetCollectedEvidenceCount() < MAX_EVIDENCE;

        if (collectButton != null)
        {
            collectButton.gameObject.SetActive(canCollect);
        }
    }

    // Thu thập vật chứng.
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
            Debug.LogWarning("Vật chứng chưa được đặt evidenceId!");
            return;
        }

        bool collected = GameSessionData.CollectEvidence(collectedItem);

        if (!collected)
        {
            Debug.Log("Vật chứng đã được thu thập: " + id);
            collectedItem.gameObject.SetActive(false);
            CloseEvidenceDetail();
            UpdateEvidenceCount();
            return;
        }

        collectedItem.isCollected = true;

        Debug.Log("Đã thu thập vật chứng: " +
                  collectedItem.evidenceName);

        // Ẩn vật chứng khỏi màn hình Explore.
        collectedItem.gameObject.SetActive(false);

        // Cập nhật số lượt.
        UpdateEvidenceCount();

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
