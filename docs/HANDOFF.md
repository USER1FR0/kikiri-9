# Kikiri-9 · Handoff para otro desarrollador o IA

Proyecto Unity 2D (pixel art) con 3 niveles de auto-runner. Estado: en producción.

## 1. Estructura del proyecto

```
C:\kikiri-9\
  Assets\
    _Project\
      Art\                 Fondos V3 adaptativos, iconos HUD, power-ups
      Animations\          .anim y .controller del pollo, enemigos, botellas
      Prefabs\
        Enemies\           Enemy_Zombie, Enemy_Mummy, Enemy_ZombieResistant,
                           Enemy_MummyResistant, Enemy_MiniBossMomia
        Obstacles\         Obstacle_Cart, Obstacle_Tomb, Obstacle_LowBranch
        Pickups\           Pickup_Bread, Pickup_BreadBoost, Pickup_Bottle_*
        UI\                UI_Root
      Scenes\              Boot, MainMenu, Level01/02/03, Sandbox
      ScriptableObjects\
        Levels\            Level01/02/03_*.asset (LevelDefinition)
      Scripts\
        Camera\            CameraFollow
        Combat\            Bread, BreadLauncher
        Enemies\           EnemyHealth, EnemyMovement, EnemySpawner,
                           EnemyIgnoresSiblings, EnemyDamageFlash,
                           EnemyHealthBar, MiniBossMomia
        Levels\            LevelController, LevelSpawner, LevelDefinition,
                           MiniBossSpawner
        Player\            PlayerController, PlayerAmmo, PlayerPunch,
                           InfiniteBreadBuff, EstelaRafagaDriver, PlayerSfx
        Session\           GameSession, GameStateManager, GameState,
                           PlayerHealth, ScoreTracker, BossArenaManager
        UI\                HUDController, LevelCompletePanel, PausePanel,
                           TutorialPanel, GameOverPanel, LevelIntroBanner,
                           LevelIntroHook, InfiniteBreadBadge, PanDoradoDriver,
                           RafagaVisualDriver, RafagaButtonHook,
                           PunchChargeIndicator, PunchChargeIndicatorInstaller
        World\             WorldScroller, ParallaxLayer, BackgroundFollowCamera,
                           ObstacleScroller, GroundFollower, ArenaCameraLock,
                           ClampPlayerToArena, BreadPickup, BreadBoostPickup,
                           BottlePickup, PickupFloater, VfxSpawner,
                           FondoAdaptativoKikiri9, FondoAdaptativoDriver
        Visuales\          KikiriPanDoradoVisual, KikiriRafagaVisual,
                           KikiriPulsoSuave   (del kit power-ups)
    Editor\                Herramientas de Editor (ver seccion 5)
    _Draft_2_Kikiri9\      Arte legacy (sprites del pollo, UI, enemigos)
```

## 2. Mecanicas principales

### 2.1 Pollo (PlayerController)

- **Auto-run**: `runSpeed = 4 u/s` (constante).
- **Salto**: espacio / W / flecha arriba. `maxJumpsInAir` se lee en VIVO cada frame desde `LevelController.Definition.maxJumpsInAir`. En arena (`BossArenaManager.IsActive`) se fuerza 1.
- **Slide**: S / flecha abajo / Shift. Encoje collider a 50% altura durante `slideDuration = 0.9s`.
- **Lanzar pan**: J / click izquierdo. Consume 1 pan de `PlayerAmmo` (o nada si `InfiniteBreadBuff` esta activo).
- **Rafaga**: K / click derecho / boton Y. Activa anim de slide (`BeginPunchAnimation`), 0.5s de invulnerabilidad, hitbox que dana 2 a enemigos a 1.2u al frente, cooldown 9s.
- **Arena del jefe**: Movimiento horizontal libre con A/D, clamp `±7u` del centro del arena (`ClampPlayerToArena`).

### 2.2 Niveles

Definidos por `LevelDefinition` (ScriptableObject). Campos clave:
- `worldSpeed / maxSpeed / targetDistance`: progresion de velocidad.
- `enemyPrefabs / obstaclePrefabs / pickupPrefabs`: pool de spawn.
- `enemyWeight / obstacleWeight / pickupWeight`: pesos de seleccion (si todos 0, reparto equitativo).
- `spawnIntervalRange / maxActiveEntities / minSpawnSeparation`: control de densidad.
- `pickupHighChance / pickupHighYBonus`: % de pickups que vuelan alto (requieren salto).
- `maxJumpsInAir`: 1 en L2/L3, 2 en L1.
- `breadLowRatio`: si `currentBread < enemies * ratio`, fuerza spawn de pan.
- `safeStartMeters`: primeros N metros sin enemigos/obstaculos (30 L2, 40 L3).

### 2.3 Jefe L3

