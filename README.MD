# MovePerson: a FPS Prototype for Unity3D

MovePerson is a platform-oriented sandbox FPS prototype reminiscent of 
arena shooters and immersive sims of the 90s.

This goal of this project is to create a prototype FPS system, combining
the acrobatics of action games with the interactivity of immersive sims 
and RPGs, that can serve as a base for a wide variety of genres such as arena shooters, open-world action RPGs, and walking simulators.

### Reproduction
* This repository is a Unity 2020.1 Project. The cloned repository is ready to add in Unity Hub.
* Clone recursively. This repository uses submodules.

### Table of [Important] Contents
 * Assets
   * [Main](/Assets/Main) - Assets necessary to the core funcionality of the underlying prototype
     * Actors - Assets associated with the objects that act as game pieces
     * Core - Assets associated with low-level functionality and generic systems
     * GUI - Assets used only in GUI
     * World - Assets that add functionality to the scene, but don't pertain to any game design
   * MPGame - Assets that are not relevant to the underlying prototype

### Submodules
 * [TetraGen - Isosurface generator](https://github.com/64bleat/TetraGen-Unity.git)
 * [AstronomicSkybox - Astronomic formulas, reak skybox and time simulator](https://github.com/64bleat/AstronomicSkybox-Unity.git)
 * [XMLSerialization - Load and save system](https://github.com/64bleat/XMLSerialization-Unity.git)