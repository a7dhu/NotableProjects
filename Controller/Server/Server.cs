// Author : Alexsis Lever & Dayi Hu

// Change Log : Version 1.1
// Last Update : 11/26/2019 - Added this class.

// Change Log : Version 1.2
// Last Update : 11/29/2019 - Set up the basis for the server side
//                            of the handshake and main processing
//                            xml document.

// Change Log : Version 1.3
// Last Update : 12/1/2019 - Updated ReadSettings() such that the xml
//                           document is read. Utilized dictionaries 
//                           for cleaner software practices. Continued 
//                           the server side of the handshake.

// Change Log : Version 1.4
// Last Update : 12/2/2019 - Added a frame loop to Main() and began 
//                           constructing the methods to update the 
//                           world and send to all connected clients.
//                           Configured new tank location in regards to
//                           existing walls.

// Change Log : Version 1.5
// Last Update : 12/3/2019 - Made a few minor changes to send over the 
//                           wall data and ensure the client can actually
//                           connect. Started processing control commands 
//                           and working through updating the world.

// Change Log : Version 1.6
// Last Update : 12/5/2019 - Finished implementation of all remaining tasks.



using Model;
using NetworkUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml;
using System.Text;
using Newtonsoft.Json;
using TankWars;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace Server
{
    class Server
    {
        // Dictionary to keep track of the clients
        public static Dictionary<long, SocketState> clients;

        // Setting member variables
        public static Dictionary<string, int> settings;

        // The server's instance of the world
        private static World theWorld;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // Initialize a server, read the settings, and start the server
            Server server = new Server();
            ReadSettings();
            theWorld.Size = settings["UniverseSize"];
            server.StartServer();

            // Create a new thread where the world can be updated 
            // and sent to all existing clients
            Thread frameLoop = new Thread(new ThreadStart(FrameLoop));
            frameLoop.Start();

            // Sleep to prevent the program from closing,
            // since all the real work is done in separate threads.
            // StartServer is non-blocking.
            Console.Read();
        }

        /// <summary>
        /// Method used to create a loop where the world is updated 
        /// every frame and sent out to all connected clients.
        /// </summary>
        private static void FrameLoop()
        {
            int frameRate = settings["MSPerFrame"];
            Stopwatch timer = new Stopwatch();
            while (true)
            {
                timer.Start();

                // Ensure the time specified in the settings has elapsed
                // before sending an update to all connected clients
                while (timer.ElapsedMilliseconds < frameRate)
                {
                }

                // Send the update and reset the timer
                SendUpdatedWorld();
                timer.Reset();
            }
        }

        /// <summary>
        /// Constructor for the server.
        /// </summary>
        public Server()
        {
            clients = new Dictionary<long, SocketState>();
            settings = new Dictionary<string, int>();
            theWorld = new World();
        }

        /// <summary>
        /// Used to start the server.
        /// </summary>
        public void StartServer()
        {
            Networking.StartServer(BeginAccept, 11000);
        }

        /// <summary>
        /// Method used when a new client tries to connect.
        /// </summary>
        /// <param name="ss">The client's socketstate.</param>
        private void BeginAccept(SocketState ss)
        {
            if (ss.ErrorOccured)
                return;

            ss.OnNetworkAction = AcceptedCallback;
            Networking.GetData(ss);
        }

        /// <summary>
        /// Callback for when a client does connect and they are added
        /// to the client dictionary. A tank is created for them and a
        /// separate call to ReceiveControlCommands allows for the client
        /// to send commands. Calls BeginAccept() to create a loop for accepting 
        /// new clients.
        /// </summary>
        /// <param name="ss">The client's socketstate.</param>
        private void AcceptedCallback(SocketState ss)
        {
            if (ss.ErrorOccured)
                return;

            // Save the client state
            // Need to lock here because clients can disconnect at any time
            lock (clients)
            {
                clients[ss.ID] = ss;
            }

            // Get the playername, send startup information, and add tank
            string totalData;
            totalData = ss.GetData();
            string[] playername = Regex.Split(totalData, @"(?<=[\n])");
            string startupData = ss.ID + "\n" + settings["UniverseSize"] + "\n";
            Networking.Send(ss.TheSocket, startupData);
            ss.RemoveData(0, playername[0].Length);
            AddTank(playername[0]);

            // Get the wall data and send it to the client
            StringBuilder wallData = new StringBuilder();
            foreach (Wall w in theWorld.Walls.Values)
            {
                //wallData.Append(WallJSONConstructor(w.P1.GetX().ToString(), w.P1.GetY().ToString(), w.P2.GetX().ToString(), w.P2.GetY().ToString()) + "\n");
                wallData.Append(JsonConvert.SerializeObject(w) + "\n");
            }
            Networking.Send(ss.TheSocket, wallData.ToString());

            // Update the OnNetworkAction in order to process commands sent
            // from the client
            ss.OnNetworkAction = ReceiveControlCommands;
            Networking.GetData(ss);
        }

        /// <summary>
        /// Method used to process control commands sent by the
        /// client. 
        /// </summary>
        /// <param name="ss">The client's socketstate.</param>
        private void ReceiveControlCommands(SocketState ss)
        {
            if (ss.ErrorOccured)
                return;

            // Process control commands 
            string totalData;
            totalData = ss.GetData();
            string[] commands = Regex.Split(totalData, @"(?<=[\n])");
            foreach (string s in commands)
            {
                if (s == "")
                {
                    continue;
                }
                // Break up the JSON command into the three key components,
                // namely moving, fire, and tdir
                JObject jObject = JObject.Parse(s);
                string moving = jObject["moving"].ToString();
                string fire = jObject["fire"].ToString();
                JToken turretInfo = jObject["tdir"];
                int tdirX = (int)turretInfo["x"];
                int tdirY = (int)turretInfo["y"];
                Vector2D tdir = new Vector2D(tdirX, tdirY);

                lock (theWorld)
                {
                    // Process the moving command appropriately
                    theWorld.Tanks[(int)ss.ID].MovingDirection = moving;

                    // Process the fire command appropriately
                    switch (fire)
                    {
                        case "none":
                            break;
                        case "main":
                            if (theWorld.Tanks[(int)ss.ID].FramesPerShot == 0)
                            {
                                AddProjectile(theWorld.Tanks[(int)ss.ID]);
                                theWorld.Tanks[(int)ss.ID].FramesPerShot++;
                            }
                            break;
                        case "alt":
                            if (theWorld.Tanks[(int)ss.ID].FramesPerShot == 0)
                            {
                                AddBeam(theWorld.Tanks[(int)ss.ID]);
                                theWorld.Tanks[(int)ss.ID].FramesPerShot++;
                            }
                            break;
                    }

                    // Process the tdir command appropriately
                    theWorld.Tanks[(int)ss.ID].Aim = tdir;
                }

                // Be sure to remove processed messages
                ss.RemoveData(0, s.Length);
            }
            Networking.GetData(ss);
        }

        /// <summary>
        /// Helper method used to go through the XML settings file.
        /// </summary>
        private static void ReadSettings()
        {
            XmlReaderSettings settingsXML = new XmlReaderSettings();
            settingsXML.IgnoreWhitespace = true;

            string filePath = AppDomain.CurrentDomain.BaseDirectory;
            using (XmlReader reader = XmlReader.Create(filePath + "..\\..\\" + "settings.xml", settingsXML))
            {
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        switch (reader.Name)
                        {
                            // Get the size of the universe
                            case "UniverseSize":
                                reader.Read();
                                settings.Add("UniverseSize", int.Parse(reader.Value));
                                break;
                            // Get the milliseconds per frame
                            case "MSPerFrame":
                                reader.Read();
                                settings.Add("MSPerFrame", int.Parse(reader.Value));
                                break;
                            // Get the frames per shot value
                            case "FramesPerShot":
                                reader.Read();
                                settings.Add("FramesPerShot", int.Parse(reader.Value));
                                break;
                            // Get the respawn rate for projectiles
                            case "RespawnRate":
                                reader.Read();
                                settings.Add("RespawnRate", int.Parse(reader.Value));
                                break;
                            // Get the number of tank hit points
                            case "TankHitPoints":
                                reader.Read();
                                settings.Add("TankHitPoints", int.Parse(reader.Value));
                                break;
                            // Get the speed of the projectiles
                            case "ProjectileSpeed":
                                reader.Read();
                                settings.Add("ProjectileSpeed", int.Parse(reader.Value));
                                break;
                            // Get the engine strength
                            case "EngineStrength":
                                reader.Read();
                                settings.Add("EngineStrength", int.Parse(reader.Value));
                                break;
                            // Get the tank size 
                            case "TankSize":
                                reader.Read();
                                settings.Add("TankSize", int.Parse(reader.Value));
                                break;
                            // Get the wall size
                            case "WallSize":
                                reader.Read();
                                settings.Add("WallSize", int.Parse(reader.Value));
                                break;
                            // Get the number of powerups
                            case "MaxNumPowerup":
                                reader.Read();
                                settings.Add("MaxNumPowerup", int.Parse(reader.Value));
                                break;
                            // Get the maximum power up delay time
                            case "MaxPowerupDelay":
                                reader.Read();
                                settings.Add("MaxPowerupDelay", int.Parse(reader.Value));
                                break;
                            // Process the walls and add them to the world
                            case "Wall":
                                string p1x = "";
                                string p1y = "";
                                string p2x = "";
                                string p2y = "";

                                // Boolean to ensure loop stays only within elements pertaining to current wall
                                bool wallAttributes = true;
                                while (wallAttributes)
                                {
                                    reader.Read();
                                    if (reader.IsStartElement())
                                    {
                                        switch (reader.Name)
                                        {
                                            // Process the x coordinate of the starting point
                                            case "p1x":
                                                reader.Read();
                                                p1x = reader.Value;
                                                break;
                                            // Process the y coordinate of the starting point
                                            case "p1y":
                                                reader.Read();
                                                p1y = reader.Value;
                                                break;
                                            // Process the x coordinate of the ending point
                                            case "p2x":
                                                reader.Read();
                                                p2x = reader.Value;
                                                break;
                                            // Process the y coordinate of the ending point
                                            case "p2y":
                                                reader.Read();
                                                p2y = reader.Value;
                                                break;
                                        }
                                    }

                                    // When loop reaches end of wall attributes, namely the end element with name "Wall",
                                    // set the looping boolean to false
                                    if (reader.NodeType == XmlNodeType.EndElement)
                                    {
                                        if (reader.Name.Equals("Wall"))
                                        {
                                            wallAttributes = false;
                                        }
                                    }
                                }

                                // Construct a JSON string for the wall and add it to the world
                                string wallJSON = WallJSONConstructor(p1x, p1y, p2x, p2y);
                                Wall w = new Wall(wallJSON);
                                lock (theWorld)
                                {
                                    theWorld.Walls[w.ID] = w;
                                }
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Helper method to construct a JSON string for walls and update
        /// the wall ID in the world.
        /// </summary>
        /// <param name="p1x">X coordinate of the starting point of the wall</param>
        /// <param name="p1y">Y coordinate of the starting point of the wall</param>
        /// <param name="p2x">X coordinate of the ending point of the wall</param>
        /// <param name="p2y">Y coordinate of the ending point of the wall</param>
        /// <returns></returns>
        private static string WallJSONConstructor(string p1x, string p1y, string p2x, string p2y)
        {
            int wallID = theWorld.ParticipantIDs["Walls"];
            string JSON = "{ \"wall\":" + wallID.ToString() + ",\"p1\":{ \"x\":" + p1x + ",\"y\":" + p1y + "},\"p2\":{ \"x\":" + p2x + ",\"y\":" + p2y + "} }";
            theWorld.ParticipantIDs["Walls"]++;
            return JSON;
        }

        /// <summary>
        /// Method used to add a new tank
        /// </summary>
        /// <param name="name"></param>
        private void AddTank(string name)
        {
            // Get tank parameters
            string tankID = theWorld.ParticipantIDs["Tanks"].ToString();
            Vector2D loc = SetNewLocation(30);
            string locX = loc.GetX().ToString();
            string locY = loc.GetY().ToString();

            // Create JSON & new tank
            string JSON = "{\"tank\":" + tankID + ",\"loc\":{\"x\":" + locX + ",\"y\":" + locY + "},\"bdir\":{\"x\":" + 0 + ",\"y\":" + -1 + "},\"tdir\":{\"x\":" + 0 + ",\"y\":" + 1 + "},\"name\":\"" + name + "\",\"hp\":3,\"score\":0,\"died\":false,\"dc\":false,\"join\":true}";
            Tank t = new Tank(JSON);
            lock (theWorld)
            {
                // Be sure to add powerups if this is the first client connecting
                if (theWorld.ParticipantIDs["Tanks"] == 0)
                {
                    AddPowerup();
                    AddPowerup();
                }
                // Add the tank to the world
                theWorld.ParticipantIDs["Tanks"]++;
                theWorld.Tanks[t.ID] = t;
            }
        }

        /// <summary>
        /// Method used to add a new projectile
        /// </summary>
        /// <param name="t"></param>
        private void AddProjectile(Tank t)
        {
            if (t.Health > 0)
            {
                // Get the projectile parameters
                string projID = theWorld.ParticipantIDs["Projectiles"].ToString();
                string locX = t.Location.GetX().ToString();
                string locY = t.Location.GetY().ToString();
                string dirX = t.Aim.GetX().ToString();
                string dirY = t.Aim.GetY().ToString();
                string owner = t.ID.ToString();

                // Compile the JSON & use it to create a new projectile
                string JSON = "{\"proj\":" + projID + ", \"loc\":{\"x\":" + locX + ",\"y\":" + locY + "}, \"dir\":{\"x\":" + dirX + ", \"y\":" + dirY + "}, \"died\":false, \"owner\":" + owner + "}";
                Projectile proj = new Projectile(JSON);
                lock (theWorld)
                {
                    // Add the projectile to the world
                    theWorld.ParticipantIDs["Projectiles"]++;
                    theWorld.Projectiles[proj.ID] = proj;
                }
            }
        }

        /// <summary>
        /// Method used to add a new beam
        /// </summary>
        /// <param name="t"></param>
        private void AddBeam(Tank t)
        {
            if (t.Health > 0)
            {
                // Get the beam parameters
                string beamID = theWorld.ParticipantIDs["Beams"].ToString();
                string locX = t.Location.GetX().ToString();
                string locY = t.Location.GetY().ToString();
                string dirX = t.Aim.GetX().ToString();
                string dirY = t.Aim.GetY().ToString();
                string owner = t.ID.ToString();

                // Compile the JSON and use it to create a new beam
                string JSON = "{\"beam\":" + beamID + ",\"org\":{\"x\":" + locX + ", \"y\":" + locY + "}, \"dir\":{\"x\":" + dirX + ", \"y\":" + dirY + "}, \"owner\":" + owner + "}";
                Beam b = new Beam(JSON);
                b.FrameCounter++;
                lock (theWorld)
                {
                    // Add the beam to the world
                    theWorld.ParticipantIDs["Beams"]++;
                    theWorld.Beams[b.ID] = b;
                }
            }
        }

        /// <summary>
        /// Method used to add a new powerup to the world
        /// </summary>
        private static void AddPowerup()
        {
            // Get the powerup parameters
            string powerupID = theWorld.ParticipantIDs["Powerups"].ToString();
            Vector2D loc = SetNewLocation(15);
            string locX = loc.GetX().ToString();
            string locY = loc.GetY().ToString();
            // Compile the JSON, create a new powerup, and add it to the world 
            string JSON = "{\"power\":" + powerupID + ",\"loc\":{\"x\":" + locX + ", \"y\":" + locY + "},\"died\":false}";
            Powerup p = new Powerup(JSON);
            theWorld.ParticipantIDs["Powerups"]++;
            theWorld.Powerups[p.ID] = p;
        }

        /// <summary>
        /// Used to set a new location for a participant
        /// </summary>
        /// <param name="length">Distance from the center of the participant to its edge</param>
        /// <returns></returns>
        private static Vector2D SetNewLocation(int length)
        {
            Random rand = new Random();
            long locX = 0;
            long locY = 0;
            Vector2D loc = new Vector2D(locX, locY);
            bool searching = true;

            while (searching)
            {
                // Choose a new location in the world
                locX = rand.Next(-(theWorld.Size) / 2, theWorld.Size / 2);
                locY = rand.Next(-(theWorld.Size) / 2, theWorld.Size / 2);
                loc = new Vector2D(locX, locY);

                bool badLocation = false;
                // Check to see if the participant's location is on a powerup
                foreach (Powerup p in theWorld.Powerups.Values)
                {
                    if ((loc - p.Location).Length() <= length + 15)
                    {
                        badLocation = true;
                    }
                }

                // Check to see if the participant's location is on another tank
                foreach (Tank t in theWorld.Tanks.Values)
                {
                    if ((loc - t.Location).Length() <= length + 30)
                    {
                        badLocation = true;
                    }
                }

                // Check to see if the participant's location is on a wall
                int wallDistance = length + 25;
                foreach (Wall w in theWorld.Walls.Values)
                {
                    // If the wall is verticle
                    if (w.P1.GetX() == w.P2.GetX())
                    {
                        // Ensure the tank is not in the range of the wall
                        if (Math.Abs(loc.GetX() - w.P1.GetX()) <= wallDistance)
                        {
                            if (w.P1.GetY() < w.P2.GetY())
                            {
                                if (loc.GetY() >= (w.P1.GetY() - wallDistance) && loc.GetY() <= (w.P2.GetY() + wallDistance))
                                {
                                    badLocation = true;
                                }
                            }
                            else
                            {
                                if (loc.GetY() >= (w.P2.GetY() - wallDistance) && loc.GetY() <= (w.P1.GetY() + wallDistance))
                                {
                                    badLocation = true;
                                }
                            }
                        }
                    }

                    // Otherwise the wall is horizontal
                    else
                    {
                        // Ensure the tank is not within the range of the wall
                        if (Math.Abs(loc.GetY() - w.P1.GetY()) <= wallDistance)
                        {
                            if (w.P1.GetX() < w.P2.GetX())
                            {
                                if (loc.GetX() >= (w.P1.GetX() - wallDistance) && loc.GetX() <= (w.P2.GetX() + wallDistance))
                                {
                                    badLocation = true;
                                }
                            }
                            else
                            {
                                if (loc.GetX() >= (w.P2.GetX() - wallDistance) && loc.GetX() <= (w.P1.GetX() + wallDistance))
                                {
                                    badLocation = true;
                                }
                            }
                        }
                    }
                }

                // If a bad location was selected, choose a new one
                if (!badLocation)
                {
                    searching = false;
                }
            }

            return loc;
        }

        /// <summary>
        /// Method used to send the updates to all connected clients
        /// </summary>
        private static void SendUpdatedWorld()
        {
            lock (theWorld)
            {
                // Process/update all participants in the world
                UpdateProjectiles();
                UpdateTanks();
                UpdateBeams();
                UpdatePowerups();
            }

            // Get the JSON version of each participant as the current
            // status of the world and send it to all connected clients
            string JSONMessage = CompileWorldJSON();
            lock (theWorld)
            {
                foreach (SocketState ss in clients.Values)
                {
                    if (ss.TheSocket.Connected)
                    {
                        Networking.Send(ss.TheSocket, JSONMessage);
                    }
                }
            }
        }

        /// <summary>
        /// Appends all participant JSON information to a string builder as
        /// the current status of the world.
        /// </summary>
        /// <returns>String containing world status</returns>
        private static string CompileWorldJSON()
        {
            StringBuilder JSONMessage = new StringBuilder();
            lock (theWorld)
            {
                foreach (Tank t in theWorld.Tanks.Values)
                {
                    JSONMessage.Append(JsonConvert.SerializeObject(t) + "\n");
                }
                foreach (Projectile p in theWorld.Projectiles.Values)
                {
                    JSONMessage.Append(JsonConvert.SerializeObject(p) + "\n");
                }
                foreach (Beam b in theWorld.Beams.Values)
                {
                    JSONMessage.Append(JsonConvert.SerializeObject(b) + "\n");
                }
                foreach (Powerup p in theWorld.Powerups.Values)
                {
                    JSONMessage.Append(JsonConvert.SerializeObject(p) + "\n");
                }
            }

            return JSONMessage.ToString();
        }

        /// <summary>
        /// Method used to update the projectiles
        /// </summary>
        private static void UpdateProjectiles()
        {
            // Ensure all projectiles that have died are kept track of
            List<Projectile> deadProjectiles = new List<Projectile>();

            foreach (Projectile p in theWorld.Projectiles.Values)
            {
                // If the projectile collides with a wall, tank, or goes off the world add it to the
                // dead projectiles list
                if (p.Died)
                {
                    deadProjectiles.Add(p);
                }
                // Otherwise, update the location
                else
                {
                    p.Location = p.Location + (p.Direction * settings["ProjectileSpeed"]);
                }

                // Since walls are modifiable, account for projectiles going outside of the world
                if (p.Location.GetX() >= theWorld.Size / 2 || p.Location.GetY() >= theWorld.Size / 2)
                {
                    p.Died = true;
                }

                // If a projectile hits a tank, update its health and the firing tank's score
                foreach (Tank t in theWorld.Tanks.Values)
                {
                    if ((t.Location - p.Location).Length() <= 30)
                    {
                        if (t.Health > 0 && t.ID != p.Owner)
                        {
                            t.Health--;
                            if (t.Health == 0)
                            {
                                theWorld.Tanks[p.Owner].PlayerScore++;
                            }

                            p.Died = true;
                        }

                        if (t.Health == 0)
                        {
                            t.RespawnRate++;
                        }
                    }
                }

                // If a projectile hits a wall, be sure that it "dies"
                foreach (Wall w in theWorld.Walls.Values)
                {
                    // If the wall is vertical
                    if (w.P1.GetX() == w.P2.GetX())
                    {
                        // Ensure the projectile is not in the range of the wall
                        if (Math.Abs(p.Location.GetX() - w.P1.GetX()) <= 25)
                        {
                            if (w.P1.GetY() < w.P2.GetY())
                            {
                                if (p.Location.GetY() >= (w.P1.GetY() - 25) && p.Location.GetY() <= (w.P2.GetY() + 25))
                                {
                                    p.Died = true;
                                }
                            }
                            else
                            {
                                if (p.Location.GetY() >= (w.P2.GetY() - 25) && p.Location.GetY() <= (w.P1.GetY() + 25))
                                {
                                    p.Died = true;
                                }
                            }
                        }
                    }

                    // Otherwise the wall is horizontal
                    else
                    {
                        // Ensure the projectile is not in the range of the wall
                        if (Math.Abs(p.Location.GetY() - w.P1.GetY()) <= 25)
                        {
                            if (w.P1.GetX() < w.P2.GetX())
                            {
                                if (p.Location.GetX() >= (w.P1.GetX() - 25) && p.Location.GetX() <= (w.P2.GetX() + 25))
                                {
                                    p.Died = true;
                                }
                            }
                            else
                            {
                                if (p.Location.GetX() >= (w.P2.GetX() - 25) && p.Location.GetX() <= (w.P1.GetX()))
                                {
                                    p.Died = true;
                                }
                            }
                        }
                    }
                }
            }

            // Remove dead projectiles
            foreach (Projectile p in deadProjectiles)
            {
                theWorld.Projectiles.Remove(p.ID);
            }
        }

        /// <summary>
        /// Method used to update the tank positions
        /// </summary>
        private static void UpdateTanks()
        {
            foreach (Tank t in theWorld.Tanks.Values)
            {
                if (!clients[t.ID].TheSocket.Connected)
                {
                    t.Destroyed = true;
                    t.Disconnected = true;
                    t.Health = 0;
                }
                // If it is respawning, continue to respawn
                if (t.RespawnRate > 0 && t.RespawnRate < settings["RespawnRate"])
                {
                    t.RespawnRate++;
                }

                // If it has completed respawning, reset the tank
                if (t.RespawnRate == settings["RespawnRate"])
                {
                    t.Health = 3;
                    t.Orientation = new Vector2D(0, 1);
                    t.RespawnRate = 0;
                    t.Aim = new Vector2D(0, 1);
                }

                // Ensure the tank is not firing within the frames per shot setting
                if (t.FramesPerShot > 0 && t.FramesPerShot < settings["FramesPerShot"])
                {
                    t.FramesPerShot++;
                }

                // If the tank's frames per shot has expired, reset it
                if (t.FramesPerShot == settings["FramesPerShot"])
                {
                    t.FramesPerShot = 0;
                }

                // Process moving commands
                Vector2D originalLocation = t.Location;
                Vector2D originalOrientation = t.Orientation;
                switch (t.MovingDirection)
                {
                    case "up":
                        t.Orientation = new Vector2D(0, -1);
                        t.Location = t.Location + (t.Orientation * settings["EngineStrength"]);
                        break;
                    case "down":
                        t.Orientation = new Vector2D(0, 1);
                        t.Location = t.Location + (t.Orientation * settings["EngineStrength"]);
                        break;
                    case "right":
                        t.Orientation = new Vector2D(1, 0);
                        t.Location = t.Location + (t.Orientation * settings["EngineStrength"]);
                        break;
                    case "left":
                        t.Orientation = new Vector2D(-1, 0);
                        t.Location = t.Location + (t.Orientation * settings["EngineStrength"]);
                        break;
                }

                // Ensure the proposed new location of a tank is not on a wall
                bool validProposal = true;
                foreach (Wall w in theWorld.Walls.Values)
                {
                    // If the wall is verticle
                    if (w.P1.GetX() == w.P2.GetX())
                    {
                        if (Math.Abs(t.Location.GetX() - w.P1.GetX()) <= 50)
                        {
                            if (w.P1.GetY() < w.P2.GetY())
                            {
                                if (t.Location.GetY() >= (w.P1.GetY() - 50) && t.Location.GetY() <= (w.P2.GetY() + 50))
                                {
                                    validProposal = false;
                                }
                            }
                            else
                            {
                                if (t.Location.GetY() >= (w.P2.GetY() - 50) && t.Location.GetY() <= (w.P1.GetY() + 50))
                                {
                                    validProposal = false;
                                }
                            }
                        }
                    }

                    // Otherwise the wall is horizontal
                    else
                    {
                        if (Math.Abs(t.Location.GetY() - w.P1.GetY()) <= 50)
                        {
                            if (w.P1.GetX() < w.P2.GetX())
                            {
                                if (t.Location.GetX() >= (w.P1.GetX() - 50) && t.Location.GetX() <= (w.P2.GetX() + 50))
                                {
                                    validProposal = false;
                                }
                            }
                            else
                            {
                                if (t.Location.GetX() >= (w.P2.GetX() - 50) && t.Location.GetX() <= (w.P1.GetX() + 50))
                                {
                                    validProposal = false;
                                }
                            }
                        }
                    }
                }

                // Ensure the new proposed location does not overlap another tank
                foreach (Tank t2 in theWorld.Tanks.Values)
                {
                    if (t2.ID != t.ID)
                    {
                        if ((t.Location - t2.Location).Length() <= 60)
                        {
                            validProposal = false;
                        }
                    }
                }

                // If the tank does not collide with another wall or tank, implement proposed
                // location and orientation
                if (!validProposal)
                {
                    t.Orientation = originalOrientation;
                    t.Location = originalLocation;
                }

                // Since walls are modifiable, handle a wraparound if the tank goes out of the world size
                if (Math.Abs(t.Location.GetX()) >= (theWorld.Size / 2) - 30)
                {
                    t.Location = new Vector2D(-t.Location.GetX(), t.Location.GetY());
                }
                if (Math.Abs(t.Location.GetY()) >= (theWorld.Size / 2) - 30)
                {
                    t.Location = new Vector2D(t.Location.GetX(), -t.Location.GetY());
                }
            }
        }

        /// <summary>
        /// Method used to update the beams.
        /// </summary>
        private static void UpdateBeams()
        {
            foreach (Beam b in theWorld.Beams.Values)
            {
                // Ensure the beams only exist for a given amount of time
                if (b.FrameCounter > 0 && b.FrameCounter < 120)
                {
                    b.FrameCounter++;
                }
                if (b.FrameCounter == 120)
                {
                    theWorld.Beams.Remove(b.ID);
                }

                // Ensure any tanks hit by a beam are removed
                foreach (Tank t in theWorld.Tanks.Values)
                {
                    if (Intersects(b.Origin, b.Direction, t.Location, 60) && t.ID != b.Owner)
                    {
                        t.Health = 0;
                        t.RespawnRate++;
                        theWorld.Tanks[b.Owner].PlayerScore++;
                    }
                }
            }
        }

        /// <summary>
        /// Determines if a ray intersects a circle
        /// </summary>
        /// <param name="rayOrig">The origin of the ray</param>
        /// <param name="rayDir">The direction of the ray</param>
        /// <param name="center">The center of the circle</param>
        /// <param name="r">The radius of the circle</param>
        /// <returns></returns>
        public static bool Intersects(Vector2D rayOrig, Vector2D rayDir, Vector2D center, double r)
        {
            double a = rayDir.Dot(rayDir);
            double b = ((rayOrig - center) * 2.0).Dot(rayDir);
            double c = (rayOrig - center).Dot(rayOrig - center) - r * r;

            // discriminant
            double disc = b * b - 4.0 * a * c;

            if (disc < 0.0)
                return false;

            // find the signs of the roots
            // technically we should also divide by 2a
            // but all we care about is the sign, not the magnitude
            double root1 = -b + Math.Sqrt(disc);
            double root2 = -b - Math.Sqrt(disc);

            return (root1 > 0.0 && root2 > 0.0);
        }

        /// <summary>
        /// Method used to update powerups
        /// </summary>
        private static void UpdatePowerups()
        {
            List<Powerup> collectedPowerups = new List<Powerup>();

            // If there is a delay, ensure no new powerups appear 
            if (theWorld.MaxDelay > 0 && theWorld.MaxDelay < settings["MaxPowerupDelay"])
            {
                theWorld.MaxDelay++;
            }

            // If the delay has ended, spawn a new powerup
            if (theWorld.MaxDelay == settings["MaxPowerupDelay"])
            {
                theWorld.MaxDelay = 0;
                AddPowerup();
            }

            foreach (Powerup p in theWorld.Powerups.Values)
            {
                // Be sure all collected powerups are added to be removed
                if (p.Collected)
                {
                    collectedPowerups.Add(p);
                    theWorld.NumPowerUps--;
                }

                // If a tank collects a powerup, update its settings
                foreach (Tank t in theWorld.Tanks.Values)
                {
                    if ((t.Location - p.Location).Length() <= 45)
                    {
                        p.Collected = true;
                        theWorld.MaxDelay++;
                    }
                }
            }

            // Remove collected powerups
            foreach (Powerup p in collectedPowerups)
            {
                theWorld.Powerups.Remove(p.ID);
                theWorld.MaxDelay++;
            }
        }
    }
}
