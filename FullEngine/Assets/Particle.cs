using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Mathematics.math;
using Unity.Mathematics;


// A very simple class extension to Vector3's to make them easy to convert to double3's
public static class VectorExtensions { 
    public static double3 ToDouble3(this Vector3 vector) {
        return new double3(vector.x, vector.y, vector.z);
    }
    public static Vector3 ToVector3(this double3 vector) {
        return new Vector3((float)vector.x, (float)vector.y, (float)vector.z); 
    }
}

public class Particle {

    //-------------------------------Instance Variables---------------------------------
    
    //-------------------------------Public---------------------------------
    
    // The color of the particle (tho this might not get used)
    private Color color; 

    //-------------------------------Private---------------------------------

    //-------------------------------Physical Attributes---------------------------------

    // The mass of the particle 
    private double mass = 1;

    // The radius of the particle (particles are, right now, only circular; I might make a superclass later)
    private double radius = 0.1;

    // The acceleration
    private double3 acceleration = new double3(0, 0, 0);

    // The coefficient of drag of the particle, so how much air wants it to not move
    private double dragCoefficient = 0.47;

    // The coefficient of restitution, so the bounciness of the particle
    private double restitution = 0.95;

    // I am going to treat the particles as springs when correcting their collisions, so we are going to give every particle a spring constant
    // I think of this as in how much the particle doesn't want to compress in on itself
    private double springConstant = 1000; 

    //-------------------------------In Code Variables---------------------------------

    // The way that we will actually change what happens to the particle
    public UnityEngine.Transform transform;
    
    // The velocity of the particle (public so that the grid can edit it)
    public double3 velocity = new double3(0, 0, 0);

    // These are the bounds that the particle will not be able to escape
    private (int, int) xBound = (-1, 1);
    private (int, int) zBound = (-1, 1);
    private (int, int) yBound = (-1, 1);

    
    //-------------------------------Constructors---------------------------------
    
    // Default constructor
    public Particle(UnityEngine.Transform transform) {
        this.transform = transform;
        this.defaultify();
    }

    // No position or velocity constructor
    public Particle(UnityEngine.Transform transform, double mass, double radius) {
        this.transform = transform;
        this.mass = mass;
        this.radius = radius;
        this.defaultify();
    }
    
    // The basic physical properties
    public Particle(UnityEngine.Transform transform, double mass, double radius, double3 position, double3 velocity) {
        this.transform = transform;
        this.mass = mass;
        this.radius = radius;
        this.transform.position = position.ToVector3();
        this.velocity = velocity;
        this.defaultify();
    }

    // Basic physical properties but with vectors as inputs
    public Particle(UnityEngine.Transform transform, double mass, double radius, Vector3 position, Vector3 velocity) {
        this.transform = transform;
        this.mass = mass;
        this.radius = radius;
        this.transform.position = position;
        this.velocity = velocity.ToDouble3();
        this.defaultify();
    }


    // This is a helped method to the constructors. It will get called by basically all 
    // of the constructors and will simply assign default values to any values that are still null
    // TODO update this so that you can actually set these values to zero too   
    // Update this to be just their initial values
    private void defaultify() {

        this.transform.localScale = new Vector3((float)this.radius * 2f,(float)this.radius * 2f,(float)this.radius * 2f);

    }

    //-------------------------------Methods---------------------------------

    //-------------------------------Public---------------------------------  

    // Updating the particle to change the location based on the velocity
    public void update() {

        // Adding in gravity
        this.force(Constants.g * this.mass);

        // Adjusting the velocity using ✨kinematics✨
        this.velocity += this.acceleration * Time.deltaTime;

        // Clearing the acceleration so that it all doesn't go haywire
        this.acceleration = new double3(0);

        // Adjusting the actual position based on this 
        this.transform.position += this.velocity.ToVector3() * Time.deltaTime;

        // Checking to make sure we haven't exited the box we are allowed to be in
        this.checkBoundaries();

    } 

    // A function that applies a force in a specific direction
    public void force(double3 f) {

        // Adding the drag force to the force f
        // Drag Force = mass * DragCoeff. * (AirDensity)/2 * velocity^2 * area 
        f += this.mass * this.dragCoefficient * Constants.AIR_DENSITY / 2 * this.velocity.ToVector3().sqrMagnitude * PI * -this.velocity.ToVector3().normalized.ToDouble3() * this.radius * this.radius;

        // Adding in the acceleration using F=ma
        this.acceleration += f / this.mass;


    }

    // Various getters for the grid class so that it can do the collision detection
    public double getMass() {
        return this.mass;
    }    
    public double getRadius() {
        return this.radius;
    }
    public double getSpringConstant() {
        return this.springConstant;
    }
    public double getRestitution() {
        return this.restitution;
    }



    //-------------------------------Private---------------------------------

    // Checking to make sure that the particle hasn't escaped
    private void checkBoundaries() {

        // Now we just check each and reverse the right things when needed

        // Y (the ground) 
        if (this.transform.position.y < yBound.Item1 + this.radius) {
            this.velocity.y = -this.velocity.y;
            this.transform.position = new Vector3(this.transform.position.x, yBound.Item1 + (float)this.radius, this.transform.position.z);
        }
        if (this.transform.position.y > yBound.Item2 - this.radius) {
            this.velocity.y = -this.velocity.y;
            this.transform.position = new Vector3(this.transform.position.x, yBound.Item2 - (float)this.radius,  this.transform.position.z);
        }

        // X
        if (this.transform.position.x < xBound.Item1 + this.radius) {
            this.velocity.x = -this.velocity.x;
            this.transform.position = new Vector3(xBound.Item1 + (float)this.radius, this.transform.position.y, this.transform.position.z);
        }
        if (this.transform.position.x > xBound.Item2 - this.radius) {
            this.velocity.x = -this.velocity.x;
            this.transform.position = new Vector3(xBound.Item2 - (float)this.radius, this.transform.position.y, this.transform.position.z);
        }

        // y
        if (this.transform.position.z < zBound.Item1 + this.radius) {
            this.velocity.z = -this.velocity.z;
            this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, zBound.Item1 + (float)this.radius);
        }
        if (this.transform.position.z > zBound.Item2 - this.radius) {
            this.velocity.z = -this.velocity.z;
            this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, zBound.Item2 - (float)this.radius);
        }

        // Checking to see if any of the position values are NaN or Infinity
        if (double.IsNaN(this.transform.position.x) || double.IsPositiveInfinity(this.transform.position.x) || double.IsNegativeInfinity(this.transform.position.x) || double.IsNaN(this.transform.position.y) || double.IsPositiveInfinity(this.transform.position.y) || double.IsNegativeInfinity(this.transform.position.y) || double.IsNaN(this.transform.position.z) || double.IsPositiveInfinity(this.transform.position.z) || double.IsNegativeInfinity(this.transform.position.z)) {
            this.transform.position = new Vector3(0f, 0f, 0f);
            Debug.Log("There was a wild value that I fixed.");
        }

    }

    

 



}