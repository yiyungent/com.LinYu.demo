using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.linlin.demo;
using Eruru.Command;

namespace com.LinYu.demo {
    class Commands {

        static readonly Dictionary<string, int> Items = new Dictionary<string, int>() {
            { "礼物", 10 }
        };

        [Command ("买")]
        public static void Buy (QQMessage qqMessage, params string[] names) {
            string name = string.Join(" ", names);
            if (Items.TryGetValue (name, out int price)) {
               int coins = LinYuSystem.GetQQValue<int>(qqMessage.QQ, "coins");
                if(coins < price) {
                    qqMessage.Reply($"你好穷，买不起{name}");
                } else {
                    LinYuSystem.SetQQValue(qqMessage.QQ, new List<KeyValuePair<string, object>>() {
                        new KeyValuePair<string, object> ("coins", coins - price ),
                        new KeyValuePair<string, object> ("gift", LinYuSystem.GetQQValue<int> (qqMessage.QQ, "gift") + 1)
                       });
                    qqMessage.Reply($"购买{name}成功");
                }
            } else {
                qqMessage.Reply($"没有{name}这个物品你");
            }
        }

        [Command ("小奏商店")]
        public static void Shop(QQMessage qqMessage) {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var item in Items) {
                stringBuilder.AppendLine($"{item.Key} 价格：{item.Value}");
            }
            qqMessage.Reply(stringBuilder);
        }

        [Command ("送小奏礼物")]
        public static void Give (QQMessage qqMessage) {
            int gift = LinYuSystem.GetQQValue<int>(qqMessage.QQ, "gift");
            if (gift <= 0) {
                qqMessage.Reply($"渣男！没有礼物也想送礼物，空手套白狼！");
            } else {
                LinYuSystem.SetQQValue(qqMessage.QQ, "gift", gift - 1);
                qqMessage.Reply("谢谢你送我礼物，你真是个好人！");
            }
        }

    }
}
