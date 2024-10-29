using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Mathematics.math;


// The class that I'm going to have for my Particle
public class Particle {

    // IVs
    public float mass;
    public float radius;
    public Vector2 velocity;
    public Vector2 position;
    public Vector2 accel;

    // Just a visual thing: color
    public Color color;

    // Here is where we get into the weeds of the weirder stuff each Particle has

    // This is the coefficient of restitution. Basically, it is a bounciness percentage
    public float restitution;

    // This is the spring constant for the balls. It is basically how springy it is 
    public float springConstant;

    // The coefficient of drag. This helps figure out air resistance
    public float dragCoeff; 
 
    // This is the transform of the Particle object that it is referencing
    public UnityEngine.Transform shownTransform;

    // These are just variables that represent the adjusted position and velocity of the particles when 
    // collision correction happens
    // Private so that I don't accidentally access them outside this class
    private Vector2 placeHolderVelocity;
    private Vector2 placeHolderPosition;
    private float placeHolderMass; // This one is a bit weird and only here because of the specific collision correction 
                                   // method I am using


    // Constructors

    // TODO update the constructors to have a function that will fill in the rest of the stuff with default values if they are null

    // Default - sets everything to a default value of a hydrogen atom
    public Particle(UnityEngine.Transform _transform) {
        
        // Setting the defaults for the intrinsic properties
        this.mass = (float)6.6464731 * pow(10,-24);
        this.radius = (float)3.1 * pow(10, -11);

        // Setting the positional things to zero
        this.velocity = Vector2.zero;
        this.position = _transform.position;

        // Adding in the transform of the Particle
        this.shownTransform = _transform;

        // Defaulting things 
        this.restitution = 1;
        this.dragCoeff = 0.47f; // 0.47 for a sphere according to wikipedia

        // Adjusting the scale
        this.shownTransform.localScale = new Vector2(this.radius*2, this.radius*2); 

        // Arbitrarily painting this particle white
        color = Color.white;

        // Setting up the placeholder
        this.placeHolderPosition = this.position;
        this.placeHolderVelocity = this.velocity;
        this.placeHolderMass = this.mass;

        this.springConstant = 270;

    }

    // Parameterized constructor
    public Particle(UnityEngine.Transform _transform, float mass, float radius) {
        this.mass = mass;
        this.radius = radius;
        this.shownTransform = _transform;
        this.shownTransform.position = _transform.position;

        // Defaulting things 
        this.restitution = 1;
        this.dragCoeff = 0.47f; // 0.47 for a sphere according to wikipedia

        // Adjusting the scale
        this.shownTransform.localScale = new Vector2(this.radius*2, this.radius*2);

        // Arbitrarily painting this particle white
        color = Color.white;

        // Setting up the placeholder
        this.placeHolderPosition = this.position;
        this.placeHolderVelocity = this.velocity;
        this.placeHolderMass = this.mass;

        this.springConstant = 270;

    }
     // Locationalized parameterized constructor (see? If I use big words I sound smart)
    public Particle(UnityEngine.Transform _transform, float mass, float radius, Vector2 position, Vector2 velocity) {
        this.mass = mass;
        this.radius = radius;
        this.velocity = velocity;
        this.shownTransform = _transform;
        this.shownTransform.position = position;

        // Defaulting things 
        this.restitution = 1;
        this.dragCoeff = 0.47f; // 0.47 for a sphere according to wikipedia

        // Adjusting the scale
        this.shownTransform.localScale = new Vector2(this.radius*2, this.radius*2);

        // Arbitrarily painting this particle white
        color = Color.white;

        // Setting up the placeholder
        this.placeHolderPosition = this.position;
        this.placeHolderVelocity = this.velocity;
        this.placeHolderMass = this.mass;

        this.springConstant = 270; 

    }

    // Methods

    // An update method that changes the location based on the velocity at that time
    public void update() {

        // Adding to the velocity based on the acceleration
        // Using vf = v0 + a*t
        this.velocity = this.velocity + this.accel * Time.deltaTime;

        // Resetting the acceleration to gravity
        this.accel = Vector2.zero;
        this.force(Constants.g * this.mass);

        // Updating the location adjusted for the change in time
        this.shownTransform.position += new Vector3(this.velocity.x * Time.deltaTime, this.velocity.y * Time.deltaTime, 0);

        // Checking the bounce to make sure it isn't going off the screen
        this.checkWalls();

    }

