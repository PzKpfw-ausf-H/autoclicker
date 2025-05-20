using WindowsInput;
using WindowsInput.Native;

namespace AutoClickerApp
{
    public partial class Form1 : Form
    {
        private List<Button> simulatedKeys = new List<Button>();    // создаем список для генерации клавиш
        private List<string> selectedKeys = new List<string>();     // список для хранения выбранных пользователем клавиш
        private List<KeyBehavior> currentBehaviors = new List<KeyBehavior>();


        private bool behaviorPageInitialized = false;

        private InputSimulator inputSimulator = new InputSimulator();

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
                MessageBox.Show("Автокликер остановлен");
                return;
            }

            autoclickerRunning = true;
            MessageBox.Show("Автокликер запущен");

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
            if (TryGetVirtualKey(key, out VirtualKeyCode code))
            {
                inputSimulator.Keyboard.KeyPress(code);
            }
        }

        /// <summary>
        /// Метод, симулирующий удержание клавиши
        /// </summary>
        /// <param name="key"></param>
        private void HoldKeyDown(string key)
        {
            if (TryGetVirtualKey(key, out VirtualKeyCode code))
            {
                inputSimulator.Keyboard.KeyDown(code);
            }
        }

        /// <summary>
        /// Метод для получения нажатой клавиши:
        /// обычные клавиши обабатываются в Enum.TryPase...
        /// специальные клавиши обрабатываются в switch-case
        /// </summary>
        /// <param name="key"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        private bool TryGetVirtualKey(string key, out VirtualKeyCode code)
        {
            key = key.ToUpper();

            if (Enum.TryParse("VK_" + key, out code)) return true;

            switch (key)
            {
                case "LMB": code = VirtualKeyCode.LBUTTON; return true;
                case "RMB": code = VirtualKeyCode.RBUTTON; return true;
                case "MMB": code = VirtualKeyCode.MBUTTON; return true;
                case "SPACE": code = VirtualKeyCode.SPACE; return true;
                case "ENTER": code = VirtualKeyCode.RETURN; return true;
                case "SHIFT": code = VirtualKeyCode.SHIFT; return true;
                case "CTRL": code = VirtualKeyCode.CONTROL; return true;
                case "TAB": code = VirtualKeyCode.TAB; return true;
                case "BACKSPACE": code = VirtualKeyCode.BACK; return true;
                case "CAPS": code = VirtualKeyCode.CAPITAL; return true;

                default: code = 0; return false;
            }
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

        /// <summary>
        /// Вспомогательный метод для создания небольшой инструкции сверху в главной страничке
        /// </summary>
        private void GenerateInstruction()
        {
            int startX = 20;
            int startY = 70;
            Label instructionLabel = new Label();
            instructionLabel.Text = "Pick one or more keys you want to simulate";
            instructionLabel.Location = new Point(startX, startY);
            instructionLabel.Size = new Size(800, 30);
            instructionLabel.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            instructionLabel.ForeColor = Color.White;
            tabPage1.Controls.Add(instructionLabel);
        }

        /// <summary>
        /// Метод для генерации нативно понятной пользователю клавиатуры
        /// Использует словарь спецклавиш выше для создания корректных отступов
        /// </summary>
        private void GenerateKeyboard()
        {
            //Создаем рамку для визуального отделения блоков клавиатуры и мышки
            GroupBox grpKeyboard = new GroupBox();
            grpKeyboard.Text = "Keyboard";
            grpKeyboard.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            grpKeyboard.ForeColor = Color.White;
            grpKeyboard.BackColor = this.BackColor;
            grpKeyboard.Location = new Point(20, 150);
            grpKeyboard.Size = new Size(710, 250);

            tabPage1.Controls.Add(grpKeyboard);

            //Остальной код
            int startX = 10;
            int startY = 20; // Теперь это от верха рамки, не формы
            int buttonHeight = 40;
            int spacing = 5;

            for (int row = 0; row < keyboardLayout.Count; row++)
            {
                var rowData = keyboardLayout[row];
                int x = startX + rowData.XOffset;

                foreach (string key in rowData.Keys)
                {
                    Button btn = new Button();
                    btn.Text = key;
                    btn.Name = "btn" + key;
                    btn.Height = buttonHeight;
                    btn.Width = specialWidths.ContainsKey(key) ? specialWidths[key] : 40;
                    btn.Location = new Point(x, startY + row * (buttonHeight + spacing));
                    btn.BackColor = Color.White;
                    btn.ForeColor = Color.Black;
                    btn.Click += KeyButton_Click;

                    //Здесь симулируем скругления у кнопок
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 1;
                    btn.FlatAppearance.BorderColor = Color.Gray;
                    btn.Font = new Font("Segoe UI", 10, FontStyle.Regular);

                    grpKeyboard.Controls.Add(btn);
                    simulatedKeys.Add(btn);

                    x += btn.Width + spacing;
                }
            }
        }
        /// <summary>
        /// Вспомогательный метод для генерации блока с мышкой и всеми ее кнопками
        /// Тк нет спец символов используются два массива и два цикла для прохода по ним
        /// </summary>
        private void GenerateMouseBlock()
        {
            //Создаем рамку для визуального отделения блоков клавиатуры и мышки
            GroupBox grpMouse = new GroupBox();
            grpMouse.Text = "Mouse";
            grpMouse.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            grpMouse.ForeColor = Color.White;
            grpMouse.BackColor = this.BackColor;
            grpMouse.Location = new Point(750, 150);
            grpMouse.Size = new Size(220, 170);
            grpMouse.Padding = new Padding(10);

            tabPage1.Controls.Add(grpMouse);

            int baseX = 10;
            int baseY = 20;
            int btnSize = 60;
            int spacing = 10;

            string[] topRow = { "LMB", "MMB", "RMB" };
            string[] bottomRow = { "X1", "X2" };

            for (int i = 0; i < topRow.Length; i++)
            {
                Button btn = new Button();
                btn.Text = topRow[i];
                btn.Name = "btn" + topRow[i];
                btn.Size = new Size(btnSize, btnSize);
                btn.Location = new Point(baseX + i * (btnSize + spacing), baseY);
                btn.BackColor = Color.White;
                btn.ForeColor = Color.Black;
                btn.Click += KeyButton_Click;

                //Здесь симулируем скругления у кнопок
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 1;
                btn.FlatAppearance.BorderColor = Color.Gray;
                btn.Font = new Font("Segoe UI", 10, FontStyle.Regular);

                grpMouse.Controls.Add(btn);
                simulatedKeys.Add(btn);
            }
            for (int i = 0; i < bottomRow.Length; i++)
            {
                Button btn = new Button();
                btn.Text = bottomRow[i];
                btn.Name = "btn" + bottomRow[i];
                btn.Size = new Size(btnSize, btnSize);
                btn.Location = new Point(baseX + i * (btnSize + spacing) + btnSize / 2, baseY + btnSize + spacing);
                btn.BackColor = Color.White;
                btn.ForeColor = Color.Black;
                btn.Click += KeyButton_Click;

                //Здесь симулируем скругления у кнопок
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 1;
                btn.FlatAppearance.BorderColor = Color.Gray;
                btn.Font = new Font("Segoe UI", 10, FontStyle.Regular);

                grpMouse.Controls.Add(btn);
                simulatedKeys.Add(btn);
            }
        }

        /// <summary>
        /// Метод, создающий кнопку сохранения настроек для кнопок и описывающий эту самую логику сохранения настроек
        /// </summary>
        private void GenerateApplyButton()
        {
            Button applyButton = new Button();
            applyButton.Text = "Применить";
            applyButton.ForeColor = Color.Black;
            applyButton.BackColor = Color.White;
            applyButton.Size = new Size(120, 30);
            applyButton.Location = new Point(10, flpKeySettings.Height - 40);
            applyButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            flpKeySettings.Controls.Add(applyButton);

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

                MessageBox.Show("Настройки сохранены", "Применено", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
        }

        /// <summary>
        /// Метод для удаления всех кнопок со второй страницы
        /// </summary>
        private void GenerateResetAllButton()
        {
            Button resetAllButton = new Button();
            resetAllButton.Text = "Сбросить все";
            resetAllButton.Size = new Size(120, 30);
            resetAllButton.ForeColor = Color.Black;
            resetAllButton.BackColor = Color.White;
            resetAllButton.Location = new Point(flpKeySettings.Width - 140, flpKeySettings.Height - 40);
            resetAllButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right; 

            flpKeySettings.Controls.Add(resetAllButton);

            resetAllButton.Click += (s, e) =>
            {
                //удаляем карточки
                flpKeySettings.Controls.Clear();

                //сбрасываем выделения кнопок
                foreach (var btn in simulatedKeys)
                {
                    btn.BackColor = Color.White;
                }

                selectedKeys.Clear();

                behaviorPageInitialized = false;
            };
        }


        /// <summary>
        /// метод-обработчик, запускается когда форма создается и начинает загружаться, но еще не видна пользователю
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            GenerateKeyboard();
            GenerateMouseBlock();
            GenerateInstruction();
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
            removeButton.Location = new Point(350, 17);
            removeButton.Width = 80;

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
                GenerateResetAllButton();
                GenerateApplyButton();
                behaviorPageInitialized = true;
            }
        }
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
