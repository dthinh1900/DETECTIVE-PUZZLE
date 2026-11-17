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
        [Header("Tên hiện trường / Case")]
        public string locationName;
        [Header("Nút chọn hiện trường trong LocationListPanel")]
        public Button locationButton;
        [Header("Background tùy chọn")]
        public Sprite background;
        [Header("EvidenceGroup tương ứng Case")]
        public GameObject evidenceGroup;
    }

    [Header("Bốn hiện trường: Element 0-3 tương ứng Case 1-4")]
    public LocationData[] locations;
    [Header("Background")]
    public Image backgroundImage;
    [Header("Tên hiện trường")]
    public TMP_Text currentLocationText;
    [Header("Danh sách hiện trường")]
    public GameObject locationListPanel;
    public Button toggleLocationListButton;
    [Header("Thông tin vật chứng")]
    public GameObject evidenceDetailPanel;
    public Image evidenceImage;
    public TMP_Text evidenceNameText;
    public TMP_Text evidenceDescriptionText;
    public Button collectButton;
    public Button closeDetailButton;
    [Header("Bộ đếm vật chứng")]
    public TMP_Text evidenceCountText;
    [Header("Trạng thái tương thích")]
    public int currentLocationIndex;

    private EvidenceItemData currentEvidence;
    private int currentCaseIndex;
    private ChapterDataLoader chapterDataLoader;

    private void Start()
    {
        if (!CaseSessionData.IsSessionStarted)
        {
            Debug.LogError("[EXPLORE] Chưa có phiên chơi.");
            return;
        }

        if (locations == null || locations.Length != 4)
        {
            Debug.LogError("[EXPLORE] Locations phải có đúng 4 phần tử, tương ứng Case 1-4.");
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError("[EXPLORE] Không tìm thấy GameManager.");
            return;
        }

        chapterDataLoader = GameManager.Instance.GetComponent<ChapterDataLoader>();
        if (chapterDataLoader == null)
        {
            Debug.LogError("[EXPLORE] GameManager chưa có ChapterDataLoader.");
            return;
        }

        evidenceSpawner = evidenceSpawner != null
            ? evidenceSpawner
            : FindFirstObjectByType<EvidenceSpawner>();

        if (evidenceSpawner == null)
        {
            Debug.LogError("[EXPLORE] Chưa gắn EvidenceSpawner.");
            return;
        }

        SetupButtons();

        if (locationListPanel != null)
            locationListPanel.SetActive(false);

        if (toggleLocationListButton != null)
            toggleLocationListButton.gameObject.SetActive(true);

        if (evidenceDetailPanel != null)
            evidenceDetailPanel.SetActive(false);

        if (collectButton != null)
        {
            collectButton.onClick.RemoveListener(CollectCurrentEvidence);
            collectButton.onClick.AddListener(CollectCurrentEvidence);
        }

        if (closeDetailButton != null)
        {
            closeDetailButton.onClick.RemoveListener(CloseEvidenceDetail);
            closeDetailButton.onClick.AddListener(CloseEvidenceDetail);
        }

        int initialCase = CaseSessionData.ViewingCaseNumber > 0
            ? CaseSessionData.ViewingCaseNumber
            : CaseSessionData.CurrentCaseNumber;

        RefreshLocationButtons();
        LoadAndShowCase(initialCase);
    }

    private void SetupButtons()
    {
        if (toggleLocationListButton != null &&
            toggleLocationListButton.onClick.GetPersistentEventCount() == 0)
        {
            toggleLocationListButton.onClick.AddListener(ToggleLocationList);
        }

        for (int i = 0; i < locations.Length; i++)
        {
            if (locations[i] == null || locations[i].locationButton == null)
                continue;

            int caseIndex = i;
            Button button = locations[i].locationButton;

            // Nếu Button chưa được gán OnClick trong Inspector, tự kết nối bằng code.
            // Nếu đã gán OnClick trong Inspector, giữ nguyên kết nối hiện tại.
            if (button.onClick.GetPersistentEventCount() == 0)
                button.onClick.AddListener(() => SelectLocation(caseIndex));
        }
    }

    private void RefreshLocationButtons()
    {
        int highestUnlockedCase = CaseSessionData.CurrentCaseNumber;

        for (int i = 0; i < locations.Length; i++)
        {
            if (locations[i] == null || locations[i].locationButton == null)
                continue;

            bool unlocked = (i + 1) <= highestUnlockedCase;
            locations[i].locationButton.gameObject.SetActive(unlocked);
            locations[i].locationButton.interactable = unlocked;
        }
    }

    // Nút trong danh sách gọi hàm này với index 0, 1, 2 hoặc 3.
    public void SelectLocation(int index)
    {
        if (index < 0 || index >= 4)
            return;

        int caseNumber = index + 1;
        if (caseNumber > CaseSessionData.CurrentCaseNumber)
        {
            Debug.LogWarning("[EXPLORE] Case " + caseNumber + " chưa được mở khóa.");
            return;
        }

        if (locationListPanel != null)
            locationListPanel.SetActive(false);

        LoadAndShowCase(caseNumber);
    }

    private void LoadAndShowCase(int caseNumber)
    {
        if (caseNumber < 1 || caseNumber > CaseSessionData.CurrentCaseNumber || caseNumber > locations.Length)
            return;

        LocationData location = locations[caseNumber - 1];
        if (location == null || location.evidenceGroup == null)
        {
            Debug.LogError("[EXPLORE] Chưa gán EvidenceGroup cho Case " + caseNumber + ".");
            return;
        }

        ChapterData chapter = chapterDataLoader.LoadCase(
            GameManager.Instance.SelectedChapter,
            caseNumber
        );

        if (chapter == null)
            return;

        if (!CaseSessionData.SetViewingCase(chapter, caseNumber))
            return;

        // Chỉ bật EvidenceGroup của hiện trường đang xem.
        for (int i = 0; i < locations.Length; i++)
        {
            if (locations[i] != null && locations[i].evidenceGroup != null)
                locations[i].evidenceGroup.SetActive(i == caseNumber - 1);
        }

        currentCaseIndex = caseNumber - 1;
        currentLocationIndex = currentCaseIndex;

        CloseEvidenceDetail();

        if (backgroundImage != null && location.background != null)
            backgroundImage.sprite = location.background;

        if (currentLocationText != null)
        {
            currentLocationText.text = string.IsNullOrWhiteSpace(location.locationName)
                ? "Hiện trường vụ án " + caseNumber
                : location.locationName;
        }

        evidenceSpawner.SpawnEvidence(
            this,
            location.evidenceGroup.transform,
            CaseSessionData.ViewingChapter,
            CaseSessionData.ViewingBranch,
            caseNumber
        );

        RefreshLocationButtons();
        UpdateEvidenceCount();

        GameManager.Instance.SaveGame();
    }

    private int GetCollectedEvidenceCount()
    {
        return GameSessionData.GetCollectedEvidenceCount(CaseSessionData.ViewingCaseNumber);
    }

    private void UpdateEvidenceCount()
    {
        if (evidenceCountText == null)
            return;

        evidenceCountText.gameObject.SetActive(true);
        evidenceCountText.text = $"Đã thu thập: {GetCollectedEvidenceCount()}/{MAX_EVIDENCE}";
    }

    public void ToggleLocationList()
    {
        if (locationListPanel == null)
        {
            Debug.LogWarning("[EXPLORE] Chưa gán Location List Panel.");
            return;
        }

        RefreshLocationButtons();
        locationListPanel.SetActive(!locationListPanel.activeSelf);
    }

    public void ShowEvidence(EvidenceItemData evidence)
    {
        int caseNumber = CaseSessionData.ViewingCaseNumber;

        if (evidence == null || evidenceDetailPanel == null ||
            evidence.isCollected ||
            GameSessionData.IsEvidenceCollected(caseNumber, evidence.evidenceId))
            return;

        currentEvidence = evidence;
        evidenceDetailPanel.SetActive(true);

        if (evidenceNameText != null)
            evidenceNameText.text = evidence.evidenceName;

        if (evidenceDescriptionText != null)
            evidenceDescriptionText.text = evidence.description;

        if (evidenceImage != null)
        {
            evidenceImage.sprite = evidence.evidenceImage;
            evidenceImage.gameObject.SetActive(evidence.evidenceImage != null);
        }

        if (collectButton != null)
        {
            bool canCollect = GetCollectedEvidenceCount() < MAX_EVIDENCE;
            collectButton.gameObject.SetActive(canCollect);
            collectButton.interactable = canCollect;
        }
    }

    private bool IsCurrentViewingBranchSpecialEvidence(EvidenceItemData evidence)
    {
        return evidence != null && CaseSessionData.ViewingBranch != null &&
               CaseSessionData.ViewingBranch.specialEvidenceId == evidence.evidenceId;
    }

    public void CollectCurrentEvidence()
    {
        if (currentEvidence == null)
            return;

        int caseNumber = CaseSessionData.ViewingCaseNumber;
        if (GetCollectedEvidenceCount() >= MAX_EVIDENCE)
        {
            Debug.LogWarning("[EXPLORE] Đã đạt giới hạn 4 vật chứng của Case này.");
            UpdateEvidenceCount();
            CloseEvidenceDetail();
            return;
        }

        EvidenceItemData item = currentEvidence;
        bool isSpecial = IsCurrentViewingBranchSpecialEvidence(item);

        if (!GameSessionData.CollectEvidence(item, caseNumber))
        {
            item.gameObject.SetActive(false);
            CloseEvidenceDetail();
            UpdateEvidenceCount();
            return;
        }

        item.isCollected = true;
        item.gameObject.SetActive(false);
        UpdateEvidenceCount();
        CloseEvidenceDetail();
        GameManager.Instance?.SaveGame();

        if (isSpecial)
        {
            GameSessionData.SetPendingSpecialEvidenceId(item.evidenceId);
            GameManager.Instance?.SaveGame();
            SceneManager.LoadScene(STORY_SCENE_NAME);
        }
    }

    public void CloseEvidenceDetail()
    {
        if (evidenceDetailPanel != null)
            evidenceDetailPanel.SetActive(false);

        currentEvidence = null;
    }

    public void OnBack()
    {
        GameManager.Instance?.SaveGame();
        SceneManager.LoadScene("Investigation");
    }
}
