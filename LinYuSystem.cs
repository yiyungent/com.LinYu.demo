using com.LinYu.demo;
using Eruru.Json;
using Eruru.MySqlHelper;
using QQMini.PluginSDK.Core;
using QQMini.PluginSDK.Core.Model;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Eruru.Command;
using Eruru.Http;

namespace com.linlin.demo
{
    public class LinYuSystem : PluginBase
    {
        /// <summary>
        /// 作者和插件信息
        /// </summary>
        public override PluginInfo PluginInfo
        {
            get
            {
                if (_PluginInfo is null)
                {
                    _PluginInfo = new PluginInfo()
                    {
                        Author = "空白",
                        Description = "BUG立华奏",
                        Name = "立华奏机器人",
                        PackageId = "com.LinYu.demo",
                        Version = new Version(1, 0, 0, 0)
                    };
                }
                return _PluginInfo;
            }
        }

    
        PluginInfo _PluginInfo;
        Random Random = new Random();

        public static MySqlHelper MySqlHelper;
        public static Config Config;

        MainWindow MainWindow;
        List<QQMessage> QQMessages = new List<QQMessage>();
        string ConfigPath;
        static object DatabaseLock = new object();
        static object QQMessageLock = new object();
        CommandSystem<QQMessage> CommandSystem = new CommandSystem<QQMessage>();

        /// <summary>
        /// config配置文件
        /// </summary>
        public override void OnInitialize()
        {
            ConfigPath = $@"Data\{PluginInfo.PackageId}\Config.json";
            LoadConfig();
            SaveConfig();
            CommandSystem.Register<Commands>();
        }

        /// <summary>
        /// 数据库链接
        /// </summary>
        void LoadConfig ()
        {
            if (!File.Exists(ConfigPath))
            {
                SaveConfig();
            }
            lock (DatabaseLock) {
                Config = JsonConvert.DeserializeFile(ConfigPath, Config);
                MySqlHelper?.Dispose();
                StringBuilder connectionString = new StringBuilder();
                connectionString.Append($"datasource={Config.DatabaseIP};");
                connectionString.Append($"database={Config.DatabaseName};");
                connectionString.Append($"user id={Config.DatabaseAccount};");
                connectionString.Append($"password={Config.DatabasePassword};");
                try
                {
                    MySqlHelper = new MySqlHelper(connectionString.ToString());
                } catch (Exception exception)
                {
                    QMLog.Error(exception.ToString ());
                }
            }
        }

