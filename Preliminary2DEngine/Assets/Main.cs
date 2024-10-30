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
    public GameObject ParticlePicture;

    // The list of particles to use
    // private Particle[] particles; // Left in comments in case I want to change back later for speed
    private List<Particle> particles;


    // A bool to make sure the mouse isn't held down
    bool down = false;



    // Start is called before the first frame update
    void Start() {

        // Making a list of particles to use
        // particles = new Particle[numberOfParticles];
        particles = new List<Particle>(numberOfParticles);

        // Looping to instantiate all of the particles
        for (int i = 0; i < numberOfParticles; i++) {

            // Making another circle and using its transform for our Particle
            // particles[i] = new Particle(Instantiate(ParticlePicture).transform, 0.1f, 0.1f, new Vector2(Random.Range(-9f, 9f), Random.Range(-4f, 4f)), new Vector2(Random.Range(-10f, 10f), Random.Range(-10f, 10f)));
            particles.Add(new Particle(Instantiate(ParticlePicture).transform, 0.1f, 0.1f, new Vector2(Random.Range(-9f, 9f), Random.Range(-4f, 4f)), new Vector2(Random.Range(-10f, 10f), Random.Range(-10f, 10f))));
            // addParticle(i);
            


        }
      
    }

    // Update is called once per frame (not good for physics)
    void Update() {


        // Making a force towards the mouse
        if (Input.GetButtonDown("Fire1") && !down) {

            // Toggling the boolean
            down = true;
            
            // The position of the mouse
            Vector2 mousePos = Input.mousePosition;

            for (int i = 0; i < numberOfParticles; i++) {

                // We just want to add together the mouse position and the position of the Particle, then normalize it 
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
        if (numberOfParticles > particles.Count) {

            // Looping to add particles
            for (int i = particles.Count; i < numberOfParticles; i++) {

                // Making another circle and using its transform for our Particle
                addParticle(i);

            }

        } else if (numberOfParticles < particles.Count) {

            // The new length (to ensure nothing weird happens if it goes fast)
            int newLength = numberOfParticles;

            // Destroying the extra particles
            for (int i = particles.Count - 1; i > newLength; i--) {


                particles[i].destroy(particles);

            }


        }

    }



    // FixedUpdate is called 50 times a secind, so it's fixed (good for physics)
    void FixedUpdate() {

        // Updating the location of each one in the particles thing
        for (int i = 0; i < particles.Count; i++) {
            particles[i].update();
        }

        // In the future, I might adjust this to use multithreading
        // But right now I don't think I will
        // Checking to see if they are colliding
        Particle.fixCollisions(particles);

        // Adding gravity
        for (int i = 0; i < particles.Count; i++) {
            Particle.addGravity(particles[i], particles);
        }

    }

    // A basic add particle method that adds a basic particle at a random point
    void addParticle(int index) {
        bool positionIsValid = false;
        Vector2 newPosition = Vector2.zero;

        while (!positionIsValid) {
            positionIsValid = true;
            newPosition = new Vector2(Random.Range(-9f, 9f), Random.Range(-4f, 4f));

            foreach (var particle in particles) {
                if (Vector3.Distance(particle.shownTransform.position, newPosition) < 0.1 + particle.radius) {
                    positionIsValid = false;
                    break;
                }
            }
        }

        Particle newParticle = new Particle(Instantiate(ParticlePicture).transform, 0.1f, 0.1f, newPosition, new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)));
        newParticle.force(new Vector2(index * 2, index));
        newParticle.restitution = 0.75f;

        particles.Add(newParticle);
    }

    
}

