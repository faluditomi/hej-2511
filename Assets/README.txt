Chasing Guard AI V1

This version comes with a player controller. The guard takes into account when the player is sprinting and, even if they are not looking in that direction but they are in range, they will hear them and pursue.


Setup:

1. In the Scene, you'll need:
	- NavMesh Surface, baked.
	- Guard Container (empty game object with a GuardContainer script).
	- Your obstacles that the AI can't see through should be on the "Cover" layer.
	- Player prefab
		- You have to get the Cinemachine package and remove the Default camera to get this player to work.
		- You can also use a different player 
2. Now you can pop in the Guard prefabs, set them up with Waypoints (prefab), play around with the numbers and it should work.