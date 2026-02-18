using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using GameWorld;
using General_logic;
using Player;
using Enemies;
using System.Diagnostics;

namespace BulletHellGame
{
    public partial class Floor1 : Window
    {
        private List<Room> rooms;
        private List<Corridor> corridors;
        private WorldMap worldMap;
        private List<Projectile> projectiles = new List<Projectile>();

        private Vector cameraOffset;
        private PlayerController playerController;
        private DispatcherTimer gameTimer;
        private Point SpawnPoint;
        private bool isMiniMapVisible = false;
        private bool isLeftMouseDown = false;
        private DateTime spawnTime;
        private DateTime lastUpdateTime;

        private Rect? treasureRect = null;
        private Weapon treasureWeapon = null;

        private int playerMoney = 0; 
        public int stageNumber = 1;

        private List<Rect> shopHpRects = new List<Rect>();
        private List<Rect> shopArmorRects = new List<Rect>();
        private Room shopRoom = null;

        private ImageBrush floorBrush;
        private ImageBrush wallBrush;

        private Dictionary<Enemy, Shape> enemyShapePool = new Dictionary<Enemy, Shape>();

        private readonly Dictionary<string, BitmapImage> _weaponTextureCache = new Dictionary<string, BitmapImage>();

    
        private (BitmapImage img, int w, int h) GetWeaponTextureAndSize(Weapon gun, bool useLeftTexture = false)
        {
            if (gun == null || gun.TextureWidth <= 0 || gun.TextureHeight <= 0)
                return (null, 0, 0);
            string uri = useLeftTexture ? (gun.TextureUriLeft ?? gun.TextureUriRight) : (gun.TextureUriRight ?? gun.TextureUriLeft);
            if (string.IsNullOrEmpty(uri))
                return (null, 0, 0);
            if (!_weaponTextureCache.TryGetValue(uri, out BitmapImage img))
            {
                img = new BitmapImage(new Uri(uri));
                _weaponTextureCache[uri] = img;
            }
            return (img, gun.TextureWidth, gun.TextureHeight);
        }

