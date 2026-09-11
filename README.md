# Dora: Spatial Questing System Assisted by Procedural Generation and Validation

## Abstract

Quests are a fundamental part of many games. 
However, limited public tooling is available for developers, as quest systems often require a tight integration with a game's logic. 
In this thesis we present a tool named _Dora_, which aims to address the variety of features and issues surrounding quest systems in games.
We aim to provide a general-purpose questing toolkit for the Unity game engine, whilst additionally being specifically focused on supporting quests requiring a spatial context. 
Such context can be provided by existing solutions, such as the Space Foundation System, which creates a semantic graph of locations in a virtual world.
Additionally, _Dora_ comes with integrated validation mechanisms to prematurely detect inconsistencies and errors during development. 
A spatially aware procedural generation system also helps developers with iterating over the placement of quest-related content in their virtual world.
The library's design is focused on allowing both game developers and designers to express their ideas.
This is achieved by making use of class inheritance, which allows code-based developers to define custom logic, as well as ScriptableObjects, which can be used by non-coding users to directly define questing scenarios via the Unity editor.

## Installation

Dora is available on [GitHub](https://github.com/GiorgioKalmund/dora) and can be installed via the **Unity Package Manager**.

1. Open the Unity Package Manager (`Window ▶ Package Management ▶ Package Manager`)
2. In the upper left corner, press on the `+` symbol and select `Install package from git URL`
3. Paste the link and click `Add`

```
https://github.com/GiorgioKalmund/dora.git
```

This will add the latest version of to your Unity project, as well as the _Space Foundation System_ and _NaughtyAttributes_.