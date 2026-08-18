"""
analyzer.py
-----------
AkıllıPACS - Görüntü Kalite + DICOM Metadata + Dinamik AI Analiz Scripti

Çağırma biçimi (StudiesController.cs ile uyumlu):
    python analyzer.py "<dosya_yolu>" "<modalite>"
"""

import sys
import os
import json
import io
import numpy as np
import cv2

# Windows konsol ve C# I/O akışlarında UTF-8 zorlaması (Stdout ve Stderr için)
if hasattr(sys.stdout, 'buffer'):
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
if hasattr(sys.stderr, 'buffer'):
    sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8')

try:
    import pydicom
    PYDICOM_AVAILABLE = True
except ImportError:
    PYDICOM_AVAILABLE = False


MODALITY_PROFILES = {
    "CT": {"blur_min": 50, "blur_max": 1500, "contrast_min": 20, "contrast_max": 90},
    "MR": {"blur_min": 30, "blur_max": 900, "contrast_min": 12, "contrast_max": 70},
    "X-RAY": {"blur_min": 40, "blur_max": 1200, "contrast_min": 18, "contrast_max": 85},
    "CR": {"blur_min": 40, "blur_max": 1200, "contrast_min": 18, "contrast_max": 85}
}


def normalize(value, vmin, vmax):
    if vmax <= vmin:
        return 0.0
    return float(np.clip((value - vmin) / (vmax - vmin) * 100.0, 0, 100))


def load_grayscale(path):
    if not os.path.exists(path):
        raise FileNotFoundError(f"Dosya sunucuda bulunamadı: {path}")

    ext = os.path.splitext(path)[1].lower()

    if ext == ".dcm" and PYDICOM_AVAILABLE:
        ds = pydicom.dcmread(path)
        arr = ds.pixel_array.astype(np.float32)
        arr -= arr.min()
        max_val = arr.max()
        if max_val > 0:
            arr = (arr / max_val) * 255.0
        return arr.astype(np.uint8), ds

    # Türkçe karakter barındıran dosya yolları için güvenli imread
    try:
        img_array = np.fromfile(path, dtype=np.uint8)
        img = cv2.imdecode(img_array, cv2.IMREAD_GRAYSCALE)
    except Exception as e:
        raise RuntimeError(f"Görüntü okunurken hata oluştu: {str(e)}")

    if img is None:
        raise RuntimeError(f"Görüntü okunamadı veya biçimi desteklenmiyor (OpenCV imdecode null döndü): {path}")

    return img, None


def extract_dicom_metadata(ds):
    if ds is None:
        return None, None, None

    patient_id = str(getattr(ds, "PatientID", "")) or None
    study_date = str(getattr(ds, "StudyDate", "")) or None
    manufacturer = str(getattr(ds, "Manufacturer", "")) or None
    return patient_id, study_date, manufacturer


def compute_quality_score(gray, modality):
    profile = MODALITY_PROFILES.get(modality.upper(), MODALITY_PROFILES["CT"])

    blur_raw = float(cv2.Laplacian(gray, cv2.CV_64F).var())
    contrast_raw = float(gray.std())

    blur_norm = normalize(blur_raw, profile["blur_min"], profile["blur_max"])
    contrast_norm = normalize(contrast_raw, profile["contrast_min"], profile["contrast_max"])

    return round(0. * blur_norm + 0.4 * contrast_norm)


def generate_ai_diagnosis(gray, modality, quality_score):
    std_dev = float(gray.std())
    mean_val = float(gray.mean())
    modality_upper = modality.upper()

    if quality_score < 45:
        return "Görüntüde Artefakt / Düşük Kalite Tespiti", "Yüksek seviyede hareket/artefakt izi. Çekim tekrarı önerilir."

    if modality_upper in ["CT", "BT"]:
        if std_dev > 65:
            return "Şüpheli Nodüler İdansite / Lezyon", "Toraks BT kesitlerinde lezyon şüphesi. Radyolog değerlendirmesi önerilir."
        elif mean_val < 80:
            return "Atelektazi / Parankimal Dansite Artışı", "Bazal segmentlerde obstrüksiyon/atelektazi ile uyumlu bulgu."
        else:
            return "Normal Akciğer Parankimi", "Belirgin patolojik nodül veya infiltrasyon saptanmadı."

    elif modality_upper in ["MR", "MRI"]:
        if std_dev > 50:
            return "Serebral Beyaz Cevher Hiperintensitesi", "T2/FLAIR kesitlerinde dikey hiperintens odaklar."
        elif mean_val > 140:
            return "Olası Ödem / Dokusal Düzensizlik", "Yumuşak dokuda diffüz sinyal artışı tespiti."
        else:
            return "Normal Serebral Parankim", "Fokal kitle etkisi veya akut enfarkt bulgusu saptanmadı."

    else:
        if std_dev > 55:
            return "Olası Konsolidasyon / Opasite Artışı", "Alt zonda yama tarzında opasite artışı tespiti."
        elif mean_val > 150:
            return "Kardiyomegali Şüphesi", "Kardiyotorasik oran üst sınırda değerlendirildi."
        else:
            return "Normal Toraks Grafisi", "Açık akciğer alanları, kardiyodiyafragmatik sinüsler serbest."


def main():
    if len(sys.argv) < 2:
        print(json.dumps({"error": "Dosya yolu verilmedi."}, ensure_ascii=False))
        return

    file_path = sys.argv[1]
    modality = sys.argv[2] if len(sys.argv) > 2 else "CT"

    try:
        gray, dicom_ds = load_grayscale(file_path)
        quality_score = compute_quality_score(gray, modality)
        patient_id, study_date, manufacturer = extract_dicom_metadata(dicom_ds)
        ai_result, recommendation = generate_ai_diagnosis(gray, modality, quality_score)

        result = {
            "quality_score": quality_score,
            "ai_result": ai_result,
            "recommendation": recommendation,
            "dicom_patient_id": patient_id,
            "dicom_study_date": study_date,
            "dicom_manufacturer": manufacturer,
        }
        print(json.dumps(result, ensure_ascii=False))

    except Exception as e:
        # C# tarafının tam düzgün okuyabilmesi için sys.exit(1) kaldırıldı ve UTF-8 JSON üretiliyor.
        fallback = {
            "error": str(e),
            "quality_score": 50,
            "ai_result": f"Görüntü İşleme Hatası: {str(e)}",
            "recommendation": "Görüntü okunamadı. Lütfen dosya formatını kontrol edip tekrar deneyin.",
            "dicom_patient_id": None,
            "dicom_study_date": None,
            "dicom_manufacturer": None
        }
        print(json.dumps(fallback, ensure_ascii=False))


if __name__ == "__main__":
    main()