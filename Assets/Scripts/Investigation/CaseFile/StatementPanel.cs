
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatementPanel : MonoBehaviour
{
    [Header("Dữ liệu nghi phạm")]
    [SerializeField] private TextAsset characterListJson;

    [Header("Danh sách nút nghi phạm")]
    [SerializeField] private Transform characterContent;
    [SerializeField] private GameObject characterButtonPrefab;

    [Header("Thông tin nhân vật")]
    [SerializeField] private TMP_Text characterNameText;

    [Header("Danh sách lời khai")]
    [SerializeField] private Transform statementContent;
    [SerializeField] private GameObject statementCardPrefab;

    [Header("Thông báo")]
    [SerializeField] private TMP_Text noStatementText;

    private List<CaseCharacterInfo> characters =
        new List<CaseCharacterInfo>();

    private string selectedCharacterId;

    private void Start()
    {
        LoadCharacters();
        GenerateCharacterButtons();
    }

    // Đọc danh sách nghi phạm từ JSON
    private void LoadCharacters()
    {
        if (characterListJson == null)
        {
            Debug.LogError("[HỒ SƠ] Chưa gán CaseCharacters.json!");
            return;
        }

        CaseCharacterListData data =
            JsonUtility.FromJson<CaseCharacterListData>(
                characterListJson.text
            );

        if (data == null || data.characters == null)
        {
            Debug.LogError("[HỒ SƠ] Không đọc được danh sách nghi phạm!");
            return;
        }

        characters.Clear();

        foreach (CaseCharacterInfo character in data.characters)
        {
            if (character == null ||
                string.IsNullOrWhiteSpace(character.characterId) ||
                string.IsNullOrWhiteSpace(character.characterName))
            {
                Debug.LogWarning("[HỒ SƠ] Có dữ liệu nghi phạm không hợp lệ!");
                continue;
            }

            characters.Add(character);
        }

        Debug.Log("[HỒ SƠ] Đã tải " + characters.Count + " nghi phạm.");
    }

    // Tự động tạo nút tên nghi phạm
    private void GenerateCharacterButtons()
    {
        if (characterContent == null || characterButtonPrefab == null)
        {
            Debug.LogError("[HỒ SƠ] Chưa gán CharacterContent hoặc ButtonPrefab!");
            return;
        }

        // Xóa các nút cũ nếu có
        foreach (Transform child in characterContent)
        {
            Destroy(child.gameObject);
        }

        foreach (CaseCharacterInfo character in characters)
        {
            GameObject buttonObject = Instantiate(
                characterButtonPrefab,
                characterContent
            );

            Button button = buttonObject.GetComponent<Button>();

            TMP_Text buttonText =
                buttonObject.GetComponentInChildren<TMP_Text>(true);

            if (buttonText != null)
            {
                buttonText.text = character.characterName;
            }
            else
            {
                Debug.LogWarning(
                    "[HỒ SƠ] Nút không có TextMeshPro: " +
                    character.characterName
                );
            }

            if (button == null)
            {
                Debug.LogError("[HỒ SƠ] CharacterButtonPrefab thiếu Button!");
                Destroy(buttonObject);
                continue;
            }

            string id = character.characterId;
            string name = character.characterName;

            button.onClick.RemoveAllListeners();

            button.onClick.AddListener(() =>
            {
                SelectCharacter(id, name);
            });
        }

        // Mặc định chọn nghi phạm đầu tiên
        if (characters.Count > 0)
        {
            SelectCharacter(
                characters[0].characterId,
                characters[0].characterName
            );
        }
        else
        {
            selectedCharacterId = null;

            if (characterNameText != null)
                characterNameText.text = "";

            RefreshStatements();
        }
    }

    // Chọn nghi phạm
    private void SelectCharacter(string characterId, string characterName)
    {
        selectedCharacterId = characterId;

        if (characterNameText != null)
        {
            characterNameText.text = characterName;
        }

        RefreshStatements();

        Debug.Log("[HỒ SƠ] Đang xem lời khai: " + characterName);
    }

    // Cập nhật danh sách lời khai
    public void RefreshStatements()
    {
        if (statementContent == null || statementCardPrefab == null)
        {
            Debug.LogError("[HỒ SƠ] Chưa gán StatementContent hoặc StatementCardPrefab!");
            return;
        }

        foreach (Transform child in statementContent)
        {
            Destroy(child.gameObject);
        }

        if (string.IsNullOrWhiteSpace(selectedCharacterId))
        {
            if (noStatementText != null)
                noStatementText.gameObject.SetActive(true);

            return;
        }

        List<StatementInfo> statements =
            GameSessionData.GetStatementsByCharacter(selectedCharacterId);

        bool hasStatements = statements != null && statements.Count > 0;

        if (noStatementText != null)
        {
            noStatementText.gameObject.SetActive(!hasStatements);
        }

        foreach (StatementInfo statement in statements)
        {
            GameObject card = Instantiate(
                statementCardPrefab,
                statementContent
            );

            TMP_Text[] texts =
                card.GetComponentsInChildren<TMP_Text>(true);

            TMP_Text titleText = null;
            TMP_Text contentText = null;

            foreach (TMP_Text text in texts)
            {
                if (text.name == "Text_StatementTitle")
                {
                    titleText = text;
                }
                else if (text.name == "Text_StatementContent")
                {
                    contentText = text;
                }
            }

            if (titleText != null)
            {
                titleText.text = statement.question;
            }

            if (contentText != null)
            {
                contentText.text = statement.answer;
            }

            if (titleText == null || contentText == null)
            {
                Debug.LogWarning(
                    "[HỒ SƠ] StatementCard thiếu Text_StatementTitle " +
                    "hoặc Text_StatementContent!"
                );
            }
        }

        Debug.Log(
            "[HỒ SƠ] Nhân vật: " + selectedCharacterId +
            " | Số lời khai: " + statements.Count
        );
    }
}
