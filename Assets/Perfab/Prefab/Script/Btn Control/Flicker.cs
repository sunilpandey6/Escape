using System;
using UnityEngine;
using UnityEngine.UI;

public static class Flicker
{
    //
    public static void FlickToggle(Image A, Color flick, Color idle,
                                ref bool state)
    {
        state = !state;
        if (A != null)
        {
            A.color = state ? flick : idle;
        }

    }

}
