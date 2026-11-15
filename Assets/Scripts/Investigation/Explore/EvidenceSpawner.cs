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

    [Header("Cấu hình")]
    [SerializeField] private string specialEvidenceId = "E016";

    private const int CORRECT_COUNT = 3;
    private const int DECOY_COUNT = 8;

    private bool hasSpawned;

    private readonly List<GameObject> spawnedObjects =
        new List<GameObject>();

    public void SpawnEvidence(ExploreController controller)
    {
        if (hasSpawned)
            return;

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
            spawnPoints.Length != 12)
        {
            Debug.LogError(
                "[SPAWNER] Chưa gán Prefab hoặc đủ 12 SpawnPoint."
            );
            return;
        }

        ChapterData chapter = CaseSessionData.CurrentChapter;
        CaseBranchData branch = CaseSessionData.CurrentBranch;

        List<string> selectedIds = SelectEvidenceIds(chapter, branch);

        if (selectedIds == null || selectedIds.Count != 12)
            return;

        Shuffle(selectedIds);

        for (int i = 0; i < 12; i++)
        {
            if (spawnPoints[i] == null)
            {
                Debug.LogError(
                    "[SPAWNER] SpawnPoint " + i + " chưa được gán."
                );
                ClearSpawnedObjects();
                return;
            }
        }

        for (int i = 0; i < selectedIds.Count; i++)
        {
            SpawnOneEvidence(
                chapter,
                selectedIds[i],
                spawnPoints[i],
                controller
            );
        }

        hasSpawned = true;

        Debug.Log(
            "[SPAWNER] Đã tạo 12 vật phẩm cho Branch " +
            branch.branchId
        );
    }

    private List<string> SelectEvidenceIds(
        ChapterData chapter,
        CaseBranchData branch)
    {
        if (chapter.items == null ||
            branch.correctEvidence == null ||
            branch.correctEvidence.Count != 5)
        {
            Debug.LogError("[SPAWNER] Dữ liệu Chapter/Branch không hợp lệ.");
            return null;
        }

        List<string> correctIds = new List<string>();

        foreach (CorrectEvidenceData evidence in branch.correctEvidence)
        {
            if (evidence != null &&
                !string.IsNullOrWhiteSpace(evidence.itemId))
            {
                correctIds.Add(evidence.itemId);
            }
        }

        if (correctIds.Count != 5)
        {
            Debug.LogError("[SPAWNER] Không đủ 5 vật chứng đúng.");
            return null;
        }

        if (!correctIds.Contains(specialEvidenceId) &&
            chapter.items.Exists(x => x.itemId == specialEvidenceId))
        {
            // E016 là vật phẩm đặc biệt, không phải vật chứng đúng.
        }
        else
        {
            Debug.LogError(
                "[SPAWNER] E016 không hợp lệ hoặc đang nằm trong danh sách đúng."
            );
            return null;
        }

        List<string> selected = new List<string>();

        List<string> shuffledCorrect = new List<string>(correctIds);
        Shuffle(shuffledCorrect);

        // Chọn 3 trong 5 vật chứng đúng.
        for (int i = 0; i < CORRECT_COUNT; i++)
            selected.Add(shuffledCorrect[i]);

        // Luôn thêm vật phẩm đặc biệt.
        selected.Add(specialEvidenceId);

        // Tạo danh sách giả, loại cả 5 vật chứng đúng và E016.
        List<string> decoys = new List<string>();

        foreach (CaseItemData item in chapter.items)
        {
            if (item == null ||
                string.IsNullOrWhiteSpace(item.itemId))
                continue;

            if (correctIds.Contains(item.itemId))
                continue;

            if (item.itemId == specialEvidenceId)
                continue;

            decoys.Add(item.itemId);
        }

        if (decoys.Count < DECOY_COUNT)
        {
            Debug.LogError("[SPAWNER] Không đủ vật phẩm giả.");
            return null;
        }

        Shuffle(decoys);

        for (int i = 0; i < DECOY_COUNT; i++)
            selected.Add(decoys[i]);

        return selected;
    }

    private void SpawnOneEvidence(
        ChapterData chapter,
        string itemId,
        RectTransform spawnPoint,
        ExploreController controller)
    {
        CaseItemData itemData = chapter.items.Find(
            x => x.itemId == itemId
        );

        if (itemData == null)
        {
            Debug.LogError(
                "[SPAWNER] Không tìm thấy dữ liệu: " + itemId
            );
            return;
        }

        Sprite sprite = GetSprite(itemId);

        if (sprite == null)
        {
            Debug.LogError(
                "[SPAWNER] Chưa gán ảnh cho " + itemId
            );
            return;
        }

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
            return;
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
            return;
        }

        evidence.evidenceId = itemData.itemId;
        evidence.evidenceName = itemData.itemName;
        evidence.description = itemData.description;
        evidence.evidenceImage = sprite;
        evidence.isCollected = false;

        evidence.SetupVisual();

        button.onClick.AddListener(
            () => controller.ShowEvidence(evidence)
        );

        newObject.name = "Evidence_" + itemId;

        spawnedObjects.Add(newObject);
    }

    private Sprite GetSprite(string itemId)
    {
        if (evidenceSprites == null)
            return null;

        foreach (EvidenceSprite entry in evidenceSprites)
        {
            if (entry != null &&
                entry.itemId == itemId)
                return entry.sprite;
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