Authors: Andrew Huang and Aidan Naito

11/26/21

Client:
Sprites
We used the provided sprites for the background, tanks, turrets, walls, and projectiles.
For powerups, we created black circles surrounded by white rings.

Animations
For beams, they are white lines that slowly grow wider and fade.
For deaths, small squares that represent debris move in random directions.

Features
Disconnecting will also play the death animation.
We implemented a way to reconnect to the server if there was a connection problem.

Some code (such as DrawObjectWithTransform) was copied from Lab11.

Server: 

Features - 
Ignores any malformed requests from clients

Respawning tanks can't respawn on top of a power up

Spawning power ups can't spawn on top of a tank

Extra: New settings for max power up, max delay for spawning power ups, and invincibility frames

Extra: Tanks have a set amount, as dicatated by settings, of invincibility frames meaning that they can't be hurt for a while after they respawn

Extra: If you die, your score decrements by 1 -- this can go negative (set ScoreDecrement to true to enable feature)