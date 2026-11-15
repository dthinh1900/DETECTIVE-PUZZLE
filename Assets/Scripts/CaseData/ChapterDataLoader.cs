
using UnityEngine;

public class ChapterDataLoader : MonoBehaviour
{
    public ChapterData LoadChapter(int chapterNumber)
    {
        string fileName = $"Chapter{chapterNumber:00}";

        TextAsset jsonFile = Resources.Load<TextAsset>(
            $"CaseData/{fileName}"
        );

        if (jsonFile == null)
        {
            Debug.LogError(
                $"Không tìm thấy file Resources/CaseData/{fileName}.json"
            );
            return null;
        }

        ChapterData chapter =
            JsonUtility.FromJson<ChapterData>(jsonFile.text);

        if (chapter == null ||
            chapter.items == null ||
            chapter.branches == null)
        {
            Debug.LogError("Dữ liệu Chapter không hợp lệ.");
            return null;
        }

        if (chapter.branches.Count != 3)
        {
            Debug.LogError("Chapter phải có đúng 3 Branch.");
            return null;
        }

        if (chapter.items.Count != 20)
        {
            Debug.LogError("Chapter phải có đúng 20 vật chứng.");
            return null;
        }

        foreach (CaseBranchData branch in chapter.branches)
        {
            if (branch.correctEvidence == null ||
                branch.correctEvidence.Count != 5)
            {
                Debug.LogError(
                    $"Branch {branch.branchId} phải có đúng 5 vật chứng đúng."
                );
                return null;
            }
        }

        Debug.Log(
            $"Đọc thành công {chapter.chapterId}: " +
            $"{chapter.items.Count} vật chứng, " +
            $"{chapter.branches.Count} Branch."
        );

        return chapter;
    }
}
