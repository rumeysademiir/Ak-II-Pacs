IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Studies') AND name = 'Status')
    ALTER TABLE Studies ADD Status NVARCHAR(50) NOT NULL DEFAULT 'Rapor Bekliyor';
GO
 
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Studies') AND name = 'DicomPatientId')
    ALTER TABLE Studies ADD DicomPatientId NVARCHAR(100) NULL;
GO
 
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Studies') AND name = 'DicomStudyDate')
    ALTER TABLE Studies ADD DicomStudyDate NVARCHAR(50) NULL;
GO
 
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Studies') AND name = 'DicomManufacturer')
    ALTER TABLE Studies ADD DicomManufacturer NVARCHAR(100) NULL;
GO
 
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Studies') AND name = 'Hl7Message')
    ALTER TABLE Studies ADD Hl7Message NVARCHAR(MAX) NULL;
GO
 
-- Var olan eski kayıtlara varsayılan statü ver (artık ayrı bir batch'te, sorun olmaz)
UPDATE Studies SET Status = 'Rapor Bekliyor' WHERE Status IS NULL;
GO