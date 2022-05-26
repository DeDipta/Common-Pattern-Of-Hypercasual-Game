using System;
using UnityEngine;

public class AudioController : MonoBehaviour, IGameSaveValueReactor
{
    [SerializeField] private Audio[] audios;
    private ISaveLoadHandler _saveLoad;
    private static bool _IsSfxOn;
    private static bool _IsMusicOn;
    public static AudioController Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void SetUp(ISaveLoadHandler saveLoad)
    {
        _saveLoad = saveLoad;
        _saveLoad.SubScribe(OnGameSaveValueChanged);
        OnGameSaveValueChanged();
    }
    public void OnGameSaveValueChanged()
    {
        _IsSfxOn = _saveLoad.LoadGameData(Constants.SFX_SAVE_KEY, Defaults.Instance.isSfxOnDefault);
        _IsMusicOn = _saveLoad.LoadGameData(Constants.MUSIC_SAVE_KEY, Defaults.Instance.isMusicOnDefault);

        AudioSource bgSource = GetCorrectAudio(AudioType.Bg);
        if (bgSource != null)
        {
            if (!_IsMusicOn)
                bgSource.Stop();
            
            else
                bgSource.Play();
        }
    }

    public void PlayAudio(AudioType type)
    {
        if (!_IsSfxOn)
            return;

        AudioSource source = GetCorrectAudio(type);

        if (source != null)
            source.Play();
    }

    private AudioSource GetCorrectAudio(AudioType audioType)
    {
        foreach (var t in audios)
        {
            if (t.type == audioType)
            {
                return t.source;
            }
        }
        return null;
    }
}

[System.Serializable]
public class Audio
{
    public AudioType type;
    public AudioSource source;
}

public enum AudioType
{
    Bg,
    Button,
}