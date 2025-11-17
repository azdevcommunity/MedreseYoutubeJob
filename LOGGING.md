# Logging Configuration

Bu proje **Serilog** kullanarak kapsamlı logging sağlar.

## Özellikler

- **Console Output**: Gerçek zamanlı log çıktısı
- **File Logging**: Günlük dosyalara otomatik kayıt (30 gün tutulur)
- **Structured Logging**: Yapılandırılmış log verisi
- **Context Enrichment**: Machine name, thread ID, timestamp gibi bilgiler otomatik eklenir
- **Environment-specific Configuration**: Development ve Production ortamları için farklı ayarlar

## Log Seviyeleri

- **Debug**: Detaylı bilgiler (Development ortamında aktif)
- **Information**: Genel bilgiler (varsayılan)
- **Warning**: Uyarılar
- **Error**: Hatalar
- **Fatal**: Kritik hatalar

## Dosya Konumları

Log dosyaları `logs/` klasöründe saklanır:
- **Development**: `logs/youtube-api-dev-YYYY-MM-DD.txt`
- **Production**: `logs/youtube-api-YYYY-MM-DD.txt`

## Konfigürasyon

### appsettings.json (Production)
```json
"Serilog": {
  "MinimumLevel": {
    "Default": "Information",
    "Override": {
      "Microsoft": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

### appsettings.dev.json (Development)
```json
"Serilog": {
  "MinimumLevel": {
    "Default": "Debug",
    "Override": {
      "Microsoft": "Warning",
      "Microsoft.EntityFrameworkCore": "Debug"
    }
  }
}
```

## Log Format

```
[2025-11-17 14:30:45.123 +03:00] [INF] [YoutubeApiSynchronize.Services.YoutubeService] Starting synchronization process...
```

Format: `[Timestamp] [Level] [SourceContext] Message`

## Grafana Loki Integration (Opsiyonel)

Grafana Loki'ye log göndermek için `LoggingConfig.cs` dosyasında ilgili kodu aktif hale getirin:

```csharp
if (!string.IsNullOrEmpty(logOptions.Value.Loki))
{
    configuration.WriteTo.GrafanaLoki(logOptions.Value.Loki, [...]);
}
```

## Kullanım Örnekleri

### Service'te Logging
```csharp
logger.Information("Starting synchronization process...");
logger.Debug("Fetched {Count} playlists", playlists.Count);
logger.Error(ex, "Error during synchronization");
```

### Controller'da Logging
```csharp
_logger.Information("YouTube synchronization started via API endpoint");
_logger.Warning("Rate limit exceeded during synchronization");
_logger.Error(ex, "Error during YouTube synchronization");
```

## Enrichment Properties

Her log otomatik olarak şu bilgileri içerir:
- `Application`: "YoutubeApiSynchronize"
- `Environment`: Çalışma ortamı (Development/Production)
- `MachineName`: Sunucu adı
- `ThreadId`: Thread ID
- `Timestamp`: Zaman damgası

## Best Practices

1. **Structured Logging Kullanın**: String interpolation yerine parametreli logging kullanın
   ```csharp
   // ✓ Doğru
   logger.Information("Video {VideoId} processed", videoId);
   
   // ✗ Yanlış
   logger.Information($"Video {videoId} processed");
   ```

2. **Uygun Log Seviyesi Seçin**:
   - `Debug`: Geliştirme sırasında detaylı bilgiler
   - `Information`: Önemli işlem adımları
   - `Warning`: Beklenmeyen ancak işlenebilir durumlar
   - `Error`: Hata durumları

3. **Exception Logging**: Hata loglarken exception'ı da ekleyin
   ```csharp
   logger.Error(ex, "Error processing video {VideoId}", videoId);
   ```

## Paketler

- `Serilog` (4.2.0)
- `Serilog.AspNetCore` (8.0.1)
- `Serilog.Extensions.Hosting` (8.0.0)
- `Serilog.Sinks.Console` (6.0.0)
- `Serilog.Sinks.File` (5.0.0)
- `Serilog.Settings.Configuration` (8.0.1)
- `Serilog.Sinks.Grafana.Loki` (8.3.0) - Opsiyonel
