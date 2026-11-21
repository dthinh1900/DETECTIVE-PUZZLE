
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ListCharacter : MonoBehaviour
{
    [Header("JSON nhân vật theo chương")]
    [SerializeField] private TextAsset characterJsonChapter1;
    [SerializeField] private TextAsset characterJsonChapter2;

    [Header("Danh sách nút nhân vật")]
    [SerializeField] private Transform characterContent;
    [SerializeField] private Button characterButtonPrefab;

    [Header("Ảnh đại diện nhân vật")]
    [SerializeField]
    private List<CharacterPortrait> characterPortraits =
        new List<CharacterPortrait>();

    [Serializable]
    private class CharacterPortrait
    {
        public int chapterId;
        public string characterId;
        public Sprite portrait;
    }

    public event Action<CaseCharacterInfo> OnCharacterSelected;

    private CaseCharacterListData characterListData;
    private int currentChapterId;

    // Khởi tạo theo chương hiện tại
    public void Initialize(int chapterId)
    {
        currentChapterId = chapterId;

        LoadCharacterData(chapterId);
        GenerateCharacterButtons();
        SelectFirstCharacter();
    }

    // Chọn JSON theo chương
    private void LoadCharacterData(int chapterId)
    {
        TextAsset selectedJson = null;

        if (chapterId == 1)
            selectedJson = characterJsonChapter1;
        else if (chapterId == 2)
            selectedJson = characterJsonChapter2;
        else
        {
            Debug.LogError(
                "[LIST CHARACTER] Chương không hợp lệ: " + chapterId
            );
            return;
        }

        if (selectedJson == null)
        {
            Debug.LogError(
                "[LIST CHARACTER] Chưa gán JSON cho chương " + chapterId
            );
            return;
        }

        characterListData =
            JsonUtility.FromJson<CaseCharacterListData>(
                selectedJson.text
            );

        if (characterListData == null ||
            characterListData.characters == null)
        {
            Debug.LogError("[LIST CHARACTER] JSON không hợp lệ!");
            return;
        }

        // isDead trong CharactersC1.json là trạng thái gốc.
        // Trạng thái của lượt chơi hiện tại được lấy từ GameSessionData.
        foreach (CaseCharacterInfo character in characterListData.characters)
        {
            if (character != null &&
                GameSessionData.IsCharacterDead(chapterId, character.characterId))
            {
                character.isDead = true;
            }
        }

        Debug.Log(
            "[LIST CHARACTER] Đã tải " +
            characterListData.characters.Length +
            " nhân vật của chương " +
            chapterId
        );
    }

    // Tạo nút nhân vật
    private void GenerateCharacterButtons()
    {
        if (characterContent == null ||
            characterButtonPrefab == null)
        {
            Debug.LogError(
                "[LIST CHARACTER] Chưa gán Content hoặc Button Prefab!"
            );
            return;
        }

        foreach (Transform child in characterContent)
        {
            Destroy(child.gameObject);
        }

        if (characterListData == null ||
            characterListData.characters == null)
            return;

        foreach (CaseCharacterInfo character
                 in characterListData.characters)
        {
            if (character == null ||
                string.IsNullOrWhiteSpace(character.characterId))
                continue;

            Button button = Instantiate(
                characterButtonPrefab,
                characterContent
            );

            button.name = "BTN_" + character.characterId;

            // Gán tên nhân vật
            TMP_Text nameText =
                button.GetComponentInChildren<TMP_Text>(true);

            if (nameText != null)
                nameText.text = character.characterName;

            // Gán ảnh đại diện
            SetCharacterPortrait(button, character);

            // Nhân vật đã mất: làm tối màu nhưng vẫn cho phép chọn
            if (character.isDead)
                SetDeadButtonColor(button);

            // Không vô hiệu hóa nút
            button.interactable = true;

            CaseCharacterInfo selectedCharacter = character;

            button.onClick.RemoveAllListeners();

            button.onClick.AddListener(() =>
            {
                SelectCharacter(selectedCharacter);
            });
        }
    }

    // Tìm và gán ảnh đại diện cho nút
    private void SetCharacterPortrait(
        Button button,
        CaseCharacterInfo character)
    {
        if (button == null || character == null)
            return;

        // Tìm GameObject tên CharacterImage bên trong prefab
        Transform imageTransform =
            FindChildRecursive(button.transform, "CharacterImage");

        if (imageTransform == null)
        {
            Debug.LogWarning(
                "[LIST CHARACTER] Không tìm thấy CharacterImage trong prefab: "
                + button.name
            );
            return;
        }

        Image portraitImage =
            imageTransform.GetComponent<Image>();

        if (portraitImage == null)
        {
            Debug.LogWarning(
                "[LIST CHARACTER] CharacterImage chưa có Image component!"
            );
            return;
        }

        Sprite portrait = GetCharacterPortrait(
            currentChapterId,
            character.characterId
        );

        if (portrait == null)
        {
            Debug.LogWarning(
                "[LIST CHARACTER] Chưa gán ảnh cho nhân vật: "
                + character.characterId
                + " - Chương "
                + currentChapterId
            );
            return;
        }

        portraitImage.sprite = portrait;
        portraitImage.preserveAspect = true;

        // Nhân vật đã mất: làm tối ảnh đại diện
        if (character.isDead)
        {
            portraitImage.color =
                new Color(0.35f, 0.35f, 0.35f, 1f);
        }
        else
        {
            portraitImage.color = Color.white;
        }
    }

    // Lấy ảnh theo chương và mã nhân vật
    private Sprite GetCharacterPortrait(
        int chapterId,
        string characterId)
    {
        foreach (CharacterPortrait item in characterPortraits)
        {
            if (item != null &&
                item.chapterId == chapterId &&
                item.characterId == characterId)
            {
                return item.portrait;
            }
        }

        return null;
    }

    // Tìm GameObject theo tên, kể cả khi nằm trong nhiều cấp con
    private Transform FindChildRecursive(
        Transform parent,
        string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;

            Transform result =
                FindChildRecursive(child, childName);

            if (result != null)
                return result;
        }

        return null;
    }

    // Làm tối màu nút nhân vật đã mất
    private void SetDeadButtonColor(Button button)
    {
        if (button == null)
            return;

        ColorBlock colors = button.colors;

        Color darkColor =
            new Color(0.35f, 0.35f, 0.35f, 1f);

        colors.normalColor = darkColor;
        colors.highlightedColor = darkColor;
        colors.pressedColor = darkColor;
        colors.selectedColor = darkColor;

        button.colors = colors;
    }

    // Tự chọn nhân vật đầu tiên
    private void SelectFirstCharacter()
    {
        if (characterListData == null ||
            characterListData.characters == null ||
            characterListData.characters.Length == 0)
        {
            Debug.LogWarning(
                "[LIST CHARACTER] Danh sách nhân vật đang trống!"
            );
            return;
        }

        CaseCharacterInfo firstCharacter =
            characterListData.characters[0];

        if (firstCharacter != null)
            SelectCharacter(firstCharacter);
    }

    // Chọn nhân vật
    private void SelectCharacter(CaseCharacterInfo character)
    {
        if (character == null)
            return;

        Debug.Log(
            "[LIST CHARACTER] Đã chọn: " +
            character.characterName
        );

        OnCharacterSelected?.Invoke(character);
    }
}
