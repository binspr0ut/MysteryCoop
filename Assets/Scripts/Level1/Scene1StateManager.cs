using Unity.Netcode;
using UnityEngine;
using System;
using System.Collections.Generic;

public class Scene1StateManager : NetworkBehaviour
{
    public static Scene1StateManager Instance;

    public NetworkVariable<Level1State> CurrentState =
        new NetworkVariable<Level1State>(Level1State.ExploreBuilding);

    public event Action<Level1State, string> OnQuestTitleChanged;
    public event Action<Level1State> OnStateChanged;


    [Header("Object References")]
    public MonoBehaviour suitcaseObject;
    public List<MonoBehaviour> otherObjects = new();

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


    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        CurrentState.OnValueChanged += HandleStateChanged;
        BroadcastTitle(CurrentState.Value);
        ApplyStateSettings(CurrentState.Value);
    }

    private void HandleStateChanged(Level1State oldState, Level1State newState)
    {
        Debug.Log($"[LEVEL STATE] {oldState} → {newState}");

        ApplyStateSettings(newState);
        BroadcastTitle(newState);


        OnStateChanged?.Invoke(newState);

        if (newState == Level1State.Completed)
        {
            NetworkManager.SceneManager.LoadScene("EndingScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }
    private void BroadcastTitle(Level1State state)
    {
        string title = GetTitle(state);
        OnQuestTitleChanged?.Invoke(state, title);
        UpdateClientTitleClientRpc(title);
    }


    public string GetTitle(Level1State state)
    {
        return state switch
        {
            Level1State.ExploreBuilding => "Explore the building and inspect key objects",
            Level1State.FindSuitcaseCode => "Find the code to unlock the suitcase",
            Level1State.Completed => "Objective Completed!",
            _ => ""
        };
    }
    public string GetTitle() => GetTitle(CurrentState.Value);

    [ClientRpc]
    private void UpdateClientTitleClientRpc(string title)
    {
        OnQuestTitleChanged?.Invoke(CurrentState.Value, title);
    }

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

        // Active objects
        SetState(shelfLockpickObject, ObjectState.Disabled);
        SetState(boxObject, ObjectState.Disabled);
        SetState(radioObject, ObjectState.Disabled);
        SetState(clockBackObject, ObjectState.Disabled);
        SetState(parabolaObject, ObjectState.Disabled);

        // Locked objects
        SetState(shelfOpenedObject, ObjectState.Disabled);
        SetState(clockObject, ObjectState.Disabled);
    }

    private void ApplyFindSuitcaseState()
    {
        // Active objects
        SetState(shelfLockpickObject, ObjectState.Active);
        SetState(boxObject, ObjectState.Active);
        SetState(radioObject, ObjectState.Active);
        SetState(clockBackObject, ObjectState.Active);
        SetState(parabolaObject, ObjectState.Active);

        // Locked objects
        SetState(shelfOpenedObject, ObjectState.Locked);
        SetState(clockObject, ObjectState.Locked);
    }



    private void SetState(MonoBehaviour obj, ObjectState state)
    {
        try
        {
            if (obj == null)
            {
                Debug.LogWarning($"[SetState] NULL object for state {state}");
                return;
            }

            if (obj is IStateObject so)
            {
                so.SetObjectState(state);
                Debug.Log($"✅ {obj.name} set to {state}");
            }
            else
            {
                Debug.LogWarning($"⚠️ {obj.name} does not implement IStateObject!");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Error while setting state for {obj}: {ex.Message}");
        }
    }





    // ===== UNLOCK METHODS CALLED BY PUZZLES =====
    [ServerRpc(RequireOwnership = false)]
    public void OnBriefcaseNoteTakenServerRpc()
    {
        if (CurrentState.Value != Level1State.ExploreBuilding) return;

        // Saat note diambil pertama kali, langsung pindah state
        CurrentState.Value = Level1State.FindSuitcaseCode;
    }

    // ===== EXPLORE PROGRESS =====

    [ServerRpc(RequireOwnership = false)]
    public void RegisterExploreInteractionServerRpc()
    {
        if (CurrentState.Value != Level1State.ExploreBuilding) return;

        currentExploreInteractions++;

        if (currentExploreInteractions >= requiredExploreInteractions)
            CurrentState.Value = Level1State.FindSuitcaseCode;
    }

    public void SetState(Level1State newState)
    {
        if (!IsServer) return;
        CurrentState.Value = newState;
    }

}
