# Guia de contribucion

## Branches

- `main`: siempre compilable y jugable. No se commitea directo.
- `feature/<area>-<desc>`: trabajo activo. Ejemplos:
  - `feature/shared-core`
  - `feature/player-controller`
  - `feature/level01-dolores`
  - `feature/ui-main-menu`
- `fix/<desc>`: bugfix especifico.

## Flujo

1. `git pull origin main`
2. `git checkout -b feature/<area>-<desc>`
3. Trabajar, commitear frecuente.
4. Push: `git push -u origin feature/<...>`
5. Abrir PR en GitHub asignando al companero como reviewer.
6. Companero revisa, aprueba, mergea con "Squash and merge".
7. Borrar branch remota.

## Commits

Formato: `<tipo>(<scope>): <descripcion corta>`

Tipos: `feat`, `fix`, `chore`, `docs`, `refactor`, `style`, `test`.

Scopes sugeridos: `player`, `enemy`, `ui`, `hud`, `audio`, `save`, `level01`..`level04`, `shared`, `build`, `repo`.

Ejemplos:
- `feat(player): add slide collider reduction`
- `fix(ui): main menu buttons not responding on gamepad`
- `chore(repo): add editorconfig`

## Reglas Unity

- Mover/renombrar archivos SOLO desde Unity (nunca desde Explorer o git mv).
- Los `.meta` siempre acompanan a su archivo en el commit.
- Nunca editar la misma escena o prefab al mismo tiempo. Avisar en el chat del equipo.
- Antes de pull/merge: guardar escena abierta (Ctrl+S) y cerrar Unity.
- Despues de pull con cambios en Packages: dejar que Unity reimporte.

## Avances y tags

Al cerrar cada avance de la rubrica, taguear en main:

```
git tag -a avance-01 -m "Avance 1: prototipo base"
git push origin avance-01
```

Tags previstos: `avance-01`..`avance-05`, `entrega-final`.

## Smart Merge para escenas y prefabs

Configurar una vez por clon (ajustar ruta a tu instalacion de Unity):

```
git config merge.unityyamlmerge.name "Unity SmartMerge"
git config merge.unityyamlmerge.driver "'C:/Program Files/Unity/Hub/Editor/6000.3.26f1/Editor/Data/Tools/UnityYAMLMerge.exe' merge -p %O %B %A %A"
git config merge.unityyamlmerge.recursive binary
```

## Config local recomendada

Una vez por maquina:

```
git config core.autocrlf true
git config core.longpaths true
git config pull.rebase false
git lfs install
```
