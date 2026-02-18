using General_logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Player
{
    public class Character
    {
        private const int StartHelth = 4;
        private const int StartArmore = 2;
        private const int StartCoins = 0;
        public int Health { get; set; }
        public Weapon Gun { get; set; }
        public int Armor { get; set; }
        private int Coins { get; set; }

        private bool immunity = false;

        public event EventHandler OnDeath;

        public Character()
        {
            Health = StartHelth;
            Gun = new Pistol();
            Armor = StartArmore;
            Coins = StartCoins;
        }

        public void TakeDamage(int damage)
        {
            if (immunity)
            {
                return; 
            }

            if (Armor != 0)
                Armor--;
            else
                Health--;

            immunity = true;

            System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += (s, e) =>
            {
                immunity = false;
                timer.Stop();
            };
            timer.Start();

            if (Health <= 0)
            {
                Health = 0;
                OnDeath?.Invoke(this, EventArgs.Empty);
            }
        }

    }
}