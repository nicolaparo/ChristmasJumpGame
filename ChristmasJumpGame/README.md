# Christmas Jump Game 🎄

A festive 2D platform game built with Blazor and ASP.NET Core, featuring an elf character collecting gifts!

## Overview

Christmas Jump Game is a web-based platformer game where you control an elf character navigating through levels, collecting gifts, and avoiding obstacles. The game is built using Blazor Server with interactive components and custom game engine.

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- A modern web browser (Chrome, Firefox, Edge, Safari)

## Installation

1. Clone or download the repository
2. Navigate to the project directory:
   ```powershell
   cd ChristmasJumpGame
   ```

## Running the Application

### Option 1: Using Visual Studio

1. Open `ChristmasJumpGame.sln` in Visual Studio
2. Press `F5` or click the "Run" button
3. The application will launch in your default browser

### Option 2: Using Command Line

1. Open a terminal in the project directory
2. Run the following command:
   ```powershell
   dotnet run
   ```
3. Open your browser and navigate to:
   - HTTPS: `https://localhost:7004`
   - HTTP: `http://localhost:5203`

## Game Controls

### Keyboard Controls

| Key | Action |
|-----|--------|
| **Arrow Left** (←) | Move left |
| **Arrow Right** (→) | Move right |
| **Space** | Jump (hold for higher jump) |
| **Left Control** | Run (increases movement speed) |

### Touch/Mobile Controller

The game includes a virtual gamepad accessible at `/gamecontroller` route for mobile devices:

- **D-Pad**: Navigate (Up, Down, Left, Right)
- **JUMP Button**: Make the character jump
- **RUN Button**: Increase movement speed

To use the mobile controller:
1. Open `/gamecontroller` on a mobile device or separate browser window
2. The controller will automatically connect to the main game instance

## Gameplay

- **Objective**: Navigate through levels and collect gifts (presents) 🎁
- **Movement**: Use arrow keys to move left and right
- **Running**: Hold Left Control while moving to run faster (speed: 6 vs normal: 4)
- **Jumping**: Press Space to jump; hold for higher jumps
- **Collision**: The character collides with solid tiles and automatically collects gifts on contact

## Features

- Pixel-art style graphics with Christmas theme
- Custom game engine built with Blazor Canvas
- Responsive controls for both keyboard and touch devices
- Level system with JSON-based level files
- Multi-device controller support (play on PC, control with phone)
- Smooth platformer physics with gravity and acceleration

## Project Structure

- `/Components` - Blazor components (UI, pages, layouts)
- `/Engine` - Custom game engine (game loop, rendering, input handling)
- `/JumpGame` - Game-specific code (player, objects, assets)
- `/Levels` - Level definition files (JSON)
- `/wwwroot` - Static assets (CSS, JavaScript, sprites)

## Technologies Used

- **ASP.NET Core 10.0** - Web framework
- **Blazor Server** - Interactive UI framework
- **Blazor.Extensions.Canvas** - HTML5 Canvas rendering
- **SignalR** - Real-time controller communication
- **SixLabors.ImageSharp** - Image processing

## Level Editor

Access the level editor at `/leveleditor` to create and modify game levels.

## Troubleshooting

- **Game doesn't load**: Ensure .NET 10.0 SDK is installed
- **Controller not connecting**: Make sure both game and controller are on the same network/browser session
- **Performance issues**: Try using a Chromium-based browser for better Canvas performance

## Development

To modify the game:
1. Edit game logic in `/JumpGame/Objects/Player.cs`
2. Add new levels in `/Levels` directory
3. Modify sprites and assets in `/JumpGame/Assets`
4. Customize UI in `/Components/Pages`

## License

This project is a demonstration/educational game. Feel free to use and modify as needed.

---

**Merry Christmas and Happy Coding! 🎅🎄**
