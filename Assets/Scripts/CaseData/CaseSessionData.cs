
using System.Collections.Generic;

public static class CaseSessionData
{
    public static string SelectedBranchId { get; private set; }

    public static ChapterData CurrentChapter { get; private set; }

    public static CaseBranchData CurrentBranch { get; private set; }

    public static bool IsSessionStarted { get; private set; }

    public static void StartNewSession(ChapterData chapter)
    {
        CurrentChapter = chapter;
        SelectedBranchId = null;
        CurrentBranch = null;
        IsSessionStarted = false;

        if (chapter == null || chapter.branches == null ||
            chapter.branches.Count != 3)
        {
            UnityEngine.Debug.LogError(
                "Chapter không hợp lệ hoặc không có đúng 3 Branch."
            );
            return;
        }

        int randomIndex = UnityEngine.Random.Range(
            0, chapter.branches.Count
        );

        CurrentBranch = chapter.branches[randomIndex];
        SelectedBranchId = CurrentBranch.branchId;
        IsSessionStarted = true;

        UnityEngine.Debug.Log(
            $"Đã chọn Branch: {SelectedBranchId}"
        );
    }

    public static void ClearSession()
    {
        CurrentChapter = null;
        CurrentBranch = null;
        SelectedBranchId = null;
        IsSessionStarted = false;
    }

    public static bool IsCorrectEvidence(string itemId)
    {
        if (CurrentBranch == null ||
            CurrentBranch.correctEvidence == null)
            return false;

        return CurrentBranch.correctEvidence.Exists(
            item => item.itemId == itemId
        );
    }

    public static string GetEvidenceDetail(string itemId)
    {
        if (CurrentChapter == null || CurrentBranch == null)
            return "";

        CaseItemData item = CurrentChapter.items.Find(
            x => x.itemId == itemId
        );

        if (item == null)
            return "";

        if (IsCorrectEvidence(itemId))
        {
            CorrectEvidenceData correct =
                CurrentBranch.correctEvidence.Find(
                    x => x.itemId == itemId
                );

            return correct != null ? correct.killerInfo : "";
        }

        return item.decoyDetail;
    }
}
