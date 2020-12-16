// Author : Alexsis Lever & Dayi Hu

// Change Log : Version 1.1
// Last Update : 11/16/2019 - Began construction of drawing panel.

// Change Log : Version 1.2
// Last Update : 11/17/2019 - Began construction of drawing delegates
//                            for various participants in the world.

// Change Log : Version 1.3
// Last Update : 11/19/2019 - Managed to finish a first draft of OnPaint
//                            and pull the images from the View's resources
//                            folder. 

// Change Log : Version 1.4
// Last Update : 11/21/2019 - Made altercations for coordinates so participants
//                            are drawn closer to their actual location.

// Change Log : Version 1.5
// Last Update : 11/22/2019 - Updated participant origin location and modified
//                            the wall drawing loop in OnPaint() so that the
//                            DrawObjectWithTransfomr() is called multiple times

// Change Log : Version 1.6
// Last Update : 11/24/2019 - Updated wall drawer to draw them correctly and 
//                            ensured projectiles were not changing colors.
//                            Split tank drawer, turret drawer, and score/name drawer.


using System;
using System.Drawing;
using System.Windows.Forms;
using Model;

namespace View
{
    class DrawingPanel : Panel
    {
        // Represents the world
        private World theWorld;

        // Load all of the images
        static Image Wall = View.Properties.Resources.WallSprite;
        static Image Background = View.Properties.Resources.Background;
        static Image BlueTank = View.Properties.Resources.BlueTank;
        static Image BlueTurret = View.Properties.Resources.BlueTurret;
        static Image DarkTank = View.Properties.Resources.DarkTank;
        static Image DarkTurret = View.Properties.Resources.DarkTurret;
        static Image GreenTank = View.Properties.Resources.GreenTank;
        static Image GreenTurret = View.Properties.Resources.GreenTurret;
        static Image LightGreenTank = View.Properties.Resources.LightGreenTank;
        static Image LightGreenTurret = View.Properties.Resources.LightGreenTurret;
        static Image OrangeTank = View.Properties.Resources.OrangeTank;
        static Image OrangeTurret = View.Properties.Resources.OrangeTurret;
        static Image PurpleTank = View.Properties.Resources.PurpleTank;
        static Image PurpleTurret = View.Properties.Resources.PurpleTurret;
        static Image RedTank = View.Properties.Resources.RedTank;
        static Image RedTurret = View.Properties.Resources.RedTurret;
        static Image YellowTank = View.Properties.Resources.YellowTank;
        static Image YellowTurret = View.Properties.Resources.YellowTurret;
        static Image BrownShot = View.Properties.Resources.shot_brown;
        static Image GreenShot = View.Properties.Resources.shot_green;
        static Image WhiteShot = View.Properties.Resources.shot_white;
        static Image YellowShot = View.Properties.Resources.shot_yellow;
        static Image BlueShot = View.Properties.Resources.shot_blue;
        static Image GreyShot = View.Properties.Resources.shot_grey;
        static Image RedShot = View.Properties.Resources.shot_red_new;
        static Image VioletShot = View.Properties.Resources.shot_violet;
        static Image Powerup = View.Properties.Resources.Powerup;

        // Add the images to arrays for color consistency beyond 8 players
        Image[] Shots = new Image[] { BlueShot, GreyShot, GreenShot, WhiteShot, BrownShot, VioletShot, RedShot, YellowShot };
        Image[] Tanks = new Image[] { BlueTank, DarkTank, GreenTank, LightGreenTank, OrangeTank, PurpleTank, RedTank, YellowTank };
        Image[] Turrets = new Image[] { BlueTurret, DarkTurret, GreenTurret, LightGreenTurret, OrangeTurret, PurpleTurret, RedTurret, YellowTurret };

        /// <summary>
        /// Constructor used to set up the basis of the world.
        /// </summary>
        /// <param name="w"></param>
        public DrawingPanel(World w)
        {
            DoubleBuffered = true;
            theWorld = w;
        }

        /// <summary>
        /// Helper method for DrawObjectWithTransform
        /// </summary>
        /// <param name="size">The world (and image) size</param>
        /// <param name="w">The worldspace coordinate</param>
        /// <returns></returns>
        private static int WorldSpaceToImageSpace(int size, double w)
        {
            return (int)w + size / 2;
        }

        // A delegate for DrawObjectWithTransform
        // Methods matching this delegate can draw whatever they want using e  
        public delegate void ObjectDrawer(object o, PaintEventArgs e);

