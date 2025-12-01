using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ChuckieEgg
{
    internal class SpriteManager
    {
        public Bitmap[] TileSprites { get; private set; } = null!;
        
        public class PlayerSprites
        {
            public Bitmap[] Idle = Array.Empty<Bitmap>();
            public Bitmap[] Walk = Array.Empty<Bitmap>();
            public Bitmap[] Climb = Array.Empty<Bitmap>();
        }

        public class EnemySprites
        {
            public Bitmap[] Walk = Array.Empty<Bitmap>();
            public Bitmap[] Eat = Array.Empty<Bitmap>();
            public Bitmap[] Climb = Array.Empty<Bitmap>();
        }

        public struct DuckSprites
        {
            public Bitmap[] Flight;
        }

        private PlayerSprites _playerSprites = new();
        private EnemySprites _enemySprites = new();
        private DuckSprites _duckSprites = new();

        public PlayerSprites Player => _playerSprites;
        public EnemySprites Enemy => _enemySprites;
        public DuckSprites Duck => _duckSprites;

        private int _spriteSize;

        public SpriteManager(string folder)
        {
            LoadAllSprites(folder);
        }

        public void LoadAllSprites(string folder)
        {
            TileSprites = LoadStrip(Path.Combine(folder, "tiles.png"));

            var playerIcons = LoadStrip(Path.Combine(folder, "player.png"));
            _playerSprites.Walk = playerIcons.Take(SpriteConstants.PlayerWalkCount).ToArray();
            _playerSprites.Idle = playerIcons.Take(SpriteConstants.PlayerIdleCount).ToArray();
            _playerSprites.Climb = playerIcons.Skip(SpriteConstants.PlayerWalkCount).Take(SpriteConstants.PlayerClimbCount).ToArray();

            var enemyIcons = LoadStrip(Path.Combine(folder, "enemy.png"));
            _enemySprites.Walk = enemyIcons.Take(SpriteConstants.EnemyWalkCount).ToArray();
            _enemySprites.Eat = enemyIcons.Skip(SpriteConstants.EnemyWalkCount).Take(SpriteConstants.EnemyEatCount).ToArray();
            _enemySprites.Climb = enemyIcons.Skip(SpriteConstants.EnemyWalkCount+SpriteConstants.EnemyEatCount).Take(SpriteConstants.EnemyClimbCount).ToArray();

            var duckIcons = LoadStrip(Path.Combine(folder, "duck.png"));
            _duckSprites.Flight = duckIcons.Take(SpriteConstants.DuckAnimCount).ToArray();
        }

        private Bitmap[] LoadStrip(string path)
        {
            Bitmap bmp = new Bitmap(path);
            _spriteSize = bmp.Height;
            int count = bmp.Width / _spriteSize;
            var frames = new Bitmap[count];
            for (int i = 0; i < count; i++)
            {
                Rectangle rect = new Rectangle(i * _spriteSize, 0, _spriteSize, _spriteSize);
                frames[i] = bmp.Clone(rect, System.Drawing.Imaging.PixelFormat.DontCare);
            }
            return frames;
        }
    }
}
