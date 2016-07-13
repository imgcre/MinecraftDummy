using Mojang.Minecraft.Protocol.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mojang.Minecraft.Protocol
{
    partial class EssentialClient
    {
        /// <summary>
        /// 服务器向客户端注册频道
        /// </summary>
        /// <param name="channels">需注册的频道数组</param>
        [ChannelHandler("REGISTER")]
        protected virtual void OnChannelRegistering(string[] channels)
        {
            
        }


        /// <summary>
        /// 服务器向客户端注销频道
        /// </summary>
        /// <param name="channels">需注销的频道数组</param>
        [ChannelHandler("UNREGISTER")]
        protected virtual void OnChannelUnregistering(string[] channels)
        {
            
        }


        /// <summary>
        /// 被选择的手
        /// </summary>
        [VarIntUnderlying]
        public enum SelectedHand
        {
            MainHand,
            OffHnad,
        }


        /// <summary>
        /// 玩家打开了书
        /// </summary>
        /// <param name="selectedHand">被选择打开书的手</param>
        [ChannelHandler("MC|BOpen")]
        protected virtual void OnBookOpend(SelectedHand selectedHand)
        {

        }


        /// <summary>
        /// 得到用于识别的brand
        /// </summary>
        /// <param name="brand"></param>
        [ChannelHandler("MC|Brand")]
        protected virtual void OnBrandCame([Variable] string brand)
        {

        }


        /// <summary>
        /// 得到村民的交易列表
        /// </summary>
        /// <param name="windowId">窗口id</param>
        /// <param name="trades">交易列表</param>
        [ChannelHandler("MC|TrList")]
        protected virtual void OnTradeListCame(int windowId, [FixedCount(FixedCountType.Byte)] Trade[] trades)
        {

        }

    }

}
