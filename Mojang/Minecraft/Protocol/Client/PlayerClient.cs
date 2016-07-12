using Mojang.Minecraft.Protocol.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mojang.Minecraft.Protocol
{
    /// <summary>
    /// 内部实现了假人登陆的流程
    /// </summary>
    public class PlayerClient : EssentialClient
    {
        public string Username { get; private set; }
        
        /// <summary>
        /// 连接至mc服务器
        /// </summary>
        /// <param name="serverAddress">服务器ip或域名</param>
        /// <param name="serverPort">服务器端口</param>
        /// <param name="Username">玩家名称</param>
        public PlayerClient(string serverAddress, ushort serverPort, string username) : base(serverAddress, serverPort)
        {
            LoginToServer(username);
        }


        /// <summary>
        /// 连接至mc服务器
        /// </summary>
        /// <param name="connectProvider">至服务器的连接</param>
        /// <param name="username">玩家名称</param>
        public PlayerClient(IConnectProvider connectProvider, string username) : base(connectProvider)
        {
            LoginToServer(username);
        }


        private void LoginToServer(string username)
        {
            Username = username;
            Handshake(47, ServerAddress, ServerPort, LoginNextState.Login).Wait();
            Login(username).Wait();
            StartRecievePackage();
        }

        public double X { get; private set; }
        public double Y { get; private set; }
        public double Z { get; private set; }
        public float Yaw { get; private set; }
        public float Pitch { get; private set; }

        protected override void OnPlayerPositionAndLookCame(double x, double y, double z, float yaw, float pitch, RelativePositions relativePositions)
        {
            if ((relativePositions | RelativePositions.X) != 0) X += x;
            else X = x;

            if ((relativePositions | RelativePositions.Y) != 0) Y += y;
            else Y = y;

            if ((relativePositions | RelativePositions.Z) != 0) Z += z;
            else Z = z;

            Yaw = yaw;
            Pitch = pitch;
            SetPositionAndLook(x, y, z, yaw, pitch, true).Wait();
            base.OnPlayerPositionAndLookCame(x, y, z, yaw, pitch, relativePositions);
        }


        protected override async Task SetPositionAndLook(double x, double feetY, double z, float yaw, float pitch, bool isOnGround)
        {
            X = x;
            Y = feetY;
            Z = z;
            Yaw = yaw;
            Pitch = pitch;
            await base.SetPositionAndLook(x, feetY, z, yaw, pitch, isOnGround);
        }


        protected override async Task SetPosition(double x, double feetY, double z, bool isOnGround)
        {
            X = x;
            Y = feetY;
            Z = z;
            await base.SetPosition(x, feetY, z, isOnGround);
        }


        protected override Task SetLook(float yaw, float pitch, bool isOnGround)
        {
            Yaw = yaw;
            Pitch = pitch;
            return base.SetLook(yaw, pitch, isOnGround);
        }

        public int EntityId { get; private set; }
        public Gamemodes Gamemodes { get; private set; }
        public Dimension Dimension { get; private set; }
        public Difficulty Difficulty { get; private set; }
        public byte MaxPlayers { get; private set; }
        public LevelType LevelType { get; private set; }

        protected override void OnGameJoined(int entityId, Gamemodes gamemode, Dimension dimension, Difficulty difficulty, byte maxPlayers, LevelType levelType, bool isDebugInfoReduced)
        {
            EntityId = entityId;
            Gamemodes = gamemode;
            Dimension = dimension;
            Difficulty = difficulty;
            MaxPlayers = maxPlayers;
            LevelType = levelType;

            base.OnGameJoined(entityId, gamemode, dimension, difficulty, maxPlayers, levelType, isDebugInfoReduced);
        }

        protected override void OnRespawned(Dimension dimension, Difficulty difficulty, Gamemodes gamemode, LevelType levelType)
        {
            Dimension = dimension;
            Difficulty = difficulty;
            LevelType = levelType;

            base.OnRespawned(dimension, difficulty, gamemode, levelType);
        }


        protected override void OnDifficultyChanged(Difficulty difficulty)
        {
            Difficulty = difficulty;

            base.OnDifficultyChanged(difficulty);
        }

        public Uuid Uuid { get; private set; }

        protected override void OnSucessfullyLogined(string uuid, string username)
        {
            Uuid = Uuid.Parse(uuid);
            Username = username;

            base.OnSucessfullyLogined(uuid, username);
        }

        public long WorldAge { get; private set; }
        public long DayTime { get; private set; }

        protected override void OnTimeUpdated(long worldAge, long dayTime)
        {
            WorldAge = worldAge;
            DayTime = dayTime;

            base.OnTimeUpdated(worldAge, dayTime);
        }


        public float Health { get; private set; }
        public int Food { get; private set; }
        public float FoodSaturation { get; private set; }

        protected override void OnHealthUpdated(float health, int food, float foodSaturation)
        {
            Health = health;
            Food = food;
            FoodSaturation = foodSaturation;

            base.OnHealthUpdated(health, food, foodSaturation);
        }

        public byte HandHeldSlot { get; private set; }

        protected override void OnHeldItemChanged(byte slot)
        {
            HandHeldSlot = slot;

            base.OnHeldItemChanged(slot);
        }


        public PlayerAbilities PlayerAbilities { get; private set; }
        public float FlyingSpeed { get; private set; }
        public float WalkingSpeed { get; private set; }

        protected override void OnPlayerAbilitiesCame(PlayerAbilities playerAbilities, float flyingSpeed, float walkingSpeed)
        {
            PlayerAbilities = playerAbilities;
            FlyingSpeed = flyingSpeed;
            WalkingSpeed = walkingSpeed;

            base.OnPlayerAbilitiesCame(playerAbilities, flyingSpeed, walkingSpeed);
        }

    }
}
