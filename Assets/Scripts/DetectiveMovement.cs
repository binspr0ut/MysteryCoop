using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class DetectiveMovement : NetworkBehaviour
{
    public Rigidbody2D rb;
    public float moveSpeed = 5f;
    public Animator animator;
    bool isFacingRight = true;
    float horizontalMovement;
    private CinemachineCamera cam;
    private PlayerInput input;
    public float HorizontalDirection => horizontalMovement;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            // Disable input for non-owners
            input.enabled = false;
        }
    }

    void Awake()
    {
        input = GetComponent<PlayerInput>();
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

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;
        rb.linearVelocity = new Vector2(horizontalMovement * moveSpeed, rb.linearVelocityY);
        animator.SetFloat("magnitude", rb.linearVelocity.magnitude);
        Flip();
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        horizontalMovement = context.ReadValue<Vector2>().x;
    }
}
