-- =============================================================
-- Sistema de Gestión de Novedades Laborales y Control Horario
-- Schema de referencia para SQL Server
-- =============================================================
-- NOTA: En la práctica, EF Core genera y mantiene el schema con
-- migrations. Este archivo sirve como referencia y para documentación.

CREATE TABLE Horarios (
    Id INT IDENTITY PRIMARY KEY,
    Nombre NVARCHAR(150) NOT NULL,
    TipoJornada INT NOT NULL,
    DiasLaborables INT NOT NULL,
    HoraEntrada TIME NOT NULL,
    HoraSalida TIME NOT NULL,
    InicioDescanso TIME NULL,
    FinDescanso TIME NULL,
    MinutosMinimosDescanso INT NOT NULL DEFAULT 0,
    ToleranciaEntradaMin INT NOT NULL DEFAULT 5,
    ToleranciaSalidaMin INT NOT NULL DEFAULT 5,
    UmbralHorasExtraMin INT NOT NULL DEFAULT 15,
    BandaInicio TIME NULL,
    BandaFin TIME NULL,
    HorasMinimasDiarias INT NULL
);

CREATE TABLE Empleados (
    Id INT IDENTITY PRIMARY KEY,
    Legajo NVARCHAR(20) NOT NULL UNIQUE,
    Nombre NVARCHAR(100) NOT NULL,
    Apellido NVARCHAR(100) NOT NULL,
    Dni NVARCHAR(15) NOT NULL,
    Cuil NVARCHAR(15) NOT NULL,
    FechaIngreso DATE NOT NULL,
    CategoriaLaboral NVARCHAR(100) NOT NULL,
    ConvenioColectivo NVARCHAR(100) NULL,
    TipoJornada INT NOT NULL,
    Estado INT NOT NULL DEFAULT 1,
    Email NVARCHAR(200) NULL,
    Telefono NVARCHAR(50) NULL,
    HorarioId INT NOT NULL,
    CONSTRAINT FK_Empleados_Horarios FOREIGN KEY (HorarioId) REFERENCES Horarios(Id)
);
CREATE INDEX IX_Empleados_Cuil ON Empleados(Cuil);

CREATE TABLE Usuarios (
    Id INT IDENTITY PRIMARY KEY,
    Email NVARCHAR(200) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    Nombre NVARCHAR(150) NOT NULL,
    Rol INT NOT NULL,
    Activo BIT NOT NULL DEFAULT 1,
    FechaCreacion DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    EmpleadoId INT NULL,
    CONSTRAINT FK_Usuarios_Empleados FOREIGN KEY (EmpleadoId) REFERENCES Empleados(Id) ON DELETE SET NULL
);

CREATE TABLE Fichadas (
    Id BIGINT IDENTITY PRIMARY KEY,
    EmpleadoId INT NOT NULL,
    Timestamp DATETIME2 NOT NULL,
    Tipo INT NOT NULL,
    Origen INT NOT NULL,
    UsuarioRegistroId INT NULL,
    FichadaCorregidaId BIGINT NULL,
    Observacion NVARCHAR(500) NULL,
    FechaInsercion DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Fichadas_Empleados FOREIGN KEY (EmpleadoId) REFERENCES Empleados(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Fichadas_Usuarios FOREIGN KEY (UsuarioRegistroId) REFERENCES Usuarios(Id) ON DELETE SET NULL,
    CONSTRAINT FK_Fichadas_FichadaCorregida FOREIGN KEY (FichadaCorregidaId) REFERENCES Fichadas(Id)
);
CREATE INDEX IX_Fichadas_Empleado_Timestamp ON Fichadas(EmpleadoId, Timestamp);

CREATE TABLE CierresMensuales (
    Id INT IDENTITY PRIMARY KEY,
    Anio INT NOT NULL,
    Mes INT NOT NULL,
    FechaCierre DATETIME2 NOT NULL,
    UsuarioCierreId INT NOT NULL,
    Estado INT NOT NULL DEFAULT 1,
    RutaArchivoExportado NVARCHAR(500) NULL,
    FormatoExportacion NVARCHAR(10) NULL,
    SnapshotJson NVARCHAR(MAX) NULL,
    CONSTRAINT FK_Cierres_Usuarios FOREIGN KEY (UsuarioCierreId) REFERENCES Usuarios(Id),
    CONSTRAINT UQ_Cierres_Periodo UNIQUE (Anio, Mes)
);

CREATE TABLE Novedades (
    Id BIGINT IDENTITY PRIMARY KEY,
    EmpleadoId INT NOT NULL,
    Tipo INT NOT NULL,
    Origen INT NOT NULL,
    Estado INT NOT NULL DEFAULT 1,
    FechaDesde DATE NOT NULL,
    FechaHasta DATE NOT NULL,
    Cantidad DECIMAL(10,2) NOT NULL,
    Observacion NVARCHAR(500) NULL,
    UsuarioCreadorId INT NULL,
    UsuarioRevisorId INT NULL,
    FechaRevision DATETIME2 NULL,
    MotivoRechazo NVARCHAR(500) NULL,
    FechaCreacion DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CierreMensualId INT NULL,
    CONSTRAINT FK_Novedades_Empleados FOREIGN KEY (EmpleadoId) REFERENCES Empleados(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Novedades_Cierres FOREIGN KEY (CierreMensualId) REFERENCES CierresMensuales(Id) ON DELETE SET NULL
);
CREATE INDEX IX_Novedades_Empleado_Fecha ON Novedades(EmpleadoId, FechaDesde);

CREATE TABLE Feriados (
    Id INT IDENTITY PRIMARY KEY,
    Fecha DATE NOT NULL UNIQUE,
    Descripcion NVARCHAR(200) NOT NULL,
    EsNacional BIT NOT NULL DEFAULT 1
);

CREATE TABLE ParametrosEmpresa (
    Id INT IDENTITY PRIMARY KEY,
    RazonSocial NVARCHAR(200) NOT NULL,
    Cuit NVARCHAR(15) NOT NULL,
    EmailContador NVARCHAR(200) NULL,
    DefaultToleranciaEntradaMin INT NOT NULL DEFAULT 5,
    DefaultUmbralHorasExtraMin INT NOT NULL DEFAULT 15,
    VentanaDobleFichadaMin INT NOT NULL DEFAULT 3,
    MultiplicadorExtraDiaHabil DECIMAL(5,2) NOT NULL DEFAULT 1.50,
    MultiplicadorExtraFeriado DECIMAL(5,2) NOT NULL DEFAULT 2.00
);
