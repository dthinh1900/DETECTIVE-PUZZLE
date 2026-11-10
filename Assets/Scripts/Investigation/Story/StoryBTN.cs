
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

    // Goi khi StoryController sinh nut moi.
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

        // Cap nhat ten Story.
        if (titleText != null)
        {
            titleText.text = title;
        }

        // Cap nhat ba icon vat pham.
        SetIcon(icon1, itemSprite1);
        SetIcon(icon2, itemSprite2);
        SetIcon(icon3, itemSprite3);

        // Cap nhat mau nen.
        if (backgroundImage != null)
        {
            backgroundImage.color = unlocked
                ? unlockedColor
                : lockedColor;
        }

        // Story da mo khoa moi hien nut bam.
        if (button != null)
        {
            button.interactable = unlocked;

            button.onClick.AddListener(()=>
            {
                Debug.Log("BTN STORY ĐÃ NHẬN CLICK");
                onClick?.Invoke();
            });
        }

        // Giu nguyen co che hien icon hien tai:
        // Story bi khoa hien icon, Story mo khoa an icon.
        if (iconsRoot != null)
        {
            iconsRoot.gameObject.SetActive(!unlocked);
        }
    }

    private void CacheReferences()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (titleText == null)
        {
            titleText = transform
                .Find("StoryTitle")
                ?.GetComponent<TMP_Text>();
        }

        if (iconsRoot == null)
        {
            iconsRoot = transform.Find("ItemIcons");
        }

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
            // Uu tien BG la GameObject con neu co.
            Transform bg = transform.Find("BG");

            if (bg != null)
            {
                backgroundImage = bg.GetComponent<Image>();
            }

            // Neu khong co BG con, lay Image tren Button.
            if (backgroundImage == null)
            {
                backgroundImage = GetComponent<Image>();
            }
        }
    }

    private void SetIcon(Image icon, Sprite sprite)
    {
        if (icon == null)
            return;

        icon.sprite = sprite;
        icon.enabled = sprite != null;
    }
}
