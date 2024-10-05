using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public static class CollisionHandler {
    public static void fixCollisions(particle[] particles) {

        List<(int index, Vector3 position)> corrections = new List<(int, Vector3)>();

        int totalParticles = particles.Length;
        Parallel.For(0, totalParticles, i => {

            // Start j from i + 1 to avoid redundant checks
            for (int j = i + 1; j < totalParticles; j++) {

                // Making sure that it doesn't try to check with itself
                if (i == j) continue;

                float distSquared = (particles[i].shownTransform.position - particles[j].shownTransform.position).sqrMagnitude;

                if (distSquared <= (particles[i].radius + particles[j].radius) * (particles[i].radius + particles[j].radius)) {
                    // Normal calculation and other collision logic...
                    Vector3 normal3 = particles[i].shownTransform.position - particles[j].shownTransform.position;
                    Vector2 normal = new Vector2(normal3.x, normal3.y).normalized;
                    Vector2 relativeVelocity = particles[i].velocity - particles[j].velocity;

                    if (Vector2.Dot(relativeVelocity, normal) >= 0) continue;

                    float restitution = Mathf.Min(particles[j].restitution, particles[i].restitution);
                    float impulseScalar = -(1 + restitution) * Vector2.Dot(relativeVelocity, normal);
                    impulseScalar /= (1 / particles[i].mass + 1 / particles[j].mass);
                    Vector2 impulse = impulseScalar * normal;

                    // Collecting corrections for the main thread
                    corrections.Add((i, particles[i].shownTransform.position - new Vector3(impulse.x / particles[i].mass, impulse.y / particles[i].mass, 0)));
                    corrections.Add((j, particles[j].shownTransform.position + new Vector3(impulse.x / particles[j].mass, impulse.y / particles[j].mass, 0)));
                }
            }
        });

        // Apply corrections back to the particles on the main thread
        foreach (var correction in corrections)
        {
            particles[correction.index].shownTransform.position = correction.position;
        }
    }
}
