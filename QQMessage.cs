using com.linlin.demo;
using QQMini.PluginSDK.Core;
using QQMini.PluginSDK.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.LinYu.demo {

    class QQMessage {

        public long RobotQQ;
        public long QQ;
        public long Group;
        public long MessageID;
        public string MessageText;
        public DateTime DateTime = DateTime.Now;

        public QQMessage(QMGroupMessageEventArgs e) {
            QQ = e.FromQQ.Id;
            Group = e.FromGroup.Id;
            MessageID = e.Message.Id;
            MessageText = e.Message.Text;
            RobotQQ = e.RobotQQ.Id;
        }

        public void Reply(object message, bool at = true) {
            Reply(QQ,Group, message, at, RobotQQ);
        }

        public static void Reply (long qq, long group, object message, bool at = true, long robotQQ = 0) {
            if(robotQQ < 1000) {
                robotQQ = LinYuSystem.Config.RobotQQ;
            }
            StringBuilder stringBuilder = new StringBuilder(message.ToString());
            if (group < 1000) {
                QMApi.CurrentApi.SendFriendMessage(robotQQ, qq, stringBuilder.ToString());
            } else {
                if (at && qq >= 1000) {
                    stringBuilder.Insert(0, $"[@{qq}]\r\n");
                }
                QMApi.CurrentApi.SendGroupMessage(robotQQ, group, stringBuilder.ToString());
            }
        }

    }

}