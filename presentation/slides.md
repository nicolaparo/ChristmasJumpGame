# BlazorGameEngine — Presentazione Tecnica

> Nota: ogni sezione corrisponde a una slide. Copia i titoli come titoli slide e i bullet come contenuto.

---

# Slide 1 — BlazorGameEngine - Architettura Interna
- Engine 2D per Blazor + HTML5 Canvas
- API ispirata a GameMaker, type-safe in C#
- Progettato per performance: batching, DI, async

**Note del relatore:**
- Breve introduzione: scopo del progetto e pubblico (sviluppatori C#/Blazor).
- Enfasi su design type-safe e integrazione Blazor/Canvas.

---

# Slide 2 — Agenda
- Architettura generale
- Game loop e interop JavaScript
- Gestione input, rendering e fisica
- Dependency Injection per asset e GameObject
- Performance, limitazioni e roadmap

**Note del relatore:**
- Indicare durata prevista e spazio per domande alla fine.

---

# Slide 3 — Architettura High-level
- Blazor Component (`GameView`) ↔ Game (C#) ↔ Canvas2D (JS)
- Game loop gestito da JavaScript con `requestAnimationFrame`
- Batching Canvas2D per ridurre l'overhead di interop

**Note del relatore:**
- Mostrare diagramma a blocchi: UI/Canvas/JS loop/C# game logic.
- Spiegare responsabilità di ogni layer.

---

# Slide 4 — Game Loop & JS Interop
- Loop: JS invoca handler C# per frame + `requestAnimationFrame`
- Limitazione FPS con `setTimeout` per target rate
- Sequenza tipica: render → update → snapshot stato input

**Snippet JS (semplificato):**
```javascript
export async function executeFrame(dotnetReference, callbackName) {
  await dotnetReference.invokeMethodAsync(callbackName);
  setTimeout(() => {
    window.requestAnimationFrame(() => executeFrame(dotnetReference, callbackName));
  }, 1000 / window.fps);
}
```

**Note del relatore:**
- Spiegare perché il loop è in JS: timing più preciso e sincronizzazione con il repaint del browser.

---

# Slide 5 — Esempio: ExecuteFrameAsync (sequenza)
- Aggiorna posizione mouse e snapshot degli eventi
- Fase di rendering: `OnDrawAsync` del gioco e dei `GameObject`
- Fase di update: `OnStepAsync` e `ExecuteStepAsync` per gli oggetti
- Salva lo stato degli input e aggiorna `controllerHub`

**Snippet C# (pseudocodice):**
```csharp
var instances = this.instances.ToArray(); // snapshot
await OnDrawAsync(canvas);
foreach(var obj in instances) await obj.OnDrawAsync(canvas);
await OnStepAsync();
foreach(var obj in instances) await obj.ExecuteStepAsync();
```

**Note del relatore:**
- Evidenziare l'ordine (render prima di update) e l'uso di `ToArray()` per evitare problemi di concorrenza.

---

# Slide 6 — Gestione Input: Pattern "Previous State"
- Doppio buffer: `keyboardPressed` vs `previousKeyboardPressed`
- Operazioni comuni: `Check` (hold), `CheckPressed` (edge), `CheckReleased`
- Semplice, stabile e preciso frame-per-frame

**Snippet (idea):**
```csharp
previousKeyboardPressed[key] = keyboardPressed[key];
// CheckPressed: !previous && current
```

**Note del relatore:**
- Mostrare tabella esemplificativa del comportamento in più frame (press, hold, release).

---

# Slide 7 — GameObject & Dependency Injection
- Istanziazione con `ActivatorUtilities.CreateInstance<T>(serviceProvider, this)`
- Uso di reflection per impostare `init`-only `OriginalX/OriginalY`
- `OnCreate()` callback per inizializzazione post-costruzione

**Note del relatore:**
- Vantaggi: testabilità, iniezione di asset e servizi condivisi, separazione dei compiti.

---

# Slide 8 — Rendering: Sprite, Transform, Batching
- Distinzione `ImageAsset` vs `SpriteAsset` (sprite sheet, frame index)
- `DrawSprite` calcola `imageX/imageY` dal `imageIndex`
- Trasformazioni: translate → scale → rotate; reset transform dopo il draw
- `BeginBatch` / `EndBatch` per minimizzare round-trips C# ↔ JS

**Snippet calcolo frame (semplificato):**
```text
imageX = (index % imagesPerRow) * (width + xGap) + xOffset
imageY = (index / imagesPerRow) * (height + yGap) + yOffset
```

**Note del relatore:**
- Spiegare come batching riduca il numero di chiamate JS per frame e migliori FPS.

---

# Slide 9 — Fisica e Collisioni
- Collision detection con AABB (`IsCollidingWith`)
- `MoveContactSolid`: muovi fino al contatto (incremento pixel/step)
- `MoveOutsideSolid`: espelli un oggetto sovrapposto ai solidi

**Snippet AABB (concetto):**
```text
collision = !(right1 <= left2 || left1 >= right2 || bottom1 <= top2 || top1 >= bottom2)
```

**Note del relatore:**
- Discutere trade-off: semplicità e velocità vs precisione (no pixel-perfect). Possibili ottimizzazioni: spatial hashing.

---

# Slide 10 — Asset Management & Registrazione DI
- `AddSprite<TSprite>()` registra come `TSprite`, `SpriteAsset`, `IGameAsset`
- Auto-discovery con reflection sull'assembly per registrare automaticamente sprite
- Consente iniezione strong-typed e enumerazione assets in `GameView`

**Note del relatore:**
- Mostrare come ciò semplifica il wiring e permette iniezione diretta di asset nei GameObject.

---

# Slide 11 — Tipo `Angle` e Safety
- Struct `Angle` con factory `FromDegrees` / `FromRadians`
- Metodi `Sin()`/`Cos()`, operatori e `Atan2`
- Evita l'ambiguità gradi vs radianti nel codice

**Snippet (concetto):**
```csharp
var a = Angle.FromDegrees(90);
float vx = a.Cos() * speed;
```

**Note del relatore:**
- Enfatizzare la miglior leggibilità e la riduzione di bug legati alle unità.

---

# Slide 12 — Performance & Ottimizzazioni
- Ridurre overhead interop con batching (molte draw calls → 1 round-trip)
- Snapshot `ToArray()` per evitare lock durante iterazioni
- Possibili evoluzioni: WebGL, object pooling, spatial hashing

**Note del relatore:**
- Dare stime qualitative sull'impatto di batching (es. 1000 draw calls → 1 chiamata).

---

# Slide 13 — Limitazioni e Roadmap
- Single-threaded (Blazor WASM)
- Architettura OOP (non ECS)
- Collisioni solo AABB (no poligoni/pixel-perfect)
- Roadmap: WebGL, pooling, streaming asset, spatial hashing

**Note del relatore:**
- Discutere trade-offs progettuali e possibili miglioramenti futuri.

---

# Slide 14 — Demo / Come eseguire il gioco
- Aprire soluzione `ChristmasJumpGame.slnx` in Visual Studio
- Eseguire `ChristmasJumpGame` (profilo Development)
- Generare slide: installare `python-pptx` e lanciare `generate_presentation.py` (opzionale)

**Comandi PowerShell rapidi:**
```powershell
python -m pip install --user python-pptx
python .\presentation\generate_presentation.py
```

**Note del relatore:**
- Mostrare come avviare e dove trovare asset/levels per demo.

---

# Slide 15 — Domande e Risorse
- Repository locale e file di riferimento: `BlazorGameEngine-Architecture.md`
- Contatti per approfondimenti o PR

**Note del relatore:**
- Aprire Q&A; invitare discussione su performance e design choices.

---

## Suggerimenti per l'impaginazione in PowerPoint
- Usa un layout chiaro: titolo in alto, massimo 4–6 bullet per slide.
- Inserire diagrammi (diagramma a blocchi per architettura, sequenza per game loop).
- Colori: palette scura per demo su schermo, testo chiaro; oppure palette chiara per stampa.
- Includere 1–2 slide con snippet di codice (formattato monospace) e 1 slide con diagramma di collisione.

---

Se vuoi, posso:
- Generare ora il file `.pptx` usando lo script (richiede Python locale),
- Oppure arricchire le slide con immagini/diagrammi (posso generare PNG/SVG),
- Tradurre la presentazione in inglese.
