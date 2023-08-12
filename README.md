# Hypixel Autotip

Automatically tip random players on the Minecraft Hypixel Network every hour.

In this repository you find 

- A script for the Minecraft Console Client (https://github.com/MCCTeam/Minecraft-Console-Client)

- A configuration file for the Minecraft Console Client

## How it works:
1. Joins Bedwars lobby
2. Get online players
3. Tip 9 random players 
4. Wait ~1 hour and repeat

## Features 
- AntiAFK plugin is used for sending /tipall every x minutes
- Automatically reconnects when the connection drops

## Installation:
1. Download the Minecraft Console Client & the 2 files in this repository
2. Copy all the files in one folder
3. Edit the MinecraftClient.ini and add your e-mail as login
4. Start the Console Client and when logged into Hypixel use the command 
`/script Autotip.cs`
5. Now you're all set and it should tip every hour