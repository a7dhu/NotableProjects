// Author : Alexsis Lever & Dayi Hu

// Change Log : Version 1.1
// Last Update : 11/12/2019 - Implemented this entire class under the design
//                            choice that the model will contain no controller
//                            aspects.

// Change Log : Version 1.2
// Last Update : 11/16/2019 - Updated the constructor to parse a JSON string
//                            relative to taking in all properties as parameters.

// Change Log : Version 1.4
// Last Update : 11/22/2019 - Added a color property.

// Change Log : Version 1.5
// Last Update : 12/03/2019 - Made Getter/Setters ignored by JSON so they are
//                            not included in serialization.


using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TankWars;
using Newtonsoft.Json.Linq;

namespace Model
{
    public class Projectile
    {
        // Represents the projectile's unique ID
        [JsonProperty(PropertyName = "proj")]
        private int projectileID;

        // Represents the projectile's location
        [JsonProperty(PropertyName = "loc")]
        private Vector2D location;

        // Represents the direction of the projectile
        [JsonProperty(PropertyName = "dir")]
        private Vector2D direction;

        // Represents whether or not the projectile hit a wall or left the bounds of the world
        [JsonProperty(PropertyName = "died")]
        private bool disappeared;

        // Represents the projectile's owner
        [JsonProperty(PropertyName = "owner")]
        private int tankID;

        // Stores the color of a tank for beyond 8 players
        private int color = 8;


        /// <summary>
        /// Constructor that determines the fields of the projectile, namely
        /// the ID, location, direction, disappeared, and the owner.
        /// </summary>
        /// <param name="json">The JSON string that represents the wall.</param>
        public Projectile(string json)
        {
            JObject jObject = JObject.Parse(json);
            projectileID = (int)jObject["proj"];

            JToken locat = jObject["loc"];
            int locatX = (int)locat["x"];
            int locatY = (int)locat["y"];
            location = new Vector2D(locatX, locatY);

            JToken direc = jObject["dir"];
            int direcX = (int)direc["x"];
            int direcY = (int)direc["y"];
            direction = new Vector2D(direcX, direcY);

            disappeared = (bool)jObject["died"];
            tankID = (int)jObject["owner"];
        }

        [JsonIgnore]
        /// <summary>
        /// Allows other methods to access and modify the origin 
        /// of a beam.
        /// </summary>
        public Vector2D Location
        {
            get { return location; }
            set { location = value; }
        }

        [JsonIgnore]
        /// <summary>
        /// Allows other methods to access and modify the direction 
        /// of a projectile.
        /// </summary>
        public Vector2D Direction
        {
            get { return direction; }
            set { direction = value; }
        }

        [JsonIgnore]
        /// <summary>
        /// Allows other methods to access and modify the unique ID 
        /// of a projectile.
        /// </summary>
        public int ID
        {
            get { return projectileID; }
            set { projectileID = value; }
        }

        [JsonIgnore]
        /// <summary>
        /// Allows other methods to access and modify the owner
        /// of a projectile.
        /// </summary>
        public int Owner
        {
            get { return tankID; }
            set { tankID = value; }
        }

        [JsonIgnore]
        /// <summary>
        /// Allows other methods to access and modify the status of the
        /// projectile's disappearence.
        /// </summary>
        public bool Died
        {
            get { return disappeared; }
            set { disappeared = value; }
        }

        [JsonIgnore]
        /// <summary>
        /// Allows other methods to access and modify the color of the 
        /// projectile.
        /// </summary>
        public int Color
        {
            get { return color; }
            set { color = value; }
        }
    }
}
