# North star - Jeu de Tir Spatial

Projet de jeu vidéo 2D développé avec MonoGame et .NET 8 dans le cadre d'un projet scolaire.

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet)
![MonoGame](https://img.shields.io/badge/MonoGame-3.8-E73C00?style=flat-square)
![C#](https://img.shields.io/badge/C%23-12.0-239120?style=flat-square&logo=csharp)

## Description

North star est un jeu de tir spatial où le joueur doit survivre face à des vagues d'ennemis, affronter des boss et collecter des power-ups.

## Prérequis

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Windows 10/11
- Carte graphique supportant OpenGL 3.0+

## Installation

### Cloner le projet

```bash
git clone https://github.com/votre-username/space-shooter.git
cd space-shooter/gameTest2
```

### Lancer le jeu

```bash
dotnet restore
dotnet build
dotnet run
```

## Commandes

### Menu principal
- `Entrée` / `Espace` : Démarrer une partie
- `L` : Afficher le classement
- `Alt+Entrée` / `F11` : Basculer plein écran

### En jeu
- `ZQSD` / `Flèches` : Déplacer le vaisseau
- `Clic gauche` : Tirer
- `Échap` : Retour au menu

## Architecture

### Structure du projet

```
gameTest2/
├── Game1.cs              # Classe principale du jeu
├── Program.cs            # Point d'entrée
├── ScoreDatabase.cs      # Gestion base de données
├── ScoreData.cs          # Modèle de données
└── Content/              # Ressources (textures, sons, polices)
```

### Technologies utilisées

- **MonoGame 3.8** - Framework de jeu
- **.NET 8** - Runtime
- **SQLite** - Base de données pour les scores
- **C# 12** - Langage de programmation

### Fonctionnalités implémentées

- Gestion des états du jeu (Menu, Jeu, GameOver)
- Système de collision
- Génération dynamique d'ennemis
- Système de score avec persistance
- Interface utilisateur interactive
- Effets sonores et musique

## Compilation

```bash
dotnet publish -c Release -r win-x64 --self-contained true
```

Le fichier exécutable sera dans : `bin/Release/net8.0/win-x64/publish/`
