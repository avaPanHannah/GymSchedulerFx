using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace GymSchedulerFx
{
    public partial class MainForm
    {
        // ════════════════════════════════════════
        //  教練詳細資訊 + 可排班時段
        // ════════════════════════════════════════
        private void ShowCoachDetail(int idx)
        {
            _selectedCoachIdx = idx;
            lblCoachName.Text = _coaches[idx];
            lblCoachRole.Text = _coachRoles[idx];

            // 載入照片：D:\ 原始路徑 → Resources\ 備份
            pbCoach.Image = null;
            string imgPath = _coachImgSrc[idx];
            if (!File.Exists(imgPath))
                imgPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", _coachImgFiles[idx]);
            if (File.Exists(imgPath))
            {
                try
                {
                    byte[] data = File.ReadAllBytes(imgPath);
                    var ms = new MemoryStream(data);
                    pbCoach.Image = new Bitmap(ms);
                }
                catch { }
            }

            RefreshAvailGrid(idx);
        }

        private void RefreshAvailGrid(int coachIdx)
        {
            pnlAvailGrid.Controls.Clear();
            string coach = _coaches[coachIdx];
            string[] dl = { "週一","週二","週三","週四","週五","週六","週日" };

            var grid = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                ColumnCount = 8, RowCount = 3,
                BackColor = Color.FromArgb(20, 20, 42),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
                Margin = new Padding(0)
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42));
            for (int i = 0; i < 7; i++) grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 7));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 33f));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 33f));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 34f));

            // Corner
            grid.Controls.Add(MkGridLbl("", Color.FromArgb(16,16,40), Color.White, false), 0, 0);
            // Day headers
            for (int c = 0; c < 7; c++)
                grid.Controls.Add(MkGridLbl(dl[c], Color.FromArgb(22,44,90), Color.FromArgb(100,205,255), true), c+1, 0);

            string[] shiftNames = { "早班", "晚班" };
            Color[] shiftFgs = { Color.FromArgb(255,200,60), Color.FromArgb(180,115,255) };
            for (int r = 0; r < 2; r++)
            {
                var shift = r == 0 ? ShiftType.Morning : ShiftType.Evening;
                grid.Controls.Add(MkGridLbl(shiftNames[r], Color.FromArgb(20,20,46), shiftFgs[r], true), 0, r+1);
                for (int c = 0; c < 7; c++)
                {
                    var day = _weekDays[c];
                    bool unavail = IsUnavailable(coach, day, shift);
                    Color bg = unavail ? Color.FromArgb(90,25,25) : Color.FromArgb(18,68,34);
                    Color fg = unavail ? Color.FromArgb(255,100,100) : Color.FromArgb(100,230,130);
                    string tip = unavail ? GetUnavailReason(coach, day, shift) : "可排班";
                    var cell = MkGridLbl(unavail?"✗":"✓", bg, fg, true);
                    cell.Cursor = Cursors.Help;
                    new ToolTip().SetToolTip(cell, tip);
                    grid.Controls.Add(cell, c+1, r+1);
                }
            }
            pnlAvailGrid.Controls.Add(grid);
        }

        private static Label MkGridLbl(string text, Color bg, Color fg, bool bold)
        {
            return new Label {
                Text = text, Dock = DockStyle.Fill, Margin = new Padding(0),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = bg, ForeColor = fg,
                Font = new Font("Microsoft JhengHei", 8, bold ? FontStyle.Bold : FontStyle.Regular)
            };
        }

        private bool IsUnavailable(string coach, DayOfWeek day, ShiftType shift)
        {
            var r = ScheduleRules.ValidateAddEntry(_schedule, coach, day, shift);
            return !r.IsSuccess;
        }

        private string GetUnavailReason(string coach, DayOfWeek day, ShiftType shift)
        {
            var r = ScheduleRules.ValidateAddEntry(_schedule, coach, day, shift);
            return r.Message.Replace("❌ ","").Replace("⚠️ ","");
        }

        // ════════════════════════════════════════
        //  Button Handlers
        // ════════════════════════════════════════
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            string coach = cmbCoach.SelectedItem.ToString();
            var day   = _weekDays[cmbDay.SelectedIndex];
            var shift = cmbShift.SelectedIndex == 0 ? ShiftType.Morning : ShiftType.Evening;

            if (IsUnavailable(coach, day, shift))
            {
                string reason = GetUnavailReason(coach, day, shift);
                SetStatus("❌ " + reason, Color.FromArgb(255,100,100));
                MessageBox.Show(reason, "無法排班", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            _schedule.Add(new ScheduleEntry { CoachName=coach, DayOfWeek=day, Shift=shift });
            SaveXml();
            RefreshAll();
            SetStatus("✅ 已新增："+coach+" "+ScheduleRules.GetDayName(day)+(shift==ShiftType.Morning?" 早班":" 晚班"), Color.FromArgb(100,220,150));
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvSchedule.SelectedRows.Count == 0) { SetStatus("⚠️ 請先選取要刪除的列", Color.Orange); return; }
            int idx = dgvSchedule.SelectedRows[0].Index;
            var sorted = SortedSchedule();
            if (idx < sorted.Count) { _schedule.Remove(sorted[idx]); SaveXml(); RefreshAll(); SetStatus("🗑️ 已刪除", Color.FromArgb(255,180,80)); }
        }

        private void BtnCheck_Click(object sender, EventArgs e)
        {
            var r = ScheduleRules.CheckMinStaff(_schedule);
            if (r.IsAllGood) {
                MessageBox.Show("✅ 班表合規！所有班次均有人值班。", "檢查通過", MessageBoxButtons.OK, MessageBoxIcon.Information);
                SetStatus("✅ 班表合規", Color.FromArgb(100,220,150));
            } else {
                MessageBox.Show("⚠️ 下列班次無人值班：\n\n"+string.Join("\n",r.Violations), "人力不足", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                SetStatus("⚠️ 發現 "+r.Violations.Count+" 個空班次", Color.FromArgb(255,150,50));
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("確定要清空本週排班？","確認",MessageBoxButtons.YesNo,MessageBoxIcon.Question)==DialogResult.Yes)
            { _schedule.Clear(); SaveXml(); RefreshAll(); SetStatus("🧹 班表已清空", Color.FromArgb(255,180,80)); }
        }

        // ════════════════════════════════════════
        //  送出班表 → 寫入 TXT（每週累加）
        // ════════════════════════════════════════
        private void BtnSubmit_Click(object sender, EventArgs e)
        {
            var check = ScheduleRules.CheckMinStaff(_schedule);
            if (!check.IsAllGood)
            {
                var ans = MessageBox.Show("⚠️ 班表仍有空缺班次：\n\n"+string.Join("\n",check.Violations)+"\n\n仍要送出？", "班表不完整", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (ans != DialogResult.Yes) return;
            }
            SubmitToHistory();
        }

        private void SubmitToHistory()
        {
            try
            {
                string weekLabel = "第 " + GetWeekLabel() + " 週排班紀錄（" + DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "）";
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("╔══════════════════════════════════════════╗");
                sb.AppendLine("║  " + weekLabel);
                sb.AppendLine("╠══════════════════════════════════════════╣");

                string[] dayNames = {"週一","週二","週三","週四","週五","週六","週日"};
                foreach (var day in _weekDays)
                {
                    var morning = _schedule.FindAll(x=>x.DayOfWeek==day&&x.Shift==ShiftType.Morning);
                    var evening = _schedule.FindAll(x=>x.DayOfWeek==day&&x.Shift==ShiftType.Evening);
                    string dn = ScheduleRules.GetDayName(day);
                    sb.AppendLine("║  "+dn+" 早班：" + (morning.Count>0 ? string.Join(", ",morning.ConvertAll(x=>x.CoachName)) : "（無人）"));
                    sb.AppendLine("║  "+dn+" 晚班：" + (evening.Count>0 ? string.Join(", ",evening.ConvertAll(x=>x.CoachName)) : "（無人）"));
                }

                sb.AppendLine("╠══════════════════════════════════════════╣");
                sb.AppendLine("║  教練工時統計：");
                foreach (var c in _coaches)
                    sb.AppendLine("║    "+c+"："+ScheduleRules.GetDays(_schedule,c)+"天 / "+ScheduleRules.GetHours(_schedule,c)+" 小時");
                sb.AppendLine("╚══════════════════════════════════════════╝");
                sb.AppendLine();

                File.AppendAllText(_historyFile, sb.ToString(), System.Text.Encoding.UTF8);
                SetStatus("📤 班表已送出並寫入 schedules_history.txt", Color.FromArgb(180,100,255));
                MessageBox.Show("✅ 班表已送出！\n\n已寫入："+_historyFile, "送出成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                SetStatus("❌ 送出失敗："+ex.Message, Color.Red);
            }
        }

        private string GetWeekLabel()
        {
            // 計算今年第幾週
            var cal = System.Globalization.CultureInfo.InvariantCulture.Calendar;
            int wk = cal.GetWeekOfYear(DateTime.Today, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
            return wk.ToString("D2");
        }

        // ════════════════════════════════════════
        //  讀取歷史 TXT
        // ════════════════════════════════════════
        private void LoadHistory()
        {
            if (!File.Exists(_historyFile))
            { rtbHistory.Text = "（尚無歷史紀錄，請先送出一週班表）\n\n檔案位置："+_historyFile; return; }
            try
            {
                using (var fs = new FileStream(_historyFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fs, System.Text.Encoding.UTF8))
                {
                    rtbHistory.Text = sr.ReadToEnd();
                }
                
                // 移至最下方以顯示最新紀錄
                rtbHistory.SelectionStart = rtbHistory.Text.Length;
                rtbHistory.ScrollToCaret();
                
                SetStatus("📁 已讀取歷史紀錄："+_historyFile, Color.FromArgb(100,200,255));
            }
            catch (Exception ex) { rtbHistory.Text = "讀檔失敗："+ex.Message; }
        }

        // ════════════════════════════════════════
        //  XML 讀寫（暫存當週）
        // ════════════════════════════════════════
        private void SaveXml()
        {
            try { var xs=new XmlSerializer(typeof(List<ScheduleEntry>)); using(var sw=new StreamWriter(_xmlFile,false,System.Text.Encoding.UTF8)) xs.Serialize(sw,_schedule); }
            catch { }
        }

        private void LoadXml()
        {
            try
            {
                if (!File.Exists(_xmlFile)) return;
                var xs=new XmlSerializer(typeof(List<ScheduleEntry>));
                using(var sr=new StreamReader(_xmlFile,System.Text.Encoding.UTF8))
                    _schedule=(List<ScheduleEntry>)xs.Deserialize(sr);
                SetStatus("✅ 已載入 "+_schedule.Count+" 筆排班", Color.FromArgb(100,220,150));
            }
            catch { }
        }

        // ════════════════════════════════════════
        //  Refresh
        // ════════════════════════════════════════
        private void RefreshAll()
        {
            dgvSchedule.Rows.Clear();
            foreach (var en in SortedSchedule())
                dgvSchedule.Rows.Add(en.CoachName, en.DayDisplayName, en.ShiftDisplayName);
            RefreshStats();
            RefreshAvailGrid(_selectedCoachIdx);
            if (pbCoach.Image == null) ShowCoachDetail(_selectedCoachIdx);
        }

        private void RefreshStats()
        {
            dgvStats.Rows.Clear();
            foreach (var c in _coaches)
            {
                int d=ScheduleRules.GetDays(_schedule,c), h=ScheduleRules.GetHours(_schedule,c);
                dgvStats.Rows.Add(c,d,h,5-d,40-h,d>=5?"⚠️ 已達上限":d>=3?"🟡 適中":"🟢 充裕");
            }
        }

        private List<ScheduleEntry> SortedSchedule()
        {
            var s = new List<ScheduleEntry>(_schedule);
            s.Sort((a,b)=>{ int d=Array.IndexOf(_weekDays,a.DayOfWeek).CompareTo(Array.IndexOf(_weekDays,b.DayOfWeek)); return d!=0?d:a.Shift.CompareTo(b.Shift); });
            return s;
        }

        private void SetStatus(string msg, Color c) { lblStatus.Text=msg; lblStatus.ForeColor=c; }
    }
}
