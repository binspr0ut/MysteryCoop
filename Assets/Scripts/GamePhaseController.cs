using Unity.Netcode;
using UnityEngine;

public enum GamePhase : byte { Cutscene = 0, RoleSelect = 1, Gameplay = 2 }

public class GamePhaseController : NetworkBehaviour
{
    public readonly NetworkVariable<GamePhase> phase =
        new NetworkVariable<GamePhase>(GamePhase.Cutscene);

    [ServerRpc(RequireOwnership = false)]
    public void StartGameplayServerRpc()
    {
        phase.Value = GamePhase.Gameplay;
        // TODO: aktifkan HUD gameplay, enable movement, disable overlay, dsb.
        // Kamu sudah di scene TextObjective, jadi tidak perlu SwitchScene.
    }
}