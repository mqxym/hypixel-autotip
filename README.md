# Hypixel Autotip

Automatically tip random players on the Minecraft Hypixel Network every hour.
In this repository you find 
- A script for the Minecraft Console Client (https://github.com/MCCTeam/Minecraft-Console-Client)
- A configuration file for the Minecraft Console Client

This repository is *not* affiliated with Hypixel. 
Rules should be checked before using this script.

## How it works:
1. Joins Bedwars lobby
2. Get online players
3. Tip a random player in the 10 available gamemodes
4. Wait ~1 hour and repeat

## Features 
- AntiAFK plugin is used for sending /tipall every x minutes
- Autotip.cs script tips random players
- The script retries tipping when the target player went offline
- Automatically reconnects when the connection drops

## Installation:
1. Download the Minecraft Console Client & the 2 files in this repository
2. Copy all the files in one folder
3. Edit the MinecraftClient.ini and add your e-mail as login
4. Edit the Autotip.cs and enter your Minecraft username `private const string TipUsername = "<your_name>"`
5. Start the Console Client and login on your browser with the Microsoft login
6. When logged into Hypixel use the command 
`/script Autotip.cs`
7. Now you're all set and it should tip every hour
