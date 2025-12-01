using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChuckieEgg
{
    public class GamePanel : Panel
    {
        public GamePanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            SetStyle(
                ControlStyles.AllPaintingInWmPaint
              | ControlStyles.OptimizedDoubleBuffer
              | ControlStyles.UserPaint,
                true
            );
        }
    }

    abstract class GameObject
    {
        public Level map;
        public float x;
        public float y;

        public int TileX => (int)Math.Floor(x + 0.5f); // Take center of tile
        public int TileY => (int)Math.Floor(y + 0.99f);

        protected GameObject(Level map, int x, int y)
        {
            this.map = map;
            this.x = x;
            this.y = y;
        }
    }

    abstract class MovableObject : GameObject
    {
        private (int x, int y) _startPosition;

        private int _moveCounter = 0;
        private readonly int _moveFrequency;

        public int DirectionX { get; protected set; } = +1;
        public int DirectionY { get; protected set; } = +1;

        protected MovableObject(Level map, int tileX, int tileY, int moveFreq) : base(map, tileX, tileY)
        {
            _startPosition.x = TileX;
            _startPosition.y = TileY;
            _moveFrequency = moveFreq;
        }

        public void Update()
        {
            _moveCounter++;
            if (_moveCounter >= _moveFrequency)
            {
                _moveCounter = 0;
                MakeMove();
            }
        }

        public abstract void MakeMove();
        public abstract void Draw(Graphics g, float scale);

        public virtual void ResetToStartPosition()
        {
            x = _startPosition.x;
            y = _startPosition.y;
            DirectionX = +1;
        }
    }

    abstract class CollectableObject : GameObject
    {
        public bool Collected { get; private set; }

        protected CollectableObject(Level map, int wherex, int wherey) : base(map, wherex, wherey) { }

        public void TryCollect()
        {
            if (Collected) return;
            Collected = true;
            Collect();
        }

        protected abstract void Collect();
    }

    class Lift : MovableObject
    {
        public const float MOVE_STEP = 0.1f;
        private int mapHeight;

        private Bitmap _liftSprite;

        public Lift(Level map, int wherex, int wherey, int moveFreq, int mapHeight, Bitmap liftFrame) : base(map, wherex, wherey, moveFreq)
        {
            this.mapHeight = mapHeight;
            this.DirectionY = -1;
            _liftSprite = liftFrame;
        }

        public override void Draw(Graphics g, float scale)
        {
            float dx = x * scale;
            float dy = y * scale;

            g.DrawImage(_liftSprite, dx, dy, scale, scale);
        }

        public override void MakeMove()
        {
            y = (y + DirectionY * MOVE_STEP + mapHeight) % mapHeight; // Pohyb nahoru - pri vyjeti z obrazovky nahore, objevi se dole
        }

        public override void ResetToStartPosition()
        {
            base.ResetToStartPosition();
        }
    }

    class Egg : CollectableObject
    {
        public Egg(Level map, int wherex, int wherey) : base(map, wherex, wherey) { }

        protected override void Collect()
        {
            map.RemainingEggs--;
            map.Score += 100;
            if (map.RemainingEggs <= 0)
                map.state = State.levelComplete;
        }
    }

    class Food : CollectableObject
    {
        public Food(Level map, int wherex, int wherey) : base(map, wherex, wherey) { }

        protected override void Collect()
        {
            map.Score += 50;
            map.IncreaseGameTime();
        }
    }

    class Ladder : GameObject
    {
        public Ladder(Level map, int wherex, int wherey) : base(map, wherex, wherey) { }
    }

    class Platform : GameObject
    {
        public Platform(Level map, int wherex, int wherey) : base(map, wherex, wherey) { }
    }
}
