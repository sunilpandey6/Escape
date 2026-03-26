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

        // Toggle flicker state
        if (frameCounter >= framesPerToggle)
        {
            frameCounter = 0;
            flickerState = !flickerState;

            if (innerImage)
                innerImage.color = flickerState ? flickerOn : flickerOff;
        }

        // End flicker
        if (flickerTimer >= flickerDuration)
        {
            isFlickering = false;
            flickerState = false;
            frameCounter = 0;

            if (innerImage)
                innerImage.color = flickerOff;

            onFlickerEnd?.Invoke();
        }
    }
}