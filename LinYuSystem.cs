using com.LinYu.demo;
using QQMini.PluginSDK.Core;
using QQMini.PluginSDK.Core.Model;
using System;
using System.Collections.Generic;
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
                        PackageId = "com.linlin.demo",
                        Version = new Version(1, 0, 0, 0)
                    };
                }
                return _PluginInfo;
            }
        }

        PluginInfo _PluginInfo;

        public override QMEventHandlerTypes OnReceiveGroupMessage(QMGroupMessageEventArgs e)
        {
            long robotQQ = 377359254;
            string atCode = $"[@{robotQQ}]";
            if (e.Message.Text.Contains(atCode))
            {
                QMApi.SendGroupMessage(robotQQ, e.FromGroup.Id, e.Message.Text.Replace(atCode, string.Empty).Trim());
            }
            return QMEventHandlerTypes.Continue;
        }
        public override void OnOpenSettingMenu()
        {
            new MainWindow().Show();
        }
    }
}
