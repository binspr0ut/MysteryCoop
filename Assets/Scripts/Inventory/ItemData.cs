using UnityEngine;

[CreateAssetMenu(fileName = "New Item Data", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    public string id;
    public string displayName;
    public Sprite icon;

    [Header("Inspect")]
    public bool isInspectable = true;
    public Sprite inspectSprite; //closeup besar
    [TextArea] public string inspectText;
}