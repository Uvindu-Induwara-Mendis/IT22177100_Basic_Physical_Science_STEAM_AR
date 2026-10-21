using UnityEngine;

// =========================================================================================
// UTILITY: ANDROID HARDWARE HAPTIC VIBRATION
// =========================================================================================
// Context: Calls the native Android Vibrator service to produce short, tactile vibration pulses
// when tapping buttons, grabbing AR parts, or completing connections (e.g. 40ms pulse).
public static class HapticFeedbackManager
{
    // Triggers short vibration pulse on Android devices (ignored in Editor/iOS)
    public static void TriggerSubtleHaptic(long duration = 40L)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
            vibrator.Call("vibrate", duration); 
        }
        catch (System.Exception) {}
#endif
    }
}
