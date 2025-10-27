// using Unity.Netcode;
// using UnityEngine;
// using UnityEngine.InputSystem;
// using Unity.Cinemachine;

// public class PlayerNetworkController : NetworkBehaviour
// {
//     [Header("References")]
//     public PlayerInput playerInput;
//     public CinemachineCamera vcam;

//     private void Start()
//     {
//         if (!IsOwner)
//         {
//             // Nonaktifkan input di non-owner
//             playerInput.enabled = false;
//             if (vcam != null) vcam.enabled = false;
//         }
//         else
//         {
//             // Kamera hanya aktif untuk local player
//             if (vcam != null)
//             {
//                 vcam.Follow = transform;
//                 vcam.LookAt = transform;
//                 vcam.enabled = true;
//             }
//         }
//     }
// }
