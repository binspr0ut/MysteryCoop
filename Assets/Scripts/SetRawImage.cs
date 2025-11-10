using UnityEngine;
using UnityEngine.UI;

public class RawImageSpriteSetter : MonoBehaviour
{
    [SerializeField] private RawImage rawImage;
    [SerializeField] private Sprite spriteToShow;

    private void Start()
    {
        if (spriteToShow != null)
        {
            // Buat Texture2D baru dari Sprite
            Texture2D croppedTexture = CropSprite(spriteToShow);
            rawImage.texture = croppedTexture;
        }
    }

    private Texture2D CropSprite(Sprite sprite)
    {
        var tex = sprite.texture;
        var rect = sprite.textureRect;

        Texture2D newTex = new Texture2D((int)rect.width, (int)rect.height);
        Color[] pixels = tex.GetPixels(
            (int)rect.x,
            (int)rect.y,
            (int)rect.width,
            (int)rect.height
        );
        newTex.SetPixels(pixels);
        newTex.Apply();
        return newTex;
    }
}
