USE AutoFixManagerDb;

CREATE TABLE IF NOT EXISTS Citas (
    Id INT NOT NULL AUTO_INCREMENT,
    ClienteId INT NOT NULL,
    VehiculoId INT NOT NULL,
    ServicioId INT NULL,
    FechaCita DATETIME(6) NOT NULL,
    Motivo VARCHAR(500) NOT NULL,
    Estado VARCHAR(20) NOT NULL,
    FechaCreacion DATETIME(6) NOT NULL,
    PRIMARY KEY (Id),
    UNIQUE KEY IX_Citas_ServicioId (ServicioId),
    KEY IX_Citas_ClienteId (ClienteId),
    KEY IX_Citas_VehiculoId (VehiculoId),
    CONSTRAINT FK_Citas_Clientes FOREIGN KEY (ClienteId)
        REFERENCES Clientes (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_Citas_Vehiculos FOREIGN KEY (VehiculoId)
        REFERENCES Vehiculos (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_Citas_Servicios FOREIGN KEY (ServicioId)
        REFERENCES Servicios (Id) ON DELETE SET NULL
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS CategoriasRepuesto (
    Id INT NOT NULL AUTO_INCREMENT,
    Nombre VARCHAR(80) NOT NULL,
    Descripcion VARCHAR(250) NULL,
    PRIMARY KEY (Id),
    UNIQUE KEY IX_CategoriasRepuesto_Nombre (Nombre)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS Proveedores (
    Id INT NOT NULL AUTO_INCREMENT,
    Nombre VARCHAR(120) NOT NULL,
    Email VARCHAR(160) NULL,
    Telefono VARCHAR(30) NULL,
    Direccion VARCHAR(250) NULL,
    Activo TINYINT(1) NOT NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS Repuestos (
    Id INT NOT NULL AUTO_INCREMENT,
    CategoriaRepuestoId INT NOT NULL,
    Codigo VARCHAR(40) NOT NULL,
    Nombre VARCHAR(120) NOT NULL,
    Descripcion VARCHAR(500) NULL,
    StockActual INT NOT NULL,
    StockMinimo INT NOT NULL,
    PrecioCosto DECIMAL(12,2) NOT NULL,
    PrecioVenta DECIMAL(12,2) NOT NULL,
    PRIMARY KEY (Id),
    UNIQUE KEY IX_Repuestos_Codigo (Codigo),
    KEY IX_Repuestos_CategoriaRepuestoId (CategoriaRepuestoId),
    CONSTRAINT CK_Repuestos_Stock CHECK (StockActual >= 0 AND StockMinimo >= 0),
    CONSTRAINT CK_Repuestos_Precios CHECK (PrecioCosto >= 0 AND PrecioVenta >= 0),
    CONSTRAINT FK_Repuestos_CategoriasRepuesto FOREIGN KEY (CategoriaRepuestoId)
        REFERENCES CategoriasRepuesto (Id) ON DELETE RESTRICT
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS OrdenesCompra (
    Id INT NOT NULL AUTO_INCREMENT,
    ProveedorId INT NOT NULL,
    UsuarioId INT NOT NULL,
    FechaCreacion DATETIME(6) NOT NULL,
    Estado VARCHAR(20) NOT NULL,
    Total DECIMAL(12,2) NOT NULL,
    PRIMARY KEY (Id),
    KEY IX_OrdenesCompra_ProveedorId_FechaCreacion (ProveedorId, FechaCreacion),
    KEY IX_OrdenesCompra_UsuarioId (UsuarioId),
    CONSTRAINT CK_OrdenesCompra_Total CHECK (Total >= 0),
    CONSTRAINT FK_OrdenesCompra_Proveedores FOREIGN KEY (ProveedorId)
        REFERENCES Proveedores (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_OrdenesCompra_Usuarios FOREIGN KEY (UsuarioId)
        REFERENCES Usuarios (Id) ON DELETE RESTRICT
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS DetallesOrdenCompra (
    OrdenCompraId INT NOT NULL,
    RepuestoId INT NOT NULL,
    Cantidad INT NOT NULL,
    CostoUnitario DECIMAL(12,2) NOT NULL,
    PRIMARY KEY (OrdenCompraId, RepuestoId),
    KEY IX_DetallesOrdenCompra_RepuestoId (RepuestoId),
    CONSTRAINT CK_DetallesOrdenCompra_Cantidad CHECK (Cantidad > 0),
    CONSTRAINT CK_DetallesOrdenCompra_CostoUnitario CHECK (CostoUnitario >= 0),
    CONSTRAINT FK_DetallesOrdenCompra_OrdenesCompra FOREIGN KEY (OrdenCompraId)
        REFERENCES OrdenesCompra (Id) ON DELETE CASCADE,
    CONSTRAINT FK_DetallesOrdenCompra_Repuestos FOREIGN KEY (RepuestoId)
        REFERENCES Repuestos (Id) ON DELETE RESTRICT
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS MovimientosInventario (
    Id INT NOT NULL AUTO_INCREMENT,
    RepuestoId INT NOT NULL,
    UsuarioId INT NULL,
    TipoMovimiento VARCHAR(20) NOT NULL,
    Cantidad INT NOT NULL,
    FechaMovimiento DATETIME(6) NOT NULL,
    Referencia VARCHAR(200) NULL,
    PRIMARY KEY (Id),
    KEY IX_MovimientosInventario_RepuestoId_FechaMovimiento (RepuestoId, FechaMovimiento),
    KEY IX_MovimientosInventario_UsuarioId (UsuarioId),
    CONSTRAINT CK_MovimientosInventario_Tipo CHECK (TipoMovimiento IN ('ENTRADA', 'SALIDA', 'AJUSTE')),
    CONSTRAINT CK_MovimientosInventario_Cantidad CHECK (Cantidad > 0),
    CONSTRAINT FK_MovimientosInventario_Repuestos FOREIGN KEY (RepuestoId)
        REFERENCES Repuestos (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_MovimientosInventario_Usuarios FOREIGN KEY (UsuarioId)
        REFERENCES Usuarios (Id) ON DELETE SET NULL
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS ServiciosRepuestos (
    ServicioId INT NOT NULL,
    RepuestoId INT NOT NULL,
    Cantidad INT NOT NULL,
    PrecioUnitario DECIMAL(12,2) NOT NULL,
    PRIMARY KEY (ServicioId, RepuestoId),
    KEY IX_ServiciosRepuestos_RepuestoId (RepuestoId),
    CONSTRAINT CK_ServiciosRepuestos_Cantidad CHECK (Cantidad > 0),
    CONSTRAINT CK_ServiciosRepuestos_PrecioUnitario CHECK (PrecioUnitario >= 0),
    CONSTRAINT FK_ServiciosRepuestos_Servicios FOREIGN KEY (ServicioId)
        REFERENCES Servicios (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_ServiciosRepuestos_Repuestos FOREIGN KEY (RepuestoId)
        REFERENCES Repuestos (Id) ON DELETE RESTRICT
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS HistorialEstadosServicio (
    Id INT NOT NULL AUTO_INCREMENT,
    ServicioId INT NOT NULL,
    UsuarioId INT NULL,
    EstadoAnterior VARCHAR(30) NULL,
    EstadoNuevo VARCHAR(30) NOT NULL,
    FechaCambio DATETIME(6) NOT NULL,
    Comentario VARCHAR(500) NULL,
    PRIMARY KEY (Id),
    KEY IX_HistorialEstadosServicio_ServicioId_FechaCambio (ServicioId, FechaCambio),
    KEY IX_HistorialEstadosServicio_UsuarioId (UsuarioId),
    CONSTRAINT FK_HistorialEstadosServicio_Servicios FOREIGN KEY (ServicioId)
        REFERENCES Servicios (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_HistorialEstadosServicio_Usuarios FOREIGN KEY (UsuarioId)
        REFERENCES Usuarios (Id) ON DELETE SET NULL
) ENGINE=InnoDB;
