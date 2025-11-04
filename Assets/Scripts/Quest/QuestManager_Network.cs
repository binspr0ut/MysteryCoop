using UnityEngine;
using Unity.Netcode;

public enum QuestState { None, ExploreBoardingHouse, FindBriefcasePasscode, Completed }

[DefaultExecutionOrder(-500)] // biar muncul lebih awal (dri gpt)
public class QuestManager_Network : NetworkBehaviour
{
    public static QuestManager_Network I;

    public NetworkVariable<QuestState> currentState = new NetworkVariable<QuestState>(
        QuestState.None, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    void Awake() { I = this; }

    void OnEnable()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnServerStarted += HandleServerStarted;

        currentState.OnValueChanged += OnStateChanged;
    }

    void OnDisable()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnServerStarted -= HandleServerStarted;

        currentState.OnValueChanged -= OnStateChanged;
    }

    void Start()
    {
        // Guard super-aman: kalau host baru dinyalakan via tombol setelah Start(),
        StartCoroutine(EnsureInitialStateWhenServerReady());
    }

    System.Collections.IEnumerator EnsureInitialStateWhenServerReady()
    {
        // Tunggu sampai NetworkManager ada
        while (NetworkManager.Singleton == null) yield return null;

        // Tunggu sampai server aktif (StartHost/StartServer dipanggil)
        while (!NetworkManager.Singleton.IsServer) yield return null;

        EnsureInitialState();
    }

    void HandleServerStarted()
    {
        if (IsServer) EnsureInitialState();
    }

    void EnsureInitialState()
    {
        if (!IsServer) return;

        if (currentState.Value == QuestState.None)
        {
            SetState(QuestState.ExploreBoardingHouse);
        }
        else
        {
            // Kalau sudah ter-set (mis. kembali ke scene yang sama), tetap paksa refresh UI
            QuestUI_Network.UpdateAllClientsUI?.Invoke(GetCurrentTitle());
        }
    }

    void OnStateChanged(QuestState oldV, QuestState newV)
    {
        QuestUI_Network.UpdateAllClientsUI?.Invoke(GetCurrentTitle());
    }

    void SetState(QuestState s)
    {
        if (!IsServer) return;
        currentState.Value = s;
        // (OnStateChanged akan memanggil refresh UI)
    }

    public string GetCurrentTitle()
    {
        switch (currentState.Value)
        {
            case QuestState.ExploreBoardingHouse: return "Telusuri Gedung/Kos-kosan ini";
            case QuestState.FindBriefcasePasscode: return "Cari kombinasi passcode untuk membuka koper";
            case QuestState.Completed: return "Semua quest selesai!";
            default: return "";
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void MarkExploreDone_ServerRpc()
    {
        if (currentState.Value == QuestState.ExploreBoardingHouse)
            SetState(QuestState.FindBriefcasePasscode);
    }

    [ServerRpc(RequireOwnership = false)]
    public void MarkAllDone_ServerRpc()
    {
        if (currentState.Value == QuestState.FindBriefcasePasscode)
            SetState(QuestState.Completed);
    }

    // Di QuestManager_Network.cs
    public void TryAdvanceFromExplore_Server()
    {
        if (!IsServer) return;
        if (currentState.Value == QuestState.ExploreBoardingHouse)
        {
            // langsung set (lebih aman dipanggil dari server code)
            // atau panggil SetState jika methodnya private → ubah ke public jika perlu
            // SetState(QuestState.FindBriefcasePasscode);
            currentState.Value = QuestState.FindBriefcasePasscode;
        }
    }
}