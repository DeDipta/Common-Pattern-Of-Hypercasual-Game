using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    private ISaveLoadHandler _saveLoadHandler;
    
    [SerializeField] private AudioController _audioController;

    private HapticFeedbackController _hapticFeedbackController;
    void Start()
    {
        SetUp();
    }

    void SetUp()
    {
        _saveLoadHandler = new SaveLoadHandler();

        _audioController.SetUp(_saveLoadHandler);
        _hapticFeedbackController = new HapticFeedbackController(_saveLoadHandler);
    }
}