using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EvidenceSpawner : MonoBehaviour
{
    [System.Serializable]
    public class EvidenceSprite { public string itemId; public Sprite sprite; }

    [Header("Prefab")]
    [SerializeField] private GameObject evidencePrefab;
    [Header("Thiết lập cũ - giữ để tương thích Inspector")]
    [SerializeField] private Transform spawnPointsRoot;
    [SerializeField] private RectTransform[] spawnPoints;
    [Header("Ảnh vật chứng - cần gán đủ ID của các Case")]
    [SerializeField] private EvidenceSprite[] evidenceSprites;

    private const int CORRECT_COUNT = 3;
    private const int DECOY_COUNT = 9;
    private const int TOTAL_COUNT = 12;
    private readonly List<GameObject> spawnedObjects = new List<GameObject>();
    private static readonly Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>();

    private void Awake() { RegisterSprites(); }
    private void OnEnable() { RegisterSprites(); }

    private void RegisterSprites()
    {
        if (evidenceSprites == null) return;
        foreach (EvidenceSprite entry in evidenceSprites)
            if (entry != null && !string.IsNullOrWhiteSpace(entry.itemId) && entry.sprite != null)
                SpriteCache[entry.itemId] = entry.sprite;
    }

    public static Sprite FindSprite(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        if (SpriteCache.TryGetValue(id, out Sprite sprite) && sprite != null) return sprite;
        EvidenceSpawner[] spawners = FindObjectsByType<EvidenceSpawner>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (EvidenceSpawner spawner in spawners) { spawner.RegisterSprites(); if (SpriteCache.TryGetValue(id, out sprite)) return sprite; }
        return null;
    }

    // Tương thích với cách gọi cũ.
    public void SpawnEvidence(ExploreController controller)
    {
        Transform group = spawnPointsRoot != null ? spawnPointsRoot : transform;
        SpawnEvidence(controller, group);
    }

    public void SpawnEvidence(ExploreController controller, Transform evidenceGroup)
    {
        SpawnEvidence(
            controller,
            evidenceGroup,
            CaseSessionData.CurrentChapter,
            CaseSessionData.CurrentBranch,
            CaseSessionData.CurrentCaseNumber
        );
    }

    // Dùng overload này khi người chơi chuyển qua một hiện trường Case cũ.
    // Không lấy CurrentCaseNumber vì đó là Case cao nhất của tiến trình cốt truyện.
    public void SpawnEvidence(
        ExploreController controller,
        Transform evidenceGroup,
        ChapterData chapter,
        CaseBranchData branch,
        int caseNumber)
    {
        if (controller == null || evidenceGroup == null)
        {
            Debug.LogError("[SPAWNER] Thiếu ExploreController/EvidenceGroup.");
            return;
        }

        if (!CaseSessionData.IsSessionStarted || chapter == null || branch == null || caseNumber < 1)
        {
            Debug.LogError("[SPAWNER] Dữ liệu Case/Branch chưa được khởi tạo.");
            return;
        }

        if (evidencePrefab == null)
        {
            Debug.LogError("[SPAWNER] Chưa gán Evidence Prefab.");
            return;
        }

        List<RectTransform> points = FindSpawnPoints(evidenceGroup);
        if (points.Count != TOTAL_COUNT)
        {
            Debug.LogError($"[SPAWNER] EvidenceGroup {evidenceGroup.name} phải có đúng 12 SpawnPoint, hiện có {points.Count}.");
            return;
        }

        List<SpawnedEvidenceSaveData> saved = GameSessionData.GetSavedSpawnedEvidence(caseNumber);

        if (saved == null)
        {
            List<string> ids = SelectEvidenceIds(chapter, branch);
            if (ids == null || ids.Count != TOTAL_COUNT) return;
            Shuffle(ids);
            saved = new List<SpawnedEvidenceSaveData>();
            for (int i = 0; i < TOTAL_COUNT; i++)
            {
                RectTransform p = points[i];
                saved.Add(new SpawnedEvidenceSaveData
                {
                    evidenceId = ids[i],
                    spawnPointIndex = i,
                    anchoredPosition = p.anchoredPosition,
                    anchorMin = p.anchorMin,
                    anchorMax = p.anchorMax,
                    pivot = p.pivot,
                    sizeDelta = p.sizeDelta,
                    localScale = p.localScale
                });
            }
            GameSessionData.SaveSpawnedEvidence(caseNumber, saved);
        }

        if (saved.Count != TOTAL_COUNT) { Debug.LogError("[SPAWNER] Save của Case không có đúng 12 vật chứng."); return; }
        Transform parent = points[0].parent != null ? points[0].parent : evidenceGroup;

        foreach (SpawnedEvidenceSaveData entry in saved)
        {
            if (entry == null || string.IsNullOrEmpty(entry.evidenceId) || FindSprite(entry.evidenceId) == null)
            {
                Debug.LogError("[SPAWNER] Thiếu ID hoặc Sprite: " + (entry == null ? "null" : entry.evidenceId));
                ClearSpawnedObjects(); return;
            }
        }

        ClearSpawnedObjects();
        foreach (SpawnedEvidenceSaveData entry in saved)
        {
            if (entry.spawnPointIndex < 0 || entry.spawnPointIndex >= TOTAL_COUNT) { Debug.LogError("[SPAWNER] SpawnPointIndex không hợp lệ."); ClearSpawnedObjects(); return; }
            SpawnOne(chapter, branch, entry, parent, controller, caseNumber);
        }
        Debug.Log($"[SPAWNER] Đã khôi phục/sinh 12 vật chứng cho Case {caseNumber}, Branch {branch.branchId}.");
    }

    private List<RectTransform> FindSpawnPoints(Transform group)
    {
        List<RectTransform> result = new List<RectTransform>();
        RectTransform[] all = group.GetComponentsInChildren<RectTransform>(true);
        foreach (RectTransform rect in all)
            if (rect != group && IsSpawnPointName(rect.name)) result.Add(rect);
        result.Sort((a, b) => ExtractNumber(a.name).CompareTo(ExtractNumber(b.name)));
        if (result.Count == 0 && spawnPoints != null)
            foreach (RectTransform p in spawnPoints) if (p != null) result.Add(p);
        return result;
    }

    private bool IsSpawnPointName(string value)
    {
        if (string.IsNullOrEmpty(value)) return false;
        string lower = value.ToLowerInvariant();
        if (lower.StartsWith("spawnpoint_") || lower.StartsWith("spawnpoint ")) return true;
        return lower.StartsWith("spawnpoint") && lower.Length > 10 && char.IsDigit(lower[10]);
    }

    private int ExtractNumber(string value)
    {
        int number = 0; string digits = "";
        for (int i = value.Length - 1; i >= 0 && char.IsDigit(value[i]); i--) digits = value[i] + digits;
        return int.TryParse(digits, out number) ? number : int.MaxValue;
    }

    private List<string> SelectEvidenceIds(ChapterData chapter, CaseBranchData branch)
    {
        if (chapter.items == null || chapter.items.Count != 20 || branch.correctEvidence == null || branch.correctEvidence.Count != 5) return null;
        List<string> correct = new List<string>();
        foreach (CorrectEvidenceData item in branch.correctEvidence)
        {
            if (item == null || string.IsNullOrEmpty(item.itemId) || correct.Contains(item.itemId)) { Debug.LogError("[SPAWNER] Danh sách vật chứng đúng không hợp lệ."); return null; }
            correct.Add(item.itemId);
        }
        if (!chapter.items.Exists(x => x != null && x.itemId == branch.specialEvidenceId) || correct.Contains(branch.specialEvidenceId)) { Debug.LogError("[SPAWNER] Special Evidence phải là một vật chứng giả có trong JSON."); return null; }
        List<string> selected = new List<string>();
        Shuffle(correct);
        for (int i = 0; i < CORRECT_COUNT; i++) selected.Add(correct[i]);
        List<string> decoys = new List<string>();
        foreach (CaseItemData item in chapter.items)
            if (item != null && !string.IsNullOrEmpty(item.itemId) && !correct.Contains(item.itemId)) decoys.Add(item.itemId);
        if (decoys.Count != 15) { Debug.LogError("[SPAWNER] Không đủ 15 vật chứng giả."); return null; }
        Shuffle(decoys);
        for (int i = 0; i < DECOY_COUNT; i++) selected.Add(decoys[i]);
        return selected;
    }

    private void SpawnOne(ChapterData chapter, CaseBranchData branch, SpawnedEvidenceSaveData saved, Transform parent, ExploreController controller, int caseNumber)
    {
        CaseItemData item = chapter.items.Find(x => x != null && x.itemId == saved.evidenceId);
        if (item == null) { Debug.LogError("[SPAWNER] Không có dữ liệu " + saved.evidenceId); return; }
        GameObject obj = Instantiate(evidencePrefab, parent);
        RectTransform rect = obj.GetComponent<RectTransform>();
        EvidenceItemData data = obj.GetComponent<EvidenceItemData>();
        Button button = obj.GetComponent<Button>();
        if (rect == null || data == null || button == null) { Debug.LogError("[SPAWNER] Prefab cần RectTransform, EvidenceItemData và Button."); Destroy(obj); return; }
        rect.anchorMin = saved.anchorMin; rect.anchorMax = saved.anchorMax; rect.pivot = saved.pivot;
        rect.anchoredPosition = saved.anchoredPosition; rect.sizeDelta = saved.sizeDelta;
        rect.localScale = saved.localScale == Vector3.zero ? Vector3.one : saved.localScale;
        data.evidenceId = item.itemId; data.evidenceName = item.itemName; data.description = item.description;
        data.evidenceImage = FindSprite(item.itemId); data.isCollected = GameSessionData.IsEvidenceCollected(caseNumber, item.itemId);
        data.isSpecialEvidence = item.itemId == branch.specialEvidenceId;
        data.SetupVisual();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => controller.ShowEvidence(data));
        obj.name = "Evidence_" + item.itemId;
        obj.SetActive(!data.isCollected);
        spawnedObjects.Add(obj);
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--) { int j = Random.Range(0, i + 1); T t = list[i]; list[i] = list[j]; list[j] = t; }
    }

    private void ClearSpawnedObjects()
    {
        foreach (GameObject obj in spawnedObjects) if (obj != null) Destroy(obj);
        spawnedObjects.Clear();
    }
}
