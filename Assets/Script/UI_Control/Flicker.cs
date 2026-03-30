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

<<<<<<< HEAD
    public static void FlickerMain(
=======
    public static void UpdateFlickerVisual(
>>>>>>> 78df3413f0ac3f0ef42fdf8a6aa56f4fe2c093b2
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

<<<<<<< HEAD
        // Handle the visual toggle logic
=======
        // Toggle flicker state
>>>>>>> 78df3413f0ac3f0ef42fdf8a6aa56f4fe2c093b2
        if (frameCounter >= framesPerToggle)
        {
            frameCounter = 0;
            flickerState = !flickerState;

            if (innerImage)
                innerImage.color = flickerState ? flickerOn : flickerOff;
        }

<<<<<<< HEAD
        // Handle the completion logic
        if (flickerTimer >= flickerDuration)
        {
            StopFlicker(ref isFlickering, ref flickerState, ref frameCounter, innerImage, flickerOff, onFlickerEnd);
        }
    }

    private static void StopFlicker(
        ref bool isFlickering,
        ref bool flickerState,
        ref int frameCounter,
        Image innerImage,
        Color flickerOff,
        Action onFlickerEnd)
    {
        isFlickering = false;
        flickerState = false;
        frameCounter = 0;

        if (innerImage)
            innerImage.color = flickerOff;

        onFlickerEnd?.Invoke();
    }
=======
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
>>>>>>> 78df3413f0ac3f0ef42fdf8a6aa56f4fe2c093b2
}