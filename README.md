# The Peace Engine

Peace Engine is a game engine written ontop of the MonoGame framework, compatible with Windows, macOS and Linux using OpenGL.

There are three main key features of Peace Engine that may interest you.

1. **Modular components** - Every API and feature of the engine is inside a modular component managed completely by the engine. You should never have to tell the engine that you're adding a feature to your game. You just tell it what other components you'll need, and it'll do the rest.
2. **Layered entity system** - Everything you see, hear or interact with in youur game is a Peace Engine entity. Entities can be spawned on different layers, they can render to the screen, accept mouse or keyboard input, and access any Engine Components you'd like. You can spaw them whenever you want.
3. **Extended Content Pipeline** - You can save a lot of disk space in your game by using Peace Engine's built-in content pipeline extensions for sound effects and textures. Instead of letting MonoGame store your raw texture or audio data in .xnb files, you can keep your files in their compressed formats (.png, .jpg, etc for textures, and .ogg for audio) and load them in as `Texture2D` or `SoundEffect` just like you would in regular MonoGame.

### Setting up the Peace Engine

So you want to set up the engine for your own game? Well, all you need is to follow [this article](https://watercolorgames.net/wiki/setting-up-the-peace-engine) on our website.

That article is a good spot to start for modding The Peacenet as well, but getting the code is a bit different.

### Other neat features

1. **Robust UI system** - The API is loosely based off Windows Forms for ease of use, but the UI itself can do many more things than Windows Forms will ever be able to. Scrollables, editables, labels, check boxes, list views, the list goes on. Animation is also extremely simple. You can very easily build complex user interfaces for your game.
2. **Ready-made configuration and save system** - with Peace Engine, you don't have to worry about writing your own configuration manager or save system. You just need to know what you want to store. We'll take care of storing it for you.
3. **Rendering things on screen has never been easier** - Thanks to our Graphics Context class, all the usual things you'd want to do in a paint program can be done in-engine - drawing circles, lines, rectangles, polygons, text, and more. Sprite shaders, scissor-testing and render offsets are also supported.
