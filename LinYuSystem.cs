using com.LinYu.demo;
using Eruru.Json;
using Eruru.MySqlHelper;
using QQMini.PluginSDK.Core;
using QQMini.PluginSDK.Core.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace com.linlin.demo
{
    public class LinYuSystem : PluginBase
    {
        public override PluginInfo PluginInfo
        {
            get
            {
                if (_PluginInfo is null)
                {
                    _PluginInfo = new PluginInfo()
                    {
                        Author = "作者",
                        Description = "插件描述",
                        Name = "插件名",
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

        public override void OnInitialize()
        {
            ConfigPath = $@"Data\{PluginInfo.PackageId}\Config.json";
            LoadConfig();
            SaveConfig();
        }

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

        public override QMEventHandlerTypes OnReceiveGroupMessage(QMGroupMessageEventArgs e)
        {
            lock (LoadConfigLock)
            {
                switch (e.Message.Text)
                {
                    case "签到":
                        CheckIn(e.FromQQ.Id, e.FromGroup.Id);
                        break;
                    case "金币":
                        Coins(e.FromQQ.Id, e.FromGroup.Id);
                        break;
                }
                return QMEventHandlerTypes.Continue;
            }
        }
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

        void Coins (long qq, long group)
        {
            int coins = Convert.ToInt32(MySqlHelper.ExecuteScalar($"select coins from checkin where qq = {qq}"));
            Reply(qq,group, $"你目前拥有{coins}枚金币");
        }

        void Reply (long qq, long group, object message)
        {
            if (group > 0)
            {
                QMApi.SendGroupMessage(Config.RobotQQ, group, $"[@{qq}]{Environment.NewLine}{message}");
                return;
            }
            QMApi.SendFriendMessage(Config.RobotQQ, qq, message.ToString());
        }

    }
}
