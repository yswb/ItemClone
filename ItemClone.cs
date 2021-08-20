using System;
using TShockAPI;
using Terraria;
using TerrariaApi.Server;
using System.Text.RegularExpressions;

namespace ItemClone
{
    [ApiVersion(2, 1)]
    public class ItemClone:TerrariaPlugin
    {

        public override string Author => "yswb";


        public override string Description => "Clone existing item";


        public override string Name => "ItemClone";

        public override Version Version => new Version(1, 0, 0, 0);

        //克隆物品栏第一格物品权限
        private static readonly string PERM_CLONE = "CloneFirst";
        //克隆所输入的物品权限，不管是否拥有物品都可以克隆
        private static readonly string PERM_CLONE_SELECTOR = "CloneSelected";

        public ItemClone(Main game) : base(game)
        {

        }

       


        public override void Initialize()
        {
            Commands.ChatCommands.Add(new Command(PERM_CLONE, CloneCommand, "clone", "c")
            {
                HelpText = "克隆输入或物品栏第一格的物品"
            });
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Deregister hooks here
            }
            base.Dispose(disposing);
        }

        private static void CloneCommand(CommandArgs args)
        {
            TSPlayer plr = args.Player;
            if (!plr.InventorySlotAvailable)
            {
                plr.SendErrorMessage("背包已满！");
                return;
            }

            NetItem netItem;
            String itemStr = args.Parameters.Count > 0 ? args.Parameters[0] : null;
            //获取用户输入的物品或者物品栏第一格
            if (!String.IsNullOrWhiteSpace(itemStr))
            {
                if (!plr.HasPermission(PERM_CLONE_SELECTOR))
                {
                    plr.SendErrorMessage("你只能克隆物品栏第一格的物品！");
                    return;
                }
                Item strItem = StrToItem(itemStr);
                if (strItem is null)
                {
                    plr.SendErrorMessage("找不到对应的物品：{0}", itemStr);
                    return;
                }
                netItem = (NetItem)strItem;

            }
            else
            {
                netItem = (NetItem)plr.TPlayer.inventory[10];
            }

            Console.WriteLine("UserName:[{3}],Clone item,ID:{0},Count:{1},Prefix:{2}", netItem.NetId, netItem.Stack, netItem.PrefixId, plr.Name);
            if (netItem.NetId == 0)
            {
                if (plr.HasPermission(PERM_CLONE_SELECTOR))
                {
                    plr.SendErrorMessage("请选择物品或者把物品放在背包第一格！");
                }
                else
                {
                    plr.SendErrorMessage("请把物品放在背包第一格！");
                }

                return;
            }
            Item item = TShock.Utils.GetItemById(netItem.NetId);
            bool succ = plr.GiveItemCheck(netItem.NetId, item.Name, item.maxStack, netItem.PrefixId);
            if (!succ)
            {
                plr.SendErrorMessage("你不能获取这个物品！");
            }


        }

        private static readonly Regex ITEM_REGEX = new Regex(@"^\[i(\/s\d+)?(\/p\d+)?:(\-?\d+)\]$");
        //字符串转为对应物品 可用格式 123, [i/s2:174], [i/p38:3063], [i:3384]

        public static Item StrToItem(string str)
        {
            int id;
            //直接输入ID也可以
            if (int.TryParse(str, out id))
            {
                Item idItem = TShock.Utils.GetItemById(id);
                if (idItem is null)
                {
                    return null;
                }
                idItem.stack = idItem.maxStack;
                return idItem;
            }

            if (!ITEM_REGEX.IsMatch(str))
            {
                Console.WriteLine("正则不匹配");
                return null;
            }

            //ID
            id = int.Parse(new Regex(@":(\-?\d+)").Match(str).Groups[1].Value);
            Item item = TShock.Utils.GetItemById(id);
            if (item is null)
            {
                return null;
            }
            //前缀
            Match prefixMatch = Regex.Match(str, @"\/p(\d+)");
            if (prefixMatch.Success)
            {
                item.prefix = (byte)int.Parse(prefixMatch.Groups[1].Value);
            }
            //数量直接取最大值
            item.stack = item.maxStack;
            return item;
        }
    }


}