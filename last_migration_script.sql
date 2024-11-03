BEGIN TRANSACTION;
GO

CREATE TABLE [VerificationSchedules] (
    [Id] int NOT NULL IDENTITY,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [EmployeeId] int NOT NULL,
    [VerificationStatus] nvarchar(max) NOT NULL,
    [VerificationType] nvarchar(max) NOT NULL,
    [NumberOfAssetsToVerify] int NOT NULL,
    [Notes] nvarchar(max) NOT NULL,
    [StoreId] int NOT NULL,
    CONSTRAINT [PK_VerificationSchedules] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_VerificationSchedules_Stores_StoreId] FOREIGN KEY ([StoreId]) REFERENCES [Stores] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [VerificationRecords] (
    [Id] int NOT NULL IDENTITY,
    [VerificationScheduleId] int NOT NULL,
    [AssetId] int NOT NULL,
    [VerificationDate] datetime2 NOT NULL,
    [Result] nvarchar(50) NOT NULL,
    [Comments] nvarchar(max) NOT NULL,
    [VerifiedBy] nvarchar(max) NOT NULL,
    [ImageUrl] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_VerificationRecords] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_VerificationRecords_Assets_AssetId] FOREIGN KEY ([AssetId]) REFERENCES [Assets] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_VerificationRecords_VerificationSchedules_VerificationScheduleId] FOREIGN KEY ([VerificationScheduleId]) REFERENCES [VerificationSchedules] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [VerificationTeams] (
    [Id] int NOT NULL IDENTITY,
    [VerificationScheduleId] int NOT NULL,
    [EmployeeId] int NOT NULL,
    [Role] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_VerificationTeams] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_VerificationTeams_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_VerificationTeams_VerificationSchedules_VerificationScheduleId] FOREIGN KEY ([VerificationScheduleId]) REFERENCES [VerificationSchedules] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_VerificationRecords_AssetId] ON [VerificationRecords] ([AssetId]);
GO

CREATE INDEX [IX_VerificationRecords_VerificationScheduleId] ON [VerificationRecords] ([VerificationScheduleId]);
GO

CREATE INDEX [IX_VerificationSchedules_StoreId] ON [VerificationSchedules] ([StoreId]);
GO

CREATE INDEX [IX_VerificationTeams_EmployeeId] ON [VerificationTeams] ([EmployeeId]);
GO

CREATE INDEX [IX_VerificationTeams_VerificationScheduleId] ON [VerificationTeams] ([VerificationScheduleId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20241030081731_AddVerificationTablesToDb', N'7.0.5');
GO

COMMIT;
GO