- Prefab `Enemy_MiniBossMomia`, aparece cuando `MetersAdvanced >= 1000m` (L3 targetDistance = 1200m).
- Al spawnear: activa `BossArenaManager.IsActive`, bloquea cámara (`ArenaCameraLock`), clampa pollo (`ClampPlayerToArena`), desactiva `LevelSpawner`, baja pitch/volumen de la musica.
- Comportamiento: wander ±3u del centro del arena, salto cada 2.2s, ataque de embestida cada 3.5s con telegraph (flash rojo) + lunge, drop de items cada 2.5s (55% pan, 35% botella verde, 10% pan dorado).
- HP 6, contact damage 1 (cooldown 1s), 500 pts al derrotarlo.
- Al derrotarlo: lluvia de 6 panes + 1 pan dorado, restaura mapa normal.

### 2.4 Pan dorado (InfiniteBreadBuff)

- Pickup especial `Pickup_BreadBoost`. Al tomarlo activa `PlayerAmmo.infinite = true` por 10s.
- Visual: sprite `pan_dorado_infinito.png` + halo pulsante (`KikiriPulsoSuave`).
- HUD: `PanDoradoHUD` Canvas con indicador_marco + barra dorada que vacia. `PanDoradoDriver` lo alimenta.

### 2.5 Boton rafaga (UI)

- `RafagaCanvas` en cada nivel, boton con `boton_listo.png` + mascara `mascara_recarga.png` radial. `KikiriRafagaVisual` muestra cooldown, `RafagaVisualDriver` lo alimenta desde `PlayerPunch.Remaining`.
- Clickeable: `RafagaButtonHook` llama `PlayerPunch.FireFromExternal()`.
- Estela: `estela_derecha.png` como hijo del pollo, `EstelaRafagaDriver` la enciende durante la animación.

### 2.6 Fondos (Backgrounds V3 adaptativos)

- 3 capas por escena: **Cielo** (no parallax), **Horizonte** (parallax 0.2), **Fachada/Near** (parallax 0.45).
- `BackgroundsV3Builder` las monta con sortingOrder negativo (-30/-20/-10) para que gameplay (0+) quede delante.
- `Camera.backgroundColor` = tono oscuro de la ciudad para tapar el inferior sin huecos.
- `GroundFill` solido 120x30 debajo del suelo.
- Suelo real en Y=**-3.2** (antes -2). Al correr `Apply Backgrounds V3` se ajusta tambien `LevelSpawner.groundY` (a -3.95) y `MiniBossSpawner.spawnY` (-2.7) para que enemigos/obstaculos no floten.

## 3. Game states (GameStateManager)

- `Playing`: normal.
- `Tutorial`: panel de controles abierto (Time.timeScale = 0).
- `Paused`: menu de pausa (Time.timeScale = 0).
- `LevelOutro`: tras meta, pollo corre a la derecha, spawner desactivado, camara congelada.
- `LevelComplete`: panel final con narrativa.
- `GameOver`: panel de muerte.

`BossArenaManager.IsActive` es un flag ORTOGONAL que cualquier sistema puede consultar (WorldScroller, PlayerController, LevelSpawner).

## 4. Flujo de escena (al cargar un Level)

1. `GameSession` singleton (de Boot) persiste.
2. `Session` GameObject del nivel contiene:
   - `GameStateManager`
   - `PauseInput`
   - `ScoreTracker`
   - `WorldScroller`
   - `LevelController` (+ `LevelDefinition`)
   - `MusicController` + AudioSource
   - `LevelIntroHook` (spawna banner al pasar a Playing)
   - `BossArenaManager` (en los 3 niveles, solo se activa en L3)
   - `MiniBossSpawner` (solo L3)
3. Pollo (tag "Player") con PlayerController, PlayerHealth, PlayerAmmo, BreadLauncher, PlayerPunch, EstelaRafagaDriver.
4. UI_Root prefab instanciado por `BuildUI`.
5. `RafagaCanvas` y `PanDoradoHUD` canvas separados.
6. BgRoot adaptativo con 3 capas + GroundFill.
7. LevelSpawner spawn de enemigos/obstaculos/pickups al frente de la cámara.

## 5. Herramientas de Editor (menu `Tools > Kikiri-9`)

Base (del proyecto original):
- `Build Main Menu`: genera Boot y MainMenu + registra escenas en Build Settings.
- `Slice Sprite Sheets`: corta los sheets de arte original (preserva GUIDs).
- `Reset Import Settings (no slicing)`: aplica settings sin tocar rects.
- `Build Level 01 Scene`: regenera L1 desde cero.
- `Register Scenes In Build`: agrega Boot/Menu/L1 a Build Settings.

