using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TankWars
{
    /// <summary>
    /// Class that represents a DrawingPanel
    /// </summary>
    public class DrawingPanel : Panel
    {
        /// <summary>
        /// Reference to the world
        /// </summary>
        private World theWorld;

        /// <summary>
        /// Dictionary that stores sprite images
        /// </summary>
        private Dictionary<string, Image> images;

        /// <summary>
        /// Reference to the GameController
        /// </summary>
        private GameController GC;

        /// <summary>
        /// List of beam animations currently being animated
        /// </summary>
        public List<BeamAnimation> beamAnims
        {
            get; private set;
        }

        /// <summary>
        /// List of death animations currently being animated
        /// </summary>
        public List<DeathAnimation> deathAnims
        {
            get; private set;
        }

        /// <summary>
        /// Constructor that initialized fields and sets the background image
        /// </summary>
        /// <param name="w">World</param>
        /// <param name="gc">GameController</param>
        public DrawingPanel(World w, GameController gc)
        {
            DoubleBuffered = true;
            theWorld = w;
            images = new Dictionary<string, Image>();
            images.Add("Background", GetImage("Background.png"));
            GC = gc;
            beamAnims = new List<BeamAnimation>();
            deathAnims = new List<DeathAnimation>();

            this.BackColor = Color.Black;
        }


        // A delegate for DrawObjectWithTransform
        // Methods matching this delegate can draw whatever they want using e  
        public delegate void ObjectDrawer(object o, PaintEventArgs e);


        /// <summary>
        /// This method performs a translation and rotation to drawn an object in the world.
        /// </summary>
        /// <param name="e">PaintEventArgs to access the graphics (for drawing)</param>
        /// <param name="o">The object to draw</param>
        /// <param name="worldX">The X coordinate of the object in world space</param>
        /// <param name="worldY">The Y coordinate of the object in world space</param>
        /// <param name="angle">The orientation of the objec, measured in degrees clockwise from "up"</param>
        /// <param name="drawer">The drawer delegate. After the transformation is applied, the delegate is invoked to draw whatever it wants</param>
        private void DrawObjectWithTransform(PaintEventArgs e, object o, double worldX, double worldY, double angle, ObjectDrawer drawer)
        {
            // "push" the current transform
            System.Drawing.Drawing2D.Matrix oldMatrix = e.Graphics.Transform.Clone();

            e.Graphics.TranslateTransform((int)worldX, (int)worldY);
            e.Graphics.RotateTransform((float)angle);
            drawer(o, e);

            // "pop" the transform
            e.Graphics.Transform = oldMatrix;
        }

        /// <summary>
        /// Helper method that gets and draws an image
        /// </summary>
        /// <param name="imageName">Name of image</param>
        /// <param name="width">Width of drawing</param>
        /// <param name="height">Height of drawing</param>
        /// <param name="e"></param>
        private void DrawCurrentImage(string imageName, int width, int height, PaintEventArgs e)
        {
            if (!images.TryGetValue(imageName, out Image image))
            {
                image = GetImage(imageName + ".png");
                images.Add(imageName, image);
            }
            e.Graphics.DrawImage(image, -width / 2, -height / 2, width, height);
        }

        /// <summary>
        /// Method that draws a tank
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void TankDrawer(object o, PaintEventArgs e)
        {
            World.Tank t = o as World.Tank;

            string tankType;
            switch (t.GetID() % 8)
            {
                case 0:
                    tankType = "BlueTank";
                    break;
                case 1:
                    tankType = "DarkTank";
                    break;
                case 2:
                    tankType = "GreenTank";
                    break;
                case 3:
                    tankType = "LightGreenTank";
                    break;
                case 4:
                    tankType = "OrangeTank";
                    break;
                case 5:
                    tankType = "PurpleTank";
                    break;
                case 6:
                    tankType = "RedTank";
                    break;
                default:
                    tankType = "YellowTank";
                    break;
            }
            DrawCurrentImage(tankType, 60, 60, e);
        }

        /// <summary>
        /// Method that draws a turret
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void TurretDrawer(object o, PaintEventArgs e)
        {
            World.Tank t = o as World.Tank;

            string turType;
            switch (t.GetID() % 8)
            {
                case 0:
                    turType = "BlueTurret";
                    break;
                case 1:
                    turType = "DarkTurret";
                    break;
                case 2:
                    turType = "GreenTurret";
                    break;
                case 3:
                    turType = "LightGreenTurret";
                    break;
                case 4:
                    turType = "OrangeTurret";
                    break;
                case 5:
                    turType = "PurpleTurret";
                    break;
                case 6:
                    turType = "RedTurret";
                    break;
                default:
                    turType = "YellowTurret";
                    break;
            }

            DrawCurrentImage(turType, 50, 50, e);
        }

        /// <summary>
        /// Method that draws a single wall sprite
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void WallDrawer(object o, PaintEventArgs e)
        {
            DrawCurrentImage("WallSprite", 50, 50, e);
        }

        /// <summary>
        /// Method that dras a projectile
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void ProjectileDrawer(object o, PaintEventArgs e)
        {
            World.Projectiles p = o as World.Projectiles;

            string shotType;
            switch (p.GetOwnerId() % 8)
            {
                case 0:
                    shotType = "shot-blue";
                    break;
                case 1:
                    shotType = "shot-brown";
                    break;
                case 2:
                    shotType = "shot-green";
                    break;
                case 3:
                    shotType = "shot-white";
                    break;
                case 4:
                    shotType = "shot-grey";
                    break;
                case 5:
                    shotType = "shot-violet";
                    break;
                case 6:
                    shotType = "shot-red";
                    break;
                default:
                    shotType = "shot-yellow";
                    break;
            }

            DrawCurrentImage(shotType, 30, 30, e);
        }

        /// <summary>
        /// Method that draws a powerup
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void PowerUpsDrawer(object o, PaintEventArgs e)
        {
            using (System.Drawing.SolidBrush blackBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black))
            using (System.Drawing.SolidBrush whiteBrush = new System.Drawing.SolidBrush(System.Drawing.Color.White))
            {
                Rectangle r1 = new Rectangle(-5, -5, 10, 10);
                Rectangle r2 = new Rectangle(-8, -8, 16, 16);

                e.Graphics.FillEllipse(whiteBrush, r2);
                e.Graphics.FillEllipse(blackBrush, r1);
            }
        }

        /// <summary>
        /// Method that draws the player info under a tank and the health bar
        /// </summary>
        /// <param name="t"></param>
        /// <param name="e"></param>
        private void DrawPlayerInfoAndHealthBar(World.Tank t, PaintEventArgs e)
        {
            using (System.Drawing.SolidBrush whiteBrush = new System.Drawing.SolidBrush(System.Drawing.Color.White))
            using (System.Drawing.SolidBrush greenBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Green))
            using (System.Drawing.SolidBrush yellowBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Yellow))
            using (System.Drawing.SolidBrush redBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Red))
            {
                // Player name and score
                StringFormat StringFormat = new StringFormat();
                StringFormat.Alignment = StringAlignment.Center;
                StringFormat.LineAlignment = StringAlignment.Center;
                e.Graphics.DrawString(t.GetName() + ": " + t.GetScore(), DefaultFont, whiteBrush, new Point((int)t.GetPlayerX(), (int)t.GetPlayerY() + 40), StringFormat);
                
                // Health bar
                switch (t.GetHP())
                {
                    case 3:
                        Rectangle r1 = new Rectangle((int)t.GetPlayerX() - 25, (int)t.GetPlayerY() - 40, 45, 5);
                        e.Graphics.FillRectangle(greenBrush, r1);
                        break;
                    case 2:
                        Rectangle r2 = new Rectangle((int)t.GetPlayerX() - 25, (int)t.GetPlayerY() - 40, 30, 5);
                        e.Graphics.FillRectangle(yellowBrush, r2);
                        break;
                    case 1:
                        Rectangle r3 = new Rectangle((int)t.GetPlayerX() - 25, (int)t.GetPlayerY() - 40, 15, 5);
                        e.Graphics.FillRectangle(redBrush, r3);
                        break;
                }
            }
        }

        // This method is invoked when the DrawingPanel needs to be re-drawn
        protected override void OnPaint(PaintEventArgs e)
        {
            if (theWorld.WorldSpace == 0)
                return;
            lock (theWorld)
            {
                int viewSize = 900; // view is square, so we can just use width

                theWorld.Players.TryGetValue(GC.ClientID, out World.Tank playerTank);
                if (playerTank == null)
                    return;

                double playerX = playerTank.GetPlayerX();
                double playerY = playerTank.GetPlayerY();
                e.Graphics.TranslateTransform((float)-playerX + (viewSize / 2), (float)-playerY + (viewSize / 2));

                images.TryGetValue("Background", out Image background);
                e.Graphics.DrawImage(background, -theWorld.WorldSpace / 2, -theWorld.WorldSpace / 2, theWorld.WorldSpace, theWorld.WorldSpace);

                foreach (World.Tank t in theWorld.Players.Values)
                {
                    if (t.GetHP() == 0)
                        continue;
                    DrawObjectWithTransform(e, t, t.GetPlayerX(), t.GetPlayerY(), t.GetPlayerDir().ToAngle(), TankDrawer);

                    DrawPlayerInfoAndHealthBar(t, e);

                    DrawObjectWithTransform(e, t, t.GetPlayerX(), t.GetPlayerY(), t.GetAim().ToAngle(), TurretDrawer);
                }

                foreach (World.Powerups p in theWorld.PowerUps.Values)
                {
                    DrawObjectWithTransform(e, p, p.GetX(), p.GetY(), 0, PowerUpsDrawer);
                }

                foreach (World.Projectiles proj in theWorld.Projs.Values)
                {
                    DrawObjectWithTransform(e, proj, proj.GetX(), proj.GetY(), proj.GetOrientation().ToAngle(), ProjectileDrawer);
                }

                foreach (World.Walls wall in theWorld.Wall.Values)
                {
                    // Drawing walls horizontally
                    if (wall.StartX() != wall.EndX())
                    {
                        if (wall.StartX() < wall.EndX())
                            for (double current = wall.StartX(); current <= wall.EndX(); current += 50)
                            {
                                DrawObjectWithTransform(e, wall, current, wall.EndY(), 0, WallDrawer);
                            }
                        else
                            for (double current = wall.StartX(); current >= wall.EndX(); current -= 50)
                            {
                                DrawObjectWithTransform(e, wall, current, wall.EndY(), 0, WallDrawer);
                            }
                    }
                    // Drawing walls vertically
                    else
                    {
                        if (wall.StartY() < wall.EndY())
                            for (double current = wall.StartY(); current <= wall.EndY(); current += 50)
                            {
                                DrawObjectWithTransform(e, wall, wall.EndX(), current, 0, WallDrawer);
                            }
                        else
                            for (double current = wall.StartY(); current >= wall.EndY(); current -= 50)
                            {
                                DrawObjectWithTransform(e, wall, wall.EndX(), current, 0, WallDrawer);
                            }
                    }
                }

                for (int i = 0; i < deathAnims.Count; i ++)
                {
                    DeathAnimation da = deathAnims[i];
                    da.PlayFrame(e);

                    if (da.frame == 10)
                    {
                        deathAnims.RemoveAt(i);
                        i--;
                    }
                }

                for (int i = 0; i < beamAnims.Count; i++)
                {
                    BeamAnimation ba = beamAnims[i];
                    ba.DrawFrame(e, theWorld.WorldSpace);

                    if (ba.frame == 15)
                    {
                        beamAnims.RemoveAt(i);
                        i--;
                    }
                }
            }

            // Do anything that Panel (from which we inherit) needs to do
            base.OnPaint(e);
        }

        /// <summary>
        /// Gets an image from a file
        /// </summary>
        /// <param name="filename">Name of file</param>
        /// <returns>Image</returns>
        private Image GetImage(string filename)
        {
            return Image.FromFile(@"..\..\..\Resources\Sprites\" + filename);
        }

        /// <summary>
        /// Class that represents a beam animation
        /// </summary>
        public class BeamAnimation
        {
            /// <summary>
            /// Current frame
            /// </summary>
            public int frame = 0;

            /// <summary>
            /// Beam that is being animated
            /// </summary>
            public World.Beams beam
            {
                get; private set;
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="b">Beam to be animated</param>
            public BeamAnimation(World.Beams b)
            {
                beam = b;
            }

            /// <summary>
            /// Draws the current frame
            /// </summary>
            /// <param name="e"></param>
            /// <param name="WorldSize">Size of the world to determine how long to draw the beam</param>
            public void DrawFrame(PaintEventArgs e, int WorldSize)
            {
                using (System.Drawing.Pen pen = new System.Drawing.Pen(Color.FromArgb(255 - (10 * frame), Color.White) , 1.0f + (frame/2)))
                {
                    e.Graphics.DrawLine(pen, new Point((int)beam.GetX(), (int)beam.GetY()), new Point((int)(beam.GetX() +
                        (beam.GetDirX() * WorldSize * 2)), (int)(beam.GetY() + (beam.GetDirY() * WorldSize * 2))));
                }

                frame++;
            }
        }

        /// <summary>
        /// Class that represents a death animation
        /// </summary>
        public class DeathAnimation
        {
            /// <summary>
            /// Current frame
            /// </summary>
            public int frame = 0;

            /// <summary>
            /// Array of rectangles that represent debris
            /// </summary>
            private Rectangle[] debris;

            /// <summary>
            /// Array of directions for each rectangle
            /// </summary>
            private Vector2D[] directions;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="t">Tank that died</param>
            public DeathAnimation(World.Tank t)
            {
                debris = new Rectangle[10];
                directions = new Vector2D[10];
                Random r = new Random();

                for (int i = 0; i < debris.Length; i ++)
                {
                    debris[i] = new Rectangle((int)t.GetPlayerX(), (int)t.GetPlayerY(), 5, 5);
                    directions[i] = new Vector2D(r.NextDouble() * 2 - 1, r.NextDouble() * 2 - 1);
                    directions[i].Normalize();
                }
            }

            /// <summary>
            /// Draws the current frame
            /// </summary>
            /// <param name="e"></param>
            public void PlayFrame(PaintEventArgs e)
            {
                using (System.Drawing.SolidBrush brush = new System.Drawing.SolidBrush(Color.Gray))
                {
                    for (int i = 0; i < debris.Length; i++) 
                    {
                        Rectangle curRec = debris[i];
                        curRec.Location = new Point((int)(curRec.X + (frame * 5 * directions[i].GetX())), (int)(curRec.Y + (frame * 5 * directions[i].GetY())));

                        e.Graphics.FillEllipse(brush, curRec);
                    }
                }

                frame++;
            }
        }
    }
}

