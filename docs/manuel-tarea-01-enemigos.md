# Tarea 01 — Enemigos (momia) + Game Over

Responsable: Manuel Mata Campos
Rama: `feature/enemies-manuel` (parte de `main` actualizada)
Deadline interno: antes del avance del viernes 16 oct 2026
Reviewer: Juan Francisco Rodriguez Guerrero (JFRG)

---

## 1. Contexto del proyecto

Kikiri-9 "El Último Pan" es un auto-runner 2D para la Unidad 2 ordinario del grupo GIDS6091 de la UTNG (instructor: Rodríguez Guerrero Juan Francisco). Equipo: JFRG + Manuel. Modalidad parejas → 4 niveles jugables + jefe final (Coronela). Debe desplegarse en PC, Web y Android. Entrega final 27 nov 2026.

Diseño original: `Assets/_Draft_2_Kikiri9/Docs/Kikiri9_documentacion_oficial.pdf`. Rúbrica: `Assets/_Draft_2_Kikiri9/Docs/rubrica_unidad2.pdf`.

Repartición por GDD (cap. 7):
- JFRG: jugador, enemigos, Coronela, niveles 3-4, integración y build PC.
- Manuel: UI, HUD, menús, audio, guardado, niveles 1-2.

Este slice da a Manuel una pieza técnica concreta ahora (enemigo + game over) para su primer avance, sin pisar código del jugador.

## 2. Estado actual del repo (lo que ya existe)

Unity 6.3 LTS (6000.3.26f1). URP 2D. New Input System. Repo con Git LFS para PNG/WAV. Rama `main` compilable.

Scripts (todos en namespace `Kikiri9.<area>`):
- `Assets/_Project/Scripts/Player/PlayerController.cs` — auto-runner, salto, slide, lanzar pan, hooks a Animator.
- `Assets/_Project/Scripts/Camera/CameraFollow.cs` — sigue al pollo en X, Y fija.
- `Assets/_Project/Scripts/World/ParallaxLayer.cs` — scroll infinito con wrap.
- `Assets/_Project/Scripts/World/GroundFollower.cs` — no se usa actualmente (ground estático).
- `Assets/_Project/Scripts/Combat/Bread.cs` — proyectil: vuela, cae, parpadea 3 veces, splash de impacto, destruye.
- `Assets/_Project/Scripts/Combat/BreadLauncher.cs` — spawnea Bread con cooldown.
- `Assets/Editor/Kikiri9SheetSlicer.cs` — recorta las hojas de sprites del `_Draft_2_Kikiri9/Art/` según `Docs/medidas_recorte.csv` y las nombra `<accion>_01`..`<accion>_NN`.
- `Assets/Editor/PlayerAnimationBuilder.cs` — genera clips y AnimatorController para Kikiri9.
- `Assets/Editor/SandboxBootstrap.cs` — arma la escena Sandbox.unity con cámara, fondo (cielo + silueta), ground, pollo con sus componentes.

Escenas:
- `Assets/_Project/Scenes/Sandbox.unity` — escena de prueba actual. Tiene pollo corriendo, cae en adoquín, salta, se desliza, lanza pan que cae y hace splash. NO contiene enemigos todavía.

Sprites ya recortados (hoja → sub-sprites nombrados):
- `Characters/momia.png` → `caminar_01..06`, `derrota_01..06` (6 col × 2 filas). Pivote (0.5, 0.125). Orientación original: izquierda (ya mira hacia donde debe caminar la momia). Celda 512×384 px.
- `Characters/kikiri9.png`, `kikiri9_reposo_dano_derrota.png`, `coronela.png`, `zombi.png` — sus propios conjuntos.
- `Items/objetos_efectos.png` → `objetos_01..04` (pan verde recogible, pan proyectil, botella verde, botella dorada), `efectos_a_01..04` (proyectil espectral, impacto naranja, brillo verde, polvo blanco), `efectos_b_01..04` (salida pan, migas, dispersión morada, destello dorado).
- `Environment/suelo.png` → `suelo_01..04`, `plataformas_01..04`.
- `Environment/obstaculos_decoracion.png` → `obstaculos_*`, `decoracion_a_*`, `decoracion_b_*`.
- `UI/interfaz.png` → `hud_*`, `controles_*`, `botones_*`, `paneles_*`.
- `Effects/electricidad_activacion_cempasuchil.png` → `relampago_*`, `activacion_*`, `cempasuchil_*`.

