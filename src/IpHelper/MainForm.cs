namespace Vurdalakov.IpHelperDotNet
{
    using System;
    using System.Diagnostics;
    using System.Windows.Forms;

    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.toolStripStatusLabelAdmin.Text = ProcessHelper.IsAdministrator() ? "Administrator" : "Normal User";

            this.RefreshList();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.RefreshList();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MsgBox.Info("IP Helper\n\nCopyright (c) 2016 Vurdalakov\n\nvurdalakov@gmail.com\nhttp://github.com/vurdalakov/iphelperdotnet");
        }

        private TcpTableEx _tcpTable = new TcpTableEx();

        private void RefreshList()
        {
            this._tcpTable.Refresh();

            this.listView.VirtualListSize = this._tcpTable.Count;

            this.listView.Invalidate();
        }

        private void listView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            var row = _tcpTable[e.ItemIndex];

            e.Item = new ListViewItem(row.ProcessName);
            e.Item.SubItems.Add(row.ProcessId.ToString());
            e.Item.SubItems.Add(row.Protocol.ToString());
            e.Item.SubItems.Add(row.LocalAddress.ToString());
            e.Item.SubItems.Add(row.LocalPort.ToString());
            if (null == row.RemoteAddress) // UDP
            {
                e.Item.SubItems.Add("");
                e.Item.SubItems.Add("");
                e.Item.SubItems.Add("");
            }
            else // TCP
            {
                e.Item.SubItems.Add(row.RemoteAddress.ToString());
                e.Item.SubItems.Add(row.RemotePort.ToString());
                e.Item.SubItems.Add(row.State.ToString());
            }
        }

        private void listView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            _tcpTable.Sort(e.Column);

            this.listView.Invalidate();
        }
    }
}
