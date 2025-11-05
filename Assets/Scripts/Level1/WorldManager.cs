using Unity.Netcode;
using UnityEngine;

public class WorldManager : NetworkBehaviour
{
    [Header("Basement")]
    public SpriteRenderer BasementDetectiveTexture;
    public SpriteRenderer BasementSpiritTexture;

    [Header("First Floor")]
    public SpriteRenderer FirstFloorDetectiveTexture;
    public SpriteRenderer FirstFloorSpiritTexture;

    [Header("Second Floor")]
    public SpriteRenderer SecondFloorDetectiveTexture;
    public SpriteRenderer SecondFloorSpiritTexture;

    [Header("Third Floor")]
    public SpriteRenderer ThirdFloorDetectiveTexture;
    public SpriteRenderer ThirdFloorSpiritTexture;

    [Header("Sky")]
    public SpriteRenderer SkyDetectiveTexture;
    public SpriteRenderer SkySpiritTexture;

    [Header("Stair")]
    public SpriteRenderer StairDetectiveTexture;
    public SpriteRenderer StairSpiritTexture;

    [Header("Gudang")]
    public SpriteRenderer GudangDetectiveTexture;
    public SpriteRenderer GudangSpiritTexture;


    public override void OnNetworkSpawn()
    {
        if (!IsClient) return;

        bool isDetective = IsHost;  // Host = Detective, Client = Spirit

        ApplyWorldTexture(isDetective);
    }

    private void ApplyWorldTexture(bool isDetective)
    {
        SetActive(BasementDetectiveTexture, isDetective);
        SetActive(BasementSpiritTexture, !isDetective);

        SetActive(FirstFloorDetectiveTexture, isDetective);
        SetActive(FirstFloorSpiritTexture, !isDetective);

        SetActive(SecondFloorDetectiveTexture, isDetective);
        SetActive(SecondFloorSpiritTexture, !isDetective);

        SetActive(ThirdFloorDetectiveTexture, isDetective);
        SetActive(ThirdFloorSpiritTexture, !isDetective);

        SetActive(SkyDetectiveTexture, isDetective);
        SetActive(SkySpiritTexture, !isDetective);

        SetActive(StairDetectiveTexture, isDetective);
        SetActive(StairSpiritTexture, !isDetective);

        SetActive(GudangDetectiveTexture, isDetective);
        SetActive(GudangSpiritTexture, !isDetective);
    }

    private void SetActive(Renderer r, bool active)
    {
        if (r != null) r.enabled = active;
    }
}
