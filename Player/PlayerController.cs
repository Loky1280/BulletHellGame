using Player;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace BulletHellGame
{
    public class PlayerController
    {
        public PlayerMove Player { get; private set; }

        public Image PlayerImage { get; private set; }

        private class AnimationData
        {
            public BitmapImage SpriteSheet { get; set; }
            public int FrameWidth { get; set; }
            public int FrameHeight { get; set; }
            public int TotalFrames { get; set; }
            public double AnimationSpeed { get; set; } = 8.0;
            public CroppedBitmap[] Frames { get; set; }
        }

        private Dictionary<string, AnimationData> animations;
        private string currentAnim = "walk_down";
        private string previousAnim = "";
        private string direction = "down";
        private bool isDashing = false;

        private int currentFrame = 0;
        private double animTimer = 0;

        private double lastPlayerX;
        private double lastPlayerY;

        public PlayerController(PlayerMove player, Image playerImage)
        {
            Player = player;
            PlayerImage = playerImage;

            lastPlayerX = Player.X;
            lastPlayerY = Player.Y;

            LoadAnimations();
        }

        private void LoadAnimations()
        {
            animations = new Dictionary<string, AnimationData>
            {
                { "dash_up", new AnimationData { SpriteSheet = new BitmapImage(new Uri("pack://application:,,,/Resource/Player/Dash/Dash_up.png")), FrameWidth = 16, FrameHeight = 28, TotalFrames = 8, AnimationSpeed = 20 } },
                { "dash_down", new AnimationData { SpriteSheet = new BitmapImage(new Uri("pack://application:,,,/Resource/Player/Dash/Dash_Down.png")), FrameWidth = 16, FrameHeight = 28, TotalFrames = 8, AnimationSpeed = 20 } },
                { "dash_left_down", new AnimationData { SpriteSheet = new BitmapImage(new Uri("pack://application:,,,/Resource/Player/Dash/Dash_left_Down.png")), FrameWidth = 22, FrameHeight = 28, TotalFrames = 8, AnimationSpeed = 20 } },
                { "dash_right_down", new AnimationData { SpriteSheet = new BitmapImage(new Uri("pack://application:,,,/Resource/Player/Dash/Dash_right_Down.png")), FrameWidth = 22, FrameHeight = 28, TotalFrames = 8, AnimationSpeed = 20 } },

                { "walk_up", new AnimationData { SpriteSheet = new BitmapImage(new Uri("pack://application:,,,/Resource/Player/Walk/walk_up.png")), FrameWidth = 16, FrameHeight = 28, TotalFrames = 8, AnimationSpeed = 8 } },
                { "walk_down", new AnimationData { SpriteSheet = new BitmapImage(new Uri("pack://application:,,,/Resource/Player/Walk/walk_down.png")), FrameWidth = 16, FrameHeight = 28, TotalFrames = 8, AnimationSpeed = 8 } },
                { "walk_left_down", new AnimationData { SpriteSheet = new BitmapImage(new Uri("pack://application:,,,/Resource/Player/Walk/walk_left_down.png")), FrameWidth = 16, FrameHeight = 28, TotalFrames = 8, AnimationSpeed = 8 } },
                { "walk_right_down", new AnimationData { SpriteSheet = new BitmapImage(new Uri("pack://application:,,,/Resource/Player/Walk/walk_right_down.png")), FrameWidth = 16, FrameHeight = 28, TotalFrames = 8, AnimationSpeed = 8 } },
            };

            foreach (var kvp in animations)
            {
                var anim = kvp.Value;
                anim.Frames = new CroppedBitmap[anim.TotalFrames];
                for (int i = 0; i < anim.TotalFrames; i++)
                {
                    anim.Frames[i] = new CroppedBitmap(
                        anim.SpriteSheet,
                        new System.Windows.Int32Rect(i * anim.FrameWidth, 0, anim.FrameWidth, anim.FrameHeight));
                }
            }
        }

        public void Animate(double dt)
        {
            double dx = Player.X - lastPlayerX;
            double dy = Player.Y - lastPlayerY;
            bool isMoving = Math.Abs(dx) > 0.1 || Math.Abs(dy) > 0.1;

            if (isMoving)
            {
                if (Math.Abs(dx) > Math.Abs(dy))
                {
                    direction = dx > 0 ? "right_down" : "left_down";
                }
                else
                {
                    direction = dy > 0 ? "down" : "up";
                }
            }

            isDashing = Player.isDashing;
            currentAnim = (isDashing ? "dash_" : "walk_") + direction;

            if (!animations.TryGetValue(currentAnim, out var anim))
                return;

            double frameDuration = anim.AnimationSpeed > 0 ? 1.0 / anim.AnimationSpeed : 0.1;

            if (currentAnim != previousAnim)
            {
                currentFrame = 0;
                animTimer = frameDuration;
                previousAnim = currentAnim;
            }

            animTimer -= dt;
            if (animTimer <= 0)
            {
                animTimer = frameDuration;
                currentFrame = (currentFrame + 1) % anim.TotalFrames;
            }

            if (anim.Frames != null && anim.Frames.Length > 0)
            {
                int index = Math.Min(currentFrame, anim.Frames.Length - 1);
                PlayerImage.Source = anim.Frames[index];
            }

            lastPlayerX = Player.X;
            lastPlayerY = Player.Y;
        }
    }
}
