# Cortez - Sistema de Produção

Sistema desktop para coleta, processamento e análise de dados de produção.

## 🚀 Tecnologias

- **Framework:** .NET 8 + WPF
- **UI:** Material Design in XAML
- **Gráficos:** LiveCharts2
- **Banco de Dados:** MySQL + Dapper
- **Coletor FTP:** FluentFTP

## 📋 Requisitos

- Windows 10/11 64-bit
- MySQL 8.0+ (ou MariaDB)

## 🔧 Desenvolvimento

### Pré-requisitos

1. [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
2. [Visual Studio 2022](https://visualstudio.microsoft.com/) (recomendado)
3. MySQL Server rodando localmente

### Clonar e Executar

```bash
# Clonar repositório
git clone https://github.com/projetosjcortica/RPRO.git
cd RPRO

# Restaurar pacotes
dotnet restore

# Executar
dotnet run --project src/RPRO.App
```

### Estrutura do Projeto

```
📦 RPRO/
├── 📁 src/
│   ├── 📁 RPRO.App/        # Aplicação WPF
│   ├── 📁 RPRO.Core/       # Entidades e Interfaces
│   ├── 📁 RPRO.Data/       # Repositórios e Acesso a Dados
│   └── 📁 RPRO.Services/   # Lógica de Negócio
└── 📁 tests/
    └── 📁 RPRO.Tests/      # Testes Unitários
```

## 📦 Build e Publicação

### Gerar Executável

```bash
# Windows PowerShell
.\build.ps1

# Ou manualmente
dotnet publish src/RPRO.App/RPRO.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish
```

O executável será gerado em `./publish/Cortez.exe`

### Gerar Instalador

1. Instale o [Inno Setup](https://jrsoftware.org/isinfo.php)
2. Abra `installer.iss` no Inno Setup
3. Compile (Ctrl+F9)
4. O instalador será gerado em `./installer/`

## 🗄️ Banco de Dados

O sistema usa MySQL. Na primeira execução, as tabelas são criadas automaticamente.

### Configuração Padrão

```
Host: localhost
Porta: 3306
Usuário: root
Senha: root
Database: cadastro
```

### Usuário Padrão

```
Username: admin
Senha: admin
```

## 📊 Funcionalidades

- ✅ Dashboard de Ração com gráficos
- ✅ Dashboard de Amendoim com métricas
- ✅ Coletor automático via FTP
- ✅ Visualização de dados paginada
- ✅ Exportação Excel/PDF
- ✅ Gerenciamento de usuários
- ✅ Configuração de matérias-primas

## 📝 Licença

Proprietário - J.Cortica © 2024