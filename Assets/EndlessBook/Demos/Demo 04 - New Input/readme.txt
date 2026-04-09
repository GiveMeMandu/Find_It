In order to use the new Unity Input System, you will need to do the following:

1) Be sure you are using EndlessBook v 1.12.0+.

2) Install the new system through the Unity Package Manager:
    a) Navigate to Window > Package Manager in the Unity Editor.
    b) Open the Input System package and select the latest version. Choose Install or "Update to xxx".

3) Import the "Demo 04 - New Input" package into your project. This is not installed by default becuase not everyone will have the prerequisites in Step #1.

4) Navigate to Edit > Project Settings > Player in the Unity Editor. Find "Active Input Handling". Choose "Input System Package (new)".

5) Delete the assembly definition file in the "Assets/EndlessBook" directory called "EndlessBookAssembly". Since the files for Demo 4 were not present when generating this assembly definition, it will have conflicts. You can always re-create the file if you need it.

Note: When running Demo #2 or #4, you will see a debug line in the editor console upon Awake showing which input system you are using.

Note: the other demos in this project use the old input system. So to run those, you will need to go back to Step #4 above and select "Input Manager (Old)".