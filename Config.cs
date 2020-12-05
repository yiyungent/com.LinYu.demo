using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.LinYu.demo
{
    public class Config
    {

        public long RobotQQ { get; set; }  = 377359254;
        public string DatabaseIP { get; set; } = "localhost";
        public string DatabaseName { get; set; } = "com.linlin.demo";
        public string DatabaseAccount { get; set; } = "root";
        public string DatabasePassword { get; set; } = "751212";
        public int CheckInMaxCoins { get; set; } = 20;
        public int CheckInMinCoins { get; set; } = 0;

    }
}
