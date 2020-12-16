// Author : Alexsis Lever & Dayi Hu

// Change Log : Version 1.1
// Last Update : 11/14/2019 - Added places for the user to insert a 
//                            player name and IP Address/Hostname. Also
//                            added a "Connect" button and set up the first
//                            basis of the controller responsible for this 
//                            connection.

// Change Log : Version 1.2
// Last Update : 11/17/2019 - Added ServerUpdateHandler and creation of
//                            drawing panel.

// Change Log : Version 1.3
// Last Update : 11/19/2019 - Added locks and changed minor things to 
//                            adapt to debugging.

// Change Log : Version 1.4
// Last Update : 11/21/2019 - Made modifications needed as revealed in 
//                            debugging.

// Change Log : Version 1.5
// Last Update : 11/22/2019 - Began configuring key events.

// Change Log : Version 1.6
// Last Update : 11/23/2019 - Finalized moving key events and projectile firing
//                            mouse click events. 


using System;
using System.Drawing;
using System.Windows.Forms;
using GameController;
using Model;
using TankWars;

namespace View
{
    public partial class Form1 : Form
    {
        private GameController.GameController controller = new GameController.GameController();
        private World theWorld;
        DrawingPanel drawingPanel;
        public static bool connected;

        public Form1()
        {
            InitializeComponent();
            theWorld = controller.GetWorld();
            controller.RegisterServerUpdateHandler(OnFrame);
            controller.RegisterErrorHandler(AllowReconnect);
            connected = false;

            // Connected boolean & update drawing panel size after connected
            
            // Set up the windows Form.
            ClientSize = new Size(800, 800);
            drawingPanel = new DrawingPanel(theWorld);
            drawingPanel.Location = new Point(0, 0);
            drawingPanel.Size = new Size(this.ClientSize.Width, this.ClientSize.Height);
            this.Controls.Add(drawingPanel);

            drawingPanel.MouseDown += Form1_MouseDown;
            drawingPanel.MouseUp += Form1_MouseUp;
            drawingPanel.MouseMove += Form1_MouseMove;
        }

        public static bool GetConnected()
        {
            return connected;
        }

        private void OnFrame()
        {
            // Don't try to redraw if the window doesn't exist yet.
            // This might happen if the controller sends an update
            // before the Form has started.
            if (!IsHandleCreated)
                return;

            MethodInvoker me = new MethodInvoker(() =>
            {
                // Invalidate this form and all its children
                // This will cause the form to redraw as soon as it can
                this.Invalidate(true);
            });
            try
            {
                this.Invoke(me);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Event handler for when user tries to connect
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void connectButton_Click(object sender, EventArgs e)
        {
            if (hostname.Text == "")
            {
                MessageBox.Show("Please enter a server address.");
                return;
            }

            if (playerName.Text.Length > 15)
            {
                MessageBox.Show("Your player name is too long, please choose a shorter one.");
                return;
            }

            MethodInvoker me = new MethodInvoker(() =>
            {
                // Disable controls & attempt to connect
                connectButton.Enabled = false;
                playerName.Enabled = false;
                hostname.Enabled = false;

                controller.ConnectToServer(hostname.Text, playerName.Text + '\n');
                connected = true;
            });
            try
            {
                this.Invoke(me);
            }
            catch
            {
            }
        }        

        /// <summary>
        /// Allows the user to reconnect if something went wrong during initial connection.
        /// </summary>
        private void AllowReconnect()
        {
            MessageBox.Show("Error connecting to server. Please try again and be sure the IP/Hostname is correct and user name is 16 characters or less.");

            // Re - enable the controls
            connectButton.Enabled = true;
            playerName.Enabled = true;
            hostname.Enabled = true;
        }

        /// <summary>
        /// Used to set the move controls when a move key is pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A)
            {
                controller.movingLeft = true;
            }

            if (e.KeyCode == Keys.S)
            {
                controller.movingDown = true;
            }

            if (e.KeyCode == Keys.D)
            {
                controller.movingRight = true;
            }

            if (e.KeyCode == Keys.W)
            {
                controller.movingUp = true;
            }
        }


        /// <summary>
        /// Used to reset the moving controls once the move keys are no longer pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A)
            {
                controller.movingLeft = false;
            }

            if (e.KeyCode == Keys.S)
            {
                controller.movingDown = false;
            }

            if (e.KeyCode == Keys.D)
            {
                controller.movingRight = false;
            }

            if (e.KeyCode == Keys.W)
            {
                controller.movingUp = false;
            }
        }

        /// <summary>
        /// Used to update the turret's position when the mouse is moved.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (connected)
            {
                Vector2D v = new Vector2D(e.Location.X - 400, e.Location.Y - 400);
                v.Normalize();
                controller.tdir = v;
            }
        }

        /// <summary>
        /// Used to determine the fire controls when the mouse is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            lock (theWorld)
            {
                if (theWorld.Beams.Count > 0)
                {
                    foreach (Beam b in theWorld.Beams.Values)
                    {
                        if (b.Owner == theWorld.PlayerID)
                        {
                            b.fired = true;
                            controller.fireBeam = true;
                            controller.fireProjectile = false;
                            break;
                        }
                        else
                        {
                            controller.fireProjectile = true;
                        }
                    }
                }
                else
                {
                    controller.fireProjectile = true;
                }
            }
        }

        /// <summary>
        /// Used to reset the fire controls once the mouse is no longer clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            controller.fireBeam = false;
            controller.fireProjectile = false;
        }
    }
}
