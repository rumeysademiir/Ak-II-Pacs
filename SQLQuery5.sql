CREATE TABLE dbo.Studies
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    -- Hasta Bilgileri
    PatientName NVARCHAR(100) NOT NULL,
    PatientId NVARCHAR(50),
    BirthDate DATE NULL,
    Gender NVARCHAR(20),

    -- Tetkik Bilgileri
    AccessionNumber NVARCHAR(100),
    StudyDescription NVARCHAR(200),
    StudyInstanceUID NVARCHAR(200),
    SeriesInstanceUID NVARCHAR(200),
    Modality NVARCHAR(20) NOT NULL,
    BodyPart NVARCHAR(100),

    -- Doktor Bilgileri
    ReferringPhysician NVARCHAR(100),
    Department NVARCHAR(100),

    -- AI Analizi
    QualityScore INT,
    PriorityScore INT,
    AiResult NVARCHAR(500),
    Confidence FLOAT,
    Recommendation NVARCHAR(500),

    -- İş Akışı
    WorkflowStatus NVARCHAR(50),
    Status NVARCHAR(50),
    CreatedAt DATETIME DEFAULT(GETDATE()),
    ReportDate DATETIME NULL,

    -- DICOM Metadata
    DicomPatientId NVARCHAR(100),
    DicomStudyDate NVARCHAR(50),
    DicomManufacturer NVARCHAR(100),
    DicomInstitution NVARCHAR(100),
    DicomModelName NVARCHAR(100),
    DicomSeriesDescription NVARCHAR(200),
    DicomPixelSpacing NVARCHAR(50),
    DicomSliceThickness NVARCHAR(50),

    -- HL7
    HL7Message NVARCHAR(MAX),

    -- Dosyalar
    ImagePath NVARCHAR(300),
    ThumbnailPath NVARCHAR(300),
    ReportPath NVARCHAR(300)
);