using System;
using Balaso;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Example MonoBehaviour class requesting iOS Tracking Authorization
/// </summary>
public class AppTrackingTransparencyManager : MonoBehaviour
{
    private static AppTrackingTransparencyManager instance;
    #if UNITY_IOS
    private static ATTResult Result { get; set; }
    #endif

    public UnityEvent onGameReadyToInit;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            return;
        }

        instance = this;
        #if UNITY_IOS
        Result = new ATTResult() { authorizationStatus = AppTrackingTransparency.AuthorizationStatus.NOT_DETERMINED, isUpdated = false };
        AppTrackingTransparency.RegisterAppForAdNetworkAttribution();
        //AppTrackingTransparency.UpdateConversionValue(3);
        #endif

    }

    void Start()
    {
        #if UNITY_IOS
        D.Log("Request AppTrackingTransparency");
        AppTrackingTransparency.OnAuthorizationRequestDone += OnAuthorizationRequestDone;
        AppTrackingTransparency.AuthorizationStatus currentStatus = AppTrackingTransparency.TrackingAuthorizationStatus;
        D.Log($"Current authorization status: {currentStatus.ToString()}");

        if (currentStatus != AppTrackingTransparency.AuthorizationStatus.AUTHORIZED)
        {
            D.Log("Requesting authorization...");
            AppTrackingTransparency.RequestTrackingAuthorization();
            Result.isUpdated = false;
        }
        else
        {
            Result.isUpdated = true;
            Result.authorizationStatus = currentStatus;
            MoveToGameInit();
        }
        #endif

        #if UNITY_ANDROID
        MoveToGameInit();
        #endif
    }

    #if UNITY_IOS

    /// <summary>
    /// Callback invoked with the user's decision
    /// </summary>
    /// <param name="status"></param>
    private void OnAuthorizationRequestDone(AppTrackingTransparency.AuthorizationStatus status)
    {

        Result.isUpdated = true;
        Result.authorizationStatus = status;

        switch (status)
        {
            case AppTrackingTransparency.AuthorizationStatus.NOT_DETERMINED:
                D.Log("AuthorizationStatus: NOT_DETERMINED");
                break;
            case AppTrackingTransparency.AuthorizationStatus.RESTRICTED:
                D.Log("AuthorizationStatus: RESTRICTED");
                break;
            case AppTrackingTransparency.AuthorizationStatus.DENIED:
                D.Log("AuthorizationStatus: DENIED");
                break;
            case AppTrackingTransparency.AuthorizationStatus.AUTHORIZED:
                D.Log("AuthorizationStatus: AUTHORIZED");
                break;
        }

        // Obtain IDFA
        D.Log($"IDFA: {AppTrackingTransparency.IdentifierForAdvertising()}");

        // START THE GAME FROM HERE
        MoveToGameInit();
    }
    #endif

    void MoveToGameInit()
    {
        if (onGameReadyToInit != null)
            functionMustCallFromMainThread = new Action(onGameReadyToInit.Invoke);
    }

    /// <summary>
    /// Issue : Game fail to run when we call Unity function from other thread, Ex : Scene loading, Audio play etc.
    /// </summary>
    Action functionMustCallFromMainThread = null;

    void Update()
    {
        if (functionMustCallFromMainThread != null)
        {
            functionMustCallFromMainThread.Invoke();
            functionMustCallFromMainThread = null;
        }
    }
}