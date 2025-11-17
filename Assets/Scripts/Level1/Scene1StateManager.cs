using Unity.Netcode;
using UnityEngine;
using System;
using System.Collections.Generic;

public class Scene1StateManager : NetworkBehaviour
{
    public static Scene1StateManager Instance;

    public NetworkVariable<Level1State> CurrentState =
        new NetworkVariable<Level1State>(Level1State.ExploreBuilding, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public event Action<Level1State, string> OnQuestTitleChanged;
    public event Action<Level1State> OnStateChanged;

    [Header("Object References")]
    [Tooltip("Suitcase / Briefcase object")]
    public MonoBehaviour suitcaseObject;

    [Header("Explore Settings")]
    public int requiredExploreInteractions = 1;
    private int currentExploreInteractions = 0;

    [Header("State Objects - Active in FindSuitcase")]
    public MonoBehaviour shelfLockpickObject;
    public MonoBehaviour boxObject;
    public MonoBehaviour radioObject;
    public MonoBehaviour clockBackObject;
    public MonoBehaviour parabolaObject;

    [Header("State Objects - Locked in FindSuitcase")]
    public MonoBehaviour shelfOpenedObject;
    public MonoBehaviour clockObject;


    /* ================== UNITY EVENTS ================== */

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {

    }

    public override void OnNetworkDespawn()
    {
        // IMPORTANT: Unregister listener to avoid double-callback after scene reload
        CurrentState.OnValueChanged -= HandleStateChanged;
    }


    /* ================== STATE CHANGE CORE ================== */

    public void ChangeState(Level1State newState)
    {
        if (!IsServer) return;

        Level1State oldState = CurrentState.Value;
        CurrentState.Value = newState;

        // Call manually for server, clients will receive via OnValueChanged
        HandleStateChanged(oldState, newState);
    }

    private void HandleStateChanged(Level1State oldState, Level1State newState)
    {
        string who = IsServer ? "[SERVER]" : "[CLIENT]";
        Debug.Log($"{who} [LEVEL STATE] {oldState} → {newState}");

        ApplyStateSettings(newState);
        BroadcastTitle(newState);

        OnStateChanged?.Invoke(newState);

        if (IsServer && newState == Level1State.Completed)
        {
            NetworkManager.SceneManager.LoadScene("EndingScene",
                UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }


    /* ================== STATE UI TITLE ================== */

    private void BroadcastTitle(Level1State state)
    {
        string title = GetTitle(state);

        // Local invoke
        OnQuestTitleChanged?.Invoke(state, title);

        // Tell all clients to update UI
        UpdateClientTitleClientRpc(title);
    }

    [ClientRpc]
    private void UpdateClientTitleClientRpc(string title)
    {
        OnQuestTitleChanged?.Invoke(CurrentState.Value, title);
    }

    public string GetTitle(Level1State state)
    {
        return state switch
        {
            Level1State.ExploreBuilding => "Find the briefcase!",
            Level1State.FindSuitcaseCode => "Unlock the briefcase!",
            Level1State.Completed => "Objective Completed!",
            _ => ""
        };
    }

    public string GetTitle() => GetTitle(CurrentState.Value);


    /* ================== APPLY STATE TO OBJECTS ================== */

    private void ApplyStateSettings(Level1State state)
    {
        switch (state)
        {
            case Level1State.ExploreBuilding:
                ApplyExploreState();
                break;

            case Level1State.FindSuitcaseCode:
                ApplyFindSuitcaseState();
                break;
        }
    }

    private void ApplyExploreState()
    {
        SetState(suitcaseObject, ObjectState.Active);

        SetState(shelfLockpickObject, ObjectState.Disabled);
        SetState(boxObject, ObjectState.Disabled);
        SetState(radioObject, ObjectState.Disabled);
        SetState(clockBackObject, ObjectState.Disabled);
        SetState(parabolaObject, ObjectState.Disabled);

        SetState(shelfOpenedObject, ObjectState.Disabled);
        SetState(clockObject, ObjectState.Disabled);

        LogStateApplied("ExploreBuilding");
    }

    private void ApplyFindSuitcaseState()
    {
        SetState(shelfLockpickObject, ObjectState.Active);
        SetState(boxObject, ObjectState.Active);
        SetState(radioObject, ObjectState.Active);
        SetState(clockBackObject, ObjectState.Active);
        SetState(parabolaObject, ObjectState.Active);

        SetState(shelfOpenedObject, ObjectState.Locked);
        SetState(clockObject, ObjectState.Locked);

        LogStateApplied("FindSuitcaseCode");
    }

    private void SetState(MonoBehaviour obj, ObjectState state)
    {
        if (obj == null)
        {
            Debug.LogWarning($"[SetState] NULL object for state {state}");
            return;
        }

        if (obj is IStateObject so)
        {
            so.SetObjectState(state);
            Debug.Log($"{(IsServer ? "[SERVER]" : "[CLIENT]")} ✅ {obj.name} set to {state}");
        }
        else
        {
            Debug.LogWarning($"⚠️ {obj.name} does not implement IStateObject!");
        }
    }

    private void LogStateApplied(string state)
    {
        Debug.Log($"{(IsServer ? "[SERVER]" : "[CLIENT]")} [STATE APPLY] {state} state applied");
    }


    /* ================== CALLS FROM PUZZLES ================== */

    [ServerRpc(RequireOwnership = false)]
    public void OnBriefcaseNoteTakenServerRpc()
    {
        if (CurrentState.Value != Level1State.ExploreBuilding) return;
        ChangeState(Level1State.FindSuitcaseCode);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RegisterExploreInteractionServerRpc()
    {
        if (CurrentState.Value != Level1State.ExploreBuilding) return;

        currentExploreInteractions++;

        if (currentExploreInteractions >= requiredExploreInteractions)
            ChangeState(Level1State.FindSuitcaseCode);
    }

    private void RegisterEvents()
    {
        if (shelfLockpickObject is ShelfLockpick shelf)
        {
            shelf.OnShelfUnlocked += () =>
            {
                // ubah shelfOpened menjadi Active
                if (shelfOpenedObject is IStateObject so)
                    so.SetObjectState(ObjectState.Active);
            };
        }
    }

}
