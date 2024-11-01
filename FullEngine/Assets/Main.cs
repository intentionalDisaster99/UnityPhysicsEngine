using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Mathematics.math;
using Unity.Mathematics;



public class Main : MonoBehaviour { 
    

    //-------------------------------Instance Variables---------------------------------
    
    //-------------------------------Public---------------------------------

    // The number of particles that we will have
    public int numberOfParticles;

    // How the particles will each look
    public GameObject particleSprite;

    //-------------------------------Private---------------------------------

    // The list in which we will be holding all of our particles
    private Grid particles;

    void Start() { 

        // Setting up the list of particles
        particles = new Grid(new double3(2, 2, 2), new Vector3Int(2, 2, 2));
        for (int i = 0; i < numberOfParticles; i++) {
            particles.addParticle(new Particle(Instantiate(particleSprite).transform, 1, 0.1, new Vector3(UnityEngine.Random.Range(-9f, 9f), UnityEngine.Random.Range(-4f, 4f), UnityEngine.Random.Range(-0f, 4f)), new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f))));
        } 

    }

    // Update is called once per frame
    void Update() {

        particles.updateAll();
        // TODO update the fixLocations thing to be called by each particle during its update call 
        // particles.fixLocations();
        particles.fixCollisions();
        


    }


}
