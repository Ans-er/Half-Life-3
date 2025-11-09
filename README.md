# HL3 – Multiplayer

Funktionen
- Host/Clients per UI
- Schaden durch Schüsse und Bomben (Server-autorisiert)
- Sofortiger Respawn am Startpunkt

Start
1. Szene laden
2. Im UI „Host“ oder in zweiter Instanz „Client“ klicken

Setup (Prefabs kurz)
- Player
 - Komponenten: "NetworkObject", "NetworkTransform", "CharacterController", "PlayerInput", "FPS_Character_Controller"
- Bullet
 - Komponenten: "NetworkObject", "NetworkTransform", "Rigidbody", "Collider", Script "bullet_behaviour"
- Bomb
 - Komponenten: "NetworkObject", "NetworkTransform", "Rigidbody", "Collider", Script "Bomb"

Steuerung
- Bewegung: WASD, Sprint: Shift, springen: Space
- Schießen: Linksklick
- Bombe: Rechtsklick

Balancing (im inspector einstellbar)
- Spieler-HP
- Schusskraft
- Bombenkraft
- Projektilschaden
- Bombenschaden/Radius/Dauer