        void SaveConfig ()
        {
            JsonConvert.Serialize(Config ?? new Config (), ConfigPath, false);
        }
        /// <summary>
        /// 当收到群组消息
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override QMEventHandlerTypes OnReceiveGroupMessage(QMGroupMessageEventArgs e)
        {
            OnReceiveMessage(new QQMessage (e));
            return QMEventHandlerTypes.Continue;
        }
        /// <summary>
        /// 设置菜单窗口
        /// </summary>
        public override void OnOpenSettingMenu()
        {
            if (MainWindow is null)
            {
                MainWindow = new MainWindow();
                MainWindow.Closed += (sender, e) =>
                {
                    MainWindow = null;
                    SaveConfig();
                    LoadConfig();
                };
                MainWindow.Show();
            }
            MainWindow.WindowState = System.Windows.WindowState.Normal;
            MainWindow.Activate();
        }
        /// <summary>
        /// 机器人签到功能
        /// </summary>
        /// <param name="qq"></param>
        /// <param name="group"></param>
        void CheckIn (QQMessage qqMessage)
        {
            int coins = Random.Next(Config.CheckInMinCoins, Config.CheckInMaxCoins + 1);
            switch (MySqlHelper.ExecuteScalar ($"select checkin ({qqMessage.QQ}, {coins})"))
            {
                case 1:
                    qqMessage. Reply("今日已签到");
                    break;
                case 2:
                case 3:
                    qqMessage.Reply( $"签到成功，金币加{coins}");
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        /// <summary>
        /// 金币查询
        /// </summary>
        /// <param name="qq"></param>
        /// <param name="group"></param>
        void Coins (QQMessage qqMessage)
        {
            int coins = Convert.ToInt32(MySqlHelper.ExecuteScalar($"select coins from checkin where qq = {qqMessage.QQ}"));
            qqMessage.Reply($"你目前拥有{coins}枚金币");
        }

        public static T GetQQValue<T> (long qq, string name) {
            lock (DatabaseLock) {
                return (T)Convert.ChangeType(MySqlHelper.ExecuteScalar($"select {name} from checkin where qq = {qq}"), typeof (T));
            }
        }

        public static void SetQQValue(long qq, string name, object value) {
            SetQQValue(qq, new List<KeyValuePair<string, object>>() {
             new KeyValuePair<string, object> (name, value)
            });
        }

        public static void SetQQValue (long qq, List<KeyValuePair<string, object>> items) {
            lock (DatabaseLock) {
                StringBuilder stringBuilder = new StringBuilder();
                for (int i=0;i<items.Count;i++) {
                    if (i > 0) {
                        stringBuilder.Append(',');
                    }
                    stringBuilder.Append($"{items[i].Key}={items[i].Value}");
                }
                 MySqlHelper.ExecuteScalar($"update checkin set {stringBuilder} where qq = {qq}");
            }
        }

        /// <summary>
        /// 当新人进群提示输出
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override QMEventHandlerTypes OnGroupMemberBeAllowAdd(QMGroupMemberIncreaseEventArgs e)
        {

            QQMessage.Reply(e.FromQQ, e.FromGroup, "欢迎新人进群：么么哒：");
            return base.OnGroupMemberBeAllowAdd(e);
        }
        /// <summary>
        /// 群员修改了群名片
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override QMEventHandlerTypes OnGroupMemberCardChange(QMGroupMemberCardChangeEventArgs e)
        {
            QQMessage.Reply (e.FromQQ, e.FromGroup, "修改了名片："+e.NewCard);
            return base.OnGroupMemberCardChange(e);
        }
        /// <summary>
        /// 当群成员离开本群
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override QMEventHandlerTypes OnGroupMemberLeave(QMGroupMemberDecreaseEventArgs e)
        {
            QQMessage.Reply (e.FromQQ, e.FromGroup, "离开了本群：" + e.FromQQ);
            return base.OnGroupMemberLeave(e);
        }
        /// <summary>
        /// 当群名字ID被改变
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override QMEventHandlerTypes OnGroupNameChange(QMGroupNameChangeEventArgs e)
        {
            QQMessage.Reply(default, e.FromGroup, "群名修改成了：" + e.NewCard);
            return base.OnGroupNameChange(e);
        }
        /// <summary>
        /// 当群员撤回消息     有BUG
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override QMEventHandlerTypes OnGroupMemberRemoveMessage(QMGroupMemberRemoveMessageEventArgs e)
        {
            lock (QQMessageLock) {
                for (int i =0;i<QQMessages.Count;i++) {
                    if(QQMessages[i].MessageID == e.MessageId) {
                        QQMessages[i].Reply($"撤回了{QQMessages[i].MessageText}");
                        break;
                    }
                }
            }
            return base.OnGroupMemberRemoveMessage(e);
        }
        /// <summary>
        /// 当群组的组员被禁言
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override QMEventHandlerTypes OnGroupMemberSetBanSpeak(QMGroupMemberBanSpeakEventArgs e)
        {
            QQMessage.Reply (e.FromQQ, e.FromGroup, "被管理员"+ e.OperateQQ+"禁言");
            return base.OnGroupMemberSetBanSpeak(e);
        }
        /// <summary>
        /// 当群组被同意申请加入请求
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override QMEventHandlerTypes OnGroupApplyAddRequest(QMGroupAddRequestEventArgs e)
        {
            QQMessage.Reply (e.FromQQ, e.FromGroup,  "被" + e.OperateQQ + "同意加入本群");
            return base.OnGroupApplyAddRequest(e);
        }
        /// <summary>
        /// 群成员被解除禁言触发
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override QMEventHandlerTypes OnGroupMemberRemoveBanSpeak(QMGroupMemberBanSpeakEventArgs e)
        {
            QQMessage.Reply (e.FromQQ, e.FromGroup,  "被管理员" + e.OperateQQ + "解除禁言");
            return base.OnGroupMemberRemoveBanSpeak(e);
        }
        /// <summary>
        /// 群组移除群成员触发事件
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override QMEventHandlerTypes OnGroupManagerRemoveMember(QMGroupMemberDecreaseEventArgs e)
        {
            QQMessage.Reply (e.FromQQ, e.FromGroup,  "被管理员：" + e.OperateQQ + "踢出去");
            return base.OnGroupManagerRemoveMember(e);
        }

        void OnReceiveMessage (QQMessage qqMessage)
        {
            DateTime expiry = DateTime.Now.AddMinutes(-2);
            lock (QQMessageLock) {
                for (int i = 0; i < QQMessages.Count; i++) {
                    if (QQMessages[i].DateTime >= expiry) {
                        QQMessages.RemoveRange(0, i);
                        break;
                    }
                }
                QQMessages.Add(qqMessage);
            }
            string atCode = $"[@{Config.RobotQQ}]";
            if (qqMessage.MessageText.Contains(atCode)) {
                string text = qqMessage.MessageText.Replace(atCode, string.Empty).Trim();
                //qqMessage.Reply(text);
                HttpRequestInformation request = new HttpRequestInformation() {
                     QueryStringParameters = new HttpParameterCollection () {
                         { "key", "free" },
                         { "appid", 0 },
                         { "msg", text }
                     },
                     Url = "http://api.qingyunke.com/api.php"
                };
                string json = new Http().Request(request);
                qqMessage.Reply(JsonObject.Parse(json)["content"].String.Replace("{br}", Environment.NewLine));
            }
            lock (DatabaseLock) {
                switch (qqMessage.MessageText) {
                    case "小奏签到":
                        CheckIn(qqMessage);
                        break;
                    case "小奏金币":
                        Coins(qqMessage);
                        break;
                    default:
                        CommandSystem.ExecuteText(qqMessage.MessageText, qqMessage);
                        break;
                }
            }
        }

    }
}
