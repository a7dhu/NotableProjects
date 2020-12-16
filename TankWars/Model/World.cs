// Author : Alexsis Lever & Dayi Hu

// Change Log : Version 1.1
// Last Update : 11/14/2019 - Implemented this entire class under the design
//                            choice that the model will contain no controller
//                            aspects.

// Change Log : Version 1.2
// Last Update : 11/17/2019 - Added playerID as a member field.

// Change Log : Version 1.3
// Last Update : 11/25/2019 - Added int array for explosions.

// Change Log : Version 1.4
// Last Update : 12/1/2019 - Added dictionary for participant IDs.

// Change Log : Version 1.5
// Last Update : 12/5/2019 - Added number of power ups member field.


using System;
using System.Collections.Generic;
using System.Text;

namespace Model
{
    public class World
    {
        private Dictionary<int, Tank> tanks;
        private Dictionary<int, Projectile> projectiles;
        private Dictionary<int, Beam> beams;
        private Dictionary<int, Powerup> powerups;
        private Dictionary<int, Wall> walls;
        private Dictionary<string, int> participantIDs;
        private int[] explosions;
        private int size;
        private int playerID;
        private int numPowerUps = 0;
        private int maxDelay = 0;

        /// <summary>
        /// Constructor that creates a basic container for all of the participants in the world
        /// via lists by participants.
        /// </summary>
        public World()
        {
            // Set up the participant dictionaries
            tanks = new Dictionary<int, Tank>();
            projectiles = new Dictionary<int, Projectile>();
            beams = new Dictionary<int, Beam>();
            powerups = new Dictionary<int, Powerup>();
            walls = new Dictionary<int, Wall>();
            participantIDs = new Dictionary<string, int>();
            size = 0;

            // Set up the dictionary for participant IDs - Server specific
            participantIDs.Add("Walls", 0);
            participantIDs.Add("Tanks", 0);
            participantIDs.Add("Projectiles", 0);
            participantIDs.Add("Beams", 0);
            participantIDs.Add("Powerups", 0);
        }

        /// <summary>
        /// Allows other methods to access and modify the set 
        /// of tank participants in the world.
        /// </summary>
        public Dictionary<int, Tank> Tanks
        {
            get { return tanks; }
            set { tanks = value; }
        }

        /// <summary>
        /// Allows other methods to access and modify the set 
        /// of projectile participants in the world.
        /// </summary>
        public Dictionary<int, Projectile> Projectiles
        {
            get { return projectiles; }
            set { projectiles = value; }
        }

        /// <summary>
        /// Allows other methods to access and modify the set 
        /// of beam participants in the world.
        /// </summary>
        public Dictionary<int, Beam> Beams
        {
            get { return beams; }
            set { beams = value; }
        }

        /// <summary>
        /// Allows other methods to access and modify the set 
        /// of powerup participants in the world.
        /// </summary>
        public Dictionary<int, Powerup> Powerups
        {
            get { return powerups; }
            set { powerups = value; }
        }

        /// <summary>
        /// Allows other methods to access and modify the set 
        /// of wall participants in the world.
        /// </summary>
        public Dictionary<int, Wall> Walls
        {
            get { return walls; }
            set { walls = value; }
        }

        /// <summary>
        /// Allows other methods to access and modify the size
        /// of the world.
        /// </summary>
        public int Size
        {
            get { return size; }
            set { size = value; }
        }

        /// <summary>
        /// Allows other methods to access and modify the player ID
        /// of the world.
        /// </summary>
        public int PlayerID
        {
            get { return playerID; }
            set { playerID = value; }
        }

        /// <summary>
        /// Allows other methods to access and modify the explosions
        /// of the world.
        /// </summary>
        public int[] Explosion
        {
            get { return explosions; }
            set { explosions = value; }
        }

        /// <summary>
        /// Allows other methods to access and modify the participant
        /// IDs of the world. Specific to the Server.
        /// </summary>
        public Dictionary<string, int> ParticipantIDs
        {
            get { return participantIDs; }
            set { participantIDs = value; }
        }

        /// <summary>
        /// Allows other methods to access and modify the number of powerups
        /// in the world.
        /// </summary>
        public int NumPowerUps
        {
            get { return numPowerUps; }
            set { numPowerUps = value; }
        }

        /// <summary>
        /// Allows other methods to access and modify the time between powerups
        /// in the world.
        /// </summary>
        public int MaxDelay
        {
            get { return maxDelay; }
            set { maxDelay = value; }
        }
    }
}
