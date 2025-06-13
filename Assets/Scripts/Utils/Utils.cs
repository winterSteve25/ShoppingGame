using Steamworks.Data;
using UnityEngine;

namespace Utils
{
    public static class Utils
    {
        public static Sprite ToSprite(this Texture2D texture2D, Vector2 pivot)
        {
            return Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), pivot);
        }

        public static Texture2D ToTexture2D(this Image image)
        {
            var avatar = new Texture2D((int)image.Width, (int)image.Height, TextureFormat.ARGB32, false);
            avatar.filterMode = FilterMode.Trilinear;

            // Flip image
            for (var x = 0; x < image.Width; x++)
            {
                for (var y = 0; y < image.Height; y++)
                {
                    var p = image.GetPixel(x, y);
                    avatar.SetPixel(x, (int)image.Height - y,
                        new UnityEngine.Color(p.r / 255.0f, p.g / 255.0f, p.b / 255.0f, p.a / 255.0f));
                }
            }

            avatar.Apply();
            return avatar;
        }
    }
}