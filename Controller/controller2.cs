using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;


// Author : Alexsis Lever & Dayi Hu

// Change Log : Version 1.1
// Last Update : 12/2/2019 - Transferred code from Demo.cs of LabSQL

// Change Log : Version 1.2
// Last Update : 12/4/2019 - Began implementing reader.Read() while loops
//                           Added data queries in command.CommandText
//                           Added a dictionary to be used as a parameter in webServer.GetAllGames
//                           Added a list to be used as a parameter in webServer.GetPlayerGames


// Change Log : Version 1.3
// Last Update : 12/5/2019 - Finished while loops, fixed data queries, 
//                           Made sure the while loops got the correct information using data query
//                           Passed parameters to appropriate webServer methods to generate tables




namespace GameController
{
    //Retirns a dictionary of information from the requested database

    class controller2
    {
        public const string connectionString = "server=atr.eng.utah.edu;" +
      "database=Library;" +
      "uid=u1041821;" +
      "password=changeme";


        /// <summary>
        ///  Test several connections and print the output to the console
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
           

            AllPhones();
            Console.WriteLine();

            PatronsPhones();
            Console.ReadLine();
        }




        public static void AllPhones()
        {
            // Connect to the DB
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    // Open a connection
                    conn.Open();

                    // Create a command
                    MySqlCommand command = conn.CreateCommand();
                    command.CommandText = "select gID, Duration from Game";

                    Dictionary<uint, GameModel> gameStats = new Dictionary<uint, GameModel>();

                    // Execute the command and cycle through the DataReader object
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            GameModel game = new GameModel((uint)reader["gID"], (uint)reader["duration"]);
                            gameStats.Add((uint)reader["gID"], game);
           
                        }
                    }


                    command.CommandText = "select g.gID, g.pID, p.Name, g.Score, g.Accuracy from GamesPlayed as g join Player as p on g.pID = p.pID";

                    // Execute the command and cycle through the DataReader object
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            gameStats[(uint)reader["g.gID"]].AddPlayer((String)reader["p.Name"], (uint)reader["g.Score"], (uint)reader["g.Accuracy"]);
                        }
                    }


                    webserver.GetAllGames(gameStats);


                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }


        public static void PatronsPhones()
        {

           // Connect to the DB
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    // Open a connection
                    conn.Open();

                    // Create a command
                    MySqlCommand command = conn.CreateCommand();
                    command.CommandText = "select g.gID, g.pID, g.Score, g.Accuracy, game.Duration, p.Name from GamesPlayed as g join Player as p on g.pID = p.pID join Game as game on g.gID = game.gID";

                    List<SessionModel> list = new List<SessionModel>();

                    // Execute the command and cycle through the DataReader object
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        String prevName = "";
                        String name = "";

                        while (reader.Read())
                        {
                            name = (String)reader["p.Name"];
                            if (!prevName.Equals(name))
                            {
                                webserver.GetPlayerGames(prevName, list);
                                list.Clear();
                               

                            }

                            list.Add(new SessionModel((uint)reader["g.gID"], (uint)reader["game.Duration"], (uint)reader["g.Score"], (uint)reader["Accuracy"]));
                            prevName = name;
                        }


                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

    }
}
