using System;
using System.Windows.Forms;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace GLMClaudeConfigurator
{
    public partial class MainForm : Form
    {
        // Importações para efeitos de sombra e bordas arredondadas
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("dwmapi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);

        [StructLayout(LayoutKind.Sequential)]
        private struct MARGINS
        {
            public int leftWidth;
            public int rightWidth;
            public int topHeight;
            public int bottomHeight;
        }

        private ProgressBar progressBar;
        private TextBox txtApiKey;
        private TextBox txtRepoPath;
        private Button btnConfigure;
        private Button btnRevert;
        private Button btnExit;
        private Panel headerPanel;
        private Panel contentPanel;
        private Label lblTitle;
        private Label lblSubtitle;
        private PictureBox iconBox;

        public MainForm()
        {
            this.Text = "GLM Claude Configurator";
            this.Size = new System.Drawing.Size(600, 450);
            this.BackColor = Color.FromArgb(15, 23, 42); // slate-900
            this.Font = new Font("Segoe UI", 10F);
            this.Padding = new Padding(0);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);

            // Aplicar efeitos de sombra e bordas arredondadas
            ApplyShadowAndRoundedCorners();

            InitializeCustomComponents();
        }

        private void ApplyShadowAndRoundedCorners()
        {
            // Configurar sombra e bordas arredondadas no Windows 10/11
            int attrValue = 2; // DWMWA_BORDER_COLOR
            DwmSetWindowAttribute(this.Handle, 34, ref attrValue, sizeof(int));

            MARGINS margins = new MARGINS
            {
                leftWidth = 10,
                rightWidth = 10,
                topHeight = 10,
                bottomHeight = 10
            };
            DwmExtendFrameIntoClientArea(this.Handle, ref margins);
        }

        private void InitializeCustomComponents()
        {

            // Header Panel com gradiente
            headerPanel = new Panel()
            {
                Location = new Point(0, 0),
                Size = new Size(600, 80),
                BackColor = Color.Transparent
            };
            headerPanel.Paint += HeaderPanel_Paint;

            // Ícone do aplicativo
            iconBox = new PictureBox()
            {
                Location = new Point(20, 20),
                Size = new Size(40, 40),
                BackColor = Color.Transparent
            };
            DrawIcon(iconBox);

            // Título
            lblTitle = new Label()
            {
                Text = "GLM Claude Configurator",
                Location = new Point(70, 15),
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = true
            };

            // Subtítulo
            lblSubtitle = new Label()
            {
                Text = "Configure sua integração GLM com Claude de forma simples",
                Location = new Point(70, 45),
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(148, 163, 184), // slate-400
                BackColor = Color.Transparent,
                AutoSize = true
            };

            // Botão de fechar customizado
            Button btnClose = new Button()
            {
                Text = "✕",
                Location = new Point(560, 10),
                Size = new Size(30, 30),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                BackColor = Color.Transparent
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => Application.Exit();
            btnClose.Paint += (s, e) =>
            {
                Button btn = (Button)s;
                using (Font closeFont = new Font("Segoe UI", 12F, FontStyle.Bold))
                {
                    TextRenderer.DrawText(e.Graphics, "✕", closeFont, btn.ClientRectangle, 
                        Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
            };
            btnClose.MouseEnter += (s, e) => btnClose.ForeColor = Color.FromArgb(239, 68, 68);
            btnClose.MouseLeave += (s, e) => btnClose.ForeColor = Color.White;

            // Content Panel com efeito de vidro
            contentPanel = new Panel()
            {
                Location = new Point(20, 100),
                Size = new Size(560, 330),
                BackColor = Color.FromArgb(30, 41, 59), // slate-800 com transparência
                BorderStyle = BorderStyle.None
            };

            // Adicionar efeito de borda arredondada ao contentPanel
            contentPanel.Paint += ContentPanel_Paint;
            contentPanel.Region = CreateRoundedRegion(contentPanel.Size, 15);

            // Controles com estilo moderno
            Label lblApiKey = new Label() 
            { 
                Text = "Chave API GLM:", 
                Location = new System.Drawing.Point(30, 30), 
                Font = new Font("Segoe UI", 11F, FontStyle.Bold), 
                ForeColor = Color.FromArgb(226, 232, 240), // slate-200
                BackColor = Color.Transparent,
                AutoSize = true
            };

            txtApiKey = new TextBox() 
            { 
                Location = new System.Drawing.Point(30, 55), 
                Width = 500, 
                Height = 40,
                BackColor = Color.FromArgb(15, 23, 42), // slate-900
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 11F),
                ForeColor = Color.White,
                PasswordChar = '•'
            };
            txtApiKey.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtApiKey.Region = CreateRoundedRegion(new Size(txtApiKey.Width, txtApiKey.Height), 8);
            txtApiKey.Paint += TextBox_Paint;

            Label lblRepoPath = new Label() 
            { 
                Text = "Caminho do Repositório:", 
                Location = new System.Drawing.Point(30, 110), 
                Font = new Font("Segoe UI", 11F, FontStyle.Bold), 
                ForeColor = Color.FromArgb(226, 232, 240), // slate-200
                BackColor = Color.Transparent,
                AutoSize = true
            };

            txtRepoPath = new TextBox() 
            { 
                Location = new System.Drawing.Point(30, 135), 
                Width = 460, 
                Height = 40,
                BackColor = Color.FromArgb(15, 23, 42), // slate-900
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 11F),
                ForeColor = Color.White,
                ReadOnly = true
            };
            txtRepoPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtRepoPath.Region = CreateRoundedRegion(new Size(txtRepoPath.Width, txtRepoPath.Height), 8);
            txtRepoPath.Paint += TextBox_Paint;
            
            Button btnSelectRepo = new Button() 
            { 
                Text = "📁", 
                Location = new System.Drawing.Point(495, 135), 
                Width = 35, 
                Height = 40,
                FlatStyle = FlatStyle.Flat, 
                BackColor = Color.FromArgb(51, 65, 85), // slate-700
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12F),
                Cursor = Cursors.Hand
            };
            btnSelectRepo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSelectRepo.FlatAppearance.BorderSize = 0;
            btnSelectRepo.FlatAppearance.MouseOverBackColor = Color.FromArgb(71, 85, 105); // slate-600
            btnSelectRepo.FlatAppearance.MouseDownBackColor = Color.FromArgb(100, 116, 139); // slate-500
            btnSelectRepo.Region = CreateRoundedRegion(new Size(btnSelectRepo.Width, btnSelectRepo.Height), 8);
            btnSelectRepo.Click += BtnSelectRepo_Click;

            // Botões com gradiente e efeitos modernos
            btnConfigure = new Button() 
            { 
                Text = "Configurar", 
                Location = new System.Drawing.Point(30, 200), 
                Width = 160, 
                Height = 45,
                FlatStyle = FlatStyle.Flat, 
                BackColor = Color.FromArgb(34, 197, 94), // green-500
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnConfigure.Click += BtnConfigure_Click;
            btnConfigure.FlatAppearance.BorderSize = 0;
            btnConfigure.Region = CreateRoundedRegion(new Size(btnConfigure.Width, btnConfigure.Height), 12);
            btnConfigure.Paint += Button_Paint;
            btnConfigure.MouseEnter += (s, e) => btnConfigure.BackColor = Color.FromArgb(22, 163, 74); // green-600
            btnConfigure.MouseLeave += (s, e) => btnConfigure.BackColor = Color.FromArgb(34, 197, 94); // green-500

            btnRevert = new Button() 
            { 
                Text = "Reverter", 
                Location = new System.Drawing.Point(200, 200), 
                Width = 160, 
                Height = 45,
                FlatStyle = FlatStyle.Flat, 
                BackColor = Color.FromArgb(239, 68, 68), // red-500
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRevert.Click += BtnRevert_Click;
            btnRevert.FlatAppearance.BorderSize = 0;
            btnRevert.Region = CreateRoundedRegion(new Size(btnRevert.Width, btnRevert.Height), 12);
            btnRevert.Paint += Button_Paint;
            btnRevert.MouseEnter += (s, e) => btnRevert.BackColor = Color.FromArgb(220, 38, 38); // red-600
            btnRevert.MouseLeave += (s, e) => btnRevert.BackColor = Color.FromArgb(239, 68, 68); // red-500

            btnExit = new Button() 
            { 
                Text = "Sair", 
                Location = new System.Drawing.Point(370, 200), 
                Width = 160, 
                Height = 45,
                FlatStyle = FlatStyle.Flat, 
                BackColor = Color.FromArgb(71, 85, 105), // slate-600
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnExit.Click += (sender, e) => Application.Exit();
            btnExit.FlatAppearance.BorderSize = 0;
            btnExit.Region = CreateRoundedRegion(new Size(btnExit.Width, btnExit.Height), 12);
            btnExit.Paint += Button_Paint;
            btnExit.MouseEnter += (s, e) => btnExit.BackColor = Color.FromArgb(51, 65, 85); // slate-700
            btnExit.MouseLeave += (s, e) => btnExit.BackColor = Color.FromArgb(71, 85, 105); // slate-600

            // ProgressBar moderno
            progressBar = new ProgressBar() 
            { 
                Location = new System.Drawing.Point(30, 270), 
                Width = 500, 
                Height = 8,
                Style = ProgressBarStyle.Marquee, 
                Visible = false,
                BackColor = Color.FromArgb(15, 23, 42), // slate-900
                ForeColor = Color.FromArgb(34, 197, 94) // green-500
            };
            progressBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // Tooltips com estilo moderno
            ToolTip toolTip = new ToolTip()
            {
                BackColor = Color.FromArgb(30, 41, 59), // slate-800
                ForeColor = Color.White,
                OwnerDraw = true
            };
            toolTip.Draw += ToolTip_Draw;
            toolTip.Popup += ToolTip_Popup;
            
            toolTip.SetToolTip(txtApiKey, "Insira sua chave API GLM");
            toolTip.SetToolTip(txtRepoPath, "Selecione o caminho do repositório");
            toolTip.SetToolTip(btnConfigure, "Configurar as variáveis e settings");
            toolTip.SetToolTip(btnRevert, "Reverter as configurações");
            toolTip.SetToolTip(btnExit, "Sair do aplicativo");
            toolTip.SetToolTip(btnSelectRepo, "Selecionar pasta");

            // Adicionar controles ao contentPanel
            contentPanel.Controls.Add(lblApiKey);
            contentPanel.Controls.Add(txtApiKey);
            contentPanel.Controls.Add(lblRepoPath);
            contentPanel.Controls.Add(txtRepoPath);
            contentPanel.Controls.Add(btnSelectRepo);
            contentPanel.Controls.Add(btnConfigure);
            contentPanel.Controls.Add(btnRevert);
            contentPanel.Controls.Add(btnExit);
            contentPanel.Controls.Add(progressBar);

            // Adicionar controles ao formulário
            this.Controls.Add(headerPanel);
            headerPanel.Controls.Add(iconBox);
            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(lblSubtitle);
            headerPanel.Controls.Add(btnClose);
            this.Controls.Add(contentPanel);
            
            // Ajustes de experiência
            this.AcceptButton = btnConfigure; // Enter para configurar
            this.CancelButton = btnExit;      // Esc para sair

            // Adicionar eventos de mouse para arrastar o formulário
            headerPanel.MouseDown += Form_MouseDown;
            lblTitle.MouseDown += Form_MouseDown;
            lblSubtitle.MouseDown += Form_MouseDown;
            iconBox.MouseDown += Form_MouseDown;

        }

        // Métodos auxiliares para efeitos visuais
        private Region CreateRoundedRegion(Size size, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(0, 0, radius * 2, radius * 2, 180, 90);
            path.AddArc(size.Width - radius * 2, 0, radius * 2, radius * 2, 270, 90);
            path.AddArc(size.Width - radius * 2, size.Height - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(0, size.Height - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
            return new Region(path);
        }

        private void HeaderPanel_Paint(object sender, PaintEventArgs e)
        {
            using (LinearGradientBrush brush = new LinearGradientBrush(
                headerPanel.ClientRectangle,
                Color.FromArgb(59, 130, 246), // blue-500
                Color.FromArgb(37, 99, 235),  // blue-600
                LinearGradientMode.Horizontal))
            {
                e.Graphics.FillRectangle(brush, headerPanel.ClientRectangle);
            }
        }

        private void ContentPanel_Paint(object sender, PaintEventArgs e)
        {
            // Adicionar efeito de borda branca sutil
            using (Pen pen = new Pen(Color.FromArgb(51, 65, 85), 2)) // slate-700
            {
                Rectangle rect = new Rectangle(1, 1, contentPanel.Width - 2, contentPanel.Height - 2);
                using (GraphicsPath path = new GraphicsPath())
                {
                    int radius = 15;
                    path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                    path.AddArc(rect.X + rect.Width - radius, rect.Y, radius, radius, 270, 90);
                    path.AddArc(rect.X + rect.Width - radius, rect.Y + rect.Height - radius, radius, radius, 0, 90);
                    path.AddArc(rect.X, rect.Y + rect.Height - radius, radius, radius, 90, 90);
                    path.CloseFigure();
                    e.Graphics.DrawPath(pen, path);
                }
            }
        }

        private void TextBox_Paint(object? sender, PaintEventArgs e)
        {
            TextBox? textBox = sender as TextBox;
            if (textBox != null)
            {
                using (Pen pen = new Pen(Color.FromArgb(51, 65, 85), 1)) // slate-700
                {
                    Rectangle rect = new Rectangle(0, 0, textBox.Width - 1, textBox.Height - 1);
                    using (GraphicsPath path = new GraphicsPath())
                    {
                        int radius = 8;
                        path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                        path.AddArc(rect.X + rect.Width - radius, rect.Y, radius, radius, 270, 90);
                        path.AddArc(rect.X + rect.Width - radius, rect.Y + rect.Height - radius, radius, radius, 0, 90);
                        path.AddArc(rect.X, rect.Y + rect.Height - radius, radius, radius, 90, 90);
                        path.CloseFigure();
                        e.Graphics.DrawPath(pen, path);
                    }
                }
            }
        }

        private void Button_Paint(object? sender, PaintEventArgs e)
        {
            Button? button = sender as Button;
            if (button != null)
            {
                using (GraphicsPath path = new GraphicsPath())
                {
                    int radius = 12;
                    Rectangle rect = new Rectangle(0, 0, button.Width - 1, button.Height - 1);
                    path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                    path.AddArc(rect.X + rect.Width - radius, rect.Y, radius, radius, 270, 90);
                    path.AddArc(rect.X + rect.Width - radius, rect.Y + rect.Height - radius, radius, radius, 0, 90);
                    path.AddArc(rect.X, rect.Y + rect.Height - radius, radius, radius, 90, 90);
                    path.CloseFigure();

                    using (LinearGradientBrush brush = new LinearGradientBrush(rect, button.BackColor, 
                        ControlPaint.Light(button.BackColor), LinearGradientMode.Vertical))
                    {
                        e.Graphics.FillPath(brush, path);
                    }

                    using (Pen pen = new Pen(ControlPaint.Light(button.BackColor, 0.8f), 1))
                    {
                        e.Graphics.DrawPath(pen, path);
                    }

                    // Adicionar texto centralizado
                    TextRenderer.DrawText(e.Graphics, button.Text, button.Font, rect, button.ForeColor, 
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
            }
        }

        private void DrawIcon(PictureBox pictureBox)
        {
            pictureBox.Paint += (sender, e) =>
            {
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    new Point(0, 0),
                    new Point(pictureBox.Width, pictureBox.Height),
                    Color.FromArgb(34, 197, 94), // green-500
                    Color.FromArgb(22, 163, 74)))  // green-600
                {
                    e.Graphics.FillEllipse(brush, 5, 5, 30, 30);
                }

                using (Font iconFont = new Font("Segoe UI", 14F, FontStyle.Bold))
                {
                    TextRenderer.DrawText(e.Graphics, "⚙", iconFont, pictureBox.ClientRectangle, 
                        Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
            };
        }

        private void ToolTip_Draw(object sender, DrawToolTipEventArgs e)
        {
            using (LinearGradientBrush brush = new LinearGradientBrush(e.Bounds,
                Color.FromArgb(30, 41, 59), // slate-800
                Color.FromArgb(51, 65, 85), // slate-700
                LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            using (Pen pen = new Pen(Color.FromArgb(71, 85, 105), 1)) // slate-600
            {
                e.Graphics.DrawRectangle(pen, new Rectangle(0, 0, e.Bounds.Width - 1, e.Bounds.Height - 1));
            }

            TextRenderer.DrawText(e.Graphics, e.ToolTipText, e.Font, e.Bounds, Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private void ToolTip_Popup(object sender, PopupEventArgs e)
        {
            e.ToolTipSize = new Size(e.ToolTipSize.Width + 10, e.ToolTipSize.Height + 10);
        }

        // Variáveis para arrastar o formulário
        private bool mouseDown;
        private Point lastLocation;

        private void Form_MouseDown(object? sender, MouseEventArgs e)
        {
            mouseDown = true;
            lastLocation = e.Location;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (mouseDown)
            {
                this.Location = new Point(
                    (this.Location.X - lastLocation.X) + e.X, 
                    (this.Location.Y - lastLocation.Y) + e.Y);
                this.Update();
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            mouseDown = false;
            base.OnMouseUp(e);
        }

        // Comentário: Manipulador de evento para abrir o seletor de pasta do repositório
        private void BtnSelectRepo_Click(object? sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Selecione a pasta do repositório";
                dialog.ShowNewFolderButton = false;

                if (!string.IsNullOrWhiteSpace(txtRepoPath.Text) && Directory.Exists(txtRepoPath.Text))
                {
                    dialog.SelectedPath = txtRepoPath.Text;
                }

                if (dialog.ShowDialog(this) == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
                {
                    txtRepoPath.Text = dialog.SelectedPath;
                }
            }
        }

        // Comentário: Função para configurar as variáveis de ambiente e o arquivo settings.json
        private void ConfigureGLM(string apiKey, string repoPath)
        {
            try
            {
                if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(repoPath))
                {
                    MessageBox.Show("Por favor, forneça a chave API e o caminho do repositório.");
                    return;
                }

                // Definir variáveis de ambiente
                Environment.SetEnvironmentVariable("ANTHROPIC_BASE_URL", "https://api.z.ai/api/anthropic", EnvironmentVariableTarget.User);
                Environment.SetEnvironmentVariable("ANTHROPIC_AUTH_TOKEN", apiKey, EnvironmentVariableTarget.User);

                // Caminho para settings.json
                string claudeDir = Path.Combine(repoPath, ".claude");
                if (!Directory.Exists(claudeDir))
                {
                    Directory.CreateDirectory(claudeDir);
                }
                string settingsPath = Path.Combine(claudeDir, "settings.json");
                string backupPath = settingsPath + ".backup";

                // Backup se existir
                if (File.Exists(settingsPath))
                {
                    File.Copy(settingsPath, backupPath, true);
                }

                // Editar ou criar settings.json
                JsonObject settings = new JsonObject();
                if (File.Exists(settingsPath))
                {
                    string json = File.ReadAllText(settingsPath);
                    var parsed = JsonNode.Parse(json) as JsonObject;
                    if (parsed != null)
                    {
                        settings = parsed;
                    }
                }

                // Adicionar configurações
                settings["model"] = "glm-4.5";
                settings["apiKey"] = apiKey;

                File.WriteAllText(settingsPath, settings.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

                MessageBox.Show("Configuração concluída com sucesso!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro durante a configuração: {ex.Message}");
            }
        }

        // Comentário: Função para reverter as configurações
        private void RevertGLM(string repoPath)
        {
            try
            {
                if (string.IsNullOrEmpty(repoPath))
                {
                    MessageBox.Show("Por favor, forneça o caminho do repositório.");
                    return;
                }

                // Remover variáveis de ambiente
                Environment.SetEnvironmentVariable("ANTHROPIC_BASE_URL", null, EnvironmentVariableTarget.User);
                Environment.SetEnvironmentVariable("ANTHROPIC_AUTH_TOKEN", null, EnvironmentVariableTarget.User);

                // No RevertGLM
                // Restaurar settings.json do backup
                string claudeDirRevert = Path.Combine(repoPath, ".claude");
                string settingsPathRevert = Path.Combine(claudeDirRevert, "settings.json");
                string backupPathRevert = settingsPathRevert + ".backup";

                if (File.Exists(backupPathRevert))
                {
                    File.Copy(backupPathRevert, settingsPathRevert, true);
                    File.Delete(backupPathRevert);
                }
                else if (File.Exists(settingsPathRevert))
                {
                    File.Delete(settingsPathRevert);
                }

                MessageBox.Show("Reversão concluída com sucesso!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro durante a reversão: {ex.Message}");
            }
        }

        private async void BtnConfigure_Click(object? sender, EventArgs e)
        {
            btnConfigure.Enabled = false;
            btnRevert.Enabled = false;
            btnExit.Enabled = false;
            progressBar.Visible = true;

            await Task.Run(() => ConfigureGLM(txtApiKey.Text, txtRepoPath.Text));

            progressBar.Visible = false;
            btnConfigure.Enabled = true;
            btnRevert.Enabled = true;
            btnExit.Enabled = true;
        }

        // Comentário: Manipulador de clique do botão Reverter para executar em background e exibir loading
        private async void BtnRevert_Click(object? sender, EventArgs e)
        {
            btnConfigure.Enabled = false;
            btnRevert.Enabled = false;
            btnExit.Enabled = false;
            progressBar.Visible = true;

            await Task.Run(() => RevertGLM(txtRepoPath.Text));

            progressBar.Visible = false;
            btnConfigure.Enabled = true;
            btnRevert.Enabled = true;
            btnExit.Enabled = true;
        }
    }
}