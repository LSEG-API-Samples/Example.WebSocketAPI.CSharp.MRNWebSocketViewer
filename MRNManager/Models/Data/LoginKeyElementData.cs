using System;
using System.Collections.Generic;
using System.Text;

namespace MarketDataWebSocket.Models.Data
{
    public class LoginKeyElementData
    {
        public int AllowSuspectData { get; set; }
        public string ApplicationId {get;set;}
        public string ApplicationName { get; set; }
        public string Position { get; set; }
        public int ProvidePermissionExpressions { get; set; }
        public int ProvidePermissionProfile { get; set; }
        public int SingleOpen { get; set; }
        public int SupportBatchRequests { get; set; }
        public int SupportEnhancedSymbolList { get; set; }
        public int SupportOMMPost { get; set; }
        public int SupportOptimizedPauseResume { get; set; }
        public int SupportPauseResume { get; set; }
        public int SupportStandby { get; set; }
        public int SupportViewRequests { get; set; }

        public override string ToString()
        {
            var msg=new StringBuilder();
            msg.Append($"AllowSuspectData:{this.AllowSuspectData}\n");
            msg.Append($"ApplicationId:{this.ApplicationId}\n");
            msg.Append($"ApplicationName:{this.ApplicationName}\n");
            msg.Append($"Position:{this.Position}\n");
            msg.Append($"ProvidePermissionExpressions:{this.ProvidePermissionExpressions}\n");
            msg.Append($"ProvidePermissionProfile:{this.ProvidePermissionProfile}\n");
            msg.Append($"SingleOpen:{this.SingleOpen}\n");
            msg.Append($"SupportBatchRequests:{this.SupportBatchRequests}\n");
            msg.Append($"SupportEnhancedSymbolList:{this.SupportEnhancedSymbolList}\n");
            msg.Append($"SupportOMMPos:{this.SupportOMMPost}\n");
            msg.Append($"SupportOptimizedPauseResume:{this.SupportOptimizedPauseResume}\n");
            msg.Append($"SupportPauseResume:{this.SupportPauseResume}\n");
            msg.Append($"SupportStandby:{this.SupportStandby}\n");
            msg.Append($"SupportViewRequests:{this.SupportViewRequests}\n");
            return msg.ToString();

        }
    }
}
