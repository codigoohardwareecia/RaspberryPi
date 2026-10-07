Página do Kiwix
https://kiwix.org/en/

Download
https://get.kiwix.org/en/solutions/applications/kiwix-reader/

Navegar baixar arquivos Zim
https://browse.library.kiwix.org/#lang=eng

## Cenário 1

Acessar o Kiwix pela internet é possivel navegar nas Wikis disponiveis apenas usando o navegador.

```
https://browse.library.kiwix.org/#lang=eng
```

## Cenário 2

O Kiwix tem uma aplicação que pode ser executada localmente, vc pode baixar:
```
https://get.kiwix.org/en/solutions/applications/kiwix-reader/
```

Após baixar, descompacte o arquivo zip em um diretório do PC ou em um Pendrive ou HD externo.

### Parte 1: Instalação do Raspbian no Raspberry Pi

- Um computador (Windows, Mac ou Linux) com leitor de cartão SD.
    
- Um cartão microSD (recomendado: Classe 10, UHS-I, de pelo menos 16 GB ou 32 GB).
    
- O software **Raspberry Pi Imager** instalado no seu computador (baixado do site oficial `[raspberrypi.com/software](https://raspberrypi.com/software)`).
  
**1. Conectar o cartão e abrir o Imager:**Passo 1.

Insira o cartão microSD no leitor de cartão do seu computador e abra o aplicativo **Raspberry Pi Imager**.

**2. Escolher o modelo do Raspberry Pi:**Passo 2.

Clique na opção **Dispositivo** (ou _Choose Device_) e selecione o modelo exato da sua placa (ex: Raspberry Pi 4, Raspberry Pi 5, Pi Zero 2 W, etc.). Isso filtra os sistemas mais otimizados para a sua placa.

**3. Selecionar o Sistema Operacional:**Passo 3.

Clique em **Sistema Operacional** (ou _Choose OS_).

- Para uso geral/desktop, escolha o recomendado: **Raspberry Pi OS (64-bit)**.

**4. Escolher o destino de armazenamento:

Clique em **Armazenamento** (ou _Choose Storage_) e escolha o seu cartão microSD na lista.

**5. Personalizar as configurações de SO:**Passo 5.

Clique em **Avançar**. O Imager perguntará se você deseja **aplicar as configurações de personalização de OS** (_OS Customization_). Clique em **Editar Configurações**:

- **Geral:** Defina o _hostname_ (nome na rede), nome de usuário e senha, dados da sua rede Wi-Fi (SSID e senha) e o fuso horário/layout de teclado.
    
- **Serviços:** Marque a opção **Ativar SSH** (use autenticação por senha ou chave pública) se planeja acessar a placa remotamente sem monitor.
    
- Clique em **Salvar** e depois em **Sim** para aplicar.
    

**6. Iniciar a gravação e validação:

Confirme que deseja apagar os dados do cartão clicando em **Sim**. O programa irá baixar a imagem, gravar no cartão e fazer a verificação dos arquivos automaticamente.

> **Dica útil:** Assim que a verificação for concluída, o Imager informará que você já pode remover o cartão. Basta inseri-lo no slot do Raspberry Pi e ligar a fonte de alimentação — o sistema fará a primeira inicialização já configurado com o seu usuário e conectado ao Wi-Fi.

### Parte 2: Instalação e Preparação das Pastas

Execute os comandos abaixo no terminal do Pi 5 (ele vai criar a pasta `KiwixData` na sua Home, instalar as ferramentas e ajustar as permissões):

Bash

```
# 1. Atualiza a lista de pacotes e instala o Kiwix
sudo apt update && sudo apt install kiwix-tools -y

# 2. Garante que a pasta de dados exista na sua Home
mkdir -p ~/KiwixData

# 3. Entra na pasta para você colocar seus arquivos .zim lá depois
cd ~/KiwixData
```

_(Agora é o momento em que você usa o WinSCP para jogar o seu arquivo do StackOverflow ou outros `.zim` dentro dessa pasta `~/KiwixData`)._
### Parte 3: Gerar a Biblioteca

Assim que terminar de transferir os arquivos pelo WinSCP, rode este comando para ler a pasta e criar o índice de múltiplos arquivos:

Bash

```
cd ~/KiwixData && kiwix-manage library.xml add *.zim
```

### Parte 4: Automatizar o Webserver no Boot (Systemd)

Para não ter que ficar abrindo o terminal e digitando comando toda vez que o Pi 5 ligar, vamos criar o serviço do sistema.

Execute este bloco único de comandos (ele já cria o arquivo de configuração com as rotas exatas, ajustando para o usuário `principal` que você está usando):

Bash

```
sudo bash -c 'cat <<EOF > /etc/systemd/system/kiwix.service
[Unit]
Description=Servidor Kiwix Webserver Múltiplos Conteúdos
After=network.target

[Service]
Type=simple
User=principal
WorkingDirectory=/home/principal/KiwixData
ExecStart=/usr/bin/kiwix-serve --port=8080 --library library.xml
Restart=on-failure

[Install]
WantedBy=multi-user.target
EOF'
```

### Parte 5: Ativar e Ligar o Servidor

Por fim, copie e cole este bloco para ativar o serviço que acabamos de criar e dar o start inicial:

Bash

```
# Recarrega o gerenciador de serviços do sistema
sudo systemctl daemon-reload

# Ativa o Kiwix para ligar junto com o Raspberry Pi 5
sudo systemctl enable kiwix.service

# Inicializa o servidor agora mesmo
sudo systemctl start kiwix.service
```

### Como testar se deu certo?

Para ter certeza de que o serviço está rodando em background sem erros, rode:

Bash

```
sudo systemctl status kiwix.service
```

Se aparecer uma bolinha verde escrito `active (running)`, está perfeito. Agora é só abrir o navegador do seu PC Windows ou celular e digitar: 
http://SEU_IP:8080