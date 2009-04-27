
// ----------------------------------------------------------------------------
//
// Ported to Unity and .Net by Ricardo J. Méndez http://www.arges-systems.com/
//
// OpenSteer -- Steering Behaviors for Autonomous Characters
//
// Copyright (c) 2002-2003, Sony Computer Entertainment America
// Original author: Craig Reynolds <craig_reynolds@playstation.sony.com>
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
//
//
// ----------------------------------------------------------------------------
//
//
// OpenSteer Boids
// 
// 09-26-02 cwr: created 
//
//
// ----------------------------------------------------------------------------


using UnityEngine;
using System.Collections;
using OpenSteer;

namespace OpenSteer.Vehicles {

    public class Boid : SimpleVehicle
    {
        // constructor
        public Boid (AbstractProximityDatabase pd)
        {
            // allocate a token for this boid in the proximity database
            proximityToken = null;
            newPD (pd);

            // reset all boid state
            reset ();
        }


        // destructor
        ~Boid ()
        {
            // delete this boid's token in the proximity database
            proximityToken = null; // TODO-CHECKs
        }


        // reset state
        void reset ()
        {
            // reset the vehicle
            base.reset();

            // steering force is clipped to this magnitude
            setMaxForce (27);

            // velocity is clipped to this magnitude
            setMaxSpeed (9);

            // initial slow speed
            setSpeed (maxSpeed() * 0.3f);

            // randomize initial orientation
            regenerateOrthonormalBasisUF (Random.insideUnitCircle);

            // randomize initial position
            setPosition (Random.insideUnitSphere * 20);

            // notify proximity database that our position has changed
            proximityToken.updateForNewPosition(Position);
        }


        /* TODO-REMOVE
        // draw this boid into the scene
        void draw ()
        {
            drawBasic3dSphericalVehicle (*this, gGray70);
            // drawTrail ();
        }
        */


        // per frame simulation update
        public void update (float currentTime, float elapsedTime)
        {
            // steer to flock and perhaps to stay within the spherical boundary
            applySteeringForce (steerToFlock () + handleBoundary(), elapsedTime);

            // notify proximity database that our position has changed
            proximityToken.updateForNewPosition (Position);
        }


        // basic flocking
        Vector3 steerToFlock ()
        {
            float separationRadius =  5.0f;
            float separationAngle  = -0.707f;
            float separationWeight =  12.0f;

            float alignmentRadius = 7.5f;
            float alignmentAngle  = 0.7f;
            float alignmentWeight = 8.0f;

            float cohesionRadius = 9.0f;
            float cohesionAngle  = -0.15f;
            float cohesionWeight = 8.0f;

            float maxRadius = Mathf.Max(separationRadius,
                                        Mathf.Max(alignmentRadius,
                                                  cohesionRadius));

            // find all flockmates within maxRadius using proximity database
            neighbors.Clear();
            proximityToken.findNeighbors (Position, maxRadius, neighbors);

            // determine each of the three component behaviors of flocking
            Vector3 separation = steerForSeparation (separationRadius,
                                                     separationAngle,
                                                     neighbors);
            Vector3 alignment  = steerForAlignment  (alignmentRadius,
                                                     alignmentAngle,
                                                     neighbors);
            Vector3 cohesion   = steerForCohesion   (cohesionRadius,
                                                     cohesionAngle,
                                                     neighbors);

            // apply weights to components (save in variables for annotation)
            Vector3 separationW = separation * separationWeight;
            Vector3 alignmentW = alignment * alignmentWeight;
            Vector3 cohesionW = cohesion * cohesionWeight;

            // annotation
            // const float s = 0.1;
            // annotationLine (position, position + (separationW * s), gRed);
            // annotationLine (position, position + (alignmentW  * s), gOrange);
            // annotationLine (position, position + (cohesionW   * s), gYellow);

            return separationW + alignmentW + cohesionW;
        }


        // Take action to stay within sphereical boundary.  Returns steering
        // value (which is normally zero) and may take other side-effecting
        // actions such as kinematically changing the Boid's position.
        Vector3 handleBoundary ()
        {
            // while inside the sphere do noting
            if (Position.magnitude < worldRadius) return Vector3.zero;

            // once outside, select strategy
            switch (boundaryCondition)
            {
                case 0:
                {
                    // steer back when outside
                    Vector3 seek = xxxsteerForSeek (Vector3.zero);
                    // Replacing ...
                    // Vector3 lateral = seek.perpendicularComponent (forward ());
                    // with ...
                    float projection = Vector3.Dot(seek, forward());
                    Vector3 parallel = forward() * projection;
                    Vector3 lateral  =  seek - parallel;
                    // End replace. See Vec3.h for details
                    return lateral;
                }
                case 1:
                {
                    // wrap around (teleport)
                    /* TODO-CHECK
                    setPosition (Position.sphericalWrapAround (Vector3.zero,
                                                                 worldRadius));
                    */
                    setPosition(Position);
                    return Vector3.zero;
                }
            }
            return Vector3.zero; // should not reach here
        }


        // make boids "bank" as they fly
        void regenerateLocalSpace (Vector3 newVelocity,
                                   float elapsedTime)
        {
            regenerateLocalSpaceForBanking (newVelocity, elapsedTime);
        }

        // switch to new proximity database -- just for demo purposes
        void newPD (AbstractProximityDatabase pd)
        {
            // allocate a token for this boid in the proximity database
            proximityToken = pd.allocateToken (this);
        }


        // cycle through various boundary conditions
        static void nextBoundaryCondition ()
        {
            const int max = 2;
            boundaryCondition = (boundaryCondition + 1) % max;
        }
        static int boundaryCondition;

        // a pointer to this boid's interface object for the proximity database
        AbstractTokenForProximityDatabase proximityToken;

        // allocate one and share amoung instances just to save memory usage
        // (change to per-instance allocation to be more MP-safe)
        static ArrayList neighbors = new ArrayList();

        static float worldRadius;
    };
}
// ----------------------------------------------------------------------------