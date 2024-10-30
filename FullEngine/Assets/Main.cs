using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Mathematics.math;



public class Main : MonoBehaviour { 
    

    //-------------------------------Instance Variables---------------------------------
    
    //-------------------------------Public---------------------------------

    // The number of particles that we will have
    public int numberOfParticles;

    // How the particles will each look
    public GameObject particleSprite;

    //-------------------------------Private---------------------------------

    // The list in which we will be holding all of our particles
    private List<Particle> particles;

    void Start() { 

        // Setting up the list of particles
        particles = new List<Particle>(numberOfParticles);
        for (int i = 0; i < numberOfParticles; i++) {
            particles.Add(new Particle(Instantiate(particleSprite).transform, 1, 0.1, new Vector3(Random.Range(-9f, 9f), Random.Range(-4f, 4f), Random.Range(-0f, 4f)), new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f))));
        } 
    }

    // Update is called once per frame
    void Update() {

        for (int i = 0; i < particles.Count; i++) {
            particles[i].update();
            Particle.fixCollisions(particles);
        }
        
    }
}