    // A method that applies a force to the Particle
    public void force(Vector2 f) {

        // We actually have to account for air resistance here
        // We need to subtract the drag force from the inputted force.
        // There comes a force when the drag force is higher than the inputted force
        // Which means it is either at terminal velocity or higher
        // Drag Force = mass * DragCoeff. * (AirDensity)/2 * velocity^2 * area 
        // These are all spheres, so they would be pi * r^2 
        // Then we have to normalize it to the opposite direction of the velocity
        Vector2 dragForce = this.mass * this.dragCoeff * Constants.AIR_DENSITY / 2 * this.velocity.magnitude * this.velocity.magnitude * this.radius * this.radius * PI * -this.velocity.normalized;  

        // Using F=ma, we can find the acceleration due to the force
        this.accel += (f + dragForce)  * (float)(1.0 / this.mass);

    }

    // A method that checks for bounces on the edge of the screen
    public void checkWalls() {

        // Checking the bottom of the screen
        if (this.shownTransform.position.y - this.radius< -Constants.stageDimensions.y) {

            // Reversing the velocity in the y direction
            this.velocity.y = -this.velocity.y * this.restitution * Constants.PARTICLE_FRICTION;

            // Checking to make sure the GROUND DOESN'T EAT IT
            if (this.shownTransform.position.y - this.radius <= -Constants.stageDimensions.y) {
                this.shownTransform.position = new Vector3(this.shownTransform.position.x, -Constants.stageDimensions.y + this.radius, 0);
            }

            
        }

        // Checking the top of the screen
        if (this.shownTransform.position.y + this.radius > Constants.stageDimensions.y) {

            // Reversing the velocity in the y direction
            this.velocity.y = -this.velocity.y * this.restitution * Constants.PARTICLE_FRICTION;

            // Checking to make sure the SKY DOESN'T EAT IT
            if (this.shownTransform.position.y + this.radius >= Constants.stageDimensions.y) {
                this.shownTransform.position = new Vector3(this.shownTransform.position.x, Constants.stageDimensions.y - this.radius, 0);
            }

        }

        // Checking the right of the screen
        if (this.shownTransform.position.x + this.radius > Constants.stageDimensions.x) {

            // Reversing the velocity in the y direction
            this.velocity.x = -this.velocity.x * this.restitution * Constants.PARTICLE_FRICTION;

            // Checking to make sure the WALL DOESN'T EAT IT
            if (this.shownTransform.position.x + this.radius >= Constants.stageDimensions.x) {
                this.shownTransform.position = new Vector3(Constants.stageDimensions.x - this.radius, this.shownTransform.position.y, 0);
            }

        }

        // Checking the left of the screen
        if (this.shownTransform.position.x - this.radius < -Constants.stageDimensions.x) {

            // Reversing the velocity in the y direction
            this.velocity.x = -this.velocity.x * this.restitution * Constants.PARTICLE_FRICTION;

            // Checking to make sure the WALL DOESN'T EAT IT
            if (this.shownTransform.position.x - this.radius <= -Constants.stageDimensions.x) {
                this.shownTransform.position = new Vector3(-Constants.stageDimensions.x + this.radius, this.shownTransform.position.y, 0);
            }

        }

    }

    // A method to return the velocity as a 3D vector instead of a 2D vector
    public Vector3 velocity3D() {

        return new Vector3(this.velocity.x, this.velocity.y, 0);

    }

    // ! WHAT IF I ADJUSTED THIS TO JUST ADD FORCE TO THE PARTICLES SO THAT THE GRAVITY IS COUNTERACTED

    // TODO I need to fix this a bit to ensure that it accounts for multiple different collisions at the same time
    // Collision Detection 
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
                float distSquared = (particles[i].shownTransform.position - particles[j].shownTransform.position).sqrMagnitude;

