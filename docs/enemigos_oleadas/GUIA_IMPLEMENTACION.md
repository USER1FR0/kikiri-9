# Guía de implementación · Kikiri-9

## 1. Importa y corta las hojas

Importa los PNG de enemigos, UI y VFX como **Sprite (2D and UI)**. Para las hojas usa **Sprite Mode: Multiple**, **Mesh Type: Full Rect**, **Filter Mode: Point**, **Compression: None**, **Wrap Mode: Clamp**, **Max Size: 1024 o superior** y **Pixels Per Unit: 100** como referencia. Conserva el tamaño original del PNG; si aparece Non Power of 2, déjalo en None. Mantén desactivados los mipmaps para las hojas de sprites.

Abre **Sprite Editor → Slice → Grid by Cell Size**. Usa la celda indicada abajo, **Offset (0,0)**, **Padding (0,0)** y el pivote **Custom** que corresponda. Pulsa Slice y Apply. Si tu versión permite conservar celdas vacías, no es necesario habilitarlo: todas las celdas entregadas contienen arte.

**No uses el recorte Automatic por silueta.** La cuadrícula fija conserva el mismo lienzo y registro aun cuando un enemigo se agacha o se disuelve. Importa las poses fijas, los PNG de UI y los destellos de un frame como **Single**. En cada pose fija aplica también el pivote de personaje.

| Grupo | Celda | Pivote normalizado | Pivote desde abajo |
|---|---:|---|---|
| Mariachi y zombi | 96 × 96 px | (0.5, 0.125) | (48, 12) px |
| Xolo | 80 × 48 px | (0.5, 0.125) | (40, 6) px |
| Combo | 128 × 64 px | (0.5, 0.5) | (64, 32) px |
| Banners | 768 × 180 px | (0.5, 0.5) | (384, 90) px |

**datos/hojas.csv** tiene la medida total y la de cada celda, FPS y loop. **datos/frames.csv** tiene los rectángulos exactos: el primer frame empieza en X=0, Y=0; el siguiente en X=ancho de celda. Las Y se miden desde abajo, como en Sprite Editor.

Las hojas miran hacia la **izquierda**, para aparecer desde la derecha del pollito. Si un enemigo entra desde el otro lado, refleja el SpriteRenderer una vez; revisa aparte el collider y los desplazamientos de tu lógica.

## 2. Orden, tiempos y uso de animaciones

Arrastra los sprites en orden de izquierda a derecha a un clip de animación. Ajusta Samples a los FPS de referencia y activa Loop Time solo cuando la tabla dice Sí. Los nombres corresponden a los PNG de cada carpeta.

| Enemigo | Hoja | Frames | FPS | Loop | Cuándo va |
|---|---|---:|---:|---|---|
| Mariachi | float_idle | 6 | 8 | Sí | Flotación al entrar y entre ataques. |
| Mariachi | attack | 4 | 10 | No | Levanta la trompeta y toca. Sonido en el frame 2, contando desde 1. No crea proyectil. |
| Mariachi | defeat | 5 | 10 | No | Se disuelve en niebla. Retira al enemigo al terminar. |
| Zombi | crouch_idle | 4 | 6 | Sí | Espera en postura agachada. |
| Zombi | jump_prepare | 3 | 10 | No | Encoge las piernas y anticipa el salto. Sonido de aviso al entrar. |
| Zombi | jump_air | 2 | 10 | No | Dos poses aéreas. Mantén la última si la fase física dura más que el clip. |
| Zombi | land | 2 | 12 | No | Absorbe el golpe al tocar el suelo. Sonido al detectar contacto. |
| Zombi | defeat | 4 | 10 | No | Retrocede y se desploma. |
| Xolo | run | 6 | 14 | Sí | Carrera baja y rápida. |
| Xolo | defeat | 4 | 10 | No | Se encoge y cae de lado. |
| VFX | combo_destello | 5 | 20 | No | Destello sobre un cambio de multiplicador. |

**Mariachi:** usa la altura de vuelo para colocarlo por encima del corredor normal y dentro del arco de salto si ese es el reto. Ajusta con la cámara y el collider reales. El sombrero y la trompeta son silueta; no hace falta que toda su extensión cause daño.

**Zombi:** secuencia sugerida: crouch_idle → jump_prepare → jump_air → land → crouch_idle. Dispara el salto con tu lógica al salir de jump_prepare. El SpriteRenderer cambia de pose; el movimiento lo lleva el objeto físico. Dispara land solo al detectar contacto real, no al final de jump_air.

