using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StoryController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject sceneStory;
    [SerializeField] private GameObject storyDetailPanel;

    [Header("Danh sach Story")]
    [SerializeField] private Transform storyContent;
    [SerializeField] private Button storyItemPrefab;

    [Header("Chapter 01 JSON")]
    [SerializeField] private TextAsset chapter1StoryJson;

    [Header("Icon Story dac biet")]
    [SerializeField] private Sprite questionMarkSprite;

    [Header("Mau nen")]
    [SerializeField]
    private Color lockedColor =
        new Color(0.27f, 0.27f, 0.27f, 1f);

    [SerializeField]
    private Color unlockedColor =
        new Color(1f, 0.9f, 0.65f, 1f);

    [Header("Story Detail")]
    [SerializeField]
    private StoryDetailController storyDetailController;

    private StoryDetailController.ChapterStoryData chapterData;
    private StoryDetailController.BranchStoryData selectedBranch;

    private IEnumerator Start()
    {
        // Cho StoryDetailController khoi tao UI truoc.
        yield return null;

        if (!LoadChapterData())
            yield break;

        if (storyDetailController == null)
        {
            Debug.LogError(
                "StoryController: Chua gan StoryDetailController."
            );
            yield break;
        }

        storyDetailController.SetChapterData(
            chapterData,
            CaseSessionData.SelectedBranchId
        );

        ShowStoryList();
        GenerateStoryItems();

        // Neu vua thu thap Evidence dac biet,
        // tu dong mo Story tuong ung.
        TryOpenPendingSpecialStory();
    }

    private bool LoadChapterData()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError(
                "StoryController: Khong tim thay GameManager."
            );
            return false;
        }

        if (GameManager.Instance.SelectedChapter != 1)
        {
            Debug.LogError(
                "StoryController hien tai chi ho tro Chapter 01."
            );
            return false;
        }

        if (!CaseSessionData.IsSessionStarted ||
            string.IsNullOrWhiteSpace(CaseSessionData.SelectedBranchId))
        {
            Debug.LogError(
                "StoryController: Chua co Branch duoc chon trong phien choi."
            );
            return false;
        }

        if (chapter1StoryJson == null)
        {
            Debug.LogError(
                "StoryController: Chua gan Chapter01Story.json."
            );
            return false;
        }

        chapterData =
            JsonUtility.FromJson<
                StoryDetailController.ChapterStoryData
            >(chapter1StoryJson.text);

        if (chapterData == null ||
            chapterData.branches == null)
        {
            Debug.LogError(
                "Chapter01Story.json khong hop le hoac thieu branches."
            );
            return false;
        }

        selectedBranch = null;

        foreach (
            StoryDetailController.BranchStoryData branch
            in chapterData.branches
        )
        {
            if (branch != null &&
                branch.branchId == CaseSessionData.SelectedBranchId)
            {
                selectedBranch = branch;
                break;
            }
        }

        if (selectedBranch == null)
        {
            Debug.LogError(
                "Khong tim thay Story cua Branch: " +
                CaseSessionData.SelectedBranchId
            );
            return false;
        }

        if (selectedBranch.stories == null ||
            selectedBranch.stories.Length != 2)
        {
            Debug.LogError(
                "Moi Branch cua Chapter 01 phai co dung 2 Story."
            );
            return false;
        }

        Debug.Log(
            "Da tai " + selectedBranch.stories.Length +
            " Story cua " + selectedBranch.branchId
        );

        return true;
    }

    private void GenerateStoryItems()
    {
        if (storyContent == null || storyItemPrefab == null)
        {
            Debug.LogError(
                "StoryController: Chua gan Content hoac StoryItemPrefab."
            );
            return;
        }

        if (selectedBranch == null ||
            selectedBranch.stories == null)
        {
            return;
        }

        foreach (Transform child in storyContent)
        {
            Destroy(child.gameObject);
        }

        foreach (
            StoryDetailController.StoryData story
            in selectedBranch.stories
        )
        {
            if (story == null)
                continue;

            Button item = Instantiate(
                storyItemPrefab,
                storyContent
            );

            StoryBTN storyButton =
                item.GetComponent<StoryBTN>();

            if (storyButton == null)
            {
                Debug.LogError(
                    "StoryController: Prefab chua co StoryBTN."
                );

                Destroy(item.gameObject);
                continue;
            }

            bool unlocked;

            Sprite[] icons = BuildStoryIcons(
                story,
                out unlocked
            );

            string title =
                string.IsNullOrWhiteSpace(story.title)
                    ? story.storyId
                    : story.title;

            storyButton.Setup(
                title,
                icons[0],
                icons[1],
                icons[2],
                unlocked,
                lockedColor,
                unlockedColor,
                () => OpenStory(story)
            );
        }
    }


    private Sprite[] BuildStoryIcons(
        StoryDetailController.StoryData story,
        out bool unlocked)
    {
        Sprite[] icons = new Sprite[3];
        unlocked = false;

        if (story == null)
            return icons;

        // STORY ĐẶC BIỆT: chỉ hiện dấu hỏi ở ô giữa.
        if (story.storyType == "special")
        {
            icons[1] = questionMarkSprite;

            unlocked =
                !string.IsNullOrWhiteSpace(story.requiredEvidenceId) &&
                GameSessionData.IsEvidenceCollected(
                    story.requiredEvidenceId
                );

            if (unlocked)
            {
                CollectedEvidenceInfo info =
                    FindCollectedEvidence(story.requiredEvidenceId);

                if (info != null && info.evidenceImage != null)
                    icons[1] = info.evidenceImage;
            }

            return icons;
        }

        // STORY THƯỜNG: mặc định cả 3 ô đều là dấu hỏi.
        if (story.storyType == "normal")
        {
            for (int i = 0; i < 3; i++)
                icons[i] = questionMarkSprite;

            if (story.candidateEvidenceIds == null ||
                story.candidateEvidenceIds.Length != 4)
            {
                Debug.LogWarning(
                    story.storyId +
                    ": Can khai bao dung 4 candidateEvidenceIds."
                );

                return icons;
            }

            HashSet<string> countedIds =
                new HashSet<string>();

            int collectedCount = 0;

            foreach (string evidenceId in story.candidateEvidenceIds)
            {
                if (string.IsNullOrWhiteSpace(evidenceId))
                    continue;

                if (!countedIds.Add(evidenceId))
                    continue;

                if (!GameSessionData.IsEvidenceCollected(evidenceId))
                    continue;

                if (collectedCount < 3)
                {
                    CollectedEvidenceInfo info =
                        FindCollectedEvidence(evidenceId);

                    if (info != null && info.evidenceImage != null)
                        icons[collectedCount] = info.evidenceImage;
                }

                collectedCount++;
            }

            int requiredCount =
                Mathf.Max(1, story.requiredEvidenceCount);

            unlocked = collectedCount >= requiredCount;

            return icons;
        }

        Debug.LogWarning(
            story.storyId + ": storyType khong hop le."
        );

        return icons;
    }


    private CollectedEvidenceInfo FindCollectedEvidence(
        string evidenceId)
    {
        List<CollectedEvidenceInfo> collected =
            GameSessionData.GetCollectedEvidence();

        foreach (CollectedEvidenceInfo info in collected)
        {
            if (info != null &&
                info.evidenceId == evidenceId)
            {
                return info;
            }
        }

        return null;
    }

    private bool IsStoryUnlocked(
        StoryDetailController.StoryData story)
    {
        bool unlocked;

        BuildStoryIcons(story, out unlocked);

        return unlocked;
    }

    private void OpenStory(
        StoryDetailController.StoryData story)
    {
        if (story == null || storyDetailController == null)
            return;

        if (!IsStoryUnlocked(story))
        {
            Debug.LogWarning(
                "Story chua du dieu kien mo khoa: " +
                story.storyId
            );
            return;
        }

        storyDetailController.OpenStory(story, null);
    }

    private void TryOpenPendingSpecialStory()
    {
        string pendingEvidenceId =
            GameSessionData.PendingSpecialEvidenceId;

        if (string.IsNullOrWhiteSpace(pendingEvidenceId))
            return;

        foreach (
            StoryDetailController.StoryData story
            in selectedBranch.stories
        )
        {
            if (story == null ||
                story.storyType != "special")
            {
                continue;
            }

            if (story.requiredEvidenceId != pendingEvidenceId)
                continue;

            GameSessionData.ClearPendingSpecialEvidenceId();

            if (IsStoryUnlocked(story))
            {
                OpenStory(story);

                Debug.Log(
                    "Tu dong mo Story dac biet: " +
                    story.storyId
                );
            }

            return;
        }

        Debug.LogWarning(
            "Khong tim thay Story dac biet cho Evidence: " +
            pendingEvidenceId
        );

        GameSessionData.ClearPendingSpecialEvidenceId();
    }

    public void ShowStoryList()
    {
        if (sceneStory != null)
            sceneStory.SetActive(true);

        if (storyDetailPanel != null)
            storyDetailPanel.SetActive(false);
    }

    public void OnBack()
    {
        SceneManager.LoadScene("Investigation");
    }
}