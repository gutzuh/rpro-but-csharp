namespace RPRO.Data;

public class DatabaseMigration
{
    private readonly DatabaseContext _db;

    public DatabaseMigration(DatabaseContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Cria as tabelas se não existirem
    /// </summary>
    public async Task MigrateAsync()
    {
        // Tabela relatorio
        await _db.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS relatorio (
                id CHAR(36) PRIMARY KEY,
                Dia VARCHAR(10),
                Hora TIME,
                Nome VARCHAR(255),
                Form1 INT DEFAULT 0,
                Form2 INT DEFAULT 0,
                Prod_1 INT DEFAULT 0, Prod_2 INT DEFAULT 0, Prod_3 INT DEFAULT 0, Prod_4 INT DEFAULT 0, Prod_5 INT DEFAULT 0,
                Prod_6 INT DEFAULT 0, Prod_7 INT DEFAULT 0, Prod_8 INT DEFAULT 0, Prod_9 INT DEFAULT 0, Prod_10 INT DEFAULT 0,
                Prod_11 INT DEFAULT 0, Prod_12 INT DEFAULT 0, Prod_13 INT DEFAULT 0, Prod_14 INT DEFAULT 0, Prod_15 INT DEFAULT 0,
                Prod_16 INT DEFAULT 0, Prod_17 INT DEFAULT 0, Prod_18 INT DEFAULT 0, Prod_19 INT DEFAULT 0, Prod_20 INT DEFAULT 0,
                Prod_21 INT DEFAULT 0, Prod_22 INT DEFAULT 0, Prod_23 INT DEFAULT 0, Prod_24 INT DEFAULT 0, Prod_25 INT DEFAULT 0,
                Prod_26 INT DEFAULT 0, Prod_27 INT DEFAULT 0, Prod_28 INT DEFAULT 0, Prod_29 INT DEFAULT 0, Prod_30 INT DEFAULT 0,
                Prod_31 INT DEFAULT 0, Prod_32 INT DEFAULT 0, Prod_33 INT DEFAULT 0, Prod_34 INT DEFAULT 0, Prod_35 INT DEFAULT 0,
                Prod_36 INT DEFAULT 0, Prod_37 INT DEFAULT 0, Prod_38 INT DEFAULT 0, Prod_39 INT DEFAULT 0, Prod_40 INT DEFAULT 0,
                INDEX idx_dia_hora (Dia, Hora),
                INDEX idx_nome (Nome),
                INDEX idx_form1 (Form1),
                INDEX idx_form2 (Form2)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
        ");

        // Tabela materia_prima
        await _db.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS materia_prima (
                id CHAR(36) PRIMARY KEY,
                num INT UNIQUE,
                produto VARCHAR(30) DEFAULT 'Sem Produto',
                medida INT DEFAULT 1,
                ativo BOOLEAN DEFAULT TRUE,
                ignorarCalculos BOOLEAN DEFAULT FALSE
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
        ");

        // Tabela amendoim
        await _db.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS amendoim (
                id INT AUTO_INCREMENT PRIMARY KEY,
                tipo VARCHAR(10) DEFAULT 'entrada',
                dia VARCHAR(10) NOT NULL,
                hora VARCHAR(8) NOT NULL,
                codigoProduto VARCHAR(50) NOT NULL,
                codigoCaixa VARCHAR(50),
                nomeProduto VARCHAR(255),
                peso DECIMAL(10,3),
                balanca VARCHAR(10),
                createdAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                INDEX idx_dia_hora (dia, hora),
                INDEX idx_codigoProduto (codigoProduto),
                INDEX idx_tipo (tipo),
                INDEX idx_tipo_dia (tipo, dia),
                UNIQUE KEY unique_record (tipo, dia, hora, codigoProduto, peso)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
        ");

        // Tabela amendoim_raw
        await _db.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS amendoim_raw (
                id INT AUTO_INCREMENT PRIMARY KEY,
                tipo VARCHAR(10) DEFAULT 'entrada',
                dia VARCHAR(10),
                hora VARCHAR(8),
                codigoProduto VARCHAR(50),
                codigoCaixa VARCHAR(50),
                nomeProduto VARCHAR(255),
                peso DECIMAL(10,3),
                balanca VARCHAR(10),
                sourceIhm VARCHAR(50),
                rawLine TEXT,
                createdAt DATETIME DEFAULT CURRENT_TIMESTAMP
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
        ");

        // Tabela users
        await _db.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS user (
                id INT AUTO_INCREMENT PRIMARY KEY,
                username VARCHAR(100) UNIQUE NOT NULL,
                password VARCHAR(255) NOT NULL,
                isAdmin BOOLEAN DEFAULT FALSE,
                displayName VARCHAR(255),
                photoPath TEXT,
                userType VARCHAR(20) DEFAULT 'racao'
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
        ");

        // Tabela settings
        await _db.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS setting (
                `key` VARCHAR(255) PRIMARY KEY,
                `value` TEXT
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
        ");

        // Criar usuário admin padrão se não existir
        var adminExists = await _db.QueryFirstOrDefaultAsync<int>(
            "SELECT COUNT(*) FROM user WHERE username = 'admin'");
        
        if (adminExists == 0)
        {
            await _db.ExecuteAsync(@"
                INSERT INTO user (username, password, isAdmin, displayName, userType)
                VALUES ('admin', 'admin', TRUE, 'Administrador', 'racao')
            ");
        }
    }
}