using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NetworkUtil;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TankWars;

namespace TankWars
{
    /// <summary>
    /// Class that represents the controller for TankWars
    /// </summary>
    public class GameController
    {
        // Delegates for all the handlers
        public delegate void CmdHandler(); 
        public delegate void ConnectionHandler();
        public delegate void ErrorHandler(string err);
        public delegate void StartUp();
        public delegate void BeamFired(World.Beams beam);
        public delegate void TankDied(World.Tank tank);

        // Events for the handlers
        public event CmdHandler CmdArrived;
        public event ConnectionHandler Connection;
        public event ErrorHandler Error;
        public event BeamFired BeamTriggered;
        public event TankDied TankKilled;

        /// <summary>
        /// SocketState to represent the server
        /// </summary>
        private SocketState theServer;

        /// <summary>
        /// The world
        /// </summary>
        private World world;

        /// <summary>
        /// List of directional commands
        /// </summary>
        private List<string> inputInstr;

        /// <summary>
        /// ID of the player
        /// </summary>
        public int ClientID
        {
            get; set;
        }

        /// <summary>
        /// Movement direction instruction
        /// </summary>
        private string movingPressed = "none";

        /// <summary>
        /// Fire instruction
        /// </summary>
        private string mousePressed = "none";

        /// <summary>
        /// Bool used to prevent firing multiple beams at once
        /// True when a beam is fired, false once right click is lifted
        /// </summary>
        private bool beamProcessed = false;

        /// <summary>
        /// Mouse direction instruction
        /// </summary>
        private Vector2D mouseDir = new Vector2D();

        /// <summary>
        /// Constructor
        /// </summary>
        public GameController()
        {
            world = new World();

            inputInstr = new List<string>();
        }

        /// <summary>
        /// Getter for the world
        /// </summary>
        /// <returns>The world</returns>
        public World GetWorld()
        {
            return world;
        }

        /// <summary>
        /// Method to connect to the server
        /// </summary>
        /// <param name="address">Server address</param>
        public void Connect(string address)
        {
            Networking.ConnectToServer(ConnectionMade, address, 11000);
        }

        /// <summary>
        /// Method for when connected to the server
        /// </summary>
        /// <param name="state">SocketState with the server</param>
        private void ConnectionMade(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                Error("Error Occurred while making a connection");
                return;
            }

            theServer = state;

            Connection();

            state.OnNetworkAction = StartUpInfo;
            Networking.GetData(state);
        }

        /// <summary>
        /// Method for when commands are received
        /// </summary>
        /// <param name="state">SocketState for the server</param>
        private void ReceiveCommands(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                Error("Connection lost to server");
                return;
            }

