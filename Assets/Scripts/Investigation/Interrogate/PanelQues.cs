
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PanelQues : MonoBehaviour
{
    [Serializable]
    public class QuestionInfo
    {
        public string questionText;

        [TextArea(2, 5)]
        public string answerText;

        [NonSerialized]
        public bool asked;
    }

    [Serializable]
    public class CharacterQuestions
    {
        public string characterId;
        public QuestionInfo[] questions;
    }

    [Serializable]
    public class InterrogateData
    {
        public CharacterQuestions[] characters;
    }

    [Header("Danh sách câu hỏi")]
    [SerializeField] private Transform questionContent;
    [SerializeField] private Button questionButtonPrefab;

    [Header("Thông tin câu hỏi")]
    [SerializeField] private TMP_Text questionCountText;
    [SerializeField] private TMP_Text answerText;

    [Header("Dữ liệu câu hỏi")]
    [SerializeField] private TextAsset questionJson;

    [Header("Cài đặt")]
    [SerializeField] private int questionLimit = 3;

    private InterrogateData questionData;

    private int selectedCharacterIndex = -1;
    private int[] askedCounts;

    private bool selectedCharacterIsDead;

    public bool HasSelectedCharacter =>
        selectedCharacterIndex >= 0;

    // Khởi tạo dữ liệu câu hỏi
    public void Initialize()
    {
        LoadQuestionData();
    }

    // Đọc JSON câu hỏi
    private void LoadQuestionData()
    {
        if (questionJson == null)
        {
            Debug.LogError("[PANEL QUES] Chưa gán Question JSON!");
            return;
        }

        questionData =
            JsonUtility.FromJson<InterrogateData>(
                questionJson.text
            );

        if (questionData == null ||
            questionData.characters == null)
        {
            Debug.LogError("[PANEL QUES] JSON câu hỏi không hợp lệ!");
            return;
        }

        askedCounts = new int[questionData.characters.Length];

        // Khôi phục câu hỏi đã hỏi và ghép các câu hỏi mới do Story bổ sung.
        // Việc ghép chỉ diễn ra trong bộ nhớ runtime; Questions.json không bị sửa.
        int chapterId = GameManager.Instance != null ? GameManager.Instance.SelectedChapter : 0;
        for (int characterIndex = 0; characterIndex < questionData.characters.Length; characterIndex++)
        {
            CharacterQuestions character = questionData.characters[characterIndex];
            if (character == null) continue;

            if (character.questions == null)
                character.questions = new QuestionInfo[0];

            System.Collections.Generic.List<StatementInfo> updates =
                GameSessionData.GetStoryStatementOverrides(chapterId, character.characterId);

            foreach (StatementInfo update in updates)
            {
                bool exists = false;
                foreach (QuestionInfo existing in character.questions)
                {
                    if (existing != null && string.Equals(existing.questionText, update.question, StringComparison.OrdinalIgnoreCase))
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    System.Array.Resize(ref character.questions, character.questions.Length + 1);
                    character.questions[character.questions.Length - 1] = new QuestionInfo
                    {
                        questionText = update.question,
                        answerText = update.answer
                    };
                }
            }

            foreach (QuestionInfo question in character.questions)
            {
                if (question == null) continue;
                question.asked = GameSessionData.IsQuestionAsked(chapterId, character.characterId, question.questionText);
                if (question.asked) askedCounts[characterIndex]++;
            }
        }

        Debug.Log(
            "[PANEL QUES] Đã tải dữ liệu " +
            questionData.characters.Length +
            " nhân vật."
        );
    }

    // Cập nhật nội dung AnsewText
    public void SetAnswerText(string message)
    {
        if (answerText != null)
        {
            answerText.text = message;
        }
        else
        {
            Debug.LogWarning(
                "[PANEL QUES] Chưa gán Answer Text trong Inspector!"
            );
        }
    }

    // Nhận nhân vật được chọn từ Controller
    public void SelectCharacter(string characterId, bool isDead)
    {
        if (questionData == null ||
            questionData.characters == null)
        {
            Debug.LogError("[PANEL QUES] Chưa tải dữ liệu câu hỏi!");
            selectedCharacterIndex = -1;
            return;
        }

        selectedCharacterIndex = -1;
        selectedCharacterIsDead = isDead;

        for (int i = 0; i < questionData.characters.Length; i++)
        {
            if (questionData.characters[i] != null &&
                questionData.characters[i].characterId == characterId)
            {
                selectedCharacterIndex = i;
                break;
            }
        }

        if (selectedCharacterIndex == -1)
        {
            ClearQuestionList();

            if (questionCountText != null)
                questionCountText.text = "";

            Debug.LogWarning(
                "[PANEL QUES] Không tìm thấy câu hỏi cho: " +
                characterId
            );
            return;
        }

        GenerateQuestions();
        UpdateQuestionCount();

        Debug.Log(
            "[PANEL QUES] Đã chọn nhân vật: " + characterId
        );
    }

    // Xóa danh sách câu hỏi trên giao diện
    private void ClearQuestionList()
    {
        if (questionContent == null)
            return;

        foreach (Transform child in questionContent)
        {
            Destroy(child.gameObject);
        }
    }

    // Tạo danh sách câu hỏi
    private void GenerateQuestions()
    {
        if (questionContent == null ||
            questionButtonPrefab == null)
        {
            Debug.LogError(
                "[PANEL QUES] Chưa gán QuestionContent hoặc Prefab!"
            );
            return;
        }

        ClearQuestionList();

        if (questionData == null ||
            selectedCharacterIndex < 0)
            return;

        QuestionInfo[] questions =
            questionData.characters[selectedCharacterIndex].questions;

        if (questions == null)
            return;

        for (int i = 0; i < questions.Length; i++)
        {
            int questionIndex = i;
            int characterIndex = selectedCharacterIndex;

            QuestionInfo question = questions[i];

            if (question == null)
                continue;

            Button button = Instantiate(
                questionButtonPrefab,
                questionContent
            );

            TMP_Text buttonText =
                button.GetComponentInChildren<TMP_Text>(true);

            if (buttonText != null)
                buttonText.text = question.questionText;

            // Nhân vật đã chết:
            // Chỉ câu hỏi từng được hỏi mới có thể bấm.
            bool canAsk =
                question.asked ||
                (!selectedCharacterIsDead &&
                 askedCounts[characterIndex] < questionLimit);

            button.interactable = canAsk;

            if (!canAsk)
            {
                ColorBlock colors = button.colors;

                colors.normalColor = Color.gray;
                colors.highlightedColor = Color.gray;
                colors.selectedColor = Color.gray;
                colors.pressedColor = Color.gray;
                colors.disabledColor = Color.gray;

                button.colors = colors;
            }

            button.onClick.RemoveAllListeners();

            button.onClick.AddListener(() =>
            {
                AskQuestion(characterIndex, questionIndex);
            });
        }
    }

    // Xử lý khi chọn câu hỏi
    private void AskQuestion(
        int characterIndex,
        int questionIndex)
    {
        if (questionData == null ||
            characterIndex < 0 ||
            characterIndex >= questionData.characters.Length)
            return;

        QuestionInfo[] questions =
            questionData.characters[characterIndex].questions;

        if (questions == null ||
            questionIndex < 0 ||
            questionIndex >= questions.Length)
            return;

        QuestionInfo question = questions[questionIndex];

        int chapterId = GameManager.Instance != null ? GameManager.Instance.SelectedChapter : 0;

        // Nếu đã hỏi: hiển thị đáp án hiệu lực (ưu tiên bản cập nhật từ Story).
        if (question.asked || GameSessionData.IsQuestionAsked(chapterId, questionData.characters[characterIndex].characterId, question.questionText))
        {
            question.asked = true;
            SetAnswerText(GameSessionData.GetEffectiveStatementAnswer(
                chapterId,
                questionData.characters[characterIndex].characterId,
                question.questionText,
                question.answerText
            ));
            return;
        }

        // Nhân vật đã mất: không được hỏi câu mới
        if (selectedCharacterIsDead)
            return;

        // Đã hết lượt hỏi mới
        if (askedCounts[characterIndex] >= questionLimit)
            return;

        if (GameManager.Instance == null)
        {
            Debug.LogError("[PANEL QUES] Không tìm thấy GameManager!");
            return;
        }

        // Đánh dấu đã hỏi và lưu trạng thái vào SaveData.
        question.asked = true;
        askedCounts[characterIndex]++;

        string characterId = questionData.characters[characterIndex].characterId;
        GameSessionData.MarkQuestionAsked(chapterId, characterId, question.questionText);

        string effectiveAnswer = GameSessionData.GetEffectiveStatementAnswer(
            chapterId, characterId, question.questionText, question.answerText
        );

        // Lưu đúng câu trả lời người chơi thực sự nhìn thấy.
        GameSessionData.AddStatement(
            chapterId,
            characterId,
            question.questionText,
            effectiveAnswer
        );

        Debug.Log(
            "[THẨM VẤN] Đã lưu: " +
            characterId +
            " | " + question.questionText
        );

        SetAnswerText(effectiveAnswer);

        GenerateQuestions();
        UpdateQuestionCount();
    }

    // Cập nhật số câu đã hỏi
    private void UpdateQuestionCount()
    {
        if (questionCountText == null ||
            askedCounts == null ||
            selectedCharacterIndex < 0 ||
            selectedCharacterIndex >= askedCounts.Length)
            return;

        questionCountText.text =
            "Đã hỏi: " +
            askedCounts[selectedCharacterIndex] +
            "/" +
            questionLimit;
    }
}
