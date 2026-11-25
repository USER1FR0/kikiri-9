# Mapa de niveles - Assets migrados, implementacion pendiente

Los assets del paquete `kikiri9_powerups_mapa` quedaron en:

```
Assets/_Project/Art/
  Mapa/
    mapa_dolores_guanajuato.png            Fondo 1774x887, prop 2:1
    nodos/
      nodo_bloqueado.png                   560x704, candado
      nodo_completado.png                  560x704, marca dorada
      nodo_disponible.png                  560x704, centro libre
      nodo_seleccionado.png                560x704, borde luminoso
    atlas/
      botones_rafaga.png                   1344x448, 3x1 (448x448)
      nodos_mapa.png                       2240x704, 4x1 (560x704)
    datos/
      medidas.csv                          tamanos, pivotes y modos
      niveles_ejemplo.csv                  4 niveles ejemplo (ID, estado, posicion)
      recortes_atlas.csv                   coords de atlas
```

Tambien se refrescaron los PNG de `PanDorado/` y `Rafaga/` con las ultimas versiones del paquete.

## Pendientes para la etapa del mapa

1. Importar PNG de `Mapa/` como Sprite Single, Point, PPU 100, Full Rect, no mipmaps.
2. Crear escena `Assets/_Project/Scenes/LevelSelect.unity` con Canvas Screen Space Overlay.
3. Dentro del Canvas:
   - `MapaContenido` RectTransform con `Aspect Ratio Fitter` (mode `Fit In Parent`, ratio 2).
   - `Fondo` Image estirado al rectangulo con `mapa_dolores_guanajuato.png`.
   - Un Button por nodo, hijo de `MapaContenido`, tamano 112x140.8 UI, pivot (0.5, 0.45454545).
   - Anchor Min = Anchor Max = (x, y) de `niveles_ejemplo.csv` (origen inferior izq).
4. Script `LevelMapController`:
   - Lee `datos/niveles_ejemplo.csv` (o un ScriptableObject equivalente) con ID, scene, state.
   - Cambia el sprite de cada nodo segun state (disponible, completado, seleccionado, bloqueado).
   - OnClick llama `GameSession.LoadLevel(id)` o `SceneManager.LoadScene(sceneName)`.
   - Guarda progreso en `GameSession` (PlayerPrefs o archivo).
5. Agregar entrada `Mapa` en el `MainMenu` que carga `LevelSelect.unity`.
6. Modificar `LevelCompletePanel.ReturnToMap` para ir a `LevelSelect` en vez del menu principal.

## Notas de layout del mapa

- Pivote de nodo y medallon: `0.5, 0.45454545` (centro del circulo).
- El centro del medallon en el PNG esta a `(280, 384)` desde arriba en un lienzo 560x704.
- El numero de nivel va con anchor y pivot `0.5, 0.45454545` para que caiga en el centro del medallon.
- El fondo NO debe recortarse por separado de los botones: todo escala en conjunto.
- Mantener proporcion 2:1 en mobile/desktop; para pantallas angostas usar ScrollRect.

## Atlas opcionales

No usar los atlas Y los PNG separados al mismo tiempo. Si se usan los atlas:
- `botones_rafaga.png`: slice Grid by Cell Size 448x448, pivot 0.5,0.5, orden: listo, pulsado, recarga_muestra (esta ultima es solo referencia visual, no se usa en runtime; la recarga real se hace con la mascara radial).
- `nodos_mapa.png`: slice Grid by Cell Size 560x704, pivot 0.5,0.45454545, orden: completado, disponible, seleccionado, bloqueado.
