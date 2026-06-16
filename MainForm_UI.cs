using System;
using System.Drawing;
using System.Windows.Forms;

namespace GymSchedulerFx
{
    public partial class MainForm
    {
        private void BuildUI()
        {
            // ── Header ──────────────────────────────────────────────
            var hdr = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = Color.FromArgb(24, 24, 48) };
            hdr.Controls.Add(new Label {
                Text = "🏋️  健身房教練排班系統",
                Font = new Font("Microsoft JhengHei", 15, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 220, 255),
                AutoSize = true, Location = new Point(14, 8)
            });
            hdr.Controls.Add(new Label {
                Text = "Gym Coach Scheduling & Management System",
                Font = new Font("Microsoft JhengHei", 8),
                ForeColor = Color.FromArgb(110, 125, 180),
                AutoSize = true, Location = new Point(16, 36)
            });

            // ── Status Bar ──────────────────────────────────────────
            lblStatus = new Label {
                Dock = DockStyle.Bottom, Height = 26,
                BackColor = Color.FromArgb(18, 18, 40),
                ForeColor = Color.FromArgb(100, 220, 150),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                Font = new Font("Microsoft JhengHei", 8.5f),
                Text = "✅ 系統就緒"
            };

            // ── Right coach panel (fixed width, docked right) ────────
            var rightPanel = new Panel {
                Dock = DockStyle.Right,
                Width = 320,
                BackColor = Color.FromArgb(20, 20, 42),
                Padding = new Padding(6, 4, 6, 4)
            };

            // ── Left tab area (fills remaining space) ────────────────
            var leftPanel = new Panel {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(18, 18, 30),
                Padding = new Padding(4, 4, 2, 0)
            };

            // 控制項加入的順序決定了 Dock 評估順序 (後加的先 Dock)
            // 順序：1. Fill 2. Right 3. Top 4. Bottom
            Controls.Add(leftPanel);    // 最先加，最後 Dock (被擠在中間)
            Controls.Add(rightPanel);   // 第二加，倒數第二 Dock
            Controls.Add(hdr);          // 第三加，第二個 Dock
            Controls.Add(lblStatus);    // 最後加，最先 Dock (緊貼邊緣)

            BuildLeftTabs(leftPanel);
            BuildRightPanel(rightPanel);
        }

        private void BuildLeftTabs(Panel container)
        {
            tabLeft = new TabControl {
                Dock = DockStyle.Fill,
                Font = new Font("Microsoft JhengHei", 9.5f, FontStyle.Bold),
                DrawMode = TabDrawMode.OwnerDrawFixed,
                ItemSize = new Size(130, 30),
                Padding = new Point(8, 4)
            };
            tabLeft.DrawItem += DrawTab;
            container.Controls.Add(tabLeft);

            BuildTab_Input();
            BuildTab_Week();
            BuildTab_Stats();
            BuildTab_History();
        }

        private void DrawTab(object sender, DrawItemEventArgs e)
        {
            bool sel = e.Index == tabLeft.SelectedIndex;
            var r = tabLeft.GetTabRect(e.Index);
            using (var b = new SolidBrush(sel ? Color.FromArgb(42, 92, 195) : Color.FromArgb(26, 26, 52)))
                e.Graphics.FillRectangle(b, r);
            using (var tb = new SolidBrush(sel ? Color.White : Color.FromArgb(140, 150, 205)))
            {
                var sf = new System.Drawing.StringFormat {
                    Alignment = System.Drawing.StringAlignment.Center,
                    LineAlignment = System.Drawing.StringAlignment.Center
                };
                e.Graphics.DrawString(tabLeft.TabPages[e.Index].Text, tabLeft.Font, tb, r, sf);
            }
        }

