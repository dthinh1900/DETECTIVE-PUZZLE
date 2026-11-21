using System;
using UnityEngine;

public static class CaseSessionData
{
    // CaseSessionData.CurrentCaseNumber là Case cao nhất mà cốt truyện đã mở tới.
    // Nó KHÔNG thay đổi khi người chơi quay lại xem một Case cũ.
    public static int CurrentCaseNumber { get; private set; }

    // Case đang được xem trong màn Explore. Có thể nhỏ hơn CurrentCaseNumber.
    public static int ViewingCaseNumber { get; private set; }

    public static string SelectedBranchId { get; private set; }

    // Kết quả cốt truyện Chapter 1, xác định theo Branch và giữ nguyên suốt phiên chơi.
    public static string SelectedKillerCharacterId { get; private set; }
    public static string DeadAfterCase1CharacterId { get; private set; }
    public static string DeadAfterCase2CharacterId { get; private set; }
    public static string DeadAfterCase3CharacterId { get; private set; }
    public static string DeadAfterCase4CharacterId { get; private set; }

    public static ChapterData CurrentChapter { get; private set; }
    public static CaseBranchData CurrentBranch { get; private set; }

    // Dữ liệu JSON/Branch của hiện trường đang xem trong Explore.
    public static ChapterData ViewingChapter { get; private set; }
    public static CaseBranchData ViewingBranch { get; private set; }

    public static bool IsSessionStarted { get; private set; }

    // Random Branch một lần khi mở Intro Chapter 1 (hoặc khi bắt đầu phiên ở Chapter khác).
    public static void StartNewSession(ChapterData chapter, int caseNumber = 1)
    {
        if (!IsValidChapter(chapter) || caseNumber < 1 || caseNumber > 4)
        {
            Debug.LogError("[SESSION] Chapter/Case không hợp lệ.");
            return;
        }

        int index = UnityEngine.Random.Range(0, chapter.branches.Count);
        CaseBranchData branch = chapter.branches[index];
        if (branch == null || string.IsNullOrWhiteSpace(branch.branchId))
            return;

        StartNewSession(chapter, caseNumber, branch.branchId);
    }

    // Branch của Chapter 1 được random trước hội thoại Intro; không random lại khi bấm Bắt đầu điều tra.
    public static void StartNewSession(ChapterData chapter, int caseNumber, string selectedBranchId)
    {
        ClearSession();

        if (!IsValidChapter(chapter) || caseNumber < 1 || caseNumber > 4 ||
            string.IsNullOrWhiteSpace(selectedBranchId))
        {
            Debug.LogError("[SESSION] Chapter/Case/Branch không hợp lệ.");
            return;
        }

        CaseBranchData branch = chapter.branches.Find(
            b => b != null && string.Equals(b.branchId, selectedBranchId, StringComparison.OrdinalIgnoreCase));

        if (branch == null)
        {
            Debug.LogError("[SESSION] Không tìm thấy Branch đã chọn: " + selectedBranchId);
            return;
        }

        SetCurrent(chapter, branch, caseNumber);
        GameSessionData.ClearCharacterDeaths();

        if (!ConfigureBranchFate(branch.branchId, null))
        {
            ClearSession();
            return;
        }

        Debug.Log($"[SESSION] New Game | Branch {SelectedBranchId} | Killer {SelectedKillerCharacterId} | Case {CurrentCaseNumber}");
    }

    public static string GetDeadCharacterAfterCase(int completedCaseNumber)
    {
        switch (completedCaseNumber)
        {
            case 1: return DeadAfterCase1CharacterId;
            case 2: return DeadAfterCase2CharacterId;
            case 3: return DeadAfterCase3CharacterId;
            case 4: return DeadAfterCase4CharacterId;
            default: return null;
        }
    }

    // Chỉ dùng khi cốt truyện thực sự mở sang Case kế tiếp.
    public static bool LoadNextCase(ChapterData nextChapter, int nextCaseNumber)
    {
        if (!IsSessionStarted || nextChapter == null || nextChapter.branches == null ||
            nextCaseNumber != CurrentCaseNumber + 1 || nextCaseNumber > 4)
            return false;

        CaseBranchData branch = nextChapter.branches.Find(
            b => b != null && b.branchId == SelectedBranchId
        );

        if (branch == null)
        {
            Debug.LogError("[SESSION] Không tìm thấy Branch " + SelectedBranchId + " ở Case " + nextCaseNumber);
            return false;
        }

        SetCurrent(nextChapter, branch, nextCaseNumber);
        return true;
    }

    public static bool RestoreSession(
        ChapterData chapter,
        string branchId,
        int caseNumber,
        string savedKillerCharacterId = null)
    {
        if (!IsValidChapter(chapter) || string.IsNullOrEmpty(branchId) || caseNumber < 1 || caseNumber > 4)
            return false;

        CaseBranchData branch = chapter.branches.Find(b => b != null && b.branchId == branchId);
        if (branch == null) return false;

        SetCurrent(chapter, branch, caseNumber);

        if (!ConfigureBranchFate(branchId, savedKillerCharacterId))
        {
            ClearSession();
            return false;
        }

        Debug.Log($"[SESSION] Restore | Branch {branchId} | Killer {SelectedKillerCharacterId} | Progress Case {caseNumber}");
        return true;
    }

