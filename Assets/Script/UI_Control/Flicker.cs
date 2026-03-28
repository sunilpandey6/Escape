using UnityEngine;
using UnityEngine.UI;
using System;

public static class Flicker
{
    public static void StartFlicker(
        ref bool isFlickering,
        ref float flickerTimer,
        ref int frameCounter,
        ref bool flickerState)
    {
        isFlickering = true;
        flickerTimer = 0f;
        frameCounter = 0;
        flickerState = false;
    }

    public static void UpdateFlickerVisual(
    ref float flickerTimer,
    ref int frameCounter,
    ref bool flickerState,
    ref bool isFlickering,
    Image innerImage,
    Color flickerOn,
    Color flickerOff,
    int framesPerToggle,
    float flickerDuration,
    Action onFlickerEnd = null)
    {
        flickerTimer += Time.deltaTime;
        frameCounter++;

        if (frameCounter >= framesPerToggle)
        {
            frameCounter = 0;
            flickerState = !flickerState;

            if (innerImage)
                innerImage.color = flickerState ? flickerOn : flickerOff;
        }

        if (flickerTimer >= flickerDuration)
        {
            StopFlicker(
                ref isFlickering,
                ref flickerState,
                ref frameCounter,
                innerImage,
                flickerOff,
                onFlickerEnd);
        }
    }

    public static void StopFlicker(
        ref bool isFlickering,
        ref bool flickerState,
        ref int frameCounter,
        Image innerImage,
        Color flickerOff,
        Action onFlickerEnd = null)
    {
        isFlickering = false;
        flickerState = false;
        frameCounter = 0;

        if (innerImage != null)
            innerImage.color = flickerOff;

            onFlickerEnd?.Invoke();
        }
    }
}