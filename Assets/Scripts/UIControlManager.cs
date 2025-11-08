using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem.OnScreen;
using System.Collections;

public class UIControlManager : MonoBehaviour
{
    [Header("References")]
    public GameObject interactButton;
    public GameObject joyStick;
    public GameObject joyStickBg;

    private InteractionDetector interactionDetector;
    private PossesDetector possesDetector;

    [Header("Detective Asset")]
    public Sprite DetectiveInteract;
    public Sprite DetectiveJoystick;
    public Sprite DetectiveJoystickBg;


    [Header("Spirit Asset")]
    public Sprite SpiritInteract;
    public Sprite SpiritJoystick;
    public Sprite SpiritJoystickBg;


    public float transparentValue;
    private bool isBound = false;

    private void Awake()
    {
        if (interactButton != null)
        {
            interactButton.GetComponent<Button>().enabled = false;
            interactButton.GetComponent<OnScreenButton>().enabled = false;
            interactButton.GetComponent<Image>().color = new Color(1f, 1f, 1f, transparentValue);
        }
    }

    private void Start()
    {
        // Jalankan coroutine supaya menunggu sampai player benar-benar spawn
        StartCoroutine(WaitForLocalPlayerAndBind());
    }

    private IEnumerator WaitForLocalPlayerAndBind()
    {
        // Tunggu NetworkManager siap
        while (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsClient)
            yield return null;

        // Tunggu sampai local player object muncul di scene
        NetworkObject localPlayer = null;
        while (localPlayer == null)
        {
            foreach (var player in FindObjectsByType<NetworkObject>(FindObjectsSortMode.None))
            {
                if (player.IsOwner && player.IsPlayerObject)
                {
                    localPlayer = player;
                    break;
                }
            }
            yield return null;
        }

        // Coba ambil detektor yang cocok
        interactionDetector = localPlayer.GetComponentInChildren<InteractionDetector>();
        possesDetector = localPlayer.GetComponentInChildren<PossesDetector>();

        if (interactionDetector != null)
        {
            BindDetectiveUI();
            Debug.Log("✅ Bound to Detective UIControl");
        }
        else if (possesDetector != null)
        {
            BindSpiritUI();
            Debug.Log("✅ Bound to Spirit UIControl");
        }
        else
        {
            Debug.LogWarning("❌ Tidak menemukan detector di player prefab!");
        }
    }

    private void BindDetectiveUI()
    {
        var img = interactButton.GetComponent<Image>();

        if (DetectiveInteract != null && img != null)
        {
            img.sprite = DetectiveInteract;
            joyStick.GetComponent<Image>().sprite = DetectiveJoystick;
            joyStickBg.GetComponent<Image>().sprite = DetectiveJoystickBg;
        }


        interactionDetector.OnRangeChanged += HandleRangeChanged;
        isBound = true;
    }

    private void BindSpiritUI()
    {
        var img = interactButton.GetComponent<Image>();
        if (SpiritInteract != null && img != null)
        {
            img.sprite = SpiritInteract;
            joyStick.GetComponent<Image>().sprite = SpiritJoystick;
            joyStickBg.GetComponent<Image>().sprite = SpiritJoystickBg;
        }
        possesDetector.OnRangeChanged += HandleRangeChanged;
        isBound = true;
    }

    private void HandleRangeChanged(bool inRange)
    {
        SetButtonState(inRange);
    }

    private void SetButtonState(bool active)
    {
        if (interactButton == null) return;

        var button = interactButton.GetComponent<Button>();
        var onScreen = interactButton.GetComponent<OnScreenButton>();
        var image = interactButton.GetComponent<Image>();

        // Enable / disable interaction
        button.enabled = active;
        onScreen.enabled = active;

        // Change color visibility
        if (image != null)
        {
            image.color = active
                ? Color.white                            // visible
                : new Color(1f, 1f, 1f, transparentValue);              // transparent
        }
    }

}