        /// <summary>
        /// This method performs a translation and rotation to drawn an object in the world.
        /// </summary>
        /// <param name="e">PaintEventArgs to access the graphics (for drawing)</param>
        /// <param name="o">The object to draw</param>
        /// <param name="worldSize">The size of one edge of the world (assuming the world is square)</param>
        /// <param name="worldX">The X coordinate of the object in world space</param>
        /// <param name="worldY">The Y coordinate of the object in world space</param>
        /// <param name="angle">The orientation of the objec, measured in degrees clockwise from "up"</param>
        /// <param name="drawer">The drawer delegate. After the transformation is applied, the delegate is invoked to draw whatever it wants</param>
        private void DrawObjectWithTransform(PaintEventArgs e, object o, int worldSize, double worldX, double worldY, double angle, ObjectDrawer drawer)
        {
            // "push" the current transform
            System.Drawing.Drawing2D.Matrix oldMatrix = e.Graphics.Transform.Clone();

            int x = WorldSpaceToImageSpace(worldSize, worldX);
            int y = WorldSpaceToImageSpace(worldSize, worldY);
            e.Graphics.TranslateTransform(x, y);
            e.Graphics.RotateTransform((float)angle);
            drawer(o, e);

            // "pop" the transform
            e.Graphics.Transform = oldMatrix;
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void TankDrawer(object o, PaintEventArgs e)
        {
            Tank t = o as Tank;

            // Determine the tank's color based on its ID
            if (t.ID < 8)
            {
                t.Color = t.ID;
            }
            else
            {
                // If the tank has an ID >= 8, assign it a random color
                Random rand = new Random();
                if (t.Color == 8)
                {
                    t.Color = rand.Next(8);
                }
            }

            // Draw the tank
            e.Graphics.DrawImage(Tanks[t.Color], -30, -30, 60, 60);
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void TurretDrawer(object o, PaintEventArgs e)
        {
            Tank t = o as Tank;

            // Determine the tank's color based on its ID
            if (t.ID < 8)
            {
                t.Color = t.ID;
            }
            else
            {
                // If the tank has an ID >= 8, assign it a random color
                Random rand = new Random();
                if (t.Color == 8)
                {
                    t.Color = rand.Next(8);
                }
            }

            e.Graphics.DrawImage(Turrets[t.Color], -25, -25, 50, 50);
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void ScoreAndNameDrawer(object o, PaintEventArgs e)
        {
            Tank t = o as Tank;

            // Draw health (do health as three dots where 3 = green, 2 = yellow, & 1 = red)
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (System.Drawing.SolidBrush greenBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Green))
            using (System.Drawing.SolidBrush yellowBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Yellow))
            using (System.Drawing.SolidBrush redBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Red))
            {
                Rectangle r1 = new Rectangle(-20, -45, 10, 10);
                Rectangle r2 = new Rectangle(-5, -45, 10, 10);
                Rectangle r3 = new Rectangle(10, -45, 10, 10);

                // Full health = 3 green dots
                if (t.Health == 3)
                {
                    e.Graphics.FillEllipse(greenBrush, r1);
                    e.Graphics.FillEllipse(greenBrush, r2);
                    e.Graphics.FillEllipse(greenBrush, r3);
                }

                // Medium health = 2 yellow dots
                if (t.Health == 2)
                {
                    e.Graphics.FillEllipse(yellowBrush, r1);
                    e.Graphics.FillEllipse(yellowBrush, r2);
                }

                // Low health = 1 red dot
                if (t.Health == 1)
                {
                    e.Graphics.FillEllipse(redBrush, r1);
                }
            }

            // Draw the player name and score
            using (Font font = new Font("Times New Roman", 16, FontStyle.Bold, GraphicsUnit.Pixel))
            {
                Point point = new Point(-30, 30);
                e.Graphics.DrawString(t.PlayerName + "   " + t.PlayerScore, font, Brushes.White, point);
            }
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void ProjectileDrawer(object o, PaintEventArgs e)
        {
            Projectile p = o as Projectile;

            // Determine what color to draw the projectile based on it's owner
            if (p.Owner < 8)
            {
                p.Color = p.Owner;
            }
            else
            {
                // If the projectile exists to a tank with ID == 8, choose a random color
                Random rand = new Random();
                if (p.Color == 8)
                {
                    p.Color = rand.Next(8);
                }
            }

            // Draw the projectile
            e.Graphics.DrawImage(Shots[p.Color], -15, -15, 30, 30);
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void PowerupDrawer(object o, PaintEventArgs e)
        {
            e.Graphics.DrawImage(Powerup, -12, -12, 24, 24);
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void BeamDrawer(object o, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (SolidBrush redBrush = new SolidBrush(Color.DarkRed))
            {
                Rectangle r = new Rectangle(-4, -550, 8, 1100);
                e.Graphics.FillRectangle(redBrush, r);
            }
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void WallDrawer(object o, PaintEventArgs e)
        {
            e.Graphics.DrawImage(Wall, -25, -25, 50, 50);
        }

        /// <summary>
        /// Helper method for drawing walls in OnPaint. Allows walls to 
        /// be drawn by calling DrawObjectWithTransform several times 
        /// as the wall coordinates are updated.
        /// </summary>
        /// <param name="w">The wall being drawn</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void DrawWall(Wall w, PaintEventArgs e)
        {
            // Get the starting and ending coordinates
            double p1X = w.P1.GetX();
            double p1Y = w.P1.GetY();
            double p2X = w.P2.GetX();
            double p2Y = w.P2.GetY();

            // If the wall is horizontal, determine which vertical coordinate is smaller
            // and use DrawObjectWithTransform as the vertical coordinate is advanced
            if (p1X == p2X)
            {
                if (p1Y < p2Y)
                {
                    while (p1Y <= p2Y)
                    {
                        DrawObjectWithTransform(e, w, theWorld.Size, p1X, p1Y, 0, WallDrawer);
                        p1Y += 50;
                    }
                }
                else
                {
                    while (p2Y <= p1Y)
                    {
                        DrawObjectWithTransform(e, w, theWorld.Size, p2X, p2Y, 0, WallDrawer);
                        p2Y += 50;
                    }
                }
            }

            // If the wall is vertical, determine which horizontal coordinate is smaller
            // and use DrawObjectWithTransform as the horizontal coordinate is advanced
            else
            {
                if (p1X < p2X)
                {
                    while (p1X <= p2X)
                    {
                        DrawObjectWithTransform(e, w, theWorld.Size, p1X, p1Y, 0, WallDrawer);
                        p1X += 50;
                    }
                }
                else
                {
                    while (p2X <= p1X)
                    {
                        DrawObjectWithTransform(e, w, theWorld.Size, p2X, p2Y, 0, WallDrawer);
                        p2X += 50;
                    }
                }
            }
        }

        /// <summary>
        /// This method is invoked when the DrawingPanel needs to be re-drawn
        /// </summary>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (Form1.GetConnected())
            {
                lock (theWorld)
                {
                    // Center the view of the world
                    double playerX = 0;
                    double playerY = 0;
                    if (theWorld.Tanks.ContainsKey(theWorld.PlayerID))
                    {
                        // Get the player's location
                        playerX = theWorld.Tanks[theWorld.PlayerID].Location.GetX();
                        playerY = theWorld.Tanks[theWorld.PlayerID].Location.GetY();

                        // Calculate view/world size ratio
                        double ratio = (double)(800) / (double)(theWorld.Size);
                        int halfSizeScaled = (int)(theWorld.Size / 2.0 * ratio);

                        // Translate the coordinates
                        double inverseTranslateX = -WorldSpaceToImageSpace(theWorld.Size, playerX) + halfSizeScaled;
                        double inverseTranslateY = -WorldSpaceToImageSpace(theWorld.Size, playerY) + halfSizeScaled;

                        // Apply transformation to graphics
                        e.Graphics.TranslateTransform((float)inverseTranslateX, (float)inverseTranslateY);
                    }
                    
                    // Draw the background
                    e.Graphics.DrawImage(Background, 0, 0, theWorld.Size, theWorld.Size);

                    // Draw the walls
                    foreach (Wall wall in theWorld.Walls.Values)
                    {
                        DrawWall(wall, e);
                    }

                    // Draw the tanks
                    foreach (Tank tank in theWorld.Tanks.Values)
                    {
                        DrawObjectWithTransform(e, tank, theWorld.Size, tank.Location.GetX(), tank.Location.GetY(), tank.Orientation.ToAngle(), TankDrawer);
                        DrawObjectWithTransform(e, tank, theWorld.Size, tank.Location.GetX(), tank.Location.GetY(), tank.Aim.ToAngle(), TurretDrawer);
                        DrawObjectWithTransform(e, tank, theWorld.Size, tank.Location.GetX(), tank.Location.GetY(), 0, ScoreAndNameDrawer);
                    }

                    // Draw the powerups
                    foreach (Powerup pow in theWorld.Powerups.Values)
                    {
                        DrawObjectWithTransform(e, pow, theWorld.Size, pow.Location.GetX(), pow.Location.GetY(), 0, PowerupDrawer);
                    }

                    // Draw the projectiles
                    foreach (Projectile proj in theWorld.Projectiles.Values)
                    {
                        // Ensure using direction is valid &, if so, be sure to normalize somewhere.
                        DrawObjectWithTransform(e, proj, theWorld.Size, proj.Location.GetX(), proj.Location.GetY(), proj.Direction.ToAngle(), ProjectileDrawer);
                    }

                    // Draw the beams
                    foreach (Beam beam in theWorld.Beams.Values)
                    {
                        // Ensure using direction is valid &, if so, be sure to normalize somewhere.
                        DrawObjectWithTransform(e, beam, theWorld.Size, beam.Origin.GetX(), beam.Origin.GetY(), beam.Direction.ToAngle(), BeamDrawer);
                    }

                    // Do anything that Panel (from which we inherit) needs to do
                    base.OnPaint(e);
                }
            }
        }
    }
}
