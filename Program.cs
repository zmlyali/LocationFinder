/*
 * LocationHelper.exe
 * Cronoi Teknoloji - Biyometrik İmza Sistemi
 *
 * Windows.Devices.Geolocation API kullanarak cihaz konumunu alır.
 *
 * Çıktı:
 *   Başarı : SUCCESS|<lat>|<lon>|<accuracy_m>|<source>|<altitude_m>
 *   Hata   : ERROR|<kod>|<mesaj>
 *
 * Canias TROIA:
 *   EXECUTECMD '*C:\path\LocationHelper.exe' INTO RESULT
 *   Sonra RESULT parse et: SUCCESS|41.070|28.941|76.000|WiFi|0
 *
 * Exit: 0=başarı  1=izin yok  2=kullanılamıyor  3=timeout  9=hata
 */

using Windows.Devices.Geolocation;

const int TIMEOUT_SEC = 12;

try
{
    // 1. İzin kontrolü
    var access = await Geolocator.RequestAccessAsync();

    if (access == GeolocationAccessStatus.Denied)
    {
        Console.WriteLine("ERROR|PERMISSION_DENIED|Konum izni reddedildi. Windows > Ayarlar > Gizlilik > Konum bölümünden izin verin.");
        return 1;
    }

    if (access == GeolocationAccessStatus.Unspecified)
    {
        Console.WriteLine("ERROR|ACCESS_UNSPECIFIED|Konum erişim durumu belirlenemedi.");
        return 2;
    }

    // 2. Geolocator - yüksek hassasiyet iste
    var geo = new Geolocator
    {
        DesiredAccuracy         = PositionAccuracy.High,
        DesiredAccuracyInMeters = 50
    };

    // 3. Konumu al
    Geoposition pos;
    try
    {
        pos = await geo.GetGeopositionAsync(
            maximumAge : TimeSpan.FromSeconds(10),
            timeout    : TimeSpan.FromSeconds(TIMEOUT_SEC)
        ).AsTask(new CancellationTokenSource(TimeSpan.FromSeconds(TIMEOUT_SEC + 2)).Token);
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine($"ERROR|TIMEOUT|Konum {TIMEOUT_SEC} saniyede alınamadı.");
        return 3;
    }
    catch (Exception ex) when ((uint)ex.HResult is 0x80070005 or 0x800704EC)
    {
        Console.WriteLine("ERROR|PERMISSION_DENIED|Windows Konum Servisi kapalı veya erişim reddedildi.");
        return 1;
    }

    // 4. Verileri çıkart
    var coord    = pos.Coordinate;
    var lat      = coord.Point.Position.Latitude;
    var lon      = coord.Point.Position.Longitude;
    var accuracy = coord.Accuracy;
    var altitude = coord.Point.Position.Altitude;
    var altRef   = coord.Point.AltitudeReferenceSystem;

    // 5. Kaynak tahmini
    string source = accuracy switch
    {
        <= 20  => "GPS",
        <= 100 => "GPS_WiFi",
        <= 500 => "WiFi",
        _      => "Network"
    };

    var altOut = (altRef != AltitudeReferenceSystem.Unspecified && altitude != 0)
        ? altitude.ToString("F1") : "0";

    // 6. Sonuç: SUCCESS|lat|lon|accuracy_m|source|altitude_m
    Console.WriteLine($"SUCCESS|{lat:F8}|{lon:F8}|{accuracy:F3}|{source}|{altOut}");
    return 0;
}
catch (Exception ex)
{
    Console.WriteLine($"ERROR|UNEXPECTED|{ex.GetType().Name}: {ex.Message} (0x{(uint)ex.HResult:X8})");
    return 9;
}
