using Unity.Netcode;
using UnityEngine;

public class LampuBelajar : NetworkBehaviour, IPossess
{
    public bool IsPossessed { get; private set; }
    public string ID { get; private set; }

    [Header("Lamp Visuals")]
    [SerializeField] private GameObject spotLight;

    [Header("UI References")]
    [SerializeField] private GameObject uiControl;      // parent UIControl
    [SerializeField] private GameObject leftStick;      // joystick yang dimatikan saat possess
    [SerializeField] private GameObject interactButton; // tombol tetap aktif untuk unpossess
    [SerializeField] private GameObject bgStick;     // tidak dimatikan

    private SpiritMovement possessedSpirit;
    private int counter = 0;

    // Disinkronkan antar client: lampu ON/OFF
    private NetworkVariable<bool> isLampOn = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server
    );

    public bool CanPossess() => !IsPossessed;

    void Start()
    {
        // Matikan lampu saat mulai
        ApplyLampState(false);
    }

    public override void OnNetworkSpawn()
    {
        // Terapkan state awal (sinkron bagi late joiner)
        ApplyLampState(isLampOn.Value);

        // Dengarkan perubahan state dari server
        isLampOn.OnValueChanged += (_, newVal) => ApplyLampState(newVal);
    }

    private void ApplyLampState(bool on)
    {
        if (spotLight != null && spotLight.activeSelf != on)
            spotLight.SetActive(on);
    }

    // ==== POSSESS LOGIC ====
    public void Possess()
    {
        var spirit = FindFirstObjectByType<SpiritMovement>();
        if (spirit != null && spirit.IsOwner)
        {
            spirit.SetVisibleServerRpc(false);
            possessedSpirit = spirit;
        }

        if (leftStick != null)
            leftStick.SetActive(false);
        if (bgStick != null)
            bgStick.SetActive(false);

        if (interactButton == null)
        {
            var ui = GameObject.Find("UIControl");
            if (ui != null)
                interactButton = ui.transform.Find("InteractButton")?.gameObject;
        }
        if (interactButton != null)
            interactButton.SetActive(true);

        Debug.Log("[LampuBelajar] Possessing object...");
        IsPossessed = true;

        ToggleLampServerRpc(true);

    }

    [ServerRpc(RequireOwnership = false)]
    private void ToggleLampServerRpc(bool on)
    {
        isLampOn.Value = on;
    }

    public void Interact()
    {
        if (IsPossessed)
        {
            Debug.Log("Unposess lampu belajar");
            Unpossess();
            IsPossessed = false;
        }
        else
        {
            if (counter == 0)
            {
                SubtitleManager.Instance.ShowSubtitle(
                                       "Dinda: Hey look!, there's a briefcase, but that is not mine, is it from the murderer?",
                                       SubtitleTarget.Spirit,
                                       SubtitleScope.Global
                                   );

                SubtitleManager.Instance.ShowSubtitle(
                    "Agung: Interesting, let me check it!”",
                    SubtitleTarget.Detective,
                    SubtitleScope.Global
                );

                counter++;
            }

            Debug.Log("Possess lampu belajar");
            Possess();
            IsPossessed = true;
        }
    } // not used

    public void Unpossess()
    {
        if (!IsPossessed) return;

        if (possessedSpirit != null)
        {
            possessedSpirit.SetVisibleServerRpc(true);
            possessedSpirit = null;
        }

        // 🔹 Aktifkan kembali LeftStick
        if (leftStick != null)
            leftStick.SetActive(true);
        if (bgStick != null)
            bgStick.SetActive(true);


        // 🔹 Lampu OFF di server
        ToggleLampServerRpc(false);

        IsPossessed = false;
        Debug.Log("[LampuBelajar] Unpossessed & Lamp turned OFF");
    }
}
