TOKYO NIGHT RUNNER  -  3D endless runner for Unity 6
=====================================================
A neon, rainy Tokyo night: run down a street of lit-up buildings with
Japanese neon signs, torii gates, lanterns, vending machines and a glowing
Tokyo Tower in the distance. Dodge barriers, taxis and vans, slide under
gantries, collect coins. Music and sound effects are generated in code.

HOW TO RUN (2 minutes)
1. Unity Hub -> New project -> "3D" template (Built-in or URP both work),
   Unity 6 recommended.
2. Copy the "Assets" folder from this zip into the new project
   (let it merge with / replace the existing Assets folder).
3. Wait for Unity to compile, then in the top menu click:
      Tokyo Runner -> Create Scene and Play
   (or open any scene and just press Play - the game builds itself at runtime).

CONTROLS
  A / D or Left / Right arrow ... change lane
  W / Up arrow / Space .......... jump (over low barriers)
  S / Down arrow / Left Ctrl .... slide (under overhead gantries)
  Space / Enter / tap ........... start / restart
  Touch screens: swipe left / right / up / down

OBSTACLES
  Striped barrier ... jump          Overhead gantry ... slide
  Yellow taxi / green van ... change lane

TROUBLESHOOTING
- Pink or invisible objects in a BUILD: Edit > Project Settings > Graphics >
  Always Included Shaders, add: Standard, Unlit/Color, Unlit/Texture,
  GUI/Text Shader (and for URP: Universal Render Pipeline/Lit and Unlit).
  Playing inside the Editor does not need this.
- Keys do nothing: Edit > Project Settings > Player > Other Settings >
  Active Input Handling = "Both".
- Japanese neon signs show as boxes: the game uses an installed Japanese font
  (Yu Gothic, Meiryo, Hiragino, Noto Sans CJK...). Without one it falls back
  to English signs (RAMEN, SUSHI, TOKYO...) automatically.
- Too slow on a weak PC: lower rain in GameManager.MakeRain (rateOverTime).

CODE MAP (Assets/Scripts)
  Bootstrap.cs ........ starts the game in any scene
  GameManager.cs ...... states, camera, fog, rain, collisions, UI
  PlayerController.cs . runner model, lanes, jump, slide, animation
  Segment.cs .......... one 40 m street slice, obstacles, coins
  Gfx.cs / Mt.cs ...... primitives, neon signs, torii, tower, materials
  Sfx.cs / Inp.cs ..... generated audio, keyboard + swipe input
