using System.ComponentModel;

namespace Scripts.Entities.Enum
{
    public enum SoundEffectsType
    {
        [Description("ItemBuy")]
        ItemBuy,
        [Description("ItemDrop")]
        ItemDrop,
        [Description("ItemEquip")]
        ItemEquip,
        [Description("ItemGrab")]
        ItemGrab,
        [Description("ItemHover")]
        ItemHover,
        [Description("ItemSell")]
        ItemSell,
        [Description("ItemUnequip")]
        ItemUnequip
    }
}