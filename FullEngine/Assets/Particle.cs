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
    private double mass;

    // The radius of the particle (particles are, right now, only circular; I might make a superclass later)
    private double radius;

    // The velocity of the particle
    private double3 velocity;

    // The acceleration
    private double3 acceleration;

    // The coefficient of drag of the particle, so how much air wants it to not move
    private double dragCoefficient;

    // The coefficient of restitution, so the bounciness of the particle
    private double restitution;

    // I am going to treat the particles as springs when correcting their collisions, so we are going to give every particle a spring constant
    // I think of this as in how much the particle doesn't want to compress in on itself
    private double springConstant; 

    //-------------------------------In Code Variables---------------------------------

    // The way that we will actually change what happens to the particle
    public UnityEngine.Transform transform;

    
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

        

        this.mass = this.mass == 0.0 ? 1 : this.mass;
        this.radius = this.radius == 0.0 ? 1 : this.radius;
        this.velocity = this.velocity.Equals(default(double3)) ? new double3(0) : this.velocity;
        this.acceleration = this.acceleration.Equals(default(double3)) ? new double3(0) : this.acceleration;
        this.dragCoefficient = this.dragCoefficient == 0.0 ? 0.47 : this.dragCoefficient;
        this.springConstant = this.springConstant == 0.0 ? 500 : this.springConstant;
        this.restitution = this.restitution == 0.0 ? 0.95 : this.restitution;

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

    // Collision Detection and correction 
    public static void fixCollisions(List<Particle> particles){

        // Looping for all of them to see which ones are close
        for (int i = 0; i < particles.Count; i++) {

            // Start j from i + 1 to avoid redundant checks
            for (int j = i + 1; j < particles.Count; j++) {

                // Making sure that it doesn't try to check with itself
                if (i == j) continue;
            
                // We don't want to do square root because it is very slow, so we are 
                // going to check to see if the distance between them is less than the
                // (2 * radius) squared 
                double distSquared = (particles[i].transform.position - particles[j].transform.position).sqrMagnitude;

                if (distSquared <= (particles[i].radius + particles[j].radius) * (particles[i].radius + particles[j].radius)) {

                    // Getting the distance here so that I don't have to find it twice
                    double dist = System.Math.Sqrt(distSquared);

                    // Fixing the velocities
                    Particle.fixVelocityForCollision(particles[i], particles[j], dist);

                    // TODO See if you can get rid of this square root
                    Particle.fixPositionForCollision(particles[i], particles[j], dist);

                }

            }

        }

    } 

    //-------------------------------Private---------------------------------

    // Checking to make sure that the particle hasn't escaped
    private void checkBoundaries() {

        // Here is where I declare the outside boundaries, I might make these editable in real time later
        var xBound = (-1, 1);
        var zBound = (-1, 1);
        float floor = -1f;  

        // Now we just check each and reverse the right things when needed

        // Y (the ground) 
        if (this.transform.position.y < floor + this.radius) {
            this.velocity.y = -this.velocity.y;
            this.transform.position = new Vector3(this.transform.position.x, floor + (float)this.radius, this.transform.position.z);
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
        // if (this.transform.position.x == Infinity || this.transform.position.y == Infinity || this.transform.position.z == Infinity ) {
        //     this.transform.position = new Vector3(0f, 0f, 0f);
        // }


    }

    // Helper function for fixing the collisions
    private static void fixVelocityForCollision(Particle one, Particle two, double dist) {

        // First off, finding the normal
        double3 normal = (one.transform.position - two.transform.position).ToDouble3();

        // Finding the relative velocity
        double3 relativeVelocity = one.velocity - two.velocity;

        // Finding the coefficient of restitution that we will use
        // The minimum because the one with the least restitution will deform slightly
        // not allowing the other to break contact
        double restitution = two.restitution > one.restitution ? one.restitution : two.restitution;

        // Finding the impulse, J = change in momentum
        double impulseScalar = -(1 + restitution) * Vector2.Dot(relativeVelocity.ToVector3(), normal.ToVector3());
        impulseScalar /= (1 / one.mass + 1 / two.mass);

        // Apply impulse to the particles
        double3 impulse = impulseScalar * normal;

        // I'm thinking that we should treat the particles as springs to find the time that they 
        // apply force to each other, that way I can find the instantaneous force and use the add force 
        // function. Theoretically, then it will treat it as a normal force that directly cancels out gravity

        // Spring force = -k * x   
        // k is the spring constant, and something that we will probably derive from the CoeffOfRest or 
        // add as an instance variable
        // Finding out how much we need to move it 
        double overlapDepth = one.radius + two.radius - dist;
        double3 springForce = 1/(1/(one.springConstant) + 1/(two.springConstant)) * overlapDepth * normal * 0.98f;
        // (it is the same for the second one, just negative)

        // Let's just try to apply this force for now. I might need to end up really increasing the spring constant for each tho
        one.force(springForce);
        two.force(-springForce);         


    }

    // A function to fix the position of the particles when there is a collision
    private static void fixPositionForCollision(Particle one, Particle two, double dist) {

        // First off, finding the normal
        double3 normal = one.transform.position.ToDouble3() - two.transform.position.ToDouble3();

        // Finding out how much we need to move it 
        double overlapDepth = one.radius + two.radius - dist;

        // Finding the correction vector and accounting for the mass of each to make it a bit smoother
        double3 correction = normal * (overlapDepth > 0 ? 0 : overlapDepth) / (one.mass + two.mass);


        /*
        While I could directly update the position, that can cause some jittering and stuff
        so I am going to lerp to the new position
        */
    
        // Moving the first one
        one.transform.position += (correction * two.mass).ToVector3();
        
        // Moving the second one
        two.transform.position -= (correction * one.mass).ToVector3();

    }


 



}