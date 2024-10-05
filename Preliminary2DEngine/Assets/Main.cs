using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Mathematics.math;

/// <summary>
/// NOTES FOR THE 3D VERSION
///    
/// doubles are more precise than floats, so probably use them instead, especially with things as small as what we are using
/// 
/// 
/// </summary>

public class Main : MonoBehaviour {

    // The number of particles
    public int numberOfParticles;

    // The gameObject that they want to use as the sprite
    public GameObject particlePicture;

    // TODO Change this to a List instead of an array
    // The list of particles to use
    private particle[] particles;


    // A bool to make sure the mouse isn't held down
    bool down = false;



    // Start is called before the first frame update
    void Start() {

        // Making a list of particles to use
        particles = new particle[numberOfParticles];

        // Looping to instantiate all of the particles
        for (int i = 0; i < numberOfParticles; i++) {

            // Making another circle and using its transform for our particle
            particles[i] = new particle(Instantiate(particlePicture).transform, 0.1f, 0.1f, new Vector2(Random.Range(-9f, 9f), Random.Range(-4f, 4f)), new Vector2(Random.Range(-10f, 10f), Random.Range(-10f, 10f)));

            // Giving each a different force
            particles[i].force(new Vector2(i * 2, i));

            // Making it so that each collision isn't elastic   
            particles[i].restitution = 0.75f;

        }
      
    }

    // Update is called once per frame
    void Update() {

        // Updating the location of each one in the particles thing
        for (int i = 0; i < particles.Length; i++) {
            particles[i].update();
        }

        // TODO Adjust this to use multithreading
        // Checking to see if they are colliding
        particle.fixCollisions(particles);

        // Adding gravity
        for (int i = 0; i < particles.Length; i++) {
            particle.addGravity(particles[i], particles);
        }

        // Making a force towards the mouse
        if (Input.GetButtonDown("Fire1") && !down) {

            // Toggling the boolean
            down = true;
            
            // The position of the mouse
            Vector2 mousePos = Input.mousePosition;

            for (int i = 0; i < numberOfParticles; i++) {

                // We just want to add together the mouse position and the position of the particle, then normalize it 
                // so that it doesn't have a crazy amount of force
                Vector3 updatedMousePos = (Camera.main.ScreenToWorldPoint(mousePos) - new Vector3(particles[i].shownTransform.position.x, particles[i].shownTransform.position.y, 0)).normalized * 500;

                // Applying the force
                particles[i].force(updatedMousePos);

            }

        } else {
            down = false;
        }

        // Making sure that the number of particles is not negative
        numberOfParticles = Mathf.Max(numberOfParticles, 0);
        
        // Checking to see if we need to get rid of some circles or add more
        if (numberOfParticles > particles.Length) {

            // The new array
            particle[] newParticles = new particle[numberOfParticles];

            // Duplicating the particles array into a bigger array
            System.Array.Copy(particles, newParticles, particles.Length);

            // Looping to add particles
            for (int i = particles.Length; i < newParticles.Length; i++) {

                // Making another circle and using its transform for our particle
                newParticles[i] = new particle(Instantiate(particlePicture).transform, 0.1f, 0.1f, new Vector2(Random.Range(-9f, 9f), Random.Range(-4f, 4f)), new Vector2(Random.Range(-10f, 10f), Random.Range(-10f, 10f)));

                // Giving each a different force
                newParticles[i].force(new Vector2(i * 2, i));

                // Making it so that each collision isn't elastic   
                newParticles[i].restitution = 0.75f;

            }

            // Resetting the particles to particles
            particles = newParticles;

        } else if (numberOfParticles < particles.Length) {

            Debug.Log("Removing...");

            // The new length (to ensure nothing weird happens if it goes fast)
            int newLength = numberOfParticles;

            // Destroying the extra particles
            for (int i = particles.Length - 1; i > newLength; i--) {
                Debug.Log("Looping...");

                // Check for null before trying to destroy
                if (particles[i] != null && particles[i].shownTransform != null && particles[i].shownTransform.parent != null) {
                Debug.Log($"Destroying particle at index {i}: {particles[i].shownTransform.parent.gameObject.name}");
                Destroy(particles[i].shownTransform.parent.gameObject);
                particles[i] = null;
                }

            }

            // Create a new smaller array to hold the remaining particles
            particle[] newParticles = new particle[newLength];

            // Duplicating the particles array into a smaller array
            System.Array.Copy(particles, newParticles, newLength);

            // Saving the new array
            particles = newParticles;

        }


    }
}


// The class that I'm going to have for my particle
public class particle {

    // IVs
    public float mass;
    public float radius;
    public Vector2 velocity;
    public Vector2 position;
    public Vector2 accel;

    // Here is where we get into the weeds of the weirder stuff each particle has

    // This is the coefficient of restitution. Basically, it is a bounciness percentage
    public float restitution;

    // The coefficient of drag. This helps figure out air resistance
    public float dragCoeff; 
 
    // This is the transform of the particle object that it is referencing
    public UnityEngine.Transform shownTransform;


    // Constructors

    // Default - sets everything to a default value of a hydrogen atom
    public particle(UnityEngine.Transform _transform) {
        
        // Setting the defaults for the intrinsic properties
        this.mass = (float)6.6464731 * pow(10,-24);
        this.radius = (float)3.1 * pow(10, -11);

        // Setting the positional things to zero
        this.velocity = Vector2.zero;
        this.position = _transform.position;

        // Adding in the transform of the particle
        this.shownTransform = _transform;

        // Defaulting things 
        this.restitution = 1;
        this.dragCoeff = 0.47f; // 0.47 for a sphere according to wikipedia

        // Adjusting the scale
        this.shownTransform.localScale = new Vector2(this.radius*2, this.radius*2); 


    }