                if (distSquared <= (particles[i].radius + particles[j].radius) * (particles[i].radius + particles[j].radius)) {

                    // Fixing the velocities
                    Particle.fixVelocityForCollision(particles[i], particles[j]);

                    // TODO See if you can get rid of this square root
                    Particle.fixPositionForCollision(particles[i], particles[j]);

                }

            }

        }

    } 

    public static void fixVelocityForCollision(Particle one, Particle two) {

        // First off, finding the normal
        Vector3 normal3 = one.shownTransform.position - two.shownTransform.position;
        Vector2 normal = new Vector2(normal3.x, normal3.y).normalized;

        // Finding the relative velocity
        Vector2 relativeVelocity = one.velocity - two.velocity;

        // Skipping if they are moving apart 
        if (Vector2.Dot(relativeVelocity, normal) >= 0) return;

        // Finding the coefficient of restitution that we will use
        // The minimum because the one with the least restitution will deform slightly
        // not allowing the other to break contact
        float restitution = Mathf.Min(two.restitution, one.restitution);

        // Finding the impulse, J = change in momentum
        float impulseScalar = -(1 + restitution) * Vector2.Dot(relativeVelocity, normal);
        impulseScalar /= (1 / one.mass + 1 / two.mass);

        // Apply impulse to the particles
        Vector2 impulse = impulseScalar * normal;
        // one.velocity += impulse / one.mass;
        // two.velocity -= impulse / two.mass;

        // I'm thinking that we should treat the particles as springs to find the time that they 
        // apply force to each other, that way I can find the instantaneous force and use the add force 
        // function. Theoretically, then it will treat it as a normal force that directly cancels out gravity

        // Spring force = -k * x   
        // k is the spring constant, and something that we will probably derive from the CoeffOfRest or 
        // add as an instance variable
        // Finding out how much we need to move it  // ! If we end up using this we need to make the distance in input to the function so that we aren't doing extra square roots
        float dist = (one.shownTransform.position - two.shownTransform.position).magnitude;
        float overlapDepth = one.radius + two.radius - dist;
        Vector2 springForce = 1/(1/(one.springConstant) + 1/(two.springConstant)) * overlapDepth * normal* 0.98f;
        // (it is the same for the second one, just negative)

        // Let's just try to apply this force for now. I might need to end up really increasing the spring constant for each tho
        one.force(springForce);
        two.force(-springForce);         


    }

    // A function to fix the position of the particles when there is a collision
    public static void fixPositionForCollision(Particle one, Particle two) {

        // First off, finding the normal
        Vector3 normal3 = one.shownTransform.position - two.shownTransform.position;
        Vector2 normal = new Vector2(normal3.x, normal3.y).normalized;


        // Finding the distance between them (annoyingly this means a costly square root)
        float dist = (one.shownTransform.position - two.shownTransform.position).magnitude;

        // Finding out how much we need to move it 
        float overlapDepth = one.radius + two.radius - dist;

        // Finding the correction vector and accounting for the mass of each to make it a bit smoother
        Vector2 correction = normal * Mathf.Max(overlapDepth, 0) / (one.mass + two.mass);


        /*
        While I could directly update the position, that can cause some jittering and stuff
        so I am going to lerp to the new position
        */
    
        // Moving the first one
        one.shownTransform.position += new Vector3(correction.x, correction.y, 0) * two.mass * Constants.PARTICLE_FRICTION;

        // Moving the second one
        two.shownTransform.position -= new Vector3(correction.x, correction.y, 0) * one.mass * Constants.PARTICLE_FRICTION;

    }


    // Gravity :D
    public static void addGravity(Particle focus, List<Particle> particles) {

        // Summing the forces 
        Vector2 f = Vector2.zero;

        // Looping through each of the other particles and adding their gravity to the focus
        for (int i = 0; i < particles.Count; i++) {

            // The force of gravity due to each one is 
            // F = G * M1 * M2 / r^2

            // First off, finding the normal
            Vector3 normal = focus.shownTransform.position - particles[i].shownTransform.position;

            // A reference to the normal as a vector2
            Vector2 flatNormal = new Vector2(normal.x, normal.y);

            // Actually calculating the force
            // Making sure that there is no divide by zero
            float dist = Vector2.Distance(focus.shownTransform.position, particles[i].shownTransform.position);
            if (dist != 0) {
                Vector2 forceG = -Constants.G * flatNormal.normalized * focus.mass * particles[i].mass / pow(dist, 2);

                // Adding the force to the total force
                f += forceG;
            }

        }

        // Applying the force
        focus.force(f);

    }

    // A function to destroy itself and it's parent
    public void destroy(List<Particle> particles) {
        GameObject temp = this.shownTransform.parent.gameObject;
        this.shownTransform = null;
        particles.Remove(this);
        if (Application.isPlaying) {
            Object.Destroy(temp);
        } else {
            Object.DestroyImmediate(temp);
        }

    }
    

}

public class ParticleDestroyer : MonoBehaviour {
    public static ParticleDestroyer Instance;

    private void Awake() {
        Instance = this;
    }

    public void DestroyParticle(Particle particle, List<Particle> particles) {

        // Making sure they aren't trying to destroy what has already been destroyed
        if (particle == null) return;
        GameObject temp = particle.shownTransform.parent.gameObject;
        particles.Remove(particle);
        Destroy(temp);
    }
}


// DEPRECATED Because I thought of a better implementation
// This is just a simple struct to use as a placeholder for particles so that I can adjust some stuff without changing the overall structure of things
// struct PlaceHolder {

//     // These are just the different members of the particle that we need to adjust
//     public Vector3 position;
//     public Vector2 velocity;
//     public float radius;

//     // This is the index of the particle in the array 
//     public int index;


//     // The constructor 
//     public Book(int _index, Vector2 _position, Vector2 _velocity, float _radius) {
//         this.index = _index;
//         this.position = _position;
//         this.velocity = _velocity;
//         this.radius = _radius;
//     }

// }