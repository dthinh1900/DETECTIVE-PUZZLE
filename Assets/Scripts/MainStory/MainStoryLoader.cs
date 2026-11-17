using UnityEngine;

public class MainStoryLoader : MonoBehaviour
{
    private MainStoryData storyData;

    public bool LoadJson()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("MainStory/MainStory_C1");
        if (jsonFile == null)
        {
            Debug.LogError("[MAIN STORY] Không tìm thấy MainStory_C1.json");
            return false;
        }

        storyData = JsonUtility.FromJson<MainStoryData>(jsonFile.text);
        if (storyData == null)
        {
            Debug.LogError("[MAIN STORY] Không thể đọc dữ liệu JSON.");
            return false;
        }

        // CommonCases phải có nội dung số 1, số 2 và số 5.
        // Số 5 là hội thoại cơ hội cuối, dùng chung cho cả ba Branch.
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

        foreach (MainStoryBranchData branch in storyData.branches)
        {
            if (branch == null || string.IsNullOrEmpty(branch.branchId))
            {
                Debug.LogError("[MAIN STORY] Có Branch không hợp lệ.");
                return false;
            }

            if (branch.cases == null || branch.cases.Count != 2)
            {
                Debug.LogError("[MAIN STORY] Branch " + branch.branchId +
                               " phải có đúng 2 vụ án riêng (số 3 và 4).");
                return false;
            }

            bool hasCase3 = branch.cases.Exists(c => c != null && c.caseNumber == 3);
            bool hasCase4 = branch.cases.Exists(c => c != null && c.caseNumber == 4);
            if (!hasCase3 || !hasCase4)
            {
                Debug.LogError("[MAIN STORY] Branch " + branch.branchId +
                               " thiếu nội dung số 3 hoặc số 4.");
                return false;
            }
        }

        MainStoryCaseData finalChance = GetCommonCase(5);
        if (finalChance.dialogues == null || finalChance.dialogues.Count == 0)
        {
            Debug.LogError("[MAIN STORY] Nội dung cơ hội cuối số 5 chưa có lời thoại.");
            return false;
        }

        Debug.Log("[MAIN STORY] Đã tải JSON: nội dung chung số 1, 2, 5 và 3 Branch có nội dung số 3, 4.");
        return true;
    }

    private bool HasCommonCase(int number)
    {
        return GetCommonCase(number) != null;
    }

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

        // Số 1, 2 và 5 dùng chung cho cả ba Branch.
        if (caseNumber == 1 || caseNumber == 2 || caseNumber == 5)
            return GetCommonCase(caseNumber);

        // Số 3 và 4 lấy theo Branch đã chọn.
        MainStoryBranchData selectedBranch = storyData.branches.Find(
            b => b != null && b.branchId == branchId
        );

        if (selectedBranch == null)
        {
            Debug.LogError("[MAIN STORY] Không tìm thấy Branch: " + branchId);
            return null;
        }

        return selectedBranch.cases.Find(c => c != null && c.caseNumber == caseNumber);
    }
}
