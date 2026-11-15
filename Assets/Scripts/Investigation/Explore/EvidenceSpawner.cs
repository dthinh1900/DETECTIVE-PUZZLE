
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EvidenceSpawner : MonoBehaviour
{
    [System.Serializable]
    public class EvidenceSprite
    {
        public string itemId;
        public Sprite sprite;
    }

    [Header("Prefab")]
    [SerializeField] private GameObject evidencePrefab;

    [Header("Vùng sinh vật phẩm")]
    [SerializeField] private Transform spawnPointsRoot;

    [Header("12 vị trí sinh vật phẩm")]
    [SerializeField] private RectTransform[] spawnPoints;

    [Header("Ảnh 20 vật phẩm")]
    [SerializeField] private EvidenceSprite[] evidenceSprites;

    private const int CORRECT_COUNT = 3;
    private const int DECOY_COUNT = 9;
    private const int TOTAL_COUNT = 12;

    private bool hasSpawned;

    private readonly List<GameObject> spawnedObjects =
        new List<GameObject>();

    public void SpawnEvidence(ExploreController controller)
    {
        if (hasSpawned)
            return;

        if (controller == null)
        {
            Debug.LogError("[SPAWNER] ExploreController chưa được gán.");
            return;
        }

        if (!CaseSessionData.IsSessionStarted ||
            CaseSessionData.CurrentChapter == null ||
            CaseSessionData.CurrentBranch == null)
        {
            Debug.LogError(
                "[SPAWNER] Chưa có phiên chơi hoặc Branch."
            );
            return;
        }

        if (evidencePrefab == null ||
            spawnPointsRoot == null ||
            spawnPoints == null ||
            spawnPoints.Length != TOTAL_COUNT)
        {
            Debug.LogError(
                "[SPAWNER] Kiểm tra Prefab, vùng sinh và đủ 12 SpawnPoint."
            );
            return;
        }

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] == null)
            {
                Debug.LogError(
                    "[SPAWNER] SpawnPoint " + i + " chưa được gán."
                );
                return;
            }
        }

        ChapterData chapter = CaseSessionData.CurrentChapter;
        CaseBranchData branch = CaseSessionData.CurrentBranch;

        List<string> selectedIds = SelectEvidenceIds(chapter, branch);

        if (selectedIds == null || selectedIds.Count != TOTAL_COUNT)
            return;

        // Kiểm tra đủ ảnh trước khi bắt đầu sinh vật phẩm.
        foreach (string itemId in selectedIds)
        {
            if (GetSprite(itemId) == null)
            {
                Debug.LogError(
                    "[SPAWNER] Chưa gán ảnh cho vật phẩm: " + itemId
                );
                return;
            }
        }

        Shuffle(selectedIds);

        for (int i = 0; i < selectedIds.Count; i++)
        {
            bool success = SpawnOneEvidence(
                chapter,
                selectedIds[i],
                spawnPoints[i],
                controller,
                branch.specialEvidenceId
            );

            if (!success)
            {
                ClearSpawnedObjects();
                return;
            }
        }

        hasSpawned = true;

        Debug.Log(
            "[SPAWNER] Đã tạo đủ 12 vật phẩm cho Branch " +
            branch.branchId +
            ". Vật chứng đặc biệt: " +
            branch.specialEvidenceId
        );
    }

    private List<string> SelectEvidenceIds(
        ChapterData chapter,
        CaseBranchData branch)
    {
        if (chapter.items == null ||
            chapter.items.Count != 20 ||
            branch.correctEvidence == null ||
            branch.correctEvidence.Count != 5)
        {
            Debug.LogError(
                "[SPAWNER] Chapter phải có 20 vật phẩm và Branch phải có 5 vật chứng đúng."
            );
            return null;
        }

        List<string> correctIds = new List<string>();

        foreach (CorrectEvidenceData evidence in branch.correctEvidence)
        {
            if (evidence == null ||
                string.IsNullOrWhiteSpace(evidence.itemId) ||
                correctIds.Contains(evidence.itemId))
            {
                Debug.LogError(
                    "[SPAWNER] Danh sách vật chứng đúng không hợp lệ hoặc bị trùng."
                );
                return null;
            }

            correctIds.Add(evidence.itemId);
        }

        // Kiểm tra ID đặc biệt được khai báo trong JSON.
        string specialId = branch.specialEvidenceId;

        if (string.IsNullOrWhiteSpace(specialId))
        {
            Debug.LogError(
                "[SPAWNER] Branch chưa khai báo specialEvidenceId."
            );
            return null;
        }

        // Vật chứng đặc biệt phải tồn tại và không thuộc nhóm đúng.
        bool specialExists = chapter.items.Exists(
            item => item != null && item.itemId == specialId
        );

        if (!specialExists || correctIds.Contains(specialId))
        {
            Debug.LogError(
                "[SPAWNER] Vật chứng đặc biệt phải tồn tại trong 20 vật phẩm và không nằm trong 5 vật chứng đúng."
            );
            return null;
        }

        // Kiểm tra 5 vật chứng đúng đều có trong danh mục 20 vật phẩm.
        foreach (string id in correctIds)
        {
            if (!chapter.items.Exists(
                item => item != null && item.itemId == id))
            {
                Debug.LogError(
                    "[SPAWNER] Không tìm thấy vật chứng đúng trong danh mục: " + id
                );
                return null;
            }
        }

        // Chọn ngẫu nhiên 3 trong 5 vật chứng đúng.
        List<string> shuffledCorrect = new List<string>(correctIds);
        Shuffle(shuffledCorrect);

        List<string> selected = new List<string>();

        for (int i = 0; i < CORRECT_COUNT; i++)
        {
            selected.Add(shuffledCorrect[i]);
        }

        // Tạo danh sách 15 vật chứng giả.
        // Vật chứng đặc biệt vẫn nằm trong danh sách này.
        List<string> decoyIds = new List<string>();

        foreach (CaseItemData item in chapter.items)
        {
            if (item == null ||
                string.IsNullOrWhiteSpace(item.itemId))
                continue;

            if (correctIds.Contains(item.itemId))
                continue;

            if (decoyIds.Contains(item.itemId))
                continue;

            decoyIds.Add(item.itemId);
        }

        if (decoyIds.Count != 15)
        {
            Debug.LogError(
                "[SPAWNER] Phải có đúng 15 vật chứng giả, hiện tại có: " +
                decoyIds.Count
            );
            return null;
        }

        // Chọn ngẫu nhiên 9 trong 15 vật chứng giả.
        // Vật chứng đặc biệt có xác suất xuất hiện 9/15 = 60%.
        Shuffle(decoyIds);

        for (int i = 0; i < DECOY_COUNT; i++)
        {
            selected.Add(decoyIds[i]);
        }

        return selected;
    }

    private bool SpawnOneEvidence(
        ChapterData chapter,
        string itemId,
        RectTransform spawnPoint,
        ExploreController controller,
        string specialEvidenceId)
    {
        CaseItemData itemData = chapter.items.Find(
            x => x != null && x.itemId == itemId
        );

        if (itemData == null)
        {
            Debug.LogError(
                "[SPAWNER] Không tìm thấy dữ liệu: " + itemId
            );
            return false;
        }

        Sprite sprite = GetSprite(itemId);

        if (sprite == null)
            return false;

        GameObject newObject = Instantiate(
            evidencePrefab,
            spawnPointsRoot
        );

        RectTransform newRect =
            newObject.GetComponent<RectTransform>();

        if (newRect == null)
        {
            Debug.LogError(
                "[SPAWNER] Prefab cần có RectTransform."
            );
            Destroy(newObject);
            return false;
        }

        newRect.anchorMin = spawnPoint.anchorMin;
        newRect.anchorMax = spawnPoint.anchorMax;
        newRect.pivot = spawnPoint.pivot;
        newRect.anchoredPosition = spawnPoint.anchoredPosition;
        newRect.sizeDelta = spawnPoint.sizeDelta;
        newRect.localScale = Vector3.one;

        EvidenceItemData evidence =
            newObject.GetComponent<EvidenceItemData>();

        Button button = newObject.GetComponent<Button>();

        if (evidence == null || button == null)
        {
            Debug.LogError(
                "[SPAWNER] Prefab thiếu Button hoặc EvidenceItemData."
            );
            Destroy(newObject);
            return false;
        }

        evidence.evidenceId = itemData.itemId;
        evidence.evidenceName = itemData.itemName;
        evidence.description = itemData.description;
        evidence.evidenceImage = sprite;
        evidence.isCollected = false;

        // Đánh dấu vật chứng đặc biệt theo JSON của Branch hiện tại.
        evidence.isSpecialEvidence =
            itemId == specialEvidenceId;

        evidence.SetupVisual();

        button.onClick.AddListener(
            () => controller.ShowEvidence(evidence)
        );

        newObject.name = "Evidence_" + itemId;

        spawnedObjects.Add(newObject);

        return true;
    }

    private Sprite GetSprite(string itemId)
    {
        if (evidenceSprites == null)
            return null;

        foreach (EvidenceSprite entry in evidenceSprites)
        {
            if (entry != null &&
                entry.itemId == itemId)
            {
                return entry.sprite;
            }
        }

        return null;
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    private void ClearSpawnedObjects()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null)
                Destroy(obj);
        }

        spawnedObjects.Clear();
    }
}
