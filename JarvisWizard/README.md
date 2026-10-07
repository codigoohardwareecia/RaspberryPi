# Raspberry Pi 5: Implantação e Configuração do Raspberry PI e ASP.NET Core
#### Apresentação

- Salve, salve galera, beleza, bem vindo a mais um projeto
- Nesse projeto iremos implantar um serviço de WebAPI ASP.NET Core no Raspberry Pi
- Nosso objetivo e automatizar o acesso aos GPIOS através de conectividade
- Nesse exemplo iremos criar um endpoint que chamaremos a partir do nosso navegador.
- Nos próximos videos iremos brincar com os pinos GPIO e também usar nossa IA para realizar comandos inteligentes

#### Criar um novo projeto

- Abra o Visual Studio 
- Na tela "Get Started" clique em "Create a new project"
- Em All Languages selecione C#, Em Platforma selecione Linux, em Template selecione WebAPI
- Nos templates selecione ASP.NET Core Web API
- Cliquem Next
- Em Project Name digite "JarvisWizard"
- Em Location selecione um diretório para o seu projeto
- Em Solution Name digite "JarvisProject"
- Clique em Next
- Em Additional information, em Framework selecione .NET 8.0 (Long Term Support)
- Em Autentication Type deixe None
- Desmarque "Configure for HTTPS"
- Desmarque "Enable container support"
- Logo abaixo marque apenas "Use controllers"
- Clique em " Create"

#### Limpando o projeto

Apague WeatherForecast.cs
Apague WeatherForecastController.cs

#### Modificando Program.cs

Abra a classe "Program.cs"

Logo abaixo de builder.Services.AddControllers(), adicione o seguinte bloco de código,  esse bloco de código concede permissão de acesso externo a API

```CSharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

Então a sua classe Program.cs vai ficar dessa forma

```CSharp
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddCors(options 
{
    options.AddPolicy("AllowAll", policy 
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
```

#### Alterendo o arquivo appsettings.json

Abra o arquivo "appsettings.json"

Substituia o bloco de código por este, a configuração do Kestrel e fundamental para ele permitir acesso a de qualquer host a partir da porta 5050

```JSON
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:5050"
      }
    }
  }
}
```

#### Adicionado a Controler Home

- Botão direito sobre a pasta Controller, selecione "Add > Controller" 
- No Wizar selecione MVC Controller Empty
- Clique em Add
- No nome do arquivo da Controller digite "HomeController.cs" e clique em Add
- Altere a sua Controller para que esteja igual ao código abaixo:

```CSharp
using Microsoft.AspNetCore.Mvc;

namespace JarvisWizard.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class HomeController : ControllerBase
    {
        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok(new { msg = "Jarvis Vivo!" });
        }
    }
}
```

#### Alterando o arquivo JarvisWizard.http

Na raiz do projeto tem um arquivo chamado JarvisWizard.http altere o mesmo para que fique como o bloco de código abaixo:

```
@JarvisWizard_HostAddress = http://localhost:5050

GET {{JarvisWizard_HostAddress}}/api/v1/home/status
Accept: application/json

```

#### Alterando o arquivo Properties/launchSettings.json

Este arquivo contém configuraçoes de inicialização

```JSON
{
  "$schema": "http://json.schemastore.org/launchsettings.json",
  "iisSettings": {
    "windowsAuthentication": false,
    "anonymousAuthentication": true,
    "iisExpress": {
      "applicationUrl": "http://localhost:9023",
      "sslPort": 44336
    }
  },
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "api/v1/home/status",
      "applicationUrl": "http://localhost:5050",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "api/v1/home/status",
      "applicationUrl": "https://localhost:7235;http://localhost:5050",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "IIS Express": {
      "commandName": "IISExpress",
      "launchBrowser": true,
      "launchUrl": "api/v1/home/status",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}

```

#### Executando o projeto

- Clique no sinal de Play e selecione http
- Clique novamente para inicar a execução
- Se o VS mencionar algo sobre HTTPS, clique em No
- O navegador padrão será aberto na URL http://localhost:5050/api/v1/home/status
- Significa que deu certo nossa configuração

#### Configurando a Publicação do projeto

- Clique com o botão direito em cima do nome do Projeto e depois em Publish
- Na tela que aparece (Target) selecione Folder e clique em Next
- Mantenha o diretório e clique em Finish, clique em Close
- Iremos fazer alguns ajustes
- Na tela do perfil de publicação, clique em "Show all settings"
- Em Configuration mantenha "Release"
- Em Target Framework "net.80"
- Em Deployment Mode "Self-contained"
- Em Target Runtime "linux-arm64"
- Logo abaixo em File Publis Options marque a opção "Produce single file"
- Clique em save

#### Gerando os arquivos de Publicação

- Na tela de publicação clique em "Publish"
- Aguarde  a conclusão
- Acesse a pasta ...JarvisProject/JarvisWizard/bin/Release/net8.0/publish/ estarão os arquivos que vc deve copiar para o Raspberry Pi, só não copie o arquivo appsettings.Development.json

#### Configurando o Raspberry

Conecte-se ao Raspberry pelo Putty
Crie a pasta no diretório do usuaário com o comando a seguir
```
mkdir /home/admin/JarvisWizard
```
Execute o seguinte comando para criarmos o arquivo de serviço do Jarvis
```
sudo nano /etc/systemd/system/jarviswizard.service
```

Cole o seguinte bloco de código, apenas atente-se para os caminhos de diretório
```

[Unit]
Description=Servico API Jarvis .NET 8
After=network.target

[Service]
# Verifique se este caminho esta correto (admin em vez de pi)
WorkingDirectory=/home/admin/JarvisWizard
# Verifique o nome exato do executavel (case-sensitive)
ExecStart=/home/admin/JarvisWizard/JarvisWizard
ExecStartPre=/usr/bin/chmod +x /home/admin/JarvisWizard/JarvisWizard
urls="http://0.0.0.0:5050"
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=jarvis-api
User=admin
Environment=ASPNETCORE_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
```
Para salvar pressione Ctrl+O e depois enter

Para sair pressionee Ctrl+X

Copiando os arquivos da nossa WebAPI .NET Core

Usando o WinSCP, navegue até a pasta do usuário no RaspberryPi, localize o diretório que 
criamos JarvisWizard, acesse a pasta, copie e cole os arquivos da publicaçõ do passo anterior

Feche o WinSCP e volte para o Putty

No Putty vamos dar permissão a nossa pasta e ao executável (binário)
```
chmod +x /home/admin/JarvisWizard/JarvisWizard
```
Recarregue o gerenciador (Deamon) dos serviços
```
sudo systemctl daemon-reload
```
Inicialize o serviço do Jarvis
```
sudo systemctl start jarvis.service
```
Para testar abre o navegador e cole a url
```
http://jarvishost:5050/api/v1/home/status
```
Pronto os dados de status devem ser exibidos no navegador

#### Nova Publicação

Para realizar uma nova publicação execute os passis a seguir

Pare o serviço
```
sudo systemctl stop jarvis.service
```
Copie os arquivos com o WinSCP para pasta JarvisWizard

Conceda as permissões novmente
```
chmod +x /home/admin/JarvisWizard/JarvisWizard
```
Recarrega o Daemon
```
sudo systemctl daemon-reload
```
Inicialize o serviço
```
sudo systemctl start jarvis.service
```

#### Conclusão
Feito isso, acessado a WebAPI hospedada no Raspberry Pi estamos prontos para executar comandos nos pinos GPIO, vamos ver isso no próximo video
