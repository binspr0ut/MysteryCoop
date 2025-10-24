using UnityEngine;

public class LampRope : MonoBehaviour, IObject
{
    [Header("Scene References")]
    [SerializeField] private GameObject BasementCover;
    [SerializeField] private SpriteRenderer BasementRenderer;

    [Header("Sprites")]
    [SerializeField] private Sprite onSprite;
    [SerializeField] private Sprite offSprite;


    private bool isOn = false;

    public bool CanInteract() => true;

    public void Interact(Transform player)
    {
        isOn = !isOn;

        // Toggle cover visibility
        BasementCover.SetActive(isOn);

        // Change texture based on state
        if (BasementRenderer != null)
        {
            BasementRenderer.sprite = isOn ? offSprite : onSprite;
        }
    }

    private void Start()
    {
        isOn = false;
        BasementCover.SetActive(!isOn);
        BasementRenderer.sprite = offSprite;
    }

}