Para regenerar todo: `Tools → Kikiri-9 → Slice Sprite Sheets`, luego `Build Player Animations`, luego `Setup Sandbox Scene`.

## 3. Reglas del enemigo según GDD

Del documento oficial (cap. 2, 3, 20):
- Momia: enemigo común de Guanajuato. Camina a la izquierda.
- Momia normal: 1 golpe de pan la neutraliza.
- Momia resistente: 2 golpes. Debe tener distintivo visual previo (NO sprite nuevo, mismo prefab con señal). Para este slice usar SOLO la normal (1 golpe).
- Velocidad del mundo nivel 1: 5-6 m/s. La momia se queda relativamente quieta o avanza despacio hacia la izquierda, aparentando moverse hacia el pollo.
- Sin gore. "Derrota" = animación de caer, no muerte sangrienta.
- Un solo prefab Shared; variantes vienen de datos, no de código duplicado.

Pollo (contexto para el game over):
- 3 corazones (fase posterior; para este slice el contacto = reset directo).
- 1 s de invulnerabilidad tras golpe (no aplica aquí; aquí el contacto es muerte instantánea = reset).

## 4. Alcance de esta tarea

### 4.1 Scripts a crear

Todos bajo `Assets/_Project/Scripts/`:

```
Enemies/
  EnemyMovement.cs
  EnemyHealth.cs
  EnemySpawner.cs
Session/
  GameOverHandler.cs
```

Editor:
```
Assets/Editor/EnemyAnimationBuilder.cs
```

Escena:
```
Assets/_Project/Scenes/Sandbox_Enemies.unity   (clonar de Sandbox.unity y modificar)
```

### 4.2 Especificación por archivo

**EnemyMovement.cs** (namespace `Kikiri9.Enemies`)
- Requiere Rigidbody2D (Kinematic).
- Serialize: `float speedX = -2f;` (hacia izquierda; el pollo corre a +4, delta aparente ~6 u/s).
- FixedUpdate: `rb.linearVelocity = new Vector2(speedX, rb.linearVelocity.y);` (preserva Y por si se agrega gravedad después).
- Auto-destroy si `transform.position.x < camera.x - 20f` (sale muy atrás de la vista; usa `Camera.main`).

**EnemyHealth.cs** (namespace `Kikiri9.Enemies`)
- Serialize: `int maxHealth = 1;`
- Serialize: `string defeatAnimTrigger = "Defeat";`
- Serialize: `float destroyAfterDefeat = 0.5f;`
- Referencia opcional a Animator (GetComponent al Awake).
- `public void TakeDamage(int amount)` → resta HP, si <= 0 llama a `Defeat()`.
- `Defeat()` → setea trigger animación, deshabilita collider y EnemyMovement, Destroy tras `destroyAfterDefeat` segundos.
- No hacer nada si ya está derrotado (flag `isDefeated`).

**EnemySpawner.cs** (namespace `Kikiri9.Enemies`)
- Serialize: `GameObject enemyPrefab;`
- Serialize: `Transform spawnReference;` (típicamente la cámara)
- Serialize: `float spawnAheadDistance = 15f;`
- Serialize: `float groundY = -2.65f;` (nivel visual del adoquín en Sandbox.unity)
- Serialize: `Vector2 intervalRange = new Vector2(2f, 3.5f);`
- Serialize: `int maxActive = 5;`
- Lleva lista de enemigos vivos; spawnea solo si `activeCount < maxActive`.
- Interval random dentro de intervalRange.
- Posición de spawn: `spawnReference.position.x + spawnAheadDistance, groundY, 0`.

