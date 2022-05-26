using System;
public interface ISaveLoadHandler
{
    void SubScribe(System.Action subscriber);
    void UnSubScribe(System.Action subscriber);
    void SaveGameData<T>(string key, T value);
    void SaveLevelData<T>(string key, T value);
    T LoadGameData<T>(string key, T defaultValue = default);
    T LoadLevelData<T>(string key, T defaultValue = default);
}

public interface IGameSaveValueReactor
{
    void OnGameSaveValueChanged();
}

public class SaveLoadHandler : ISaveLoadHandler
{
    private const string GAME_DATA_PATH = "GameData.txt";
    private const string LEVEL_DATA_PATH = "LevelData.txt";
    
    private Action OnSaveDataUpdated { get; set; }
    
    public void SubScribe(Action subscriber)
    {
        OnSaveDataUpdated += subscriber;
    }

    public void UnSubScribe(Action subscriber)
    {
        OnSaveDataUpdated -= subscriber;
    }

    public void SaveGameData<T>(string key, T value)
    {
        ES3.Save(key, value, GAME_DATA_PATH);
        OnSaveDataUpdated?.Invoke();
    }

    public void SaveLevelData<T>(string key, T value)
    {
        ES3.Save(key, value, LEVEL_DATA_PATH);
        OnSaveDataUpdated?.Invoke();
    }

    public T LoadGameData<T>(string key, T defaultValue = default)
    {
        if (ES3.KeyExists(key, GAME_DATA_PATH))
        {
            return ES3.Load<T>(key, GAME_DATA_PATH);
        }
        else
        {
            SaveGameData(key, defaultValue);
        }

        return defaultValue;
    }

    public T LoadLevelData<T>(string key, T defaultValue = default)
    {
        if (ES3.KeyExists(key, LEVEL_DATA_PATH))
        {
            return ES3.Load<T>(key, LEVEL_DATA_PATH);
        }
        else
        {
            SaveGameData(key, defaultValue);
        }

        return defaultValue;
    }
}