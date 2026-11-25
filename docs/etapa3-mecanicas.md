# Etapa 3: Combo + Hit-Stop + Shake + Death FX + Enemigos nuevos + Oleadas + Pan Bomba

## Lo que YA quedo en codigo (compila)

### Feel / FX (automatico, no requiere setup en escena)
- `Session/FX/HitStop.cs` - freeze 0.1x timeScale 0.03s al matar
- `Session/FX/CameraShake.cs` - shake Perlin, se auto-agrega a Camera.main
- `Session/FX/ComboManager.cs` - combo x1->x2->x3->x5->MAX. Rompe solo con danio
- `Session/FX/DeathFeedback.cs` - zoom + fade negro + slow motion al morir
- `Session/FX/FxBootstrapper.cs` - instancia los singletons al cargar cualquier escena Level*

Enganches:
- EnemyHealth.Defeat multiplica puntos * combo, notifica kill, hit-stop, shake
- PlayerHealth.TakeDamage rompe combo, shake fuerte, hit-stop, dispara death feedback si muere
- PlayerController.StartSlide instancia SlideHitbox (tag Enemy, dano slide) para xolo

### Enemigos nuevos (scripts listos, falta prefab + animator)
- `Enemies/New/EnemyVulnerability.cs` - define a que es vulnerable
  - mariachi: solo pan en el aire (breadOnlyInAir = true)
  - xolo: solo slide (bread = false, rafaga = false)
  - zombi saltarin: todos
- `Enemies/New/MariachiGhost.cs` - vuelo senoidal
- `Enemies/New/ZombiJumping.cs` - patron crouch/prepare/air/land
- `Enemies/New/XoloRunner.cs` - carrera pegada al suelo
- `EnemyHealth.TakeDamage(int, DamageSource, Vector3)` sobrecarga que filtra por EnemyVulnerability

### Oleadas (sistema generico)
- `Levels/Waves/WaveDefinition.cs` - ScriptableObject (triggerMeters, entries, rewards, endsLevelOnClear)
- `Levels/Waves/WaveManager.cs` - dispara oleadas segun MetersAdvanced
- `Levels/Waves/WaveBanner.cs` - canvas overlay propio para los banners (sin tocar HUD)

### Pan bomba
- `Combat/PanBomba/PanBombaAmmo.cs` - stock separado
- `Combat/PanBomba/PanBombaPickup.cs` - pickup +1 stock
- `Combat/PanBomba/PanBomba.cs` - explosion radio 2.5u, dano 3 por enemigo
- `BreadLauncher.TryLaunch` da prioridad al pan bomba si hay stock
- `UI/PanBombaHUDBadge.cs` - auto-agrega sibling a la derecha del contador de pan normal
  (no toca ni mueve el HUD existente)

### Combate
- `Player/SlideHitbox.cs` - hitbox trigger durante slide que dana con DamageSource.Slide

## Setup en Unity (manual, no lo pude hacer por script)

### 1. Correr importer
`Tools > Kikiri-9 > Import New Content (Enemies + Oleadas + Audio)`
Slicea los sheets, importa banners, VFX y audio.

### 2. Crear prefabs de enemigos nuevos
Para cada uno (mariachi, zombi_saltarin, xolo):
1. GameObject vacio llamado `Enemy_Mariachi` / `Enemy_ZombiSaltarin` / `Enemy_Xolo`
2. Tag `Enemy`
3. SpriteRenderer con el sprite `pose.png` del enemigo
4. Rigidbody2D (Kinematic para mariachi, Dynamic para los otros)
5. CapsuleCollider2D ajustado a la silueta
6. Scripts:
   - Mariachi: MariachiGhost + EnemyHealth (hp 1, puntos 150) + EnemyMovement NO + EnemyVulnerability (bread true, slide false, rafaga false, breadOnlyInAir true)
   - Zombi saltarin: ZombiJumping + EnemyHealth (hp 1, puntos 120) + EnemyVulnerability (todos true)
   - Xolo: XoloRunner + EnemyHealth (hp 1, puntos 100) + EnemyVulnerability (bread false, slide true, rafaga false)