**GameOverHandler.cs** (namespace `Kikiri9.Session`)
- Componente a poner en el GameObject Kikiri9.
- Serialize: `float restartDelay = 0.3f;`
- Serialize: `bool fadeToBlack = false;` (si true, agrega un overlay negro simple; para esta versión déjalo opcional, basta con reload directo).
- OnCollisionEnter2D y OnTriggerEnter2D: si `other.CompareTag("Enemy")` llama a `GameOver()`.
- `GameOver()` → debounce (una sola vez), opcional desactivar PlayerController, `Invoke(nameof(Reload), restartDelay)`.
- `Reload()` → `SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);`

### 4.3 Animaciones de la momia

**EnemyAnimationBuilder.cs** (namespace `Kikiri9.EditorTools`)

Menú: `Tools → Kikiri-9 → Build Enemy Animations`.

Genera en `Assets/_Project/Animations/`:
- `Momia_Walk.anim` (fila `caminar_01..06`, 10 fps, loop true)
- `Momia_Defeat.anim` (fila `derrota_01..06`, 8 fps, loop false)
- `Momia.controller` con parámetros:
  - Trigger `Defeat`
- Estados:
  - `Walk` (default) con clip Momia_Walk
  - `Defeat` con clip Momia_Defeat
- Transición `Walk → Defeat` por trigger `Defeat`, hasExitTime=false, duration=0.

Reutilizar el patrón exacto de `PlayerAnimationBuilder.cs` (AnimatorController.CreateAnimatorControllerAtPath, EditorCurveBinding.PPtrCurve, etc.). Hoja fuente: `Assets/_Draft_2_Kikiri9/Art/Characters/momia.png`.

### 4.4 Prefab de la momia

Crear a mano en Unity (NO script):
1. GameObject vacío llamado `Enemy_Mummy`.
2. SpriteRenderer con `caminar_01`. Sorting Order 15.
3. Rigidbody2D: Body Type `Kinematic`, Freeze Rotation Z.
4. CapsuleCollider2D: Direction Vertical, size ~ (1.2, 1.7), offset (0, 0.85). Afinar visualmente.
5. Animator: controller = `Momia.controller`.
6. EnemyMovement, EnemyHealth components añadidos.
7. Tag = `Enemy` (crear el tag en Project Settings si no existe).
8. Guardar como prefab en `Assets/_Project/Prefabs/Enemies/Enemy_Mummy.prefab`.

### 4.5 Escena de prueba

`Assets/_Project/Scenes/Sandbox_Enemies.unity`:
1. Duplicar `Sandbox.unity` → renombrar.
2. Agregar GameObject vacío `EnemySpawner` con componente `EnemySpawner`. Asignar enemyPrefab = Enemy_Mummy, spawnReference = Main Camera, groundY = Y correspondiente al adoquín (prueba con -2.65).
3. Agregar componente `GameOverHandler` al GameObject Kikiri9.
4. Build Settings: añadir ambas escenas (Sandbox y Sandbox_Enemies) para que `LoadScene(buildIndex)` funcione.

## 5. Integración con lo existente (NO lo hagas tú, Manuel; lo hará JFRG tras el merge)

Después del merge, JFRG modificará `Bread.cs` para que detecte colisión con tag `Enemy` y llame a `EnemyHealth.TakeDamage(1)` antes de ejecutar el splash. También moverá tu spawner y GameOverHandler a Sandbox.unity principal.

## 6. Lo que NO debes tocar

Para evitar conflictos de merge, Manuel NO modifica:
- `PlayerController.cs`
- `BreadLauncher.cs`
- `Bread.cs`
- `CameraFollow.cs`
- `ParallaxLayer.cs`
- `SandboxBootstrap.cs`
- `Kikiri9SheetSlicer.cs`
- `PlayerAnimationBuilder.cs`
- `Sandbox.unity`
- `ProjectSettings/*` (excepto TagManager.asset para agregar el tag Enemy — avisar en el PR)
- `Packages/manifest.json`

## 7. Convenciones del proyecto

