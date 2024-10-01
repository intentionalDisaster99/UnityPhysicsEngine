using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Mathematics.math;

public static class Constants {


    // Yay, the gravitational constant of the earth
    public static readonly Vector2 g = new Vector2(0, -9.807f);

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