# Programátorská dokumentace

Program je napsán v jazyce C# a využívá WinForms pro ovládání uživatelského rozhraní a vykreslování. Projekt je rozdělen do několika souborů a tříd za cílem přehlednější dekompozice a oddělení zodpovědností.

Složky:
- `ChuckieEgg/` - hlavní kód hry
- `Resources/Icons` - grafické ikony 
- `Resources/Audio` - zvukové efekty
- `Resources/Levels` - úrovně

Ve hře je použito písmo **ZX Spectrum-7** od autora **Sizenko Alexander (Style-7)**.

## Hlavní soubory a třídy

### `Form1`
- Hlavní okno programu (WinForms `Form`)
- Zajišťuje inicializaci hry, nastavení okna, zpracování kláves, přepínání mezi režimy (fullscreen, titulní obrazovka)
- Ovládá `Timer`, který spouští herní smyčku

Pro efektivní vykreslování do `GamePanel_Paint` je využito interpolace pomocí NearestNeighbor algoritmu. Pro zamezení problikávání při kreslení je použit double buffering. 

Form zachytává všechny stisky kláves pomocí `KeyDown`/`KeyUp` do `HashSet<Keys>` a posílá je dále třídě `Game` v metodě `Update()`, která řídí herní logiku. 


### `Game`
- Obsahuje hlavní logiku hry a aktuální stav (`GameState`)
- Zajišťuje přechody mezi stavy (např. Playing, GameOver, LevelComplete)
- Spravuje skóre, bonusy, čas a aktuální úroveň
- Volá aktualizaci aktuálního levelu a předává informace do uživatelského rozhraní (`GameUIManager`)

Ve třídě Game jsou ukládány všechny informace o aktuálním stavu hry a volá příslušné metody (`HandlePlaying()`, `HandleGameOver()`, apod.). 

Dalé se stará o řízení hry na základě provedených ticků časovače. Ticks jsou dále využívány v objektech s pohybem, které se pohybují s danou frekvencí uplynulých ticků. Ústřední metoda je metoda `Update()` volaná každý tick z Form, která volá odpovídající logiku a aktualizuje UI přes `GameUIManager`. 

### `Level`
- Reprezentuje jednu herní mapu načtenou ze souboru
- Uchovává mapu ve formě 2D pole znaků (_grid) a seznamy všech objektů (hráče, sběratelné předměty, pohyblivé objekty)
- Řídí pohyb objektů (`MoveAllObjects()`), kolize a vykreslování (`Draw()`)

Level načítá `.txt` soubory z adresáře `Resources/Levels/`, převádí je do 2D gridu a vytváří objekty podle znakových symbolů. Level také obsahuje public metody, které předávají informace o jednotlivých políčkách na mapě a jsou využívány všemi pohyblivými objekty.

### `Player`
- Reprezentuje hráče
- Obsahuje logiku pro pohyb, skákání, lezení, padání, kolize, sbírání objektů a ztrátu životů
- Používá stavový automat (`MovementState`) pro řízení aktuálního režimu hráče (chůze, pád, skok)
- Implementuje animace pomocí polí bitmap (idleAnim, walkAnim, climbAnim) podle stavu
- Obsahuje funkci `Draw()` pro vykreslení své aktuální animace

Třída dědí z `MovableObject`, která vyžaduje implementaci metody `MakeMove()` a `Draw()`.
Logika pohybu se provádí na základě stisknuté klávesy a snímaných hodnot pro aktuální pozici hráče pomocí metody `SampleSensors()`, která volá veřejné metody z třídy `Level`.

### `Enemy`
- Reprezentuje nepřátele (malé kachny)
- Obsahuje logiku pro pohyb, lezení
- Obsahuje metodu `Draw` pro vykreslení své aktuální animace

Dědí z `MovableObject` a implementuje logiku pohybu pomocí metody `MakeMove()`. Kachny chodí po plošinách, dokážou šplhat po žebřících a mění směr při nárazu do překážky.

### `Duck`
- Reprezentace velké kachny
- Implementuje logiku sledování pozice hráče
- Obsahuje metodu `Draw` pro vykreslení své aktuální animace

Dědí z `MovableObject` a má vlastní implementaci `MakeMove()`, kde sleduje pozici hráče. Létá po mapě bez ohledu na překážky.

### `GameObjects`
- Obsahuje implementace základních typů objektů ve hře
- Definuje abstraktní třídy `GameObject`, `MovableObject` a `CollectableObject`
- Zajišťuje společné vlastnosti jako pozice, metoda `Draw`, `MakeMove`, `Collect`

### `GameUIManager`
- Spravuje herní uživatelské rozhraní
- Ovládá přechodové obrazovky, informace o skóre a čase
- Umožňuje zobrazení fullscreen zpráv a přechodvých animací
- Dynamicky mění velikost a vzhled UI prvků při změně velikosti okna (`ResizeUILayout`)

Využívá `TableLayoutPanel` pro zobrazení skóre, času a levelu v horní části Form. Využívá písmo `ZX Spectrum-7` načtené z `.ttf` souboru.

### `SpriteManager`
- Načítá a uchovává bitmapy ze složky `Resources/Icons`
- Poskytuje sprite sady pro různé objekty (hráč, enemy, kachna, dlaždice)

### `AudioManager`
- Načítá a umožňuje přehrávání zvukových efektů

### `GameConstants`
- Obsahuje definice globálních konstant a výčtových typů
- Funguje jako konfigurace hry

Rozdělen do několika statických tříd: `GameConstants`, `UIConstants`, `SpriteConstants`, `TileSprite`, `PhysicsSettings`, `PlayerSettings`.
Dále definuje výčtové typy `ArrowDown`, `State`, `EnemyState` pro řízení chování herních objektů.


