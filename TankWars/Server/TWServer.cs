using NetworkUtil;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Xml;
using TankWars;
using Newtonsoft.Json;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace Server
{
    /// <summary>
    /// Class that represents server side for TankWars game
    /// </summary>
    /// <author> Andrew Huang & Aidan Naito </author>
    class TWServer
    {
        /// <summary>
        /// Dictionary that stores the clients with their IDs
        /// </summary>
        static Dictionary<long, SocketState> clients;

        /// <summary>
        /// List to keep track of clients that disconnect
        /// </summary>
        static List<int> DisconnectedClients;

        /// <summary>
        /// The world
        /// </summary>
        static World world = new World();

        /// <summary>
        /// Int to note how many ms per frame
        /// </summary>
        static int MSPerFrame;

        /// <summary>
        /// Dictionary that stores commands
        /// </summary>
        static Dictionary<long, World.ControlCommands> commands;

        /// <summary>
        /// List that keeps track of beams
        /// </summary>
        static List<World.Beams> BeamsToSend;

        /// <summary>
        /// Main game loop
        /// 
        /// Updates world and sends to clients
        /// </summary>
        static void Main(string[] args)
        {
            TWServer server = new TWServer();

            Networking.StartServer(AcceptClient, 11000);

            Stopwatch watch = new Stopwatch();
            watch.Start();
            while (true)
            {
                while (watch.ElapsedMilliseconds < MSPerFrame)
                {

                }
                watch.Restart();

                Update();

                //Sending data to all clients
                List<int> ProjecileRemovals = new List<int>();
                lock (clients)
                {
                    foreach (SocketState ss in clients.Values)
                    {
                        lock (world)
                        {
                            foreach (World.Tank t in world.Players.Values)
                            {
                                string message = JsonConvert.SerializeObject(t);
                                Networking.Send(ss.TheSocket, message + "\n");
                            }
                            foreach (World.Powerups p in world.PowerUps.Values)
                            {
                                string message = JsonConvert.SerializeObject(p);
                                Networking.Send(ss.TheSocket, message + "\n");
                            }
                            foreach (World.Projectiles p in world.Projs.Values)
                            {
                                string message = JsonConvert.SerializeObject(p);
                                Networking.Send(ss.TheSocket, message + "\n");
                            }
                            foreach (World.Beams b in BeamsToSend)
                            {
                                string message = JsonConvert.SerializeObject(b);
                                Networking.Send(ss.TheSocket, message + "\n");
                            } 
                        }
                    }
                    lock (world)
                    {
                        foreach (World.Tank t in world.Players.Values)
                        {
                            if (t.IsDead())
                                t.SetDead(false);
                        }
                        foreach (World.Projectiles p in world.Projs.Values)
                        {
                            if (p.IsDead())
                                ProjecileRemovals.Add(p.GetID());
                        }
                        foreach (int ID in ProjecileRemovals)
                        {
                            world.Projs.Remove(ID);
                        }
                        List<int> PowerupRemovals = new List<int>();
                        foreach (World.Powerups p in world.PowerUps.Values)
                        {
                            if (p.IsDead())
                                PowerupRemovals.Add(p.GetID());
                        }
                        foreach (int i in PowerupRemovals)
                        {
                            world.PowerUps.Remove(i);
                        } 
                    }
                    lock (DisconnectedClients)
                    {
                        foreach (int i in DisconnectedClients)
                        {
                            world.Players.Remove(i);
                        }

                        DisconnectedClients.Clear();
                    }
                    BeamsToSend.Clear();
                }
            }
        }

        /// <summary>
        /// Constructor for server -- reads XML settings file
        /// </summary>
        public TWServer()
        {
            clients = new Dictionary<long, SocketState>();
            DisconnectedClients = new List<int>();
            commands = new Dictionary<long, World.ControlCommands>();
            BeamsToSend = new List<World.Beams>();

            using (XmlReader read = XmlReader.Create(@"..\..\..\..\Resources\Settings.xml"))
            {
                while(read.Read())
                {
                    if (read.IsStartElement())
                    {
                        switch (read.Name)
                        {
                            case "WorldSize":
                                read.Read();
                                world.WorldSpace = int.Parse(read.Value);
                                break;
                            case "MSPerFrame":
                                read.Read();
                                MSPerFrame = int.Parse(read.Value);
                                break;
                            case "FramesPerShot":
                                read.Read();
                                World.Tank.FireMaxFrameCount = int.Parse(read.Value);
                                break;
                            case "RespawnRate":
                                read.Read();
                                World.Tank.RespawnMaxFrameCount = int.Parse(read.Value);
                                break;
                            case "Wall":
                                read.Read();
                                //moves reader to <x>
                                while (!read.IsStartElement())
                                {
                                    read.Read();
                                }
                                read.Read();
                                Vector2D start = ReadWall(read);
                                read.ReadEndElement();

                                //moves reader to <y>
                                while (!read.IsStartElement())
                                {
                                    read.Read();
                                }
                                read.Read();
                                Vector2D end = ReadWall(read);
                                read.ReadEndElement();

                                World.Walls w = new World.Walls(start, end);
                                world.Wall.Add(w.GetID(), w);
                                break;
                            case "MaxPower":
                                read.Read();
                                world.PowerUpMaxSet(int.Parse(read.Value));
                                break;
                            case "MaxPowerDelay":
                                read.Read();
                                world.SetPowerUpDelay(int.Parse(read.Value));
                                break;
                            case "InvincibilityFrames":
                                read.Read();
                                World.Tank.InvincibleMaxFrame = int.Parse(read.Value);
                                break;
                            case "ScoreDecrement":
                                read.Read();
                                if (read.Value == "false")
                                    world.ScoreDecrement = false;
                                else if (read.Value == "true")
                                    world.ScoreDecrement = true;
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Read wall data from XML setting file
        /// </summary>
        /// <param name="read"> XML reader </param>
        /// <returns> Vector2D location from settings </returns>
        private static Vector2D ReadWall(XmlReader read)
        {
            int x; int y;

            read.ReadStartElement("x");
            x = int.Parse(read.Value);
            read.Read();
            read.ReadEndElement();


            read.ReadStartElement("y");
            y = int.Parse(read.Value);
            read.Read();
            read.ReadEndElement();

            return new Vector2D(x, y);
        }

        /// <summary>
        /// Accept client from TCP listener
        /// </summary>
        /// <param name="state"> new client </param>
        private static void AcceptClient(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                return;
            }

            state.OnNetworkAction = SendStartInfo;
            Networking.GetData(state);
        }

        /// <summary>
        /// Sends the start up info the current client.
        /// 
        /// If any thing goes wrong during this process, it is not added to our list of clients
        /// </summary>
        /// <param name="state"> new client </param>
        private static void SendStartInfo(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                return;
            }

            string totalData = state.GetData();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");

            World.Tank player = new World.Tank((int)state.ID, parts[0], 3);
            state.RemoveData(0, parts[0].Length);
            lock (world)
            {
                world.Players.Add((int)state.ID, player);
            }

            Networking.Send(state.TheSocket, "" + state.ID + "\n" + world.WorldSpace + "\n");
            foreach (World.Walls w in world.Wall.Values)
            {
                string message = JsonConvert.SerializeObject(w);
                Networking.Send(state.TheSocket, message + "\n");
            }

            lock (clients)
            {
                clients.Add(state.ID, state);
            }

            world.SpawnTank(player);

            state.OnNetworkAction = ReceiveCommands;
            Networking.GetData(state);
        }

        /// <summary>
        /// Start to recieve commands from the client
        /// </summary>
        /// <param name="state"> client sending stuff </param>
        private static void ReceiveCommands(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                Disconnect(state);
                return;
            }

            ProcessCommands(state);
            Networking.GetData(state);
        }

        /// <summary>
        /// Process the client's commands
        /// </summary>
        /// <param name="state"> client </param>
        private static void ProcessCommands(SocketState state)
        {
            string totalData = state.GetData();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");
            foreach (string part in parts)
            {
                if (part.Length == 0)
                    continue;

                if (part[part.Length - 1] != '\n')
                    break;

                lock (commands)
                {
                    try
                    {
                        World.ControlCommands c = JsonConvert.DeserializeObject<World.ControlCommands>(part);
                        if (!commands.ContainsKey(state.ID))
                            commands.Add(state.ID, c);

                        //Prioritizes beam command so it's not missed if receiving multiple commands per frame
                        if (c.GetFire() == "alt")
                            commands[state.ID] = c;
                    }
                    catch (Exception)
                    {

                    }
                }
                state.RemoveData(0, part.Length);
            }
        }

        /// <summary>
        /// Method that handles disconnections
        /// </summary>
        /// <param name="state">Client that disconnected</param>
        private static void Disconnect(SocketState state)
        {
            lock (clients)
            {
                lock (DisconnectedClients)
                {
                    DisconnectedClients.Add((int)state.ID);

                    clients.Remove(state.ID);
                } 
            }
        }

        /// <summary>
        /// Updates the world according to the commands
        /// </summary>
        private static void Update()
        {
            lock (commands)
            {
                foreach (KeyValuePair<long, World.ControlCommands> c in commands)
                {
                    lock (world)
                    {
                        //Changes the tank's properties according to the inputted commands
                        World.Tank t = world.Players[(int)c.Key];
                        Vector2D currentLoc = t.GetPosition();

                        if (t.GetHP() == 0)
                            continue;

                        t.ChangeDirAndVel(c.Value.GetDir());

                        t.SetAim(c.Value.GetTurDir());

                        world.MoveTank(t);

                        switch (c.Value.GetFire())
                        {
                            case "main":
                                if (t.CanFire())
                                {
                                    world.FireProjectile(t);
                                }
                                break;
                            case "alt":
                                if (t.CanFireBeam())
                                {
                                    world.FireBeam(t, BeamsToSend);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
                commands.Clear();
            }

            lock (world)
            {
                //Update the world
                foreach (World.Projectiles p in world.Projs.Values)
                {
                    world.MoveProjectile(p);
                }

                foreach (World.Tank tank in world.Players.Values)
                {
                    //Handles when a tank disconnects
                    if (DisconnectedClients.Contains(tank.GetID()))
                    {
                        tank.SetDC(true);
                        world.TankDied(tank);
                        continue;
                    }

                    //Handles Respawn
                    if (tank.GetHP() == 0)
                    {
                        if (tank.CanRespawn())
                            world.SpawnTank(tank);
                        else
                            tank.MoveDeathFrame();
                    }

                    //Increment invincibility frames if still invincibile
                    if (tank.IsInvincible())
                        tank.IncrementInvincCount();

                    tank.MoveFireFrame();
                }

                //Handles world power up spawning
                if (!world.PowerUpsAtMax())
                {
                    if (world.CanRespawn())
                    {
                        world.SpawnPowerup();
                    }
                    else
                    {
                        world.DecrementDelayCount();
                    }
                }
            }
        }
    }
}
