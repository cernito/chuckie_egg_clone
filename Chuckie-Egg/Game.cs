using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;   
using System.Xml.Linq;

namespace ChuckieEgg
{
    internal class Game
    {
        private Level _map;
        private GameUIManager _uiManager;

        private GameState _gameState = GameState.TitleScreen;

        private int _gameTime = UIConstants.StartGameTime;
        private int _tickCounter = 0;
        private int _bonusTickCounter = 0;
        private int _ticksPerSecond;
        private DateTime? _gameOverTimer = null;
        private DateTime? _deathTimer = null;
        private DateTime? _levelCompleteTimer = null;

        private int _gameScore = 0;
        private int _bonusScore = 0;
        private int _currentLevel = 1;

        private int _nextLifeScoreThreshold = GameConstants.ExtraLifeThreshold;

        public GameState CurrentState { get { return _gameState; } }
        public int CurrentLevel { get { return _currentLevel; } }

        public Game(GameUIManager uiManager)
        {
            _ticksPerSecond = GameConstants.TickCalculationBase / GameConstants.TimerInterval;
            _uiManager = uiManager;
            AudioManager.Initialize();
        }

        public void Draw(Graphics g, Size panelSize)
        {
            if (_map == null) return;
                
            if (_gameState == GameState.Playing || _gameState == GameState.Paused)
                _map.Draw(g, panelSize);
        }

        public void Update(HashSet<Keys> pressedKeys)
        {
            switch (_gameState)
            {
                case GameState.TitleScreen:
                    _uiManager.HideUI();
                    _uiManager.ShowTitle();
                    break;
                case GameState.Playing:
                    switch (_map.state)
                    {
                        case State.playing:
                            HandlePlaying(pressedKeys);
                            break;
                        case State.playerDied:
                            HandlePlayerDeath();
                            break;
                        case State.gameOver:
                            HandleGameOver();
                            return;
                        case State.levelComplete:
                            HandleLevelComplete();
                            break;
                        default:
                            break;
                    }
                    break;
            }

            int totalScore = _gameScore + _map.Score;
            _uiManager.UpdateUI(
                level: _currentLevel, 
                score: totalScore, 
                bonus: _bonusScore, 
                time: _gameTime
             );
        }

        public void ChangeState(GameState newState)
        {
            _gameState = newState;
        }

        private void HandlePlaying(HashSet<Keys> pressedKeys)
        {
            _uiManager.ShowUI();
            _map.MoveAllObjects(DetermineDirection(pressedKeys));
            UpdateGameTimer();
            UpdateGameScore();
        }

        private ArrowDown DetermineDirection(HashSet<Keys> pressedKeys)
        {
            bool left = pressedKeys.Contains(Keys.Left);
            bool right = pressedKeys.Contains(Keys.Right);
            bool up = pressedKeys.Contains(Keys.Up);
            bool down = pressedKeys.Contains(Keys.Down);
            bool jump = pressedKeys.Contains(Keys.Space);

            if (jump && left && !right) return ArrowDown.jumpLeft;
            if (jump && right && !left) return ArrowDown.jumpRight;
            if (jump) return ArrowDown.jump;

            if (left && !right) return ArrowDown.left;
            if (right && !left) return ArrowDown.right;

            if (up) return ArrowDown.up;
            if (down) return ArrowDown.down;

            return ArrowDown.none;
        }

        private void HandlePlayerDeath()
        {
            if (_deathTimer == null)
            {
                AudioManager.StopMovementLoop();
                AudioManager.PlaySFX(SFX.death);
                _deathTimer = DateTime.Now;
            }
            var elapsed = (DateTime.Now - _deathTimer.Value).TotalSeconds;


            if (elapsed >= UIConstants.GetReadyDelay && elapsed < UIConstants.DeathFreeze)
            {
                _uiManager.HideUI();
                _uiManager.StartTransition(() =>
                {
                    _uiManager.ShowMessage("getReady");
                });
            }

            if (elapsed >= UIConstants.DeathFreeze)
            {
                _uiManager.StartTransition(() =>
                {
                    _uiManager.HideFullScreenMessage();
                    _map.RespawnPlayer();
                    _deathTimer = null;
                });
            }
        }

        private void HandleGameOver()
        {
            if (_gameOverTimer == null)
            {
                _gameOverTimer = DateTime.Now;
            }

            var elapsed = (DateTime.Now - _gameOverTimer.Value).TotalSeconds;

            if (elapsed >= UIConstants.GameOverDelay && elapsed < UIConstants.DeathFreeze)
            {
                _uiManager.HideUI();
                _uiManager.StartTransition(() =>
                {
                    _uiManager.ShowMessage("gameOver");
                });
            }

            if (elapsed >= UIConstants.DeathFreeze)
            {
                _uiManager.StartTransition(() =>
                {
                    _uiManager.HideFullScreenMessage();
                    _currentLevel = 1;
                    _gameScore = 0;
                    _bonusScore = 0;
                    LoadLevel(_currentLevel);
                    _gameOverTimer = null;
                });
            }
        }

        private void HandleLevelComplete()
        {
            if (_levelCompleteTimer == null)
            {
                AudioManager.StopMovementLoop();
                AudioManager.PlaySFX(SFX.win);
                _levelCompleteTimer = DateTime.Now;
            }

            var elapsed = (DateTime.Now - _levelCompleteTimer.Value).TotalSeconds;

            if (elapsed >= UIConstants.VictoryFreeze)
            {
                _uiManager.HideUI();
                _uiManager.StartTransition(() =>
                {
                    _currentLevel += 1;
                    if (_currentLevel > GameConstants.MaxLevel)
                        _currentLevel = 1;

                    int mapNum = ((_currentLevel - 1) % GameConstants.LevelCount) + 1;
                    LoadLevel(mapNum);
                    _levelCompleteTimer = null;
                });
            }
        }

        public void LoadLevel()
        {
            LoadLevel(_currentLevel);
        }

        private void LoadLevel(int levelNum)
        {

            AudioManager.StopMovementLoop();
            _gameTime = UIConstants.StartGameTime;
            _map = new Level($"Resources/Levels/level{levelNum}.txt", "Resources/Icons", this, _gameScore + _bonusScore);

            _gameScore = _gameScore + _bonusScore;
            _bonusScore = Math.Min(_currentLevel * GameConstants.BonusPerLevel, GameConstants.BonusMax);

            _map.state = State.playing;
            _levelCompleteTimer = null;
            _gameOverTimer = null;
            _deathTimer = null;
        }

        private void UpdateGameScore()
        {
            // Update score
            int totalScore = _gameScore + _map.Score;
            if (totalScore > _nextLifeScoreThreshold)
            {
                _map.player.IncreaseLife();
                _nextLifeScoreThreshold += GameConstants.ExtraLifeThreshold;
            }

            // Update bonus
            _bonusTickCounter++;
            if (_bonusTickCounter >= _ticksPerSecond * GameConstants.BonusDecreaseInterval)
            {
                _bonusScore = Math.Max(0, _bonusScore - GameConstants.BonusDecreaseAmount);
                _bonusTickCounter = 0;
            }
        }

        private void UpdateGameTimer()
        {
            if (_map.state != State.playing) return;

            _tickCounter++;

            if (_tickCounter >= _ticksPerSecond / 4)
            {
                _gameTime--;
                _tickCounter = 0;

                if (_gameTime <= 0)
                {
                    _map.state = State.gameOver;
                }
            }
        }

        public void IncreaseGameTime()
        {
            _gameTime = Math.Min(_gameTime + UIConstants.TimeIncrease, UIConstants.StartGameTime);
        }
    }
}
