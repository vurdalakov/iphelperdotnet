namespace Vurdalakov.IpHelperDotNet
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Runtime.InteropServices;

    // A managed wrapper around IP Helper API.
    // The Internet Protocol Helper (IP Helper) API enables the retrieval and modification of network configuration settings for the local computer.
    // https://msdn.microsoft.com/en-us/library/aa366073.aspx

    public enum TcpRowProtocol
    {
        TCP,
        TCPv6,
        UDP,
        UDPv6
    }

    public class TcpRow
    {
        public IPAddress LocalAddress { get; private set; }
        public Int32 LocalPort { get; private set; }
        public IPAddress RemoteAddress { get; private set; }
        public Int32 RemotePort { get; private set; }
        public TcpState State { get; private set; }
        public Int32 ProcessId { get; private set; }
        public TcpRowProtocol Protocol { get; private set; }

        public TcpRow(TcpRow tcpRow)
        {
            this.LocalAddress = tcpRow.LocalAddress;
            this.LocalPort = tcpRow.LocalPort;

            this.RemoteAddress = tcpRow.RemoteAddress;
            this.RemotePort = tcpRow.RemotePort;

            this.State = tcpRow.State;
            this.ProcessId = tcpRow.ProcessId;

            this.Protocol = tcpRow.Protocol;
        }

        public TcpRow(IpHelperNative.MIB_TCPROW_OWNER_PID tcpRow, TcpRowProtocol protocol)
        {
            this.LocalAddress = new IPAddress(tcpRow.dwLocalAddr);
            this.LocalPort = NetworkToHostOrder(tcpRow.dwLocalPort);

            this.RemoteAddress = new IPAddress(tcpRow.dwRemoteAddr);
            this.RemotePort = NetworkToHostOrder(tcpRow.dwRemotePort);

            this.State = tcpRow.dwState;
            this.ProcessId = (int)tcpRow.dwOwningPid;

            this.Protocol = protocol;
        }

        public TcpRow(IpHelperNative.MIB_TCP6ROW_OWNER_PID tcpRow, TcpRowProtocol protocol)
        {
            this.LocalAddress = new IPAddress(tcpRow.ucLocalAddr);
            this.LocalPort = NetworkToHostOrder(tcpRow.dwLocalPort);

            if (49667 == this.LocalPort)
                Console.Write("1");

            this.RemoteAddress = new IPAddress(tcpRow.ucRemoteAddr);
            this.RemotePort = NetworkToHostOrder(tcpRow.dwRemotePort);

            this.State = tcpRow.dwState;
            this.ProcessId = (int)tcpRow.dwOwningPid;

            this.Protocol = protocol;
        }

        public TcpRow(IpHelperNative.MIB_UDPROW_OWNER_PID udpRow, TcpRowProtocol protocol)
        {
            this.LocalAddress = new IPAddress(udpRow.dwLocalAddr);
            this.LocalPort = NetworkToHostOrder(udpRow.dwLocalPort);

            this.ProcessId = (int)udpRow.dwOwningPid;

            this.Protocol = protocol;
        }

        public TcpRow(IpHelperNative.MIB_UDP6ROW_OWNER_PID udpRow, TcpRowProtocol protocol)
        {
            this.LocalAddress = new IPAddress(udpRow.ucLocalAddr);
            this.LocalPort = NetworkToHostOrder(udpRow.dwLocalPort);

            this.ProcessId = (int)udpRow.dwOwningPid;

            this.Protocol = protocol;
        }

        private int NetworkToHostOrder(UInt32 port)
        {
            return (int)(((port & 0xFF000000) >> 8) | ((port & 0x00FF0000) << 8) | ((port & 0x0000FF00) >> 8) | ((port & 0x000000FF) << 8));
        }
    }

    public static class IpHelper
    {
        public static TcpRow[] GetRows(bool sorted)
        {
            var tcpRows = new List<TcpRow>();

            GetTcpRows(sorted, tcpRows, IpHelperNative.AF_INET);
            GetTcpRows(sorted, tcpRows, IpHelperNative.AF_INET6);
            GetUdpRows(sorted, tcpRows, IpHelperNative.AF_INET);
            GetUdpRows(sorted, tcpRows, IpHelperNative.AF_INET6);

            return tcpRows.ToArray();
        }

        private static TcpRow[] GetTcpRows(bool sorted, List<TcpRow> tcpRows, UInt32 af)
        {
            UInt32 tableSize = 0;

            if (IpHelperNative.GetExtendedTcpTable(IntPtr.Zero, ref tableSize, sorted, af, IpHelperNative.TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL, 0) != 0)
            {
                IntPtr tablePtr = IntPtr.Zero;

                try
                {
                    tablePtr = Marshal.AllocHGlobal((int)tableSize);

                    if (0 == IpHelperNative.GetExtendedTcpTable(tablePtr, ref tableSize, sorted, af, IpHelperNative.TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL, 0))
                    {
                        IpHelperNative.MIB_TCPTABLE_OWNER_PID table =
                            (IpHelperNative.MIB_TCPTABLE_OWNER_PID)Marshal.PtrToStructure(tablePtr, typeof(IpHelperNative.MIB_TCPTABLE_OWNER_PID));

                        IntPtr rowPtr = (IntPtr)((long)tablePtr + Marshal.SizeOf(table.dwNumEntries));
                        for (int i = 0; i < table.dwNumEntries; ++i)
                        {
                            if (IpHelperNative.AF_INET == af)
                            {
                                tcpRows.Add(new TcpRow((IpHelperNative.MIB_TCPROW_OWNER_PID)Marshal.PtrToStructure(rowPtr, typeof(IpHelperNative.MIB_TCPROW_OWNER_PID)),
                                    TcpRowProtocol.TCP));
                                rowPtr = (IntPtr)((long)rowPtr + Marshal.SizeOf(typeof(IpHelperNative.MIB_TCPROW_OWNER_PID)));
                            }
                            else
                            {
                                tcpRows.Add(new TcpRow((IpHelperNative.MIB_TCP6ROW_OWNER_PID)Marshal.PtrToStructure(rowPtr, typeof(IpHelperNative.MIB_TCP6ROW_OWNER_PID)),
                                    TcpRowProtocol.TCPv6));
                                rowPtr = (IntPtr)((long)rowPtr + Marshal.SizeOf(typeof(IpHelperNative.MIB_TCP6ROW_OWNER_PID)));
                            }
                        }
                    }
                }
                finally
                {
                    if (tablePtr != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(tablePtr);
                    }
                }
            }

            return tcpRows.ToArray();
        }

        private static TcpRow[] GetUdpRows(bool sorted, List<TcpRow> tcpRows, UInt32 af)
        {
            UInt32 tableSize = 0;

            if (IpHelperNative.GetExtendedUdpTable(IntPtr.Zero, ref tableSize, sorted, af, IpHelperNative.UDP_TABLE_CLASS.UDP_TABLE_OWNER_PID, 0) != 0)
            {
                IntPtr tablePtr = IntPtr.Zero;

                try
                {
                    tablePtr = Marshal.AllocHGlobal((int)tableSize);

                    if (0 == IpHelperNative.GetExtendedUdpTable(tablePtr, ref tableSize, sorted, af, IpHelperNative.UDP_TABLE_CLASS.UDP_TABLE_OWNER_PID, 0))
                    {
                        IpHelperNative.MIB_UDPTABLE_OWNER_PID table =
                            (IpHelperNative.MIB_UDPTABLE_OWNER_PID)Marshal.PtrToStructure(tablePtr, typeof(IpHelperNative.MIB_UDPTABLE_OWNER_PID));

                        IntPtr rowPtr = (IntPtr)((long)tablePtr + Marshal.SizeOf(table.dwNumEntries));
                        for (int i = 0; i < table.dwNumEntries; ++i)
                        {
                            if (IpHelperNative.AF_INET == af)
                            {
                                tcpRows.Add(new TcpRow((IpHelperNative.MIB_UDPROW_OWNER_PID)Marshal.PtrToStructure(rowPtr, typeof(IpHelperNative.MIB_UDPROW_OWNER_PID)),
                                    TcpRowProtocol.UDP));
                                rowPtr = (IntPtr)((long)rowPtr + Marshal.SizeOf(typeof(IpHelperNative.MIB_UDPROW_OWNER_PID)));
                            }
                            else
                            {
                                tcpRows.Add(new TcpRow((IpHelperNative.MIB_UDP6ROW_OWNER_PID)Marshal.PtrToStructure(rowPtr, typeof(IpHelperNative.MIB_UDP6ROW_OWNER_PID)),
                                    TcpRowProtocol.UDPv6));
                                rowPtr = (IntPtr)((long)rowPtr + Marshal.SizeOf(typeof(IpHelperNative.MIB_UDP6ROW_OWNER_PID)));
                            }
                        }
                    }
                }
                finally
                {
                    if (tablePtr != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(tablePtr);
                    }
                }
            }

            return tcpRows.ToArray();
        }

        public static TcpRow[] GetRowsByPid(int processId)
        {
            List<TcpRow> tcpRows = new List<TcpRow>();

            foreach (var tcpRow in IpHelper.GetRows(true))
            {
                if (tcpRow.ProcessId == processId)
                {
                    tcpRows.Add(tcpRow);
                }
            }

            return tcpRows.ToArray();
        }
    }

    public class IpHelperNative
    {
        public const UInt32 AF_INET = 2;
        public const UInt32 AF_INET6 = 23;

        [DllImport("iphlpapi.dll", SetLastError = true)]
        public static extern uint GetExtendedTcpTable(IntPtr pTcpTable, ref UInt32 pdwSize, Boolean bOrder, UInt32 ulAf, TCP_TABLE_CLASS TableClass, UInt32 Reserved);

        public enum TCP_TABLE_CLASS
        {
            TCP_TABLE_BASIC_LISTENER,
            TCP_TABLE_BASIC_CONNECTIONS,
            TCP_TABLE_BASIC_ALL,
            TCP_TABLE_OWNER_PID_LISTENER,
            TCP_TABLE_OWNER_PID_CONNECTIONS,
            TCP_TABLE_OWNER_PID_ALL,
            TCP_TABLE_OWNER_MODULE_LISTENER,
            TCP_TABLE_OWNER_MODULE_CONNECTIONS,
            TCP_TABLE_OWNER_MODULE_ALL
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_TCPTABLE_OWNER_PID
        {
            public UInt32 dwNumEntries;
            public MIB_TCPROW_OWNER_PID table;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_TCPROW_OWNER_PID
        {
            public TcpState dwState;
            public UInt32 dwLocalAddr;
            public UInt32 dwLocalPort;
            public UInt32 dwRemoteAddr;
            public UInt32 dwRemotePort;
            public UInt32 dwOwningPid;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_TCP6ROW_OWNER_PID
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public Byte[] ucLocalAddr;
            public UInt32 dwLocalScopeId;
            public UInt32 dwLocalPort;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public Byte[] ucRemoteAddr;
            public UInt32 dwRemoteScopeId;
            public UInt32 dwRemotePort;
            public TcpState dwState;
            public UInt32 dwOwningPid;
        }

        [DllImport("iphlpapi.dll", SetLastError = true)]
        public static extern uint GetExtendedUdpTable(IntPtr pUdpTable, ref UInt32 pdwSize, Boolean bOrder, UInt32 ulAf, UDP_TABLE_CLASS TableClass, UInt32 Reserved);

        public enum UDP_TABLE_CLASS
        {
            UDP_TABLE_BASIC,
            UDP_TABLE_OWNER_PID,
            UDP_TABLE_OWNER_MODULE
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_UDPTABLE_OWNER_PID
        {
            public UInt32 dwNumEntries;
            public MIB_UDPROW_OWNER_PID table;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_UDPROW_OWNER_PID
        {
            public UInt32 dwLocalAddr;
            public UInt32 dwLocalPort;
            public UInt32 dwOwningPid;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_UDP6ROW_OWNER_PID
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public Byte[] ucLocalAddr;
            public UInt32 dwLocalScopeId;
            public UInt32 dwLocalPort;
            public UInt32 dwOwningPid;
        }
    }
}
