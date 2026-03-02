CREATE DATABASE ImageCropperDb;

USE ImageCropperDb;

CREATE TABLE Configs (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ScaleDown FLOAT NOT NULL,
    LogoPosition NVARCHAR(50) NOT NULL,
    LogoImage VARBINARY(MAX) NULL
);
