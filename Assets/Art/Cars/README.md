# Car sprites

Source: "2D Race Cars" by looneybits — https://opengameart.org/content/2d-race-cars — licensed CC0 (public domain).

| Sheet      | Original file         | Car         |
|------------|-----------------------|-------------|
| racer.png  | cars/pitstop_car_1.png | Red Arrow   |
| sports.png | cars/pitstop_car_9.png | Green Flash |
| rally.png  | cars/pitstop_car_4.png | Blue Bolt   |

Each PNG is a horizontal three-cell sheet: straight, left-steering and right-steering frames, facing up, transparent background. The originals face down and have no steering art, so each frame was rotated 180° and the left/right frames were made by turning the two front wheels 18°. The Unity setup tool (`Tools/Racing/Rebuild Pastel Assets and Scenes`) trims the transparent margins with one shared rectangle and stores the three sprite subassets in the matching `Assets/Resources/Cars/*.asset`. Do not delete the source textures: the sprites reference them.
