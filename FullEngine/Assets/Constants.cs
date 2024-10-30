using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Mathematics.math;
using Unity.Mathematics;

public static class Constants {

    // Yay, the gravitational constant of the earth
    public static readonly double3 g = new double3(0, -9.807, 0);

    // Yay, the fundamental gravitational constant
    public static readonly double G = 6.67430 * pow(10, -11);

    // The air density (kg/m^3)
    public static readonly double AIR_DENSITY = 1.225;

    // The friction between the different balls
    // This will have to be an instance variable before long
    public static readonly double PARTICLE_FRICTION = 0.95;

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