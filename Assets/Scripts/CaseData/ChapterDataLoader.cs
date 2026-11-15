
using UnityEngine;

public class ChapterDataLoader : MonoBehaviour
{
    public ChapterData LoadCase(int chapterNumber, int caseNumber)
    {
        string fileName = $"C{chapterNumber}_Vuan{caseNumber}";

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
            Debug.LogError($"Dữ liệu {fileName}.json không hợp lệ.");
            return null;
        }

        if (chapter.items.Count != 20)
        {
            Debug.LogError($"{fileName} phải có đúng 20 vật chứng.");
            return null;
        }

        if (chapter.branches.Count != 3)
        {
            Debug.LogError($"{fileName} phải có đúng 3 Branch.");
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

            if (string.IsNullOrEmpty(branch.specialEvidenceId))
            {
                Debug.LogError(
                    $"Branch {branch.branchId} chưa có specialEvidenceId."
                );
                return null;
            }

            bool specialIsCorrect = branch.correctEvidence.Exists(
                item => item.itemId == branch.specialEvidenceId
            );

            bool specialExists = chapter.items.Exists(
                item => item.itemId == branch.specialEvidenceId
            );

            if (!specialExists || specialIsCorrect)
            {
                Debug.LogError(
                    $"Vật chứng đặc biệt của {branch.branchId} phải nằm trong 15 vật chứng giả."
                );
                return null;
            }
        }

        Debug.Log(
            $"Đọc thành công {fileName}: " +
            $"{chapter.items.Count} vật chứng, " +
            $"{chapter.branches.Count} Branch."
        );

        return chapter;
    }
}
