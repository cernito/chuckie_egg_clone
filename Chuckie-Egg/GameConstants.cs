namespace ChuckieEgg
{
    enum ArrowDown { none, left, up, right, down, jump, jumpLeft, jumpRight }
    public enum State { notStarted, playing, levelComplete, gameOver, playerDied }

    public static class GameConstants
    {
        public const int TileSize = 32;
        public const int TimerInterval = 8;
        public const int TickCalculationBase = 1000;

        public const int FormWidth = 1080;
        public const int FormHeight = 760;

        public const int PlayerMoveFreq = 3;
        public const int LiftMoveFreq = 5;
        public const int DuckMoveFreq = 30;
        public const int EnemyMoveFreq = 50;

        public const int ExtraLifeThreshold = 10000;
        public const int BonusPerLevel = 1000;
        public const int BonusMax = 9000;
        public const int BonusDecreaseAmount = 10;
        public const int BonusDecreaseInterval = 1;

        public const int LevelCount = 8;
        public const int MaxLevel = LevelCount * 4;
        public const int DuckStartLevel = 9;
        public const int MixedStartLevel = 17;
        public const int MoreEnemiesStartLevel = 25;
    }

    public static class UIConstants
    {
        public const int StartGameTime = 900;
        public const int TimeIncrease = 16;

        public const float DeathFreeze = 5.1f;
        public const float VictoryFreeze = 1.0f;

        public const float TransitionDuration = 1.5f;
        public const float TransitionMidpoint = 0.5f;
        public const int TransitionMultiplier = 2;

        public const float GetReadyDelay = 2f;
        public const float GameOverDelay = 1.0f;
    }

    public static class SpriteConstants
    {
        public const int EnemyWalkCount = 2;
        public const int EnemyEatCount = 2;
        public const int EnemyClimbCount = 2;

        public const int PlayerIdleCount = 1;
        public const int PlayerWalkCount = 3;
        public const int PlayerClimbCount = 6;

        public const int DuckAnimCount = 2;
    }

    public static class TileSprite
    {
        public const string TileChars = ".=HLOf";
        public const char Empty = '.';
        public const char Player = 'P';
        public const char Enemy = 'E';
        public const char EnemyAdditional = 'e';
        public const char Duck = 'D';
        public const char Platform = '=';
        public const char Ladder = 'H';
        public const char Lift = 'L';
        public const char Egg = 'O';
        public const char Food = 'f';
        public const char Wall = '#';
    }

    public static class PhysicsSettings
    {
        public const float Gravity = 0.15f;
        public const float GroundEpsilon = 0.12f;
        public static float SnapEpsilon = 0.3f;
    }

    public static class PlayerSettings
    {
        public const float MoveStep = 0.1f;
        public const float FallStep = 0.18f;
        
        public const float JumpHeight = 0.6f;
        public const float JumpTiles = 2.2f;
        public const int JumpDuration = 25;
        public static float HorizontalJumpSpeed => JumpTiles / JumpDuration;
        
        public const int LifeCount = 5;
    }
}
