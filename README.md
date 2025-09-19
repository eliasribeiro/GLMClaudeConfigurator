# GLMClaudeConfigurator

Aplicativo Windows Forms (.NET) para configurar rapidamente o uso do modelo GLM com Claude no seu repositório local.

Recursos
- Seleção de pasta do repositório via diálogo nativo de pastas
- Layout moderno com foco em usabilidade (tipografia Segoe UI, cores modernas, botões flat, hover/pressed)
- Máscara para a chave de API
- Configurar: define variáveis de ambiente e cria/atualiza .claude/settings.json (com backup)
- Reverter: remove variáveis, restaura settings.json de backup ou remove o arquivo
- Feedback visual com barra de progresso (loading) e execução em background para evitar travamentos

Requisitos
- Windows
- .NET 10.0 (Windows) ou superior

Como executar
1. Abra um terminal na pasta do projeto GLMClaudeConfigurator.
2. Execute:
   dotnet run

Como usar
1. Informe sua chave de API GLM.
2. Selecione a pasta do repositório (botão "...").
3. Clique em "Configurar" para aplicar as configurações.
4. Para desfazer, clique em "Reverter". O processo é assíncrono e mostra loading.

Detalhes técnicos
- Ambiente:
  - ANTHROPIC_BASE_URL = https://api.z.ai/api/anthropic
  - ANTHROPIC_AUTH_TOKEN = <sua_chave_api>
- Arquivo .claude/settings.json:
  - model: "glm-4.5"
  - apiKey: "<sua_chave_api>"
- Backup automático: .claude/settings.json.backup

Estrutura
- GLMClaudeConfigurator/
  - MainForm.cs
  - Program.cs
  - GLMClaudeConfigurator.csproj

Observações
- Não armazene chaves de API no repositório.
- O .gitignore padrão já está incluso neste projeto.