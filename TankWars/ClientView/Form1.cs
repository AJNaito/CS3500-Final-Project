using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TankWars;

namespace ClientView
{
    /// <summary>
    /// Class that represents the client's view
    /// </summary>
    public partial class Form1 : Form
    {
        /// <summary>
        /// Reference to GameController
        /// </summary>
        private GameController GC;

        /// <summary>
        /// Reference to the world
        /// </summary>
        private World world;

        /// <summary>
        /// Reference to the DrawingPanel
        /// </summary>
        private DrawingPanel draw;

        /// <summary>
        /// Sizes of the menu and the view of the game
        /// </summary>
        private const int menuSize = 50, viewSize = 900;

        /// <summary>
        /// Constructor that initializes the form, adds handlers to events, and initializes the fields
        /// </summary>
        /// <param name="game">GameController</param>
        public Form1(GameController game)
        {
            InitializeComponent();

            this.Size = new Size(1000, 900);

            GC = game;
            GC.Connection += HandleConnection;
            GC.Error += HandleError;
            GC.CmdArrived += CommandHandler;
            GC.BeamTriggered += BeamHandler;
            GC.TankKilled += DeathHandler;

            world = GC.GetWorld();

            draw = new DrawingPanel(world, GC);
            draw.Location = new Point(0, menuSize);
            draw.Size = new Size(viewSize, viewSize);
            this.Controls.Add(draw);

            draw.MouseMove += draw_MouseMove;
            draw.MouseDown += draw_MouseDown;
            draw.MouseUp += draw_MouseUp;
            this.KeyDown += KeyDownHandler;
            this.KeyUp += KeyUpHandler;
        }

        /// <summary>
        /// Handler upon connecting that sends the player name
        /// </summary>
        private void HandleConnection()
        {
            GC.SendCommands(PlayerName.Text + Environment.NewLine);
        }

        /// <summary>
        /// Handler for any connection errors and tries to reconnect
        /// </summary>
        /// <param name="msg">Error message</param>
        private void HandleError(string msg)
        {
            try
            {
                MethodInvoker invoker = new MethodInvoker(() => Reconnect(msg));
                Invoke(invoker);
            }
            catch { }
        }

        /// <summary>
        /// Method to try to reconnect
        /// </summary>
        /// <param name="msg">Error message</param>
        private void Reconnect(string msg)
        {
            DialogResult result = MessageBox.Show("Would you like to try to reconnect?", msg, MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                world.Clear();
                GC.Connect(ServerAddress.Text);
            }
            else
            {
                KeyPreview = false;

                ServerAddress.Enabled = true;
                PlayerName.Enabled = true;
                ConnectButton.Enabled = true;
            }
        }

        /// <summary>
        /// Handler for when keys are pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W)
                GC.HandleMoveRequest("up");
            else if (e.KeyCode == Keys.A)
                GC.HandleMoveRequest("left");
            else if (e.KeyCode == Keys.S)
                GC.HandleMoveRequest("down");
            else if (e.KeyCode == Keys.D)
                GC.HandleMoveRequest("right");

            e.SuppressKeyPress = true;
            e.Handled = true;
        }

        /// <summary>
        /// Handler for when keys are lifted
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KeyUpHandler(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W)
                GC.HandleCancelMoveRequest("up");
            else if (e.KeyCode == Keys.A)
                GC.HandleCancelMoveRequest("left");
            else if (e.KeyCode == Keys.S)
                GC.HandleCancelMoveRequest("down");
            else if (e.KeyCode == Keys.D)
                GC.HandleCancelMoveRequest("right");
        }

        /// <summary>
        /// Handler for when to animate beams
        /// </summary>
        /// <param name="b"></param>
        private void BeamHandler(World.Beams b)
        {
            draw.beamAnims.Add(new DrawingPanel.BeamAnimation(b));
        }

        /// <summary>
        /// Handler for when to animate deaths
        /// </summary>
        /// <param name="t"></param>
        private void DeathHandler(World.Tank t)
        {
            draw.deathAnims.Add(new DrawingPanel.DeathAnimation(t));
        }

        /// <summary>
        /// Handler for when the mouse buttons are pressed down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void draw_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                GC.HandleFireRequest("main");
            else if (e.Button == MouseButtons.Right)
                GC.HandleFireRequest("alt");
        }

        /// <summary>
        /// Handler for when mouse buttons are lifted
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void draw_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
                GC.HandleFireRequest("none");
        }

        /// <summary>
        /// Handler for when the mouse moves
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void draw_MouseMove(object sender, MouseEventArgs e)
        {
            GC.HandleMouseMoveRequest(e.X, e.Y);
        }

        /// <summary>
        /// Handler for when commands are received to redraw the DrawingPanel
        /// </summary>
        private void CommandHandler()
        {
            try
            {
                MethodInvoker invalidator = new MethodInvoker(() => Invalidate(true));
                Invoke(invalidator);
            }
            catch { }
        }

        /// <summary>
        /// Handler for when the connect button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectButton_Click(object sender, EventArgs e)
        {
            if (ServerAddress.Text.Length == 0)
                MessageBox.Show("Please input a server address");
            else
            {
                if (PlayerName.Text.Length >= 16)
                {
                    MessageBox.Show("Error", "Player Name can't exceed 16 characters", MessageBoxButtons.OK);
                }
                else
                {
                    GC.Connect(ServerAddress.Text);

                    KeyPreview = true;

                    ServerAddress.Enabled = false;
                    PlayerName.Enabled = false;
                    ConnectButton.Enabled = false;
                }
            }
        }
    }
}
