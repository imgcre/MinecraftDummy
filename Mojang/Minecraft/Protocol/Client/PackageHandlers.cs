using Mojang.Minecraft.Protocol.Providers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Mojang.Minecraft.Protocol
{
    public partial class EssentialClient
    {
        /// <summary>
        /// 服务端将会不断发送包含了一个随机数字标识符的保持在线，客户端必须以相同的数据包回复。
        /// <para>如果客户端超过30s没有回复，服务端将会踢出玩家。</para>
        /// </summary>
        /// <param name="keepAliveCode">随机数字标识符</param>
        [PackageHandler(0x00)]
        protected virtual void OnAliveKeeping([Variable] int keepAliveCode)
        {
            KeepAlive(keepAliveCode).Wait();
        }


        [PackageHandler(0x00, State.Status)]
        protected virtual void OnResponded(Chat response)
        {

        }


        /// <summary>
        /// 在登陆时被请出服务器
        /// </summary>
        /// <param name="reason">被请出的原因</param>
        [PackageHandler(0x00, State.Login)]
        protected virtual void OnDisconnectedWhenLogin([Variable] string reason)
        {

        }


        [Flags]
        public enum Gamemodes : byte
        {
            Survival,
            Creative,
            Adventure,
            Spectator,
            Hardcore = 0x08,
        }


        public enum Dimension : sbyte
        {
            Nether = -1,
            Overworld,
            End,
        }


        public enum Difficulty : byte
        {
            Peaceful, Easy, Normal, Hard,
        }


        [StringUnderlying]
        public enum LevelType
        {
            Default, Flat, LargeBiomes, Amplified, Default_1_1,
        }


        
        /// <summary>
        /// 加入游戏后服务器将发送有关世界的概括，此封包目前无效
        /// </summary>
        /// <param name="entityId">玩家的实体ID</param>
        /// <param name="gamemode">0:生存模式 1:创造模式 2:冒险模式 第三位(0x08)是极限模式的标识符</param>
        /// <param name="dimension">-1:下界  0:主世界  1:末地</param>
        /// <param name="difficulty">从0~3分别对应了 和平，简单，普通和困难</param>
        /// <param name="maxPlayers">客户端会以此来绘制玩家列表</param>
        /// <param name="levelType">default:普通世界 flat:超平坦 largeBiomes:巨大生物群系 amlified:放大世界 default_1_1:老算法的普通世界</param>
        [PackageHandler(0x01)]
        protected virtual void OnGameJoined(int entityId, Gamemodes gamemode, Dimension dimension, Difficulty difficulty, byte maxPlayers, LevelType levelType, bool isDebugInfoReduced)
        {
            ChangeState(StateAction.RequestStats).Wait();
        }


        [PackageHandler(0x01, State.Status)]
        protected virtual void OnPong(long payload)
        {

        }



        public enum ChatPosition : byte
        {
            Chat,
            SystemMessage,
            AboveHotbar,
        }


        /// <summary>
        /// 接受来自服务器的消息
        /// </summary>
        /// <param name="jsonData">聊天消息</param>
        [PackageHandler(0x02)]
        protected virtual void OnChatted(Chat jsonData, ChatPosition chatPosition)
        {

        }


        [PackageHandler(0x02, State.Login)]
        protected virtual void OnSucessfullyLogined([Variable] string uuid, [Variable] string username)
        {
            _ConnectState = State.Play;
        }


        [PackageHandler(0x03, State.Login)]
        protected virtual void OnRequestedEncryption([Variable] string serverId, [VariableCount] byte[] publicKey, [VariableCount] byte[] verifyToken)
        {

        }


        public enum Animation : byte
        {
            SwingArm, TakeDamage, LeaveBed, EatFood, CriticalEffect, MagicCriticalEffect
        }


        //有问题
        /// <summary>
        /// 时间是基于tick(刻)的，1s=20ticks，游戏中一天共24000刻，大约相当于现实中的20分钟。
        /// <para>dayTime是通过worldAge%24000得到的，0是日出。6000是正午，1200是日落，1800是午夜。</para>
        /// <para>服务器默认每秒将worldAge增加20</para>
        /// </summary>
        /// <param name="worldAge">以tick为单位，不受服务器指令影响</param>
        /// <param name="dayTime">世界（或地区）时间，以tick计如果为负数，太阳的位置（时间）将停留在此值绝对值的位置</param>
        [PackageHandler(0x03)]
        protected virtual void OnTimeUpdated(long worldAge, long dayTime)
        {
            
        }


        
        public enum EquipmentSlot : short
        {
            Held, Boots, Leggings, Chestplate, helmet
        }
        
        
        
        [PackageHandler(0x04)]
        protected virtual void OnEntityEquipped([Variable] int entityId, EquipmentSlot slot, Slot item)
        {
            
        }
        


        /// <summary>
        /// 服务器在客户端登陆后发送的指定玩家出生点（包括出生位置和指南针指向），它也可以在任何时候被发送，不过那只会影响指南针的指向。
        /// </summary>
        /// <param name="location">出生点</param>
        [PackageHandler(0X05)]
        protected virtual void OnSpawnPositionCame(Position location)
        {

        }


        //只有在重生的时候才会发送？
        /// <summary>
        /// 服务器发送给玩家以设置或更新他们的生命值
        /// <para>foodSaturation是作为food的过饱和度，food在foodSaturation大于0时不会降低，登陆的玩家会自动获得5.0的foodSaturation，进食将同时增加foodSaturation和food。</para>
        /// </summary>
        /// <param name="health">20 = 满生命值， 0即以下为死亡</param>
        /// <param name="food">0-20</param>
        /// <param name="foodSaturation">从0.0到5.0的整数增量变化</param>
        [PackageHandler(0X06)]
        protected virtual void OnHealthUpdated(float health, [Variable] int food, float foodSaturation)
        {

        }


        /// <summary>
        /// 要更改玩家的维度，需要发送一个包含了合适的维度的重生数据包，紧跟着新维度的区块数据，最后是一个“坐标视点“包
        /// </summary>
        /// <param name="gamemode">0:生存模式 1:创造模式 2:冒险模式 第三位(0x08)是极限模式的标识符</param>
        /// <param name="dimension">-1:下界  0:主世界  1:末地</param>
        /// <param name="difficulty">从0~3分别对应了 和平，简单，普通和困难</param>
        /// <param name="levelType">default:普通世界 flat:超平坦 largeBiomes:巨大生物群系 amlified:放大世界 default_1_1:老算法的普通世界</param>
        [PackageHandler(0X07)]
        protected virtual void OnRespawned([UnderlyingTypeRedefined(UnderlyingType.Int)] Dimension dimension, Difficulty difficulty, Gamemodes gamemode, LevelType levelType)
        {
            
        }


        /// <summary>
        /// 如果某标志位被设置，那么此标志位所表示的坐标值就是相对的
        /// </summary>
        [Flags]
        public enum RelativePositions : byte
        {
            X = 0x01,
            Y = 0x02,
            Z = 0x04,
            Y_ROT = 0x08,
            X_ROT = 0X10,
        }


        /// <summary>
        /// 更新玩家在服务器上的坐标。如果服务器上次已知的玩家坐标和最新发送的数据包的数据的坐标位置相差超过100单位的话，将导致玩家因“移动速度太快 :( (使用作弊器?)”而提出服务器。
        /// <para>一般来说如果定点小数的X轴或Z轴的值大于3.2E7D将导致客户端被踢出并显示“无效的坐标”。</para>
        /// </summary>
        /// <param name="x">绝对/相对坐标的X值</param>
        /// <param name="y">绝对/相对坐标的X值</param>
        /// <param name="z">绝对/相对坐标的X值</param>
        /// <param name="yaw">偏航（yaw）以角度计算，而且会遵循经典三角规则。偏航的圆单位是在XZ平面上以(0,1)为原点绕逆时针旋转，90度为(-1,0)，180度时为(0,-1)，270度时为(1,0)。</param>
        /// <param name="pitch">仰角（pitch）是以角度计算，0代表直视前方，-90表示看正上方，而90表示看正下方。</param>
        /// <param name="relativePositions">如果某标志位被设置，那么此标志位所表示的坐标值就是相对的</param>
        [PackageHandler(0X08)]
        protected virtual void OnPlayerPositionAndLookCame(double x, double y, double z, float yaw, float pitch, RelativePositions relativePositions)
        {
            
        }


        /// <summary>
        /// 发送数据包来改变玩家所选择的物品槽
        /// </summary>
        /// <param name="slot">玩家所选择的物品槽(0-8)</param>
        [PackageHandler(0X09)]  
        protected virtual void OnHeldItemChanged(byte slot)
        {
            
        }


        /// <summary>
        /// 这个数据包将告诉玩家上床了。 有匹配的实体ID的客户端将会进入床模式。 
        /// <para>这个数据包将发送给所有附近的玩家（包括已经在床上的玩家）</para>
        /// </summary>
        /// <param name="EntityId">玩家ID</param>
        /// <param name="location">床头部分的方块的所在位置</param>
        [PackageHandler(0X0a)]
        protected virtual void OnBedUsed([Variable] int EntityId, Position location)
        {

        }


        /// <summary>
        /// 发送任何一个实体都将改变动作。
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="animation"></param>
        [PackageHandler(0x0b)]
        protected virtual void OnAnimated([Variable] int entityId, Animation animation)
        {

        }


        
        /// <summary>
        /// 玩家生成
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="playerUuid"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="yaw"></param>
        /// <param name="pitch"></param>
        /// <param name="currentItem">物品/方块id</param>
        /// <param name="metadata"></param>
        [PackageHandler(0x0c)]
        protected virtual void OnPlayerSpawned([Variable] int entityId, Uuid playerUuid, Fixed x, Fixed y, Fixed z, Angle yaw, Angle pitch, short currentItem, EntityMetadata metadata)
        {

        }
        
        

        /// <summary>
        /// 服务器将在玩家捡起一个落在地上的物品时发出这个包。这个包的意义是向你展示这个东西飞向你。
        /// <para>物品不会加到你的背包中。服务器只会在每次客户端发送的玩家位置坐标[以及玩家的位置和所看的方向]发生改变后检查物品是否捡起。</para>
        /// </summary>
        /// <param name="colloectedEntityId"></param>
        /// <param name="collectorEntityId"></param>
        [PackageHandler(0x0d)]
        protected virtual void OnItemCollected([ Variable] int colloectedEntityId, [Variable] int collectorEntityId)
        {

        }




        /*
        //TODO: enum绑定
        /// <summary>
        /// 实体/交通工具被生成
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="type">对象的类型（详情见对象）</param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="pitch">偏航值以2p/256记</param>
        /// <param name="yaw">仰角值以2p/256记</param>
        /// <param name="data">代表对象类型的值</param>
        [PackageHandler(0x0e)]
        protected virtual void OnObjectSpawned([Variable] int entityId, byte type, Fixed x, Fixed y, Fixed z, Angle pitch, Angle yaw, int data)
        {

        }
        */


        public enum MobType : byte
        {
            Mob = 48,
            Monster,
            Creeper,
            Skeleton,
            Spider,
            GiantZombie,
            Zombie,
            Slime,
            Ghast,
            ZombiePigman,
            Enderman,
            CaveSpider,
            Silverfish,
            Blaze,
            MagmaCube,
            EnderDragon,
            Wither,
            Bat,
            Witch,
            Endermite,
            Guardian,
            Shulker,
            Pig = 90,
            Sheep,
            Cow,
            Chicken,
            Squid,
            Wolf,
            Mooshroom,
            Snowman,
            Ocelot,
            IronGolem,
            Horse,
            Rabbit,
            Villager = 120,
        }


        /// <summary>
        /// 实体被生成
        /// </summary>
        /// <param name="entityId">分配的实体id</param>
        /// <param name="type">实体类型</param>
        /// <param name="x">实体坐标x分量</param>
        /// <param name="y">实体坐标y分量</param>
        /// <param name="z">实体坐标z分量</param>
        /// <param name="yaw">实体偏航</param>
        /// <param name="pitch">实体仰角</param>
        /// <param name="headPitch">实体头部偏航</param>
        /// <param name="velocityX">实体速度x分量</param>
        /// <param name="velocityY">实体速度y分量</param>
        /// <param name="velocityZ">实体速度z分量</param>
        /// <param name="metadata">实体元数据</param>
        [PackageHandler(0x0f)]
        protected virtual void OnMobSpawned([Variable] int entityId, MobType mobType, Fixed x, Fixed y, Fixed z, Angle yaw, Angle pitch, Angle headPitch, short velocityX, short velocityY, short velocityZ, EntityMetadata metadata)
        {

        }
        


        public enum PaintingDirection : byte
        {
            North,
            West,
            South,
            East,
        }


        /// <summary>
        /// 画被生成
        /// </summary>
        /// <param name="entityId">画框实体id</param>
        /// <param name="title">画框名称</param>
        /// <param name="location">画框坐标中心</param>
        /// <param name="direction">画框朝向</param>
        [PackageHandler(0x10)]
        protected virtual void OnPaintingSpawned([Variable] int entityId, [Variable] string title, Position location, PaintingDirection direction)
        {

        }


        /// <summary>
        /// 生成一个或多个经验球。
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="count">一次收集到将获得的经验数量</param>
        [PackageHandler(0x11)]
        protected virtual void OnExperienceOrbSpawned([Variable] int entityId, Fixed x, Fixed y, Fixed z, short count)
        {
            
        }


        /// <summary>
        /// 当实体速度改变时发送
        /// <para>速度是以1/8000方块每刻（tick,1tick=50ms）；例如，-1343将会移动(-1343 / 8000) = −0.167875每刻（或-3,3575方块每秒）</para>
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="velocityX"></param>
        /// <param name="velocityY"></param>
        /// <param name="velocityZ"></param>
        [PackageHandler(0x12)]
        protected virtual void OnEntityVelocityChanged([Variable] int entityId, short velocityX, short velocityY, short velocityZ)
        {

        }


        /// <summary>
        /// 服务端在一列实体被客户端破坏时发送。
        /// </summary>
        /// <param name="entityIds"></param>
        [PackageHandler(0x13)]
        protected virtual void OnEntitiesDestroyed([Variable, VariableCount] int[] entityIds)
        {

        }


        /// <summary>
        /// 当从服务器发送到客户端时，它将初始化实体。 对玩家实体而言，这个包或其他任何移动/改变视点包都是每一刻(tick)会发送的。
        /// </summary>
        /// <param name="entityId"></param>
        [PackageHandler(0x14)]
        protected virtual void OnEntityInitialized([Variable] int entityId)
        {

        }


        /// <summary>
        ///实体移动且距离小于4个方块
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="deltaX"></param>
        /// <param name="deltaY"></param>
        /// <param name="deltaZ"></param>
        /// <param name="isOnGround"></param>
        [PackageHandler(0x15)]
        protected virtual void OnEntityRelativeMoved([Variable] int entityId, byte deltaX, byte deltaY, byte deltaZ, bool isOnGround)
        {

        }


        /// <summary>
        /// 实体转向
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="yaw"></param>
        /// <param name="pitch"></param>
        /// <param name="isOnGround"></param>
        [PackageHandler(0x16)]
        protected virtual void OnEntityLookChanged([Variable] int entityId, Angle yaw, Angle pitch, bool isOnGround)
        {

        }


        /// <summary>
        /// 实体移动且距离小于4个方块和实体转向
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="deltaX"></param>
        /// <param name="deltaY"></param>
        /// <param name="deltaZ"></param>
        /// <param name="yaw"></param>
        /// <param name="pitch"></param>
        /// <param name="isOnGround"></param>
        [PackageHandler(0x17)]
        protected virtual void OnEntityRelativeMovedAndEntityLookChanged([Variable] int entityId, byte deltaX, byte deltaY, byte deltaZ, Angle yaw, Angle pitch, bool isOnGround)
        {

        }


        /// <summary>
        /// 实体移动距离且超过四个方块
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="yaw"></param>
        /// <param name="pitch"></param>
        /// <param name="isOnGround"></param>
        [PackageHandler(0x18)]
        protected virtual void OnEntityTeleported([Variable] int entityId, int x, int y, int z, Angle yaw, Angle pitch, bool isOnGround)
        {

        }


        /// <summary>
        /// 实体面朝方向改变
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="headYaw"></param>
        [PackageHandler(0x19)]
        protected virtual void OnEntityHeadLookChanged([Variable] int entityId, Angle headYaw)
        {

        }


        public enum EntityStatus : byte
        {
            MobSpawn = 1,
            LivingEntityHurt,
            LivingEntityDead,
            IronGolemThrowingUpArms,//铁傀儡挥手
            WolfOrOCelotOrHorseTaming = 6,
            WolfOrOCelotOrHorseTamed,
            WolfShakingWater,
            EatingAccepted,
            SheepEatingGrassOrTNTIgnited,
            IronGolemHandingOverRose,
            VillagerMating,
            VillagerAngry,
            VillagerHappy,
            WitchMagic,
            ZombieConvertingIntoVillager,
            FireworkExploding,
            AnimalInLove,
            ResetSquidRotation,
            Explosing,
            Guarded,
            EnablesReducedDebug,
            DisablesReducedDebug,
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="entityStatus"></param>
        [PackageHandler(0x1a)]
        protected virtual void OnEntityStatusChanged(int entityId, EntityStatus entityStatus)
        {

        }


        //TODO: ATTACH?
        /// <summary>
        /// 实体被依附
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="vehicleId">交通工具的实体ID</param>
        /// <param name="leash">如果为true将绑定实体于交通工具上</param>
        [PackageHandler(0x1b)]
        protected virtual void OnEntityAttached(int entityId, int vehicleId, bool leash)
        {

        }

        
        /*TODO: 元数据格式错误问题于实体元数据改变
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="metadata"></param>
        [PackageHandler(0x1c)]
        protected virtual void OnEntityMetadataChanged([Variable] int entityId, EntityMetadata metadata)
        {

        }
        */


        public enum PotionEffect : byte
        {
            MoveSpeed = 1,
            MoveSlowdown,
            DigSpeed,
            DigSlowDown,
            DamageBoost,
            Heal,
            Harm,
            Jump,
            Confusion,
            Regeneration,
            Resistance,
            FireResistance,
            WaterBreathing,
            Invisibility,
            Blindness,
            NightVision,
            Hunger,
            Weakness,
            Poison,
            Wither,
            HealthBoost,
            Absorption,
            Saturation,
            Glowing,
            Levitation,
            Luck,
            Unluck,
        }


        /// <summary>
        /// 实体获得药水效果或药水效果改变
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="effect"></param>
        /// <param name="amplifier">药水效果等级</param>
        /// <param name="duration">效果所持续的时间，单位为秒</param>
        /// <param name="hideParticles">是否隐藏颗粒</param>
        [PackageHandler(0x1d)]
        protected virtual void OnEntityEffectChanged([Variable] int entityId, PotionEffect potionEffect, byte amplifier, [Variable] int duration, bool hideParticles)
        {

        }


        /// <summary>
        /// 实体的药水效果被移除
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="potionEffect"></param>
        [PackageHandler(0x1e)]
        protected virtual void OnEntityEffectRemoved([Variable] int entityId, PotionEffect potionEffect)
        {

        }


        /// <summary>
        /// 用户的经验被设置
        /// </summary>
        /// <param name="experienceBar">取值范围在[0, 1]</param>
        /// <param name="level"></param>
        /// <param name="totalExperience">详见http://minecraft.gamepedia.com/Experience#Leveling_up</param>
        [PackageHandler(0x1f)]
        protected virtual void OnExperienceSet(float experienceBar, [Variable] int level, [Variable] int totalExperience)
        {

        }


        /*TODO: 玩家数据api
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="entityProperties"></param>
        [PackageHandler(0x20)]
        protected virtual void OnEntityPropertiesCame([Variable] int entityId, [FixedCount] EntityProperty[] entityProperties)
        {

        }
        */



        public enum GlobalEntityType : byte
        {
            Thunderbolt = 1
        }


        //TODO: 一堆未实现的有关区块的协议



        /// <summary>
        /// 在半径512范围内生成雷电
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="globalEntityType"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        [PackageHandler(0x2c)]
        protected virtual void OnGlobalEntitySpawned([Variable] int entityId, GlobalEntityType globalEntityType, Fixed x, Fixed y, Fixed z)
        {

        }


        public const byte PlayerInventory = 0;


        [StringUnderlying]
        public enum Inventory
        {
            [Renamed("minecraft:chest")]Chest,
            [Renamed("minecraft:crafting_table")]CraftingTable,
            [Renamed("minecraft:furnace")]Furnace,
            [Renamed("minecraft:dispenser")]Dispenser,
            [Renamed("minecraft:enchanting_table")]EnchantingTable,
            [Renamed("minecraft:brewing_stand")]BrewingStand,
            [Renamed("minecraft:villager")]Villager,
            [Renamed("minecraft:beacon")]Beacon,
            [Renamed("minecraft:anvil")]Anvil,
            [Renamed("minecraft:hopper")]Hopper,
            [Renamed("minecraft:dropper")]Dropper,
            EntityHorse,
        }

        /// <summary>
        /// 诸如箱子、工作台或熔炉窗口被打开
        /// </summary>
        /// <param name="windowId"></param>
        /// <param name="windowType">窗口类型</param>
        /// <param name="WindowTitle">窗口标题</param>
        /// <param name="numSlots">窗口栏位数</param>
        /// <param name="entityId">马的实体ID，仅适用与窗口类型为EntityHorse的情况</param>
        [PackageHandler(0x2d)]
        protected virtual void OnWindowOpened(byte windowId, [Variable] string windowType, Chat WindowTitle, byte numSlots, [Optional(Inventory.EntityHorse)] int? entityId)
        {

        }
        


        /// <summary>
        /// 服务器强制关闭窗口
        /// </summary>
        /// <param name="windowId">被关闭的窗口ID，0表示背包</param>
        [PackageHandler(0x2e)]
        protected virtual void OnWindowClosed(byte windowId)
        {

        }


        /// <summary>
        /// 某窗口中栏位的物品被增加/移除
        /// </summary>
        /// <param name="windowId">被更新的窗口ID，0表示背包。仅当窗口打开或窗口关闭（更新背包）时触发。</param>
        /// <param name="slot">所更新的栏位</param>
        /// <param name="slotData"></param>
        [PackageHandler(0x2f)]
        protected virtual void OnSlotSet(byte windowId, short slot, Slot slotData)
        {

        }
        


        /*
        //TODO:slot可空有歧义, 窗口栏位物品事件
        /// <summary>
        /// 窗口中栏位的物品被增加/移除
        /// </summary>
        /// <param name="windowId">被更改的物品所对应的窗口ID。0表示玩家背包。</param>
        /// <param name="slotData"></param>
        [PackageHandler(0x30)]
        protected virtual void OnWindowItemsChanged(byte windowId, [FixedCount(PrefixLengthFieldType.Short)] Slot[] slotData)
        {

        }
        */


        /// <summary>
        /// 窗口属性被改变
        /// </summary>
        /// <param name="windowId"></param>
        /// <param name="property">将被更新的状态类型</param>
        /// <param name="value">新状态值</param>
        [PackageHandler(0x31)]
        protected virtual void OnWindowPropertyChanged(byte windowId, short property, short value)
        {

        }


        /// <summary>
        /// 确认窗口操作请求是否被接受
        /// </summary>
        /// <param name="windowId"></param>
        /// <param name="actionNumber"></param>
        /// <param name="accepted"></param>
        [PackageHandler(0x32)]
        protected virtual void OnWindowTransactionConfirming(byte windowId, short actionNumber, bool accepted)
        {

        }


        /// <summary>
        /// 牌子被覆盖或创建
        /// </summary>
        /// <param name="location">牌子方块的坐标</param>
        /// <param name="line1">第一行内容</param>
        /// <param name="line2">第二行内容</param>
        /// <param name="line3">第三行内容</param>
        /// <param name="line4">第四行内容</param>
        [PackageHandler(0x33)]
        protected virtual void OnSignUpdated(Position location, Chat line1, Chat line2, Chat line3, Chat line4)
        {

        }


        //0x34 地图


        public enum BlockEntityUpdateAction : byte
        {
            SetSpawnPotential = 1,
            SetCommandBlockText,
            SetBeaconLevelOrPrimaryOrSecondaryPower,
            SetRotationOrSkinOfMobHead,
            SetFlowerPotType,
            SetBannerBaseColorOrPattern,
        }


        /*
        //有歧义: Optional
        /// <summary>
        /// 方块实体被更新
        /// </summary>
        /// <param name="location"></param>
        /// <param name="blockEntityUpdateAction">更新类型</param>
        /// <param name="Tag"></param>
        [PackageHandler(0x35)]
        protected virtual void OnBlockEntityUpdated(Position location, BlockEntityUpdateAction blockEntityUpdateAction, [Optional] Nbt Tag)
        {

        }
        */


        //SignEditorOpened
        /// <summary>
        /// 主动放置牌子的反馈
        /// </summary>
        /// <param name="location">牌子方块的坐标</param>
        [PackageHandler(0x36)]
        protected virtual void OnSignEditorOpened(Position location)
        {

        }


        [PackageHandler(0x37)]
        protected virtual void OnStatisticsCame([VariableCount] Statistic[] statistic)
        {

        }


        //TODO: enum => package field 表驱动法
        //0x38

        [Flags]
        public enum PlayerAbilities : byte
        {
            CreativeMode = 0x01,
            Flying = 0x02,
            FlyEnabled = 0x04,
            GodMode = 0x08
        }


        [PackageHandler(0x39)]
        protected virtual void OnPlayerAbilitiesCame(PlayerAbilities playerAbilities, float flyingSpeed, float walkingSpeed)
        {
            
        }

        /// <summary>
        /// 自动补全方法的响应。在非指令补全的情况下，这个列表是玩家的用户名
        /// </summary>
        /// <param name="matches"></param>
        [PackageHandler(0x3a)]
        protected virtual void OnTabCompleteResponded([VariableCount, Variable] string[] matches)
        {

        }

        public enum ScoreboardUpdateMode : byte
        {
            Create,
            Remove,
            UpdateDisplayText,
        }

        [StringUnderlying]
        public enum ScoreboardType
        {
            Integer,
            Hearts,
        }


        /// <summary>
        /// 记分板被更新
        /// </summary>
        /// <param name="objectiveName">容器名称</param>
        /// <param name="scoreboardUpdateMode"></param>
        /// <param name="objectiveValue">仅当mode为Create或UpdateDisplayText时可用</param>
        /// <param name="type">仅当mode为Create或UpdateDisplayText时可用。"integer"或"hearts"</param>
        [PackageHandler(0x3b)]
        protected virtual void OnScoreboardObjectiveUpdated
        (
            [Variable] string objectiveName, 
            ScoreboardUpdateMode scoreboardUpdateMode,
            [Optional(ScoreboardUpdateMode.Create, ScoreboardUpdateMode.UpdateDisplayText), Variable] string objectiveValue, 
            [Optional(ScoreboardUpdateMode.Create, ScoreboardUpdateMode.UpdateDisplayText)] ScoreboardType type
        )
        {

        }

        public enum ScoreUpdateAction : byte
        {
            CreateOrUpdate,
            Remove,
        }


        /// <summary>
        /// 计分板内容被更新
        /// </summary>
        /// <param name="scoreName">需更新或删除的计分板名</param>
        /// <param name="scoreUpdateAction"></param>
        /// <param name="objectiveName">分数所属的容器的容器名</param>
        /// <param name="value">需显示的分数值，仅当更新/删除的时候此值不为1</param>
        [PackageHandler(0x3c)]
        protected virtual void OnScoreUpdated([Variable] string scoreName, ScoreUpdateAction scoreUpdateAction, [Variable] string objectiveName, [Optional, Variable] int value)
        {

        }

        public enum ScoreboardDisplayPosition : byte
        {
            List,
            Sidebar,
            BelowName,
        }


        /// <summary>
        /// 客户端需显示计分板
        /// </summary>
        /// <param name="scoreboardDisplayPosition"></param>
        /// <param name="scoreName">计分板的唯一名称</param>
        [PackageHandler(0x3d)]
        protected virtual void OnScoreboardDisplaying(ScoreboardDisplayPosition scoreboardDisplayPosition, [Variable] string scoreName)
        {

        }



        [PackageHandler(0x3f)]
        protected virtual void OnPluginMessageCame(Channel channel)
        {

        }


        //OnDisconnected
        [PackageHandler(0x40)]
        protected virtual void OnDisconnected(Chat reason)
        {

        }


        [PackageHandler(0x41)]
        protected virtual void OnDifficultyChanged(Difficulty difficulty)
        {

        }


        [VarIntUnderlying]
        public enum CombatEvent
        {
            EnterCombat,
            EndCombat,
            EntityDead,
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="combatEvent"></param>
        /// <param name="duration">Only for end combat</param>
        /// <param name="playerId">Only for entity dead </param>
        /// <param name="entityId">Only for end combat and entity dead</param>
        /// <param name="message">Only for entity dead</param>
        [PackageHandler(0x42)]
        protected virtual void OnCombating
        (
            CombatEvent combatEvent,
            [Optional(CombatEvent.EndCombat), Variable] int duration,
            [Optional(CombatEvent.EntityDead), Variable] int playerId,
            [Optional(CombatEvent.EndCombat, CombatEvent.EntityDead)] int entityId,
            [Optional(CombatEvent.EntityDead), Variable] string message
        )
        {

        }






    }
}
