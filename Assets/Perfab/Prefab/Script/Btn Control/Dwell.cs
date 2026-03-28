using System;
using UnityEngine;
using UnityEngine.UI;

public static class Dwell
{
    // 
    public static bool isFlickering { get; private set; } = false;

    public static void DwellMain(
    ref float timer,
    ref bool hasTriggered,
    float dwellTime,
    Image borderImage,
    Color idleColor,
    Color midColor,
    Color activeColor,
    Action onDwellComplete)
    {
        Color c = borderImage.color;
        c.a = 1f;
        borderImage.color = c; 
        timer += Time.deltaTime;
        float progress = Mathf.Clamp01(timer / dwellTime);
        if (borderImage)
        {
            if (progress < 0.5f)
            {
                float t = progress / 0.5f;
                borderImage.color = Color.Lerp(idleColor, midColor, t);
            }
            else
            {
                float t = (progress - 0.5f) / 0.5f;
                borderImage.color = Color.Lerp(midColor, activeColor, t);
            }
        }
        if (timer >= dwellTime && !hasTriggered)
        {
            hasTriggered = true;
            onDwellComplete?.Invoke();
            
        }
    }

    public static void ResetDwell( ref float timer, ref bool hasTriggered, Image borderImage, Color idleColor)
    {
        timer = 0f;
        hasTriggered = false;
        if (borderImage)
        {
            Color c = borderImage.color;
            c.a = 0f;
            borderImage.color = idleColor;
            borderImage.color = c;
        }
    }

    public static void Invisble( Image image)
    {
        if (image)
        {
            Color c = image.color;
            c.a = 0f;
            image.color = c;
        }
    }

    public static void Invisble(SpriteRenderer image)
    {
        if (image)
        {
            Color c = image.color;
            c.a = 0f;
            image.color = c;
        }
    }
    public static void Visble(Image image)
    {
        if (image)
        {
            Color c = image.color;
            c.a = 1f;
            image.color = c;
        }

    }
    public static void Visble(SpriteRenderer image)
    {
        if (image)
        {
            Color c = image.color;
            c.a = 1f;
            image.color = c;
        }

    }
    public static void Visble(Image image, Color idle)
    {
        if (image)
        {
            Color c = image.color;
            c.a = 1f;
            image.color = c;
        }
        if (idle != null)
            image.color = idle;
    }
}
