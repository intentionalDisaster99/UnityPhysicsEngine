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
            particles[i] = new particle(Instantiate(particlePicture).transform, 0.1f, 0.1f, new Vector2(Random.Range(-9f, 9f), 0), Vector2.zero);

            // Giving each a different force
            particles[i].force(new Vector2(i * 2, i));

            particles[i].restitution = 0.75f;

        }
      
    }

    // Update is called once per frame
    void Update() {

        // Updating the location of each one in the particles thing
        for (int i = 0; i < numberOfParticles; i++) {
            particles[i].updateLocation();
        }

        // Checking to see if they are colliding
        particle.fixCollisions(particles);

        // Making a force towards the mouse
        if (Input.GetButtonDown("Fire1") && !down) {

            // Toggling the boolean
            down = true;
            
            // The position of the mouse
            Vector2 mousePos = Input.mousePosition;

            for (int i = 0; i < numberOfParticles; i++) {

                // We just want to add together the mouse position and the position of the particle, then normalize it 
                // so that it doesn't have a crazy amount of force
                Vector3 updatedMousePos = (Camera.main.ScreenToWorldPoint(mousePos) - new Vector3(particles[i].shownTransform.position.x, particles[i].shownTransform.position.y, 0)).normalized * 100;

                // Applying the force
                particles[i].force(updatedMousePos);

            }

        } else {
            down = false;
        }
        

    }
}


// The class that I'm going to have for my particle
class particle {

    // IVs
    public float mass;
    public float radius;
    public Vector2 velocity;
    public Vector2 position;
    public Vector2 accel;

    // Here is where we get into the weeds of the weirder stuff each particle has

    // This is the coefficient of restitution. Basically, it is a bounciness percentage
    public float restitution;
 
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

        // Adjusting the scale
        this.shownTransform.localScale = new Vector2(this.radius*2, this.radius*2);
    }

    // Methods

    // An update method that changes the location based on the velocity at that time
    public void updateLocation() {

        // Adding to the velocity based on the acceleration
        // Using vf = v0 + a*t
        this.velocity = this.velocity + this.accel * Time.deltaTime;

        // Resetting the acceleration now that we have accounted for it
        this.accel = Constants.g;

        // Updating the location adjusted for the change in time
        this.shownTransform.position += new Vector3(this.velocity.x * Time.deltaTime, this.velocity.y * Time.deltaTime, 0);

        // Checking the bounce to make sure it isn't going off the screen
        this.checkBounces();

    }

    // A method that applies a force to the particle
    public void force(Vector2 f) {

        // Using F=ma, we can find the acceleration due to the force
        this.accel += f  * (float)(1.0 / this.mass);

    }

    // A method that checks for bounces on the edge of the screen
    public void checkBounces() {

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

    // Collision Detection
    public static void fixCollisions(particle[] particles){

        // Looping for all of them to see which ones are close
        for (int i = 0; i < particles.Length; i++) {

            for (int j = 0; j < particles.Length; j++) {

                // Making sure that it doesn't try to check with itself
                if (i == j) continue;
            
                // We don't want to do square root because it is very slow, so we are 
                // going to check to see if the distance between them is less than the
                // (2 * radius) squared 
                float dist = Vector3.Distance(particles[i].shownTransform.position, particles[j].shownTransform.position);
                // pow((particles[i].shownTransform.position.x - particles[j].shownTransform.position.x), 2) + pow((particles[j].shownTransform.position.x - particles[j].shownTransform.position.y), 2);

                if (dist <= (particles[i].radius + particles[i].radius)) {

                    // Now we have to solve the issue.
                    // So, to solve them, I think I need to find the line that connects the centers
                    // of the circle and then fix them by reflecting their velocities across the line. 

                    // Then I just need to use conservation of momentum to figure out the velocity. But then also impulse 👍
                    // Shouldn't be too hard, right?


                    // Right????

                    // First off, finding the normal
                    Vector3 normal = particles[i].shownTransform.position - particles[j].shownTransform.position;

                    // A reference to the normal as a vector2
                    Vector2 flatNormal = new Vector2(normal.x, normal.y);

                    if (Vector2.Dot(particles[i].velocity - particles[j].velocity, flatNormal) > 0) continue; // Skip if they are moving apart

                    // I need to use the reflection formula for both to find the new velocities
                    // new = vector - 2 * (vector dot normal) * normal
                    particles[i].velocity = particles[i].velocity - 2 * (Vector2.Dot(particles[i].velocity, flatNormal.normalized)) * flatNormal.normalized;
                    particles[j].velocity = particles[j].velocity - 2 * (Vector2.Dot(particles[j].velocity, flatNormal.normalized)) * flatNormal.normalized;

                    // Finding the coefficient of restitution that we will use
                    // The minimum because the one with the least restitution will deform slightly
                    // not allowing the other to break contact
                    float restitution = Mathf.Min(particles[j].restitution, particles[i].restitution);

                    // Finding the impulse, J = change in momentum
                    float impulseScalar = -(1 + restitution) * Vector3.Dot(particles[i].velocity - particles[j].velocity, normal);;
                    impulseScalar /= 1 / particles[i].mass + 1 / particles[j].mass;

                    // Apply impulse to the particles
                    Vector3 impulse = impulseScalar * normal;
                    Vector2 flatImpulse = new Vector2(impulse.x, impulse.y);
                    particles[i].velocity -= flatImpulse / particles[i].mass;
                    particles[j].velocity += flatImpulse / particles[j].mass;

                    // Now to also move them out of each other
                    // It is just along the line that they collided

                    // Moving the first one
                    particles[i].shownTransform.position = particles[i].shownTransform.position - normal.normalized * (normal.magnitude - particles[i].radius - particles[j].radius);

                    // Moving the second one
                    particles[j].shownTransform.position = particles[j].shownTransform.position + normal.normalized * (normal.magnitude - particles[j].radius - particles[i].radius);

                }

            }
        }

    } 

}

