using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Balaso.AppTrackingTransparency;

public class ATTResult
{

#if UNITY_IOS
    public AuthorizationStatus authorizationStatus;
    public bool isUpdated = false;
#endif

}
