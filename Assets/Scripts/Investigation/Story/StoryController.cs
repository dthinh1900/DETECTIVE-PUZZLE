
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StoryController : MonoBehaviour
{
    [Serializable]
    public class StoryInfo
    {
        public string storyId;
        public string title;

        [TextArea(4, 12)]
        public string content;

        [Tooltip("Bat len neu Story da mo khoa.")]
        public bool unlocked;

        [Header("Ba vat pham can tim")]
        public Sprite itemIcon1;
        public Sprite itemIcon2;
        public Sprite itemIcon3;
    }

    [Header("Panels")]
    [SerializeField] private GameObject sceneStory;
    [SerializeField] private GameObject storyDetailPanel;

    [Header("Danh sach Story")]
    [SerializeField] private Transform storyContent;
    [SerializeField] private Button storyItemPrefab;

    [Header("Chapter 1 JSON")]
    [SerializeField] private TextAsset chapter1StoryJson;

    [Header("Mau nen")]
    [SerializeField]
    private Color lockedColor =
        new Color(0.27f, 0.27f, 0.27f, 1f);

    [SerializeField]
    private Color unlockedColor =
        new Color(1f, 0.9f, 0.65f, 1f);

    [Header("Du lieu Story")]
    [SerializeField] private StoryInfo[] stories;

    [Header("Story Detail")]
    [SerializeField]
    private StoryDetailController storyDetailController;

    private StoryDetailController.ChapterStoryData chapterData;

    private void Start()
    {
        // 1. Tai JSON truoc.
        if (!LoadChapterData())
            return;

        // 2. Truyen du lieu cho StoryDetailController.
        if (storyDetailController == null)
        {
            Debug.LogError(
                "StoryController: Chua gan StoryDetailController."
            );
            return;
        }

        storyDetailController.SetChapterData(chapterData);

        // 3. Hien danh sach va sinh cac nut Story.
        ShowStoryList();
        GenerateStoryItems();
    }

    private bool LoadChapterData()
    {
        if (chapter1StoryJson == null)
        {
            Debug.LogError(
                "StoryController: Chua gan Chapter1Story.json!"
            );
            return false;
        }

        chapterData =
            JsonUtility.FromJson<
                StoryDetailController.ChapterStoryData
            >(chapter1StoryJson.text);

        if (chapterData == null ||
            chapterData.stories == null)
        {
            Debug.LogError(
                "StoryController: JSON khong hop le " +
                "hoac thieu mang stories!"
            );

            chapterData = null;
            return false;
        }

        Debug.Log(
            "StoryController: Da tai " +
            chapterData.stories.Length +
            " Story cua Chuong 1."
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

        // Xoa cac nut cu.
        foreach (Transform child in storyContent)
        {
            Destroy(child.gameObject);
        }

        if (stories == null)
            return;

        for (int i = 0; i < stories.Length; i++)
        {
            int index = i;
            StoryInfo story = stories[index];

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

            storyButton.Setup(
                story.title,
                story.itemIcon1,
                story.itemIcon2,
                story.itemIcon3,
                story.unlocked,
                lockedColor,
                unlockedColor,
                () => OpenStory(index)
            );
        }
    }

    private void OpenStory(int index)
    {
        if (stories == null ||
            index < 0 ||
            index >= stories.Length)
        {
            return;
        }

        StoryInfo story = stories[index];

        if (story == null || !story.unlocked)
            return;

        if (string.IsNullOrWhiteSpace(story.storyId))
        {
            Debug.LogError(
                "StoryController: Story ID khong duoc de trong."
            );
            return;
        }

        storyDetailController.OpenStory(
            story.storyId,
            null
        );
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
