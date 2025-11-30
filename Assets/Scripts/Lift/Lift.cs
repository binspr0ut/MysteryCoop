using System;
using System.Security.Cryptography;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Lift : NetworkBehaviour, IObject, IStateObject
{
    public bool IsInteracted { get; private set; }
    public string ID { get; private set; }

    [Header("Lift UI References")]
    public GameObject LiftOverlay;
    public GameObject ControlUI;
    private float fadeDuration = 0.4f;

    private ulong interactorClientId; // simpan siapa yang terakhir interaksi
    private ObjectState currentState = ObjectState.Locked;
    [SerializeField] private Collider2D interactionCollider;
    private bool hasChangedState = false;

    [Header("Fade Overlay")]
    public CanvasGroup blackFadeCanvas;  // drag panel hitam
    public AudioSource audioSource;

    [Header("Lift Animation")]
    public Animator lift1Animator;
    public Animator lift2Animator;
    public Animator lift3Animator;
    public TextMeshProUGUI lift1Text;
    public TextMeshProUGUI lift2Text;
    public TextMeshProUGUI lift3Text;


    public float liftAnimDuration = 1.0f;   // durasi animasi LiftOpen

    public int currentFloor = 1;

    public void SetObjectState(ObjectState state)
    {
        currentState = state;

        switch (state)
        {
            case ObjectState.Disabled:
                interactionCollider.enabled = false;
                break;

            case ObjectState.Locked:
                interactionCollider.enabled = true;
                break;

            case ObjectState.Active:
                interactionCollider.enabled = true;
                break;
        }
    }

    public bool CanInteract() => true;

    // Dijalankan saat player tekan tombol "E" atau setara
    public void Interact(Transform playerTransform)
    {
        if (currentState == ObjectState.Locked)
        {
            SubtitleManager.Instance.ShowSubtitle("Looks like the electricity is down.", SubtitleTarget.Detective, SubtitleScope.Global);
            SubtitleManager.Instance.ShowSubtitle("Let’s check the outlet in the basement", SubtitleTarget.Spirit, SubtitleScope.Global);
            if (!hasChangedState)
            {
                Scene1StateManager.Instance.ChangeState(Level1State.TurnElectricity);
                hasChangedState = true;
            }
        }
        else if (currentState == ObjectState.Active)
        {
            ControlUI.SetActive(false);
            LiftOverlay.SetActive(true);
            IsInteracted = true;

            var netObj = playerTransform.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                interactorClientId = netObj.OwnerClientId;
                Debug.Log($"Lift interacted by client {interactorClientId}");
            }
            else
            {
                Debug.LogWarning("Player has no NetworkObject!");
            }
        }
    }

    public bool CanPossess() => currentState == ObjectState.Active || currentState == ObjectState.Locked;

    public void CloseLift()
    {
        Debug.Log("CloseLift triggered");
        LiftOverlay?.SetActive(false);
        ControlUI?.SetActive(true);
        IsInteracted = false;
    }

    //1ST Floor
    public void Go1stFloor()
    {
        Debug.Log("GoFirstFloor pressed");

        if (IsServer)
        {
            // kirim perintah ke client owner untuk teleport dirinya
            Go1stFloorClientRpc(interactorClientId);
        }
        else
        {
            // kirim ke server dulu, baru server broadcast ke owner
            Go1stFloorServerRpc();
        }
        CloseLift();
    }

    [ServerRpc(RequireOwnership = false)]
    private void Go1stFloorServerRpc(ServerRpcParams rpcParams = default)
    {
        Debug.Log("Server received GoUp request");
        Go1stFloorClientRpc(interactorClientId);
    }

    [ClientRpc]
    private void Go1stFloorClientRpc(ulong targetClientId)
    {
        // hanya player owner yang eksekusi ini
        if (NetworkManager.Singleton.LocalClientId != targetClientId) return;

        var player = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().transform;
        Vector3 newPos = player.position;
        newPos.x = 5.6f;
        newPos.y = -1.1f;
        NetworkObject playerObj = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(targetClientId);
        NetworkObjectReference playerRef = playerObj;

        PlayFadeSequenceClientRpc(playerRef, newPos, targetClientId, currentFloor, 1);
        currentFloor = 1;

        Debug.Log($"[Client {targetClientId}] moved self up to {newPos}");
    }

    // 2ND FLOOR
    public void Go2ndFloor()
    {
        Debug.Log("GoUp UI pressed");

        if (IsServer)
        {
            // kirim perintah ke client owner untuk teleport dirinya
            Go2ndFloorClientRpc(interactorClientId);
        }
        else
        {
            // kirim ke server dulu, baru server broadcast ke owner
            Go2ndFloorServerRpc();
        }
        CloseLift();
    }

    [ServerRpc(RequireOwnership = false)]
    private void Go2ndFloorServerRpc(ServerRpcParams rpcParams = default)
    {
        Debug.Log("Server received GoUp request");
        Go2ndFloorClientRpc(interactorClientId);
    }

    [ClientRpc]
    private void Go2ndFloorClientRpc(ulong targetClientId)
    {
        // hanya player owner yang eksekusi ini
        if (NetworkManager.Singleton.LocalClientId != targetClientId) return;

        var player = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().transform;
        Vector3 newPos = player.position;
        newPos.x = 5.6f;
        newPos.y = 7.8f;
        NetworkObject playerObj = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(targetClientId);
        NetworkObjectReference playerRef = playerObj;

        PlayFadeSequenceClientRpc(playerRef, newPos, targetClientId, currentFloor, 2);
        currentFloor = 2;
        Debug.Log($"[Client {targetClientId}] moved self up to {newPos}");
    }

    public void Go3rdFloor()
    {
        Debug.Log("Go 3rd Floor Pressed");

        if (IsServer)
        {
            // kirim perintah ke client owner untuk teleport dirinya
            GoUp3rdFloorClientRpc(interactorClientId);
        }
        else
        {
            // kirim ke server dulu, baru server broadcast ke owner
            Go3rdFloorServerRpc();
        }
        CloseLift();
    }



    [ServerRpc(RequireOwnership = false)]
    private void Go3rdFloorServerRpc(ServerRpcParams rpcParams = default)
    {
        Debug.Log("Server received GoUp request");
        GoUp3rdFloorClientRpc(interactorClientId);
    }



    [ClientRpc]
    private void GoUp3rdFloorClientRpc(ulong targetClientId)
    {
        // hanya player owner yang eksekusi ini
        if (NetworkManager.Singleton.LocalClientId != targetClientId) return;

        var player = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().transform;
        Vector3 newPos = player.position;
        newPos.x = 5.6f;
        newPos.y = 16f;
        NetworkObject playerObj = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(targetClientId);
        NetworkObjectReference playerRef = playerObj;

        PlayFadeSequenceClientRpc(playerRef, newPos, targetClientId, currentFloor, 3);
        currentFloor = 3;
        Debug.Log($"[Client {targetClientId}] moved self up to {newPos}");
    }

    private void Start()
    {
        ID ??= GlobalHelper.GenerateUniqueID(gameObject);
        if (LiftOverlay != null)
            LiftOverlay.SetActive(false);
    }

    [ClientRpc]
    private void PlayFadeSequenceClientRpc(NetworkObjectReference playerRef, Vector3 newPos, ulong targetClientId, int from, int to)
    {
        // Resolve safe: client bisa resolve reference TANPA error server
        if (!playerRef.TryGet(out NetworkObject playerObj))
        {
            Debug.LogWarning("PlayerRef not resolved yet");
            return;
        }

        bool isOwner = (NetworkManager.Singleton.LocalClientId == targetClientId);

        Transform player = playerObj.transform;
        SpriteRenderer sr = player.GetComponentInChildren<SpriteRenderer>();
        if (sr == null) return;

        float fadeT = fadeDuration;
        // OWNER: Fade canvas + fade player
        if (isOwner)
        {
            FadeCanvas(blackFadeCanvas, 0f, 1f, 0.2f).setOnComplete(() =>
            {
                FadeOutInPlayer(from, to);
            });
        }
        else
        {
            // OTHER CLIENTS: fade player only
            FadeOutInPlayer(from, to);
        }

        void FadeOutInPlayer(int from, int to)
        {
            PlayLiftAnimation(from, to);
            LeanTween.value(player.gameObject, 1f, 0f, fadeT)
                .setOnUpdate(v =>
                {
                    var c = sr.color;
                    c.a = v;
                    sr.color = c;
                })
                .setOnComplete(() =>
                {
                    // 2. TELEPORT (langsung setelah fade-out selesai)
                    player.position = newPos;
                    audioSource.Play();

                    // 3. Delay 1 frame → memastikan posisi update dulu
                    LeanTween.delayedCall(1f, () =>
                    {
                        // 4. FADE IN
                        LeanTween.value(player.gameObject, 0f, 1f, fadeT)
                            .setOnUpdate(v =>
                            {
                                var c = sr.color;
                                c.a = v;
                                sr.color = c;
                            })
                            .setOnComplete(() =>
                            {
                                if (isOwner)
                                    FadeCanvas(blackFadeCanvas, 1f, 0f, fadeT);
                            });
                    });
                });
        }

    }

    private void PlayLiftAnimation(int from, int to)
    {
        Animator fromAnimator = null;
        Animator toAnimator = null;


        if (from == 1) fromAnimator = lift1Animator;
        else if (from == 2) fromAnimator = lift2Animator;
        else if (from == 3) fromAnimator = lift3Animator;

        if (to == 1) toAnimator = lift1Animator;
        else if (to == 2) toAnimator = lift2Animator;
        else if (to == 3) toAnimator = lift3Animator;

        if (fromAnimator)
            fromAnimator.SetTrigger("isOpen");

        LeanTween.delayedCall(liftAnimDuration, () =>
        {
            if (toAnimator)
                toAnimator.SetTrigger("isOpen");

            lift1Text.text = to.ToString();
            lift2Text.text = to.ToString();
            lift3Text.text = to.ToString();
        });
    }
    private LTDescr FadeCanvas(CanvasGroup cg, float from, float to, float time)
    {
        cg.alpha = from;
        return LeanTween.value(cg.gameObject, from, to, time)
            .setOnUpdate((float a) =>
            {
                cg.alpha = a;
            });
    }
}