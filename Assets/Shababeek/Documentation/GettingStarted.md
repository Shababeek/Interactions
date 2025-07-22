# Getting Started with the Shababeek Interaction System

## Introduction
Welcome to the Shababeek Interaction System! This guide will help you set up your first interactive scene in Unity.

## Prerequisites
- Unity 2020.3 or newer
- Unity Input System package installed

## Installation
1. Copy the `Assets/Shababeek` folder into your Unity project.
2. (Optional) Move `Assets/Shababeek/Interactions` to your `Packages` folder for embedded use.

## First Scene Setup
1. Create a new scene or open an existing one.
2. Create a Config asset: Right-click in Project > Create > Shababeek > Interactions > Config.
3. Assign hand data, layers, and input settings in the Config asset.
4. Add a Hand prefab or component to your scene for each hand.
5. Add an Interactor component (e.g., TriggerInteractor, RaycastInteractor) to each hand.

## Creating Your First Interactable
1. Add a GameObject to your scene (e.g., a Cube).
2. Add an Interactable component (e.g., Grabable) to the GameObject.
3. Configure interaction settings in the Inspector (hand, button, feedback, etc.).
4. Play the scene and test grabbing or interacting with the object.

## Next Steps
- Explore the FeedbackSystem for haptics and audio.
- Create custom interactables by inheriting from InteractableBase.
- Read the User Manual and Scripting Reference for advanced features. 