            ProcessCommands(state);
            Networking.GetData(state);
        }

        /// <summary>
        /// Method for sending commands
        /// </summary>
        /// <param name="message">Message to be sent</param>
        public void SendCommands(string message)
        {
            if (theServer != null)
            {
                Networking.Send(theServer.TheSocket, message);
            }
        }

        /// <summary>
        /// Method for receiving startup info
        /// </summary>
        /// <param name="state">SocketState for the server</param>
        private void StartUpInfo(SocketState state)
        {
            string totalData = state.GetData();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");

            //First string should be player ID
            ClientID = int.Parse(parts[0]);

            //Second should be size of world
            world.WorldSpace = int.Parse(parts[1]);

            state.RemoveData(0, parts[0].Length + parts[1].Length);

            state.OnNetworkAction = ReceiveCommands;
            Networking.GetData(state);
        }

        /// <summary>
        /// Method for processing commands
        /// </summary>
        /// <param name="state">SocketState for the server</param>
        private void ProcessCommands(SocketState state)
        {
            string totalData = state.GetData();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");
            foreach (string part in parts)
            {
                if (part.Length == 0)
                    continue;

                if (part[part.Length - 1] != '\n')
                    break;

                JObject obj = JObject.Parse(part);
                lock (world)
                {
                    if (obj["tank"] != null)
                    {
                        World.Tank t = JsonConvert.DeserializeObject<World.Tank>(part);

                        if (world.Players.ContainsKey(t.GetID()))
                        {
                            world.Players[t.GetID()] = t;
                        }
                        else
                        {
                            world.Players.Add(t.GetID(), t);
                        }

                        if (t.IsDead())
                            TankKilled(t);
                        
                        if (t.IsDisconnected())
                        {
                            world.Players.Remove(t.GetID());
                            TankKilled(t);
                        }
                    }
                    else if (obj["proj"] != null)
                    {
                        World.Projectiles p = JsonConvert.DeserializeObject<World.Projectiles>(part);

                        if (world.Projs.ContainsKey(p.GetID()))
                        {
                            world.Projs[p.GetID()] = p;
                        }
                        else
                        {
                            world.Projs.Add(p.GetID(), p);
                        }

                        if (p.IsDead())
                            world.Projs.Remove(p.GetID());
                    }
                    else if (obj["beam"] != null)
                    {
                        BeamTriggered(JsonConvert.DeserializeObject<World.Beams>(part));
                    }
                    else if (obj["power"] != null)
                    {
                        World.Powerups p = JsonConvert.DeserializeObject<World.Powerups>(part);

                        if (world.PowerUps.ContainsKey(p.GetID()))
                        {
                            world.PowerUps[p.GetID()] = p;
                        }
                        else
                        {
                            world.PowerUps.Add(p.GetID(), p);
                        }

                        if (p.IsDead())
                            world.PowerUps.Remove(p.GetID());
                    }
                    else if (obj["wall"] != null)
                    {
                        World.Walls w = JsonConvert.DeserializeObject<World.Walls>(part);
                        world.Wall.Add(w.GetID(), w);
                    }
                }
                state.RemoveData(0, part.Length);
            }
            CmdArrived();

            ProcessInputs();
        }

        /// <summary>
        /// Method for processing inputs
        /// </summary>
        public void ProcessInputs()
        {
            World.ControlCommands commands;
            if (mousePressed == "none")
                beamProcessed = false;
            if (mousePressed == "alt" && beamProcessed)
                commands = new World.ControlCommands(movingPressed, "none", mouseDir);
            else
                commands = new World.ControlCommands(movingPressed, mousePressed, mouseDir);
            if (mousePressed == "alt")
                beamProcessed = true;
            string message = JsonConvert.SerializeObject(commands);
            
            Networking.Send(theServer.TheSocket, message + Environment.NewLine);
        }

        /// <summary>
        /// Method for movement that sets the most recent input to be sent to the server
        /// </summary>
        /// <param name="direction">Direction of movement</param>
        public void HandleMoveRequest(string direction)
        {
            if (!inputInstr.Contains(direction))
                inputInstr.Add(direction);

            movingPressed = inputInstr[inputInstr.Count - 1];
        }

        /// <summary>
        /// Method for when a movement is cancelled
        /// </summary>
        /// <param name="direction">Direction of movement</param>
        public void HandleCancelMoveRequest(string direction)
        {
            inputInstr.Remove(direction);

            if (inputInstr.Count == 0)
                movingPressed = "none";
            else
                movingPressed = inputInstr[inputInstr.Count - 1];
        }

        /// <summary>
        /// Method for the type of fire
        /// </summary>
        /// <param name="type">Fire type</param>
        public void HandleFireRequest(string type)
        {
            mousePressed = type;
        }

        /// <summary>
        /// Method used to set the direction of the turret using mouse position
        /// </summary>
        /// <param name="x">x position of mouse</param>
        /// <param name="y">y position of mouse</param>
        public void HandleMouseMoveRequest(double x, double y)
        {
            mouseDir = new Vector2D(x - 450, y - 450);
            mouseDir.Normalize();
        }
    }
}