Del proyecto en Claude Projects (y lo acordado):
- Código en inglés (clases, métodos, variables). Comentarios en español cuando aporten, no obligatorios.
- UI y textos para el usuario en español.
- TypeScript estricto no aplica aquí (Unity C#), pero: sin `object` genérico, usar tipos específicos. Sin `dynamic`.
- KISS: código simple sin over-engineering. No introducir ScriptableObjects, patrones observer, pooling ni DI para este slice. Hazlo directo con MonoBehaviours y campos SerializeField.
- Un GameObject puede ser destruido con Destroy; no reutilizar instancias (pooling se añade después si hay problema de rendimiento).
- Prohibidos emojis en código, comentarios y commits.
- Sin NgRx-equivalentes. Estado local al componente.
- Namespaces: `Kikiri9.Enemies`, `Kikiri9.Session`, `Kikiri9.EditorTools`.

## 8. Flujo de Git

```
git checkout main
git pull origin main
git checkout -b feature/enemies-manuel

# Trabajar, commit frecuente con prefijos:
# feat(enemy): add EnemyMovement
# feat(enemy): spawner with interval range
# feat(session): game over handler reloads scene
# chore(scene): add Sandbox_Enemies for testing

git push -u origin feature/enemies-manuel
# Abrir PR en GitHub con reviewer JFRG
```

Commits atómicos. No mezclar varios features en un commit. Pre-pull antes de cada sesión por si JFRG empujó algo.

## 9. Checklist de pruebas (antes de abrir el PR)

Abrir `Sandbox_Enemies.unity`, Play, y verificar:

- [ ] La momia aparece a la derecha de la cámara.
- [ ] Camina a la izquierda a velocidad razonable.
- [ ] Se destruye sola si queda muy atrás de la cámara.
- [ ] El spawner no excede `maxActive`.
- [ ] Al chocar con el pollo, la escena se recarga tras 0.3s.
- [ ] La escena recargada vuelve a spawnear momias normalmente (no quedan referencias zombie).
- [ ] La animación caminar_01..06 se reproduce en loop.
- [ ] Al llamar manualmente `GetComponent<EnemyHealth>().TakeDamage(1)` desde Debug (o botón temporal), la momia reproduce defeat y se destruye.
- [ ] Console limpia, sin warnings ni errores en rojo.
- [ ] En Sandbox.unity (sin tocarla) todo sigue funcionando igual.

## 10. Entregable

- Rama `feature/enemies-manuel` pusheada.
- PR abierto a `main` con descripción breve de qué hace y screenshots/GIF del test funcionando.
- Reviewer: JFRG.
- Después del merge, JFRG integra Bread→Enemy damage y actualiza Sandbox.unity.

---

## Prompt para iniciar el chat (cópialo y pégalo tal cual)

```
Soy Manuel Mata Campos, trabajando con JFRG en Kikiri-9 (auto-runner 2D para Unity 6.3 LTS). 
JFRG me pasó la carpeta del proyecto completo y el archivo docs/manuel-tarea-01-enemigos.md 
con mi tarea asignada.

Lee ese MD completo primero. Ahí está todo: contexto del juego, estado actual del repo, 
scripts que debo crear, especificaciones exactas, convenciones, qué puedo y qué NO puedo tocar, 
flujo de Git y checklist de pruebas.

Objetivo: ejecutar la tarea tal como está definida. 
1. Crea los scripts especificados en los namespaces correctos.
2. Genera el AnimatorController de la momia con el patrón del PlayerAnimationBuilder existente.
3. Dime cuándo tengo que hacer pasos manuales en el Unity Editor (crear prefab, duplicar escena, 
   configurar componentes en Inspector, añadir tag Enemy).
4. Respeta convenciones KISS, sin over-engineering, código C# limpio, nombres en inglés, 
   sin emojis.
5. Cuando compile y pase el checklist del MD, prepárame los comandos git para commit + push + PR.

Puedes leer cualquier script existente en Assets/_Project/Scripts/ y Assets/Editor/ para 
entender el estilo. Antes de escribir cada archivo nuevo, confírmame rápido lo que vas a hacer. 
No toques nada de la lista "NO debes tocar" del MD.
```
