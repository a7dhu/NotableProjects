// Author : Alexsis Lever & Dayi Hu

// Change Log : Version 1.1
// Last Update : 11/12/2019 - Implemented this entire class under the design
//                            choice that the model will contain no controller
//                            aspects.

// Change Log : Version 1.2
// Last Update : 11/16/2019 - Updated the constructor to parse a JSON string
//                            relative to taking in all properties as parameters.

// Change Log : Version 1.3
// Last Update : 11/24/2019 - Added a frame counter and fired property.

// Change Log : Version 1.4
// Last Update : 12/03/2019 - Made Getter/Setters ignored by JSON so they are
//                            not included in serialization.

// Change Log : Version 1.5
// Last Update : 12/05/2019 - Added Getter/Setter for framecounter.



using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TankWars;
using Newtonsoft.Json.Linq;

namespace Model
{
    public class Beam
    {
        // Represents the beams's unique ID
        [JsonProperty(PropertyName = "beam")]
        private int beamID;

        // Represents the origin of the beam
        [JsonProperty(PropertyName = "org")]
        private Vector2D origin;

        // Represents the direction of the beam
        [JsonProperty(PropertyName = "dir")]
        private Vector2D direction;

        // Represents the owner of the beam
        [JsonProperty(PropertyName = "owner")]
        private int tankID;

        private int framecounter = 0;

        public bool fired;


        /// <summary>
        /// Constructor that determines the fields of the beam, namely
        /// the ID, origin, direction, and owner.
        /// </summary>
        /// <param name="json">The JSON string that represents the wall.</param>
        public Beam(string json)
        {
            JObject jObject = JObject.Parse(json);
            beamID = (int)jObject["beam"];

            JToken org = jObject["org"];
            int orgX = (int)org["x"];
            int orgY = (int)org["y"];
            origin = new Vector2D(orgX, orgY);

            JToken direc = jObject["dir"];
            int direcX = (int)direc["x"];
            int direcY = (int)direc["y"];
            direction = new Vector2D(direcX, direcY);

            tankID = (int)jObject["owner"];
        }

        [JsonIgnore]
        /// <summary>
        /// Allows other methods to access and modify the origin 
        /// of a beam.
        /// </summary>
        public Vector2D Origin
        {
            get { return origin; }
            set { origin = value; }
        }

        [JsonIgnore]
        /// <summary>
        /// Allows other methods to access and modify the direction 
        /// of a beam.
        /// </summary>
        public Vector2D Direction
        {
            get { return direction; }
            set { direction = value; }
        }

        [JsonIgnore]
        /// <summary>
        /// Allows other methods to access and modify the unique ID 
        /// of a beam.
        /// </summary>
        public int ID
        {
            get { return beamID; }
            set { beamID = value; }
        }

        [JsonIgnore]
        /// <summary>
        /// Allows other methods to access and modify the owner
        /// of a beam.
        /// </summary>
        public int Owner
        {
            get { return tankID; }
            set { tankID = value; }
        }

        [JsonIgnore]
        /// <summary>
        /// Allows other methods to access and modify the frame count
        /// of a beam.
        /// </summary>
        public int FrameCounter
        {
            get { return framecounter; }
            set { framecounter = value; }
        }
    }
}
