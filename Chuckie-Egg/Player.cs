using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing.Text;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.CodeDom;
using static System.Windows.Forms.AxHost;
using System.Numerics;
using static ChuckieEgg.Player;

namespace ChuckieEgg
{
    class Player : MovableObject
    {
        private float _newX;
        private float _newY;

        private readonly Bitmap[] _idleAnim;
        private readonly Bitmap[] _walkAnim;
        private readonly Bitmap[] _climbAnim;

        private int _currentFrame = 0;
        private Bitmap[] _frames;

        private enum MovementState { IdleWalk, IdleClimb, Walking, Climbing, Jumping, Falling };
        private MovementState _state;

        private bool _isFalling = false;
        private bool _isJumping = false;
        private bool _isOnGround = true;
        private bool _isOnLadder = false;
        private bool _ladderAllowedLeft = false;
        private bool _ladderAllowedRight = false;
        private bool _isOnLift = false;
        private bool _groundToLeft = false;
        private bool _groundToRight = false;

        private float _jumpVx;
        private float _jumpVy;
        private readonly float _jumpGravity;

        private int _livesRemaining;

        private enum JumpType { None, Jump, JumpLeft, JumpRight}
        private JumpType _jumpType;

        public Player(Level map, int wherex, int wherey, int moveFreq,
                      Bitmap[] idleAnim, Bitmap[] walkAnim, Bitmap[] climbAnim)
            : base(map, wherex, wherey, moveFreq)
        {
            _idleAnim = idleAnim;
            _walkAnim = walkAnim;
            _climbAnim = climbAnim;
            _jumpGravity = -8f * PlayerSettings.JumpHeight / (PlayerSettings.JumpDuration * PlayerSettings.JumpDuration);
            _frames = idleAnim;
            _state = MovementState.IdleWalk;
            _jumpType = JumpType.None;
            _livesRemaining = PlayerSettings.LifeCount;
        }

        public override void Draw(Graphics g, float scale)
        {
            var bmp = _frames[_currentFrame];

            float dx = x * scale;
            float dy = y * scale;
            float scalex = scale;
            float scaley = scale;

            bool walkingLeft = _state == MovementState.Walking && map.arrowDown == ArrowDown.left;
            bool idleWalkLeft = _state == MovementState.IdleWalk && DirectionX < 0;
            bool jumpingLeft = _state == MovementState.Jumping && 
                (_jumpType == JumpType.JumpLeft || 
                (_jumpType == JumpType.Jump && DirectionX < 0));
            bool fallingLeft = _state == MovementState.Falling && DirectionX < 0;
            if (walkingLeft || idleWalkLeft || jumpingLeft || fallingLeft)
            {
                dx = dx + scale;
                scalex = -scale;
            }

            g.DrawImage(bmp, dx, dy, scalex, scaley);
        }

        private void AdvanceFrames()
        {
            _frames = _state == MovementState.IdleWalk ? _idleAnim
                : _state == MovementState.Walking ? _walkAnim
                : _state == MovementState.Jumping ? _walkAnim
                : _state == MovementState.Falling ? _walkAnim
                : _climbAnim;

            if (_state == MovementState.IdleClimb) { }
            else
            {
                _currentFrame = (_currentFrame + 1) % _frames.Length;
            }
            if (_isJumping) _currentFrame = Math.Min(2, _frames.Length - 1 );
            if (_isFalling) _currentFrame = Math.Min(1, _frames.Length - 1);
            bool onEndOfLadder = _isOnLadder && _isOnGround && _state == MovementState.Climbing
                && (TileY - y) <= PlayerSettings.MoveStep;
            if (onEndOfLadder) _currentFrame = 0;
        }

