CREATE DATABASE IF NOT EXISTS AutoFixManagerDb
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_0900_ai_ci;
USE AutoFixManagerDb;

CREATE TABLE IF NOT EXISTS Clientes (
    Id INT NOT NULL AUTO_INCREMENT,
    Nombre VARCHAR(80) NOT NULL,
    Apellido VARCHAR(80) NOT NULL,
    Email VARCHAR(160) NOT NULL,
    Telefono VARCHAR(30) NOT NULL,
    FechaRegistro DATETIME(6) NOT NULL,
    PRIMARY KEY (Id),
    UNIQUE KEY IX_Clientes_Email (Email)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS Usuarios (
    Id INT NOT NULL AUTO_INCREMENT,
    NombreUsuario VARCHAR(80) NOT NULL,
    PasswordHash LONGTEXT NOT NULL,
    Rol VARCHAR(20) NOT NULL,
    ClienteId INT NULL,
    PRIMARY KEY (Id),
    UNIQUE KEY IX_Usuarios_NombreUsuario (NombreUsuario),
    UNIQUE KEY IX_Usuarios_ClienteId (ClienteId),
    CONSTRAINT CK_Usuarios_Rol CHECK (Rol IN ('ADMIN', 'MECANICO', 'CLIENTE')),
    CONSTRAINT FK_Usuarios_Clientes FOREIGN KEY (ClienteId)
        REFERENCES Clientes (Id) ON DELETE RESTRICT
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS Vehiculos (
    Id INT NOT NULL AUTO_INCREMENT,
    ClienteId INT NOT NULL,
    Placa VARCHAR(12) NOT NULL,
    Marca VARCHAR(60) NOT NULL,
    Modelo VARCHAR(80) NOT NULL,
    Anio INT NOT NULL,
    Color VARCHAR(40) NOT NULL,
    PRIMARY KEY (Id),
    UNIQUE KEY IX_Vehiculos_Placa (Placa),
    KEY IX_Vehiculos_ClienteId (ClienteId),
    CONSTRAINT FK_Vehiculos_Clientes FOREIGN KEY (ClienteId)
        REFERENCES Clientes (Id) ON DELETE RESTRICT
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS Servicios (
    Id INT NOT NULL AUTO_INCREMENT,
    ClienteId INT NOT NULL,
    VehiculoId INT NOT NULL,
    MecanicoId INT NULL,
    Descripcion VARCHAR(500) NOT NULL,
    TipoMantenimiento VARCHAR(80) NOT NULL,
    FechaIngreso DATETIME(6) NOT NULL,
    CostoEstimado DECIMAL(12,2) NOT NULL,
    Estado VARCHAR(30) NOT NULL,
    FechaCreacion DATETIME(6) NOT NULL,
    PRIMARY KEY (Id),
    KEY IX_Servicios_ClienteId_Estado (ClienteId, Estado),
    KEY IX_Servicios_VehiculoId (VehiculoId),
    KEY IX_Servicios_MecanicoId (MecanicoId),
    CONSTRAINT CK_Servicios_CostoEstimado CHECK (CostoEstimado >= 0),
    CONSTRAINT FK_Servicios_Clientes FOREIGN KEY (ClienteId)
        REFERENCES Clientes (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_Servicios_Vehiculos FOREIGN KEY (VehiculoId)
        REFERENCES Vehiculos (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_Servicios_Usuarios_Mecanico FOREIGN KEY (MecanicoId)
        REFERENCES Usuarios (Id) ON DELETE RESTRICT
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS ServiciosMecanicos (
    ServicioId INT NOT NULL,
    MecanicoId INT NOT NULL,
    FechaAsignacion DATETIME(6) NOT NULL,
    PRIMARY KEY (ServicioId, MecanicoId),
    KEY IX_ServiciosMecanicos_MecanicoId (MecanicoId),
    CONSTRAINT FK_ServiciosMecanicos_Servicios FOREIGN KEY (ServicioId)
        REFERENCES Servicios (Id) ON DELETE CASCADE,
    CONSTRAINT FK_ServiciosMecanicos_Usuarios FOREIGN KEY (MecanicoId)
        REFERENCES Usuarios (Id) ON DELETE RESTRICT
) ENGINE=InnoDB;

INSERT INTO ServiciosMecanicos (ServicioId, MecanicoId, FechaAsignacion)
SELECT s.Id, s.MecanicoId, UTC_TIMESTAMP(6)
FROM Servicios AS s
LEFT JOIN ServiciosMecanicos AS sm
    ON sm.ServicioId = s.Id AND sm.MecanicoId = s.MecanicoId
WHERE s.MecanicoId IS NOT NULL AND sm.ServicioId IS NULL;

CREATE TABLE IF NOT EXISTS RefreshTokens (
    Id INT NOT NULL AUTO_INCREMENT,
    UsuarioId INT NOT NULL,
    TokenHash VARCHAR(64) NOT NULL,
    CreadoEn DATETIME(6) NOT NULL,
    ExpiraEn DATETIME(6) NOT NULL,
    RevocadoEn DATETIME(6) NULL,
    ReemplazadoPorHash VARCHAR(64) NULL,
    PRIMARY KEY (Id),
    UNIQUE KEY IX_RefreshTokens_TokenHash (TokenHash),
    KEY IX_RefreshTokens_UsuarioId_RevocadoEn_ExpiraEn (UsuarioId, RevocadoEn, ExpiraEn),
    CONSTRAINT FK_RefreshTokens_Usuarios FOREIGN KEY (UsuarioId)
        REFERENCES Usuarios (Id) ON DELETE CASCADE
) ENGINE=InnoDB;

DELIMITER //
DROP PROCEDURE IF EXISTS sp_ResumenServiciosPorEstado//
CREATE PROCEDURE sp_ResumenServiciosPorEstado()
BEGIN
    SELECT Estado, COUNT(*) AS Cantidad,
           COALESCE(SUM(CostoEstimado), 0) AS CostoTotal
    FROM Servicios
    GROUP BY Estado
    ORDER BY Estado;
END//
DELIMITER ;
