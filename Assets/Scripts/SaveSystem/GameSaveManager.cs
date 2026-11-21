using System;
using System.IO;
using System.Text;
using UnityEngine;

public static class GameSaveManager
{
    private const string SAVE_FILE_NAME = "SaveData.json";
    public static string SavePath => Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);

    public static bool Save(GameSaveData data)
    {
        if (data == null) return false;
        string tempPath = SavePath + ".tmp";
        try
        {
            Normalize(data);
            data.hasSaveData = true;
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(tempPath, json, new UTF8Encoding(false));
            if (File.Exists(SavePath)) File.Delete(SavePath);
            File.Move(tempPath, SavePath);
            Debug.Log("[SAVE] Đã lưu: " + SavePath);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("[SAVE] Lỗi ghi file: " + e);
            return false;
        }
    }

    public static GameSaveData Load()
    {
        if (!File.Exists(SavePath)) return null;
        try
        {
            GameSaveData data = JsonUtility.FromJson<GameSaveData>(File.ReadAllText(SavePath, Encoding.UTF8));
            if (data == null || !data.hasSaveData) return null;
            if (data.saveVersion != 3 && data.saveVersion != 4 && data.saveVersion != 5)
            {
                Debug.LogWarning("[SAVE] Save không tương thích với cấu trúc hiện tại. Hãy bắt đầu New Game.");
                return null;
            }

            // Save Version 3 không có dữ liệu mật mã; Version 3/4 cũng chưa lưu giới hạn thời gian.
            // GameManager sẽ dùng defaultTimerSeconds làm mức ban đầu khi Continue các Save cũ.
            if (data.saveVersion == 3 || data.saveVersion == 4)
                data.saveVersion = 5;

            Normalize(data);
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError("[SAVE] Lỗi đọc file: " + e);
            return null;
        }
    }

    public static bool HasSave()
    {
        GameSaveData data = Load();
        return data != null && data.hasSaveData;
    }

    public static void DeleteSave()
    {
        try
        {
            if (File.Exists(SavePath)) File.Delete(SavePath);
            if (File.Exists(SavePath + ".tmp")) File.Delete(SavePath + ".tmp");
        }
        catch (Exception e) { Debug.LogError("[SAVE] Không xóa được Save: " + e); }
    }

    public static void LogSavePath() => Debug.Log("[SAVE] Đường dẫn: " + SavePath);

    private static void Normalize(GameSaveData d)
    {
        if (d.cases == null) d.cases = new System.Collections.Generic.List<CaseSaveData>();
        if (d.collectedEvidence == null) d.collectedEvidence = new System.Collections.Generic.List<CollectedEvidenceSaveData>();
        if (d.statements == null) d.statements = new System.Collections.Generic.List<StatementSaveData>();
        if (d.storyStatementOverrides == null) d.storyStatementOverrides = new System.Collections.Generic.List<StatementSaveData>();
        if (d.askedQuestionKeys == null) d.askedQuestionKeys = new System.Collections.Generic.List<string>();
        if (d.completedStoryKeys == null) d.completedStoryKeys = new System.Collections.Generic.List<string>();
        if (d.collectedCipherIds == null) d.collectedCipherIds = new System.Collections.Generic.List<string>();
        if (d.solvedCipherIds == null) d.solvedCipherIds = new System.Collections.Generic.List<string>();
        if (d.deadCharacters == null) d.deadCharacters = new System.Collections.Generic.List<CharacterDeathSaveData>();
        if (d.viewingCaseNumber < 1 || d.viewingCaseNumber > d.currentCaseNumber)
            d.viewingCaseNumber = d.currentCaseNumber;
        foreach (CaseSaveData c in d.cases)
            if (c != null && c.spawnedEvidence == null) c.spawnedEvidence = new System.Collections.Generic.List<SpawnedEvidenceSaveData>();
    }
}
