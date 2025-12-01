using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ChuckieEgg
{
    class Duck : MovableObject
    {
        float MaxSpeed = 0.72f;
        float Steering = 0.13f;
        float vx = 0f;
        float vy = 0f;

        private readonly Bitmap[] _moveAnim;
        private int _currentFrame = 0;

        private Player _player;

        public Duck(Level map, int wherex, int wherey, int moveFreq, Bitmap[] moveFrames, Player player) : base(map, wherex, wherey, moveFreq)
        {
            _moveAnim = moveFrames;
            _player = player;
        }

        public override void Draw(Graphics g, float scale)
        {
            var bmp = _moveAnim[_currentFrame];

            float duckScale = scale * 1.5f;

            float dx = (x + 0.5f) * scale - duckScale / 2;
            float dy = (y + 0.5f) * scale - duckScale / 2;
            float scalex = duckScale;
            float scaley = duckScale;

            if (DirectionX < 0)
            {
                dx += duckScale;
                scalex = -duckScale;
            }
            g.DrawImage(bmp, dx, dy, scalex, scaley);
        }

        public override void MakeMove()
        {
            float dx = _player.x - x;
            float dy = _player.y - y;

            float dist = (float)Math.Sqrt(dx * dx + dy * dy);
            if (dist > 0.01f)
            {
                float desiredVx = MaxSpeed * dx / dist;
                float desiredVy = MaxSpeed * dy / dist;

                vx += (desiredVx - vx) * Steering;
                vy += (desiredVy - vy) * Steering;
            }

            x += vx;
            y += vy;

            DirectionX = dx >= 0 ? +1 : -1;

            _currentFrame = (_currentFrame + 1) % _moveAnim.Length;
        }
    }
}
