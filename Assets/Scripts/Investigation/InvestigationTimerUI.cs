
using TMPro;
using UnityEngine;

public class InvestigationTimerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;

    private GameManager subscribedManager;

    private void OnEnable()
    {
        SubscribeToTimer();
    }

    private void Start()
    {
        SubscribeToTimer();
    }

    private void OnDisable()
    {
        if (subscribedManager != null)
        {
            subscribedManager.OnTimerUpdated -= UpdateTimerText;
        }

        subscribedManager = null;
    }

    private void SubscribeToTimer()
    {
        if (GameManager.Instance == null)
            return;

        if (subscribedManager == GameManager.Instance)
        {
            UpdateTimerText(GameManager.Instance.RemainingTime);
            return;
        }

        if (subscribedManager != null)
        {
            subscribedManager.OnTimerUpdated -= UpdateTimerText;
        }

        subscribedManager = GameManager.Instance;
        subscribedManager.OnTimerUpdated += UpdateTimerText;

        UpdateTimerText(subscribedManager.RemainingTime);
    }

    private void UpdateTimerText(float time)
    {
        if (timerText == null)
            return;

        int totalSeconds = Mathf.CeilToInt(time);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text = $"{minutes:00}:{seconds:00}";
    }
}
