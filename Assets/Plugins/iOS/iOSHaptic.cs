using System.Runtime.InteropServices;
using UnityEngine;

public static class iOSHaptic
{
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void Haptic_Impact(int style);

    [DllImport("__Internal")]
    private static extern void Haptic_Notification(int type);

    [DllImport("__Internal")]
    private static extern void Haptic_Selection();
#else
    // Editor fallback (no error)
    private static void Haptic_Impact(int style) { }
    private static void Haptic_Notification(int type) { }
    private static void Haptic_Selection() { }
#endif

    // ===============================
    // PUBLIC WRAPPERS
    // ===============================
    public static void ImpactLight() => Haptic_Impact(0);
    public static void ImpactMedium() => Haptic_Impact(1);
    public static void ImpactHeavy() => Haptic_Impact(2);
    public static void ImpactRigid() => Haptic_Impact(3);
    public static void ImpactSoft() => Haptic_Impact(4);

    public static void NotifySuccess() => Haptic_Notification(0);
    public static void NotifyWarning() => Haptic_Notification(1);
    public static void NotifyError() => Haptic_Notification(2);

    public static void Selection() => Haptic_Selection();
}
