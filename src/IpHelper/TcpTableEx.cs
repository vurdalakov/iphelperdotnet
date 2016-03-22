namespace Vurdalakov.IpHelperDotNet
{
    using System;
    using System.Collections.Generic;
    using System.Net.NetworkInformation;

    public class TcpTableEx : List<TcpRowEx>
    {
        private TcpRowExComparer _tcpRowExComparer = new TcpRowExComparer(1, true);

        public Int32 EstablishedCount { get; private set; }
        public Int32 ListeningCount { get; private set; }
        public Int32 TimeWaitCount { get; private set; }
        public Int32 CloseWaitCount { get; private set; }

        public void Refresh()
        {
            this.Clear();

            EstablishedCount = 0;
            ListeningCount = 0;
            TimeWaitCount = 0;
            CloseWaitCount = 0;

            var tcpRows = IpHelper.GetRows(true);
            foreach (var tcpRow in tcpRows)
            {
                this.Add(new TcpRowEx(tcpRow));

                switch (tcpRow.State)
                {
                    case TcpState.Established:
                        EstablishedCount++;
                        break;
                    case TcpState.Listen:
                        ListeningCount++;
                        break;
                    case TcpState.TimeWait:
                        TimeWaitCount++;
                        break;
                    case TcpState.CloseWait:
                        CloseWaitCount++;
                        break;
                }
            }

            this.Sort(_tcpRowExComparer);
        }

        public void Sort(Int32 sortColumn)
        {
            _tcpRowExComparer = new TcpRowExComparer(sortColumn,
                sortColumn == _tcpRowExComparer.SortColumn ? !_tcpRowExComparer.SortOrderAscending : true);

            this.Sort(_tcpRowExComparer);
        }
    }

    public class TcpRowExComparer : Comparer<TcpRowEx>
    {
        public Int32 SortColumn { get; set; }
        public Boolean SortOrderAscending { get; set; }

        public TcpRowExComparer(Int32 sortColumn, Boolean sortOrderAscending)
        {
            SortColumn = sortColumn;
            SortOrderAscending = sortOrderAscending;
        }

        public override int Compare(TcpRowEx x, TcpRowEx y)
        {
            var t = typeof(TcpRowEx);

            String[] props = { "ProcessName", "ProcessId", "Protocol", "LocalAddress", "LocalPort", "RemoteAddress", "RemotePort", "State" };
            var p = t.GetProperty(props[SortColumn]);

            var xv = p.GetValue(x, null);
            var yv = p.GetValue(y, null);
            if (p.PropertyType.Equals(typeof(Int32)))
            {
                return ((Int32)xv).CompareTo((Int32)yv);
            }
            else
            {
                return xv.ToString().CompareTo(yv.ToString());
            }
        }
    }
}
