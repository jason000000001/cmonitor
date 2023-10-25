﻿using cmonitor.server.client.reports.share;
using common.libs;

namespace cmonitor.server.client.reports.llock
{
    public sealed class LLockReport : IReport
    {
        public string Name => "LLock";

        private LLockReportInfo report = new LLockReportInfo();
        bool lastValue = false;

        private readonly Config config;
        private readonly ShareReport shareReport;
        private readonly ClientConfig clientConfig;

        public LLockReport(Config config, ShareReport shareReport, ClientConfig clientConfig)
        {
            this.config = config;
            this.shareReport = shareReport;
            this.clientConfig = clientConfig;

            Update(clientConfig.LLock);
        }

        DateTime startTime = new DateTime(1970, 1, 1);
        public object GetReports(ReportType reportType)
        {
            if (shareReport.GetShare(Name, out ShareItemInfo share) && string.IsNullOrWhiteSpace(share.Value) == false && long.TryParse(share.Value, out long time))
            {
                report.Value = (long)(DateTime.UtcNow.Subtract(startTime)).TotalMilliseconds - time < 1000;
            }
            if (reportType == ReportType.Full || report.Value != lastValue)
            {
                lastValue = report.Value;
                return report;
            }
            return null;
        }

        public void Update(bool open)
        {
            clientConfig.LLock = open;
            Task.Run(() =>
            {
                CommandHelper.Windows(string.Empty, new string[] { "taskkill /f /t /im \"llock.win.exe\"" });
                if (open)
                {
                    CommandHelper.Windows(string.Empty, new string[] {
                        $"start llock.win.exe {config.ShareMemoryKey} {config.ShareMemoryLength} {Config.ShareMemoryLLockIndex}"
                    });
                }
            });
        }
    }

    public sealed class LLockReportInfo
    {
        public bool Value { get; set; }
    }
}