        public Floor1()
        {
            InitializeComponent();

            LevelGenerator generator = new LevelGenerator();
            var result = generator.GenerateRooms(8 * stageNumber, 800, 600);
            rooms = result.Rooms;
            Room MainRoom = rooms[0];
            Room BossRoom = rooms.FirstOrDefault(r => r.IsBossRoom);
            corridors = result.Corridors;

            worldMap = new WorldMap();
            worldMap.BuildFromLevel(rooms, corridors,
                "pack://application:,,,/Resource/World/Floor128x128.png",
                "pack://application:,,,/Resource/World/Empty.png",
                "pack://application:,,,/Resource/World/Corridor64x64.png");

            worldMap.BuildMiniMap(rooms, corridors, SpawnPoint, 230, 0.03);

            if (BossRoom != null)
            {
                double bossX = BossRoom.X + BossRoom.Width / 2;
                double bossY = BossRoom.Y + BossRoom.Height / 2;
                var boss = new Enemies.BossEnemy(new Point(bossX, bossY));
                BossRoom.enemies = new List<Enemies.Enemy> { boss };
            }
            FillWorld.SpawnEnemiesToRooms(rooms, corridors, MainRoom, BossRoom);
            SpawnPoint = GenerateHelp.GetSpawnPoint(MainRoom);
            SpawnPoint.X += 15;
            SpawnPoint.Y += 15;

            spawnTime = DateTime.Now;

            var playerMove = new PlayerMove(SpawnPoint.X - 25, SpawnPoint.Y - 25);
            playerMove.Character = new Character();
            playerMove.Character.OnDeath += (s, e) =>
            {
                gameTimer.Stop();
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            };

            playerController = new PlayerController(playerMove, PlayerImage);

            lastUpdateTime = DateTime.Now;
            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromMilliseconds(16); 
            gameTimer.Tick += GameLoop;
            gameTimer.Start();

            MouseDown += OnMouseDown;
            MouseUp += OnMouseUp;
            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;

            Room treasureRoom = rooms.FirstOrDefault(r => r.IsTreasureRoom);
            if (treasureRoom != null)
            {
                double centerX = treasureRoom.X + treasureRoom.Width / 2 - 25;
                double centerY = treasureRoom.Y + treasureRoom.Height / 2 - 25;
                treasureRect = new Rect(centerX, centerY, 50, 50);
                var rnd = new Random();
                int weaponType = rnd.Next(3);
                switch (weaponType)
                {
                    case 0: treasureWeapon = new ShotGun(); break;
                    case 1: treasureWeapon = new AutomaticWeapon(); break;
                    default: treasureWeapon = new Rifle(); break;
                }
            }

            shopRoom = rooms.FirstOrDefault(r => r.IsShopRoom);
            if (shopRoom != null)
            {
                double centerX = shopRoom.X + shopRoom.Width / 2;
                double centerY = shopRoom.Y + shopRoom.Height / 2;
                double spacing = 60;
                double itemSize = 40;
                shopHpRects.Clear();
                shopArmorRects.Clear();
                shopHpRects.Add(new Rect(centerX - spacing - itemSize, centerY - itemSize / 2, itemSize, itemSize));
                shopHpRects.Add(new Rect(centerX - spacing * 2 - itemSize * 2, centerY - itemSize / 2, itemSize, itemSize));
                shopArmorRects.Add(new Rect(centerX + spacing, centerY - itemSize / 2, itemSize, itemSize));
                shopArmorRects.Add(new Rect(centerX + spacing * 2 + itemSize, centerY - itemSize / 2, itemSize, itemSize));
            }

            foreach (var room in rooms)
            {
                if (room.enemies == null) continue;
                foreach (var enemy in room.enemies)
                {
                    Shape enemyShape = enemy.GetShape();
                    enemyShape.Fill = Brushes.Red;
                    enemyShape.Stroke = null;
                    enemyShapePool[enemy] = enemyShape;

                    enemy.OnEnemyKilled += () =>
                    {
                        if (new Random().NextDouble() < 0.25)
                            playerMoney++;

                        if (enemy is BossEnemy)
                        {
                            gameTimer.Stop();
                            new FinishPage().Show();
                            this.Close();
                        }
                    };
                }
            }

            wallBrush = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resource/World/Empty.png")));

            
        }

