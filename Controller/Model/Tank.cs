// Author : Alexsis Lever & Dayi Hu

// Change Log : Version 1.1
// Last Update : 11/12/2019 - Implemented this entire class under the design
//                            choice that the model will contain no controller
//                            aspects.

// Change Log : Version 1.2
// Last Update : 11/16/2019 - Updated the constructor to parse a JSON string
//                            relative to taking in all properties as parameters.

// Change Log : Version 1.3
// Last Update : 11/21/2019 - Added a color property.

// Change Log : Version 1.4
// Last Update : 12/03/2019 - Made Getter/Setters ignored by JSON so they are
//                            not included in serialization.

// Change Log : Version 1.5
// Last Update : 12/05/2019 - Added a respawn rate, a moving direction string
//                            and a frames per shot variable.



using Newtonsoft.Json;
using System;
using TankWars;
using Newtonsoft.Json.Linq;

namespace Model
{
    public class Tank
    {
        // Represents the tank's unique ID
        [JsonProperty(PropertyName = "tank")]
        private int tankID;

        // Represents the player's name
        [JsonProperty(PropertyName = "name")]
        private string playerName;

        // Represents the tank's location
        [JsonProperty(PropertyName = "loc")]
        private Vector2D location;

        // Represents the tank's orientation
        [JsonProperty(PropertyName = "bdir")]
        private Vector2D orientation;

        // Represents the tank's aim direction
        [JsonProperty(PropertyName = "tdir")]
        private Vector2D aim;

        // Represents the player's score
        [JsonProperty(PropertyName = "score")]
        private int playerScore;

        // Represents the player's health
        [JsonProperty(PropertyName = "hp")]
        private int health;

        // Represents the tank was destroyed
        [JsonProperty(PropertyName = "died")]
        private bool destroyed;

        // Represents the player disconnected
        [JsonProperty(PropertyName = "dc")]
        private bool disconnected;

        // Represents the player connected
        [JsonProperty(PropertyName = "join")]
        private bool connected;

        // Stores the color of a tank for beyond 8 players
        private int color = 8;

        private int respawnRate = 0;

        private int framesPerShot = 0;

        private string movingDirection = "none";

        /// <summary>
        /// The constructor for a tank that takes into account all parameters including name, orientation,
        /// direction, score, health, connected status, and death status.
        /// </summary>
        /// <param name="json">The JSON string that represents the wall.</param>
        public Tank(string json)
        {
            JObject jObject = JObject.Parse(json);
            tankID = (int)jObject["tank"];
            playerName = (string)jObject["name"];

            JToken locat = jObject["loc"];
            int locatX = (int)locat["x"];
            int locatY = (int)locat["y"];
            location = new Vector2D(locatX, locatY);

            JToken orien = jObject["bdir"];
            int orienX = (int)orien["x"];
            int orienY = (int)orien["y"];
            orientation = new Vector2D(orienX, orienY);

            JToken turret = jObject["tdir"];
            int turretX = (int)turret["x"];
            int turretY = (int)turret["y"];
            aim = new Vector2D(turretX, turretY);

            playerScore = (int)jObject["score"];
            health = (int)jObject["hp"];
            destroyed = (bool)jObject["died"];
            disconnected = (bool)jObject["dc"];
            connected = (bool)jObject["join"];
        }

        [JsonIgnore]
        /// <summary>
        /// Allows other methods to access and modify the unique ID 
        /// of a tank.
        /// </summary>
        public int ID
        {
            get { return tankID; }
            set { tankID = value; }
        }

        [JsonIgnore]
        /// <summary>
        /// Allows other methods to access and modify the unique name 
        /// of a player.
        /// </summary>
        public string PlayerName
        {
            get { return playerName; }
            set { playerName = value; }
        }

        [JsonIgnore]
        /// <summary>
        /// Allows other methods to access and modify the unique location 
        /// of a tank.
        /// </summary>
        public Vector2D Location
        {
            get { return location; }
            set { location = value; }
        }

        [JsonIgnore]
        /// <summary>
        /// Allows other methods to access and modify the unique orientation 
        /// of a tank.
        /// </summary>
        public Vector2D Orientation
        {
            get { return orientation; }
            set { orientation = value; }
        }

        [JsonIgnore]
        /// <summary>
        /// Allows other methods to access and modify the unique projectile aim
        /// of a tank.
        /// </summary>
        public Vector2D Aim
        {
            get { return aim; }
            set { aim = value; }
        }

        [JsonIgnore]
        /// <summary>
        /// Allows other methods to access and modify the unique score 
        /// of a tank.
        /// </summary>
        public int PlayerScore
        {
            get { return playerScore; }
            set { playerScore = value; }
        }

        [JsonIgnore]
        /// <summary>
        /// Allows other methods to access and modify the unique health status
        /// of a tank.
        /// </summary>
        public int Health
        {
            get { return health; }
            set { health = value; }
        }

        [JsonIgnore]
        /// <summary>
        /// Allows other methods to access and modify the status
        /// of whether or not a tank has been destroyed.
        /// </summary>
        public bool Destroyed
        {
            get { return destroyed; }
            set { destroyed = value; }
        }

        [JsonIgnore]
        /// <summary>
        /// Allows other methods to access and modify the status
        /// of whether or not a tank has been disconnected.
        /// </summary>
        public bool Disconnected
        {
            get { return disconnected; }
            set { disconnected = value; }
        }

        [JsonIgnore]
        /// <summary>
        /// Allows other methods to access and modify the status
        /// of whether or not a tank has been connected.
        /// </summary>
        public bool Connected
        {
            get { return connected; }
            set { connected = value; }
        }

        [JsonIgnore]
        /// <summary>
        /// Allows other methods to access and modify the color
        /// of a tank.
        /// </summary>
        public int Color
        {
            get { return color; }
            set { color = value; }
        }

        [JsonIgnore]
        /// <summary>
        /// Allows other methods to access and modify the respawn rate
        /// of a tank.
        /// </summary>
        public int RespawnRate
        {
            get { return respawnRate; }
            set { respawnRate = value; }
        }

        [JsonIgnore]
        /// <summary>
        /// Allows other methods to access and modify the respawn rate
        /// of a tank.
        /// </summary>
        public int FramesPerShot
        {
            get { return framesPerShot; }
            set { framesPerShot = value; }
        }

        [JsonIgnore]
        /// <summary>
        /// Allows other methods to access and modify the movement direction
        /// of a tank.
        /// </summary>
        public string MovingDirection
        {
            get { return movingDirection; }
            set { movingDirection = value; }
        }
    }
}
