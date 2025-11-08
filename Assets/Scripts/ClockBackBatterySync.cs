using UnityEngine;

public class ClockBackBatterySync : MonoBehaviour
{
    public static ClockBackBatterySync Instance;

    private void Awake()
    {
        Instance = this;
    }

    public int CurrentBattery { get; private set; }

    public event System.Action<int> OnBatteryChanged;

    public void UpdateBatteryCount(int count)
    {
        CurrentBattery = count;
        OnBatteryChanged?.Invoke(count);
    }
}
