# Fazendo livestream usando o FFMPEG no RaspnerryPi 5

## 1. Preparação da Imagem

Prepare o cartão SD usando o Raspberry Pi Imager
## 1. Instalando o FFmpeg no Raspberry Pi

O Raspberry Pi OS já possui o FFmpeg nos seus repositórios oficiais. Abra o terminal e execute:

```Bash
sudo apt update
sudo apt install ffmpeg -y
```

Para verificar se deu certo e ver se ele suporta os encoders de hardware do Pi (como o `h264_v4l2m2m`), digite: 
```Bash
ffmpeg -encoders | grep h264
```
#### Instalar o NGINX
```Bash
sudo apt update 
sudo apt install nginx libnginx-mod-rtmp -y
```
### Preparar a pasta e permissões

O Nginx precisa de um lugar para salvar os pedacinhos do vídeo:
```
# Criar a pasta do stream
sudo mkdir -p /var/www/html/stream
sudo chown -R www-data:www-data /var/www/html/stream
sudo chmod -R 755 /var/www/html/stream

# Adicionar seu usuário aos grupos de hardware
sudo usermod -aG video,audio principal
```
### Configurar o Nginx (O arquivo `/etc/nginx/nginx.conf`)

Abra o arquivo e atualize o bloco `rtmp` que criamos antes, substitua todo o texto pelo texto abaixo:

sudo nano /etc/nginx/nginx.conf

```
user www-data;
worker_processes auto;
pid /run/nginx.pid;
include /etc/nginx/modules-enabled/*.conf;

events {
    worker_connections 768;
    # multi_accept on;
}

rtmp {
    server {
        listen 1935;
        chunk_size 4096;

        application live {
            live on;
            record off;

            # Configurações de HLS
            hls on;
            hls_path /var/www/html/stream;
            hls_fragment 1;
            hls_playlist_length  30;
            hls_cleanup on;
            hls_variant _low BANDWIDTH=500000;
        }
    }
}

http {
    include /etc/nginx/mime.types;
    default_type application/octet-stream;

    sendfile on;
    tcp_nopush on;
    tcp_nodelay on;
    keepalive_timeout 65;
    types_hash_max_size 2048;

    server {
        listen 80;
        server_name localhost;

        # Site principal
        location / {
            root /var/www/html;
            index index.html;
        }

        # Entrega dos arquivos do Streaming (HLS)
        location /stream {
            alias /var/www/html/stream;
            
            # Configurações para evitar erro de CORS em players web
            add_header 'Access-Control-Allow-Origin' '*' always;
            add_header 'Cache-Control' 'no-cache';
            
            # Tipos de arquivos HLS
            types {
                application/vnd.apple.mpegurl m3u8;
                video/mp2t ts;
            }
        }
    }
}

```
_Dica: 
Para selecionar o texto aperte Alt + A, mova o cursor até onde deseka excluir e pressione Ctrl+k
Para salvar no Nano, aperte `Ctrl + O`, `Enter` e depois `Ctrl + X`._

Reinicie o serviço do Nginx:
```
sudo systemctl restart nginx
```
### Criar a "Página Web" para assistir

Navegadores precisam de um "player" de vídeo. Vamos criar um arquivo HTML simples usando o player HLS.JS.

Crie o arquivo: `
sudo nano /var/www/html/index.html` 

E cole este código:
```HTML
<html>
<body>
  <video id="video" controls style="width: 100%;"></video>

  <script src="https://cdn.jsdelivr.net/npm/hls.js@latest"></script>
  <script>
   var video = document.getElementById('video');

   if (video.requestFullscreen) {
        video.requestFullscreen();
    } else if (video.webkitRequestFullscreen) { /* Safari / iOS */
        video.webkitRequestFullscreen();
    } else if (video.msRequestFullscreen) { /* IE11 / Edge antigo */
        video.msRequestFullscreen();
    }
    
  var videoSrc = 'stream/tela.m3u8'; 

  if (Hls.isSupported()) {
    // Para Android/Chrome
    var hls = new Hls();
    hls.loadSource(videoSrc);
    hls.attachMedia(video);
    hls.on(Hls.Events.MANIFEST_PARSED, function() {
      video.play();
    });
  } 
  else if (video.canPlayType('application/vnd.apple.mpegurl')) {
    // Para iOS/Safari Nativo
    video.src = videoSrc;
    video.addEventListener('loadedmetadata', function() {
      video.play();
    });
  }
  </script>
</body>
</html>
```


Se precisar listar os arquivos use:
ls -lh /var/www/html/stream
#### Rodar o FFmpeg via linha de comando

```Bash
ffmpeg -loglevel warning -f v4l2 -input_format mjpeg -video_size 1280x720 -framerate 30 -i /dev/video0 -f alsa -channels 1 -i hw:2,0 -c:v libx264 -preset ultrafast -tune zerolatency -pix_fmt yuv420p -b:v 2M -g 30 -keyint_min 30 -force_key_frames "expr:gte(t,n_forced*1)" -c:a aac -b:a 128k -ar 44100 -f flv rtmp://localhost/live/tela
```
### Como assistir

Há três formas de assistir, a primeira e abrindo o navegador e digitando a url "http://jarvispi/stream/tela.m3u8"

A segunda é pelo VLC, vá em Media > Open Network Stream > 
Cole a url http://jarvispi/stream/tela.m3u8

A terceira é pelo player embutido no html hospedado no Nginx digite http://jarvispi e a página vai ser carregada.

Desativar IP V6
sudo sysctl -w net.ipv6.conf.all.disable_ipv6=1 && sudo sysctl -w net.ipv6.conf.default.disable_ipv6=1
#### Iniciar automaticamente

Precisamos criar um arquivo de serviço
```
sudo nano /etc/systemd/system/livestream.service
```

Copie todo texto para o novo arquivo
```
[Unit]
Description=Servico de Streaming FFmpeg
After=network-online.target nginx.service
Wants=network-online.target

[Service]
User=principal
Group=video
ExecStart=/usr/bin/ffmpeg -loglevel warning \
    -f v4l2 -input_format mjpeg -video_size 1280x720 -framerate 30 -i /dev/video0 \
    -f alsa -channels 1 -i hw:2,0 \
    -c:v libx264 -preset ultrafast -tune zerolatency -pix_fmt yuv420p \
    -b:v 2M -g 30 -keyint_min 30 -force_key_frames "expr:gte(t,n_forced*1)" \
    -c:a aac -b:a 128k -ar 44100 \
    -f flv rtmp://localhost/live/tela
Restart=always
RestartSec=5

[Install]
WantedBy=multi-user.target
```

Habilitar a inicialização 
```
sudo systemctl enable livestream.service
```
Recarregar o Daemon

```
sudo systemctl daemon-reload
```

Iniciar o serviço

```
sudo systemctl start livestream.service
```

Verificar se houve algum erro

```
journalctl -u livestream.service -f
```

Antes de reiniciar teste se deu tudo certo

#### Reiniciando Raspberry PI

Vamos reiniciar do jeito certo para garantir que os arquivos não se corrompam
```
sudo reboot
```

Faça um teste e se deu tudo certo reincie de novo tirando o cabo de força
