
# graylog-nlog entegrasyonu

proje locale çekilir
  ```  
git clone https://github.com/beyt34/graylog-with-nlog.git  
```  

proje root klasöründen docker compose çalıştırılır
  ```  
 docker-compose up -d
 ```  

dockercompose dosyası  
**projede özellikle default tcp/udp portu 12201 yerine 11101 portu kullanılmıştır.**
```  
# https://docs.graylog.org/en/4.1/pages/installation/docker.html

version: '3'
services:
# MongoDB: https://hub.docker.com/_/mongo/
mongo:
image: mongo:4.2
networks:
 - graylog
# Elasticsearch: https://www.elastic.co/guide/en/elasticsearch/reference/7.10/docker.html
elasticsearch:
image: docker.elastic.co/elasticsearch/elasticsearch:7.15.0
environment:
 - http.host=0.0.0.0
 - transport.host=localhost
 - network.host=0.0.0.0
 - "ES_JAVA_OPTS=-Xms512m -Xmx512m"
ulimits:
memlock:
soft: -1
hard: -1
deploy:
resources:
limits:
memory: 1g
networks:
 - graylog
# Graylog: https://hub.docker.com/r/graylog/graylog/
graylog:
image: graylog/graylog:4.1.5
environment:
# CHANGE ME (must be at least 16 characters)!
 - GRAYLOG_PASSWORD_SECRET=somepasswordpepper
# Password: admin
 - GRAYLOG_ROOT_PASSWORD_SHA2=8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918
 - GRAYLOG_HTTP_EXTERNAL_URI=http://127.0.0.1:9000/
entrypoint: /usr/bin/tini -- wait-for-it elasticsearch:9200 -- /docker-entrypoint.sh
networks:
 - graylog
restart: always
depends_on:
 - mongo
 - elasticsearch
ports:
# Graylog web interface and REST API
 - 9000:9000
# Syslog TCP
 - 1514:1514
# Syslog UDP
 - 1514:1514/udp
# GELF TCP
 - 11101:11101
# GELF UDP
 - 11101:11101/udp
networks:
graylog:
driver: bridge
```  

- localhost:9000 portundan graylog arayüzü açılır
- user - pwd admin ile giriş yapılır
- system / input menüsünden inputs a tıklanır
- gelf http 11101 portu global olarak tanımlanır


sonrasında uygulama çalıştırılır  
graylog ve local file log attığı test edilir

kullanılan nuget paketleri
```  
<PackageReference Include="NLog.Web.AspNetCore" Version="4.14.0" />  
<PackageReference Include="NLog.Gelf" Version="1.1.4" />  
```  

Program.cs deki nlog ayarı
```  
	 public static void Main(string[] args)        {   
            var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();   
            try   
            {   
                logger.Debug("init main");   
                CreateHostBuilder(args).Build().Run();   
            }   
            catch (Exception exception)   
            {   
                //NLog: catch setup errors   
                logger.Error(exception, "Stopped program because of exception");   
                throw;   
            }   
            finally   
            {   
                NLog.LogManager.Shutdown();   
            }   
        }  
  
	private static IHostBuilder CreateHostBuilder(string[] args) =>   
            Host.CreateDefaultBuilder(args)   
                .ConfigureWebHostDefaults(webBuilder =>   
                {   
                    webBuilder.UseStartup<Startup>();   
                })   
                .ConfigureLogging(logging =>   
                {   
                    logging.ClearProviders();   
                    logging.SetMinimumLevel(LogLevel.Trace);   
                })   
                .UseNLog();  // NLog: Setup NLog for Dependency injection  
```  

nlog.config ayarı
```  
<?xml version="1.0" encoding="utf-8" ?> <nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" >   
  <extensions>   
    <!-- https://github.com/mantasaudickas/NLog.Gelf -->   
    <add assembly="NLog.Gelf" />   
  </extensions>   
  <variable name="methodInfo"   
            value="${callsite:className=true:includeNamespace=false:fileName=true:includeSourcePath=false:methodName=true:cleanNamesOfAnonymousDelegates=true:cleanNamesOfAsyncContinuations=true}" />   
  <variable name="layout"   
            value="${longdate}|${pad:padding=4:inner=${threadid}}|${pad:padding=-5:inner=${uppercase:${level}}}|${mdc:item=RequestGuid}|${logger}|${message}${onexception:inner=${newline}${exception:format=tostring}}" />   
  <variable name="logPath"   
            value="${basedir}/Log" />   
  <targets async="true">   
    <target name="AsyncWrapper"   
            xsi:type="AsyncWrapper"   
            overflowAction="Grow"   
            queueLimit="50000"   
            batchSize="500"   
            timeToSleepBetweenBatches="0">   
      <target name="file"   
              xsi:type="File"   
              encoding="utf-8"   
              keepFileOpen="true"   
              openFileCacheTimeout="30"   
              maxArchiveFiles="0"   
              archiveFileName="${logPath}/${shortdate}/${date:format=HHmm}.{#}.log"   
              archiveNumbering="Sequence"   
              archiveAboveSize="10485760"   
              fileName="${logPath}/${shortdate}.log"   
              layout="${layout}" />   
    </target>   
    <target name="GelfHttp"    
            xsi:type="GelfHttp"    
            serverUrl="http://localhost:11101/gelf"    
            facility="GraylogNlog"    
            layout="${layout}"/>   
  </targets>   
  <rules>   
    <logger name="*" minlevel="Trace" writeTo="AsyncWrapper, GelfHttp" />   
</rules> </nlog>  
```  

weatherforecast controller daki leveline göre log atma fonksiyonları
```  
	public WeatherForecastController(ILogger<WeatherForecastController> logger)   
        {   
            _logger = logger;   
            Thread.Sleep(100);   
            _logger.LogTrace("ctor LogTrace...");   
            Thread.Sleep(100);   
            _logger.LogDebug("ctor LogDebug...");   
            Thread.Sleep(100);   
            _logger.LogInformation("ctor LogInformation...");   
            Thread.Sleep(100);   
            _logger.LogWarning("ctor LogWarning...");   
            Thread.Sleep(100);   
            _logger.LogError("ctor LogError...");   
            Thread.Sleep(100);   
            _logger.LogCritical("ctor LogCritical...");   
        }  
```  

detaylı açıklamalar için link;  
https://www.evernote.com/shard/s599/sh/256e8f81-b2b2-97fc-24da-3938c3b02744/587205506f1fbbfce7623a5d2be9abbd