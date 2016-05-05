using UnityEngine;
using System.Collections;

public class BackgroundResizer : MonoBehaviour
{
    public bool ResizeOnStartup = false;

    void Start()
    {        
        if(ResizeOnStartup)
        {
            FitToScreen();
        }
    }

    protected void FitToScreen()
    {
        // Centering sprite
        transform.localPosition = Vector3.zero;

        // Resizing
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            Vector3 newScale = new Vector3(1, 1, 1);

            float spriteWidth = renderer.sprite.bounds.size.x;
            float spriteHeight = renderer.sprite.bounds.size.y;

            float screenHeight = Camera.main.orthographicSize * 2.0f;
            float screenWidth = screenHeight / Screen.height * Screen.width;

            newScale.x = screenWidth / spriteWidth;
            newScale.y = screenHeight / spriteHeight;

            transform.localScale = newScale;
        }
        else
        {
            Debug.LogError("Couldn't resize sprite, because reference to sprite renderer was null.");
        }
    }
}
