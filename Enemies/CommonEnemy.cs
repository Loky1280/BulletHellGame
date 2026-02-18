using System.Windows.Media.Imaging;
using General_logic;
using System.Windows;

namespace Enemies
{
    public class CommonEnemy : Enemy
    {
        private static BitmapSource[] _frames;
        private const int FrameW = 23, FrameH = 28, FrameCount = 4;

        private static void EnsureFrames()
        {
            if (_frames != null) return;
            var sheet = new BitmapImage(new System.Uri("pack://application:,,,/Resource/Enemy/PistolEnemy.png"));
            _frames = new BitmapSource[FrameCount];
            for (int i = 0; i < FrameCount; i++)
                _frames[i] = new CroppedBitmap(sheet, new System.Windows.Int32Rect(i * FrameW, 0, FrameW, FrameH));
        }

        protected override BitmapSource[] GetAnimationFrames() { EnsureFrames(); return _frames; }
        public override int GetAnimationFrameWidth() => FrameW;
        public override int GetAnimationFrameHeight() => FrameH;

        public CommonEnemy(Point position, Weapon weapon) : base(position, weapon)
        {
            Health = MaxHealth = 4;
            MoveSpeed = 80;
            Damage = 10;
            AttackRadius = 200;
            CloseRange = 30;
            NormalAttackCooldown = 1.0;
            FastAttackCooldown = 0.3;
            AttackCooldown = NormalAttackCooldown;
            AttackTimer = 0;
            IsAlive = true;
            IsBoss = false;
        }
    }
}