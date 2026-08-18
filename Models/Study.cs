namespace AkıllıPacs.Models
{
    public class Study
    {
        public int Id { get; set; }

        // Hasta Bilgileri
        public string PatientName { get; set; }
        public string PatientId { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Gender { get; set; }

        // Tetkik Bilgileri
        public string AccessionNumber { get; set; }
        public string StudyDescription { get; set; }
        public string StudyInstanceUID { get; set; }
        public string SeriesInstanceUID { get; set; }
        public string Modality { get; set; }
        public string BodyPart { get; set; }

        // Doktor Bilgileri
        public string ReferringPhysician { get; set; }
        public string Department { get; set; }

        // AI Analizi
        public int QualityScore { get; set; }
        public int PriorityScore { get; set; }
        public string AiResult { get; set; }
        public double Confidence { get; set; }
        public string Recommendation { get; set; }

        // İş Akışı & Raporlama
        public string WorkflowStatus { get; set; }
        public string Status { get; set; }
        public string ReportText { get; set; } = string.Empty; // <-- EKLENEN KRİTİK ALAN

        // Tarihler
        public DateTime CreatedAt { get; set; }
        public DateTime? ReportDate { get; set; }

        // DICOM Metadata
        public string DicomPatientId { get; set; }
        public string DicomStudyDate { get; set; }
        public string DicomManufacturer { get; set; }
        public string DicomInstitution { get; set; }
        public string DicomModelName { get; set; }
        public string DicomSeriesDescription { get; set; }
        public string DicomPixelSpacing { get; set; }
        public string DicomSliceThickness { get; set; }

        // HL7
        public string HL7Message { get; set; }

        // Dosya
        public string ImagePath { get; set; }
        public string ThumbnailPath { get; set; }
        public string ReportPath { get; set; }
    }
}