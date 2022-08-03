using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static WindowsFormsApp19.chessboardmanager;
using static WindowsFormsApp19.SocketData;

namespace WindowsFormsApp19
{
    public partial class Form1 : Form
    {
        #region properties
        chessboardmanager chessboard;

        SocketManager socket;

        #endregion

        public Form1()
        {
            InitializeComponent();

            Control.CheckForIllegalCrossThreadCalls = false;

            chessboard = new chessboardmanager(pnlchess, txtName, pictureBox2);

            chessboard.EndedGame += Chessboard_EndedGame;
            chessboard.PlayerMarked += Chessboard_PlayerMarked;

            prbar.Step = cons.cool_step;
            prbar.Maximum = cons.cool_time;
            prbar.Value = 0;


            tmbar.Interval = cons.cool_interval;

            socket = new SocketManager();

            NewGame();


        }

        #region Method
        void EndGame()
        {
            tmbar.Stop();
            pnlchess.Enabled = false;
            undoToolStripMenuItem.Enabled = false;
           //    MessageBox.Show("Kết thúc");
        }
        void NewGame()
        {
            prbar.Value = 0;
            tmbar.Stop();
            undoToolStripMenuItem.Enabled = true;
            chessboard.drawchess();

        }
        void Undo()
        {
            chessboard.Undo();
            prbar.Value = 0;

        }
        void Quit()
        {
            Application.Exit();

        }
        void Chessboard_PlayerMarked(object sender, ButtonClickEvent e)
        {
            tmbar.Start();
            pnlchess.Enabled = false;
            prbar.Value = 0;

            socket.Send(new SocketData((int)SocketCommand.SEND_POINT, "", e.ClickedPoint));

            undoToolStripMenuItem.Enabled = false;  
            Listen();
        }

        private void Chessboard_EndedGame(object sender, EventArgs e)
        {
            EndGame();

            socket.Send(new SocketData((int)SocketCommand.END_GAME, "", new Point()));

        }

        private void tmbar_Tick(object sender, EventArgs e)
        {
            prbar.PerformStep();

            if (prbar.Value >= prbar.Maximum)
            {
                EndGame();

                socket.Send(new SocketData((int)SocketCommand.TIME_OUT, "", new Point()));

            }
        }

        private void newGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewGame();
            socket.Send(new SocketData((int)SocketCommand.NEW_GAME, "", new Point()));

            pnlchess.Enabled = true;
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            chessboard.Undo();
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Quit();
        }



        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Bạn có muốn thoát", "Thông Báo", MessageBoxButtons.OKCancel) != System.Windows.Forms.DialogResult.OK)
            {
                e.Cancel = true;
            }
            else
            {
                try
                {
                    socket.Send(new SocketData((int)SocketCommand.QUIT, "", new Point()));
                }
                catch { }
            }
        }

        #endregion

        private void btnPlay_Click(object sender, EventArgs e)
        {
            socket.IP = txbLan.Text;

            if (!socket.ConnectSever())
            {
                socket.isSever = true;
                pnlchess.Enabled = true;
                socket.CreateSever();
            }
            else
            {
                socket.isSever = false;
                pnlchess.Enabled = false;
                Listen();
            }

        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            txbLan.Text = socket.GetlocalIPv4(NetworkInterfaceType.Wireless80211);
            if (string.IsNullOrEmpty(txbLan.Text))
            {
                txbLan.Text = socket.GetlocalIPv4(NetworkInterfaceType.Ethernet);
            }
        }
        void Listen()
        {
            Thread listenThread = new Thread(() =>
            {
                try
                {
                    SocketData data = (SocketData)socket.Receive();
                    ProcessData(data);
                }
                catch (Exception e)
                {

                }

            });
            listenThread.IsBackground = true;
            listenThread.Start();

        }

        private void ProcessData(SocketData data)
        {
            switch (data.Command)
            {
                case (int)SocketCommand.NOTIFY:
                    MessageBox.Show(data.Message);
                    break;
                case (int)SocketCommand.NEW_GAME:
                    this.Invoke((MethodInvoker)(() =>
                    {
                        NewGame();
                        pnlchess.Enabled = false;
                    }));
                    break;
                case (int)SocketCommand.SEND_POINT:
                    this.Invoke((MethodInvoker)(() =>
                    {
                        prbar.Value = 0;
                        pnlchess.Enabled = true;
                        tmbar.Start();
                        chessboard.OtherPlayerMark(data.Point);
                        undoToolStripMenuItem.Enabled = true;
                    }));                                      
                    break;
                case (int)SocketCommand.UNDO:
                    Undo();
                    prbar.Value = 0;
                    break;
                case (int)SocketCommand.END_GAME:
                    MessageBox.Show("Đã 5 con trên 1 dòng");
                    break;
                case (int)SocketCommand.TIME_OUT:
                    MessageBox.Show("Hết giờ");
                    break;
                case (int)SocketCommand.QUIT:
                    tmbar.Stop();
                    MessageBox.Show("Người chơi đã thoát");
                    break;
                default:
                    break;
            }
            Listen();
        }
    }
}
