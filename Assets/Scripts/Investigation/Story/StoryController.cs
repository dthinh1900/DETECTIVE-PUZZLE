
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

    [Header("Hai panel")]
    [SerializeField] private GameObject sceneStory;
    [SerializeField] private GameObject storyDetailPanel;

    [Header("Danh sach Story")]
    [SerializeField] private Transform storyContent;
    [SerializeField] private Button storyItemPrefab;

    [Header("Mau nen")]
    [SerializeField]
    private Color lockedColor =
        new Color(0.27f, 0.27f, 0.27f, 1f);

    [SerializeField]
    private Color unlockedColor =
        new Color(1f, 0.9f, 0.65f, 1f);

    [Header("Du lieu kiem thu")]
    [SerializeField] private StoryInfo[] stories;

    private void Start()
    {
        ShowStoryList();
        GenerateStoryItems();
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

        // Xoa cac Button cu trong Content.
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

            Button item = Instantiate(
                storyItemPrefab,
                storyContent
            );

            // Lay cac doi tuong con theo ten trong Prefab.
            TMP_Text titleText =
                item.transform.Find("StoryTitle")
                ?.GetComponent<TMP_Text>();

            Transform iconsRoot =
                item.transform.Find("ItemIcons");

            Image backgroundImage =
                item.GetComponent<Image>();

            if (titleText != null)
                titleText.text = story.title;

            // Dat mau nen theo trang thai khoa.
            if (backgroundImage != null)
            {
                backgroundImage.color = story.unlocked
                    ? unlockedColor
                    : lockedColor;
            }

            // Gan ba icon vat pham.
            if (iconsRoot != null)
            {
                Image icon1 = iconsRoot
                    .Find("Icon_Item_1")
                    ?.GetComponent<Image>();

                Image icon2 = iconsRoot
                    .Find("Icon_Item_2")
                    ?.GetComponent<Image>();

                Image icon3 = iconsRoot
                    .Find("Icon_Item_3")
                    ?.GetComponent<Image>();

                SetIcon(icon1, story.itemIcon1);
                SetIcon(icon2, story.itemIcon2);
                SetIcon(icon3, story.itemIcon3);

                // Da mo khoa thi an ca ba icon.
                iconsRoot.gameObject.SetActive(
                    !story.unlocked
                );
            }

            // Story bi khoa thi khong bam duoc.
            item.interactable = story.unlocked;

            item.onClick.AddListener(
                () => OpenStory(index)
            );
        }
    }

    private void SetIcon(Image icon, Sprite sprite)
    {
        if (icon == null)
            return;

        icon.sprite = sprite;
        icon.enabled = sprite != null;
    }

    private void OpenStory(int index)
    {
        if (stories == null ||
            index < 0 ||
            index >= stories.Length)
        {
            return;
        }

        if (!stories[index].unlocked)
            return;

        // Tam thoi chi chuyen panel.
        // Noi dung StoryDetailPanel se lam sau.
        if (sceneStory != null)
            sceneStory.SetActive(false);

        if (storyDetailPanel != null)
            storyDetailPanel.SetActive(true);
    }

    public void ShowStoryList()
    {
        if (storyDetailPanel != null)
            storyDetailPanel.SetActive(false);

        if (sceneStory != null)
            sceneStory.SetActive(true);
    }

    public void OnBack()
    {
        SceneManager.LoadScene("Investigation");
    }
}
