using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChuckieEgg
{
    internal class GameUIManager
    {
        private PrivateFontCollection _pfc;
        private Dictionary<string, Font> _fonts;

        private Panel blackOverlay;
        private Label lblTitle;
        private Label lblPrompt;
        private RichTextBox messageRichText;

        private Dictionary<string, (string top, string bottom, Size? size)> _messagePresets = new()
        {
            ["intro"] = ("YOU ARE IN A HEN HOUSE ON A MISSION!\n\n",
                            "There are 8 unique levels.\n\n" +
                            "Your job? Collect every dozen eggs scattered across each level - before the hens catch you.\n\n" +
                            "Climb ladders, jump across platforms, and time your moves carefully.\n\n" +
                            "You start with 5 lives. Collect food and eggs to gain points — every 10,000 points gives you an extra life.\n\n" +
                            "Finish all levels, and the challenge loops ... it gets more difficult!",
                            null),
            ["howToPlay"] = ("HOW TO PLAY\n\n",
                             "Move    ...    Left, Right\n\n" +
                             "Climb   ...       Up, Down\n\n" +
                             "Jump    ...          Space\n\n\n\n" +
                             "Exit         ...       Esc\n\n" +
                             "Fullscreen   ...       F11\n\n\n\n" +
                             "Press SPACE to continue.",
                             null),
            ["thanks"] = ("\n\n\n\n\n\n\nTHANKS FOR PLAYING!", "", null),
            ["gameOver"] = ("Game Over", "Player 01", new Size(600, 300)),
            ["getReady"] = ("Get Ready", "Player 01", new Size(600, 300)),
            ["paused"] = ("Paused","", new Size(600, 300))
        };

        private Control.ControlCollection mainControls;
        private TableLayoutPanel tableLayoutPanel;
        private Label levelLabel;
        private Label scoreLabel;
        private Label bonusLabel;
        private Label timeLabel;

        private bool _inTransition = false;
        private float _curtainHeight = 0f;
        private DateTime? _transitionTimer = null;
        private Action? _onTransitionComplete = null;

        public bool IsInTransition => _inTransition;
        public float CurtainHeight => _curtainHeight;

        public GameUIManager(
            Control.ControlCollection controls, 
            TableLayoutPanel panel, 
            Label levelL,
            Label scoreL,
            Label bonusL,
            Label timeL)
        {
            mainControls = controls;
            tableLayoutPanel = panel;
            levelLabel = levelL;
            scoreLabel = scoreL;
            bonusLabel = bonusL;
            timeLabel = timeL;

            LoadFonts();
            StyleAllLabels();
            CreateBlackOverlay();
        }

        [MemberNotNull(nameof(_pfc), nameof(_fonts))]
        private void LoadFonts()
        {
            _pfc = new PrivateFontCollection();
            _pfc.AddFontFile("Resources/zx_spectrum-7.ttf");

            var family = _pfc.Families[0];
            _fonts = new Dictionary<string, Font>()
            {
                ["label"] = new Font(family, 18, FontStyle.Bold),
                ["labelLarge"] = new Font(family, 22, FontStyle.Bold),
                ["message"] = new Font(family, 26, FontStyle.Bold),
                ["title"] = new Font(family, 90, FontStyle.Bold)
            };
        }

        private void StyleAllLabels()
        {
            foreach (Label lbl in tableLayoutPanel.Controls.OfType<Label>())
            {
                lbl.Font = _fonts["label"];
                lbl.ForeColor = Color.Black;
                lbl.BackColor = Color.Magenta;

                lbl.Padding = new Padding(4, 2, 4, 2);
                lbl.Margin = new Padding(4, 2, 4, 2);

                lbl.AutoSize = true;
                lbl.Dock = DockStyle.None;

                var pos = tableLayoutPanel.GetPositionFromControl(lbl);

                if (pos.Column == 1 || pos.Column == 2)
                {
                    lbl.Anchor = AnchorStyles.Left;
                    lbl.TextAlign = ContentAlignment.BottomCenter;
                }
                else if (pos.Column == 4)
                {
                    lbl.Anchor = AnchorStyles.Right;
                    lbl.TextAlign = ContentAlignment.BottomCenter;
                }
                else
                {

                    lbl.Anchor = AnchorStyles.None;
                    lbl.TextAlign = ContentAlignment.BottomCenter;
                }
            }
        }

        public void ResizeUILayout(Size clientSize)
        {
            tableLayoutPanel.ColumnStyles.Clear();
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            for (int i = 1; i < 5; i++)
            {
                int newCellWidth = clientSize.Width > GameConstants.FormWidth ? 350 : 225;
                Font newFont = clientSize.Width > GameConstants.FormWidth ? _fonts["labelLarge"] : _fonts["label"];

                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, newCellWidth));
                foreach (Label lbl in tableLayoutPanel.Controls.OfType<Label>())
                {
                    lbl.Font = newFont;
                }
            }
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            
            tableLayoutPanel.RowStyles[0].SizeType = SizeType.Percent;
            tableLayoutPanel.RowStyles[0].Height = 30f;
            tableLayoutPanel.RowStyles[1].SizeType = SizeType.Percent;
            tableLayoutPanel.RowStyles[1].Height = 35f;
            tableLayoutPanel.RowStyles[2].SizeType = SizeType.Percent;
            tableLayoutPanel.RowStyles[2].Height = 35f;
            
        }

        private Label CreateLabel(Font font, Color foreColor)
        {
            return new Label
            {
                AutoSize = true,
                Font = font,
                ForeColor = foreColor,
                BackColor = Color.Black,
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false
            };
        }

        [MemberNotNull(nameof(blackOverlay), nameof(lblTitle), nameof(lblPrompt), nameof(messageRichText))]
        private void CreateBlackOverlay()
        {
            // Create black panel that covers everything
            blackOverlay = new Panel
            {
                BackColor = Color.Black,
                Dock = DockStyle.Fill,
                Visible = false,
            };

            // Big title label
            lblTitle = CreateLabel(_fonts["title"], Color.Yellow);
            lblPrompt = CreateLabel(_fonts["message"], Color.Cyan);

            messageRichText = new RichTextBox
            {
                BackColor = Color.Black,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                Font = _fonts["message"],
                SelectionAlignment = HorizontalAlignment.Center,
                Size = new Size(400, 150),
                Anchor = AnchorStyles.None,
                Visible = false
            };

            blackOverlay.Controls.Add(lblTitle);
            blackOverlay.Controls.Add(lblPrompt);
            blackOverlay.Controls.Add(messageRichText);
            mainControls.Add(blackOverlay);
            blackOverlay.BringToFront();
        }

        public void ShowTitle()
        {
            messageRichText.Visible = false;

            lblTitle.Text = "CHUCKIE EGG";
            lblPrompt.Text = "Press SPACE to continue.";
            lblTitle.Visible = true;
            lblPrompt.Visible = true;

            int w = blackOverlay.Width;
            int h = blackOverlay.Height;
            int cx = w / 2;

            float titleYFraction = 0.32f;
            int titleY = (int)(h * titleYFraction) - (lblTitle.Height / 2);

            float promptYFraction = 0.60f;
            int promptY = (int)(h * promptYFraction) - (lblPrompt.Height / 2);

            lblTitle.Location = new Point(cx - lblTitle.Width / 2, titleY);
            lblPrompt.Location = new Point(cx - lblPrompt.Width / 2, promptY);

            blackOverlay.Visible = true;
            blackOverlay.BringToFront();
        }

        public void ShowFullScreenMessage(string topMessage, string bottomMessage, Size? overrideSize = null)
        {
            messageRichText.Visible = true;
            messageRichText.Clear();

            messageRichText.SelectionColor = Color.Yellow;
            messageRichText.SelectionAlignment = HorizontalAlignment.Center;
            messageRichText.AppendText(topMessage + "\n\n");
            
            messageRichText.SelectionColor = Color.Cyan;
            messageRichText.SelectionAlignment = HorizontalAlignment.Center;
            messageRichText.AppendText(bottomMessage);

            Size targetSize = overrideSize ?? new Size(GameConstants.FormWidth + 200, GameConstants.FormHeight);
            int maxWidth = blackOverlay.Width - 100;
            int maxHeight = blackOverlay.Height - 100;
            targetSize.Width = Math.Min(targetSize.Width, maxWidth);
            targetSize.Height = Math.Min(targetSize.Height, maxHeight);

            messageRichText.Size = targetSize;

            messageRichText.Location = new Point(
                (blackOverlay.Width - messageRichText.Width) / 2,
                (blackOverlay.Height - messageRichText.Height) / 2
            );
            
            blackOverlay.Visible = true;
            blackOverlay.BringToFront();
        }

        public void ShowMessage(string key)
        {
            HideUI();
            HideFullScreenMessage();
            if (_messagePresets.TryGetValue(key, out var msg))
            {
                ShowFullScreenMessage(msg.top, msg.bottom, msg.size);
            }
        }

        public void HideFullScreenMessage()
        {
            blackOverlay.Visible = false;
            lblTitle.Visible = false;
            lblPrompt.Visible = false;
            messageRichText.Visible = false;
        }

        public void ShowUI()
        {
            foreach (Control ctrl in tableLayoutPanel.Controls.OfType<Label>())
            {
                ctrl.Visible = true;
            }
        }

        public void HideUI()
        {
            foreach (Control ctrl in tableLayoutPanel.Controls.OfType<Label>())
            {
                ctrl.Visible = false;
            }
        }

        public void UpdateUI(int level, int score, int bonus, int time)
        {
            levelLabel.Text = $"LEVEL {level:D2}";
            bonusLabel.Text = $"BONUS {bonus:D4}";
            scoreLabel.Text = $"{score:D6}";
            timeLabel.Text = $"TIME {time:D3}";

        }

        public void StartTransition(Action onComplete)
        {
            _inTransition = true;
            _curtainHeight = 0f;
            _transitionTimer = DateTime.Now;
            _onTransitionComplete = onComplete;
        }

        public void UpdateTransition(Size panelSize)
        {
            if (!_inTransition) return;

            var elapsed = (DateTime.Now - _transitionTimer.Value).TotalSeconds;
            var progress = Math.Min(elapsed / UIConstants.TransitionDuration, 1.0);

            if (progress <= UIConstants.TransitionMidpoint)
            {
                _curtainHeight = (float)(panelSize.Height * (progress * UIConstants.TransitionMultiplier));
            }
            else
            {
                _curtainHeight = (float)(panelSize.Height * (UIConstants.TransitionMultiplier - progress * UIConstants.TransitionMultiplier));

                if (_onTransitionComplete != null && progress > UIConstants.TransitionMidpoint)
                {
                    _onTransitionComplete();
                    _onTransitionComplete = null;
                }
            }

            if (progress >= 1.0)
            {
                _inTransition = false;
                _transitionTimer = null;
            }
        }
    }
}
