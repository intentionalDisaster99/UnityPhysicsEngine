using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public static class CollisionHandler {
    public static int fixCollisions(List<Particle> particles) {

        // Recording the total number of particles in case it changes
        int totalParticles = particles.Count;

        List<(int index, Vector3 posCorrection, Vector2 velCorrection)> corrections = new List<(int, Vector3, Vector2)>();
        Vector3[] positions = new Vector3[totalParticles];
        Vector2[] velocities = new Vector2[totalParticles];

        // Collecting positions and velocities before starting the parallel loop
        for (int i = 0; i < totalParticles; i++)
        {
            positions[i] = particles[i].shownTransform.position;
            velocities[i] = particles[i].velocity;
        }

        Parallel.For(0, totalParticles, i => {
            for (int j = i + 1; j < totalParticles; j++) {

                // Skipping if it is the same particle
                if (i == j) continue;

                // Finding the squared distance (to skip out on the costly square roots)
                float distSquared = (positions[i] - positions[j]).sqrMagnitude;

                // Checking to see if we actually need to check for the collision
                if (distSquared <= (particles[i].radius + particles[j].radius) * (particles[i].radius + particles[j].radius)) {

                    // Finding the normal and relative velocity
                    Vector3 normal3 = positions[i] - positions[j];
                    Vector2 normal = new Vector2(normal3.x, normal3.y).normalized;
                    Vector2 relativeVelocity = velocities[i] - velocities[j];

                    // Finding the overlap depth (annoyingly we have to use the sqrt here)
                    float overlapDepth = particles[i].radius + particles[j].radius - Mathf.Sqrt(distSquared);

                    // Finding the correction vector and accounting for the mass of each to make it a bit smoother
                    Vector2 correction = normal * Mathf.Max(overlapDepth, 0f) / (particles[i].mass + particles[j].mass);

                    // Making sure that they aren't going away from each other already, in which case we're good on velocity
                    Vector2 impulse;
                    if (Vector2.Dot(relativeVelocity, normal) < 0) {

                        // Finding the impulse for later
                        float restitution = Mathf.Min(particles[j].restitution, particles[i].restitution);
                        float impulseScalar = -(1 + restitution) * Vector2.Dot(relativeVelocity, normal);
                        impulseScalar /= (1 / particles[i].mass + 1 / particles[j].mass);
                        impulse = impulseScalar * normal;

                    } else {impulse = Vector2.zero;};

                    // TODO fix the stuff here - there is some stuff do to with gravity pulling the particles into each other and also trying to still adjust the position if the particles are not moving in the same direction. This is where the index out of bounds or whatever it was is coming from
                    
                    // Saving the corrections for later so we can add them in the same thread
                    corrections.Add((j, new Vector3(-correction.x, -correction.y, 0) * particles[i].mass, -impulse / particles[j].mass));
                    corrections.Add((i, new Vector3(correction.x, correction.y, 0) * particles[j].mass, impulse / particles[i].mass));

                }
            }
        });

        foreach (var correction in corrections)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                particles[correction.index].shownTransform.position += correction.posCorrection;
                particles[correction.index].velocity += correction.velCorrection;
            });
        }

        // Telling them that it's done
        return 1;

    }
}
