namespace AutoClickerApp
{
    public partial class Form1 : Form
    {
        private List<Button> simulatedKeys = new List<Button>();    // создаем список для генерации клавиш
        private List<string> selectedKeys = new List<string>();     // список для хранения выбранных пользователем клавиш
        private bool behaviorPageInitialized = false;

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

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)     // метод-обработчик, запускается когда форма создается и начинает загружаться, но еще не видна пользователю
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
}
