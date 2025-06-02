using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AutoClickerApp
{
    public partial class Form1 : Form
    {
        
        private List<Button> simulatedKeys = new List<Button>();    // создаем список для генерации клавиш
        private List<string> selectedKeys = new List<string>();     // список для хранения выбранных пользователем клавиш
        private List<KeyBehavior> currentBehaviors = new List<KeyBehavior>();


        private bool behaviorPageInitialized = false;

        private Panel bottomButtonsPanel = null;

        private bool autoclickerRunning = false;

        private List<KeyLayout> keyboardLayout = new List<KeyLayout>
        

        /*
         keyboardLayout это список строк клавиатуры.
        Каждая строка - массив клавиш + остступ слева, чтобы строки не начинались просто друг под другом
         */

        
        {
            new KeyLayout(new[] { "`", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "-", "=", "Backspace" }, xOffset: 0),
            new KeyLayout(new[] { "Tab", "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P", "[", "]", "\\" }, xOffset: 10),
            new KeyLayout(new[] { "Caps", "A", "S", "D", "F", "G", "H", "J", "K", "L", ";", "'", "Enter" }, xOffset: 20),
            new KeyLayout(new[] { "Shift", "Z", "X", "C", "V", "B", "N", "M", ",", ".", "/", "RShift" }, xOffset: 30),
            new KeyLayout(new[] { "Ctrl", "Win", "Alt", "Space", "AltGr", "Fn", "Del" }, xOffset: 0),
        };

        private Dictionary<string, int> specialWidths = new Dictionary<string, int>()   // Таблица, где указаны нестандартные размеры кнопок (спецклавиши)
        {
            {"Tab", 60},
            {"Caps", 70},
            {"Shift", 90},
            {"RShift", 90},
            {"BackSpace", 80},
            {"Enter", 80},
            {"Space", 300},
            {"Ctrl", 60},
            {"Win", 60},
            {"Alt", 60},
            {"AltGr", 60},
            {"Fn", 60},
            {"Del", 60}
        };

        public Form1()
        {
            InitializeComponent();
            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;
        }

        /// <summary>
        /// Метод, который запускает автокликер по нажатии F6
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F6)
            {
                ToggleAutoclicker();
            }
        }

        /// <summary>
        /// Метод, описывающий разное поведение автокликера в зависимости от:
        /// запущен кликер или нет;
        /// способ кликания (одиночный, цикл или удержание)
        /// </summary>
        private void ToggleAutoclicker()
        {
            UpdateCurrentBehaviors();

            if (autoclickerRunning)
            {
                autoclickerRunning = false;
                //MessageBox.Show("Автокликер остановлен");
                return;
            }

            autoclickerRunning = true;
            //MessageBox.Show("Автокликер запущен");

            foreach(var behavior in currentBehaviors)
            {
                string key = behavior.KeyName;
                string mode = behavior.Mode;
                int interval = behavior.IntervalMs;

                if (mode == "Одиночное")
                {
                    SimulateKeyPress(key);
                }
                else if (mode == "Цикл")
                {
                    Task.Run(() =>
                    {
                        while (autoclickerRunning)
                        {
                            SimulateKeyPress(key);
                            Thread.Sleep(interval);
                        }
                    });
                }
                else if (mode == "Удержание")
                {
                    HoldKeyDown(key);
                }
            }
        }

        /// <summary>
        /// Метод, симулирующий один клик клавиши
        /// </summary>
        /// <param name="key"></param>
        private void SimulateKeyPress(string key)
        {
            if (TryGetVirtualKey(key, out ushort vk))
            {
                SendKeyDown(vk);
                SendKeyUp(vk);
            }
        }

        /// <summary>
        /// Метод, симулирующий удержание клавиши
        /// </summary>
        /// <param name="key"></param>
        private void HoldKeyDown(string key)
        {
            if (TryGetVirtualKey(key, out ushort vk))
            {
                SendKeyDown(vk);
            }
        }

        private void SendKeyDown(ushort vk)
        {
            var input = new NativeMethods.INPUT
            {
                type = NativeMethods.INPUT_KEYBOARD,
                u = new NativeMethods.InputUnion
                {
                    ki = new NativeMethods.KEYBDINPUT
                    {
                        wVk = vk,
                        dwFlags = 0
                    }
                }
            };

            NativeMethods.SendInput(1, new[] {input}, Marshal.SizeOf(typeof(NativeMethods.INPUT)));
        }

        private void SendKeyUp(ushort vk)
        {
            var input = new NativeMethods.INPUT
            {
                type = NativeMethods.INPUT_KEYBOARD,
                u = new NativeMethods.InputUnion
                {
                    ki = new NativeMethods.KEYBDINPUT
                    {
                        wVk = vk,
                        dwFlags = NativeMethods.KEYEVENTF_KEYUP
                    }
                }
            };

            NativeMethods.SendInput(1, new[] {input}, Marshal.SizeOf(typeof(NativeMethods.INPUT)));
        }

        /// <summary>
        /// Метод для получения нажатой клавиши:
        /// обычные клавиши обабатываются в Enum.TryPase...
        /// специальные клавиши обрабатываются в switch-case
        /// </summary>
        /// <param name="key"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        private bool TryGetVirtualKey(string key, out ushort vk)
        {
            vk = 0;
            key = key.ToUpper();

            Dictionary<string, ushort> mapping = new Dictionary<string, ushort>
            {
                ["ENTER"] = 0x0D,
                ["TAB"] = 0x09,
                ["SPACE"] = 0x20,
                ["SHIFT"] = 0x10,
                ["CTRL"] = 0x11,
                ["ALT"] = 0x12,
                ["BACKSPACE"] = 0x08,
                ["CAPS"] = 0x14,
                ["ESC"] = 0x1B,
                ["LMB"] = 0x01, // Mouse — не будем использовать в SendInput клавиатурой
                ["RMB"] = 0x02,
                ["MMB"] = 0x04,
            };

            if (mapping.ContainsKey(key))
            {
                vk = mapping[key];
                return true;
            }

            if (key.Length == 1)
            {
                vk = (ushort)key[0]; //сиволы от A-Z, 0-9
                return true;
            }

            return false;
        }

        private void UpdateCurrentBehaviors()
        {
            currentBehaviors.Clear();

            foreach (Control control in flpKeySettings.Controls)
            {
                if (control is Panel card)
                {
                    string key = card.Controls.OfType<Button>().First().Text;
                    string mode = card.Controls.OfType<ComboBox>().First().SelectedItem.ToString();
                    int interval = 0;

                    var numeric = card.Controls.OfType<NumericUpDown>().FirstOrDefault();
                    if (mode == "Цикл" && numeric != null)
                    {
                        interval = (int)numeric.Value;
                    }

                    currentBehaviors.Add(new KeyBehavior
                    {
                        KeyName = key,
                        Mode = mode,
                        IntervalMs = interval,
                    });
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
            {
                ToggleAutoclicker();
            }
            base.WndProc(ref m);
        }

        /// <summary>
        /// Вспомогательный метод для создания небольшой инструкции сверху в главной страничке
        /// </summary>
        private void GenerateInstruction()
        {
            int startX = 20;
            int startY = 20;
            Label instructionLabel = new Label();
            instructionLabel.Text = "Pick one or more keys you want to simulate";
            instructionLabel.Location = new Point(startX, startY);
            instructionLabel.Size = new Size(800, 30);
            instructionLabel.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            instructionLabel.ForeColor = Color.White;
            tabPage1.Controls.Add(instructionLabel);
        }

        /// <summary>
        /// Метод, создающий нативно понятную адаптивную разметку как для мыши, так и для клавиатуры
        /// Для достижения адаптива все помещается в FlowLayoutPanel, которой задаются необходимые параметры вроде
        /// AutoSize, Padding, Margin, WrapContents и прочие. Для достижения размещения горизонтально,
        /// блоки мышки и клавиатуры были помещены в один контейнер
        /// </summary>
        private void GenerateKeyboardAndMouse()
        {
            FlowLayoutPanel horizontalPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                WrapContents = false,
                Location = new Point(0, 150)
            };

            // === БЛОК КЛАВИАТУРЫ ===
            GroupBox grpKeyboard = new GroupBox
            {
                Text = "Keyboard",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = this.BackColor,
                
                MinimumSize = new Size(725, 325),
                Padding = new Padding(100)
            };

            FlowLayoutPanel keyboardPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Size = new Size(700, 280),
                Location = new Point(10, 30),
                Margin = new Padding(10),
                BackColor = this.BackColor
            };

            foreach (var rowData in keyboardLayout)
            {
                FlowLayoutPanel rowPanel = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.LeftToRight,
                    Margin = new Padding(rowData.XOffset, 5, 0, 0),
                    Location = new Point(0, 0),
                    Width = 700,
                    Height = 50,
                };

                foreach (var key in rowData.Keys)
                {
                    Button btn = new Button
                    {
                        Text = key,
                        Name = "btn" + key,
                        Height = 40,
                        Width = specialWidths.ContainsKey(key) ? specialWidths[key] : 40,
                        BackColor = Color.White,
                        ForeColor = Color.Black,
                        Font = new Font("Segoe UI", 10, FontStyle.Regular),
                        FlatStyle = FlatStyle.Flat
                    };
                    btn.FlatAppearance.BorderSize = 1;
                    btn.FlatAppearance.BorderColor = Color.Gray;
                    btn.Click += KeyButton_Click;

                    rowPanel.Controls.Add(btn);
                    simulatedKeys.Add(btn);
                }

                keyboardPanel.Controls.Add(rowPanel);
            }

            grpKeyboard.Controls.Add(keyboardPanel);
            horizontalPanel.Controls.Add(grpKeyboard);

            // === БЛОК МЫШКИ ===
            GroupBox grpMouse = new GroupBox
            {
                Text = "Mouse",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = this.BackColor,
                Size = new Size(230, 200),
                Margin = Padding.Empty
            };

            Panel mousePanel = new Panel
            {
                Size = new Size(220, 160),
                Location = new Point(5, 30),
                BackColor = this.BackColor,
            };

            string[] topMouse = { "LMB", "MMB", "RMB" };
            FlowLayoutPanel topRow = new FlowLayoutPanel
            {
                Width = 200,
                Height = 75,
                Location = new Point(5, 30),
                BackColor = this.BackColor,
            };
            foreach (var key in topMouse) topRow.Controls.Add(CreateMouseButton(key));

            string[] bottomBar = { "X1", "X2" };
            FlowLayoutPanel bottomRow = new FlowLayoutPanel
            {
                Width = 200,
                Height = 75,
                Location = new Point(5, 100),
                BackColor = this.BackColor
            };
            foreach(var key in bottomBar) bottomRow.Controls.Add(CreateMouseButton(key));

            mousePanel.Controls.Add(topRow);
            mousePanel.Controls.Add(bottomRow);
            grpMouse.Controls.Add(mousePanel);

            horizontalPanel.Controls.Add(grpMouse);

            // === Добавляем весь блок ===
            tabPage1.Controls.Add(horizontalPanel);
        }



        private Button CreateMouseButton(string key)
        {
            Button btn = new Button();
            btn.Text = key;
            btn.Name = "btn" + key;
            btn.Size = new Size(60, 60);
            btn.BackColor = Color.White;
            btn.ForeColor = Color.Black;
            btn.Click += KeyButton_Click;

            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Color.Gray;
            btn.Font = new Font("Segoe UI", 9, FontStyle.Regular);

            simulatedKeys.Add(btn);
            return btn;
        }

        /// <summary>
        /// Объединенный метод для создания кнопок "Сбросить" и "Применить"
        /// </summary>
        private void GenerateApplyAndResetButtons()
        {
            if (bottomButtonsPanel != null)
            {
                return;
            }


            bottomButtonsPanel = new Panel
            {
                Size = new Size(700, 60),
                Dock = DockStyle.Bottom,
                Name = "BottomButtonsPanel"
            };

            Button applyButton = new Button
            {
                Text = "Apply",
                Size = new Size(120, 45),
                Location = new Point(10, 5),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                ForeColor = Color.Black,
                BackColor = Color.White
            };
            applyButton.Click += (s, e) =>
            {
                currentBehaviors.Clear();

                foreach (Control control in flpKeySettings.Controls)
                {
                    if (control is Panel card)
                    {
                        string key = card.Controls.OfType<Button>().First().Text;
                        string mode = card.Controls.OfType<ComboBox>().First().SelectedItem.ToString();
                        int interval = 0;

                        var numeric = card.Controls.OfType<NumericUpDown>().FirstOrDefault();
                        if (mode == "Цикл" && numeric != null)
                        {
                            interval = (int)numeric.Value;
                        }
                        currentBehaviors.Add(new KeyBehavior
                        {
                            KeyName = key,
                            Mode = mode,
                            IntervalMs = interval,
                        });
                    }
                }
            };
            Button resetButton = new Button
            {
                Text = "Reset all",
                Size = new Size(120, 45),
                Location = new Point(570, 5),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                ForeColor = Color.Black,
                BackColor = Color.White
            };
            resetButton.Click += (s, e) =>
            {
                flpKeySettings.Controls.Clear();

                foreach (var btn in simulatedKeys)
                {
                    btn.BackColor = Color.White;
                }

                selectedKeys.Clear();

                behaviorPageInitialized = false;
            };

            bottomButtonsPanel.Controls.Add(applyButton);
            bottomButtonsPanel.Controls.Add(resetButton);
            tabPage2.Controls.Add(bottomButtonsPanel);
        }


        /// <summary>
        /// метод-обработчик, запускается когда форма создается и начинает загружаться, но еще не видна пользователю
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            GenerateKeyboardAndMouse();
            GenerateInstruction();

            RegisterHotKey(this.Handle, HOTKEY_ID, MOD_NONE, (uint)Keys.F6);
        }

        /// <summary>
        /// Метод-обработчик для нажатий кнопок. Отлавливает клавиши как sender и приводит их к Button
        /// </summary>
        /// <param name="sender">Тот, кто "отослал" сигнал, конкретная кнопка или клавиша</param>
        /// <param name="e"></param>
        private void KeyButton_Click(object sender, EventArgs e)
        {
            Button clickedButton = sender as Button;

            if (clickedButton != null)
            {
                string key = clickedButton.Text;

                if (selectedKeys.Contains(key))
                {
                    selectedKeys.Remove(key);
                    clickedButton.BackColor = Color.White;
                }
                else
                {
                    selectedKeys.Add(key);
                    clickedButton.BackColor = ColorTranslator.FromHtml("#76F076");
                }
            }
        }

        /// <summary>
        /// Метод для создания карточек для каждой клавиши с индивидуальынми настройками
        /// Имеется логика удаления кнопок из списка
        /// Также имеется логика появления интервала только при выбранном режиме "интервал"
        /// </summary>
        /// <param name="keyName"></param>
        private void CreateKeySettingCard(string keyName)
        {
            Panel card = new Panel();
            card.Width = 650;
            card.Height = 60;
            card.Margin = new Padding(10);
            card.BackColor = Color.LightGray;

            Button keyButton = new Button();
            keyButton.Text = keyName;
            keyButton.Enabled = false;
            keyButton.BackColor = Color.White;
            keyButton.ForeColor = Color.Black;
            keyButton.Width = 60;
            keyButton.Height = 40;
            keyButton.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            keyButton.Location = new Point(10, 10);

            //Комбо-бокс с выбором поведения для каждой клавиши
            ComboBox behaviorBox = new ComboBox();
            behaviorBox.Items.AddRange(new[] { "Одиночное", "Цикл", "Удержание" });
            behaviorBox.SelectedIndex = 0;
            behaviorBox.DropDownStyle = ComboBoxStyle.DropDownList;
            behaviorBox.Location = new Point(90, 18);
            behaviorBox.Width = 120;

            //Поле для интервала
            NumericUpDown intervalBox = new NumericUpDown();
            intervalBox.Minimum = 1;
            intervalBox.Maximum = 10000;
            intervalBox.Value = 1000;
            intervalBox.Location = new Point(220, 18);
            intervalBox.Width = 80;
            intervalBox.Visible = false; //скрыт по умолчанию

            // подпись "мс"
            Label msLabel = new Label();
            msLabel.Text = "мс";
            msLabel.Location = new Point(305, 21);
            msLabel.Width = 30;
            msLabel.Visible = false;

            //Кнопка для удаления клавиши из списка
            Button removeButton = new Button();
            removeButton.Text = "Удалить";
            removeButton.Location = new Point(500, 6);
            removeButton.Size = new Size(90, 45);
            removeButton.ForeColor = Color.Black;
            removeButton.BackColor = Color.White;


            //Логика отображения интервала только для режима "цикл"
            behaviorBox.SelectedIndexChanged += (s, e) =>
            {
                bool showInteval = behaviorBox.SelectedItem.ToString() == "Цикл";
                intervalBox.Visible = showInteval;
                msLabel.Visible = showInteval;
            };

            removeButton.Click += (s, e) =>
            {
                flpKeySettings.Controls.Remove(card);
                selectedKeys.Remove(keyName);

                var buttonToReset = simulatedKeys.FirstOrDefault(b => b.Text == keyName);
                if (buttonToReset != null)
                {
                    buttonToReset.BackColor = Color.White;
                }
            };

            //Запихиваем все в карточку
            card.Controls.Add(keyButton);
            card.Controls.Add(behaviorBox);
            card.Controls.Add(intervalBox);
            card.Controls.Add(msLabel);
            card.Controls.Add(removeButton);

            flpKeySettings.Controls.Add(card);
        }

        /// <summary>
        /// Метод-обработчик перехода с одной страницы на другую
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == tabPage2 && !behaviorPageInitialized)
            {
                foreach (string key in selectedKeys)
                {
                    CreateKeySettingCard(key);
                }
                GenerateApplyAndResetButtons();
                behaviorPageInitialized = true;
            }
        }

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int HOTKEY_ID = 9000;
        private const uint MOD_NONE = 0x0000;
        private const int WM_HOTKEY = 0x0312;
        


    }

    /// <summary>
    /// Вспомогательный класс, чтобы удобно описывать целую строку клавиатуры
    /// Помогает хранить и перебирать строки клавиатуры прямо со сдвигами НЕ вручную
    /// </summary>
    public class KeyLayout
    {
        public string[] Keys
        {
            get;
        }
        
        public int XOffset
        {
            get;
        }

        public KeyLayout(string[] keys, int xOffset)
        {
            Keys = keys;
            XOffset = xOffset;
        }
    }

    public class KeyBehavior
    {
        public string KeyName { get; set; }
        public string Mode { get; set; }
        public int IntervalMs { get; set; }
    }
        

}