        // ── Tab 1: 排班輸入 ─────────────────────────────────────────
        private void BuildTab_Input()
        {
            tabInput = new TabPage("📋 排班輸入") { BackColor = Color.FromArgb(20, 20, 36), Padding = new Padding(8) };
            tabLeft.TabPages.Add(tabInput);

            // 使用 TableLayoutPanel 保證絕不重疊
            var layout = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                ColumnCount = 1, RowCount = 3,
                BackColor = Color.Transparent,
                Margin = new Padding(0)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100)); // 輸入區 (縮小高度)
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));  // 標題
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // 表格
            tabInput.Controls.Add(layout);

            // Input panel
            var pnl = new Panel {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(26, 28, 55),
                Margin = new Padding(0, 0, 0, 8),
                Padding = new Padding(10, 4, 10, 4)
            };
            pnl.Paint += (s, e) => {
                using (var p = new System.Drawing.Pen(Color.FromArgb(50, 70, 145), 1))
                    e.Graphics.DrawRectangle(p, 0, 0, pnl.Width - 1, pnl.Height - 1);
            };

            // Row 1: Labels
            var row1 = new TableLayoutPanel {
                Dock = DockStyle.Top, Height = 22, BackColor = Color.Transparent,
                ColumnCount = 5, RowCount = 1
            };
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 16));
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22));
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 23));
            row1.Controls.Add(new Label(), 0, 0);
            row1.Controls.Add(MkHdrLbl("👤 教練"), 1, 0);
            row1.Controls.Add(MkHdrLbl("📅 日期"), 2, 0);
            row1.Controls.Add(MkHdrLbl("🕐 班別"), 3, 0);
            row1.Controls.Add(new Label(), 4, 0);

            // Row 2: ComboBoxes
            var row2 = new TableLayoutPanel {
                Dock = DockStyle.Top, Height = 32, BackColor = Color.Transparent,
                ColumnCount = 5, RowCount = 1
            };
            row2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 16));
            row2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            row2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22));
            row2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            row2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 23));

            cmbCoach = MkCmbDock();
            foreach (var c in _coaches) cmbCoach.Items.Add(c);
            cmbCoach.SelectedIndex = 0;
            cmbCoach.SelectedIndexChanged += (s, e) => ShowCoachDetail(cmbCoach.SelectedIndex);

            cmbDay = MkCmbDock();
            foreach (var d in new[] { "週一","週二","週三","週四","週五","週六","週日" }) cmbDay.Items.Add(d);
            cmbDay.SelectedIndex = 0;

            cmbShift = MkCmbDock();
            cmbShift.Items.Add("早班  (06:00 - 14:00)");
            cmbShift.Items.Add("晚班  (14:00 - 22:00)");
            cmbShift.SelectedIndex = 0;

            row2.Controls.Add(new Label(), 0, 0);
            row2.Controls.Add(cmbCoach, 1, 0);
            row2.Controls.Add(cmbDay, 2, 0);
            row2.Controls.Add(cmbShift, 3, 0);
            row2.Controls.Add(new Label(), 4, 0);

            var spacer = new Panel { Dock = DockStyle.Top, Height = 4, BackColor = Color.Transparent };

            // Row 3: Buttons
            var row3 = new FlowLayoutPanel {
                Dock = DockStyle.Top, Height = 36, BackColor = Color.Transparent,
                FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = false,
                Padding = new Padding(14, 0, 0, 0)
            };
            btnAdd    = MkBtnFlow("➕ 新增排班",  Color.FromArgb(36, 145, 72));  btnAdd.Click += BtnAdd_Click;
            btnDelete = MkBtnFlow("🗑️ 刪除選取", Color.FromArgb(168, 50, 50));   btnDelete.Click += BtnDelete_Click;
            btnCheck  = MkBtnFlow("🔍 檢查班表",  Color.FromArgb(40, 105, 195)); btnCheck.Click += BtnCheck_Click;
            btnClear  = MkBtnFlow("🧹 清空",      Color.FromArgb(130, 85, 20));  btnClear.Width = 80; btnClear.Click += BtnClear_Click;
            btnSubmit = MkBtnFlow("📤 送出本週班表", Color.FromArgb(145, 38, 158)); btnSubmit.Width = 152; btnSubmit.Click += BtnSubmit_Click;
            row3.Controls.AddRange(new Control[] { btnAdd, btnDelete, btnCheck, btnClear, btnSubmit });

            // WinForms Dock.Top 順序：最後加的會在最上面
            pnl.Controls.Add(row3);
            pnl.Controls.Add(spacer);
            pnl.Controls.Add(row2);
            pnl.Controls.Add(row1);
            
            layout.Controls.Add(pnl, 0, 0);

            // List header
            var lhdr = new Label {
                Dock = DockStyle.Fill,
                Text = "📌 本週排班明細",
                Font = new Font("Microsoft JhengHei", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(172, 182, 225),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.BottomLeft,
                Padding = new Padding(4, 0, 0, 4)
            };
            layout.Controls.Add(lhdr, 0, 1);

            // DGV
            dgvSchedule = new DataGridView { Dock = DockStyle.Fill, Margin = new Padding(0) };
            StyleDgv(dgvSchedule);
            dgvSchedule.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "教練姓名", Width = 110 });
            dgvSchedule.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "日期", Width = 80 });
            dgvSchedule.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "班別時段", Width = 200 });
            layout.Controls.Add(dgvSchedule, 0, 2);
        }

        // ── Tab 2: 週班表 ────────────────────────────────────────────
        private void BuildTab_Week()
        {
            tabSchedule = new TabPage("📊 週班表") { BackColor = Color.FromArgb(20, 20, 36), Padding = new Padding(8) };
            tabLeft.TabPages.Add(tabSchedule);
            tabLeft.Selected += (s, e) => { if (tabLeft.SelectedTab == tabSchedule) RefreshWeekTable(); };
        }

        private void RefreshWeekTable()
        {
            tabSchedule.Controls.Clear();
            var lbl = new Label {
                Dock = DockStyle.Top, Height = 28,
                Text = "📊 本週班表全覽（橫軸＝日期，縱軸＝班別）",
                Font = new Font("Microsoft JhengHei", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(172, 182, 225),
                TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(4, 0, 0, 0)
            };
            tabSchedule.Controls.Add(lbl);

            // Use TableLayoutPanel for responsive grid
            var grid = new TableLayoutPanel {
                Dock = DockStyle.Top, Height = 220,
                ColumnCount = 8, RowCount = 3,
                BackColor = Color.FromArgb(18, 18, 36),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };
            // Columns: 1 label col + 7 day cols
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 75));
            for (int i = 0; i < 7; i++) grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 7));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            string[] dl = { "週一","週二","週三","週四","週五","週六","週日" };

            // Header row
            grid.Controls.Add(MkCell("", Color.FromArgb(16, 16, 38), Color.White, true), 0, 0);
            for (int c = 0; c < 7; c++)
                grid.Controls.Add(MkCell(dl[c], Color.FromArgb(24, 46, 92), Color.FromArgb(100, 205, 255), true), c + 1, 0);

            // Data rows
            for (int r = 0; r < 2; r++)
            {
                var shift = r == 0 ? ShiftType.Morning : ShiftType.Evening;
                string shiftTxt = r == 0 ? "早班\n06-14" : "晚班\n14-22";
                Color shiftFg = r == 0 ? Color.FromArgb(255, 205, 70) : Color.FromArgb(185, 120, 255);
                grid.Controls.Add(MkCell(shiftTxt, Color.FromArgb(20, 20, 46), shiftFg, true), 0, r + 1);

                for (int c = 0; c < 7; c++)
                {
                    var day = _weekDays[c];
                    var lst = _schedule.FindAll(e => e.DayOfWeek == day && e.Shift == shift);
                    string t = lst.Count > 0 ? string.Join("\n", lst.ConvertAll(e => e.CoachName)) : "─";
                    Color bg = lst.Count > 0 ? Color.FromArgb(18, 58, 34) : Color.FromArgb(18, 18, 36);
                    Color fg = lst.Count > 0 ? Color.FromArgb(100, 255, 155) : Color.FromArgb(65, 65, 100);
                    grid.Controls.Add(MkCell(t, bg, fg, false), c + 1, r + 1);
                }
            }
            // WinForms 順序：後加先 Dock (Top 在上)
            tabSchedule.Controls.Add(grid);
            tabSchedule.Controls.Add(lbl);
        }

        private Label MkCell(string text, Color bg, Color fg, bool bold)
        {
            return new Label {
                Text = text, Dock = DockStyle.Fill, Margin = new Padding(0),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = bg, ForeColor = fg,
                Font = new Font("Microsoft JhengHei", 8.5f, bold ? FontStyle.Bold : FontStyle.Regular)
            };
        }

        // ── Tab 3: 統計 ──────────────────────────────────────────────
        private void BuildTab_Stats()
        {
            tabStats = new TabPage("📈 時數統計") { BackColor = Color.FromArgb(20, 20, 36), Padding = new Padding(8) };
            tabLeft.TabPages.Add(tabStats);

            var lhdr = new Label {
                Dock = DockStyle.Top, Height = 28,
                Text = "📈 教練本週工時統計",
                Font = new Font("Microsoft JhengHei", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(172, 182, 225),
                TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(4, 0, 0, 0)
            };
            tabStats.Controls.Add(lhdr);

            dgvStats = new DataGridView { Dock = DockStyle.Fill };
            StyleDgv(dgvStats);
            dgvStats.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "教練姓名" });
            dgvStats.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "排班天數" });
            dgvStats.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "總工時(h)" });
            dgvStats.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "剩餘天數" });
            dgvStats.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "剩餘工時(h)" });
            dgvStats.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "狀態" });
            // 後加先 Dock
            tabStats.Controls.Add(dgvStats);
            tabStats.Controls.Add(lhdr);
            tabLeft.Selected += (s, e) => { if (tabLeft.SelectedTab == tabStats) RefreshStats(); };
        }

        // ── Tab 4: 歷史排班（讀檔）──────────────────────────────────
        private void BuildTab_History()
        {
            tabHistory = new TabPage("📁 歷史排班") { BackColor = Color.FromArgb(20, 20, 36), Padding = new Padding(8) };
            tabLeft.TabPages.Add(tabHistory);

            var topPnl = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Color.Transparent };
            topPnl.Controls.Add(new Label {
                Text = "📁 歷史排班紀錄（讀自 schedules_history.txt）",
                Font = new Font("Microsoft JhengHei", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(172, 182, 225),
                Location = new Point(0, 8), AutoSize = true
            });
            var btnR = MkBtnFlow("🔄 重新讀取", Color.FromArgb(35, 100, 170));
            btnR.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            btnR.Location = new Point(topPnl.Width - 145, 2);
            btnR.Click += (s, e) => LoadHistory();
            topPnl.Controls.Add(btnR);
            tabHistory.Controls.Add(topPnl);

            rtbHistory = new RichTextBox {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(14, 14, 30),
                ForeColor = Color.FromArgb(200, 210, 255),
                Font = new Font("Consolas", 9.5f),
                ReadOnly = true, BorderStyle = BorderStyle.None
            };
            // 後加先 Dock
            tabHistory.Controls.Add(rtbHistory);
            tabHistory.Controls.Add(topPnl);
            tabLeft.Selected += (s, e) => { if (tabLeft.SelectedTab == tabHistory) LoadHistory(); };
        }

        // ── 右側教練資訊欄 ─────────────────────────────────────────
        private void BuildRightPanel(Panel right)
        {

            // Use a single DockFill panel with TableLayoutPanel
            var tbl = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                ColumnCount = 1, RowCount = 5,
                BackColor = Color.Transparent
            };
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 220)); // coach list (極大化)
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 145)); // photo (縮小)
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));  // name+role
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));  // avail title
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // avail grid
            right.Controls.Add(tbl);

            // Row 0: Coach list
            var listPnl = new Panel { Dock = DockStyle.Fill };
            var lblTitle = new Label {
                Text = "👥 教練列表",
                Font = new Font("Microsoft JhengHei", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(160, 175, 225),
                Dock = DockStyle.Top, Height = 20, TextAlign = ContentAlignment.MiddleLeft
            };
            lbCoaches = new ListBox {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(26, 26, 52),
                ForeColor = Color.White,
                Font = new Font("Microsoft JhengHei", 10, FontStyle.Bold),
                BorderStyle = BorderStyle.FixedSingle,
                IntegralHeight = false // 強制填滿高度
            };
            foreach (var c in _coaches) lbCoaches.Items.Add(c);
            lbCoaches.SelectedIndex = 0;
            lbCoaches.SelectedIndexChanged += (s, e) => {
                if (lbCoaches.SelectedIndex >= 0) {
                    _selectedCoachIdx = lbCoaches.SelectedIndex;
                    ShowCoachDetail(lbCoaches.SelectedIndex);
                    cmbCoach.SelectedIndex = lbCoaches.SelectedIndex;
                }
            };
            // Fill 必須先加，Top 後加才會停留在頂部
            listPnl.Controls.Add(lbCoaches);
            listPnl.Controls.Add(lblTitle);
            tbl.Controls.Add(listPnl, 0, 0);

            // Row 1: Photo
            pbCoach = new PictureBox {
                Dock = DockStyle.Fill, Margin = new Padding(0, 4, 0, 4),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(30, 30, 60),
                BorderStyle = BorderStyle.FixedSingle
            };
            tbl.Controls.Add(pbCoach, 0, 1);

            // Row 2: Name + Role
            var namePnl = new Panel { Dock = DockStyle.Fill };
            lblCoachName = new Label {
                Dock = DockStyle.Top, Height = 28,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Microsoft JhengHei", 13, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 220, 255),
                BackColor = Color.Transparent
            };
            lblCoachRole = new Label {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Microsoft JhengHei", 8.5f),
                ForeColor = Color.FromArgb(150, 160, 215),
                BackColor = Color.Transparent
            };
            namePnl.Controls.Add(lblCoachRole);
            namePnl.Controls.Add(lblCoachName);
            tbl.Controls.Add(namePnl, 0, 2);

            // Row 3: Avail title
            lblAvailTitle = new Label {
                Dock = DockStyle.Fill,
                Text = "📅 可排班時段（🟢可  🔴不可）",
                Font = new Font("Microsoft JhengHei", 8, FontStyle.Bold),
                ForeColor = Color.FromArgb(160, 175, 225),
                TextAlign = ContentAlignment.MiddleLeft
            };
            tbl.Controls.Add(lblAvailTitle, 0, 3);

            // Row 4: Avail grid (TableLayoutPanel)
            pnlAvailGrid = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(20, 20, 42) };
            tbl.Controls.Add(pnlAvailGrid, 0, 4);
        }

        // ── Helpers ─────────────────────────────────────────────────
        private static Label MkHdrLbl(string text)
        {
            return new Label {
                Text = text, Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(172, 182, 225),
                Font = new Font("Microsoft JhengHei", 8.5f, FontStyle.Bold),
                TextAlign = ContentAlignment.BottomLeft
            };
        }

        private static ComboBox MkCmbDock()
        {
            return new ComboBox {
                Dock = DockStyle.Fill, Margin = new Padding(0, 0, 6, 0),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(34, 34, 58),
                ForeColor = Color.White,
                Font = new Font("Microsoft JhengHei", 9.5f),
                FlatStyle = FlatStyle.Flat
            };
        }

        private static Button MkBtnFlow(string text, Color bg)
        {
            var b = new Button {
                Text = text, Width = 128, Height = 34,
                Margin = new Padding(0, 0, 6, 0),
                BackColor = bg, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft JhengHei", 8.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        internal static void StyleDgv(DataGridView d)
        {
            d.BackgroundColor = Color.FromArgb(14, 14, 30);
            d.GridColor = Color.FromArgb(42, 48, 85);
            d.DefaultCellStyle.BackColor = Color.FromArgb(22, 22, 44);
            d.DefaultCellStyle.ForeColor = Color.FromArgb(210, 218, 255);
            d.DefaultCellStyle.Font = new Font("Microsoft JhengHei", 9.5f);
            d.DefaultCellStyle.SelectionBackColor = Color.FromArgb(42, 88, 188);
            d.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(28, 52, 115);
            d.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(100, 200, 255);
            d.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft JhengHei", 9.5f, FontStyle.Bold);
            d.EnableHeadersVisualStyles = false;
            d.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            d.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            d.ReadOnly = true;
            d.AllowUserToAddRows = false;
            d.RowHeadersVisible = false;
            d.BorderStyle = BorderStyle.None;
        }
    }
}
