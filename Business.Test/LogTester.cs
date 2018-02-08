using Salmon.Common.ScLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Test
{
    public class LogTester
    {
        public void WriteLog()
        {
            LoggerFactory.CommonLog.Info("Write Log Finished!");
        }
    }
}
