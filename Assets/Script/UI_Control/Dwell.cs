using System;
using UnityEngine;
using UnityEngine.UI;


public static class Dwell
{
    public static bool isFlickering { get; private set; } = false;

    public static void HandleDwell(
           bool isHovering,
           bool isFlickering,
           ref float timer,
           ref bool hasTriggered,
           ref float flickerTimer,
           ref int frameCounter,
           ref bool flickerState,
           ref bool isFlickeringRef,
           float dwellTime,
           Image borderImage,
           Image innerImage,
           Color idleColor,
           Color midColor,
           Color activeColor,
           Color flickerOn,
           Color flickerOff,
           int framesPerToggle,
           float flickerDuration,
           Action onDwellComplete,
           Action onFlickerEnd = null)
    {

        if (isFlickering)
    {
        Flicker.UpdateFlickerVisual(
            ref flickerTimer,
            ref frameCounter,
            ref flickerState,
            ref isFlickeringRef,
            innerImage,
            flickerOn,
            flickerOff,
            framesPerToggle,
            flickerDuration,
            onFlickerEnd);
        return;
    }

    if (isHovering)
    {
        DwellMain(
            ref timer,
            ref hasTriggered,
            dwellTime,
            borderImage,
            idleColor,
            midColor,
            activeColor,
            onDwellComplete);
    }
    else
    {
        ResetDwell(
            ref timer,
            ref hasTriggered,
            borderImage,
            idleColor);
    }
    }

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
            onDwellComplete?.Invoke();
            hasTriggered = true;
        }
    }

    public static void ResetDwell(
      ref float timer,
      ref bool hasTriggered,
      Image borderImage,
      Color idleColor)
    {
        timer = 0f;
        hasTriggered = false;

        if (borderImage)
            borderImage.color = idleColor;
    }


}