        private void GameLoop(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            double dt = Math.Min((now - lastUpdateTime).TotalSeconds, 0.1);
            lastUpdateTime = now;

            List<Rect> allowedAreas = new List<Rect>();

            Room currentRoom = GenerateHelp.GetPlayerCurrentRoom(rooms, playerController.Player);

            if (currentRoom != null && currentRoom.PeaceFulTime > 0)
            {
                currentRoom.PeaceFulTime -= dt;
                if (currentRoom.PeaceFulTime < 0) currentRoom.PeaceFulTime = 0;
            }

            FillWorld.HandleRoomEntry(currentRoom);

            bool playerInRoom = currentRoom != null;
            Point playerPos = new Point(playerController.Player.X, playerController.Player.Y);

            foreach (var room in rooms)
            {
                if (room.enemies == null) continue;
                if (room.IsVisited && room.enemies.Count > 0)
                {
                    bool hasAliveEnemies = false;
                    foreach (var enemy in room.enemies)
                    {
                        if (enemy.IsAlive)
                        {
                            hasAliveEnemies = true;
                            enemy.UpdateAI(playerPos, playerInRoom && room == currentRoom, dt, projectiles, room.enemies, room.PeaceFulTime);
                            enemy.UpdateAnimation(dt);
                        }
                    }
                    if (!hasAliveEnemies)
                    {
                        room.IsVisited = false;
                    }
                }
            }

            if (currentRoom != null && currentRoom.IsBattleBegin)
            {
                allowedAreas.Add(currentRoom.Bounds);
            }
            else
            {
                foreach (var room in rooms)
                    allowedAreas.Add(room.Bounds);

                foreach (var corridor in corridors)
                    foreach (var segment in corridor.Segments)
                        allowedAreas.Add(segment);
            }

            List<Enemy> currentEnemies = currentRoom?.enemies ?? new List<Enemy>();
            playerController.Player.Update(allowedAreas, currentEnemies, dt);

            if (isLeftMouseDown)
            {

                if (Mouse.LeftButton != MouseButtonState.Pressed)
                {
                    isLeftMouseDown = false;
                }
                else
                {
                    Point mousePos = Mouse.GetPosition(GameCanvas);
                    Point worldMousePos = new Point(mousePos.X + cameraOffset.X, mousePos.Y + cameraOffset.Y);

                    List<Projectile> additionalPellets;
                    var projectile = playerController.Player.Shoot(worldMousePos, out additionalPellets);
                    if (projectile != null)
                    {
                        projectiles.Add(projectile);
                        projectiles.AddRange(additionalPellets);
                    }
                }
            }

            if (treasureRect.HasValue && treasureWeapon != null)
            {
                if (Collision.CheckPlayerTreasureCollision(playerController.Player, PlayerImage.Width, PlayerImage.Height, treasureRect.Value))
                {
                    playerController.Player.Character.Gun = treasureWeapon;
                    treasureRect = null;
                    treasureWeapon = null;
                }
            }

            if (shopRoom != null && shopRoom == currentRoom)
            {
                int itemIndex;
                if (Collision.CheckPlayerShopItemCollision(playerController.Player, PlayerImage.Width, PlayerImage.Height, shopHpRects, out itemIndex))
                {
                    if (playerMoney >= 2 && playerController.Player.Character.Health < 4)
                    {
                        playerMoney -= 2;
                        playerController.Player.Character.Health++;
                        shopHpRects.RemoveAt(itemIndex);
                    }
                }
                if (Collision.CheckPlayerShopItemCollision(playerController.Player, PlayerImage.Width, PlayerImage.Height, shopArmorRects, out itemIndex))
                {
                    if (playerMoney >= 3)
                    {
                        playerMoney -= 3;
                        playerController.Player.Character.Armor++;
                        shopArmorRects.RemoveAt(itemIndex);
                    }
                }
            }

            if (isMiniMapVisible)
                DrawMiniMap();

            if (playerController.Player.Character?.Gun is Weapon weapon)
            {
                weapon.Update(dt);
            }

            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                var proj = projectiles[i];
                if (proj == null)
                {
                    projectiles.RemoveAt(i);
                    continue;
                }

                proj.Update(dt);

                if (Collision.CheckProjectileWallCollision(proj, rooms, corridors))
                {
                    projectiles.RemoveAt(i);
                    continue;
                }

                bool hitTarget = false;
                if (proj.Owner is PlayerMove)
                {
                    Enemy hitEnemy;
                    if (Collision.CheckProjectileEnemyCollision(proj, currentRoom?.enemies, out hitEnemy))
                    {
                        hitEnemy.TakeDamage(proj.Damage);
                        hitTarget = true;
                    }
                }
                else if (proj.Owner is Enemy)
                {
                    if (currentRoom != null && Collision.CheckProjectilePlayerCollision(proj, playerController.Player, PlayerImage.Width, PlayerImage.Height))
                    {
                        playerController.Player.Character.TakeDamage(proj.Damage);
                        hitTarget = true;
                    }
                }

                if (hitTarget)
                {
                    projectiles.RemoveAt(i);
                    continue;
                }

                if (Collision.IsProjectileOutOfBounds(proj))
                {
                    projectiles.RemoveAt(i);
                }
            }

            double windowCenterX = ActualWidth / 2;
            double windowCenterY = ActualHeight / 2;

