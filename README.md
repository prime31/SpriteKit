SpriteKit
=========

SpriteKit is a lightweight sprite creation and animation library. It is designed to do a few things and then get out of your way.


Usage: Sprite Sheet and Sprite Creation
---

The first thing you will want to do is create a sprite sheet to work with. This can be done by choosing the "Create New Sprite Sheet..." item from the SpriteKit menu. SpriteKit will then ask you for a source folder for the images files. Later on you can add and remove files from the folder and SpriteKit can update the texture atlases at the click of a button (or automatically is you only modify an image).

Once you have a sprite sheet created create an empty GameObject and stick the SKSprite component on it. Use the inspector to setup your sprite. Thats all it takes.


Sprite Animations
---

Setting up sprite animations is also quite simple. In the project window select all the images you want to be in the animation. Choose the "Create Animation from Selected Images..." menu item in the SpriteKit menu. A wizard will appear letting you set the animation name, fps, loop type, etc. Once your animation is setup you can play it with the following line of code (assuming you called the animation "walk"):

    GetComponent<SKSprite>().startAnimation( "walk" );


Programmatic Sprite Creation
---

You can create sprites programmatically at any time. The SKSprite class has a few different variants of the createSprite method. Once example is:

    // first, you need the sprite sheet that owns the sprite. You can get it by name or by the sprite's file name
    var spriteSheet = SKSpriteSheet.sheetWithName( "MyCharacterSprite" );
    var newSprite = SKSprite.createSprite( spriteSheet, "joe-blow", SKSprite.SpriteAnchor.BottomLeft );


Nine-Slice Sprites
---

SpriteKit includes a SKNineSliceSprite class as well. Just drag it onto an empty GameObject, choose your sprite sheet and image name from the inspector and you are ready to go. You can use the inspector to setup the top, right, bottom and left endcap sizes and SpriteKit will update in real time so you can get them perfect.

The SKNineSliceSprite class is also a great example of you to create your own extensions to SpriteKit. You can use it as a template to setup any kind of Mesh that you need for your sprites. For example, you could make hexagon shaped sprites by just overriding the generateVerts method and making the verts be in the shape of a hexagon. This could be taken even further to making a text/font system right in SpriteKit. The sky is the limit.

