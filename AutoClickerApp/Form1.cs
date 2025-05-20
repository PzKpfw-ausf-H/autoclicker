namespace AutoClickerApp
{
    public partial class Form1 : Form
    {
        private List<Button> simulatedKeys = new List<Button>();    // ������� ������ ��� ��������� ��������� ������ ����������

        private List<KeyLayout> keyboardLayout = new List<KeyLayout>
        /*
         keyboardLayout ��� ������ ����� ����������.
        ������ ������ - ������ ������ + ������� �����, ����� ������ �� ���������� ������ ���� ��� ������
         */
        {
            new KeyLayout(new[] { "`", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "-", "=", "Backspace" }, xOffset: 0),
            new KeyLayout(new[] { "Tab", "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P", "[", "]", "\\" }, xOffset: 10),
            new KeyLayout(new[] { "Caps", "A", "S", "D", "F", "G", "H", "J", "K", "L", ";", "'", "Enter" }, xOffset: 20),
            new KeyLayout(new[] { "Shift", "Z", "X", "C", "V", "B", "N", "M", ",", ".", "/", "RShift" }, xOffset: 30),
            new KeyLayout(new[] { "Ctrl", "Win", "Alt", "Space", "AltGr", "Fn", "Del" }, xOffset: 0),
        };

        private Dictionary<string, int> specialWidths = new Dictionary<string, int>()   // �������, ��� ������� ������������� ������� ������ (�����������)
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
        /// ��������������� ����� ��� �������� ��������� ���������� ������ � ������� ���������
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
        /// ����� ��� ��������� ������� �������� ������������ ����������
        /// ���������� ������� ���������� ���� ��� �������� ���������� ��������
        /// </summary>
        private void GenerateKeyboard()
        {
            //������� ����� ��� ����������� ��������� ������ ���������� � �����
            GroupBox grpKeyboard = new GroupBox();
            grpKeyboard.Text = "Keyboard";
            grpKeyboard.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            grpKeyboard.ForeColor = Color.White;
            grpKeyboard.BackColor = this.BackColor;
            grpKeyboard.Location = new Point(20, 150);
            grpKeyboard.Size = new Size(710, 250);

            tabPage1.Controls.Add(grpKeyboard);

            //��������� ���
            int startX = 10;
            int startY = 20; // ������ ��� �� ����� �����, �� �����
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

                    //����� ���������� ���������� � ������
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
        /// ��������������� ����� ��� ��������� ����� � ������ � ����� �� ��������
        /// �� ��� ���� �������� ������������ ��� ������� � ��� ����� ��� ������� �� ���
        /// </summary>
        private void GenerateMouseBlock()
        {
            //������� ����� ��� ����������� ��������� ������ ���������� � �����
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

                //����� ���������� ���������� � ������
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

                //����� ���������� ���������� � ������
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

        private void Form1_Load(object sender, EventArgs e)     // �����-����������, ����������� ����� ����� ��������� � �������� �����������, �� ��� �� ����� ������������
        {
            GenerateKeyboard();
            GenerateMouseBlock();
            GenerateInstruction();
        }

        /// <summary>
        /// �����-���������� ��� ������� ������. ����������� ������� ��� sender � �������� �� � Button
        /// </summary>
        /// <param name="sender">���, ��� "�������" ������, ���������� ������ ��� �������</param>
        /// <param name="e"></param>
        private void KeyButton_Click(object sender, EventArgs e)
        {
            Button clickedButton = sender as Button;

            if (clickedButton != null)
            {
                clickedButton.BackColor = ColorTranslator.FromHtml("#76F076");
                MessageBox.Show($"������� {clickedButton.Text} ������� ��� ���������!");
            }
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }
    }

    /// <summary>
    /// ��������������� �����, ����� ������ ��������� ����� ������ ����������
    /// �������� ������� � ���������� ������ ���������� ����� �� �������� �� �������
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