            cameraOffset = new Vector(playerController.Player.X - windowCenterX + PlayerImage.Width / 2,
                                      playerController.Player.Y - windowCenterY + PlayerImage.Height / 2);
            if (isMiniMapVisible)
            {
                TimeSpan elapsed = DateTime.Now - spawnTime;
                TimerText.Text = $"Час гри: {elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
                TimerText.Visibility = Visibility.Visible;
            }
            else
            {
                TimerText.Visibility = Visibility.Collapsed;
            }

            playerController.Animate(dt);

            HpText.Text = $"HP: {playerController.Player.Character?.Health ?? 0}";
            ArmorText.Text = $"Armor: {playerController.Player.Character?.Armor ?? 0}";
            MoneyText.Text = $"Money: {playerMoney}";

            Weapon weapone = playerController.Player.Character.Gun;
            if (weapone != null)
            {
                AmmoText.Text = $"Ammo: {weapone.AmmoInWeapon}/{weapone.MaxAmmo}";
                ReloadText.Visibility = weapone.IsReloading ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                AmmoText.Text = "Ammo: 0/0";
                ReloadText.Visibility = Visibility.Collapsed;
            }

            RedrawWorld(dt);
        }

        
       

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                isMiniMapVisible = !isMiniMapVisible;
                MiniMapCanvas.Visibility = isMiniMapVisible ? Visibility.Visible : Visibility.Collapsed;
                if (isMiniMapVisible)
                    DrawMiniMap();
            }

            if (e.Key == Key.R)
            {
                if (playerController.Player.Character?.Gun is Weapon weapon)
                {
                    weapon.Reload();
                }
            }

            playerController.Player.OnKeyDown(e.Key);
        }
        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                isLeftMouseDown = true;
                Point mousePos = e.GetPosition(GameCanvas);
                Point worldMousePos = new Point(mousePos.X + cameraOffset.X, mousePos.Y + cameraOffset.Y);

                List<Projectile> additionalPellets;
                var projectile = playerController.Player.Shoot(worldMousePos, out additionalPellets);
                if (projectile != null)
                {
                    projectiles.Add(projectile);
                    projectiles.AddRange(additionalPellets);
                }
            }
        }
        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                isLeftMouseDown = false;
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            playerController.Player.OnKeyUp(e.Key);
        }

        private void RedrawWorld(double dt)
        {
            GameCanvas.Children.Clear();

            if (worldMap?.MapBitmap != null)
            {
                var mapImage = new Image
                {
                    Source = worldMap.MapBitmap,
                    Width = worldMap.WorldBounds.Width,
                    Height = worldMap.WorldBounds.Height
                };
                Canvas.SetLeft(mapImage, worldMap.WorldBounds.Left - cameraOffset.X);
                Canvas.SetTop(mapImage, worldMap.WorldBounds.Top - cameraOffset.Y);
                GameCanvas.Children.Add(mapImage);
            }

            foreach (Room room in rooms)
            {
                if (room.enemies == null) continue;
                foreach (var enemy in room.enemies)
                {
                    if (!enemy.IsAlive) continue;

                    var frameBitmap = enemy.GetCurrentFrameBitmap();
                    int frameW = enemy.GetAnimationFrameWidth();
                    int frameH = enemy.GetAnimationFrameHeight();

                    if (frameBitmap != null && frameW > 0 && frameH > 0)
                    {
                        var enemyImage = new Image
                        {
                            Source = frameBitmap,
                            Width = frameW,
                            Height = frameH
                        };
                        Canvas.SetLeft(enemyImage, enemy.Position.X - cameraOffset.X - frameW / 2.0);
                        Canvas.SetTop(enemyImage, enemy.Position.Y - cameraOffset.Y - frameH / 2.0);
                        GameCanvas.Children.Add(enemyImage);
                    }
                    else
                    {
                        if (!enemyShapePool.TryGetValue(enemy, out Shape enemyShape))
                        {
                            enemyShape = enemy.GetShape();
                            enemyShape.Fill = Brushes.Red;
                            enemyShape.Stroke = null;
                            enemyShapePool[enemy] = enemyShape;
                        }
                        Canvas.SetLeft(enemyShape, enemy.Position.X - cameraOffset.X - enemyShape.Width / 2);
                        Canvas.SetTop(enemyShape, enemy.Position.Y - cameraOffset.Y - enemyShape.Height / 2);
                        GameCanvas.Children.Add(enemyShape);
                    }

                    if (enemy.Gun != null)
                    {
                        double playerX = playerController.Player.X;
                        double playerY = playerController.Player.Y;
                        bool enemyGunLeft = playerX < enemy.Position.X;
                        var (enemyGunImg, enemyGunW, enemyGunH) = GetWeaponTextureAndSize(enemy.Gun, enemyGunLeft);
                        if (enemyGunImg != null && enemyGunW > 0 && enemyGunH > 0)
                        {
                            double angleDeg = Math.Atan2(playerY - enemy.Position.Y, playerX - enemy.Position.X) * 180 / Math.PI;
                            double rotation = enemyGunLeft ? angleDeg + 180 : angleDeg;
                            double enemyScreenX = enemy.Position.X - cameraOffset.X;
                            double enemyScreenY = enemy.Position.Y - cameraOffset.Y;
                            var enemyGunImage = new Image
                            {
                                Source = enemyGunImg,
                                Width = enemyGunW,
                                Height = enemyGunH,
                                RenderTransformOrigin = new Point(enemyGunLeft ? 1 : 0, 0.5),
                                RenderTransform = new RotateTransform(rotation)
                            };
                            Canvas.SetLeft(enemyGunImage, enemyGunLeft ? enemyScreenX - enemyGunW : enemyScreenX);
                            Canvas.SetTop(enemyGunImage, enemyScreenY - enemyGunH / 2);
                            GameCanvas.Children.Add(enemyGunImage);
                        }
                    }
                }
            }

            foreach (var proj in projectiles)
            {
                Ellipse bullet = new Ellipse
                {
                    Width = 19,
                    Height = 19,                  
                    Fill = proj.Owner is Enemy ? Brushes.Red : Brushes.Blue,
                    StrokeThickness = 3
                };
                if(proj.Owner is PlayerMove && playerController.Player.Character?.Gun is ShotGun)
                {
                    bullet.Width = 12;
                    bullet.Height = 12;                                      
                }
                Canvas.SetLeft(bullet, proj.Position.X - cameraOffset.X - bullet.Width / 2);
                Canvas.SetTop(bullet, proj.Position.Y - cameraOffset.Y - bullet.Height / 2);
                GameCanvas.Children.Add(bullet);
            }

            Canvas.SetLeft(PlayerImage, ActualWidth / 2 - PlayerImage.Width / 2);
            Canvas.SetTop(PlayerImage, ActualHeight / 2 - PlayerImage.Height / 2);
            GameCanvas.Children.Add(PlayerImage);

            var gun = playerController.Player.Character?.Gun;
            Point mousePos = Mouse.GetPosition(GameCanvas);
            double centerX = ActualWidth / 2;
            double centerY = ActualHeight / 2;
            bool mouseOnLeft = mousePos.X < centerX;
            var (gunImg, gunW, gunH) = GetWeaponTextureAndSize(gun, mouseOnLeft);
            if (gunImg != null && gunW > 0 && gunH > 0)
            {
                double angleDeg = Math.Atan2(mousePos.Y - centerY, mousePos.X - centerX) * 180 / Math.PI;
                double rotation = mouseOnLeft ? angleDeg + 180 : angleDeg;
                var gunImage = new Image
                {
                    Source = gunImg,
                    Width = gunW,
                    Height = gunH,
                    RenderTransformOrigin = new Point(mouseOnLeft ? 1 : 0, 0.5),
                    RenderTransform = new RotateTransform(rotation)
                };
                Canvas.SetLeft(gunImage, mouseOnLeft ? centerX - gunW : centerX);
                Canvas.SetTop(gunImage, centerY - gunH / 2);
                GameCanvas.Children.Add(gunImage);
            }

            if (treasureRect.HasValue)
            {
                var treasureRectBox = treasureRect.Value;
                var (treasureImg, tw, th) = GetWeaponTextureAndSize(treasureWeapon, useLeftTexture: false);
                if (treasureImg != null && tw > 0 && th > 0)
                {
                    var treasureWeaponImage = new Image
                    {
                        Source = treasureImg,
                        Width = tw,
                        Height = th
                    };
                    Canvas.SetLeft(treasureWeaponImage, treasureRectBox.X + (treasureRectBox.Width - tw) / 2 - cameraOffset.X);
                    Canvas.SetTop(treasureWeaponImage, treasureRectBox.Y + (treasureRectBox.Height - th) / 2 - cameraOffset.Y);
                    GameCanvas.Children.Add(treasureWeaponImage);
                }
                var treasureBox = new Rectangle
                {
                    Width = treasureRectBox.Width,
                    Height = treasureRectBox.Height,
                    Fill = Brushes.Transparent,
                    Stroke = Brushes.Gold,
                    StrokeThickness = 3
                };
                Canvas.SetLeft(treasureBox, treasureRectBox.X - cameraOffset.X);
                Canvas.SetTop(treasureBox, treasureRectBox.Y - cameraOffset.Y);
                GameCanvas.Children.Add(treasureBox);
            }

            if (shopRoom != null)
            {
                foreach (var shopHpRect in shopHpRects)
                {
                    Rectangle hpRect = new Rectangle
                    {
                        Width = shopHpRect.Width,
                        Height = shopHpRect.Height,
                        Fill = Brushes.Pink,
                        Stroke = Brushes.Red,
                        StrokeThickness = 2
                    };
                    Canvas.SetLeft(hpRect, shopHpRect.X - cameraOffset.X);
                    Canvas.SetTop(hpRect, shopHpRect.Y - cameraOffset.Y);
                    GameCanvas.Children.Add(hpRect);
                }
                foreach (var shopArmorRect in shopArmorRects)
                {
                    Rectangle armorRect = new Rectangle
                    {
                        Width = shopArmorRect.Width,
                        Height = shopArmorRect.Height,
                        Fill = Brushes.LightBlue,
                        Stroke = Brushes.DarkBlue,
                        StrokeThickness = 2
                    };
                    Canvas.SetLeft(armorRect, shopArmorRect.X - cameraOffset.X);
                    Canvas.SetTop(armorRect, shopArmorRect.Y - cameraOffset.Y);
                    GameCanvas.Children.Add(armorRect);
                }
            }
        }

        private void DrawMiniMap()
        {
            MiniMapCanvas.Children.Clear();

            if (worldMap?.MiniMapBitmap != null)
            {
                var miniMapImage = new Image
                {
                    Source = worldMap.MiniMapBitmap,
                    Width = 230,
                    Height = 230
                };
                Canvas.SetLeft(miniMapImage, 0);
                Canvas.SetTop(miniMapImage, 0);
                MiniMapCanvas.Children.Add(miniMapImage);
            }

            Ellipse playerDot = new Ellipse
            {
                Width = 6,
                Height = 6,
                Fill = Brushes.Red
            };

            double scale = 0.03;
            var center = worldMap?.MiniMapCenter ?? SpawnPoint;
            double playerOffsetX = (playerController.Player.X - center.X) * scale + MiniMapCanvas.Width / 2;
            double playerOffsetY = (playerController.Player.Y - center.Y) * scale + MiniMapCanvas.Height / 2;

            Canvas.SetLeft(playerDot, playerOffsetX - playerDot.Width / 2);
            Canvas.SetTop(playerDot, playerOffsetY - playerDot.Height / 2);
            MiniMapCanvas.Children.Add(playerDot);
        }

        public void AddProjectile(Projectile projectile)
        {
            projectiles.Add(projectile);
        }
    }
}
