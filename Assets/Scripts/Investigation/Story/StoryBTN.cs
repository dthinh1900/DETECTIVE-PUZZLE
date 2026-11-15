
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoryBTN : MonoBehaviour
{
    [Header("Button")]
    [SerializeField] private Button button;

    [Header("Story Title")]
    [SerializeField] private TMP_Text titleText;

    [Header("Background")]
    [SerializeField] private Image backgroundImage;

    [Header("Item Icons")]
    [SerializeField] private Transform iconsRoot;
    [SerializeField] private Image icon1;
    [SerializeField] private Image icon2;
    [SerializeField] private Image icon3;

    public void Setup(
        string title,
        Sprite itemSprite1,
        Sprite itemSprite2,
        Sprite itemSprite3,
        bool unlocked,
        Color lockedColor,
        Color unlockedColor,
        Action onClick)
    {
        CacheReferences();

        if (titleText != null)
            titleText.text = title;

        // Nền luôn màu trắng.
        if (backgroundImage != null)
            backgroundImage.color = Color.white;

        if (unlocked)
        {
            // Đã đủ vật chứng: ẩn toàn bộ icon.
            if (icon1 != null)
                icon1.gameObject.SetActive(false);

            if (icon2 != null)
                icon2.gameObject.SetActive(false);

            if (icon3 != null)
                icon3.gameObject.SetActive(false);
        }
        else
        {
            // Story đặc biệt: chỉ hiển thị ô giữa.
            bool isSpecialStory =
                itemSprite1 == null &&
                itemSprite2 != null &&
                itemSprite3 == null;

            if (icon1 != null)
                icon1.gameObject.SetActive(!isSpecialStory);

            if (icon2 != null)
                icon2.gameObject.SetActive(true);

            if (icon3 != null)
                icon3.gameObject.SetActive(!isSpecialStory);

            // Chỉ thay Sprite, giữ nguyên kích thước và vị trí.
            SetIcon(icon1, itemSprite1);
            SetIcon(icon2, itemSprite2);
            SetIcon(icon3, itemSprite3);
        }

        if (button != null)
        {
            button.interactable = unlocked;

            button.onClick.RemoveAllListeners();

            button.onClick.AddListener(() =>
            {
                onClick?.Invoke();
            });
        }

        if (iconsRoot != null)
            iconsRoot.gameObject.SetActive(true);
    }

    private void CacheReferences()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (titleText == null)
        {
            titleText = transform
                .Find("StoryTitle")
                ?.GetComponent<TMP_Text>();
        }

        if (iconsRoot == null)
            iconsRoot = transform.Find("ItemIcons");

        if (icon1 == null && iconsRoot != null)
        {
            icon1 = iconsRoot
                .Find("Icon_Item_1")
                ?.GetComponent<Image>();
        }

        if (icon2 == null && iconsRoot != null)
        {
            icon2 = iconsRoot
                .Find("Icon_Item_2")
                ?.GetComponent<Image>();
        }

        if (icon3 == null && iconsRoot != null)
        {
            icon3 = iconsRoot
                .Find("Icon_Item_3")
                ?.GetComponent<Image>();
        }

        if (backgroundImage == null)
        {
            Transform bg = transform.Find("BG");

            if (bg != null)
                backgroundImage = bg.GetComponent<Image>();

            if (backgroundImage == null)
                backgroundImage = GetComponent<Image>();
        }
    }

    private void SetIcon(Image icon, Sprite sprite)
    {
        if (icon == null)
            return;

        // Có Sprite mới thì thay ảnh.
        // Không có Sprite thì giữ ảnh dấu hỏi đã gán trong Prefab.
        if (sprite != null)
            icon.sprite = sprite;

        icon.enabled = true;
        icon.preserveAspect = true;
    }
}
