using com.LinYu.demo;
using Eruru.Json;
using Eruru.MySqlHelper;
using QQMini.PluginSDK.Core;
using QQMini.PluginSDK.Core.Model;
using System;
using System.IO;
using System.Text;


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
        string ConfigPath;
        object LoadConfigLock = new object();

        /// <summary>
        /// config配置文件
        /// </summary>
        public override void OnInitialize()
        {
            ConfigPath = $@"Data\{PluginInfo.PackageId}\Config.json";
            LoadConfig();
            SaveConfig();
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
            lock (LoadConfigLock) {
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
            long RobotQQ = 377359254;
            string atCode = $"[@{RobotQQ}]";
            if (e.Message.Text.Contains(atCode))
            {
                Reply(e.FromQQ, e.FromGroup.Id, e.Message.Text.Replace(atCode, string.Empty).Trim());
            }
            lock (LoadConfigLock)
            {
                switch (e.Message.Text)
                {
                    case "小奏签到":
                        CheckIn(e.FromQQ.Id, e.FromGroup.Id);
                        break;
                    case "小奏金币":
                        Coins(e.FromQQ.Id, e.FromGroup.Id);
                        break;
                }
            }
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
        void CheckIn (long qq, long group)
        {
            int coins = Random.Next(Config.CheckInMinCoins, Config.CheckInMaxCoins + 1);
            switch (MySqlHelper.ExecuteScalar ($"select checkin ({qq}, {coins})"))
            {
                case 1:
                    Reply(qq, group, "今日已签到");
                    break;
                case 2:
                case 3:
                    Reply(qq, group, $"签到成功，金币加{coins}");
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
        void Coins (long qq, long group)
        {
            int coins = Convert.ToInt32(MySqlHelper.ExecuteScalar($"select coins from checkin where qq = {qq}"));
            Reply(qq,group, $"你目前拥有{coins}枚金币");
        }
        /// <summary>
        /// 向群组或QQ好友发送一条消息
        /// </summary>
        /// <param name="qq"></param>
        /// <param name="group"></param>
        /// <param name="message"></param>
        void Reply (long qq, long group, object message)
        {
            if (group > 0)
            {
                QMApi.SendGroupMessage(Config.RobotQQ, group, $"[@{qq}]{Environment.NewLine}{message}");
                return;
            }
            QMApi.SendFriendMessage(Config.RobotQQ, qq, message.ToString());
        }
        /// <summary>
        /// 当新人进群提示输出
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override QMEventHandlerTypes OnGroupMemberBeAllowAdd(QMGroupMemberIncreaseEventArgs e)
        {
            Reply(e.FromQQ, e.FromGroup, "欢迎新人进群：么么哒：");
            return base.OnGroupMemberBeAllowAdd(e);
        }
        /// <summary>
        /// 群员修改了群名片
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override QMEventHandlerTypes OnGroupMemberCardChange(QMGroupMemberCardChangeEventArgs e)
        {
            Reply(e.FromQQ, e.FromGroup,"修改了名片："+e.NewCard);
            return base.OnGroupMemberCardChange(e);
        }
        /// <summary>
        /// 当群成员离开本群
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override QMEventHandlerTypes OnGroupMemberLeave(QMGroupMemberDecreaseEventArgs e)
        {
            Reply(e.FromQQ, e.FromGroup, "离开了本群：" + e.FromQQ);
            return base.OnGroupMemberLeave(e);
        }
        /// <summary>
        /// 当群名字ID被改变
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override QMEventHandlerTypes OnGroupNameChange(QMGroupNameChangeEventArgs e)
        {
            Reply(e.RobotQQ, e.FromGroup.Id, "群名修改成了："+e.NewCard);
            return base.OnGroupNameChange(e);
        }
        /// <summary>
        /// 当群员撤回消息     有BUG
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override QMEventHandlerTypes OnGroupMemberRemoveMessage(QMGroupMemberRemoveMessageEventArgs e)
        {
            Reply(e.FromQQ, e.FromGroup,e.FromQQ+"撤回了一条涩涩的消息");
            return base.OnGroupMemberRemoveMessage(e);
        }
        /// <summary>
        /// 当群组的组员被禁言
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override QMEventHandlerTypes OnGroupMemberSetBanSpeak(QMGroupMemberBanSpeakEventArgs e)
        {
            Reply(e.FromQQ, e.FromGroup,"被管理员"+ e.OperateQQ+"禁言");
            return base.OnGroupMemberSetBanSpeak(e);
        }
        /// <summary>
        /// 当群组被同意申请加入请求
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override QMEventHandlerTypes OnGroupApplyAddRequest(QMGroupAddRequestEventArgs e)
        {
            Reply(e.FromQQ, e.FromGroup, "被" + e.OperateQQ + "同意加入本群");
            return base.OnGroupApplyAddRequest(e);
        }
        /// <summary>
        /// 群成员被解除禁言触发
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override QMEventHandlerTypes OnGroupMemberRemoveBanSpeak(QMGroupMemberBanSpeakEventArgs e)
        {
            Reply(e.FromQQ, e.FromGroup, "被管理员" + e.OperateQQ + "解除禁言");
            return base.OnGroupMemberRemoveBanSpeak(e);
        }
        /// <summary>
        /// 群组移除群成员触发事件
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override QMEventHandlerTypes OnGroupManagerRemoveMember(QMGroupMemberDecreaseEventArgs e)
        {
            Reply(e.FromQQ, e.FromGroup, "被管理员：" + e.OperateQQ + "踢出去");
            return base.OnGroupManagerRemoveMember(e);
        }
    }
}