Mios (del refactor):
- `Build Levels 2 And 3`: regenera prefabs variantes (zombi/momia resistentes, low branch, bread boost, mini-boss), crea LevelDefinition L2/L3 con pesos, crea escenas L2/L3 duplicando L1, encadena LevelCompletePanel.nextSceneName.
- `Apply Adaptive Backgrounds V3 to Level X / ALL Levels`: monta las capas de fondo V3 y ajusta spawn Y.
- `Add Punch Mechanic to ALL Levels`: agrega PlayerPunch al pollo.
- `PowerUps > Install All`: importa assets del kit pan dorado + rafaga, actualiza Pickup_BreadBoost con pan_dorado_infinito, monta RafagaCanvas y PanDoradoHUD en los 3 niveles, adjunta estela al pollo.
- `Relink UI Sprites`: re-liga por nombre las referencias del UI_Root cuando el slicer genera nuevos GUIDs.

## 6. Convenciones

- **Codigo**: ingles para variables/funciones. UI y labels en español.
- **KISS**: sin over-engineering, sin frameworks pesados.
- **Patrones**: SerializeField + Inspector para dependencias; nada de DI.
- **Signals (Angular)**: N/A (proyecto Unity C#).
- **Singletons**: GameStateManager, WorldScroller, BossArenaManager, GameSession, ScoreTracker.
- **Comentarios**: solo donde el codigo no se explica solo. Sin emojis.

## 7. Bugs conocidos / pendientes

| Bug | Sintoma | Donde buscar | Plan sugerido |
|-----|---------|--------------|---------------|
| Enemigos a veces flotan | Posicion Y mal calibrada con sprite pivot | `LevelSpawner.groundY` + pivot de cada prefab de enemigo | Verificar pivot del sprite `momia.png`/`zombi.png`, ajustar spawnY o pivot del prefab. |
| Jefe L3 patron rigido | Movimientos estilo "robot" | `MiniBossMomia.FixedUpdate` y `LungeAttack` | Añadir estados (idle, alert, lunge, recover) con tiempos aleatorios. |
| Pollo salto doble en L1 | L1SO tiene `maxJumpsInAir = 2` por defecto | `Level01_Dolores.asset` | Cambiar a 1 si se quiere salto unico universal. |
| Rafaga sin anim a veces | Animator no tiene transicion "IsSliding=true" | `Kikiri9.controller` Animator state machine | Añadir estado "Rafaga" o transicion directa a "deslizar" desde cualquier estado si IsSliding=true. |
| Lag intermitente | Muchos GameObjects renderizando parallax | `BackgroundsV3Builder` tileFactor, `LevelSpawner.maxActive` | Bajar tileFactor a 2, maxActive a 5, limitar VfxSpawner. |
| Selector de niveles | No implementado | `MainMenu.unity`, kit `mapa/` | Hacer en segunda etapa (user explicito). |

## 8. Como pedir cambios a otro chat / IA

Templates de instrucciones utiles:

**Fix de bug**: "En el proyecto Kikiri-9 (ver C:\kikiri-9), el componente X en el archivo Y.cs tiene el bug Z: [descripcion]. Reemplaza el metodo M por una version que [solucion]."

**Nueva mecanica**: "Agrega una nueva mecanica M al pollo. El input es tecla K, cooldown Xs, dano Y. Reutiliza el patron de PlayerPunch.cs. El HUD indicator va en esquina Z del PanDoradoHUD canvas."

**Nuevo nivel**: "Crea un Level04_Zacatecas usando Levels2And3Bootstrap.cs como template. Fondos nuevos en Backgrounds_V3/zacatecas_*.png. Boss: nuevo mini-boss MariachiCharro. Target 1500m, speed 10/13."

**Asset refactor**: "Los sprites del tipo X se desacomodaron. Corre Tools > Kikiri-9 > Slice Sprite Sheets para re-cortarlos preservando GUIDs y luego Tools > Kikiri-9 > Relink UI Sprites para reatar el UI."

## 9. Dependencias externas / assets

- Unity 6.3 LTS (6000.3.26f1).
- Input System package.
- Universal RP (2D).
- Cinemachine (usado en L2/L3 escenas).
- TextMeshPro.
- Arte: carpetas `_Draft_2_Kikiri9/Art` (sprites originales), `_Project/Art/Backgrounds_V3` (fondos adaptativos), `_Project/Art/PowerUps` (kit pan dorado + rafaga).

## 10. Decisiones de diseno recientes

- Fondos adaptativos V3 reemplazan parallax V2 tileado.
- Suelo bajado a -3.2 para dar espacio vertical al arte.
- Boss L3 estilo Metal Slug (freeze mundo + arena cerrada).
- Rafaga como pan gratuito con cooldown largo.
- Safe zone al inicio de L2/L3 (30-40m sin enemigos).
- Peso de pool: L2 60/10/30, L3 65/8/27 (enemigos/obstaculos/pickups).

---

_Ultima actualizacion: 2026-11-10. Autor: colaborador IA sobre base del proyecto._