**Xolo:** usa un collider bajo en el cuerpo y prueba que se pueda evitar saltando o con ráfaga. Si después le agregas una embestida elevada para deslizarse por debajo, ajusta ese comportamiento por separado.

**Derrotas:** desactiva el daño de contacto cuando se confirma la derrota, reproduce el clip una sola vez y retira el objeto al finalizar. No lances otra animación de movimiento sobre la de derrota.

**Destello de golpe:** cada enemigo tiene hit_flash.png. El mariachi incluye dos celdas (blanco y recuperación); el zombi y el xolo tienen una blanca. Son máscaras de sus poses de referencia: úsalas como flash corto opcional. Si quieres conservar la pose exacta del momento de impacto, usa un material que conserve el alfa y vuelva blanco el sprite actual. En ambos casos vuelve al estado real después de un flash de unos 50–80 ms. El destello no concede invulnerabilidad ni causa daño por sí mismo.

## 3. Banner de oleada y combo

- **ui/oleada_inicio.png:** marco de 768 × 180. Añade un texto TMP centrado: OLEADA 1, OLEADA 2, etc.
- **ui/oleada_completa.png:** segundo marco con remates dorados. Superpón OLEADA COMPLETA como texto.
- **vfx/combo_destello.png:** 640 × 64, cinco frames de 128 × 64. Reproduce una vez y oculta al terminar. El valor ×2, ×6, ×10 o MAX va en TMP aparte, usando la fuente de tu juego.

Coloca el banner debajo del temporizador o bajo el marcador central si el temporizador está en la esquina. Ancla el conjunto al HUD y respeta el área segura. Como punto de partida, muestra el aviso 0.8 a 1.2 s con una entrada y salida corta de opacidad. El combo puede vivir cerca del marcador; evita que cada golpe reinicie de forma brusca el texto.

## 4. Audio: qué se reproduce y cuándo

Son los temas y los efectos de la prueba aprobada, exportados por separado en **WAV PCM de 48 kHz, 16 bits**. Los temas son loops de **15.00 s** (Dolores Hidalgo) y **12.47 s** (Guanajuato), sin el silencio separador de la prueba. La duración exacta está en datos/audio.csv.

| Archivo en audio/ | Evento |
|---|---|
| musica/dolores_hidalgo_loop.wav | Música del nivel de Dolores Hidalgo. |
| musica/guanajuato_loop.wav | Música del nivel de Guanajuato. |
| efectos/mariachi_trompeta.wav | Nota cosmética al tocar la trompeta. |
| efectos/zombi_aviso.wav | Comienza la preparación de salto. |
| efectos/zombi_aterrizaje.wav | Contacto real con el piso. |
| efectos/xolo_patitas.wav | Ráfaga corta de pasos; úsala por intervalos, no cada frame. |
| efectos/impacto.wav | Impacto confirmado contra un enemigo. |
| efectos/combo.wav | Sube el multiplicador. |
| efectos/oleada_inicio.wav | Arranca la oleada y aparece el aviso. |
| efectos/oleada_completa.wav | Oleada terminada, una vez por evento. |
| efectos/pan_dorado.wav | Se recoge el pan dorado. |
| efectos/rafaga.wav | Comienza la ráfaga real del pollito. |

Crea dos rutas de mezcla: **Música** y **Efectos**. Usa un AudioSource con Loop para el tema y reproducción de un disparo para los efectos. Como punto de partida, usa la música a volumen 0.35–0.50 y los avisos a 0.65–0.85, ajustando por oído con tu mezcla actual. Para efectos cortos conserva la frecuencia original y prueba Decompress On Load si quieres dispararlos sin decodificación en ese instante. El archivo de pasos ya tiene varias pisadas; no lo reinicies en cada paso.

Prueba dos vueltas seguidas de cada tema y verifica que trompeta, aviso del zombi y cierre de oleada se distingan encima de la música.

## 5. Comprobación al integrarlo

1. Confirma el número de sprites por hoja y su orientación.
2. Revisa que el pivote se mantenga estable cuando el zombi se agacha y el xolo toca el piso.
3. Verifica que las transiciones físicas del zombi funcionen con distintas duraciones de salto.
4. Revisa el xolo sobre fondos oscuros y el mariachi sobre cielos violetas al tamaño real de cámara.
5. Reproduce una oleada completa y revisa los avisos, el combo, el audio y la retirada de enemigos.

El paquete trae arte, audio y datos de importación. No incluye prefabs, Animator Controllers ni lógica de combate: conéctalo a los sistemas que ya tienes en la escena.
