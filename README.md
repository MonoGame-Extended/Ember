# Ember - MonoGame.Extended Particle Editor

![Ember Banner](./logo/ember-banner.png)

## An ImGui-based particle system editor for MonoGame Extended's particle system

![Ember Editor](https://img.shields.io/badge/MonoGame-Extended-blue)
![.NET 8](https://img.shields.io/badge/.NET-8.0-purple)
![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20macOS%20%7C%20Linux-lightgrey)

## Features

### Particle System Editing

- **Visual Particle Editor**: Real-time particle system editing with immediate visual feedback
- **Multiple Emitters**: Support for multiple particle emitters within a single effect
- **HSL Color Space**: Advanced color manipulation using HSL (Hue, Saturation, Lightness) format
- **Live Preview**: See changes instantly as you modify parameters

### Comprehensive Modifier System

Ember provides full support for all MonoGame Extended particle modifiers:

#### Core Modifiers

- **Age Modifier**: Applies interpolators to particles based on their lifetime progression
- **Drag Modifier**: Simulates fluid resistance with configurable density and drag coefficients
- **Linear Gravity Modifier**: Applies constant directional forces (gravity, wind effects)
- **Opacity Fast Fade Modifier**: Rapid linear opacity fade-out effects
- **Rotation Modifier**: Controls particle rotation rate over time
- **Velocity Color Modifier**: Changes particle colors based on movement speed
- **Velocity Modifier**: Applies interpolators based on particle velocity magnitude
- **Vortex Modifier**: Creates gravitational attraction effects toward a central point

#### Container Modifiers

- **Circle Container**: Constrains particles within or outside circular boundaries with bouncing
- **Rectangle Container**: Constrains particles within rectangular boundaries with bouncing
- **Rectangle Loop Container**: Wraps particles to opposite sides when exiting rectangular boundaries

All container modifiers support configurable restitution coefficients for realistic bouncing behavior.

### Emitter Profiles

Support for all MonoGame Extended emission patterns:

- **Point Profile**: Emit from a single point with random directions
- **Line Profile**: Emit uniformly along a line segment
- **Circle Profile**: Emit from circular areas with controllable radiation patterns (inward, outward, random)
- **Ring Profile**: Emit from ring-shaped areas
- **Box Profile**: Emit from rectangle perimeters
- **Box Fill Profile**: Emit from filled rectangular areas
- **Box Uniform Profile**: Uniform distribution along rectangle edges
- **Spray Profile**: Cone-shaped emission patterns

### Advanced Interpolation System

Interpolators enable smooth property transitions over particle lifetimes:

- **Color Interpolator**: Smooth HSL color transitions
- **Hue Interpolator**: Hue cycling while preserving saturation and lightness
- **Opacity Interpolator**: Fade-in/fade-out effects
- **Rotation Interpolator**: Smooth rotation changes
- **Scale Interpolator**: Size transitions over time
- **Velocity Interpolator**: Velocity magnitude and direction changes

Interpolators integrate with **Age Modifier** (time-based) and **Velocity Modifier** (speed-based) for complex effects.

## Installation

### Prerequisites

- .NET 8.0 SDK or later

### Building from Source

1. **Clone the repository**

   ```bash
   git clone https://github.com/Monogame-Extended/Ember.git
   cd Ember
   ```

2. **Initialize submodules**

   ```bash
   git submodule update --init --recursive
   ```

3. **Build the project**

   ```bash
   dotnet build src/Ember/Ember.csproj
   ```

4. **Run the application**

   ```bash
   dotnet run --project src/Ember/Ember.csproj
   ```

## Usage

### Getting Started

1. **Create a New Project**
   - Launch Ember
   - Click "Create New Project" or use `File > New`
   - Choose a location for your project file

2. **Add Particle Emitters**
   - Use the "Add Emitter" button in the Particle Emitters panel
   - Configure emitter properties (capacity, lifespan, quantity, speed, etc.)
   - Select an emission profile that fits your needs

3. **Add Textures**
   - Click "Choose Texture" to add texture files to your project
   - Textures are automatically copied to your project directory
   - Configure source rectangles for sprite sheet support

4. **Configure Modifiers**
   - Add modifiers to control particle behavior over time
   - Use Age Modifier with interpolators for smooth time-based transitions
   - Use Velocity Modifier with interpolators for speed-based effects
   - Combine multiple modifiers for complex behaviors
   - Configure container modifiers to constrain particle movement

5. **Fine-tune Parameters**
   - Adjust particle release parameters (color, scale, rotation, speed, mass)
   - Configure modifier-specific properties (gravity strength, drag coefficients, vortex mass)
   - Set container boundaries and restitution coefficients
   - Use HSL color controls for precise color manipulation

6. **Save and Export**
   - Save your project using `File > Save`
   - Projects are saved in `.ember` format for future editing

### Project Structure

```bash
your-project/
├── your-project.ember    # Project file
├── texture1.png          # Texture files
├── texture2.jpg
└── ...
```

### Advanced Techniques

#### Creating Complex Effects

- **Combine Age and Velocity Modifiers**: Use Age Modifier for time-based changes and Velocity Modifier for speed-responsive effects
- **Layer Multiple Interpolators**: Add multiple interpolators to a single Age Modifier for complex transitions
- **Use Container Modifiers**: Create boundaries with Circle or Rectangle containers, or use Rectangle Loop for wrapping effects
- **Vortex + Gravity Combinations**: Combine Vortex and Linear Gravity modifiers for complex force fields

## Contributing

Please refer to the [CONTRIBUTING](CONTRIBUTING.md) document.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Acknowledgments

- [Mercury Particle Engine](https://github.com/Matthew-Davey/mercury-particle-engine) - The original particle engine that MonoGame Extended's particle system is based on
- [MonoGame Foundation](https://www.monogame.net/) - For the excellent MonoGame framework
- [MonoGame Extended Contributors](https://github.com/MonoGame-Extended/MonoGame.Extended) - For the comprehensive particle system
- [Hexa.NET.ImGui](https://github.com/HexaEngine/Hexa.NET.ImGui) - For the Dear ImGui C# wrapper
- [JetBrains](https://www.jetbrains.com/) - For the mono font
- [Font Awesome](https://fontawesome.com/) - For icons

## Support

- Create an issue for bug reports or feature requests
- Join the [MonoGame Extended Discord](https://discord.gg/FvZ8Z7EzPJ) for questions and community support
