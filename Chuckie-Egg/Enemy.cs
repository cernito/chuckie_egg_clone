using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChuckieEgg
{
    class Enemy : MovableObject
    {
        public const float MoveStep = 1 / 2f;
        private float _directionStep = MoveStep;
        private const float ClimbStep = 0.25f;
        private float _directionClimb = ClimbStep;

        private enum EnemyState { Walking, Eating, Climbing }
        private EnemyState _state = EnemyState.Walking;
        private Random _random = new Random();

        private readonly Bitmap[] _walkAnim;
        private readonly Bitmap[] _eatAnim;
        private readonly Bitmap[] _climbAnim;
        private Bitmap[] _frames;
        private int _currentFrame = 0;

        private int _eatAnimCount = 0;

        public Enemy(Level map, int wherex, int wherey, int moveFreq, Bitmap[] walkFrames, Bitmap[] eatFrames, Bitmap[] climbFrames) : base(map, wherex, wherey, moveFreq)
        {
            _walkAnim = walkFrames;
            _eatAnim = eatFrames;
            _climbAnim = climbFrames;
            _frames = walkFrames;
        }

        public override void Draw(Graphics g, float scale)
        {
            _frames = (_state == EnemyState.Walking ? _walkAnim : _state == EnemyState.Eating ? _eatAnim : _climbAnim);
            var bmp = _frames[_currentFrame];

            float enemyScale = scale * 1.12f;

            float dx = (x + 0.5f) * scale - enemyScale / 2;
            float dy = (y + 0.43f) * scale - enemyScale / 2;

            float scalex = enemyScale;
            float scaley = enemyScale;

            if ((_state == EnemyState.Walking || _state == EnemyState.Eating) && _directionStep < 0)
            {
                dx += enemyScale;
                scalex = -enemyScale;
            }
            g.DrawImage(bmp, dx, dy, scalex, scaley);
        }

        public override void MakeMove()
        {
            switch (_state)
            {
                case EnemyState.Walking:
                    HandleWalking();
                    break;
                case EnemyState.Eating:
                    HandleEating();
                    break;
                case EnemyState.Climbing:
                    HandleClimbing();
                    break;
            }

            _frames = (_state == EnemyState.Walking) ? _walkAnim : _state == EnemyState.Eating ? _eatAnim : _climbAnim;
            _currentFrame = (_currentFrame + 1) % _frames.Length;
        }

        private void HandleWalking()
        {
            float new_x = x + _directionStep;
            int newTileX = (int)Math.Floor(new_x + 0.5f);

            if (map.IsFood((int)Math.Floor(x + (_directionStep < 0 ? -0.5f + 0.99f : 0.5f)), TileY))
            {   
                _state = EnemyState.Eating;
                return;
            }

            if (map.IsLadderAndCentered(newTileX, TileY, new_x))
            {
                bool canClimbUp = map.IsLadderAndCentered(newTileX, TileY - 1, new_x);
                bool canClimbDown = map.IsLadderAndCentered(newTileX, TileY + 1, new_x);

                if (canClimbUp || canClimbDown)
                {
                    List<int> choices = new List<int> { 0 };
                    if (canClimbUp) choices.Add(1);
                    if (canClimbDown) choices.Add(2);

                    int pick = choices[_random.Next(choices.Count)];

                    if (pick == 1)
                    {
                        _state = EnemyState.Climbing;
                        _directionClimb = -ClimbStep;
                    }
                    else if (pick == 2)
                    {
                        _state = EnemyState.Climbing;
                        _directionClimb = +ClimbStep;
                    }
                }
            }

            if (map.IsOnGround(newTileX, TileY, y))
            {
                if (_state == EnemyState.Climbing)
                {
                    x = newTileX;
                }
                else
                {
                    x = new_x;
                }
            }
            else
            {
                _directionStep = -_directionStep;
            }
        }

        private void HandleEating()
        {
            _eatAnimCount++;

            if (_eatAnimCount >= 3)
            {
                _state = EnemyState.Walking;
                _eatAnimCount = 0;
                return;
            }

            if (_eatAnimCount == 2)
            {
                map.TryCollectAt((int)Math.Floor(x + (_directionStep < 0 ? -0.5f + 0.99f : 0.5f)), TileY, byEnemy : true);
            }

        }

        private void HandleClimbing()
        {
            float new_y = y + _directionClimb;
            int newTileY = (int)Math.Floor(new_y + 0.99f);

            if (map.IsOnGround(TileX, newTileY, new_y) && new_y + PhysicsSettings.GroundEpsilon >= newTileY)
            {
                bool canClimbInDirection = map.IsLadderAndCentered(TileX, newTileY + _directionClimb < 0 ? -1 : +1, x);
                bool groundLeft = map.IsOnGround(TileX - 1, newTileY, new_y);
                bool groundRight = map.IsOnGround(TileX + 1, newTileY, new_y);

                if (groundLeft || groundRight)
                {
                    List<int> choices = new List<int> { };
                    if (canClimbInDirection) choices.Add(0);
                    if (groundLeft) choices.Add(1);
                    if (groundRight) choices.Add(2);

                    int pick = choices[_random.Next(choices.Count)];

                    if (pick == 1)
                    {
                        _state = EnemyState.Walking;
                        _directionStep = -MoveStep;
                    }
                    else if (pick == 2)
                    {
                        _state = EnemyState.Walking;
                        _directionStep = +MoveStep;
                    }
                }
            }

            if (map.IsLadderAndCentered(TileX, newTileY, x))
            {
                y = new_y;
            }
            else
            {
                _directionClimb = -_directionClimb;
            }
        }

        public override void ResetToStartPosition()
        {
            base.ResetToStartPosition();
            _state = EnemyState.Walking;
        }
    }
}
