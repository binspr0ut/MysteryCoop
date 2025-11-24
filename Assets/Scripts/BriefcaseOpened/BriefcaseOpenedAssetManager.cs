using Microsoft.Unity.VisualStudio.Editor;
using Unity.Netcode;
using UnityEngine;

public class BriefcaseOpenedAssetManager : NetworkBehaviour
{
    [Header("Calendar")]
    public GameObject SeptemberDetectiveTexture;
    public GameObject SeptemberSpiritTexture;

    public GameObject OktoberDetectiveTexture;
    public GameObject OktoberSpiritTexture;

    public GameObject NovemberDetectiveTexture;
    public GameObject NovemberSpiritTexture;

    public GameObject DesemberDetectiveTexture;
    public GameObject DesemberSpiritTexture;


    public override void OnNetworkSpawn()
    {
        if (!IsClient) return;

        bool isDetective = IsHost;  // Host = Detective, Client = Spirit

        ApplyWorldTexture(isDetective);
    }

    private void ApplyWorldTexture(bool isDetective)
    {
        SetActive(SeptemberDetectiveTexture, isDetective);
        SetActive(SeptemberSpiritTexture, !isDetective);

        SetActive(OktoberDetectiveTexture, isDetective);
        SetActive(OktoberSpiritTexture, !isDetective);

        SetActive(NovemberDetectiveTexture, isDetective);
        SetActive(NovemberSpiritTexture, !isDetective);

        SetActive(DesemberDetectiveTexture, isDetective);
        SetActive(DesemberSpiritTexture, !isDetective);
    }

    private void SetActive(GameObject r, bool active)
    {
        if (r != null) r.SetActive(active);
    }
}
