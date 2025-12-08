<ins>Controls:</ins>

<i>Xylo</i> can be fully controlled with just the mouse, but there are a few useful hotkeys:

* ESC: Pause
* Space: Replay the level's melody
* P: Open the piano
  
There are also a few debug keys for testing:

* Shift + D + 1: Deletes all save data

<ins>In-Editor Debugging:</ins>

* Overwrite saved data in new play sessions: Managers > LoadingManager > Turn on AlwaysResetData
* Enter a specific level in World1 scene: Managers > LoadingManager > OverrideCurrentLevel = 0 (Level 1) or 1 (Level 2)
* Automatically trigger a win at the end of the playthough of each section: LevelManager > LevelManager > Turn on AutoWin
* Jump around different sections without having to complete the previous section: LevelManager > LevelManager > Turn on FreeAdvance
* Start the marble at a specific position with a specific velocity: LevelManager > LevelManager > Turn on UseManualStart and enter ManualPosition and ManualVelocity vectors

<ins>Credits:</ins>

All code, art, sound, and design by Samantha Mroz unless specified otherwise

Fonts Used:
* [Arista Pro](https://www.dafont.com/search.php?q=arista+pro)
* [Altone Trial](https://www.dafont.com/altone.font)

Art Credits:
* [3D Leap Land](https://essssam.itch.io/3d-leap-land): Essssam via itch.io
* [Simple Toon Shader](https://assetstore.unity.com/packages/vfx/shaders/simple-toon-185038): Dmitry Chalovskiy via Unity Asset Store
* [Panoramic Cartoon Skybox](https://assetstore.unity.com/packages/2d/textures-materials/sky/panoramic-cartoon-skybox-220659): Awaii Studios via Unity Asset Store

Code Credits:
* [LeanTween](https://assetstore.unity.com/packages/tools/animation/leantween-3595?srsltid=AfmBOoqsmcbh6Z0t4ixhgweImlnU-jq2ws1UJZfidHzhO8qk8ICxaf4Y): DentedPixel via UnityAssetStore
