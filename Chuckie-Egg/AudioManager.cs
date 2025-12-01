using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Media;

namespace ChuckieEgg
{
    enum SFX { pickupEgg, pickupFood, death, win, walk, climb, fall, jump }

    internal class AudioManager
    {
        private static WaveOutEvent _loopDevice;
        private static SFX? _currentLoopKey = null;

        private static readonly Dictionary<SFX, LoopStream> _loopStreamCache = new();
        private static readonly Dictionary<SFX, string> _loopingPaths = new()
        {
            { SFX.walk,  "Resources/Audio/walk.wav"  },
            { SFX.climb, "Resources/Audio/climb.wav" },
            { SFX.fall,  "Resources/Audio/fall.wav"  },
            { SFX.jump,  "Resources/Audio/jump.wav"  }
        };

        private static readonly Dictionary<SFX, SoundPlayer> _sfxPlayers = new();
        private static readonly Dictionary<SFX, string> _sfxPaths = new()
        {
            { SFX.pickupEgg,  "Resources/Audio/pickupEgg.wav"  },
            { SFX.pickupFood, "Resources/Audio/pickupFood.wav" },
            { SFX.death,      "Resources/Audio/death.wav"       },
            { SFX.win,        "Resources/Audio/win.wav"         }
        };

        public static void Initialize()
        {
            foreach (var kvp in _loopingPaths)
            {
                var reader = new AudioFileReader(kvp.Value);
                _loopStreamCache[kvp.Key] = new LoopStream(reader);
            }

            foreach (var kvp in _sfxPaths)
            {
                var player = new SoundPlayer(kvp.Value);
                player.Load();
                _sfxPlayers[kvp.Key] = player;
            }
        }

        public static void PlayMovementLoop(SFX sfx)
        {
            if (_currentLoopKey == sfx) return;

            _loopDevice?.Stop();

            if (_loopStreamCache.TryGetValue(sfx, out var loop))
            {
                if (_loopDevice == null)
                    _loopDevice = new WaveOutEvent();

                loop.Position = 0;
                _loopDevice.Init(loop);
                _loopDevice.Play();
                _currentLoopKey = sfx;
            }
        }

        public static void StopMovementLoop()
        {
            _loopDevice?.Stop();
            _loopDevice = null;
            _currentLoopKey = null;
        }

        public static void PlaySFX(SFX sfx)
        {
            if (_sfxPlayers.TryGetValue(sfx, out var player))
            {
                player.Play();
            }
        }
    }
}



// Following block of code taken from
// https://www.markheath.net/post/looped-playback-in-net-with-naudio

/// <summary>
/// Stream for looping playback
/// </summary>
public class LoopStream : WaveStream
{
    WaveStream sourceStream;

    /// <summary>
    /// Creates a new Loop stream
    /// </summary>
    /// <param name="sourceStream">The stream to read from. Note: the Read method of this stream should return 0 when it reaches the end
    /// or else we will not loop to the start again.</param>
    public LoopStream(WaveStream sourceStream)
    {
        this.sourceStream = sourceStream;
        this.EnableLooping = true;
    }

    /// <summary>
    /// Use this to turn looping on or off
    /// </summary>
    public bool EnableLooping { get; set; }

    /// <summary>
    /// Return source stream's wave format
    /// </summary>
    public override WaveFormat WaveFormat
    {
        get { return sourceStream.WaveFormat; }
    }

    /// <summary>
    /// LoopStream simply returns
    /// </summary>
    public override long Length
    {
        get { return sourceStream.Length; }
    }

    /// <summary>
    /// LoopStream simply passes on positioning to source stream
    /// </summary>
    public override long Position
    {
        get { return sourceStream.Position; }
        set { sourceStream.Position = value; }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        int totalBytesRead = 0;

        while (totalBytesRead < count)
        {
            int bytesRead = sourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
            if (bytesRead == 0)
            {
                if (sourceStream.Position == 0 || !EnableLooping)
                {
                    // something wrong with the source stream
                    break;
                }
                // loop
                sourceStream.Position = 0;
            }
            totalBytesRead += bytesRead;
        }
        return totalBytesRead;
    }
}
