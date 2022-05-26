using MoreMountains.NiceVibrations;
using UnityEngine;

public class HapticFeedbackController : IGameSaveValueReactor
{
    private readonly ISaveLoadHandler _saveLoad;
    private static bool _IsVibrationOn;

    public HapticFeedbackController(ISaveLoadHandler saveLoad)
    {
        _saveLoad = saveLoad;
        _saveLoad.SubScribe(OnGameSaveValueChanged);
        OnGameSaveValueChanged();
    }
    
    public void OnGameSaveValueChanged()
    {
        _IsVibrationOn = _saveLoad.LoadGameData(Constants.VIBRATION_SAVE_KEY, Defaults.Instance.isVibrationOnDefault);
    }

    public static void PlayHaptic(HapticTypes hapticTypes)
    {
        if (_IsVibrationOn)
        {
            D.Log($"PLAYING HAPTICS TYPE {hapticTypes}");
            #if !UNITY_EDITOR
                        MMVibrationManager.Haptic(hapticTypes);
            #endif
        }
        else
        {
            D.Log("Haptic Disabled");
        }
    }

    public void PlayAndroidHaptic()
    {
        Handheld.Vibrate();
    }
}