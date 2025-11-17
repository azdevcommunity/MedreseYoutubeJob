# Docker Container'da Logging Sorunları Çözümü

## Yapılan Değişiklikler

### 1. Dockerfile Güncellemeleri
- ✅ `appsettings.json` ve `appsettings.dev.json` container'a kopyalanıyor
- ✅ `logs/` klasörü container'da oluşturuluyor
- ✅ ANSI color redirection aktif edildi
- ✅ Container detection environment variable'ı ayarlandı

### 2. Serilog Konfigürasyonu
- ✅ Console sink'i `standardErrorFromLevel: "Error"` ile yapılandırıldı
- ✅ Program.cs'de Serilog başlatılıyor
- ✅ Startup log eklendi

### 3. Environment Variables

Container'da şu environment variable'ları ayarlayın:

```bash
ASPNETCORE_ENVIRONMENT=Production
DOTNET_SYSTEM_CONSOLE_ALLOW_ANSI_COLOR_REDIRECTION=true
DOTNET_RUNNING_IN_CONTAINER=true
```

## Kontrol Listesi

### Docker Build
```bash
# Yeni image oluştur
docker build -t youtube-api:latest .

# Dockerfile'ı kontrol et
docker build --no-cache -t youtube-api:latest .
```

### Container Çalıştırma
```bash
# Doğru ortam değişkenleriyle çalıştır
docker run -e ASPNETCORE_ENVIRONMENT=Production \
           -e DOTNET_SYSTEM_CONSOLE_ALLOW_ANSI_COLOR_REDIRECTION=true \
           -e DOTNET_RUNNING_IN_CONTAINER=true \
           -p 8085:8085 \
           youtube-api:latest

# Logs'u takip et
docker logs -f <container-id>
```

### Dokploy Konfigürasyonu

Dokploy'da container'ı deploy ederken:

1. **Environment Variables** bölümüne ekle:
   ```
   ASPNETCORE_ENVIRONMENT=Production
   DOTNET_SYSTEM_CONSOLE_ALLOW_ANSI_COLOR_REDIRECTION=true
   DOTNET_RUNNING_IN_CONTAINER=true
   ```

2. **Logs** sekmesinde container loglarını kontrol et

3. **Restart Policy**: `unless-stopped` olarak ayarla

## Debugging

### 1. Container'a Bağlan
```bash
docker exec -it <container-id> /bin/bash
```

### 2. appsettings.json Kontrol Et
```bash
cat /app/appsettings.json
```

### 3. Logs Klasörünü Kontrol Et
```bash
ls -la /app/logs/
cat /app/logs/youtube-api-*.txt
```

### 4. Application Çalışıyor mu?
```bash
curl http://localhost:8085/api/health
```

## Sık Karşılaşılan Sorunlar

### Problem: Loglar görünmüyor
**Çözüm**: 
- `ASPNETCORE_ENVIRONMENT` değişkenini kontrol et
- `appsettings.json` container'da var mı kontrol et
- `docker logs` komutunu kullan

### Problem: Sadece hata logları görünüyor
**Çözüm**:
- `Serilog.MinimumLevel.Default` değerini kontrol et
- `appsettings.json`'da `Information` olarak ayarlanmış mı kontrol et

### Problem: Loglar çok hızlı kayboluyor
**Çözüm**:
- File sink'i etkin hale getir (zaten etkin)
- `docker logs --tail 100` ile son 100 satırı göster

## Dosya Logları

Container'da dosya logları şu konumda saklanır:
```
/app/logs/youtube-api-YYYY-MM-DD.txt
```

Dokploy'da volume mount etmek için:
```
/app/logs -> /data/youtube-api/logs
```

## Test Etme

### 1. Health Check
```bash
curl http://localhost:8085/api/health
```

### 2. Sync Endpoint
```bash
curl -X POST http://localhost:8085/api/youtube/sync
```

### 3. Logs'u Kontrol Et
```bash
docker logs <container-id> | grep "Starting synchronization"
```

## Başarılı Deployment Göstergeleri

✅ Container başlıyor
✅ `docker logs` komutunda loglar görünüyor
✅ `/api/health` endpoint'i 200 döndürüyor
✅ `/app/logs/` klasöründe dosya logları var
✅ Serilog format'ında loglar görünüyor: `[TIMESTAMP] [LEVEL] [SOURCE] MESSAGE`
