using System.Drawing.Design;
using System.Drawing.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Media;
using System.Xml.Linq;
using System.Drawing.Printing;
using System.Drawing.Drawing2D;

namespace ChuckieEgg
{
    public enum GameState
    {
        TitleScreen,
        Introduction,
        Instructions,
        Menu,
        GetReady,
        Playing,
        Paused,
        GameOver,
        LevelComplete,
        Exiting
    }

    public partial class Form1 : Form
    {
        private Game _game;
        private GameUIManager _uiManager;
        private bool _isFullscreen = true;
        private HashSet<Keys> _pressedKeys = new HashSet<Keys>();
        
        public Form1()
        {
            InitializeComponent();
            _uiManager = new GameUIManager(this.Controls, tableLayoutPanel1, LLevel, LScoreValue, LBonus, LTime);
            _game = new Game(_uiManager);
            SetupDoubleBuffering();
            SetupForm();
        }

        private void SetupDoubleBuffering()
        {
            // Game panel flickering on invalidate:
            // solution found at: https://stackoverflow.com/questions/8046560/how-to-stop-flickering-c-sharp-winforms
            typeof(Panel).InvokeMember("DoubleBuffered",
            BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
            null, PGamePanel, new object[] { true });
        }

        private void SetupForm()
        {
            this.BackColor = Color.Black;
            this.KeyPreview = true;
            this.FormBorderStyle = FormBorderStyle.None;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            this.SizeGripStyle = SizeGripStyle.Hide;
            this.MinimumSize = new Size(GameConstants.FormWidth / 2, GameConstants.FormHeight / 2);
            this.DoubleBuffered = true;
            this.Visible = true;
            Cursor.Hide();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Interval = GameConstants.TimerInterval;
            timer1.Tick += timer1_Tick;
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (_uiManager.IsInTransition)
            {
                _uiManager.UpdateTransition(PGamePanel.ClientSize);
                PGamePanel.Invalidate();
                return;
            }

            switch (_game.CurrentState)
            {
                case GameState.TitleScreen:
                    _uiManager.HideUI();
                    _uiManager.ShowTitle();
                    break;
                case GameState.Paused:
                    break;
                case GameState.Playing:
                    _game.Update(_pressedKeys);
                    break;
            }
            PGamePanel.Invalidate();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                _uiManager.ShowMessage("thanks");
                _game.ChangeState(GameState.Exiting);
                _uiManager.StartTransition(async () =>
                {
                    await Task.Delay(500);
                    this.Close();
                });
                return true;
            }

            if (keyData == Keys.F11)
            {
                ToggleFullscreen();
            }

            if (keyData == Keys.P)
            {
                switch (_game.CurrentState)
                {
                    case GameState.Paused:
                        _game.ChangeState(GameState.Playing);
                        //_uiManager.HideFullScreenMessage();
                        break;
                    case GameState.Playing:
                        _game.ChangeState(GameState.Paused);
                        //_uiManager.ShowMessage("paused");
                        break;
                    default:
                        break;
                }
                
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (_game.CurrentState)
            {
                case GameState.TitleScreen:
                    if (e.KeyCode == Keys.Space)
                    {
                        _uiManager.ShowMessage("intro");
                        _game.ChangeState(GameState.Introduction);
                    }
                    break;
                case GameState.Introduction:
                    if (e.KeyCode == Keys.Space)
                    {
                        _uiManager.ShowMessage("howToPlay");
                        _game.ChangeState(GameState.Instructions);
                    }
                    break;
                case GameState.Instructions:
                    if (e.KeyCode == Keys.Space)
                    {
                        _uiManager.ShowMessage("getReady");
                        _game.ChangeState(GameState.GetReady);
                    }
                    break;
                case GameState.GetReady:
                    _uiManager.HideFullScreenMessage();
                    _uiManager.StartTransition(() =>
                    {
                        _game.LoadLevel();
                    });
                    _game.ChangeState(GameState.Playing);  // Start Game
                    break;
                case GameState.Playing:
                    _pressedKeys.Add(e.KeyCode);
                    break;
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (_game.CurrentState == GameState.Playing)
            {
                _pressedKeys.Remove(e.KeyCode);
            }
        }

        private void P_GamePanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.CompositingMode = CompositingMode.SourceOver;
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.SmoothingMode = SmoothingMode.None;
            e.Graphics.Clear(Color.Black);

            _game.Draw(e.Graphics, PGamePanel.ClientSize);

            // Transition overlay
            if (_uiManager.IsInTransition && _uiManager.CurtainHeight > 0)
            {
                using (Brush blackBrush = new SolidBrush(Color.Black))
                {
                    e.Graphics.FillRectangle(blackBrush, 0, 0, this.ClientSize.Width, _uiManager.CurtainHeight);
                }
            }

            //using (SolidBrush dimBrush = new SolidBrush(Color.FromArgb(40, 0, 0, 0)))
            //{
            //    e.Graphics.FillRectangle(dimBrush, PGamePanel.ClientRectangle);
            //}
        }

        private void ToggleFullscreen()
        {
            if (_isFullscreen)
            {
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.WindowState = FormWindowState.Normal;
                this.ShowIcon = true;
                this.Size = new Size(GameConstants.FormWidth, GameConstants.FormHeight);
                Cursor.Show();
            }
            else
            {
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
                Cursor.Hide();
            }

            _isFullscreen = !_isFullscreen;
        }

        private void Form1_Resize_1(object sender, EventArgs e)
        {
            if (_uiManager == null) return;
            _uiManager.ResizeUILayout(this.ClientSize);
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void LBonus_Click(object sender, EventArgs e)
        {

        }
    }
}
