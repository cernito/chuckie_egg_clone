using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace ChuckieEgg
{
    class Level
    {
        private Game _game;
        private SpriteManager _spriteManager;

        private char[,] _grid;
        private int _width;
        private int _height;

        public int RemainingEggs;
        public int Score;

        private List<MovableObject> _movableObjectsApartFromPlayer = new();
        private List<CollectableObject> _collectables = new();
        public Player player = null!;

        public ArrowDown arrowDown;
        public State state = State.notStarted;

        public Level(string mapPath, string iconsFolder, Game game, int score)
        {
            _game = game;
            _spriteManager = new SpriteManager(iconsFolder);
            LoadMap(mapPath);
            this.state = State.playing;
            Score = score;
        }

        [MemberNotNull(nameof(_grid), nameof(player))]
        public void LoadMap(string path)
        {
            _movableObjectsApartFromPlayer = new List<MovableObject>();
            _collectables = new List<CollectableObject>();

            // Read map from file
            System.IO.StreamReader sr = new System.IO.StreamReader(path);
            string? widthLine = sr.ReadLine();
            string? heightLine = sr.ReadLine();

            if (widthLine == null || heightLine == null)
            {
                sr.Close();
                throw new Exception("Invalid map file: missing width or height.");
            }

            _width = int.Parse(widthLine);
            _height = int.Parse(heightLine);
            _grid = new char[_width, _height];
            RemainingEggs = 0;

            for (int y = 0; y < _height; y++)
            {
                string row = sr.ReadLine();
                for (int x = 0; x < _width; x++)
                {
                    char ch = row[x];
                    _grid[x, y] = ch;

                    LoadObjectFromChar(ch, x, y);
                }
            }

            if (player == null)
            {
                sr.Close();
                throw new Exception("Level file must contain a player ('P').");
            }

            // Add duck
            if (_game.CurrentLevel >= GameConstants.DuckStartLevel)
            {
                var dSprites = _spriteManager.Duck;
                _movableObjectsApartFromPlayer.Add(new Duck(this, 2, 2, GameConstants.DuckMoveFreq, dSprites.Flight, player));
            }

            sr.Close();
        }

        private void LoadObjectFromChar(char ch, int x, int y)
        {
            switch (ch)
            {
                case TileSprite.Player:
                    var pSprites = _spriteManager.Player;
                    this.player = new Player(
                        this, x, y, GameConstants.PlayerMoveFreq, pSprites.Idle, pSprites.Walk, pSprites.Climb
                    );
                    _grid[x, y] = TileSprite.Empty;
                    break;
                case TileSprite.Enemy:
                    if (_game.CurrentLevel < GameConstants.DuckStartLevel || _game.CurrentLevel >= GameConstants.MixedStartLevel)
                    {
                        var eSprites = _spriteManager.Enemy;
                        _movableObjectsApartFromPlayer.Add(
                            new Enemy(this, x, y, GameConstants.EnemyMoveFreq, eSprites.Walk, eSprites.Eat, eSprites.Climb)
                         );
                    }
                    _grid[x, y] = TileSprite.Empty;
                    break;
                case TileSprite.EnemyAdditional:
                    if (_game.CurrentLevel >= GameConstants.MoreEnemiesStartLevel)
                    {
                        var eSprites = _spriteManager.Enemy;
                        _movableObjectsApartFromPlayer.Add(
                            new Enemy(this, x, y, GameConstants.EnemyMoveFreq, eSprites.Walk, eSprites.Eat, eSprites.Climb)
                        );
                    }
                    _grid[x, y] = TileSprite.Empty;
                    break;
                case TileSprite.Egg:
                    _collectables.Add(new Egg(this, x, y));
                    _grid[x, y] = TileSprite.Empty;
                    RemainingEggs++;
                    break;
                case TileSprite.Food:
                    _collectables.Add(new Food(this, x, y));
                    _grid[x, y] = TileSprite.Empty;
                    break;
                case TileSprite.Ladder:
                    Ladder ladder = new Ladder(this, x, y);
                    break;
                case TileSprite.Lift:
                    _movableObjectsApartFromPlayer.Add(
                        new Lift(this, x, y, GameConstants.LiftMoveFreq, _height, 
                        _spriteManager.TileSprites[TileSprite.TileChars.IndexOf(TileSprite.Lift)])
                    );
                    _grid[x, y] = TileSprite.Empty;
                    break;
                case TileSprite.Platform:
                    Platform platform = new Platform(this, x, y);
                    break;
                default:
                    break;
            }
        }

        public void Draw(Graphics g, Size canvasSize)
        {
            float scaleX = canvasSize.Width / (float)_width;
            float scaleY = canvasSize.Height / (float)_height;
            float scale = Math.Min(scaleX, scaleY);

            float gameWidth = _width * scale;
            float gameHeight = _height * scale;

            // Calculate offset to center the game
            float offsetX = (canvasSize.Width - gameWidth) / 2;
            float offsetY = (canvasSize.Height - gameHeight) / 2;

            g.TranslateTransform(offsetX, offsetY);

            for (int x = 1; x < _width - 1; x++)
            {
                for (int y = 1; y < _height - 1; y++)
                {
                    char c = _grid[x, y];
                    int idx = TileSprite.TileChars.IndexOf(c);
                    float nx = x * scale;
                    float ny = y * scale;
                    g.DrawImage(_spriteManager.TileSprites[idx], nx, ny, scale, scale);
                }
            }

            // Draw collectibles
            foreach (CollectableObject co in _collectables)
            {
                float cx = co.x * scale;
                float cy = co.y * scale;
                char sprite = co is Egg ? TileSprite.Egg : TileSprite.Food;
                int idx = TileSprite.TileChars.IndexOf(sprite);
                g.DrawImage(_spriteManager.TileSprites[idx], cx, cy, scale, scale);
            }

            // Draw enemies and lifts
            foreach (MovableObject mo in _movableObjectsApartFromPlayer)
            {
                mo.Draw(g, scale);
            }

            // Draw player
            player.Draw(g, scale);            
        }

        public void MoveAllObjects(ArrowDown keyDown)
        {
            arrowDown = keyDown;
            foreach (MovableObject o in _movableObjectsApartFromPlayer)
            {
                o.Update();
            }
            player.Update();
        }

        public void RespawnPlayer()
        {
            player.ResetToStartPosition();
            foreach (var obj in _movableObjectsApartFromPlayer)
            {
                obj.ResetToStartPosition();
            }
            state = State.playing;
        }

        public bool HasEnemy(int x, int y)
        {
            return _movableObjectsApartFromPlayer
                .Any(e => (e is Enemy || e is Duck) 
                     && e.TileX == player.TileX && e.TileY == player.TileY);
        }

        public bool CanMoveTo(float x, float y)
        {
            if (x <= 0 || x >= _width) return false;
            if (y < 0 || y >= _height) return false;

            int tx = (int)Math.Floor(x + 0.5f);
            int ty = (int)Math.Floor(y + 0.99f);
            char newTile = _grid[tx, ty];
            
            return newTile != TileSprite.Platform
                && newTile != TileSprite.Wall;
        }

        public bool IsOnGround(int tileX, int tileY, float y)
        {
            if (tileY >= _height - 1) return false;

            char below = _grid[tileX, tileY + 1];
            bool onPlatform = below == TileSprite.Platform && (tileY - y) < PhysicsSettings.SnapEpsilon;
            if (onPlatform) 
                return true;

            bool ladderAsGround = IsLadder(tileX, tileY) 
                && (_grid[tileX - 1, tileY + 1] == TileSprite.Platform 
                || _grid[tileX + 1, tileY + 1] == TileSprite.Platform);
            if (ladderAsGround)
                 return true;

            return false;
        }

        public bool IsFalling(int x, int y, ArrowDown direction)
        {
            if (y >= _height - 1) return true;
            char thisTile = _grid[x, y];
            char below = _grid[x, y + 1];
            return thisTile == TileSprite.Empty
                && below == TileSprite.Empty
                && !IsOnLift(player.x, player.y)
                && (direction == ArrowDown.left && !IsOnGround(x, y, y)
                || direction == ArrowDown.right && !IsOnGround((int)Math.Floor(x + 0.5f), y, y)
                || direction == ArrowDown.none && !IsOnGround(x, y, y));
        }

        public bool IsOnLift(float x, float y)
        {
            return _movableObjectsApartFromPlayer
                .OfType<Lift>()
                .Any(l => (l.TileX <= x + 0.5f && x + 0.5f <= l.TileX + 1) && Math.Abs(l.y - y - 1) <= 0.5f);
        }

        public Lift? GetLiftAt(int tileX, int tileY)
        {
            // Find a lift that's close to the specified position (allow some horizontal movement)
            return _movableObjectsApartFromPlayer.OfType<Lift>().FirstOrDefault(l =>
                 Math.Abs(l.TileX - tileX) <= 1 && Math.Abs(l.y - tileY) < 1.2f
            );
        }

        public bool IsLadder(int x, int y)
        {
            if (y < 0) return false;
            return _grid[x, y] == TileSprite.Ladder;
        }

        public bool IsPlatform(int x, int y)
        {
            return _grid[x, y] == TileSprite.Platform;
        }

        public bool IsLadderAndCentered(int tileX, int tileY, float x)
        {
            float tileCenterX = (int)Math.Floor(x + 0.5f);
            float distanceFromCenter = Math.Abs(x - tileCenterX);
            bool isCentered = distanceFromCenter < 0.2f;
            return IsLadder(tileX, tileY) && isCentered;
        }

        public bool FellOutOfBounds(int x, int y)
        {
            return y >= _height - 1 || _grid[x, y] == TileSprite.Wall;
        }

        public void IncreaseGameTime()
        {
            _game.IncreaseGameTime();
        }

        public bool IsFood(int tileX, int tileY)
        {
            return _collectables.OfType<Food>().Any( f => f.TileX == tileX && f.TileY == tileY );
        }

        public bool IsEgg(int tileX, int tileY)
        {
            return _collectables.OfType<Egg>().Any (f => f.TileX == tileX && f.TileY == tileY);
        }

        public void TryCollectAt(int tileX, int tileY, bool byEnemy = false)
        {
            var c = _collectables.FirstOrDefault(o => !o.Collected && o.TileX == tileX && o.TileY == tileY);

            if (c != null)
            {
                if (!byEnemy)
                {
                    if (c is Egg) AudioManager.PlaySFX(SFX.pickupEgg);
                    else if (c is Food) AudioManager.PlaySFX(SFX.pickupFood);
                    c.TryCollect();
                }
                _collectables.Remove(c);
            }

            if (RemainingEggs == 0) state = State.levelComplete;
        }
    }
}
