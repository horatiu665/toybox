# ToyBox

Welcome to ToyBox - a collection of C# scripts and tools for Unity I've been collecting, using and polishing since approx 2014. They are mostly dependency-free, except some of them use `[DebugButton]` because it makes life so much easier, even for a small tool. Some of them are mine, while some have been collected from other people over the internet. I'll try to describe them here so you know what you're getting into. 

## AssemblyDefinitionSmart

A small tool, inspired from frustration, to generate assembly definition (.asmdef) files in a folder and each of its children where an Editor/ folder is detected, and making references to the root assembly. It can be even smarter, but it already saves some time when you wanna separate some code, or export a package or something.

Usage: select a folder in Project, right-click on it, Create > Assembly Definition (smart) and check out all your cool assembly definitions in all the children of your selected folder.

![image](https://user-images.githubusercontent.com/5824753/110971811-9e896880-835b-11eb-8aa4-e13db92bcc26.png)

## BuildToPlatform

Sometimes for multi-platform projects (and without cloud build), one must make multiple builds for different platforms.

This tool is a nifty editor window with buttons to build to selected platforms. It incorporates basically 2 clicks: Switch Platform.... and Build (or Build & Run). This way you don't have to wait for Switch Platform and click Build again later, but open the Build > Build To Platform window, select the platform, click build and go make a coffee and wait for your build.

![build window](https://user-images.githubusercontent.com/5824753/110971766-903b4c80-835b-11eb-859a-4b7a62f6e853.png)

## DebugButton

A super useful trick to make functions accessible in the Unity Inspector, for development/debug purposes. Simply add [DebugButton] before any function, and a nice button will appear in the (non-custom) inspector for that script. 

![debug button](https://user-images.githubusercontent.com/5824753/110955469-edc69d80-8349-11eb-9247-7e2b23077246.png)

Note: this was snatched from Kaae, a friend of a friend, and tweaked by me. There are also other versions if this out there, consider exploring them.

## DefaultAsset

An attribute to use in combination with ScriptableObjects, or maybe other types of assets and prefabs (but easier with scriptableobjects because they can be found easily). Write the attribute like this: 
```
[DefaultAsset("default fish")]
public GameObject defaultFish;
```
And it will try to assign the first result of the search result for that query. So as an example, the default fish is found by name. But if there is a duplicate file with the name that comes first, it's not so reliable so you should basically watch out with this one.

![default fish](https://user-images.githubusercontent.com/5824753/110957986-79d9c480-834c-11eb-85c5-27d4774c27ab.png)

## DraggableWindow

A UI script that you can put on a RectTransform to allow it to be dragged by mouse/touch, and to not allow it to go outside the screen. Use the RectTransformUtility.cs script for a few functions that can help with the world space / rect space / canvas space transformations (or at least can provide some insight in how that stuff works).

![dragga](https://user-images.githubusercontent.com/5824753/110972293-20799180-835c-11eb-8bec-ed0444a57426.gif)

## DrawLineInGame

Utility to draw some debug lines just like Debug.DrawLine but in the game view and even in a build, using cubes.
There is also a function for not letting them disappear if there is some condition, intended use case is in VR if you hold a button, you can keep the debug on screen to inspect it further.

Here it is drawing some random lines from a point.

![draw line in game](https://user-images.githubusercontent.com/5824753/110961790-79dbc380-8350-11eb-8f08-78ced3a3ec26.gif)

## EnumButtons

One of my favorite little tools: use `[EnumButtons]` in front of an enum field to make a one-row selectable enum. It also supports flags.

![fish and flags](https://user-images.githubusercontent.com/5824753/110964197-3afb3d00-8353-11eb-9611-714b69b2b8fb.gif)

## EnumLongSelection

Long enums are a pain, and the EnumButtons won't be useful there. Good thing there is `[EnumLongSelection]`, it makes a window with all the enum options and it keeps the game running while you change the value. Useful in play mode for VR when a player is testing and a designer is tweaking values and changing enums while the player plays in VR. This could use a search function and some optimization but even this way it is usable in some cases. Here it is in a bad example, where it uses the enum KeyCode.

```
[EnumLongSelection(typeof(KeyCode))]
public KeyCode demoLongEnum;
```

![long enum example](https://user-images.githubusercontent.com/5824753/110966817-fb822000-8355-11eb-8864-b24c8e690751.gif)

## EventOn

A selection of scripts (currently just one but a few more ideas come to mind, might expand later) that connect UnityEvents to events from Unity. Put the script on an object and hook it up around the scene or in your prefab to do various things, such as turn objects on/off, run functions, etc, and also use a DebugButton to do that action immediately
- EventOnEnable.cs - run a unity event on OnEnable

What it looks like:

![EventOnEnable](https://user-images.githubusercontent.com/5824753/110969569-1dc96d00-8359-11eb-860c-61853f96b3eb.png)

## HoratiusQuickOrganizer

A tool for organizing assets. How to use: open the Horatius Quick Organizer window from the Tools/ menu in unity, select your completely unorganized assets, and click Flatten or Organize to create order (or chaos).

![horatius quick organizer](https://user-images.githubusercontent.com/5824753/110971528-48b4c080-835b-11eb-99a7-899ec45554db.gif)

## ReadOnlyAttribute

This one is found multiple places online, use by placing `[ReadOnly]` above a field and it will appear gray in the inspector, and one cannot change the value, only read it. It also features a runtime version, where a field is readonly, but only when the game is playing in Play Mode.

![image](https://user-images.githubusercontent.com/5824753/110972922-da70fd80-835c-11eb-8c4b-d06824c111b6.png)

## SceneReference

This one is totally snatched from here (and very useful when making games with multiple scenes): https://gist.githubusercontent.com/JohannesMP/ec7d3f0bcf167dab3d0d3bb480e0e07b/raw/e4d0c7f636ceb3d01a12303e775f268000f39f26/SceneReference.cs

## TimescaleHack

Drop the TimescaleHack prefab in the scene (or the TimescaleHack.cs script on any object) and it will draw a little GUI interface that can slowdown time or accelerate it, and it snaps to 1 in the middle. Useful when debugging physics, AI or stuff that changes too fast to notice in real time.

![timescale hack](https://user-images.githubusercontent.com/5824753/110973648-cc6fac80-835d-11eb-8537-f986c7356647.png)

## ConeCastExtension

Use it to cast a cone shape rather than a spherecast or boxcast.
Doesn't work super well but it sort of works.

## CreatePrefabsFromAllChildren
Have you ever not done something in Unity just because you dreaded the amount of work it might take, only to wish there was a tool that could quickly do it for you? If that thing was making a lot of prefabs at once, this tool is for you!

Create prefabs out of all children of a transform in the scene, or apply the changes made to existing prefabs parented under the same scene object, with the click of a button. It even handles unique names by suffixing some numbers, if you have multiple children you want to make prefabs out of, and they share the same name.

Unity will work for a bit cause it's slow to update many prefabs at once, but it beats getting carpal tunnel syndrome from too many clicks! Here is demo with 4 prefabs but it can handle endless amounts.

![prefabs stuff](https://user-images.githubusercontent.com/5824753/111464584-39e65900-8721-11eb-82af-8f9a2fefc887.gif)

## DestroyAllScripts

When exporting an object for sharing in another project, it can be useful to export without scripts, so that you can use Unity's built-in "Include Dependencies" checkmark on the export package screen. This script helps you remove all MonoBehaviours from an object so you don't have to do it by hand. Note: it doesn't always work as there are kinks to this export process, but it is a marginally-useful utility nonetheless.

## FakeParenting

A useful script that makes a transform follow another every update, with toggled position/rotation and update/fixedupdate/lateupdate. There are also smooth-follow versions out there, but sometimes you just need a simple reliable one.

![image](https://user-images.githubusercontent.com/5824753/111475458-61dbb980-872d-11eb-8b96-9b460fc08f3a.png)

## FlyCamExtended

A version of the FlyCamExtended from this place: http://wiki.unity3d.com/index.php/FlyCam_Extended
But with some added functionality such as lock cursor, holding Alt disables camera rotation (useful for edit-time tweaks and for recording footage of the game view while tweaking stuff), smooth acceleration, some more parameters in the Inspector.

![image](https://user-images.githubusercontent.com/5824753/111475855-ceef4f00-872d-11eb-8394-6669c1344be4.png)

## GridCellAutoSizer

A UI script that can help with a basic functionality of grids: you want them to look like an evenly spaced grid independent of the size of the parent of the grid, and covering the whole width. It only works with constant cell count width/height but super useful for that. So here you go!

![grid shitz](https://user-images.githubusercontent.com/5824753/111481364-2ba13880-8733-11eb-829c-f9bd7b656ec0.gif)

## LinqExtensions

Some extensions of Linq queries that are typical for physics work in Unity, or whatnot.
`ClosestRaycastHit` - find the nearest hit out of a list of RaycastHit
`AggregateSmart` - a silly version of the Aggregate() to find one element that satisfies some condition when compared to all the other elements of a list (but ignoring nullable types when the list contains null stuff)
`MaxIndex` - find the index of the max element, not the max value as Max() does
`ClosestColliderBounds` and `ClosestCollider` and `ClosestTransform` - find the closest distance to one of those things out of a list of things

... maybe more on the way, now that I have a place to put those things as I develop them.

## LookAtObj 

Simple script to make a transform point at another transform, with an option to point at the main camera without need of reference (and using a cached Camera.main for performance).

![image](https://user-images.githubusercontent.com/5824753/111487292-84bf9b00-8738-11eb-9aa2-fb369946acb9.png)



# License
MIT because you are welcome to use this stuff however you want, and credit is appreciated.
