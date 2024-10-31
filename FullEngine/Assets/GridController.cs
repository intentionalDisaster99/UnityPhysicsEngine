using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Mathematics.math;
using Unity.Mathematics;

/// <summary>
/// This is basically just a grid class that I can use to separate the different sections of the world so that 
/// I can more quickly check to see if they might touch 
/// 
/// This might also make it easier to do multiprocessing, as I can just check each cell in parallel 
/// </summary>


public class Grid {

    //-------------------------------Instance Variables---------------------------------

    // The list that I am going to use for each cell
    private List<Particle>[,,] grid;
    
    // The physical sizes of the grid (like the length from top to bottom, left to right, and back to front)
    private double3 physicalSize;

    // The number of cells in each direction
    private Vector3Int numberOfCells;

    // A possible offset, tho I doubt that I will actually use it
    private double3 offset;

    // A public number of particles inside this grid system
    public int Count = 0;

    // A linked list of all of the particles that we have so that we can iterate through them faster
    public LinkedList<Particle> runningList = new LinkedList<Particle>();

    //-------------------------------Constructors---------------------------------

    // The main constructor, which ignores the offset assuming that it is at zero
    public Grid(double3 physicalSize, Vector3Int cellNumber) {

        this.physicalSize = physicalSize;
        this.numberOfCells = cellNumber;
        
        // Setting the offset to the default: zero
        this.offset = new double3(0, 0, 0); 

        // Setting up the list of particles
        this.grid = new List<Particle>[this.numberOfCells.x, this.numberOfCells.y, this.numberOfCells.z];

        // Making sure there are no null lists
        for (int x = 0; x < this.numberOfCells.x; x++) {
            for (int y = 0; y < this.numberOfCells.y; y++) {
                for (int z = 0; z < this.numberOfCells.z; z++) {
                    this.grid[x, y, z] = new List<Particle>();
                }
            }
        }
    }

    // The constructor that uses an offset
    public Grid(double3 physicalSize, Vector3Int cellNumber, double3 offset) {

        this.physicalSize = physicalSize;
        this.numberOfCells = cellNumber;
        this.offset = offset; 

        // Setting up the list of particles
        this.grid = new List<Particle>[this.numberOfCells.x, this.numberOfCells.y, this.numberOfCells.z];

        // Making sure there are no null lists
        for (int x = 0; x < this.numberOfCells.x; x++) {
            for (int y = 0; y < this.numberOfCells.y; y++) {
                for (int z = 0; z < this.numberOfCells.z; z++) {
                    this.grid[x, y, z] = new List<Particle>();
                }
            }
        }

    }

    //-------------------------------Methods---------------------------------

    //-------------------------------Private Methods---------------------------------


    // The main method to add things to the list 
    public void addParticle(Particle particle) {

        // Getting the place where we need to put the particle
        Vector3Int index = this.getParticleIndex(particle);

        // Putting the particle in the right place
        this.grid[index.x, index.y, index.z].Add(particle);

        // Increasing the count 
        this.Count++;

        // Adding this particle to the running list 
        this.runningList.AddLast(particle);

    } 

    // A method that will check to see if the particles are in the right place
    // TODO With the new linked list, I can more quickly just clear the grid and then add in the ones I have saved in the running list
    public void fixLocations() {



        // // Creating a new list where we can put everything in its right place
        // List<Particle>[,,] temp = new List<Particle>[this.numberOfCells.x, this.numberOfCells.y, this.numberOfCells.z];

        // // Making sure there are no null lists
        // for (int x = 0; x < this.numberOfCells.x; x++) {
        //     for (int y = 0; y < this.numberOfCells.y; y++) {
        //         for (int z = 0; z < this.numberOfCells.z; z++) {
        //             temp[x, y, z] = new List<Particle>();
        //         }
        //     }
        // }

        // Clearing the list 
        this.grid = new List<Particle>[this.numberOfCells.x, this.numberOfCells.y, this.numberOfCells.z];

        // Initializing the new grid lists 
        for (int x = 0; x < this.numberOfCells.x; x++) {

            for (int y = 0; y < this.numberOfCells.y; y++) {

                for (int z = 0; z < this.numberOfCells.z; z++) {

                    this.grid[x, y, z] = new List<Particle>();

                }

            }

        }

        // Pre-allocating the space for the indices
        Vector3Int indices = new Vector3Int(0, 0, 0);

        // Looping for every particle in the array
        var node = runningList.First;
        while (node != null)
        {

            // Getting the indices of the particle
            indices = this.getParticleIndex(node.Value);

            // Pushing that particle into the right place
            this.grid[indices.x, indices.y, indices.z].Add(node.Value);

            // Going to the next particle
            node = node.Next;
        }

    }

