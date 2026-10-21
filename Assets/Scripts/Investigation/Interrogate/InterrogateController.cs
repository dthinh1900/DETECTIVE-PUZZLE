
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InterrogateController : MonoBehaviour
{
    // =========================
    // DATA: QUESTION JSON
    // =========================

    [System.Serializable]
    public class QuestionInfo
    {
        public string questionText;

        [TextArea(2, 5)]
        public string answerText;

        [NonSerialized]
        public bool asked;
    }

    [System.Serializable]
    public class CharacterQuestions
    {
        public string characterId;
        public QuestionInfo[] questions;
    }

    [System.Serializable]
    public class InterrogateData
    {
        public CharacterQuestions[] characters;
    }

    // =========================
    // CHARACTER UI
    // =========================

    [Header("Character UI")]
    [SerializeField] private Image characterIMG;
    [SerializeField] private TMP_Text characterText;

    // =========================
    // QUESTION UI
    // =========================

    [Header("Question UI")]
    [SerializeField] private GameObject panelQues;
    [SerializeField] private Transform questionContent;
    [SerializeField] private Button questionButtonPrefab;
    [SerializeField] private TMP_Text questionCountText;
    [SerializeField] private TMP_Text answerText;

    // =========================
    // JSON
    // =========================

    [Header("Question JSON")]
    [SerializeField] private TextAsset questionJson;

    private InterrogateData questionData;

    // =========================
    // SETTINGS
    // =========================

    [Header("Question Settings")]
    [SerializeField] private int questionLimit = 3;

    private int selectedCharacterIndex = -1;
    private int[] askedCounts;

    // =========================
    // START
    // =========================

    private void Start()
    {
        if (panelQues != null)
        {
            panelQues.SetActive(false);
        }

        LoadQuestionData();
    }

    private void LoadQuestionData()
    {
        if (questionJson == null)
        {
            Debug.LogError(
                "Chưa gán file JSON vào Question Json!"
            );
            return;
        }

        questionData =
            JsonUtility.FromJson<InterrogateData>(
                questionJson.text
            );

        if (questionData == null ||
            questionData.characters == null)
        {
            Debug.LogError(
                "JSON không hợp lệ hoặc thiếu danh sách characters!"
            );
            return;
        }

        askedCounts = new int[
            questionData.characters.Length
        ];

        Debug.Log(
            "Đã tải dữ liệu " +
            questionData.characters.Length +
            " nhân vật."
        );
    }

    // =========================
    // SELECT CHARACTER
    // =========================

    public void SelectCharacter(int index)
    {
        if (questionData == null ||
            questionData.characters == null ||
            index < 0 ||
            index >= questionData.characters.Length)
        {
            Debug.LogWarning(
                "Index nhân vật không hợp lệ: " + index
            );
            return;
        }

        selectedCharacterIndex = index;

        if (characterText != null)
        {
            characterText.text =
                questionData.characters[index].characterId;
        }

        if (answerText != null)
        {
            answerText.text = "";
        }

        if (panelQues != null && panelQues.activeSelf)
        {
            GenerateQuestions();
        }

        UpdateQuestionCount();
    }

    // =========================
    // OPEN / CLOSE QUESTION PANEL
    // =========================

    public void OpenQues()
    {
        if (selectedCharacterIndex < 0)
        {
            Debug.LogWarning(
                "Hãy chọn nhân vật trước khi mở câu hỏi!"
            );
            return;
        }

        if (panelQues == null)
        {
            Debug.LogError(
                "Chưa gán Panel_Ques trong Inspector!"
            );
            return;
        }

        panelQues.SetActive(true);

        GenerateQuestions();
        UpdateQuestionCount();
    }

    public void CloseQues()
    {
        if (panelQues != null)
        {
            panelQues.SetActive(false);
        }
    }

    // =========================
    // GENERATE QUESTION BUTTONS
    // =========================


    private void GenerateQuestions()
    {
        if (questionContent == null ||
            questionButtonPrefab == null)
        {
            Debug.LogError(
                "Chưa gán Question Content hoặc Question Button Prefab!"
            );
            return;
        }

        foreach (Transform child in questionContent)
        {
            Destroy(child.gameObject);
        }

        if (questionData == null ||
            selectedCharacterIndex < 0)
        {
            return;
        }

        QuestionInfo[] questions =
            questionData.characters[selectedCharacterIndex].questions;

        if (questions == null)
        {
            return;
        }

        for (int i = 0; i < questions.Length; i++)
        {
            int questionIndex = i;
            int characterIndex = selectedCharacterIndex;

            QuestionInfo question = questions[i];

            Button button = Instantiate(
                questionButtonPrefab,
                questionContent
            );

            TMP_Text buttonText =
                button.GetComponentInChildren<TMP_Text>();

            if (buttonText != null)
            {
                buttonText.text = question.questionText;
            }

            // Câu đã hỏi luôn bấm được để xem lại đáp án.
            // Câu chưa hỏi bị khóa khi đã đủ lượt.
            bool canAsk =
                question.asked ||
                askedCounts[characterIndex] < questionLimit;

            button.interactable = canAsk;

            // Chỉ đổi màu câu chưa hỏi bị khóa.
            if (!canAsk)
            {
                ColorBlock colors = button.colors;
                colors.normalColor = Color.gray;
                colors.highlightedColor = Color.gray;
                colors.selectedColor = Color.gray;
                colors.disabledColor = Color.gray;

                button.colors = colors;
            }

            button.onClick.AddListener(() =>
            {
                AskQuestion(characterIndex, questionIndex);
            });
        }
    }


    // =========================
    // ASK QUESTION
    // =========================




    private void AskQuestion(
        int characterIndex,
        int questionIndex)
    {
        if (questionData == null ||
            characterIndex < 0 ||
            characterIndex >= questionData.characters.Length)
        {
            return;
        }

        QuestionInfo[] questions =
            questionData.characters[characterIndex].questions;

        if (questions == null ||
            questionIndex < 0 ||
            questionIndex >= questions.Length)
        {
            return;
        }

        QuestionInfo question = questions[questionIndex];

        // Đã hỏi rồi: chỉ hiện lại đáp án, không trừ lượt.
        if (question.asked)
        {
            if (answerText != null)
            {
                answerText.text = question.answerText;
            }

            return;
        }

        // Chặn câu chưa hỏi nếu đã hết lượt.
        if (askedCounts[characterIndex] >= questionLimit)
        {
            return;
        }

        // Đánh dấu đã hỏi và tăng lượt đúng một lần.
        question.asked = true;
        askedCounts[characterIndex]++;

        if (answerText != null)
        {
            answerText.text = question.answerText;
        }

        GenerateQuestions();
        UpdateQuestionCount();
    }



    // =========================
    // UPDATE QUESTION COUNT
    // =========================

    private void UpdateQuestionCount()
    {
        if (questionCountText == null ||
            askedCounts == null ||
            selectedCharacterIndex < 0 ||
            selectedCharacterIndex >= askedCounts.Length)
        {
            return;
        }

        questionCountText.text =
            "Đã hỏi: " +
            askedCounts[selectedCharacterIndex] +
            "/" + questionLimit;
    }

    // =========================
    // BACK TO INVESTIGATION
    // =========================

    public void OnBack()
    {
        SceneManager.LoadScene("Investigation");
    }

    // =========================
    // CHARACTER BUTTONS
    // =========================

    public void Bao()
    {
        SelectCharacter(0);
    }

    public void Chi()
    {
        SelectCharacter(1);
    }

    public void Dung()
    {
        SelectCharacter(2);
    }

    public void Giang()
    {
        SelectCharacter(3);
    }

    public void Hai()
    {
        SelectCharacter(4);
    }
}
