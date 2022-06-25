using UnityEngine;

public class Defaults : MonoBehaviour
{
    public static Defaults Instance;

    private void Awake()
    {
        Instance = this;
    }

    public int levelDefault;
    public bool isTutorialAvailable;
    public bool isMusicOnDefault;
    public bool isSfxOnDefault;
    public bool isVibrationOnDefault;
}