using UnityEngine;
using UnityEngine.UI;        // untuk Image lamp indicator

public class ElectricPuzzle : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject puzzlePanel;   // isi: PuzzleRoot

    [Header("SFX UI")]
    [SerializeField] private AudioClip openPuzzleSFX;
    [SerializeField] private AudioClip closePuzzleSFX;

    [Header("Groups")]
    [SerializeField] private GameObject wireGroup;     // parent untuk node kabel (WireGroup)
    [SerializeField] private GameObject switchGroup;   // parent untuk saklar (SwitchGroup)
    [SerializeField] private GameObject lampGroup;     // parent untuk lampu indikator (LampGroup)

    [Header("Background Sprites")]
    [SerializeField] private Image backgroundImage;    // Image di BackgroundPanel
    [SerializeField] private Sprite detectiveSprite;   // punya saklar & kabel
    [SerializeField] private Sprite spiritSprite;      // khusus kabel

    [Header("Wire Visuals (Spirit view)")]
    [SerializeField] private Image[] wireImages;   // urutan sesuai WireColor



    private bool isOpen = false;
    private bool isSolved = false;

    public bool IsOpen => isOpen;
    public bool IsSolved => isSolved;

    // ===========================
    //  LOGIC KABEL & SAKLAR
    // ===========================

    [Header("Lamp Indicators (urutan sesuai enum WireColor)")]
    [SerializeField] private Image[] lampImages;    // size harus sesuai jumlah warna = 4

    [Header("Lamp Colors")]
    [SerializeField] private Color lampOnColor = Color.yellow;
    [SerializeField] private Color lampOffColor = Color.gray;

    // kabelConnected[i] = kabel warna i tersambung
    private bool[] cableConnected = new bool[4];

    // switchOn[i] = saklar warna i ON
    private bool[] switchOn = new bool[4];


    // ===========================
    //   PUZZLE PANEL OPEN/CLOSE
    // ===========================

    //SOUNFEFFFFFFEKK
    private void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        if (AudioManager.Instance == null) return;

        AudioManager.Instance.PlaySFX(clip);
    }


    // Dipanggil dari ElectricTrigger saat pemain interact
    public void OpenPuzzle()
    {
        // fungsi generic, kalau kamu mau buka semua
        if (isSolved || puzzlePanel == null) return;

        isOpen = true;
        puzzlePanel.SetActive(true);

        // default: kedua group aktif
        if (wireGroup != null) wireGroup.SetActive(true);
        if (switchGroup != null) switchGroup.SetActive(true);

        // 🔊 SFX buka UI
        PlaySFX(openPuzzleSFX);

        Debug.Log("[ElectricPuzzle] OpenPuzzle");
    }

    // 👻 Dipanggil dari ElectricTriggerSpirit (khusus HANTU)
    public void OpenForSpirit()
    {
        if (isSolved || puzzlePanel == null) return;

        isOpen = true;
        puzzlePanel.SetActive(true);

        if (wireGroup != null) wireGroup.SetActive(true);     // 👻 lihat kabel
        if (switchGroup != null) switchGroup.SetActive(false); // saklar disembunyikan
        if (lampGroup != null) lampGroup.SetActive(false);    // lampu disembunyikan

        // ganti background ke versi SPIRIT
        if (backgroundImage != null && spiritSprite != null)
            backgroundImage.sprite = spiritSprite;

        // 🔊 SFX buka UI
        PlaySFX(openPuzzleSFX);

        Debug.Log("[ElectricPuzzle] OpenForSpirit");
    }

    // 🕵️ Dipanggil dari ElectricTrigger (khusus DETEKTIF)
    public void OpenForDetective()
    {
        if (isSolved || puzzlePanel == null) return;

        isOpen = true;
        puzzlePanel.SetActive(true);

        if (wireGroup != null) wireGroup.SetActive(false);   // 🕵️ gak bisa main kabel
        if (switchGroup != null) switchGroup.SetActive(true); // cuma saklar
        if (lampGroup != null) lampGroup.SetActive(true);     // lampu indikator kelihatan

        // ganti background ke versi DETEKTIF
        if (backgroundImage != null && detectiveSprite != null)
            backgroundImage.sprite = detectiveSprite;

        // 🔊 SFX buka UI
        PlaySFX(openPuzzleSFX);

        Debug.Log("[ElectricPuzzle] OpenForDetective");
    }

    public void ClosePuzzle()
    {
        if (puzzlePanel == null) return;

        isOpen = false;
        puzzlePanel.SetActive(false);

        // 🔊 SFX tutup UI
        PlaySFX(closePuzzleSFX);

        Debug.Log("[ElectricPuzzle] ClosePuzzle");
    }

    // Dipanggil dari tombol X / Close
    public void OnCloseButtonPressed()
    {
        ClosePuzzle();
    }


    // ===========================
    //          KABEL
    // ===========================

    // Dipanggil oleh WireNode 
    public void SetCableConnected(WireColor color, bool connected)
    {
        int i = (int)color;
        cableConnected[i] = connected;

        UpdateLampVisual(color);

        // Nyalain / matiin visual kabel penuh
        if (wireImages != null && i >= 0 && i < wireImages.Length && wireImages[i] != null)
        {
            wireImages[i].enabled = connected;
            wireImages[i].gameObject.SetActive(connected);
        }

        // kalau kabel diputus, saklar otomatis mati
        if (!connected)
        {
            switchOn[i] = false;
        }

        CheckSolved();
    }

    // update warna lampu indikator sesuai status kabel
    private void UpdateLampVisual(WireColor color)
    {
        int i = (int)color;

        if (lampImages == null || i >= lampImages.Length) return;
        if (lampImages[i] == null) return;

        lampImages[i].color = cableConnected[i] ? lampOnColor : lampOffColor;
    }


    public bool IsCableConnected(WireColor color)
    {
        int i = (int)color;
        if (i < 0 || i >= cableConnected.Length) return false;
        return cableConnected[i];
    }

    // dipakai Spirit untuk preview kabel waktu drag
    public void SetWirePreview(WireColor color, bool visible)
    {
        int i = (int)color;
        if (wireImages == null || i >= wireImages.Length) return;
        if (wireImages[i] == null) return;

        // kalau kabel sudah resmi connected, jangan dimatikan paksa
        if (cableConnected[i] && !visible) return;

        wireImages[i].enabled = visible;
        if (wireImages[i] != null)
            wireImages[i].gameObject.SetActive(visible);
    }


    // ===========================
    //          SAKLAR
    // ===========================

    // Dipanggil dari ElectricSwitch
    public void ToggleSwitch(WireColor color)
    {
        int i = (int)color;

        // Saklar hanya bekerja kalau kabel warna itu sudah tersambung
        if (!cableConnected[i])
        {
            Debug.Log($"[ElectricPuzzle] Switch {color} ignored (cable not connected)");
            return;
        }

        switchOn[i] = !switchOn[i];
        Debug.Log($"[ElectricPuzzle] Switch {color} is now {(switchOn[i] ? "ON" : "OFF")}");

        CheckSolved();
    }

    public bool IsSwitchOn(WireColor color)
    {
        int i = (int)color;
        if (i < 0 || i >= switchOn.Length) return false;
        return switchOn[i];
    }



    // ===========================
    //       CEK PUZZLE SELESAI
    // ===========================

    private void CheckSolved()
    {
        if (isSolved) return;

        for (int i = 0; i < cableConnected.Length; i++)
        {
            if (!cableConnected[i]) return;
            if (!switchOn[i]) return;
        }

        // Semua kabel connected & semua saklar ON
        Debug.Log("[ElectricPuzzle] ALL cables & switches done → MarkSolved()");
        MarkSolved();
    }


    // Dipanggil nanti kalau puzzle sudah solved (semua kabel & saklar benar)
    public void MarkSolved()
    {
        if (isSolved) return;

        isSolved = true;
        ClosePuzzle();

        // ⚠ Komentar kamu tetap dipertahankan
        // Dipanggil nanti kalau puzzle sudah solved (semua kabel & saklar benar)
        if (Scene1StateManager.Instance != null)
        {
            Scene1StateManager.Instance.OnElectricPuzzleSolvedServerRpc();
        }
    }
}