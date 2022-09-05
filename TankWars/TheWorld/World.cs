using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TankWars
{
    /// <summary>
    /// Model class to store data for TankWars
    /// </summary>
    public class World
    {
        /// <summary>
        /// Dictionary that keeps track of all the tanks
        /// </summary>
        public Dictionary<int, Tank> Players
        {
            get; private set;
        }

        /// <summary>
        /// Dictionary that keeps track of all the powerups
        /// </summary>
        public Dictionary<int, Powerups> PowerUps
        {
            get; private set;
        }

        /// <summary>
        /// Dictionary that keeps track of all the walls
        /// </summary>
        public Dictionary<int, Walls> Wall
        {
            get; private set;
        }

        /// <summary>
        /// Dictionary that keeps track of all the projectiles
        /// </summary>
        public Dictionary<int, Projectiles> Projs
        {
            get; private set;
        }

        /// <summary>
        /// Constructor that initializes the dictionaries and randomly generates the wait time for a new powerup
        /// </summary>
        public World()
        {
            Players = new Dictionary<int, Tank>();
            PowerUps = new Dictionary<int, Powerups>();
            Wall = new Dictionary<int, Walls>();
            Projs = new Dictionary<int, Projectiles>();

            CurrentDelay = rng.Next(MaxDelay);
        }

        /// <summary>
        /// Sets the max power ups that can be spawned in the world -- should be in settings
        /// </summary>
        /// <param name="max"></param>
        public void PowerUpMaxSet(int max)
        {
            MaxPowerUps = max;
        }

        /// <summary>
        /// Sets the max power up spawn delay -- should be in settings
        /// </summary>
        /// <param name="delay"></param>
        public void SetPowerUpDelay(int delay)
        {
            MaxDelay = delay;

            CurrentDelay = rng.Next(MaxDelay);
        }

        /// <summary>
        /// Int that represents the size of the world
        /// </summary>
        public int WorldSpace
        {
            get; set;
        }

        /// <summary>
        /// Int the represents the respawn rate of the world
        /// </summary>
        public int RespawnRate
        {
            get; set;
        }

        public bool ScoreDecrement
        {
            get; set;
        }

        // Spawn fields for power ups
        private int MaxPowerUps;
        private int MaxDelay;
        private int CurrentDelay;
        private Random rng = new Random();

        // Spawning Methods for Power up

        /// <summary>
        /// If the world can spawn another power up
        /// </summary>
        /// <returns></returns>
        public bool CanRespawn()
        {
            return CurrentDelay == 0;
        }

        /// <summary>
        /// Decreases the current delay by 1 frame
        /// </summary>
        public void DecrementDelayCount()
        {
            CurrentDelay--;
        }

        /// <summary>
        /// Resets the current delay after power up spawns
        /// </summary>
        public void ResetDelayCount()
        {
            CurrentDelay = rng.Next(MaxDelay);
        }

        /// <summary>
        /// Checks if the world can spawn another power up
        /// </summary>
        /// <returns> true if current power ups not at max </returns>
        public bool PowerUpsAtMax()
        {
            return PowerUps.Count == MaxPowerUps;
        }

        /// <summary>
        /// Method to clear the dictionaries
        /// </summary>
        public void Clear()
        {
            Players = new Dictionary<int, Tank>();
            PowerUps = new Dictionary<int, Powerups>();
            Wall = new Dictionary<int, Walls>();
            Projs = new Dictionary<int, Projectiles>();
        }

        /// <summary>
        /// Moves the current tank in the world
        /// </summary>
        /// <param name="t"> Current tank </param>
        public void MoveTank(Tank t)
        {
            Vector2D newLoc = t.GetPosition() + t.velocity;
            bool collision = false;
            //Checks collisions
            foreach (Walls wall in Wall.Values)
            {
                if (wall.CollidesTank(newLoc))
                {
                    collision = true;
                    t.velocity = new Vector2D(0, 0);
                    break;
                }
            }

            foreach (Powerups p in PowerUps.Values)
            {
                if (t.TankCollidesPowerup(p.GetLocation()))
                {
                    t.SetPowerUpCounter(t.GetPowerUpCount() + 1);
                    p.SetDead(true);
                }
            }

            //Wrap around logic
            if (newLoc.GetX() < -WorldSpace / 2)
                newLoc = new Vector2D(newLoc.GetX() * -1 - 5, newLoc.GetY());
            else if (newLoc.GetX() > WorldSpace / 2)
                newLoc = new Vector2D(newLoc.GetX() * -1 + 5, newLoc.GetY());
            if (newLoc.GetY() < -WorldSpace / 2 + 5)
                newLoc = new Vector2D(newLoc.GetX(), newLoc.GetY() * -1 - 5);
            else if (newLoc.GetY() > WorldSpace / 2 - 5)
                newLoc = new Vector2D(newLoc.GetX(), newLoc.GetY() * -1 + 5);

            if (!collision)
                t.SetLocation(newLoc);
        }

        /// <summary>
        /// Spawns a tank at a random location that isn't in a wall or powerup
        /// </summary>
        /// <param name="tank"> tank being spawned </param>
        public void SpawnTank(Tank tank)
        {
            Random rand = new Random();
            Vector2D newLoc = new Vector2D(rand.Next(-WorldSpace / 2, WorldSpace / 2), rand.Next(-WorldSpace / 2, WorldSpace / 2));
            Tank temp = new Tank();
            temp.SetLocation(newLoc);

            bool collides = false;
            foreach (Walls w in Wall.Values)
            {
                if (w.CollidesTank(temp.GetPosition()))
                {
                    collides = true;
                    break;
                }
            }

            if (!collides)
            {
                foreach (Powerups p in PowerUps.Values)
                {
                    if (temp.TankCollidesPowerup(p.GetLocation()))
                    {
                        collides = true;
                        break;
                    }
                }
            }

            if (collides)
                SpawnTank(tank);
            else
            {
                tank.SetLocation(newLoc);
                tank.SetHP(3);
                tank.ResetDeathFrame();
                tank.ResetInvincibilityCounter();
            }
        }

        /// <summary>
        /// Spawns a powerup at a random location that isn't in a wall or tank
        /// </summary>
        public void SpawnPowerup()
        {
            Random rand = new Random();
            Vector2D newLoc = new Vector2D(rand.Next((-WorldSpace + 10) / 2, (WorldSpace - 10) / 2), rand.Next(-WorldSpace / 2, WorldSpace / 2));

            bool collides = false;
            foreach (Walls w in Wall.Values)
            {
                if (w.CollidesPowerup(newLoc))
                {
                    collides = true;
                    break;
                }
            }
            if (!collides)
            {
                foreach (Tank t in Players.Values)
                {
                    if (t.TankCollidesPowerup(newLoc))
                    {
                        collides = true;
                        break;
                    }
                }
            }
            if (collides)
                SpawnPowerup();
            else
            {
                Powerups p = new Powerups(newLoc);
                PowerUps.Add(p.GetID(), p);

                ResetDelayCount();
            }
        }

        /// <summary>
        /// Determines if a ray interescts a circle
        /// </summary>
        /// <param name="rayOrig">The origin of the ray</param>
        /// <param name="rayDir">The direction of the ray</param>
        /// <param name="center">The center of the circle</param>
        /// <param name="r">The radius of the circle</param>
        /// <returns></returns>
        public static bool Intersects(Vector2D rayOrig, Vector2D rayDir, Vector2D center, double r)
        {
            // ray-circle intersection test
            // P: hit point
            // ray: P = O + tV
            // circle: (P-C)dot(P-C)-r^2 = 0
            // substituting to solve for t gives a quadratic equation:
            // a = VdotV
            // b = 2(O-C)dotV
            // c = (O-C)dot(O-C)-r^2
            // if the discriminant is negative, miss (no solution for P)
            // otherwise, if both roots are positive, hit

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
        /// Moves current projectile in the world
        /// </summary>
        /// <param name="p"> current projectile</param>
        public void MoveProjectile(Projectiles p)
        {
            Vector2D newLoc = p.GetLocation() + (p.GetOrientation() * 25);
            foreach (Walls wall in Wall.Values)
            {
                if (wall.CollidesProjectile(newLoc))
                {
                    p.SetDead(true);
                }
            }

            foreach (Tank tank in Players.Values)
            {
                if (tank.TankCollidesProj(p))
                {
                    if (!tank.IsInvincible())
                        tank.SetHP(tank.GetHP() - 1);

                    if (tank.GetHP() == 0)
                    {
                        Tank t = Players[p.GetOwnerId()];

                        t.SetScore(t.GetScore() + 1);
                        TankDied(tank);
                    }

                    p.SetDead(true);
                }
            }

            if (newLoc.GetX() < -WorldSpace / 2 || newLoc.GetX() > WorldSpace / 2 ||
                    newLoc.GetY() < -WorldSpace / 2 || newLoc.GetY() > WorldSpace / 2)
            {
                p.SetDead(true);
            }
            else
                p.SetLocation(newLoc);
        }

        /// <summary>
        /// Called when tank fires a projectile -- resets the fire delay
        /// </summary>
        /// <param name="t"></param>
        public void FireProjectile(Tank t)
        {
            Projectiles p = new Projectiles(t.GetID(), t.GetPosition(), t.GetAim());

            //Only called inside lock
            Projs.Add(p.GetID(), p);

            t.ResetFireFrame();
        }

        /// <summary>
        /// Called when a tank fires a beam
        /// </summary>
        /// <param name="t"> tank that fired </param>
        /// <param name="BeamsToSend"> list of beams to send to other players </param>
        public void FireBeam(Tank t, List<Beams> BeamsToSend)
        {
            Beams b = new Beams(t.GetID(), t.GetPosition(), t.GetAim());
            t.SetPowerUpCounter(t.GetPowerUpCount() - 1);
            BeamsToSend.Add(b);

            //Checks if beam hit anyone
            foreach (Tank t2 in Players.Values)
            {
                if (t2.GetID() == t.GetID())
                    continue;

                if (Intersects(b.GetOrigin(), b.GetDirection(), t2.GetPosition(), 30)
                    && t2.GetHP() > 0 && !t2.IsInvincible())
                {
                    TankDied(t2);

                    t.SetScore(t.GetScore() + 1);
                }
            }
        }

        /// <summary>
        /// Handles logic when the tank died
        /// </summary>
        public void TankDied(Tank t)
        {
            t.SetDead(true);
            t.SetHP(0);

            if (ScoreDecrement)
                t.SetScore(t.GetScore() - 1);
        }

        /// <summary>
        /// Class that represents a tank
        /// Contains all the required fields for Json such as ID, location, orientation, etc.
        /// </summary>
        [JsonObject(MemberSerialization.OptIn)]
        public class Tank
        {
            [JsonProperty(PropertyName = "tank")]
            private int ID;

            [JsonProperty(PropertyName = "loc")]
            private Vector2D location;

            [JsonProperty(PropertyName = "bdir")]
            private Vector2D orientation;

            [JsonProperty(PropertyName = "tdir")]
            private Vector2D aiming = new Vector2D(0, -1);

            [JsonProperty(PropertyName = "name")]
            private string name;

            [JsonProperty(PropertyName = "hp")]
            private int hitPoints = 3;

            [JsonProperty(PropertyName = "score")]
            private int score = 0;

            [JsonProperty(PropertyName = "died")]
            private bool died = false;

            [JsonProperty(PropertyName = "dc")]
            private bool disconnected = false;

            [JsonProperty(PropertyName = "join")]
            private bool joined = false;

            //Given size of the tank
            public const int Size = 60;

            //Counters for various delays in the tank
            private int FireFrameCounter;
            private int RespawnFrameCounter;
            private int InvincibilityFrameCounter;

            //Counter for power ups
            private int PowerUpCounter;

            //Speed and velocity of the tank
            private int Speed;
            public Vector2D velocity;

            /// <summary>
            /// Properties for max delays for various functions
            /// </summary>
            public static int FireMaxFrameCount
            {
                get; set;
            }
            public static int RespawnMaxFrameCount
            {
                get; set;
            }
            public static int InvincibleMaxFrame
            {
                get; set;
            }

            // Doubles used for collision checking
            double top, bot, left, right;

            /// <summary>
            /// Default constructor for Json
            /// </summary>
            public Tank()
            {

            }

            /// <summary>
            /// Tank constructor
            /// 
            /// Sets values for ID, name, FireFrameCounter, location, orientation, speed, and velocity
            /// </summary>
            /// <param name="_ID"> ID of the tank </param>
            /// <param name="Name"> name of the tank </param>
            /// <param name="speed"> speed of the tank </param>
            public Tank(int _ID, string Name, int speed)
            {
                name = Name;
                ID = _ID;
                FireFrameCounter = FireMaxFrameCount;
                location = new Vector2D(0, 0);
                orientation = new Vector2D(0, -1);

                Speed = speed;

                velocity = new Vector2D(0, 0);
            }

            /// <summary>
            /// Checks if tank collides with the inputted projectile
            /// </summary>
            /// <param name="proj"> projectile being checked </param>
            /// <returns> if projectile is colliding </returns>
            public bool TankCollidesProj(Projectiles proj)
            {
                double expansion = Size / 2 + Projectiles.Size / 2;
                left = Math.Min(location.GetX(), location.GetX());
                right = Math.Max(location.GetX(), location.GetX());
                top = Math.Min(location.GetY(), location.GetY());
                bot = Math.Max(location.GetY(), location.GetY());

                return left - expansion < proj.GetX()
                    && proj.GetX() < right + expansion
                    && top - expansion < proj.GetY()
                    && proj.GetY() < bot + expansion
                    && hitPoints > 0
                    && proj.GetOwnerId() != ID;
            }

            /// <summary>
            /// Checks if tank collides with a powerup's location
            /// </summary>
            /// <param name="powerLoc"> location of power up being checked </param>
            /// <returns> if tank collides with power up </returns>
            public bool TankCollidesPowerup(Vector2D powerLoc)
            {
                double expansion = Size / 2;
                left = Math.Min(location.GetX(), location.GetX());
                right = Math.Max(location.GetX(), location.GetX());
                top = Math.Min(location.GetY(), location.GetY());
                bot = Math.Max(location.GetY(), location.GetY());

                return left - expansion < powerLoc.GetX()
                    && powerLoc.GetX() < right + expansion
                    && top - expansion < powerLoc.GetY()
                    && powerLoc.GetY() < bot + expansion;
            }

            /// <summary>
            /// Sets how many power ups the tank has
            /// </summary>
            /// <param name="count"></param>
            public void SetPowerUpCounter(int count)
            {
                PowerUpCounter = count;
            }

            /// <summary>
            /// Gets how many power ups the tank has
            /// </summary>
            /// <returns> amount of power ups </returns>
            public int GetPowerUpCount()
            {
                return PowerUpCounter;
            }

            /// <summary>
            /// Sets hitpoints of tank
            /// </summary>
            /// <param name="Hp"> health being set </param>
            public void SetHP(int Hp)
            {
                hitPoints = Hp;
            }

            /// <summary>
            /// Setting disconnected for the tank
            /// </summary>
            /// <param name="dc"> tank disconnected </param>
            public void SetDC(bool dc)
            {
                disconnected = dc;
            }

            /// <summary>
            /// Checks if the tank can fire a projectile
            /// </summary>
            /// <returns> if the tank can shoot </returns>
            public bool CanFire()
            {
                return FireFrameCounter == FireMaxFrameCount;
            }

            /// <summary>
            /// Moves the current frame forward -- if not already at the max
            /// </summary>
            public void MoveFireFrame()
            {
                if (FireFrameCounter != FireMaxFrameCount)
                    FireFrameCounter++;
            }

            /// <summary>
            /// Resets the fire delay after a tank shoots
            /// </summary>
            public void ResetFireFrame()
            {
                FireFrameCounter = 0;
            }

            /// <summary>
            /// Resets the respawn delay
            /// </summary>
            public void ResetDeathFrame()
            {
                RespawnFrameCounter = 0;
            }

            /// <summary>
            /// Increments the invincibility frames
            /// </summary>
            public void IncrementInvincCount()
            {
                InvincibilityFrameCounter++;
            }

            /// <summary>
            /// Checks if the tank is still invincible
            /// </summary>
            /// <returns></returns>
            public bool IsInvincible()
            {
                return InvincibilityFrameCounter != InvincibleMaxFrame;
            }

            /// <summary>
            /// Resets the invicibility frames
            /// </summary>
            public void ResetInvincibilityCounter()
            {
                InvincibilityFrameCounter = 0;
            }

            /// <summary>
            /// Checks if the tank can fire a beam
            /// </summary>
            /// <returns></returns>
            public bool CanFireBeam()
            {
                return PowerUpCounter != 0;
            }

            /// <summary>
            /// Moves the respawn frame forward
            /// </summary>
            public void MoveDeathFrame()
            {
                RespawnFrameCounter++;
            }

            /// <summary>
            /// Sets the score for the tank
            /// </summary>
            /// <param name="_score"> new score </param>
            public void SetScore(int _score)
            {
                score = _score;
            }

            /// <summary>
            /// Changes the direction and velocity of the tank depending on input
            /// </summary>
            /// <param name="dir"></param>
            public void ChangeDirAndVel(string dir)
            {
                switch (dir)
                {
                    case "left":
                        SetPlayerDirection(new Vector2D(-1, 0));
                        velocity = orientation * Speed;
                        break;
                    case "right":
                        SetPlayerDirection(new Vector2D(1, 0));
                        velocity = orientation * Speed;
                        break;
                    case "up":
                        SetPlayerDirection(new Vector2D(0, -1));
                        velocity = orientation * Speed;
                        break;
                    case "down":
                        SetPlayerDirection(new Vector2D(0, 1));
                        velocity = orientation * Speed;
                        break;
                    default:
                        velocity = orientation * 0;
                        break;
                }
            }

            /// <summary>
            /// Getter for the direction of the tank
            /// </summary>
            /// <returns>Tank direction</returns>
            public Vector2D GetPlayerDir()
            {
                return orientation;
            }

            /// <summary>
            /// Getter for if the player is disconnected
            /// </summary>
            /// <returns>If player is disconnected</returns>
            public bool IsDisconnected()
            {
                return disconnected;
            }

            /// <summary>
            /// Getter for x position of player
            /// </summary>
            /// <returns>Player's x position</returns>
            public double GetPlayerX()
            {
                return location.GetX();
            }

            /// <summary>
            /// Getter for y position of player
            /// </summary>
            /// <returns>Player's y position</returns>
            public double GetPlayerY()
            {
                return location.GetY();
            }

            /// <summary>
            /// Gets the position of the tank
            /// </summary>
            /// <returns>Position of tank</returns>
            public Vector2D GetPosition()
            {
                return location;
            }

            /// <summary>
            /// Getter for ID
            /// </summary>
            /// <returns>Player's ID</returns>
            public int GetID()
            {
                return ID;
            }

            /// <summary>
            /// Getter for the player name
            /// </summary>
            /// <returns>Player's name</returns>
            public string GetName()
            {
                return name;
            }

            /// <summary>
            /// Getter for the direction of the turret
            /// </summary>
            /// <returns>Turret direction</returns>
            public Vector2D GetAim()
            {
                return aiming;
            }

            /// <summary>
            /// Getter for if the player died
            /// </summary>
            /// <returns>If the player died</returns>
            public bool IsDead()
            {
                return died;
            }

            /// <summary>
            /// Getter for HP
            /// </summary>
            /// <returns>Tank's HP</returns>
            public int GetHP()
            {
                return hitPoints;
            }

            /// <summary>
            /// Getter for score
            /// </summary>
            /// <returns>Player's score</returns>
            public int GetScore()
            {
                return score;
            }

            /// <summary>
            /// Sets the location of the tank
            /// </summary>
            /// <param name="loc"></param>
            public void SetLocation(Vector2D loc)
            {
                location = loc;
            }

            /// <summary>
            /// Sets the direction of the tank
            /// </summary>
            /// <param name="dir"></param>
            public void SetPlayerDirection(Vector2D dir)
            {
                orientation = dir;
            }

            /// <summary>
            /// Sets the orientation of the tank's turrent
            /// </summary>
            /// <param name="aim"></param>
            public void SetAim(Vector2D aim)
            {
                aiming = aim;
            }

            /// <summary>
            /// Sets the tank as dead or not
            /// </summary>
            /// <param name="ded"> dead tank or not </param>
            public void SetDead(bool ded)
            {
                died = ded;
            }

            /// <summary>
            /// Checks if the tank can respawn
            /// </summary>
            /// <returns></returns>
            public bool CanRespawn()
            {
                return RespawnFrameCounter == RespawnMaxFrameCount;
            }
        }

        /// <summary>
        /// Class that represents a projectile
        /// Contains required fields for Json such as ID, location, orientation, etc.
        /// </summary>
        [JsonObject(MemberSerialization.OptIn)]
        public class Projectiles
        {
            [JsonProperty(PropertyName = "proj")]
            private int ID;

            [JsonProperty(PropertyName = "loc")]
            private Vector2D location;

            [JsonProperty(PropertyName = "dir")]
            private Vector2D orientation;

            [JsonProperty(PropertyName = "died")]
            private bool isDead;

            [JsonProperty(PropertyName = "owner")]
            private int ownerID;

            //Given size of the projectile
            public const int Size = 30;

            //Keeps track of the ID for projectile class
            static int AddID = 0;

            /// <summary>
            /// Default constructor for Json
            /// </summary>
            public Projectiles()
            {

            }

            /// <summary>
            /// Constructor for projectile
            /// 
            /// Sets values for _ownerID, location, orientation and its own ID
            /// </summary>
            /// <param name="_ownerID"> tank that fired it </param>
            /// <param name="loc"> location of projectile </param>
            /// <param name="_orientation"> orientation of projectile </param>
            public Projectiles(int _ownerID, Vector2D loc, Vector2D _orientation)
            {
                ID = AddID++;
                location = loc;
                ownerID = _ownerID;
                orientation = _orientation;
                isDead = false;
            }

            /// <summary>
            /// Sets the location of the projectile
            /// </summary>
            /// <param name="newLoc"></param>
            public void SetLocation(Vector2D newLoc)
            {
                location = newLoc;
            }

            /// <summary>
            /// Sets the projectile to dead or not
            /// </summary>
            /// <param name="dead"> projectile is dead</param>
            public void SetDead(bool dead)
            {
                isDead = dead;
            }

            /// <summary>
            /// Getter for ID
            /// </summary>
            /// <returns>Projectile ID</returns>
            public int GetID()
            {
                return ID;
            }

            /// <summary>
            /// Getter for if the projectile is dead
            /// </summary>
            /// <returns>If projectile is dead</returns>
            public bool IsDead()
            {
                return isDead;
            }

            /// <summary>
            /// Getter for owner ID
            /// </summary>
            /// <returns>ID of tank that fired projectile</returns>
            public int GetOwnerId()
            {
                return ownerID;
            }

            /// <summary>
            /// Getter for orientation
            /// </summary>
            /// <returns>Orientation of projectile</returns>
            public Vector2D GetOrientation()
            {
                return orientation;
            }

            /// <summary>
            /// Getter for x position
            /// </summary>
            /// <returns>X position of projectile</returns>
            public double GetX()
            {
                return location.GetX();
            }

            /// <summary>
            /// Getter for y position
            /// </summary>
            /// <returns>Y position of projectile</returns>
            public double GetY()
            {
                return location.GetY();
            }

            /// <summary>
            /// Gets location of the projectile
            /// </summary>
            /// <returns></returns>
            public Vector2D GetLocation()
            {
                return location;
            }
        }

        /// <summary>
        /// Class that represents a wall
        /// Contains required fields for Json
        /// </summary>
        [JsonObject(MemberSerialization.OptIn)]
        public class Walls
        {
            [JsonProperty(PropertyName = "wall")]
            private int ID;

            [JsonProperty(PropertyName = "p1")]
            private Vector2D startpoint;

            [JsonProperty(PropertyName = "p2")]
            private Vector2D endpoint;

            //thickness of the wall
            private const int thickness = 50;

            //Buffer for the wall
            double top, bot, left, right;

            //ID for wall class
            static int nextID = 0;

            /// <summary>
            /// Default constructor for Json
            /// </summary>
            public Walls()
            {

            }

            /// <summary>
            /// Constructor for walls
            /// 
            /// Sets values for ID, startpoint, endpoint, and the buffer for the wall
            /// </summary>
            /// <param name="start">startpoint of the wall segment</param>
            /// <param name="end">endpoint of the wall segment</param>
            public Walls(Vector2D start, Vector2D end)
            {
                ID = nextID++;
                startpoint = start;
                endpoint = end;

                left = Math.Min(start.GetX(), end.GetX());
                right = Math.Max(start.GetX(), end.GetX());
                top = Math.Min(start.GetY(), end.GetY());
                bot = Math.Max(start.GetY(), end.GetY());
            }

            /// <summary>
            /// Checks if wall collides with the tank
            /// </summary>
            /// <param name="tankLoc"> current tank </param>
            /// <returns> collision or not </returns>
            public bool CollidesTank(Vector2D tankLoc)
            {
                double expansion = thickness / 2 + Tank.Size / 2;
                return left - expansion < tankLoc.GetX() 
                    && tankLoc.GetX() < right + expansion 
                    && top - expansion < tankLoc.GetY() 
                    && tankLoc.GetY() < bot + expansion;
            }

            /// <summary>
            /// Checks if wall collides with projectile
            /// </summary>
            /// <param name="projLoc"> current projectile </param>
            /// <returns> collision or not </returns>
            public bool CollidesProjectile(Vector2D projLoc)
            {
                double expansion = thickness / 2 + Projectiles.Size / 2;
                return left - expansion < projLoc.GetX()
                    && projLoc.GetX() < right + expansion
                    && top - expansion < projLoc.GetY()
                    && projLoc.GetY() < bot + expansion;
            }

            /// <summary>
            /// Checks if wall collides with power up
            /// </summary>
            /// <param name="powerLoc"> current power up</param>
            /// <returns> collision or not </returns>
            public bool CollidesPowerup(Vector2D powerLoc)
            {
                double expansion = thickness / 2 + 5;
                return left - expansion < powerLoc.GetX()
                    && powerLoc.GetX() < right + expansion
                    && top - expansion < powerLoc.GetY()
                    && powerLoc.GetY() < bot + expansion;
            }

            /// <summary>
            /// Getter for ID
            /// </summary>
            /// <returns>Wall ID</returns>
            public int GetID()
            {
                return ID;
            }

            /// <summary>
            /// Getter for x position of the startpoint
            /// </summary>
            /// <returns>X position of the starting point</returns>
            public double StartX()
            {
                return startpoint.GetX();
            }

            /// <summary>
            /// Getter for y position of the startpoint
            /// </summary>
            /// <returns>Y position of the starting point</returns>
            public double StartY()
            {
                return startpoint.GetY();
            }

            /// <summary>
            /// Getter for x position of the endpoint
            /// </summary>
            /// <returns>X position of the endpoint</returns>
            public double EndX()
            {
                return endpoint.GetX();
            }

            /// <summary>
            /// Getter for the y position of the endpoint
            /// </summary>
            /// <returns>Y position of the endpoint</returns>
            public double EndY()
            {
                return endpoint.GetY();
            }
        }

        /// <summary>
        /// Class that represents a beam
        /// Contains required fields for Json
        /// </summary>
        [JsonObject(MemberSerialization.OptIn)]
        public class Beams
        {
            [JsonProperty(PropertyName = "beam")]
            private int ID;

            [JsonProperty(PropertyName = "org")]
            private Vector2D origin;

            [JsonProperty(PropertyName = "dir")]
            private Vector2D direction;

            [JsonProperty(PropertyName = "owner")]
            private int ownerID;

            private static int AddID = 0;

            /// <summary>
            /// Default constructor for Json
            /// </summary>
            public Beams()
            {

            }

            /// <summary>
            /// Constructor for the beam class
            /// 
            /// Sets the values for ownerID, ID, origin, and direction
            /// </summary>
            /// <param name="_ownerID">tank that fired it</param>
            /// <param name="org">orientation of the beam</param>
            /// <param name="dir">direction of the beam</param>
            public Beams(int _ownerID, Vector2D org, Vector2D dir)
            {
                ID = AddID++;
                ownerID = _ownerID;
                origin = org;
                direction = dir;
            }

            /// <summary>
            /// Getter for x position of the originating point
            /// </summary>
            /// <returns>X position of origin</returns>
            public double GetX()
            {
                return origin.GetX();
            }

            /// <summary>
            /// Getter for y position of the originating point
            /// </summary>
            /// <returns>Y position of origin</returns>
            public double GetY()
            {
                return origin.GetY();
            }

            /// <summary>
            /// Returns the origin of the beam
            /// </summary>
            /// <returns> the location of the origin </returns>
            public Vector2D GetOrigin()
            {
                return origin;
            }

            /// <summary>
            /// Getter for x value of the direction
            /// </summary>
            /// <returns>X value of direction</returns>
            public double GetDirX()
            {
                return direction.GetX();
            }

            /// <summary>
            /// Getter for y value of the direction
            /// </summary>
            /// <returns>Y value of direction</returns>
            public double GetDirY()
            {
                return direction.GetY();
            }

            /// <summary>
            /// Gets the direction of the beam
            /// </summary>
            /// <returns> the direction of the beam </returns>
            public Vector2D GetDirection()
            {
                return direction;
            }

            /// <summary>
            /// Gets the owner of the beam (ID)
            /// </summary>
            /// <returns> ID of who shot it </returns>
            public int GetOwnerID()
            {
                return ownerID;
            }
        }

        /// <summary>
        /// Class that represents a powerup
        /// Contains required fields for Json
        /// </summary>
        [JsonObject(MemberSerialization.OptIn)]
        public class Powerups
        {
            [JsonProperty(PropertyName = "power")]
            private int ID;

            [JsonProperty(PropertyName = "loc")]
            private Vector2D location;

            [JsonProperty(PropertyName = "died")]
            private bool isDead;

            //ID of power up class
            private static int AddID = 0;

            /// <summary>
            /// Default constructor for Json
            /// </summary>
            public Powerups()
            {
                ID = AddID++;
            }

            /// <summary>
            /// Constructor for the power up class
            /// 
            /// Sets values for location, ID, and isDead
            /// </summary>
            /// <param name="loc">location of the power up</param>
            public Powerups(Vector2D loc)
            {
                ID = AddID++;
                location = loc;
                isDead = false;
            }

            /// <summary>
            /// Getter for ID
            /// </summary>
            /// <returns>ID of powerup</returns>
            public int GetID()
            {
                return ID;
            }

            /// <summary>
            /// Getter for if powerup is dead
            /// </summary>
            /// <returns>If powerup is dead</returns>
            public bool IsDead()
            {
                return isDead;
            }

            /// <summary>
            /// Sets if power up is dead or not
            /// </summary>
            /// <param name="dead"> is dead or not</param>
            public void SetDead(bool dead)
            {
                isDead = dead;
            }

            /// <summary>
            /// Getter for x position
            /// </summary>
            /// <returns>X position of powerup</returns>
            public double GetX()
            {
                return location.GetX();
            }

            /// <summary>
            /// Getter for y position
            /// </summary>
            /// <returns>Y position of powerup</returns>
            public double GetY()
            {
                return location.GetY();
            }

            /// <summary>
            /// Gets the location of the power up
            /// </summary>
            /// <returns> X and Y of the Powerup </returns>
            public Vector2D GetLocation()
            {
                return location;
            }
        }

        /// <summary>
        /// Class for commands to be sent to the server
        /// Contains required fields for Json
        /// </summary>
        [JsonObject(MemberSerialization.OptIn)]
        public class ControlCommands
        {
            [JsonProperty(PropertyName = "moving")]
            private string movement;

            [JsonProperty(PropertyName = "fire")]
            private string fireType;

            [JsonProperty(PropertyName = "tdir")]
            private Vector2D turretDirection;

            /// <summary>
            /// Default constructor for Json
            /// </summary>
            public ControlCommands()
            {

            }

            /// <summary>
            /// Constructor to build a command to be sent to the server
            /// </summary>
            /// <param name="mov">Movement direction</param>
            /// <param name="ft">Fire type</param>
            /// <param name="td">Direction of turret</param>
            public ControlCommands(string mov, string ft, Vector2D td)
            {
                movement = mov;
                fireType = ft;
                turretDirection = td;
            }

            /// <summary>
            /// Gets the direction of the command
            /// </summary>
            /// <returns>direction</returns>
            public string GetDir()
            {
                return movement;
            }

            /// <summary>
            /// Gets the Fire type of the command
            /// </summary>
            /// <returns>fire type</returns>
            public string GetFire()
            {
                return fireType;
            }

            /// <summary>
            /// Gets direction of turrent direction of command
            /// </summary>
            /// <returns>turrent direction</returns>
            public Vector2D GetTurDir()
            {
                return turretDirection;
            }
        }
    }
}
