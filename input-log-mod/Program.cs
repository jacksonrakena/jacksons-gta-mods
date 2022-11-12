using System;
using System.Collections.Generic;
using GTA;
using System.Drawing;
using GTA.UI;
using System.Windows.Forms;
using Control = GTA.Control;
using System.Threading;
using System.Collections.ObjectModel;
using System.Linq;
using System.Data;

namespace InputDetect
{
    public class DisplayRow
    {
        public string Key { get; set; }
        public string Presses { get; set; }
    }
    public class StatisticsDisplayScript : Script
    {
        public const int DISPLAY_UPDATE_INTERVAL_TICKS = 50;
        private ulong lastUpdateTime = 0;
        private ulong currentTick = 0;
        private readonly StatisticsDisplay _display;
        private readonly object _tableUpdateLock = new object();
        public DataTable DataTable = new DataTable();
        public StatisticsDisplayScript()
        {
            _display = new StatisticsDisplay();
            _display.Opacity = 60;
            
            var keyCol = DataTable.Columns.Add("Key");
            using (System.Drawing.Font font = new System.Drawing.Font(
            _display.dataGridView1.DefaultCellStyle.Font.FontFamily, 25, FontStyle.Bold))
            {
                _display.dataGridView1.DefaultCellStyle.Font = font;
            }
            _display.dataGridView1.RowsDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            _display.dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
            var valCol = DataTable.Columns.Add("Presses", typeof(int));
            var computedCol = DataTable.Columns.Add();
            DataTable.DefaultView.Sort = "Presses DESC";
            DataTable.PrimaryKey = new[] { keyCol };
            DataTable.AcceptChanges();
            _display.dataGridView1.DataSource = DataTable;
            _display.dataGridView1.Columns[0].Visible = false;
            _display.dataGridView1.Columns[1].Visible = false;
            _display.dataGridView1.RowHeadersVisible = false;
            _display.dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.None;
            _display.dataGridView1.ColumnHeadersVisible = false;
            _display.dataGridView1.DefaultCellStyle.SelectionBackColor = _display.dataGridView1.DefaultCellStyle.BackColor;
            _display.dataGridView1.DefaultCellStyle.SelectionForeColor = _display.dataGridView1.DefaultCellStyle.ForeColor;
            _display.dataGridView1.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            new Thread(e =>
            {
                Application.EnableVisualStyles();
                Application.Run(_display);
            }).Start();
            Tick += DoTick;
        }

        public void DoTick(object sender, EventArgs e)
        {
            currentTick++;
            if (currentTick - lastUpdateTime > DISPLAY_UPDATE_INTERVAL_TICKS)
            {
                lock (_tableUpdateLock)
                {
                    var dt = DataTable.Copy();
                    if (DataTable == null) return;
                    bool changed = false;
                    foreach (var d in DeathTracker.Presses.OrderByDescending(d => d.Value))
                    {
                        var r = dt.Rows.Find(d.Key);
                        r ??= dt.Rows.Add(d.Key, 0, "");
                        var str = $"{d.Key} - {d.Value}";
                        if ((int) r[1] != d.Value)
                        {
                            r.BeginEdit();
                            r[1] = d.Value;
                            r[2] = str;
                            r.EndEdit();
                            changed = true;
                            dt.AcceptChanges();
                        }
                    }
                    if (changed) dt.AcceptChanges();
                    _display.Invoke(new Action<DataTable>((DataTable table) => {
                        _display.dataGridView1.DataSource = table;
                    }), dt);
                    lastUpdateTime = currentTick;
                }
            }
        }
    }
    
    public class DeathTracker: Script
    {
        private bool _isInMenu;
        public static int deaths;
        public static ulong lastTimeAltActive = 0;
        public static Dictionary<string, int> Presses = new Dictionary<string, int>();

        public DeathTracker()
        {
            Tick += DeathCountScript_Tick;
            KeyDown += DeathTracker_KeyDown;
            KeyUp += DeathTracker_KeyUp;
        }

        public HashSet<string> ActiveKeys = new HashSet<string>();

        private void DeathTracker_KeyDown(object sender, KeyEventArgs e)
        {
            if (ActiveKeys.Add(e.KeyCode.ToString("G")))
            {
                if (!Presses.ContainsKey(e.KeyCode.ToString("G"))) Presses[e.KeyCode.ToString("G")] = 1;
                else Presses[e.KeyCode.ToString("G")]++;
            }
        }

        private void DeathTracker_KeyUp(object sender, KeyEventArgs e)
        {
            ActiveKeys.Remove(e.KeyCode.ToString("G"));
        }

        private readonly TextElement deathCounter = 
            new TextElement("", new PointF((float) 0.4*GTA.UI.Screen.Width, (float) 0.95*GTA.UI.Screen.Height), 0.8f, Color.White, GTA.UI.Font.ChaletComprimeCologne, Alignment.Left, true, true);

        public void Increment(string key)
        {
            if (!Presses.ContainsKey(key)) Presses[key] = 1;
            else Presses[key]++;
        }
        private void DeathCountScript_Tick(object sender, EventArgs e)
        {
            _isInMenu = Game.IsPaused || Game.IsCutsceneActive || Game.IsLoading;
            if (!_isInMenu)
            {
                if (GTA.Game.IsControlJustPressed(Control.Attack) && ActiveKeys.Add("LClick"))
                {
                    Increment("LClick");
                }
                if (Game.IsControlJustReleased(Control.Attack)) ActiveKeys.Remove("LClick");
                if (Game.IsControlJustPressed(Control.Aim) && ActiveKeys.Add("RClick"))
                {
                    Increment("RClick");
                }
                if (Game.IsControlJustReleased(Control.Aim)) ActiveKeys.Remove("RClick");
                deathCounter.Caption = string.Join(" ", ActiveKeys);
                deathCounter.ScaledDraw();
            }
        }
    }
}
