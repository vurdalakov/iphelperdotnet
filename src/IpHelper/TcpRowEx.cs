namespace Vurdalakov.IpHelperDotNet
{
    using System;
    using System.Diagnostics;

    public class TcpRowEx : TcpRow
    {
        public String ProcessName { get; private set; }

        public TcpRowEx(TcpRow tcpRow) : base(tcpRow)
        {
            try
            {
                switch (ProcessId)
                {
                    case 0:
                        ProcessName = "<System Idle>";
                        break;
                    case 4:
                        ProcessName = "<System>";
                        break;
                    default:
                        ProcessName = Process.GetProcessById(ProcessId).MainModule.ModuleName;
                        break;
                }
            }
            catch
            {
                ProcessName = "<access denied>";
            }
        }
    }
}
