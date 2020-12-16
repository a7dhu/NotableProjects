// Author : Alexsis Lever & Dayi Hu

// Change Log : Version 1.1
// Last Update : 11/12/2019 - Implemented this entire class under the design
//                            choice that the model will contain no controller
//                            aspects.

// Change Log : Version 1.2
// Last Update : 11/16/2019 - Updated the constructor to parse a JSON string
//                            relative to taking in all properties as parameters.

// Change Log : Version 1.3
// Last Update : 12/03/2019 - Made Getter/Setters ignored by JSON so they are
//                            not included in serialization.

// Change Log : Version 1.4
// Last Update : 12/05/2019 - Added max delay property.



using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TankWars;
using Newtonsoft.Json.Linq;

namespace Model
{
    public class Powerup
    {
        // Represents the powerup's unique ID
        [JsonProperty(PropertyName = "power")]
        private int powerID;

        // Represents the location of the powerup
        [JsonProperty(PropertyName = "loc")]
        private Vector2D location;

        // Indicates whether or not the powerup "died" (i.e. collected by a player)
        [JsonProperty(PropertyName = "died")]
        private bool collected;

        private int maxdelay = 0;


        /// <summary>
        /// Constructor that determines the fields of the powerup, namely
        /// the ID, location, and whether or not it has been collected by a player.
        /// </summary>
        /// <param name="json">The JSON string that represents the wall.</param>
        public Powerup(string json)
        {
            JObject jObject = JObject.Parse(json);
            powerID = (int)jObject["power"];

            JToken locat = jObject["loc"];
            int locatX = (int)locat["x"];
            int locatY = (int)locat["y"];
            location = new Vector2D(locatX, locatY);

            collected = (bool)jObject["died"];
        }

        [JsonIgnore]
        /// <summary>
        /// Allows other methods to access and modify the collected property
        /// of a powerup.
        /// </summary>
        public bool Collected
        {
            get { return collected; }
            set { collected = value; }
        }

        [JsonIgnore]
        /// <summary>
        /// Allows other methods to access and modify the location property
        /// of a powerup.
        /// </summary>
        public Vector2D Location
        {
            get { return location; }
            set { location = value; }
        }

        [JsonIgnore]
        /// <summary>
        /// Allows other methods to access and modify the unique ID 
        /// of a powerup.
        /// </summary>
        public int ID
        {
            get { return powerID; }
            set { powerID = value; }
        }

        [JsonIgnore]
        /// <summary>
        /// Allows other methods to access and modify the max delay
        /// of a powerup.
        /// </summary>
        public int MaxDelay
        {
            get { return maxdelay; }
            set { maxdelay = value; }
        }
    }
}
