using UnityEngine;

public class Lamp : MonoBehaviour, IPossess
{
    public bool IsPossessed { get; private set; }
    public string ID { get; private set; }
    private Color[] colors = { Color.red, Color.green, Color.blue, Color.yellow, Color.cyan, Color.magenta, Color.white };
    private int currentColorIndex = 0;
    public GameObject spirit;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public SpriteRenderer lightRenderer;
    public bool CanPossess() => true;

    public void Possess()
    {
        if (IsPossessed) return;
        IsPossessed = true;
        // Nonaktifkan hanya visual renderer
        var sprite = spirit.GetComponent<SpriteRenderer>();
        if (sprite) sprite.enabled = false;

        var collider = spirit.GetComponent<Collider2D>();
        if (collider) collider.enabled = false;
        Debug.Log("Lamp possessed!");


    }

    public void Interact()
    {
        if (!IsPossessed) return;

        // Ganti warna
        currentColorIndex = (currentColorIndex + 1) % colors.Length;
        lightRenderer.color = colors[currentColorIndex];
        Debug.Log($"Lamp color changed to {lightRenderer.color}");
    }

    private void Start()
    {

    }


    public void Unpossess()
    {
        if (!IsPossessed) return;
        IsPossessed = false;
        spirit.transform.position = transform.position + Vector3.up * 1f;
        lightRenderer.color = colors[6];
        // Aktifkan kembali visual
        var sprite = spirit.GetComponent<SpriteRenderer>();
        if (sprite) sprite.enabled = true;

        var collider = spirit.GetComponent<Collider2D>();
        if (collider) collider.enabled = true;
        Debug.Log("Lamp unpossessed!");
    }


}
