using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpiritMovement : NetworkBehaviour
{
    public Rigidbody2D rb;
    public float moveSpeed = 5f;
    public Animator animator;
    bool isFacingRight = false;

    float horizontalMovement;
    float verticalMovement;
    private CinemachineCamera cam;

    private PlayerInput input;

    private SpriteRenderer spriteRenderer;
    private Collider2D col;
    private readonly NetworkVariable<bool> isVisible = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            // Disable input for non-owners
            input.enabled = false;
        }

        // Saat nilai berubah, update visual semua client
        isVisible.OnValueChanged += (_, newValue) =>
        {
            SetVisible(newValue);
        };
    }

    [ServerRpc]
    public void SetVisibleServerRpc(bool visible)
    {
        isVisible.Value = visible;
    }

    private void SetVisible(bool visible)
    {
        if (spriteRenderer != null) spriteRenderer.enabled = visible;
        if (col != null) col.enabled = visible;
    }

    void Awake()
    {
        input = GetComponent<PlayerInput>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>(true); // 🔹 tambahkan
        col = GetComponent<Collider2D>();
    }


    void Start()
    {
        cam = GetComponentInChildren<CinemachineCamera>(true);

        if (cam != null)
            cam.gameObject.SetActive(IsOwner); // aktif hanya untuk player sendiri

    }

    void OnEnable()
    {
        Debug.Log($"{name} spawned, IsOwner={IsOwner}, IsLocalPlayer={IsLocalPlayer}, ClientID={OwnerClientId}");
    }


    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        rb.linearVelocity = new Vector2(horizontalMovement * moveSpeed, verticalMovement * moveSpeed);
        animator.SetFloat("magnitude", rb.linearVelocity.magnitude);
        Flip();
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        horizontalMovement = context.ReadValue<Vector2>().x;
        verticalMovement = context.ReadValue<Vector2>().y;
    }

    private void Flip()
    {
        if (isFacingRight && horizontalMovement < 0 || !isFacingRight && horizontalMovement > 0)
        {
            isFacingRight = !isFacingRight;
            Vector3 ls = transform.localScale;
            ls.x *= -1f;
            transform.localScale = ls;

        }
    }


}