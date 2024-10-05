using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Mathematics.math;

public static class Constants {


    // Yay, the gravitational constant of the earth
    public static readonly Vector2 g = Vector2.zero;//new Vector2(0, -9.807f);

    // Yay, the fundamental gravitational constant
    public static readonly float G = 6.67430f * pow(10, -11);

    // The air density (kg/m^3)
    public static readonly float AIR_DENSITY = 1.225f;

    // The size of the screen
    // (0, 0) is in the middle of the screen
    private static Vector2? _stageDimensions; 
    // A method to initialize it when we need it
    public static Vector2 stageDimensions {
        get {
            if (!_stageDimensions.HasValue) {
                _stageDimensions = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
            }
            return _stageDimensions.Value;
        }
    }
    
    


}