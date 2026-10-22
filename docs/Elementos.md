# Catálogo de items, objetos y enemigos de Kikiri-9

## Personajes

| Item | Qué es | Funcionalidad |
|---|---|---|
| Kikiri-9 | Pollo cyborg protagonista | Auto-corre a la derecha. 3 corazones. Salta, se desliza, lanza pan. 1s de invulnerabilidad tras golpe. |
| Zombi | Enemigo de Dolores Hidalgo (niveles 1-2) | Camina hacia la izquierda. 1 golpe de pan lo derrota. Contacto con pollo quita 1 corazón. |
| Momia | Enemigo de Guanajuato (niveles 3-4) | Camina hacia la izquierda. Normal: 1 golpe. Resistente (con señal visual): 2 golpes. Contacto quita 1 corazón. |
| Coronela | Jefe final (nivel 4) | 12 HP. Lanza proyectil espectral cada 2.5s con aviso de 0.7s. Al derrotarla termina el juego. |

## Items coleccionables y proyectiles

| Item | Qué es | Funcionalidad |
|---|---|---|
| objetos_01 Pan recogible | Pan de muerto en el suelo | Trigger: suma 3 panes a la mochila (máx 12). Reproduce SFX + brillo verde. No afecta victoria. |
| objetos_02 Pan proyectil | Pan lanzado por el pollo | Cuesta 1 pan de la mochila. Vuela con caída suave. Daño 1 al enemigo. Al impactar: splash naranja + parpadeo + desaparece. |
| objetos_03 Botella verde | Moneda del juego | Al recoger: +1 moneda persistente, +25 pts. Se conserva al perder. No da salud ni munición. |
| objetos_04 Botella dorada | Premio por completar nivel | Entregada solo en primera victoria por nivel. Nivel 1: 1 botella. Niveles 2-3: 2 botellas. Nivel 4: 3 botellas. No se gastan. |

## Efectos visuales (sin daño ni colisión)

| Item | Qué es | Funcionalidad |
|---|---|---|
| efectos_a_01 Proyectil espectral | Ataque morado de la Coronela | Vuela hacia el pollo. Quita 1 corazón al impactar. Solo nivel 4. |
| efectos_a_02 Impacto naranja | Splash cuando el pan aterriza o golpea | Decorativo. Aparece 0.4s y desaparece. Sin daño. |
| efectos_a_03 Brillo verde | Confirmación de recogida exitosa | Se dispara al recoger pan o botella. Decorativo breve. |
| efectos_a_04 Polvo blanco | Pisadas, aterrizaje, deslizamiento | Decorativo. Reafirma el movimiento del pollo. |
| efectos_b_01 Salida de pan | Chispa de lanzamiento | Aparece en la mochila al lanzar. Decorativo breve. |
| efectos_b_02 Migas | Residuos tras impactar enemigo | Decorativo. Sin daño ni puntos. |
| efectos_b_03 Dispersión morada | Desaparición del proyectil espectral | Decorativo cuando el pan destruye el proyectil o la Coronela es derrotada. |
| efectos_b_04 Destello dorado | Celebración al recoger botella dorada | Decorativo. Confirma el premio del nivel. |

## Entorno y obstáculos (ground sheet y decoración)

| Item | Qué es | Funcionalidad |
|---|---|---|
| suelo (fila 1 y 2) | Tiles del adoquín/suelo | Superficie de carrera. Collider sólido donde camina el pollo. |
| plataformas | Variante de suelo elevada | Base elevada. Solo si el nivel la usa. Collider sólido. |
| Tumba | Obstáculo bajo | Se salta. Choque = 1 corazón. |
| Carreta | Obstáculo bajo | Se salta. Choque = 1 corazón. |
| Arco bajo / papel bajo | Obstáculo con hueco inferior | Se pasa deslizando. Solo bloquea altura normal, no slide. |
| Altar | Decoración (ofrenda) | Visual únicamente. No es checkpoint ni cura. |
| Florero | Decoración | Solo visual. |
| Velas | Decoración ambiental | Solo visual. |
| Barril | Decoración | Solo visual. No es obstáculo. |
| Farol | Decoración callejera | Solo visual. |
| Banco | Decoración | Solo visual. |
| Flores | Cempasúchil decorativo | Solo visual. |
| Calavera | Decoración | Solo visual. |

## Fondos y narrativa

| Item | Qué es | Funcionalidad |
|---|---|---|
| cielo.png | Capa de cielo con luna y tormenta | Fondo lejano con parallax casi nulo. Siempre visible. |
| silueta_guanajuato.png | Silueta de cerros, casas y basílica | Capa intermedia con parallax medio. Da profundidad. |
| guanajuato.png | Callejón con balcones y Pípila | Fondo cercano (niveles 3-4). |
| dolores_hidalgo.png | Parroquia, campana, cerámica | Fondo cercano (niveles 1-2). |
| menu_guanajuato.png | Composición de portada | Fondo del menú principal. |
| Story 01-08 | Ilustraciones narrativas | Pantallas completas con texto encima. Transición entre niveles. No jugables. |

## Mecánicas globales

| Sistema | Qué hace |
|---|---|
| Salud | 3 corazones por nivel. 1 impacto = -1 corazón. 1s de invulnerabilidad tras golpe. 0 corazones = Game Over. |
| Mochila de panes | 8 panes iniciales, capacidad base 12. Cada disparo consume 1. Pan recogible suma 3. |
| Mochila ampliable | Mejora de tienda: 12 → 15 panes por 25 monedas. Opcional. |
| Munición ampliable | Mejora de tienda: 8 → 10 panes iniciales por 40 monedas. Opcional. |
| Monedas | Botellas verdes. Persisten entre partidas. Se gastan en tienda. |
| Puntaje | Zombi: 100. Momia normal: 100. Momia resistente: 150. Botella verde: 25. Coronela: 1000. Solo cuenta una vez por objeto. |
| Victoria | Niveles 1-3: llegar a la distancia objetivo. Nivel 4: derrotar a la Coronela. |
| Derrota | Perder 3 corazones. Muestra Game Over con reintentar o volver al menú. |