    // Parameterized constructor
    public particle(UnityEngine.Transform _transform, float mass, float radius) {
        this.mass = mass;
        this.radius = radius;
        this.shownTransform = _transform;
        this.shownTransform.position = _transform.position;

        // Defaulting things 
        this.restitution = 1;
        this.dragCoeff = 0.47f; // 0.47 for a sphere according to wikipedia

        // Adjusting the scale
        this.shownTransform.localScale = new Vector2(this.radius*2, this.radius*2);
    }

    // Locationalized parameterized constructor (see? If I use big words I sound smart)
    public particle(UnityEngine.Transform _transform, float mass, float radius, Vector2 position, Vector2 velocity) {
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

    // A method that applies a force to the particle
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
            this.velocity.y = -this.velocity.y * this.restitution;

            // Checking to make sure the GROUND DOESN'T EAT IT
            if (this.shownTransform.position.y - this.radius <= -Constants.stageDimensions.y) {
                this.shownTransform.position = new Vector3(this.shownTransform.position.x, -Constants.stageDimensions.y + this.radius, 0);
            }

            
        }

        // Checking the top of the screen
        if (this.shownTransform.position.y + this.radius > Constants.stageDimensions.y) {

            // Reversing the velocity in the y direction
            this.velocity.y = -this.velocity.y * this.restitution;

            // Checking to make sure the SKY DOESN'T EAT IT
            if (this.shownTransform.position.y + this.radius >= Constants.stageDimensions.y) {
                this.shownTransform.position = new Vector3(this.shownTransform.position.x, Constants.stageDimensions.y - this.radius, 0);
            }

        }

        // Checking the right of the screen
        if (this.shownTransform.position.x + this.radius > Constants.stageDimensions.x) {

            // Reversing the velocity in the y direction
            this.velocity.x = -this.velocity.x * this.restitution;

            // Checking to make sure the WALL DOESN'T EAT IT
            if (this.shownTransform.position.x + this.radius >= Constants.stageDimensions.x) {
                this.shownTransform.position = new Vector3(Constants.stageDimensions.x - this.radius, this.shownTransform.position.y, 0);
            }

        }

        // Checking the left of the screen
        if (this.shownTransform.position.x - this.radius < -Constants.stageDimensions.x) {

            // Reversing the velocity in the y direction
            this.velocity.x = -this.velocity.x * this.restitution;

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

    // ! Deprecated! 
    // Collision Detection 
    public static void fixCollisions(particle[] particles){

        // Looping for all of them to see which ones are close
        for (int i = 0; i < particles.Length; i++) {

            // Start j from i + 1 to avoid redundant checks
            for (int j = i + 1; j < particles.Length; j++) {

                // Making sure that it doesn't try to check with itself
                if (i == j) continue;
            
                // We don't want to do square root because it is very slow, so we are 
                // going to check to see if the distance between them is less than the
                // (2 * radius) squared 
                float distSquared = (particles[i].shownTransform.position - particles[j].shownTransform.position).sqrMagnitude;

                if (distSquared <= (particles[i].radius + particles[j].radius) * (particles[i].radius + particles[j].radius)) {

                    // First off, finding the normal
                    Vector3 normal3 = particles[i].shownTransform.position - particles[j].shownTransform.position;
                    Vector2 normal = new Vector2(normal3.x, normal3.y).normalized;

                    // Finding the relative velocity
                    Vector2 relativeVelocity = particles[i].velocity - particles[j].velocity;

                    // Skipping if they are moving apart 
                    if (Vector2.Dot(relativeVelocity, normal) >= 0) continue;

                    // Finding the coefficient of restitution that we will use
                    // The minimum because the one with the least restitution will deform slightly
                    // not allowing the other to break contact
                    float restitution = Mathf.Min(particles[j].restitution, particles[i].restitution);

                    // Finding the impulse, J = change in momentum
                    float impulseScalar = -(1 + restitution) * Vector2.Dot(relativeVelocity, normal);
                    impulseScalar /= (1 / particles[i].mass + 1 / particles[j].mass);

                    // Apply impulse to the particles
                    Vector2 impulse = impulseScalar * normal;
                    particles[i].velocity += impulse / particles[i].mass;
                    particles[j].velocity -= impulse / particles[j].mass;

                    // TODO See if you can get rid of this square root
                    // Finding out how much we need to move it (annoyingly this means we need the square root)
                    float overlapDepth = particles[i].radius + particles[j].radius - Mathf.Sqrt(distSquared);

                    // Finding the correction vector and accounting for the mass of each to make it a bit smoother
                    Vector2 correction = normal * Mathf.Max(overlapDepth, 0)      / (particles[i].mass + particles[j].mass);

                    // Moving the first one
                    particles[i].shownTransform.position -= new Vector3(correction.x, correction.y, 0) * particles[j].mass;

                    // Moving the second one
                    particles[j].shownTransform.position += new Vector3(correction.x, correction.y, 0) * particles[i].mass;

                }

            }
        }

    } 

    // Gravity :D
    public static void addGravity(particle focus, particle[] particles) {

        // Summing the forces 
        Vector2 f = Vector2.zero;

        // Looping through each of the other particles and adding their gravity to the focus
        for (int i = 0; i < particles.Length; i++) {

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

}

