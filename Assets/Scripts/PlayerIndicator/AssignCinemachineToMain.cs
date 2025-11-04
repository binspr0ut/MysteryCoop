// using UnityEngine;
// using Unity.Netcode;
// using Cinemachine; // <-- penting

// public class AssignCinemachineToMain : NetworkBehaviour
// {
//     public CinemachineCamera cam;  // <-- CM3 pakai CinemachineCamera

//     public override void OnNetworkSpawn()
//     {
//         if (IsOwner)
//         {
//             var mainCam = Camera.main;
//             if (mainCam != null)
//             {
//                 // pastikan ada Brain di Main Camera
//                 var brain = mainCam.GetComponent<CinemachineBrain>();
//                 if (brain == null) brain = mainCam.gameObject.AddComponent<CinemachineBrain>();

//                 cam.Priority = 100;    // aktifkan kamera milik player ini
//                 Debug.Log($"🎥 {gameObject.name} now controls MainCamera");
//             }
//             else
//             {
//                 Debug.LogWarning("Main Camera not found (tag 'MainCamera').");
//             }
//         }
//         else
//         {
//             cam.Priority = 0;         // matikan kamera milik pemain lain di klien ini
//         }
//     }
// }