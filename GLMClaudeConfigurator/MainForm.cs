using System;
using System.Windows.Forms;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Drawing;
using System.Threading.Tasks;

namespace GLMClaudeConfigurator
{
    public partial class MainForm : Form
    {
        private ProgressBar progressBar;
        private TextBox txtApiKey;
        private TextBox txtRepoPath;
        private Button btnConfigure;
        private Button btnRevert;
        private Button btnExit;

        public MainForm()
        {
            this.Text = "Configurador GLM Claude";
            this.Size = new System.Drawing.Size(500, 300);
            this.BackColor = Color.FromArgb(248, 250, 252); // slate-50 like
            this.Font = new Font("Segoe UI", 9F);
            this.Padding = new Padding(20);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Panel container for card-like effect
            Panel mainPanel = new Panel()
            {
                Location = new Point(20, 20),
                Size = new Size(440, 220),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            // Controles inside panel
            Label lblApiKey = new Label() { Text = "Chave API GLM:", Location = new System.Drawing.Point(20, 20), Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(55, 65, 81) }; // gray-700
            txtApiKey = new TextBox() { Location = new System.Drawing.Point(150, 20), Width = 250, BackColor = Color.FromArgb(243, 244, 246), BorderStyle = BorderStyle.FixedSingle }; // gray-100
            txtApiKey.UseSystemPasswordChar = true; // Ocultar conteúdo por segurança
            txtApiKey.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right; // Responsivo

            Label lblRepoPath = new Label() { Text = "Caminho do Repositório:", Location = new System.Drawing.Point(20, 60), Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(55, 65, 81) };
            txtRepoPath = new TextBox() { Location = new System.Drawing.Point(150, 60), Width = 220, BackColor = Color.FromArgb(243, 244, 246), BorderStyle = BorderStyle.FixedSingle };
            txtRepoPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right; // Responsivo
            
            Button btnSelectRepo = new Button() { Text = "...", Location = new System.Drawing.Point(370, 60), Width = 30, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(209, 213, 219), ForeColor = Color.Black }; // gray-300
            btnSelectRepo.Anchor = AnchorStyles.Top | AnchorStyles.Right; // Responsivo
            btnSelectRepo.FlatAppearance.BorderSize = 0;
            btnSelectRepo.FlatAppearance.MouseOverBackColor = Color.FromArgb(156, 163, 175); // gray-400
            btnSelectRepo.FlatAppearance.MouseDownBackColor = Color.FromArgb(107, 114, 128); // gray-500
            btnSelectRepo.Cursor = Cursors.Hand;
            btnSelectRepo.Click += BtnSelectRepo_Click; // Abrir seletor de pasta

            btnConfigure = new Button() { Text = "Configurar", Location = new System.Drawing.Point(20, 120), Width = 100, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(59, 130, 246), ForeColor = Color.White }; // blue-500
            btnConfigure.Click += BtnConfigure_Click;
            btnConfigure.FlatAppearance.BorderSize = 0;
            btnConfigure.FlatAppearance.MouseOverBackColor = Color.FromArgb(37, 99, 235); // blue-600
            btnConfigure.FlatAppearance.MouseDownBackColor = Color.FromArgb(29, 78, 216); // blue-700
            btnConfigure.Cursor = Cursors.Hand;

            btnRevert = new Button() { Text = "Reverter", Location = new System.Drawing.Point(130, 120), Width = 100, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(239, 68, 68), ForeColor = Color.White }; // red-500
            btnRevert.Click += BtnRevert_Click;
            btnRevert.FlatAppearance.BorderSize = 0;
            btnRevert.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 38, 38); // red-600
            btnRevert.FlatAppearance.MouseDownBackColor = Color.FromArgb(185, 28, 28); // red-700
            btnRevert.Cursor = Cursors.Hand;

            btnExit = new Button() { Text = "Sair", Location = new System.Drawing.Point(240, 120), Width = 100, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(107, 114, 128), ForeColor = Color.White }; // gray-500
            btnExit.Click += (sender, e) => Application.Exit();
            btnExit.FlatAppearance.BorderSize = 0;
            btnExit.FlatAppearance.MouseOverBackColor = Color.FromArgb(75, 85, 99); // gray-600
            btnExit.FlatAppearance.MouseDownBackColor = Color.FromArgb(55, 65, 81); // gray-700
            btnExit.Cursor = Cursors.Hand;

            progressBar = new ProgressBar() { Location = new System.Drawing.Point(20, 180), Width = 380, Style = ProgressBarStyle.Marquee, Visible = false };
            progressBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right; // Responsivo

            // Adicionar tooltips
            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(txtApiKey, "Insira sua chave API GLM");
            toolTip.SetToolTip(txtRepoPath, "Selecione o caminho do repositório");
            toolTip.SetToolTip(btnConfigure, "Configurar as variáveis e settings");
            toolTip.SetToolTip(btnRevert, "Reverter as configurações");
            toolTip.SetToolTip(btnExit, "Sair do aplicativo");
            toolTip.SetToolTip(btnSelectRepo, "Selecionar pasta");

            mainPanel.Controls.Add(lblApiKey);
            mainPanel.Controls.Add(txtApiKey);
            mainPanel.Controls.Add(lblRepoPath);
            mainPanel.Controls.Add(txtRepoPath);
            mainPanel.Controls.Add(btnSelectRepo);
            mainPanel.Controls.Add(btnConfigure);
            mainPanel.Controls.Add(btnRevert);
            mainPanel.Controls.Add(btnExit);
            mainPanel.Controls.Add(progressBar);

            this.Controls.Add(mainPanel);
            
            // Ajustes de experiência
            this.AcceptButton = btnConfigure; // Enter para configurar
            this.CancelButton = btnExit;      // Esc para sair

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