using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using AkıllıPacs.Models;
using System.Diagnostics;
using System.Text.Json;
using System.Text;

namespace AkıllıPacs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudiesController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public StudiesController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        }

        // --- 1. VERİTABANINDAKİ TÜM KAYITLARI ÇEKEN GET METODU (Worklist) ---
        [HttpGet]
        public async Task<IActionResult> GetStudies()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    var sql = @"SELECT 
                                    Id, 
                                    PatientName, 
                                    PatientId, 
                                    AccessionNumber, 
                                    StudyDescription, 
                                    Modality, 
                                    ImagePath, 
                                    QualityScore, 
                                    PriorityScore, 
                                    WorkflowStatus, 
                                    Status, 
                                    CreatedAt, 
                                    DicomPatientId, 
                                    DicomStudyDate, 
                                    DicomManufacturer, 
                                    HL7Message,
                                    AiResult,
                                    Confidence,
                                    Recommendation,
                                    BirthDate,
                                    Gender,
                                    BodyPart,
                                    Department,
                                    ReferringPhysician,
                                    DicomInstitution,
                                    DicomModelName,
                                    DicomSeriesDescription,
                                    DicomPixelSpacing,
                                    DicomSliceThickness,
                                    ReportText,
                                    ReportDate,
                                    StudyInstanceUID,
                                    SeriesInstanceUID,
                                    ThumbnailPath,
                                    ReportPath
                                FROM Studies 
                                ORDER BY CreatedAt DESC";

                    var studies = await connection.QueryAsync<Study>(sql);
                    return Ok(studies);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Veritabanından okuma hatası: {ex.Message}");
            }
        }

        // --- 2. YENİ ANALİZ EKLEYEN POST METODU ---
        [HttpPost]
        public async Task<IActionResult> CreateStudy([FromForm] string patientName, [FromForm] string modality, IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return BadRequest("Lütfen geçerli bir görüntü dosyası yükleyin.");

            try
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pacs_images");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                // Python analiz scriptini çalıştır ve sonuçları al
                var analysis = await AnalyzeImageAsync(filePath, modality);
                int priorityScore = analysis.QualityScore < 70 ? 90 : 30;

                var newStudy = new Study
                {
                    PatientName = patientName,
                    PatientId = $"P{Random.Shared.Next(100000, 999999)}",
                    AccessionNumber = $"ACC-{DateTime.Now:yyyyMMddHHmmss}",
                    StudyDescription = $"{modality} Thorax",
                    BodyPart = "Thorax",
                    ReferringPhysician = "Dr. Mehmet Kaya",
                    Department = "Radyoloji",
                    AiResult = !string.IsNullOrEmpty(analysis.AiResult) ? analysis.AiResult : "Normal/Belirgin Bulgu Saptanmadı",
                    Confidence = Math.Round(0.80 + Random.Shared.NextDouble() * 0.19, 2),
                    Recommendation = !string.IsNullOrEmpty(analysis.Recommendation) ? analysis.Recommendation : "Rutin raporlama",
                    WorkflowStatus = "AI Analysis",
                    Modality = modality.ToUpper(),
                    ImagePath = $"pacs_images/{uniqueFileName}",
                    QualityScore = analysis.QualityScore,
                    PriorityScore = priorityScore,
                    CreatedAt = DateTime.Now,
                    Status = "Rapor Bekliyor",
                    ReportText = "",
                    DicomPatientId = analysis.DicomPatientId,
                    DicomStudyDate = analysis.DicomStudyDate,
                    DicomManufacturer = analysis.DicomManufacturer,
                    BirthDate = new DateTime(1990, 1, 1),
                    Gender = "F",
                    DicomInstitution = "Akıllı PACS",
                    DicomModelName = "Virtual Scanner",
                    DicomSeriesDescription = $"{modality} Series",
                    DicomPixelSpacing = @"0.50\0.50",
                    DicomSliceThickness = "1.0 mm",
                    StudyInstanceUID = Guid.NewGuid().ToString(),
                    SeriesInstanceUID = Guid.NewGuid().ToString(),
                    ThumbnailPath = "",
                    ReportPath = ""
                };

                using (var connection = GetConnection())
                {
                    var sqlInsert = @"INSERT INTO Studies
                                    (
                                        PatientName,
                                        PatientId,
                                        AccessionNumber,
                                        StudyDescription,
                                        Modality,
                                        ImagePath,
                                        QualityScore,
                                        PriorityScore,
                                        WorkflowStatus,
                                        Status,
                                        CreatedAt,
                                        DicomPatientId,
                                        DicomStudyDate,
                                        DicomManufacturer,
                                        AiResult,
                                        Confidence,
                                        Recommendation,
                                        BirthDate,
                                        Gender,
                                        BodyPart,
                                        Department,
                                        ReferringPhysician,
                                        DicomInstitution,
                                        DicomModelName,
                                        DicomSeriesDescription,
                                        DicomPixelSpacing,
                                        DicomSliceThickness,
                                        ReportText,
                                        StudyInstanceUID,
                                        SeriesInstanceUID,
                                        ThumbnailPath,
                                        ReportPath
                                    ) 
                                    VALUES 
                                    (
                                        @PatientName,
                                        @PatientId,
                                        @AccessionNumber,
                                        @StudyDescription,
                                        @Modality,
                                        @ImagePath,
                                        @QualityScore,
                                        @PriorityScore,
                                        @WorkflowStatus,
                                        @Status,
                                        @CreatedAt,
                                        @DicomPatientId,
                                        @DicomStudyDate,
                                        @DicomManufacturer,
                                        @AiResult,
                                        @Confidence,
                                        @Recommendation,
                                        @BirthDate,
                                        @Gender,
                                        @BodyPart,
                                        @Department,
                                        @ReferringPhysician,
                                        @DicomInstitution,
                                        @DicomModelName,
                                        @DicomSeriesDescription,
                                        @DicomPixelSpacing,
                                        @DicomSliceThickness,
                                        @ReportText,
                                        @StudyInstanceUID,
                                        @SeriesInstanceUID,
                                        @ThumbnailPath,
                                        @ReportPath
                                    );
                                    SELECT CAST(SCOPE_IDENTITY() as int);";

                    var insertedId = await connection.QuerySingleAsync<int>(sqlInsert, newStudy);
                    newStudy.Id = insertedId;

                    newStudy.HL7Message = BuildHl7Message(newStudy);
                    LogHl7Message(newStudy.HL7Message);

                    var sqlUpdateHl7 = @"UPDATE Studies SET HL7Message = @HL7Message WHERE Id = @Id";
                    await connection.ExecuteAsync(sqlUpdateHl7, new { HL7Message = newStudy.HL7Message, Id = newStudy.Id });
                }

                string priorityStatus = priorityScore > 50 ? "Yüksek Öncelik" : "Normal";

                return Ok(new
                {
                    message = "Görüntü başarıyla yüklendi ve kaydedildi.",
                    qualityScore = newStudy.QualityScore,
                    priorityStatus = priorityStatus,
                    study = newStudy
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Sistemsel hata: {ex.Message}");
            }
        }

        // --- 3. RAPOR METNİ VE STATÜSÜNÜ GÜNCELLEYEN DİNAMİK ENDPOINT (Taslak & Onay) ---
        [HttpPost("{id}/update-report")]
        public async Task<IActionResult> UpdateReport(int id, [FromBody] UpdateReportDto model)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    var sql = @"UPDATE Studies 
                                SET ReportText = @ReportText, 
                                    Status = @Status, 
                                    WorkflowStatus = CASE WHEN @Status = 'Rapor Tamamlandı' THEN 'Reported' ELSE 'Draft' END,
                                    ReportDate = GETDATE()
                                WHERE Id = @Id";

                    var affected = await connection.ExecuteAsync(sql, new
                    {
                        Id = id,
                        ReportText = model.ReportText,
                        Status = model.Status
                    });

                    if (affected == 0)
                        return NotFound($"Study Id {id} bulunamadı.");

                    return Ok(new { message = "Rapor ve statü güncellendi.", studyId = id, status = model.Status });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Sistemsel hata: {ex.Message}");
            }
        }

        // --- 4. HIZLI RAPORU TAMAMLANDI OLARAK İŞARETLEYEN BUTON ENDPOINT'İ ---
        [HttpPost("{id}/complete-report")]
        public async Task<IActionResult> CompleteReport(int id)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    var sql = @"UPDATE Studies 
                                SET Status = 'Rapor Tamamlandı', 
                                    WorkflowStatus = 'Reported',
                                    ReportDate = GETDATE()
                                WHERE Id = @Id";

                    var affected = await connection.ExecuteAsync(sql, new { Id = id });

                    if (affected == 0)
                        return NotFound($"Study Id {id} bulunamadı.");

                    return Ok(new { message = "Rapor durumu güncellendi.", studyId = id, status = "Rapor Tamamlandı", workflowStatus = "Reported" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Sistemsel hata: {ex.Message}");
            }
        }

        // --- 5. RADYOLOG NOTUNU KAYDEDEN ENDPOINT ---
        [HttpPost("{id}/save-note")]
        public async Task<IActionResult> SaveNote(int id, [FromBody] SaveNoteDto model)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    var sql = @"UPDATE Studies 
                                SET ReportText = @Note, 
                                    ReportDate = GETDATE()
                                WHERE Id = @Id";

                    var affected = await connection.ExecuteAsync(sql, new
                    {
                        Id = id,
                        Note = model.Note
                    });

                    if (affected == 0)
                        return NotFound($"Study Id {id} bulunamadı.");

                    return Ok(new { message = "Radyolog notu başarıyla kaydedildi.", studyId = id });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Sistemsel hata: {ex.Message}");
            }
        }

        // --- 6. INTERPACS WADO-RS: Dış Sistemlerin Görüntü Çekmesi İçin Endpoint ---
        [HttpGet("wado-rs/studies/{studyInstanceUid}")]
        public async Task<IActionResult> GetStudyByUid(string studyInstanceUid)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    var sql = "SELECT * FROM Studies WHERE StudyInstanceUID = @UID";
                    var study = await connection.QueryFirstOrDefaultAsync<Study>(sql, new { UID = studyInstanceUid });

                    if (study == null)
                        return NotFound(new { error = "WADO-RS: Belirtilen StudyInstanceUID ile eşleşen tetkik bulunamadı." });

                    return Ok(new
                    {
                        DICOMweb = "WADO-RS 1.0",
                        StudyInstanceUID = study.StudyInstanceUID,
                        PatientName = study.PatientName,
                        PatientID = study.DicomPatientId ?? study.PatientId,
                        Modality = study.Modality,
                        StudyDate = study.CreatedAt.ToString("yyyyMMdd"),
                        RetrieveURL = $"{Request.Scheme}://{Request.Host}/{study.ImagePath}",
                        QualityScore = study.QualityScore,
                        AiResult = study.AiResult
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"WADO-RS Hatası: {ex.Message}");
            }
        }

        // --- Python analiz scriptini çalıştırıp kalite skoru + AI teşhis sonucu + DICOM metadata döndürür ---
        private async Task<AnalysisResult> AnalyzeImageAsync(string filePath, string modality)
        {
            string pythonScriptPath = Path.Combine(Directory.GetCurrentDirectory(), "analyzer.py");

            if (System.IO.File.Exists(pythonScriptPath))
            {
                try
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = "python",
                        Arguments = $"\"{pythonScriptPath}\" \"{filePath}\" \"{modality}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        StandardOutputEncoding = Encoding.UTF8,
                        StandardErrorEncoding = Encoding.UTF8,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (var process = Process.Start(startInfo))
                    {
                        if (process != null)
                        {
                            string resultJson = await process.StandardOutput.ReadToEndAsync();
                            string stderr = await process.StandardError.ReadToEndAsync();
                            await process.WaitForExitAsync();

                            if (!string.IsNullOrEmpty(stderr))
                            {
                                Console.WriteLine("PYTHON ERR: " + stderr);
                            }

                            if (!string.IsNullOrWhiteSpace(resultJson) && resultJson.Contains("quality_score"))
                            {
                                using (JsonDocument doc = JsonDocument.Parse(resultJson))
                                {
                                    var root = doc.RootElement;
                                    int score = root.TryGetProperty("quality_score", out var s) ? s.GetInt32() : 50;

                                    string? patientId = root.TryGetProperty("dicom_patient_id", out var p) && p.ValueKind != JsonValueKind.Null
                                        ? p.GetString() : null;
                                    string? studyDate = root.TryGetProperty("dicom_study_date", out var d) && d.ValueKind != JsonValueKind.Null
                                        ? d.GetString() : null;
                                    string? manufacturer = root.TryGetProperty("dicom_manufacturer", out var m) && m.ValueKind != JsonValueKind.Null
                                        ? m.GetString() : null;

                                    string? aiResult = root.TryGetProperty("ai_result", out var ai) && ai.ValueKind != JsonValueKind.Null
                                        ? ai.GetString() : null;
                                    string? recommendation = root.TryGetProperty("recommendation", out var rec) && rec.ValueKind != JsonValueKind.Null
                                        ? rec.GetString() : null;

                                    return new AnalysisResult(score, patientId, studyDate, manufacturer, aiResult, recommendation);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("C# Process Exec Error: " + ex.Message);
                }
            }

            byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(fileBytes);
                int hashInt = BitConverter.ToInt32(hash, 0);
                int fallbackScore = 45 + (Math.Abs(hashInt) % 51);
                return new AnalysisResult(fallbackScore, null, null, null, "Normal / Genel Görünüm", "Rutin raporlama");
            }
        }

        private string BuildHl7Message(Study study)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var sb = new StringBuilder();

            sb.Append($"MSH|^~\\&|AKILLIPACS|RADYOLOJI|RIS|HASTANE|{timestamp}||ORU^R01|MSG{study.Id:D6}|P|2.3\r");
            sb.Append($"PID|1||{study.DicomPatientId ?? "BILINMIYOR"}||{study.PatientName}\r");
            sb.Append($"OBR|1||{study.Id}|{study.Modality}^Görüntüleme|||{timestamp}\r");
            sb.Append($"OBX|1|NM|KALITE_SKORU||{study.QualityScore}|puan|||||F\r");
            sb.Append($"OBX|2|NM|ONCELIK_SKORU||{study.PriorityScore}|puan|||||F\r");

            return sb.ToString();
        }

        private void LogHl7Message(string message)
        {
            try
            {
                var logFolder = Path.Combine(Directory.GetCurrentDirectory(), "App_Data");
                if (!Directory.Exists(logFolder))
                    Directory.CreateDirectory(logFolder);

                var logPath = Path.Combine(logFolder, "hl7_log.txt");
                System.IO.File.AppendAllText(logPath, message + "\r\n---\r\n");
            }
            catch { }
        }

        private record AnalysisResult(
            int QualityScore,
            string? DicomPatientId,
            string? DicomStudyDate,
            string? DicomManufacturer,
            string? AiResult,
            string? Recommendation
        );
    }

    public class UpdateReportDto
    {
        public string ReportText { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class SaveNoteDto
    {
        public string Note { get; set; } = string.Empty;
    }
}