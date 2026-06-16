using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace GymSchedulerFx
{
    public partial class MainForm : Form
    {
        private List<ScheduleEntry> _schedule = new List<ScheduleEntry>();
        private readonly string[] _coaches = { "Andy", "Ava", "Charlie", "Hannah", "Elvis" };
        private readonly string[] _coachRoles = { "重訓教練", "瑜珈教練", "有氧教練", "皮拉提斯教練", "拳擊教練" };
        private readonly string[] _coachImgFiles = { "coach_andy.png","coach_ava.png","coach_charlie.png","coach_hannah.png","coach_elvis.png" };
        private readonly string[] _coachImgSrc = {
            @"D:\螢幕擷取畫面 2026-06-12 211642.png",
            @"D:\螢幕擷取畫面 2026-06-12 211709.png",
            @"D:\螢幕擷取畫面 2026-06-12 211735.png",
            @"D:\螢幕擷取畫面 2026-06-12 211801.png",
            @"D:\螢幕擷取畫面 2026-06-12 211814.png"
        };
        private readonly DayOfWeek[] _weekDays = {
            DayOfWeek.Monday,DayOfWeek.Tuesday,DayOfWeek.Wednesday,
            DayOfWeek.Thursday,DayOfWeek.Friday,DayOfWeek.Saturday,DayOfWeek.Sunday
        };
        private readonly string _historyFile;
        private readonly string _xmlFile;
        private int _selectedCoachIdx = 0;

        // UI refs
        private TabControl tabLeft;
        private TabPage tabInput, tabWeek, tabStats, tabHistory;
        private ComboBox cmbCoach, cmbDay, cmbShift;
        private Button btnAdd, btnDelete, btnCheck, btnSubmit, btnClear;
        private DataGridView dgvSchedule, dgvStats;
        private RichTextBox rtbHistory;
        private Label lblStatus;
        // Right panel
        private PictureBox pbCoach;
        private Label lblCoachName, lblCoachRole, lblAvailTitle;
        private Panel pnlAvailGrid;
        private ListBox lbCoaches;
        private TabPage tabSchedule;

        public MainForm()
        {
            _historyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "schedules_history.txt");
            _xmlFile     = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "schedule.xml");
            InitializeComponent();
            Text = "🏋️ 健身房教練排班系統";
            Size = new Size(1400, 860);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(18, 18, 30);
            Font = new Font("Microsoft JhengHei", 9.5f);
            MinimumSize = new Size(1200, 720);
            BuildUI();
            LoadXml();
            RefreshAll();
            ShowCoachDetail(0);
        }
    }
}