    // A simple method to find the indices of a particle in this grid
    public Vector3Int getParticleIndex(Particle particle) {

        // Figuring out where I need to place this particle
        int x = (int)math.floor((particle.transform.position.x - this.offset.x) / this.physicalSize.x * this.numberOfCells.x);
        int y = (int)math.floor((particle.transform.position.y - this.offset.y) / this.physicalSize.y * this.numberOfCells.y);
        int z = (int)math.floor((particle.transform.position.z - this.offset.z) / this.physicalSize.z * this.numberOfCells.z);

        // Ensure coordinates are within bounds
        x = math.clamp(x, 0, this.numberOfCells.x - 1);
        y = math.clamp(y, 0, this.numberOfCells.y - 1);
        z = math.clamp(z, 0, this.numberOfCells.z - 1);

        return new Vector3Int(x, y, z);

    }


    public void fixCollisions(){

        // Looping through all of the cells
        // ! Here is where we should add multi-threading so that we can check different cells at the same time
        for (int x = 0; x < this.numberOfCells.x; x++) {
            for (int y = 0; y < this.numberOfCells.y; y++) {
                for (int z = 0; z < this.numberOfCells.z; z++) {

                    // NOTE: If this is acting too weirdly then we can make it also check the cells next to it for 
                    //       more accuracy. I think that is not worth it at this point tho because we would get 
                    //       strangeness with multithreading 
                    // Looping through the particles in the cell
                    for (int i = 0; i < this.grid[x, y, z].Count; i++) {

                        for (int j = i+1; j < this.grid[x, y, z].Count; j++) {  

                            // This is a backup to make sure that it doesn't try to collide with itself
                            // We shouldn't need it tho because of the way that I am looping through
                            // if (i == j) continue;

                            // The particles that we are using
                            Particle one = this.grid[x, y, z][i];
                            Particle two = this.grid[x, y, z][j];
                        
                            // We don't want to do square root because it is very slow, so we are 
                            // going to check to see if the distance between them is less than the
                            // (2 * radius) squared 
                            double distSquared = (one.transform.position - two.transform.position).sqrMagnitude;

                            if (distSquared <= (one.getRadius() + two.getRadius()) * (one.getRadius() + two.getRadius())) {

                                // Getting the distance here so that I don't have to find it twice
                                double dist = System.Math.Sqrt(distSquared);

                                // Fixing the velocities by using a spring force
                                Grid.fixVelocityForCollision(one, two, dist);

                                // Fixing the positions of the particles so that they aren't clashing anymore
                                Grid.fixPositionForCollision(one, two, dist);

                            
                            }

                        }

                    }

                }

            }

        }

    }

    // A method that will update every single particle in this grid
    public void updateAll() {

        // There are easier ways to do this, but I want speed
        var node = runningList.First;
        while (node != null) {
            node.Value.update();
            node = node.Next;
        }

    }
    
    //-------------------------------Private Methods---------------------------------

    // Helper function for fixing the collisions
    private static void fixVelocityForCollision(Particle one, Particle two, double dist) {

        // First off, finding the normal
        double3 normal = (one.transform.position - two.transform.position).ToDouble3();

        // Finding the relative velocity
        double3 relativeVelocity = one.velocity - two.velocity;

        // Finding the coefficient of restitution that we will use
        // The minimum because the one with the least restitution will deform slightly
        // not allowing the other to break contact
        double restitution = two.getRestitution() > one.getRestitution() ? one.getRestitution() : two.getRestitution();

        // Finding the impulse, J = change in momentum
        double impulseScalar = -(0 + restitution) * Vector3.Dot(relativeVelocity.ToVector3(), normal.ToVector3());
        impulseScalar /= (0 / one.getMass() + 1 / two.getMass());

        // Apply impulse to the particles
        double3 impulse = impulseScalar * normal;

        // I'm thinking that we should treat the particles as springs to find the time that they 
        // apply force to each other, that way I can find the instantaneous force and use the add force 
        // function. Theoretically, then it will treat it as a normal force that directly cancels out gravity

        // Spring force = -k * x   
        // k is the spring constant, and something that we will probably derive from the CoeffOfRest or 
        // add as an instance variable
        // Finding out how much we need to move it 
        double overlapDepth = one.getRadius() + two.getRadius() - dist;
        double3 springForce = 1/(1/(one.getSpringConstant()) + 1/(two.getSpringConstant())) * overlapDepth * normal * 0.98f;
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
        double overlapDepth = one.getRadius() + two.getRadius() - dist;

        // Finding the correction vector and accounting for the mass of each to make it a bit smoother
        double3 correction = normal * (overlapDepth > 0 ? 0 : overlapDepth) / (one.getMass() + two.getMass());


        /*
        While I could directly update the position, that can cause some jittering and stuff
        so I am going to lerp to the new position
        */
    
        // Moving the first one
        one.transform.position += (correction * two.getMass()).ToVector3();
        
        // Moving the second one
        two.transform.position -= (correction * one.getMass()).ToVector3();

    }




} 