        public override void MakeMove()
        {
            SampleSensors();
            UpdateCollisions();

            _newX = x; _newY = y;
            
            if (_isFalling)
            {
                UpdateFall();
                AdvanceFrames();
                return;
            }
            
            if (_isJumping)
            {
                UpdateJump();
                AdvanceFrames();
                return;
            }

            switch (map.arrowDown)
            {
                case ArrowDown.none:
                    AudioManager.StopMovementLoop();
                    if (_state == MovementState.Walking)
                        _state = MovementState.IdleWalk;
                    else if (_state == MovementState.Climbing)
                        _state = MovementState.IdleClimb;
                    break;
                case ArrowDown.left:
                    MoveLeft();
                    break;
                case ArrowDown.right:
                    MoveRight();
                    break;
                case ArrowDown.up:
                    MoveUp();
                    break;
                case ArrowDown.down:
                    MoveDown();
                    break;
                case ArrowDown.jumpLeft:
                case ArrowDown.jumpRight:
                case ArrowDown.jump:
                    StartJump();
                    break;
            }

            int newTileX = (int)Math.Floor(_newX + 0.5f);
            int newTileY = (int)Math.Floor(_newY + 0.99f);

            if (map.IsFalling(newTileX, newTileY, map.arrowDown))
            {
                _isFalling = true;
                _newX = newTileX - DirectionX * 0.25f;
                _state = MovementState.Falling;
                AudioManager.PlayMovementLoop(SFX.fall);
            }

            if (map.CanMoveTo(_newX, _newY))
            {
                x = _newX;
                y = _newY;
            }

            AdvanceFrames();
        }

        private void MoveLeft() => MoveHorizontal(-1);
        private void MoveRight() => MoveHorizontal(1);
        private void MoveUp() => MoveVertical(-1);
        private void MoveDown() => MoveVertical(1);

        private void MoveHorizontal(int dirX)
        {
            if (_isOnGround && PhysicsSettings.GroundEpsilon <= Math.Abs(TileY - y)) return;
            if (_isOnLadder && !_isOnGround) return;

            _state = MovementState.Walking;
            _newX = x + dirX * PlayerSettings.MoveStep;
            DirectionX = dirX;

            AudioManager.PlayMovementLoop(SFX.walk);
        }

        private void MoveVertical(int dirY)
        {
            bool canClimbUp = dirY < 0 && map.IsLadderAndCentered(TileX, TileY - 1, x);
            bool canClimbDown = dirY > 0 && map.IsLadderAndCentered(TileX, TileY, x);


            if (_isOnLadder && (dirY < 0 ? canClimbUp : canClimbDown))
            {
                if (_state != MovementState.Climbing)
                {
                    SnapToLadder();
                    _state = MovementState.Climbing;
                }

                _newY = y + dirY * (PlayerSettings.MoveStep / 1.05f);
                DirectionY = dirY;
                AudioManager.PlayMovementLoop(SFX.climb);
            }
        }

        void UpdateFall()
        {
            y += PlayerSettings.FallStep;
            if (map.IsOnGround(TileX, TileY, y))
            {
                AudioManager.StopMovementLoop();
                _isFalling = false;
                SnapToGround();
                _state = MovementState.IdleWalk;
            }
        }

        void StartJump()
        {
            _jumpVx = (map.arrowDown == ArrowDown.jumpLeft ? -1
                      : map.arrowDown == ArrowDown.jumpRight ? +1
                      : 0) * PlayerSettings.HorizontalJumpSpeed;

            _jumpVy = 4f * PlayerSettings.JumpHeight / PlayerSettings.JumpDuration;

            _jumpType = _jumpVx < 0 ? JumpType.JumpLeft : _jumpVx == 0 ? JumpType.Jump : JumpType.JumpRight;

            if ((_state == MovementState.Climbing || _state == MovementState.IdleClimb) && _jumpType == JumpType.Jump) 
                _state = MovementState.IdleClimb;
            else _state = MovementState.Jumping;
            
            _isJumping = true;
            
            AudioManager.PlayMovementLoop(SFX.jump);
        }

