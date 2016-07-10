using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mojang.Minecraft.Protocol
{
    //去除参数类型: /[\[\w\]]*?(?<!,) /

    public partial class EssentialClient
    {
        [VarIntUnderlying]
        public enum LoginNextState
        {
            Ping = 1,
            Login,
        }


        /// <summary>
        /// 以握手开始Ping或登陆
        /// </summary>
        /// <param name="protocolVersion">协议版本</param>
        /// <param name="serverAddress">服务器地址,可以是主机名(如"localhost")也可以是IP</param>
        /// <param name="serverPort">端口号,如25565</param>
        /// <param name="nextState">代表本次握手的目的</param>
        /// <returns></returns>
        [PackageSender(0x00, State.Handshaking)]
        protected async Task Handshake([Variable] int protocolVersion, [Variable] string serverAddress, ushort serverPort, LoginNextState nextState)
        {
            await SendPackage(protocolVersion, serverAddress, serverPort, nextState);
            switch (nextState)
            {
                case LoginNextState.Ping:
                    _ConnectState = State.Status;
                    break;
                case LoginNextState.Login:
                    _ConnectState = State.Login;
                    break;
            }
        }


        [PackageSender(0x00, State.Status)]
        protected virtual async Task HandshakeRequest()
            => await SendPackage();


        [PackageSender(0x00, State.Login)]
        protected virtual async Task Login([Variable] string name)
            => await SendPackage(name);

        
        /// <summary>
        /// 响应服务器的在线请求
        /// </summary>
        /// <param name="keepAlive"></param>
        /// <returns></returns>
        [PackageSender(0x00)]
        protected virtual async Task KeepAlive([Variable] int keepAlive)
            => await SendPackage(keepAlive);


        [PackageSender(0x01, State.Status)]
        protected virtual async Task Ping(long payload)
            => await SendPackage(payload);


        [PackageSender(0x01, State.Login)]
        protected virtual async Task ResponseEncryption([VariableCount] byte[] sharedSecret, [VariableCount] byte[] verifyToken)
            => await SendPackage(sharedSecret, verifyToken);


        /// <summary>
        /// 聊天
        /// </summary>
        /// <param name="message">默认的服务器会检查消息是否以‘/’开头。若不是，则将发送者的名字添加到消息前并将它发送给所有客户端端（包括发送者自己）。如果是以‘/’开头，服务器将认为它是一个指令并尝试处理它。</param>
        /// <returns></returns>
        [PackageSender(0x01)]
        protected virtual async Task Chat([Variable] string message)
            => await SendPackage(message);


        [VarIntUnderlying]
        public enum EntityUseWay
        {
            Interact,
            Attack,
            //InteractAt
        }


        //TODO: 1.8存在实体用法重载
        //
        /// <summary>
        /// 攻击或右键实体(比如玩家,马,矿车...)
        /// <para>原版服务器只会在实体距离玩家不超过4单位长度,并且两者之间视线不受阻碍时,才会处理此数据包;在1.8中若两者之间没有视线,则距离不能超过3.若视线通畅,则距离不能超过6.</para>
        /// <para>注意在创造模式中,鼠标中间选取是在客户端进行,但它也会发送给服务器一个创造模式物品栏动作数据包.</para>
        /// </summary>
        /// <param name="targetEntityId"></param>
        /// <param name="entityUseWay"></param>
        /// <returns></returns>
        [PackageSender(0x02)]
        protected virtual async Task UseEntity([Variable] int targetEntityId, EntityUseWay entityUseWay)
            => await SendPackage(targetEntityId, entityUseWay);
        


        /// <summary>
        /// 改变是飞行状态
        /// <para>当从一定高度落下时,跌落伤害会在此项从False变为True时施加在玩家身上. 伤害程度是根据上次此项由True变False时,玩家站立的地点的高度来决定.</para>
        /// <para>注意:并非只有此数据包会引起跌落伤害计算,其他的几种跟移动相关的数据包也能引发跌落伤害.</para>
        /// </summary>
        /// <param name="isOnGround"></param>
        /// <returns>true为在地上或者在游泳,false为其他情况</returns>
        [PackageSender(0x03)]
        protected virtual async Task ChangeOnGroundState(bool isOnGround)
            => await SendPackage(isOnGround);


        /// <summary>
        /// 更新位置.
        /// <para>如果头部Y减去脚底Y小于0.1或大于1.65,服务器会以“Illegal Stance”为理由踢出玩家. </para>
        /// <para> 如果新位置到旧位置(以服务器数据为准)的距离超过100个单位长度的话,玩家会被以"You moved too quickly :( (Hacking?)"为理由踢出.</para>
        /// <para>此外,如果X或Z的定点数(即浮点数的尾数部分)大于3.2E7D的话,服务器也会以"Illegal position"为理由踢出玩家.</para>
        /// </summary>
        /// <param name="x">绝对坐标(即相对世界原点位置)X</param>
        /// <param name="feetY">玩家脚底的绝对坐标Y.</param>
        /// <param name="headY">玩家头部的绝对坐标Y. 通常为头部Y坐标减1.62,用来修正玩家的碰撞盒</param>
        /// <param name="z">绝对坐标Z</param>
        /// <param name="isOnGround">true为在地上或者在游泳,false为其他情况</param>
        /// <returns></returns>
        [PackageSender(0x04)]
        protected virtual async Task SetPosition(double x, double feetY, double z, bool isOnGround)
            => await SendPackage(x, feetY, z, isOnGround);


        /// <summary>
        /// 更新玩家正在观察的方向.
        /// </summary>
        /// <param name="yaw">围绕X轴的绝对旋转,角度制</param>
        /// <param name="pitch">围绕Y轴的绝对旋转,角度制</param>
        /// <param name="isOnGround">true为在地上或者在游泳,false为其他情况</param>
        /// <returns></returns>
        [PackageSender(0x05)]
        protected virtual async Task SetLook(float yaw, float pitch, bool isOnGround)
            => await SendPackage(yaw, pitch, isOnGround);


        /// <summary>
        /// 同时结合了玩家位置和观察的数据包.
        /// </summary>
        /// <param name="x">绝对坐标(即相对世界原点位置)X</param>
        /// <param name="feetY">玩家脚底的绝对坐标Y. 通常为头部Y坐标减1.62,用来修正玩家的碰撞盒</param>
        /// <param name="z">绝对坐标Z</param>
        /// <param name="yaw">围绕X轴的绝对旋转,角度制</param>
        /// <param name="pitch">围绕Y轴的绝对旋转,角度制</param>
        /// <param name="isOnGround">true为在地上或者在游泳,false为其他情况</param>
        /// <returns></returns>
        [PackageSender(0x06)]
        protected virtual async Task SetPositionAndLook(double x, double feetY, double z, float yaw, float pitch, bool isOnGround)
            => await SendPackage(x, feetY, z, yaw, pitch, isOnGround);


        public enum DiggingAction : byte
        {
            StartedDigging,
            CancelledDigging,
            FinishedDigging,
            DropItemStack,
            DropItem,
            ShootArrowOrFinishEating,
        }


        public enum BlockFace : byte
        {
            NegativeY,
            PositiveY,
            NegativeZ,
            PositiveZ,
            NegativeX,
            PositiveX,
        }


        /// <summary>
        /// 采集方块. 只接受距离玩家6个单位距离以内的挖掘
        /// </summary>
        /// <param name="diggingAction">玩家当期挖掘的进度(见下表)</param>
        /// <param name="location">方块坐标</param>
        /// <param name="blockFace">挖掘的面</param>
        /// <returns></returns>
        [PackageSender(0x07)]
        protected virtual async Task DigBlock(DiggingAction diggingAction, Position location, BlockFace blockFace)
            => await SendPackage(diggingAction, location, blockFace);


        /// <summary>
        /// 放置方块
        /// </summary>
        /// <param name="location">方块坐标</param>
        /// <param name="blockFace">在放置前光标所指向方块的面</param>
        /// <param name="heldItem">手上拿的物品</param>
        /// <param name="cursorPositionX">放置前,光标指在砖块上的相对位置</param>
        /// <param name="cursorPositionY"></param>
        /// <param name="cursorPositionZ"></param>
        /// <returns></returns>
        [PackageSender(0x08)]
        protected virtual async Task PlaceBlock(Position location, BlockFace blockFace, Slot heldItem, byte cursorPositionX, byte cursorPositionY, byte cursorPositionZ)
            => await SendPackage(location, blockFace, heldItem, cursorPositionX, cursorPositionY, cursorPositionZ);


        /// <summary>
        /// 改变手持物
        /// </summary>
        /// <param name="slot">欲选择的栏位</param>
        /// <returns></returns>
        [PackageSender(0x09)]
        protected virtual async Task ChangeHeldItem(short slot)
            => await SendPackage(slot);

        
        /// <summary>
        /// 产生挥手的动作
        /// </summary>
        /// <returns></returns>
        [PackageSender(0x0a)]
        protected virtual async Task SwingArm()
            => await SendPackage();


        [VarIntUnderlying]
        public enum EntityAction
        {
            Crouch,
            Uncrouch,
            LeaveBed,
            StartSprinting,
            StopSprinting,
            JumpWithHorse,
            OpenInventory,
        }


        /// <summary>
        /// 在蹲着，离开床或奔跑
        /// </summary>
        /// <param name="entityId">玩家ID</param>
        /// <param name="entityAction"></param>
        /// <param name="jumpBoost">马跳加速，范围从0到100</param>
        /// <returns></returns>
        [PackageSender(0x0b)]
        protected virtual async Task ActEntity([Variable] int entityId, EntityAction entityAction, [Variable] int jumpBoost)
            => await SendPackage(entityId, entityAction, jumpBoost);


        [Flags]
        public enum SteerFlags : byte
        {
            Jump = 0x01,
            Unmount = 0x02,
        }


        /// <summary>
        /// 驾驶交通工具
        /// </summary>
        /// <param name="sideways">向左为正</param>
        /// <param name="forward">向前为正</param>
        /// <param name="steerFlags"></param>
        /// <returns></returns>
        [PackageSender(0x0c)]
        protected virtual async Task SteerVehicle(float sideways, float forward, SteerFlags steerFlags)
            => await SendPackage(sideways, forward, steerFlags);



        /// <summary>
        /// 关闭窗口
        /// </summary>
        /// <param name="windowId">要关闭的窗口的ID，0表示背包</param>
        /// <returns></returns>
        [PackageSender(0x0d)]
        protected virtual async Task CloseWindow(byte windowId)
            => await SendPackage(windowId);

        /*
        
        模式 按键 栏位 触发方式
        0 0 Normal 鼠标左键点击 
        1 Normal 鼠标右键点击 
        1 0 Normal Shift+鼠标左键点击 
        1 Normal Shift+鼠标右键点击 
        2 0 Normal 数字键1 
        1 Normal 数字键2 
        2 Normal 数字键3 
        ... ... ... 
        8 Normal 数字键9 
        3 2 Normal 鼠标中键 
        4 0 Normal 丢弃物品（Q） 
        1 Normal Ctrl+丢弃物品（Q） 
        0 -999 左键什么都不拿点击背包外(No-op) 
        1 -999 右键什么都不拿点击背包外(No-op) 
        5 0 -999 开始鼠标左键（或中键）拖拽 
        4 -999 开始鼠标右键拖拽 
        1 Normal 鼠标左键拖拽来增加栏位 
        5 Normal 鼠标右键拖拽来增加栏位 
        2 -999 结束鼠标左键拖拽 
        6 -999 结束鼠标右键拖拽 
        6 0 Normal 双击
        */
        /// <summary>
        /// 点击窗口中的栏位
        /// </summary>
        /// <param name="windowId">所点击的窗口ID，0表示玩家背包</param>
        /// <param name="slot">点击的栏位</param>
        /// <param name="button">所用来点击的键</param>
        /// <param name="actionNumber">这个动作的唯一数字，用来控制事物</param>
        /// <param name="mode">背包操作模式</param>
        /// <param name="clickedItem"></param>
        /// <returns></returns>
        [PackageSender(0x0e)]
        protected virtual async Task ClickWindow(byte windowId, short slot, byte button, short actionNumber, byte mode, Slot clickedItem)
            => await SendPackage(windowId, slot, button, actionNumber, mode, clickedItem);


        /// <summary>
        /// 确认窗口事务
        /// </summary>
        /// <param name="windowId">发生操作的窗口ID</param>
        /// <param name="actionNumber">每一个操作都有一个唯一数字，这个字段与这个数字有关</param>
        /// <param name="accepted">操作是否被接受</param>
        /// <returns></returns>
        [PackageSender(0x0f)]
        protected virtual async Task ConfirmWindowTransaction(byte windowId, short actionNumber, bool accepted)
            => await SendPackage(windowId, actionNumber, accepted);


        /// <summary>
        /// 创造模式获得物品
        /// </summary>
        /// <param name="slot">背包栏位</param>
        /// <param name="item">所需获得的物品slot数据</param>
        /// <returns></returns>
        [PackageSender(0x10)]
        protected virtual async Task SetInventorySlot(short slot, Slot item)
            => await SendPackage(slot, item);


        /// <summary>
        /// 附魔物品
        /// </summary>
        /// <param name="windowId">附魔窗口的id</param>
        /// <param name="enchantment">附魔物品在附魔台窗口的位置，从0开始作为最高的一个</param>
        /// <returns></returns>
        [PackageSender(0x11)]
        protected virtual async Task EnchantItem(byte windowId, byte enchantment)
            => await SendPackage(windowId, enchantment);


        /// <summary>
        /// 修改牌子
        /// </summary>
        /// <param name="location">牌子坐标</param>
        /// <param name="line1">牌子的第一行</param>
        /// <param name="line2">牌子的第二行</param>
        /// <param name="line3">牌子的第三行</param>
        /// <param name="line4">牌子的第四行</param>
        /// <returns></returns>
        [PackageSender(0x12)]
        protected virtual async Task UpdateSign(Position location, Chat line1, Chat line2, Chat line3, Chat line4)
            => await SendPackage(location, line1, line2, line3, line4);

        /*

        /// <summary>
        /// 设置玩家能力
        /// </summary>
        /// <param name="playerAbilities">玩家能力标志</param>
        /// <param name="flyingSpeed">飞行速度</param>
        /// <param name="walkingSpeed">行走速度</param>
        /// <returns></returns>
        [PackageSender(0x13)]
        protected virtual async Task SetPlayerAbilities(PlayerAbilities playerAbilities, float flyingSpeed, float walkingSpeed)
            => await SendPackage(playerAbilities, flyingSpeed, walkingSpeed);

        */

        //TODO: 无法实现自动补全，协议问题？
        /// <summary>
        /// 输入文本进行补全请求
        /// </summary>
        /// <param name="text"></param>
        /// <param name="hasPosition"></param>
        /// <param name="lookedAtBlock">只有在hasPosition==true时才能不为null</param>
        /// <returns></returns>
        [PackageSender(0x14)]
        protected virtual async Task TabCompleteRequest([Variable] string text, bool hasPosition, [Optional] Position? lookedAtBlock)
            => await SendPackage(text, hasPosition, lookedAtBlock);


        public enum ChatMode : byte
        {
            Enabled,
            CommandsOnly,
            Hidden,
        }


        [Flags]
        public enum DisplayedSkinParts : byte
        {
            CapeEnabled = 0x01,
            JacketEnabled = 0x02,
            LeftSleeveEnabled = 0x04,
            RightSleeveEnabled = 0x08,
            LeftPantsLegEnabled = 0x10,
            RightPantsLegEnabled = 0x20,
            HatEnabled = 0x40,
        }

        /// <summary>
        /// 改变或设置客户端设置
        /// </summary>
        /// <param name="locale">地区化，例如en_GB</param>
        /// <param name="viewDistance">客户端渲染距离（区块）</param>
        /// <param name="chatMode">聊天设置</param>
        /// <param name="chatColors">“颜色”设置</param>
        /// <param name="displayedSkinParts">皮肤部分</param>
        /// <returns></returns>
        [PackageSender(0x15)]
        protected virtual async Task SetClientSettings([Variable] string locale, byte viewDistance, ChatMode chatMode, bool chatColors, DisplayedSkinParts displayedSkinParts)
            => await SendPackage(locale, viewDistance, chatMode, chatColors, displayedSkinParts);


        [VarIntUnderlying]
        public enum StateAction
        {
            PerformRespawn, RequestStats, TakingInventoryAchievement
        }


        /// <summary>
        /// 完成连接或复活
        /// </summary>
        /// <param name="action">表示需改变的状态</param>
        /// <returns></returns>
        [PackageSender(0x16)]
        protected virtual async Task ChangeState(StateAction action)
            => await SendPackage(action);


        //PluginMessage 0x17


        /// <summary>
        /// 观察某玩家
        /// </summary>
        /// <param name="targetPlayer">目标玩家</param>
        /// <returns></returns>
        [PackageSender(0x18)]
        protected virtual async Task Spectate(Uuid targetPlayer)
            => await SendPackage(targetPlayer);


        [VarIntUnderlying]
        public enum ResourcePackResult
        {
            SuccessfullyLoaded,
            Declined,
            FailedDownload,
            Accepted,
        }


        /// <summary>
        /// 设置资源包状态
        /// </summary>
        /// <param name="hash">资源包hash值</param>
        /// <param name="reslut">欲设置的结果</param>
        /// <returns></returns>
        [PackageSender(0x19)]
        protected virtual async Task SetResourcePackStatus([Variable] string hash, ResourcePackResult reslut)
            => await SendPackage(hash, reslut);

    }
}
