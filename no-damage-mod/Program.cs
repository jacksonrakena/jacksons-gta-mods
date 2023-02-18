using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA.Native;
using GTA;
using System.Drawing;
using GTA.UI;
using Microsoft.Win32;
using System.IO;
using System.Windows.Forms;
using ModCommon;

namespace DarkViperOhko
{
    public class OhkoConstants : Script
    {
        public static bool enabled = true;
        public const string MOD_IDENTIFIER = "ohko";
        
        public static readonly ModConfig Configuration = new ModConfig(MOD_IDENTIFIER);
        public static readonly ModConfigSection Keybinds = Configuration.GetSection("Keybinds");
        public static readonly ModConfigSection Display = Configuration.GetSection("Display");

        public static readonly Keys IncreaseDeathCount = Keybinds.GetEnum("IncreaseDeathCount", Keys.PageUp);// (Keys) Enum.Parse(typeof(Keys), Configuration.IniReadValue("Keybinds", "IncreaseDeathCount") ?? "PageUp");
        public static readonly Keys DecreaseDeathCount = Keybinds.GetEnum("DecreaseDeathCount", Keys.PageDown);//(Keys)Enum.Parse(typeof(Keys), Configuration.IniReadValue("Keybinds", "DecreaseDeathCount") ?? "PageDown");
        public static readonly Keys ToggleMod = Keybinds.GetEnum("ToggleMod", Keys.F8); //(Keys)Enum.Parse(typeof(Keys), Configuration.IniReadValue("Keybinds", "ToggleMod") ?? "F8");
        public static readonly Keys ToggleDeathCounter = Keybinds.GetEnum("ToggleDeathCounter", Keys.F7);// (Keys)Enum.Parse(typeof(Keys), Configuration.IniReadValue("Keybinds", "ToggleDeathCounter") ?? "F7");
        public static readonly bool ShowDefaultDeathCounter = Display.GetBoolean("ShowDefaultDeathCounter", true);// bool.Parse(Configuration.IniReadValue("Display", "ShowDefaultDeathCounter") ?? "true");

        public static bool RenderDeathCounter = ShowDefaultDeathCounter;

        public OhkoConstants()
        {
            KeyUp += OhkoConstants_KeyUp;
        }

        private void OhkoConstants_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == ToggleDeathCounter)
            {
                RenderDeathCounter = !RenderDeathCounter;
            }
        }
    }
    
    public class DeathTracker: Script
    {
        public static int deaths;
        public static bool isDead;
        public static string deathSavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ohko_stats.txt");

        public DeathTracker()
        {
            Tick += DeathCountScript_Tick;
            KeyUp += DeathTracker_KeyUp;
            try
            {
                if (File.Exists(deathSavePath))
                {
                    deaths = int.Parse(File.ReadAllText(deathSavePath));
                }
            }
            catch (Exception)
            {
                Notification.Show("Failed to load deaths. Is ohko_stats.txt corrupt?");
            }
        }

        private void DeathTracker_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == OhkoConstants.IncreaseDeathCount)
            {
                SetDeathCount(deaths + 1);
            } else if (e.KeyCode == OhkoConstants.DecreaseDeathCount)
            {
                SetDeathCount(deaths - 1);
            }
        }

        private void SetDeathCount(int newd)
        {
            deaths = newd;
            try
            {
                File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ohko_stats.txt"), deaths.ToString());
            }
            catch (Exception ex)
            {
                Notification.Show(ex.Message);
            }
        }
        
        private readonly TextElement deathCounter = new TextElement("0", new PointF(1100, 450), 1f, Color.White, GTA.UI.Font.ChaletComprimeCologne, Alignment.Left, true, true);

        private void DeathCountScript_Tick(object sender, EventArgs e)
        {
            if (isDead && !Game.Player.IsDead)
            {
                isDead = false;
            }
            else if (!isDead && Game.Player.IsDead)
            {
                isDead = true;
                SetDeathCount(deaths + 1);
            }
            
            if (OhkoConstants.RenderDeathCounter && OhkoConstants.enabled)
            {
                deathCounter.Caption = deaths + " death" + (deaths != 1 ? "s" : "");
                deathCounter.ScaledDraw();
            }
        }
    }

    public class OhkoScript: Script
    {
        private Model trevor = new Model(PedHash.Trevor);
        public OhkoScript()
        {
            Tick += OhkoScript_Tick;
            KeyUp += OhkoScript_KeyUp;
            Interval = 1000;
            Notification.Show("Loaded Abyssal's No Damage mod.");
        }

        private void OhkoScript_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == OhkoConstants.ToggleMod)
            {
                OhkoConstants.enabled = !OhkoConstants.enabled;
                if (!OhkoConstants.enabled)
                {
                    Function.Call(Hash.SET_ENTITY_MAX_HEALTH, Game.Player.Character.Handle, 200);
                    Function.Call(Hash.SET_ENTITY_HEALTH, Game.Player.Character.Handle, 200);

                    Game.Player.IsSpecialAbilityEnabled = true;
                    Game.Player.RefillSpecialAbility();
                }

                Notification.Show(OhkoConstants.enabled ? "OHKO Enabled" : "OHKO Disabled");
                OhkoScript_Tick(null, null);
            }
        }

        private bool IsTrevor()
        {
            return Game.Player.Character.Model == trevor;
        }

        private void OhkoScript_Tick(object sender, EventArgs e)
        {
            try
            {
                if (!OhkoConstants.enabled)
                {
                    if (Function.Call<int>(Hash.GET_ENTITY_MAX_HEALTH, Game.Player.Character.Handle) != 200)
                    {
                        Function.Call(Hash.SET_ENTITY_MAX_HEALTH, Game.Player.Character.Handle, 200);
                    }
                    Game.Player.IsSpecialAbilityEnabled = true;
                } 
                else
                {
                    if (Function.Call<int>(Hash.GET_ENTITY_MAX_HEALTH, Game.Player.Character.Handle) > 101)
                    {
                        Function.Call(Hash.SET_ENTITY_MAX_HEALTH, Game.Player.Character.Handle, 101);
                        Function.Call(Hash.SET_ENTITY_HEALTH, Game.Player.Character.Handle, 101);
                    }
                    if (Function.Call<int>(Hash.GET_ENTITY_HEALTH, Game.Player.Character.Handle) > 101)
                    {
                        Function.Call(Hash.SET_ENTITY_HEALTH, Game.Player.Character.Handle, 101);
                    }

                    Game.Player.Character.Armor = 0;
                    Game.Player.MaxArmor = 100;
                    
                    if (IsTrevor())
                    {
                        Game.Player.IsSpecialAbilityEnabled = false;
                        Game.Player.DepleteSpecialAbility();
                    } else
                    {
                        Game.Player.IsSpecialAbilityEnabled = true;
                    }
                }
            }
            catch (Exception e2)
            {
                try
                {
                    Notification.Show("Exception during tick: " + e2.Message);
                }
                catch (Exception)
                {

                }
            }
        }
    }
}
