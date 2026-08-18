IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Studies') AND name = 'PatientName')
    ALTER TABLE Studies ADD PatientName NVARCHAR(100) NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Studies') AND name = 'Modality')
    ALTER TABLE Studies ADD Modality NVARCHAR(20) NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Studies') AND name = 'ImagePath')
    ALTER TABLE Studies ADD ImagePath NVARCHAR(255) NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Studies') AND name = 'QualityScore')
    ALTER TABLE Studies ADD QualityScore INT NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Studies') AND name = 'PriorityScore')
    ALTER TABLE Studies ADD PriorityScore INT NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Studies') AND name = 'CreatedAt')
    ALTER TABLE Studies ADD CreatedAt DATETIME DEFAULT GETDATE();
