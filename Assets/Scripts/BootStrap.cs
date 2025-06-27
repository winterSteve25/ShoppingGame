using PrimeTween;
using UnityEngine;

public static class BootStrap
{
    [RuntimeInitializeOnLoadMethod]
    private static void Boot()
    {
        PrimeTweenConfig.warnTweenOnDisabledTarget = false;
        PrimeTweenConfig.warnEndValueEqualsCurrent = false;
    }
}