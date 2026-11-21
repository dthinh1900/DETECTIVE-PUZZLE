using System;
using System.Collections.Generic;
using UnityEngine;

public class MainStoryLoader : MonoBehaviour
{
    private MainStoryData storyData;

    public static MainStoryData LoadChapter1Data()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("MainStory/MainStory_C1");
        if (jsonFile == null)
        {
            Debug.LogError("[MAIN STORY] Không tìm thấy MainStory_C1.json");
            return null;
        }

        MainStoryData data = JsonUtility.FromJson<MainStoryData>(jsonFile.text);
        if (data == null)
            Debug.LogError("[MAIN STORY] Không thể đọc dữ liệu JSON.");

        return data;
    }

    public static MainStoryBranchData GetBranchConfiguration(string branchId)
    {
        if (string.IsNullOrWhiteSpace(branchId))
            return null;

        MainStoryData data = LoadChapter1Data();
        if (data == null || data.branches == null)
            return null;

        return data.branches.Find(b => b != null &&
            string.Equals(b.branchId, branchId, StringComparison.OrdinalIgnoreCase));
    }

    // Mỗi Branch gồm đúng 1 hung thủ và 4 nạn nhân khác nhau.
    public static bool IsValidBranchFate(MainStoryBranchData branch)
    {
        if (branch == null)
            return false;

        string[] roleIds =
        {
            branch.killerCharacterId,
            branch.deadAfterCase1CharacterId,
            branch.deadAfterCase2CharacterId,
            branch.deadAfterCase3CharacterId,
            branch.deadAfterCase4CharacterId
        };

        HashSet<string> unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (string id in roleIds)
        {
            if (string.IsNullOrWhiteSpace(id) || !unique.Add(id.Trim()))
                return false;
        }

        HashSet<string> expected = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "BAO", "CHI", "DUNG", "GIANG", "HAI"
        };

        return unique.SetEquals(expected);
    }

    public bool LoadJson()
    {
        storyData = LoadChapter1Data();
        if (storyData == null)
            return false;

        if (storyData.commonCases == null ||
            !HasCommonCase(1) || !HasCommonCase(2) || !HasCommonCase(5))
        {
            Debug.LogError("[MAIN STORY] commonCases phải có đủ nội dung số 1, 2 và 5.");
            return false;
        }

        if (storyData.branches == null || storyData.branches.Count != 3)
        {
            Debug.LogError("[MAIN STORY] Phải có đúng 3 Branch.");
            return false;
        }

        HashSet<string> killerIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (MainStoryBranchData branch in storyData.branches)
        {
            if (branch == null || string.IsNullOrWhiteSpace(branch.branchId))
            {
                Debug.LogError("[MAIN STORY] Có Branch không hợp lệ.");
                return false;
            }

            if (!IsValidBranchFate(branch))
            {
                Debug.LogError("[MAIN STORY] Branch " + branch.branchId +
                    " phải phân vai đủ 5 nhân vật BAO, CHI, DUNG, GIANG, HAI; mỗi người chỉ xuất hiện một lần.");
                return false;
            }

            if (!killerIds.Add(branch.killerCharacterId.Trim()))
            {
                Debug.LogError("[MAIN STORY] Mỗi Branch phải có một hung thủ khác nhau.");
                return false;
            }

            if (branch.cases == null || branch.cases.Count != 2 ||
                !branch.cases.Exists(c => c != null && c.caseNumber == 3) ||
                !branch.cases.Exists(c => c != null && c.caseNumber == 4))
            {
                Debug.LogError("[MAIN STORY] Branch " + branch.branchId +
                    " phải có đúng 2 vụ án riêng (số 3 và 4).");
                return false;
            }
        }

        MainStoryCaseData finalChance = GetCommonCase(5);
        if (finalChance.dialogues == null || finalChance.dialogues.Count == 0)
        {
            Debug.LogError("[MAIN STORY] Nội dung cơ hội cuối số 5 chưa có lời thoại.");
            return false;
        }

        Debug.Log("[MAIN STORY] Đã tải JSON: common Cases 1, 2, 5 và 3 Branch có Case 3, 4.");
        return true;
    }

    private bool HasCommonCase(int number) => GetCommonCase(number) != null;

    private MainStoryCaseData GetCommonCase(int number)
    {
        if (storyData == null || storyData.commonCases == null)
            return null;

        return storyData.commonCases.Find(c => c != null && c.caseNumber == number);
    }

    public MainStoryCaseData GetCase(int caseNumber, string branchId)
    {
        if (storyData == null)
        {
            Debug.LogError("[MAIN STORY] Chưa tải JSON.");
            return null;
        }

        if (caseNumber == 1 || caseNumber == 2 || caseNumber == 5)
            return GetCommonCase(caseNumber);

        MainStoryBranchData selectedBranch = storyData.branches.Find(
            b => b != null && string.Equals(b.branchId, branchId, StringComparison.OrdinalIgnoreCase));

        if (selectedBranch == null)
        {
            Debug.LogError("[MAIN STORY] Không tìm thấy Branch: " + branchId);
            return null;
        }

        return selectedBranch.cases.Find(c => c != null && c.caseNumber == caseNumber);
    }
}
