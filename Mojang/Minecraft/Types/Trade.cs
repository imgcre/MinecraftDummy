using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mojang.Minecraft.Protocol.Providers;

namespace Mojang.Minecraft
{
    public class Trade : IPackageField
    {
        /// <summary>
        /// 村民需购买的首项物品
        /// </summary>
        public Slot FirstInputItem { get; private set; }

        /// <summary>
        /// 村民出售的物品 
        /// </summary>
        public Slot OutputItem { get; private set; }

        /// <summary>
        /// 村民需购买的第二项物品, 可能为null
        /// </summary>
        public Slot SecondInputItem { get; private set; }

        /// <summary>
        /// 交易是否被禁用
        /// </summary>
        public bool TradeDisabled { get; private set; }

        /// <summary>
        /// 已交易次数
        /// </summary>
        public int TradeUseTime { get; private set; }

        /// <summary>
        /// 最大交易次数
        /// </summary>
        public int MaxTradeUseTime { get; private set; }

        void IPackageField.AppendIntoField(FieldMaker fieldMaker)
        {
            throw new NotSupportedException();
        }

        void IPackageField.FromField(FieldMatcher fieldMatcher)
        {
            FirstInputItem = fieldMatcher.Match<Slot>();
            OutputItem = fieldMatcher.Match<Slot>();
            var hasSecondItem = fieldMatcher.Match<bool>();
            if (hasSecondItem)
                SecondInputItem = fieldMatcher.Match<Slot>();

            TradeDisabled = fieldMatcher.Match<bool>();
            TradeUseTime = fieldMatcher.Match<int>();
            MaxTradeUseTime = fieldMatcher.Match<int>();
        }
    }
}
