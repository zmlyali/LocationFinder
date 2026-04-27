# LocationHelper.exe
**Cronoi Teknoloji — Biyometrik İmza Sistemi**

Windows Location API'sini kullanarak cihaz konumunu alır ve Canias TROIA'ya iletir.

---

## Build (Windows Developer PC'de)

```
dotnet publish -r win-x64 -c Release
```

Çıktı: `bin\Release\net8.0-windows10.0.19041.0\win-x64\publish\LocationHelper.exe`

Gereksinimler:
- .NET 8 SDK  →  https://dotnet.microsoft.com/download/dotnet/8
- Windows 10+ hedef (crosscompile için EnableWindowsTargeting=true zaten ayarlı)

---

## Çıktı Formatı

```
SUCCESS|41.07030212|28.94183412|76.000|WiFi|0
```

| Alan       | Açıklama                               |
|------------|----------------------------------------|
| SUCCESS    | Durum (SUCCESS veya ERROR)             |
| lat        | Enlem (8 ondalık)                      |
| lon        | Boylam (8 ondalık)                     |
| accuracy_m | Doğruluk (metre)                       |
| source     | GPS / GPS_WiFi / WiFi / Network        |
| altitude_m | Rakım metre (0 = bilinmiyor)           |

Hata örneği:
```
ERROR|PERMISSION_DENIED|Konum izni reddedildi...
```

Exit kodları: `0`=başarı `1`=izin yok `2`=kullanılamıyor `3`=timeout `9`=hata

---

## Canias TROIA Entegrasyonu

```troia
OBJECT: STRING LOCRESULT, STRING LOCSTATUS, STRING LOCLAT, STRING LOCLON,
        STRING LOCACC, STRING LOCSRC, STRING LOCALT;

/* LocationHelper.exe'yi çalıştır */
EXECUTECMD '*C:\CANIAS\Tools\LocationHelper.exe' INTO LOCRESULT;

/* Parse et: SUCCESS|lat|lon|accuracy|source|altitude */
LOCLAT  = TOKEN(LOCRESULT, '|', 2);
LOCLON  = TOKEN(LOCRESULT, '|', 3);
LOCACC  = TOKEN(LOCRESULT, '|', 4);
LOCSRC  = TOKEN(LOCRESULT, '|', 5);
LOCALT  = TOKEN(LOCRESULT, '|', 6);
LOCSTATUS = TOKEN(LOCRESULT, '|', 1);

IF LOCSTATUS == 'SUCCESS' THEN
    /* JSON'a ekle veya DB'ye yaz */
    DUMP LOCLAT;
    DUMP LOCLON;
ELSE
    DUMP LOCRESULT;   /* Hata mesajını logla */
ENDIF;
```

---

## Yerleştirme

`LocationHelper.exe` dosyasını şu konumlardan birine koyun:
- `C:\CANIAS\Tools\LocationHelper.exe`
- veya Canias sistem PATH'inde herhangi bir klasör

---

## Windows Konum Servisi

Programın çalışması için:
1. **Ayarlar → Gizlilik ve Güvenlik → Konum**
2. "Konum hizmetleri" → **Açık**
3. "Masaüstü uygulamalarının konumunuza erişmesine izin ver" → **Açık**