7. Escala transform.localScale = 1.1 (los tres, que se vean un pelin mas imponentes)
8. Animator con los sprites sliceados (ver docs/enemigos_oleadas/GUIA_IMPLEMENTACION.md)
9. Guardar en `Assets/_Project/Prefabs/Enemies/`

### 3. Crear prefab Pickup_PanBomba
1. Duplicar `Pickup_BreadBoost.prefab`, renombrar a `Pickup_PanBomba.prefab`
2. Cambiar sprite al del pan bomba (asset siguiente que me pases)
3. Quitar BreadBoostPickup + BreadBoostAura
4. Agregar PanBombaPickup
5. Tag: Pickup

### 4. Crear 3 WaveDefinitions para L3
`Create > Kikiri9 > Wave Definition` x 3:

**Wave 1** (triggerMeters 400, bannerLabel "OLEADA 1/3", spawnInterval 0.9)
- entries: 2 Enemy_Zombie, 2 Enemy_Mummy, 1 Enemy_ZombiSaltarin
- rewards: 3 Pickup_Bread, 1 Pickup_Bottle_Green
- endsLevelOnClear: false

**Wave 2** (triggerMeters 750, bannerLabel "OLEADA 2/3", spawnInterval 0.8)
- entries: 2 Enemy_MummyResistant, 2 Enemy_Xolo, 2 Enemy_Mariachi
- rewards: 3 Pickup_Bread, 2 Pickup_Bottle_Green, 1 Pickup_PanBomba
- endsLevelOnClear: false

**Wave 3** (triggerMeters 1000, bannerLabel "OLEADA FINAL", spawnInterval 0.7)
- entries: 3 Enemy_ZombiSaltarin, 2 Enemy_Mariachi, 2 Enemy_Xolo, 1 Enemy_MummyResistant
- rewards: 5 Pickup_Bread, 3 Pickup_Bottle_Green, 2 Pickup_PanBomba, 1 Pickup_BreadBoost
- endsLevelOnClear: TRUE

### 5. Agregar WaveManager a escena Level03
1. Abrir Level03_Guanajuato.unity
2. GameObject vacio `WaveManager` con el componente WaveManager
3. Asignar los 3 WaveDefinition en el array `waves`
4. Asignar bannerStartSprite = oleada_inicio.png, bannerCompleteSprite = oleada_completa.png
5. Asignar waveStartSfx = oleada_inicio.wav, waveCompleteSfx = oleada_completa.wav
6. Desconectar el MiniBossSpawner en la escena (desactivar el GameObject,
   NO borrarlo - se reactiva en L4)

### 6. Asignar sprites del pan bomba al BreadLauncher del pollo
En cada Level01/02/03 (en el prefab del pollo o la escena):
- BreadLauncher.panBombaSprite = (sprite del pan bomba, cuando lo tengas)
- BreadLauncher.panBombaExplosionSprite = (sprite de explosion, placeholder si no)

### 7. Asignar sprite del icono de pan bomba al HUD badge
En FxBootstrapper se crea automaticamente pero no tiene sprite asignado.
Hay que crear un GameObject en escena con PanBombaHUDBadge y asignar el sprite,
o bien agregar un parametro static que lo inyecte.

### 8. Audios: asignar clips en los scripts
- ComboManager.comboSfx = combo.wav (via Inspector del GameObject ComboManager que cree el bootstrapper, o pre-agregar en escena)
- WaveManager usa los que le pases
- PanBombaPickup.pickupSfx = pan_dorado.wav (reusamos ese)
- PlayerSfx: swap rafaga sfx con rafaga.wav del paquete nuevo
- Musica: asignar dolores_hidalgo_loop.wav a Level01/02, guanajuato_loop.wav a Level03

