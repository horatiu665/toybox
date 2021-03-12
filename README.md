# ToyBox

Welcome to ToyBox - a collection of C# scripts and tools for Unity I've been collecting, using and polishing since approx 2014. They are mostly dependency-free, except some of them use [DebugButton] because it makes life so much easier, even for a small tool. Some of them are mine, while some have been collected from other people over the internet. I'll try to describe them here so you know what you're getting into. 

## AssemblyDefinitionSmart

A small tool, inspired from frustration, to generate assembly definition (.asmdef) files in a folder and each of its children where an Editor/ folder is detected, and making references to the root assembly. It can be even smarter, but it already saves some time when you wanna separate some code, or export a package or something.

Usage: select a folder in Project, right-click on it, Create > Assembly Definition (smart) and check out all your cool assembly definitions in all the children of your selected folder.

## BuildToPlatform

Sometimes for multi-platform projects (and without cloud build), one must make multiple builds for different platforms.

This tool is a nifty editor window with buttons to build to selected platforms. It incorporates basically 2 clicks: Switch Platform.... and Build (or Build & Run). This way you don't have to wait for Switch Platform and click Build again later, but open the Build > Build To Platform window, select the platform, click build and go make a coffee and wait for your build.

## DebugButton

A super useful trick to make functions accessible in the Unity Inspector, for development/debug purposes. Simply add [DebugButton] before any function, and a nice button will appear in the (non-custom) inspector for that script. 

![image](https://user-images.githubusercontent.com/5824753/110955469-edc69d80-8349-11eb-9247-7e2b23077246.png)

Note: this was snatched from Kaae, a friend of a friend, and tweaked by me. There are also other versions if this out there, consider exploring them.

## DefaultAsset

An attribute to use in combination with ScriptableObjects, or maybe other types of assets and prefabs (but easier with scriptableobjects because they can be found easily). Write the attribute like this: 
```
[DefaultAsset("default fish")]
public GameObject defaultFish;
```
And it will try to assign the first result of the search result for that query. So as an example, the default fish is found by name. But if there is a duplicate file with the name that comes first, it's not so reliable so you should basically watch out with this one.

![image](https://user-images.githubusercontent.com/5824753/110957986-79d9c480-834c-11eb-85c5-27d4774c27ab.png)



## 

# License
MIT because you are welcome to use this stuff however you want, and credit is appreciated.
