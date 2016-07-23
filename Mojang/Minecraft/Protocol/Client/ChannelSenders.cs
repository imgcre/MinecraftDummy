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
        private ChannelSenderManager _ChannelSenderManager;


        /// <summary>
        /// 向服务器注册频道
        /// </summary>
        /// <param name="channels">欲注册的频道名称数组</param>
        /// <returns></returns>
        [ChannelSender("REGISTER")]
        protected async Task RegisterChannel(string[] channels)
            => await _ChannelSenderManager.Send(new object[] { channels });


        /// <summary>
        /// 向服务器注销频道
        /// </summary>
        /// <param name="channels">欲注销的频道名称数组</param>
        /// <returns></returns>
        [ChannelSender("UNREGISTER")]
        protected async Task UnregisterChannel(string[] channels)
            => await _ChannelSenderManager.Send(new object[] { channels });


        //MC|AdvCmd


        /*
        TAG_Compound(''): 1 entry
        {
            TAG_List('pages'): 2 entries
            {
                 TAG_String(0): 'Something on Page 1'
                 TAG_String(1): 'Something on Page 2'
            }
        }
        */
        /// <summary>
        /// 编辑未签名的书（书与笔）
        /// </summary>
        /// <param name="content">书本的内容</param>
        /// <returns></returns>
        [ChannelSender("MC|BEdit")]
        protected async Task EditBook(Slot content)
            => await _ChannelSenderManager.Send(content);


        /*
        TAG_Compound(''): 3 entires
        {
             TAG_String('author'): 'Steve'
             TAG_String('title'): 'A Wonderful Book'
             TAG_List('pages'): 2 entries
             {
                  TAG_String(0): 'Something on Page 1'
                  TAG_String(1): 'Something on Page 2'
             }
         }
         */
        /// <summary>
        /// 为书签名
        /// </summary>
        /// <param name="content">书本的内容，Slot中的物品id必须为成书</param>
        /// <returns></returns>
        [ChannelSender("MC|BSign")]
        protected async Task SignBook(Slot content)
            => await _ChannelSenderManager.Send(content);


        /// <summary>
        /// 设置信标的属性
        /// </summary>
        /// <param name="firstProperty">第一属性</param>
        /// <param name="secondProperty">第二属性</param>
        /// <returns></returns>
        [ChannelSender("MC|Beacon")]
        protected async Task Beacon(int firstProperty, int secondProperty)
            => await _ChannelSenderManager.Send(firstProperty, secondProperty);


        /// <summary>
        /// 设置用于客户端与服务器相互识别的brand
        /// </summary>
        /// <param name="brand"></param>
        /// <returns></returns>
        [ChannelSender("MC|Brand")]
        protected async Task SetBrand([Variable] string brand)
            => await _ChannelSenderManager.Send(brand);


        /// <summary>
        /// 在使用铁砧时，命名物品
        /// </summary>
        /// <param name="newItemName">物品的新名称</param>
        /// <returns></returns>
        [ChannelSender("MC|ItemName")]
        protected async Task SetItemName([Variable] string newItemName)
            => await _ChannelSenderManager.Send(newItemName);


        //TODO: 翻译问题
        /// <summary>
        /// 以手持槽位与在给定的容器槽位中的物品交换出空槽位
        /// </summary>
        /// <param name="slot">欲从容器中抢出的槽位</param>
        /// <returns></returns>
        [ChannelSender("MC|PickItem")]
        protected async Task PickItem([Variable] int slot)
            => await _ChannelSenderManager.Send(slot);


        //TODO: 翻译问题
        /// <summary>
        /// 选择交易项目
        /// </summary>
        /// <param name="slot">玩家当前（交易）容器被选择的槽位</param>
        /// <returns></returns>
        [ChannelSender("MC|TrSel")]
        protected async Task SelectTrade(int slot)
            => await _ChannelSenderManager.Send(slot);


    }
}