        private void UpdateJump()
        {
            // Snap to ladder
            if (map.arrowDown == ArrowDown.up && _isOnLadder)
            {
                _isJumping = false;
                SnapToLadder();
                _jumpType = JumpType.Jump;
                _state = MovementState.Climbing;
                return;
            }

            x += _jumpVx;
            y -= _jumpVy;
            _jumpVy += _jumpGravity;
            if (_jumpVy <= -0.18f) _jumpVy = -0.18f;

            // Check horizontal collision
            int tx = TileX, ty = TileY;
            if (!map.CanMoveTo(x, y))
            {
                _jumpVx = -_jumpVx;
                x += _jumpVx;
            }   

            // Ladnding detection
            bool landingOnGround = _jumpVy < 0 && map.IsOnGround(TileX, TileY, y)
                && (TileY - y) < 0.18f;
            // For lifts, check in a wider area and also check if we're close to any lift
            var nearbyLift = map.GetLiftAt(TileX, TileY);
            bool landingOnLift = nearbyLift != null
                && _jumpVy <= 0
                && nearbyLift.y >= y
                && Math.Abs(nearbyLift.y - 1 - y) <= PhysicsSettings.GroundEpsilon // And close enough to land
                && nearbyLift.TileX <= x + 0.5f && x + 0.5f <= nearbyLift.TileX + 1;

            _newX = x;
            _newY = y;

            if (landingOnGround || landingOnLift)
            {
                AudioManager.StopMovementLoop();
                _isJumping = false;
                _isFalling = false;
                if (_state != MovementState.IdleClimb) _state = MovementState.IdleWalk;

                if (landingOnLift)
                {
                   y = nearbyLift.y - 1f;
                }
                else
                {
                    y = TileY;
                }

                x = _newX;
                return;
            }
        }

        public void UpdateCollisions()
        {
            if (!_isJumping && _isOnLift)
            {
                _isFalling = false;
                var lift = map.GetLiftAt(TileX, TileY);
                if (lift != null && lift.y >= y)
                {
                    y = lift.y - 1;
                    _state = MovementState.IdleWalk;
                }
            }


            if (_isOnGround && !_isOnLadder && !_isJumping)
            {
                SnapToGround();
            }

            if (map.HasEnemy(TileX, TileY)) { LoseLife(); }
            if (map.FellOutOfBounds(TileX, TileY)) { LoseLife(); } 
            map.TryCollectAt(TileX, (int)Math.Floor(y + 0.5f));
        }

        private void SampleSensors()
        {
            _isOnGround = map.IsOnGround(TileX, TileY, y);
            _isOnLadder = map.IsLadderAndCentered(TileX, TileY, x) || map.IsLadderAndCentered(TileX, (int)Math.Floor(y), x);
            _isOnLift = map.IsOnLift(x, y);
            _groundToLeft = map.CanMoveTo(TileX - 1, TileY) && map.IsOnGround(TileX - 1, TileY, y);
            _groundToRight = map.CanMoveTo(TileX + 1, TileY) && map.IsOnGround(TileX + 1, TileY, y);
            _ladderAllowedLeft = _isOnLadder && (_groundToLeft || _groundToRight);
            _ladderAllowedRight = _isOnLadder && (_groundToRight || _groundToLeft);
        }

        public void SnapToGround()
        {
            y = TileY;
            _newY = y;
        }

        public void SnapToLadder()
        {
            y = (float)Math.Round(_newY / PlayerSettings.MoveStep) * PlayerSettings.MoveStep;
            x = TileX;
            _newX = x;
            _newY = y;
        }

        private void LoseLife()
        {
            _livesRemaining--;
            if (_livesRemaining <= 0)
            {
                map.state = State.gameOver;
                _livesRemaining = PlayerSettings.LifeCount;
                return;
            };
            map.state = State.playerDied;
            _state = MovementState.IdleWalk;
        }

        public void IncreaseLife()
        {
            _livesRemaining++;
        }

        public override void ResetToStartPosition()
        {
            base.ResetToStartPosition();
            _isFalling = false;
            _isJumping = false;
        }
    }
}
