# crj3d-clone
Color Roll Jam 3D Clone for a Case Study

# Semi-functional level editor

https://github.com/user-attachments/assets/0142e60d-94de-4b36-a27b-59133de019ee


This editor caches the IDs of roll objects to check if they are under any other roll or not, does this with physics simulation and positions checks on editor so there's no runtime overhead.

If level design is incorrect (Collisions or Y level) editor does nothing to fix it, however since these IDs are avaible in the inspector Level Designer can handle the situation on their own.

## Known Bugs and Limitations of Level Editor:
+ Jittery game view.
+ World Matrix/Camera sometimes get broken and clicking the rolls or their handles are buggy. (Available on the video, changing camera angle works as a band aid fix.)

## Key Points
+ 0 Runtime Singletons
+ Every roll and target boxes are pooled by [Unity's Pool](https://docs.unity3d.com/ScriptReference/Pool.ObjectPool_1.html).
+ Levels are NOT prefabs, instead Scriptable Objects are holding only the needed data. (Position, scale, color etc. etc.)

### Used Assets
+ DOTween

### I would definetly use these assets if third party code was allowed to reduce development time from 3-4 to ~2.
+ Zenject
+ Odin Inspector