    private static bool ConfigureBranchFate(string branchId, string savedKillerCharacterId)
    {
        SelectedKillerCharacterId = null;
        DeadAfterCase1CharacterId = null;
        DeadAfterCase2CharacterId = null;
        DeadAfterCase3CharacterId = null;
        DeadAfterCase4CharacterId = null;

        if (GameManager.Instance == null || GameManager.Instance.SelectedChapter != 1)
            return true;

        MainStoryBranchData fate = MainStoryLoader.GetBranchConfiguration(branchId);
        if (!MainStoryLoader.IsValidBranchFate(fate))
        {
            Debug.LogError("[SESSION] Cấu hình hung thủ/thứ tự sát hại không hợp lệ cho Branch " + branchId);
            return false;
        }

        SelectedKillerCharacterId = fate.killerCharacterId.Trim();
        DeadAfterCase1CharacterId = fate.deadAfterCase1CharacterId.Trim();
        DeadAfterCase2CharacterId = fate.deadAfterCase2CharacterId.Trim();
        DeadAfterCase3CharacterId = fate.deadAfterCase3CharacterId.Trim();
        DeadAfterCase4CharacterId = fate.deadAfterCase4CharacterId.Trim();

        if (!string.IsNullOrWhiteSpace(savedKillerCharacterId) &&
            !string.Equals(savedKillerCharacterId.Trim(), SelectedKillerCharacterId, StringComparison.OrdinalIgnoreCase))
        {
            Debug.LogWarning("[SESSION] Hung thủ trong Save không khớp Branch. Đã khôi phục theo cấu hình cố định của Branch " + branchId);
        }

        return true;
    }

    // Chuyển hiện trường trong Explore mà KHÔNG làm thay đổi tiến trình cốt truyện.
    // Chỉ cho phép xem những Case đã được mở (caseNumber <= CurrentCaseNumber).
    public static bool SetViewingCase(ChapterData chapter, int caseNumber)
    {
        if (!IsSessionStarted || caseNumber < 1 || caseNumber > CurrentCaseNumber || !IsValidChapter(chapter))
        {
            Debug.LogWarning("[SESSION] Case chưa mở khóa hoặc dữ liệu hiện trường không hợp lệ: " + caseNumber);
            return false;
        }

        CaseBranchData branch = chapter.branches.Find(
            b => b != null && b.branchId == SelectedBranchId
        );

        if (branch == null)
        {
            Debug.LogError("[SESSION] Case " + caseNumber + " không có Branch " + SelectedBranchId);
            return false;
        }

        ViewingCaseNumber = caseNumber;
        ViewingChapter = chapter;
        ViewingBranch = branch;
        Debug.Log("[SESSION] Đang xem hiện trường Case " + caseNumber + " | Branch " + SelectedBranchId);
        return true;
    }

    private static bool IsValidChapter(ChapterData chapter)
    {
        return chapter != null && chapter.items != null && chapter.items.Count == 20 &&
               chapter.branches != null && chapter.branches.Count == 3;
    }

    private static void SetCurrent(ChapterData chapter, CaseBranchData branch, int caseNumber)
    {
        CurrentChapter = chapter;
        CurrentBranch = branch;
        SelectedBranchId = branch.branchId;
        CurrentCaseNumber = caseNumber;

        // Khi New Game hoặc cốt truyện mở Case mới, mặc định hiện trường đang xem là Case mới.
        ViewingCaseNumber = caseNumber;
        ViewingChapter = chapter;
        ViewingBranch = branch;
        IsSessionStarted = true;
    }

    public static void ClearSession()
    {
        CurrentChapter = null;
        CurrentBranch = null;
        ViewingChapter = null;
        ViewingBranch = null;
        SelectedBranchId = null;
        SelectedKillerCharacterId = null;
        DeadAfterCase1CharacterId = null;
        DeadAfterCase2CharacterId = null;
        DeadAfterCase3CharacterId = null;
        DeadAfterCase4CharacterId = null;
        CurrentCaseNumber = 0;
        ViewingCaseNumber = 0;
        IsSessionStarted = false;
    }

    public static bool IsCorrectEvidence(string itemId)
    {
        return CurrentBranch != null && CurrentBranch.correctEvidence != null &&
               CurrentBranch.correctEvidence.Exists(x => x != null && x.itemId == itemId);
    }

    public static string GetEvidenceDetail(string itemId)
    {
        if (CurrentChapter == null || CurrentChapter.items == null || CurrentBranch == null) return "";
        CaseItemData item = CurrentChapter.items.Find(x => x != null && x.itemId == itemId);
        if (item == null) return "";

        if (IsCorrectEvidence(itemId))
        {
            CorrectEvidenceData correct = CurrentBranch.correctEvidence.Find(x => x != null && x.itemId == itemId);
            return correct != null ? correct.killerInfo : "";
        }

        return item.decoyDetail;
    }
}
