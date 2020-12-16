// Author : Alexsis Lever & Dayi Hu

// Change Log : Version 1.1
// Last Update : 11/14/2019 - Transferred code to handle the first
//                            message sent over by the server, namely
//                            the player ID and the world size. 

// Change Log : Version 1.2
// Last Update : 11/16/2019 - Continued tranferring code from view
//                            while adding handlers and receiving
//                            initial messages from the server.

// Change Log : Version 1.3
// Last Update : 11/17/2019 - Finished adding the handshake and 
//                            event loop to process messages sent
//                            from the server.

// Change Log : Version 1.4
// Last Update : 11/19/2019 - Made minor changes as a means to start
//                            debugging.

// Change Log : Version 1.5
// Last Update : 11/21/2019 - Made minor changes as a means to continue
//                            debugging and ensure all messages from the
//                            server are actually processed.

// Change Log : Version 1.6
// Last Update : 11/24/2019 - Made minor changes so projectiles draw as one
//                            color.


using NetworkUtil;
using Model;
using System.Text.RegularExpressions;
using TankWars;

namespace GameController
{
    public class GameController
    {
        //private SocketState theServer;
        private World theWorld;
        private string playername;

        public delegate void ErrorHandler();
        private event ErrorHandler ErrorOccured;

        public delegate void ServerUpdateHandler();
        private event ServerUpdateHandler Updated;

        public bool movingUp;
        public bool movingDown;
        public bool movingLeft;
        public bool movingRight;
        public bool fireBeam;
        public bool fireProjectile;

        public Vector2D tdir;

        private SocketState theServer;

        public GameController()
        {
            theWorld = new World();
        }

        public void RegisterErrorHandler(ErrorHandler h)
        {
            ErrorOccured += h;
        }

        public void RegisterServerUpdateHandler(ServerUpdateHandler h)
        {
            Updated += h;
        }

        public World GetWorld()
        {
            return theWorld;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hostname"></param>
        public void ConnectToServer(string hostname, string player)
        {
            playername = player;
            Networking.ConnectToServer(OnConnect, hostname, 11000);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ss"></param>
        private void OnConnect(SocketState ss)
        {
            if (ss.ErrorOccured)
            {
                ErrorOccured();
                return;
            }

            // Save the SocketState so we can use it to send messages
            theServer = ss;


            // Start an event loop to receive messages from the server
            ss.OnNetworkAction = ReceiveStartup;
            Networking.Send(ss.TheSocket, playername);
            Networking.GetData(ss);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ss"></param>
        private void ReceiveStartup(SocketState ss)
        {
            if (ss.ErrorOccured)
            {
                ErrorOccured();
                return;
            }

            // Extract startup data
            string totalData;
            totalData = ss.GetData();
            string[] startupData = Regex.Split(totalData, @"(?<=[\n])");
            theWorld.PlayerID = int.Parse(startupData[0]);
            theWorld.Size = int.Parse(startupData[1]);
            ss.RemoveData(0, startupData[0].Length + startupData[1].Length);

            // Get the World information
            ss.OnNetworkAction = ReceiveWorld;
            Networking.GetData(ss);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ss"></param>
        public void ReceiveWorld(SocketState ss)
        {
            if (ss.ErrorOccured)
            {
                ErrorOccured();
                return;
            }

            // Process the messages from the server and update the world
            ProcessMessages(ss);

            SendServerMessage();

            // Notify any listeners (the view) that a new game world has arrived from the server
            if (Updated != null)
            {
                Updated();
            }

            // Continue the event loop
            Networking.GetData(ss);
        }

       private void ProcessMessages(SocketState ss)
       {
            string totalData;
            totalData = ss.GetData();
            string[] messages = Regex.Split(totalData, @"(?<=[\n])");
            lock (theWorld)
            {
                foreach (string m in messages)
                {
                    // Ignore empty strings
                    if (m.Length == 0)
                    {
                        continue;
                    }

                    // The regex splitter will include the last string even if it doesn't end with a '\n',
                    // So we need to ignore it if this happens. 
                    if (m[m.Length - 1] != '\n')
                    {
                        break;
                    }

                    // Process this line of JSON as a wall
                    if (m[2] == 'w')
                    {
                        Wall wall = new Wall(m);
                        theWorld.Walls[wall.ID] = wall;
                    }

                    // Process this line of JSON as a tank
                    else if (m[2] == 't')
                    {
                        Tank tank = new Tank(m);
                        if (tank.Destroyed || tank.Disconnected)
                            theWorld.Tanks.Remove(tank.ID);
                        else
                        {
                            //  Ensure tank color stays consistent
                            if (theWorld.Tanks.ContainsKey(tank.ID))
                            {
                                tank.Color = theWorld.Tanks[tank.ID].Color;
                            }
                            // Add the tank
                            theWorld.Tanks[tank.ID] = tank;
                        }
                    }

                    // Process this line of JSON as a projectile
                    else if (m[2] == 'p' && m[3] == 'r')
                    {
                        Projectile proj = new Projectile(m);
                        if (proj.Died)
                            theWorld.Projectiles.Remove(proj.ID);
                        else
                        {
                            if (theWorld.Projectiles.ContainsKey(proj.ID))
                            {
                                proj.Color = theWorld.Tanks[proj.Owner].Color;
                            }
                            theWorld.Projectiles[proj.ID] = proj;
                        }      
                    }

                    // Process this line of JSON as a powerup
                    else if (m[2] == 'p' && m[3] == 'o')
                    {
                        Powerup pow = new Powerup(m);
                        if (pow.Collected)
                            theWorld.Powerups.Remove(pow.ID);
                        else
                            theWorld.Powerups[pow.ID] = pow;
                    }

                    // Process this line of JSON as a beam
                    else
                    {
                        Beam beam = new Beam(m);
                        if (beam.framecounter < 5 && beam.fired)
                        {
                            beam.framecounter++;
                            theWorld.Beams[beam.ID] = beam;
                        }
                        else
                        {
                            theWorld.Beams.Remove(beam.ID);
                        }
                    }

                    // Be sure to remove processed messages
                    ss.RemoveData(0, m.Length);
                }
            }
            
       }

        /// <summary>
        /// Method used for the controller to send messages to the server.
        /// </summary>
        public void SendServerMessage()
        {
            lock (theWorld)
            {
                // Set up a moving and firing command initially set to "none"
                string moving = "none";
                string fire = "none";

                // Determine if the tank is moving
                if (movingUp)
                {
                    moving = "up";
                }
                else if (movingDown)
                {
                    moving = "down";
                }
                else if (movingLeft)
                {
                    moving = "left";
                }
                else if (movingRight)
                {
                    moving = "right";
                }

                // Determine if a beam or projectile are being fired
                if (fireBeam)
                {
                    fire = "alt";
                }
                else if (fireProjectile)
                {
                    fire = "main";
                }

                // Get the x and y coordinates
                string x = tdir.GetX().ToString();
                string y = tdir.GetY().ToString();

                // Compile and send the message to the server
                string message = "{\"moving\":\"" + moving + "\",\"fire\":\"" + fire + "\",\"tdir\":{\"x\":" + x + ",\"y\":" + y + "}}\n";
                Networking.Send(theServer.TheSocket, message);
            }
        }
    }
}
