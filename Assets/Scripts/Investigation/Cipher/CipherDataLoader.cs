using System;
using System.Collections.Generic;
using UnityEngine;

public static class CipherDataLoader
{
    private const string CHAPTER1_RESOURCE_PATH = "CipherData/C1_Cipher";
    private static CipherChapterData cachedChapter1;
    private static string cachedJsonText;

    public static CipherChapterData LoadChapter1Data(bool forceReload = false)
    {
        TextAsset json = Resources.Load<TextAsset>(CHAPTER1_RESOURCE_PATH);
        if (json == null)
        {
            Debug.LogError("[CIPHER] Không tìm thấy Resources/" + CHAPTER1_RESOURCE_PATH + ".json");
            return null;
        }

        if (!forceReload && cachedChapter1 != null && cachedJsonText == json.text)
            return cachedChapter1;

        CipherChapterData data = JsonUtility.FromJson<CipherChapterData>(json.text);
        if (data == null || data.chapterId != 1 || data.branches == null || data.branches.Count != 3)
        {
            Debug.LogError("[CIPHER] C1_Cipher.json không đúng cấu trúc hoặc phải có đúng 3 Branch.");
            cachedChapter1 = null;
            cachedJsonText = null;
            return null;
        }

        string[] requiredBranches = { "BR01", "BR02", "BR03" };
        HashSet<string> allCipherIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (string branchId in requiredBranches)
        {
            CipherBranchData branch = data.branches.Find(b => b != null &&
                string.Equals(b.branchId, branchId, StringComparison.OrdinalIgnoreCase));

            MainStoryBranchData fate = MainStoryLoader.GetBranchConfiguration(branchId);
            if (branch == null || branch.ciphers == null || branch.ciphers.Count != 4 ||
                !MainStoryLoader.IsValidBranchFate(fate))
            {
                Debug.LogError("[CIPHER] Branch " + branchId + " phải có đủ 4 mật mã và cấu hình tử vong hợp lệ.");
                return null;
            }

            string[] expectedTargets =
            {
                fate.deadAfterCase1CharacterId,
                fate.deadAfterCase2CharacterId,
                fate.deadAfterCase3CharacterId,
                fate.deadAfterCase4CharacterId
            };

            for (int receiveAfterCase = 0; receiveAfterCase <= 3; receiveAfterCase++)
            {
                CipherEntryData cipher = branch.ciphers.Find(c => c != null && c.caseNumber == receiveAfterCase);
                if (cipher == null ||
                    string.IsNullOrWhiteSpace(cipher.cipherId) ||
                    string.IsNullOrWhiteSpace(cipher.targetCharacterId) ||
                    string.IsNullOrWhiteSpace(cipher.imageResourcePath))
                {
                    Debug.LogError("[CIPHER] Branch " + branchId + " thiếu mật mã ở thứ tự " + receiveAfterCase + ".");
                    return null;
                }

                if (!allCipherIds.Add(cipher.cipherId.Trim()))
                {
                    Debug.LogError("[CIPHER] cipherId bị trùng: " + cipher.cipherId);
                    return null;
                }

                if (!string.Equals(cipher.targetCharacterId.Trim(), expectedTargets[receiveAfterCase],
                    StringComparison.OrdinalIgnoreCase))
                {
                    Debug.LogError("[CIPHER] Mục tiêu " + cipher.cipherId +
                        " không khớp thứ tự sát hại của " + branchId + ". Mong đợi: " +
                        expectedTargets[receiveAfterCase]);
                    return null;
                }

                if (string.Equals(cipher.targetCharacterId.Trim(), fate.killerCharacterId,
                    StringComparison.OrdinalIgnoreCase))
                {
                    Debug.LogError("[CIPHER] Không được tạo mật mã hướng đến hung thủ: " + branchId);
                    return null;
                }
            }
        }

        cachedChapter1 = data;
        cachedJsonText = json.text;
        return cachedChapter1;
    }

    public static CipherEntryData GetIntroCipher(string branchId)
    {
        CipherBranchData branch = GetBranch(branchId);
        return branch != null && branch.ciphers != null
            ? branch.ciphers.Find(c => c != null && c.caseNumber == 0)
            : null;
    }

    // Sau MainStory Case 1, 2, 3 lần lượt nhận mật mã 1, 2, 3.
    // Không có mật mã thưởng sau Case 4.
    public static CipherEntryData GetStoryCipher(string branchId, int completedCaseNumber)
    {
        if (string.IsNullOrWhiteSpace(branchId) || completedCaseNumber < 1 || completedCaseNumber > 3)
            return null;

        CipherBranchData branch = GetBranch(branchId);
        return branch != null && branch.ciphers != null
            ? branch.ciphers.Find(c => c != null && c.caseNumber == completedCaseNumber)
            : null;
    }

    public static List<CipherEntryData> GetCiphersForBranch(string branchId)
    {
        List<CipherEntryData> result = new List<CipherEntryData>();
        CipherBranchData branch = GetBranch(branchId);
        if (branch == null || branch.ciphers == null)
            return result;

        result.AddRange(branch.ciphers.FindAll(c => c != null));
        result.Sort((a, b) => a.caseNumber.CompareTo(b.caseNumber));
        return result;
    }

    public static Sprite LoadCipherImage(CipherEntryData cipher)
    {
        if (cipher == null || string.IsNullOrWhiteSpace(cipher.imageResourcePath))
            return null;

        Sprite sprite = Resources.Load<Sprite>(cipher.imageResourcePath.Trim());
        if (sprite == null)
        {
            Debug.LogWarning("[CIPHER] Không tìm thấy Sprite tại Resources/" +
                             cipher.imageResourcePath + " (đường dẫn trong C1_Cipher.json).");
        }
        return sprite;
    }

    public static string GetCharacterDisplayName(string characterId)
    {
        if (string.IsNullOrWhiteSpace(characterId))
            return "Chưa xác định";

        switch (characterId.Trim().ToUpperInvariant())
        {
            case "BAO": return "Bảo";
            case "CHI": return "Chi";
            case "DUNG": return "Dũng";
            case "GIANG": return "Giang";
            case "HAI": return "Hải";
            default: return characterId;
        }
    }

    private static CipherBranchData GetBranch(string branchId)
    {
        if (string.IsNullOrWhiteSpace(branchId))
            return null;

        CipherChapterData data = LoadChapter1Data();
        if (data == null || data.branches == null)
            return null;

        return data.branches.Find(b => b != null &&
            string.Equals(b.branchId, branchId, StringComparison.OrdinalIgnoreCase));
    }
}
