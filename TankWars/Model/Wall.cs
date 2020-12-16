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



using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TankWars;
using Newtonsoft.Json.Linq;

namespace Model
{
    public class Wall
    {
        // Represents the walls's unique ID
        [JsonProperty(PropertyName = "wall")]
        private int wallID;

        // Represents the first endpoint of a wall
        [JsonProperty(PropertyName = "p1")]
        private Vector2D startPoint;

        // Represents the second endpoint of a wall
        [JsonProperty(PropertyName = "p2")]
        private Vector2D endPoint;


        /// <summary>
        /// Constructor that determines the fields of the wall, namely
        /// the ID and both endpoints.
        /// </summary>
        /// <param name="json">The JSON string that represents the wall.</param>
        public Wall(string json)
        {
            JObject jObject = JObject.Parse(json);
            wallID = (int)jObject["wall"];

            JToken start = jObject["p1"];
            int startX = (int)start["x"];
            int startY = (int)start["y"];
            startPoint = new Vector2D(startX, startY);

            JToken end = jObject["p2"];
            int endX = (int)end["x"];
            int endY = (int)end["y"];
            endPoint = new Vector2D(endX, endY);
        }

        [JsonIgnore]
        /// <summary>
        /// Allows other methods to access and modify the first endpoint 
        /// of a wall.
        /// </summary>
        public Vector2D P1
        {
            get { return startPoint; }
            set { startPoint = value; }
        }

        [JsonIgnore]
        /// <summary>
        /// Allows other methods to access and modify the second endpoint 
        /// of a wall.
        /// </summary>
        public Vector2D P2
        {
            get { return endPoint; }
            set { endPoint = value; }
        }

        [JsonIgnore]
        /// <summary>
        /// Allows other methods to access and modify the unique ID 
        /// of a powerup.
        /// </summary>
        public int ID
        {
            get { return wallID; }
            set { wallID = value; }
        }
    }
}
