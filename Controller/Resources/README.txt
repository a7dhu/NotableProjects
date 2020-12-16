Author : Alexsis Lever & Dayi Hu
Notes for Grader : Thank you for taking the time to grade our solution! There are a few minor things in the server we worked on 
				and have implementation for, however are not fully functional : Clients disconnecting does not disrupt the game 
				and tanks are no longer drawn, however the explosion goes on repeat. The beams also have code implemented, however
				they are not drawn despite tanks being able to pick up powerups.


DOCUMENTATION FOR PS8 : 
Date : 11/10/2019
Spent most of the time understanding and trying to parse through the assignment description.
We started with the model, which we decided would be strictly for creating the structures
used by all participants. We structured the solution into resources, model, controller, and
view. Most of the model (tank, beam, projectile, powerup, and wall) were implemented.

Date : 11/12/2019
Finished constructing and verifying that the model implementation is correct. Went over
how all of the three pieces fit together to really hone in on separation of concerns. 
Confirmed that all model components will contain no controller aspects.

Date : 11/14/2019
Began understanding and setting up the view. Parsed through the responsibilities of the view VS
the controller and shifted some code originally placed in the view to the controller. The view
has textboxes for the player name and the hostname. The controller now has a basis for handling
the initial connection from the form.

Date : 11/16/2019
Worked mostly on transfering the rest of view components to controller to set up the main event
loop that receives messages from the server.

Date: 11/17/2019
Finished controller component that processes messages from the server and began setting up the
delegates in the DrawingPanel.cs. 

Date: 11/19/2019
Finished initial implementation of OnPaint, added images to a resource file in View, made minor tweaks
to Controller & Form1 for debugging purposes, and finished the initial implementation of participant drawers.

Date: 11/21/2019
Made modifications to controller, view, and drawing panel as needed for debugging. Ensured that objects are
actually drawn on the screen and messages from the server are processed.

Date: 11/22/2019
Made modifications to DrawingPanel.cs to ensure all participants are drawn in the correct location and modified the
wall drawing loop in OnPaint to call DrawObjectWithTransform() multiple times. 

Date: 11/23/2019
Made more minor changes so that the projectiles draw as one color based on the tank color, reset the wall
location, and finished key press and mouse click events. 

Date: 11/25/2019
Made final changes so that the tank draws correctly and the player name/health orient correctly. Also finished
turret tracking the mouse and firing of projectiles.


DOCUMENTATION FOR PS9 : 
Date: 11/26/2019
Began parsing through and making design decisions. For the server side of the handshake, we will follow the outline
provided in Lecture 25 and utilize the Networking design to create an event loop. The frame loop has yet to be decided.
For the model, we will begin by adding the motion, tanks, and collision updates as well as being able to compile the
appropriate JSON strings to send to the server. 

Date: 11/29/2019
Set up the basis of the server side of the handshake per outline provided in lecture 25 and added an xml settings file
with appropriate processing method. The method currently cannot access the file, but that will be fixed. We modified
the sample xml file such that walls have attributes p1x, p1y, p2x, and p2y for ease of attribute access and added all
additional necessary settings.

Date: 12/01/2019
Modified ReadSettings() in Server.cs such that the xml document was processed in full, particularly the x and y coordinates
of the starting and ending points of the wall. Decided to utilize dictionaries to keep track of both the settings (in Server.cs)
as well as participant IDs relative to having a series of member variables (in World.cs). Continued adding to the server side of
the handshake by beginning AddTank() and SetTankLocation(). SetTankLocation() may need to be slightly altered in the future 
depending on if it is necessary to pass the tankID. Created web server class. Created http headers and set up tables in web server.
Added model.cs, containing session model, player model, and game model.

Date: 12/02/2019
Added a frame loop to the Main() method, but want to verify with TA's or instructor whether this is valid or if an actual thread
should be constructed and contain the loop. Began constructing the method to update the world and send it to all connected clients,
primarily focusing on the latter. Utilized JsonConvert.Serialize() to ease conversion of participant JSON strings and used a string 
builder to easily add on each participant's string of JSON. Finished configuring a valid new location for a tank in regards to existing
walls. Created Database controller class. Created tables in MySQL Workbench.

Date: 12/03/2019
Fixed a series of methods via debugging to ensure the client can actually connect and that objects can be drawn. Made minor modifications
to the participant classes in the model in order to continue using JsonConvert.Serialize().

Date: 12/04/2019
Began implementing reader.Read() while loops in Database controller. Added data queries in command.CommandText. 
Added a dictionary to be used as a parameter in webServer.GetAllGames
Added a list to be used as a parameter in webServer.GetPlayerGames. Improved tables in MYSQL Workbench

Date: 12/05/2019
Improved web server. Finished while loops in Database Controller. Fixed data queries in Database Controller. 
Made sure the while loops got the correct information using data query. Passed parameters to appropriate webServer methods to generate tables.
Finished tables in MYSQL Workbench.
to the participant classes in the model in order to continue using JsonConvert.Serialize(). 

Date: 12/05/2019
Finished main implementation for the server side of the game. Clients disconnecting does not disrupt the game and tanks are no longer drawn,
however the explosion goes on repeat and this was something we were unable to fix entirely. The beams also have code implemented, however
they are not drawn despite tanks being able to pick up powerups.