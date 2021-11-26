![Language](https://img.shields.io/badge/Developed%20in-.NET%20Core%205-blue)
![Running on](https://img.shields.io/badge/Running%20on-Windows%20|%20Docker-brightgreen)
[![License: MIT](https://img.shields.io/badge/License-MIT-brightgreen)](https://opensource.org/licenses/MIT)

# WakeOnLAN And Exit (WOL AE)

# Summary

- [Description](#description)
- [Third party libraries](#yhird-party-libraries)
- [How to use](#how-to-use)
- [License](#license)

# Description

## What is? What it does?

WOL AE is a console application which sends a WOL packet to the network (and exit).

It has been developed because the *synonet* command in Synology NAS is missing the options to specify port number and ip address to which forward the requests.

## Should I have a Synology NAS to run it?

No, the app has been developed in .NET Core 5 in order to make it **docker runnable** (this makes it DSM7-compatible) and **multi-platform** too.

# Third party libraries

[Nate McMaster CommandLineUtils](https://github.com/natemcmaster/CommandLineUtils) has been included to save time when parsing arguments from command line.

It is very easy to understand and to use, it deserves a mention for sure!

# How to use
```
Usage: WOL_AE.exe [options] <mac>

Arguments:
  mac              The MAC address to wake up

Options:
  -ip|--ipAddress  Optional: the IP address to which send the WOL packets (255.255.255.255 by default)
  -p|--port        Optional: the port to be used when sending WOL packets (9 by default)
  -v|--verbose     Enable verbose logging
  -?|-h|--help     Show help information.
```
## Windows

Just run the application via command prompt with the parameters specified above.

## Docker

* Create an image starting from the project files (the *'Dockerfile'* is already included and configured)
* Run a new container with the following options:
  * *'--rm IMAGE_NAME'* to delete the containers after their execution (there's no need to keep them since they are ephimeral)
  * *'--network host'* to ensure the container can use the same network as the system itself, without limitations
  * **(Windows Docker CLI only)** *'-it'* to enable the interactive mode; this ensures that multiple arguments can be passed within the *'docker run'* command

## Build and Run example
**On Windows client**
* Open a command prompt in the project folder
* Build the project
    ```
    dotnet publish -c Release
    ```
* Build the container
    ```
    docker build -t wol_ae-image -f Dockerfile .
    ```
* Run the container locally (to test it)
    ```
    docker run -it --network host --rm wol_ae-image AA-BB-CC-11-22-33 -ip 192.168.1.255 -p 2020 -v
    ```
* Export the image
    ```
    docker save -o wol_ae.tar wol_ae-image
    ```
**On Synology DSM 7**
* Upload the exported docker image
* Manually import via the docker package interface
* Go to Control Panel and create a scheduled task (user: *root*) with the following command:
    ```
    docker run --network host --rm wol_ae-image AA-BB-CC-11-22-33 -ip 192.168.1.255 -p 2020 -v
    ```
* Run the task and check the result

# How to test

Even if kinda rough, a quick way of testing consists in using the software [WOL Magic Packet Sender](http://magicpacket.free.fr), which can be installed and started in *Receive* mode to check if the WOL packet is correctly received.

# License

MIT...because sharing is caring ❤️