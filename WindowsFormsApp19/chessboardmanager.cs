using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp19
{
    public class chessboardmanager
    {
        #region properties (thuộc tính khu vực)
        private Panel chessboard;
        private List<Player> player;

        public Panel Chessboard { get => chessboard; set => chessboard = value; }
        public List<Player> Player { get => player; set => player = value; }

        private int currentPlayer;
        public int CurrentPlayer { get => currentPlayer; set => currentPlayer = value; }

        private TextBox playerName;

        public TextBox PlayerName { get => playerName; set => playerName = value; }

        private PictureBox playerMark;

        public PictureBox PlayerMark { get => playerMark; set => playerMark = value; }

        private List<List<Button>> matrix;
        public List<List<Button>> Matrix { get => matrix; set => matrix = value; }

        private event EventHandler<ButtonClickEvent> playerMarked;
        public event EventHandler<ButtonClickEvent> PlayerMarked
        {
            add
            {
                playerMarked += value;
            }
            remove
            {
                playerMarked -= value;
            }
        }

        private event EventHandler endedGame;
        public event EventHandler EndedGame
        {
            add
            {
                endedGame += value;
            }
            remove
            {
                endedGame -= value;
            }
        }

        private Stack<PlayInfo> playTimeLine;
        public Stack<PlayInfo> PlayTimeLine { get => playTimeLine; set => playTimeLine = value; }


        #endregion
        #region Initialize (khởi tạo vùng)
        public chessboardmanager(Panel chessboard, TextBox playerName, PictureBox mark)
        {
            this.chessboard = chessboard;
            this.PlayerName = playerName;
            this.PlayerMark = mark;

            this.player = new List<Player>()
            {
                new Player("NV1",Image.FromFile(Application.StartupPath + "\\Resources\\o.jpg")),
                new Player("NV2",Image.FromFile(Application.StartupPath + "\\Resources\\x.jpg"))

            };

        }
        #endregion
        #region Method (phương pháp vùng)
        public void drawchess()
        {
            chessboard.Enabled = true;
            chessboard.Controls.Clear();
            playTimeLine = new Stack<PlayInfo>();

            currentPlayer = 0;

            Changeplayer();

            Matrix = new List<List<Button>>();

            Button oldbutton = new Button() { Width = 0, Location = new Point(0, 0) };
            for (int i = 0; i < cons.chessh; i++)
            {
                Matrix.Add(new List<Button>());
                for (int j = 0; j < cons.chessw; j++)
                {
                    Button btn = new Button()
                    {
                        Width = cons.chesswidth,
                        Height = cons.chessheight,
                        Location = new Point(oldbutton.Location.X + oldbutton.Width, oldbutton.Location.Y),
                        BackgroundImageLayout = ImageLayout.Stretch,
                        Tag = i.ToString()
                    };
                    btn.Click += Btn_Click;

                    chessboard.Controls.Add(btn);

                    Matrix[i].Add(btn);

                    oldbutton = btn;
                }
                oldbutton.Location = new Point(0, oldbutton.Location.Y + cons.chessheight);
                oldbutton.Width = 0;
                oldbutton.Height = 0;

            }
        }

        private void Btn_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;

            if (btn.BackgroundImage != null)
                return;

            Mark(btn);

            playTimeLine.Push(new PlayInfo(GetChessPoin(btn), currentPlayer));

            currentPlayer = currentPlayer == 1 ? 0 : 1;

            Changeplayer();

            if (playerMarked != null)
                playerMarked(this, new ButtonClickEvent(GetChessPoin(btn)));

            if (isEndGame(btn))
            {
                EndGame();
            }           
        }
        public void OtherPlayerMark(Point point)
        {
            Button btn = Matrix[point.Y][point.X];

            if (btn.BackgroundImage != null)
                return;

            Mark(btn);

            playTimeLine.Push(new PlayInfo(GetChessPoin(btn), currentPlayer));

            currentPlayer = currentPlayer == 1 ? 0 : 1;

            Changeplayer();
            
            if (isEndGame(btn))
            {
                EndGame();
            }
        }

        public bool Undo()
        {

            if (PlayTimeLine.Count <= 0)
                return false;

            bool isUndo1 = UndoAstep();
            bool isUndo2 = UndoAstep();

            PlayInfo oldPoint = PlayTimeLine.Peek();

            currentPlayer = oldPoint.CurrentPlayer == 1 ? 0 : 1;

            return isUndo1 && isUndo2;
        }
        private bool UndoAstep()
        {


            PlayInfo oldPoint = PlayTimeLine.Pop();
            Button btn = Matrix[oldPoint.Point.Y][oldPoint.Point.X];
            btn.BackgroundImage = null;


            if (PlayTimeLine.Count <= 0)
            {
                currentPlayer = 0;
            }
            else
            {
                oldPoint = PlayTimeLine.Peek();
            }
            Changeplayer();

            return true;
        }
        private void EndGame()
        {
            if (endedGame != null)
                endedGame(this, new EventArgs());
        }

        private bool isEndGame(Button btn)
        {
            return isEndHorizontal(btn) || isEndVertical(btn) || isEndPrimary(btn) || isEndSub(btn);
        }

        private Point GetChessPoin(Button btn)
        {


            int vertical = Convert.ToInt32(btn.Tag);
            int horizontal = Matrix[vertical].IndexOf(btn);

            Point point = new Point(horizontal, vertical);

            return point;
        }
        private bool isEndHorizontal(Button btn)
        {

            Point point = GetChessPoin(btn);

            int countLeft = 0;
            for (int i = point.X; i >= 0; i--)
            {
                if (Matrix[point.Y][i].BackgroundImage == btn.BackgroundImage)
                {
                    countLeft++;
                }
                else
                    break;
            }

            int countRight = 0;
            for (int i = point.X + 1; i < cons.chessw; i++)
            {
                if (Matrix[point.Y][i].BackgroundImage == btn.BackgroundImage)
                {
                    countRight++;
                }
                else
                    break;
            }

            return countLeft + countRight == 5;
        }
        private bool isEndVertical(Button btn)
        {
            Point point = GetChessPoin(btn);

            int countTop = 0;
            for (int i = point.Y; i >= 0; i--)
            {
                if (Matrix[i][point.X].BackgroundImage == btn.BackgroundImage)
                {
                    countTop++;
                }
                else
                    break;
            }

            int countBottom = 0;
            for (int i = point.Y + 1; i < cons.chessh; i++)
            {
                if (Matrix[i][point.X].BackgroundImage == btn.BackgroundImage)
                {
                    countBottom++;
                }
                else
                    break;
            }
            return countTop + countBottom == 5;
        }
        private bool isEndPrimary(Button btn)
        {
            Point point = GetChessPoin(btn);

            int countTop = 0;
            for (int i = 0; i <= point.X; i++)
            {
                if (point.X - i < 0 || point.Y - i < 0)
                    break;

                if (Matrix[point.Y - i][point.X - i].BackgroundImage == btn.BackgroundImage)
                {
                    countTop++;
                }
                else
                    break;
            }

            int countBottom = 0;
            for (int i = 1; i <= cons.chessw - point.X; i++)
            {
                if (point.Y + i >= cons.chessh || point.X + i >= cons.chessw)
                    break;

                if (Matrix[point.Y + i][point.X + i].BackgroundImage == btn.BackgroundImage)
                {
                    countBottom++;
                }
                else
                    break;
            }
            return countTop + countBottom == 5;
        }
        private bool isEndSub(Button btn)
        {
            Point point = GetChessPoin(btn);

            int countTop = 0;
            for (int i = 0; i <= point.X; i++)
            {
                if (point.X + i > cons.chessw || point.Y - i < 0)
                    break;

                if (Matrix[point.Y - i][point.X + i].BackgroundImage == btn.BackgroundImage)
                {
                    countTop++;
                }
                else
                    break;
            }

            int countBottom = 0;
            for (int i = 1; i <= cons.chessw - point.X; i++)
            {
                if (point.Y + i >= cons.chessh || point.X - i < 0)
                    break;

                if (Matrix[point.Y + i][point.X + i].BackgroundImage == btn.BackgroundImage)
                {
                    countBottom++;
                }
                else
                    break;
            }
            return countTop + countBottom == 5;
        }
        private void Mark(Button btn)
        {
            btn.BackgroundImage = Player[currentPlayer].Mark;

        }
        private void Changeplayer()
        {
            PlayerName.Text = Player[currentPlayer].Name;

            playerMark.Image = Player[currentPlayer].Mark;
        }

        #endregion


        public class ButtonClickEvent : EventArgs
        {
            private Point clickedPoint;

            public Point ClickedPoint { get => clickedPoint; set => clickedPoint = value; }
        
        public ButtonClickEvent(Point point)
        {
            this.ClickedPoint = point;
        }
            } 
    }
}
