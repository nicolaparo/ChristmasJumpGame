# BlazorGameEngine - Architettura Interna

## Introduzione

**BlazorGameEngine** è un game engine 2D sviluppato completamente in C# per Blazor, che sfrutta HTML5 Canvas per il rendering. L'engine combina il pattern di dependency injection di ASP.NET Core con un'architettura orientata agli oggetti ispirata ai classici game engine 2D come GameMaker.

## Architettura Generale

### Stack Tecnologico

```
┌─────────────────────────────────────┐
│     Blazor Component (GameView)     │
├─────────────────────────────────────┤
│     Game Loop (JavaScript)          │
├─────────────────────────────────────┤
│     Canvas2D API (Blazor.Extensions)│
├─────────────────────────────────────┤
│     HTML5 Canvas                     │
└─────────────────────────────────────┘
```

L'engine si basa su tre componenti principali:
- **Game**: la classe base per il gioco, gestisce il game loop e lo stato globale
- **GameObject**: entità del gioco che possono essere istanziate, aggiornate e renderizzate
- **GameView**: il componente Razor che collega il game loop al rendering Canvas

---

## 1. Il Game Loop - Cuore dell'Engine

### JavaScript Interop: Il Timing Preciso

Il game loop è gestato da JavaScript per garantire un timing preciso con `requestAnimationFrame`:

```javascript
// gameinterop.js
export async function executeFrame(dotnetReference, callbackFunctionName) {
    await dotnetReference.invokeMethodAsync(callbackFunctionName);
    setTimeout(() => {
        window.requestAnimationFrame(() => executeFrame(dotnetReference, callbackFunctionName));
    }, 1000 / (window.fps));
};

export function setFPS(fps) {
    window.fps = fps;
}
```

**Perché questa architettura?**
- `requestAnimationFrame` sincronizza con il refresh del browser
- Il `setTimeout` permette di limitare gli FPS (es. 60 FPS)
- La chiamata asincrona a C# (`invokeMethodAsync`) mantiene il loop reattivo

### Il Frame di Gioco in C#

Ogni frame esegue questa sequenza nel metodo `ExecuteFrameAsync`:

```csharp
internal async Task ExecuteFrameAsync(RenderingContext context)
{
    // 1. Aggiorna la posizione del mouse
    (PreviousMouseX, PreviousMouseY) = (MouseX, MouseY);
    (MouseX, MouseY) = ((float)(lastMouseMoveEventArgs?.OffsetX ?? 0), 
                        (float)(lastMouseMoveEventArgs?.OffsetY ?? 0));

    // 2. Snapshot delle istanze (per evitare problemi di concorrenza)
    var instances = this.instances.ToArray();

    // 3. FASE DI RENDERING
    if (context is Canvas2DContext canvas2DContext)
    {
        await OnDrawAsync(canvas2DContext);           // Draw del Game
        foreach (var obj in instances)
            await obj.OnDrawAsync(canvas2DContext);   // Draw di ogni GameObject
    }

    // 4. FASE DI UPDATE
    await OnStepAsync();                               // Step del Game
    foreach (var obj in instances)
        await obj.ExecuteStepAsync();                  // Step di ogni GameObject

    // 5. Salva lo stato degli input per il prossimo frame
    foreach (var button in mousePressed.Keys)
        previousMousePessed[button] = mousePressed[button];
    foreach (var key in keyboardPressed.Keys)
        previousKeyboardPressed[key] = keyboardPressed[key];

    // 6. Aggiorna controller (se presente)
    controllerHub?.Update();
}
```

**Dettagli chiave:**
- Lo snapshot `ToArray()` previene modifiche concorrenti durante l'iterazione
- Il pattern "previous state" permette di rilevare `Pressed` e `Released`
- Il rendering avviene **prima** dell'update logico

---

## 2. Gestione degli Input - Pattern "Previous State"

### Il Problema: Distinguere "Hold" da "Pressed"

Un gioco deve distinguere tra:
- **Check**: "Il tasto è premuto?" (tenuto premuto)
- **CheckPressed**: "Il tasto è stato appena premuto?" (solo il primo frame)
- **CheckReleased**: "Il tasto è stato appena rilasciato?"

### La Soluzione: Double Buffering degli Stati

```csharp
// Stato corrente e precedente
private readonly Dictionary<string, bool> keyboardPressed = new();
private readonly Dictionary<string, bool> previousKeyboardPressed = new();

// Aggiornamento ad ogni frame
foreach (var key in keyboardPressed.Keys)
    previousKeyboardPressed[key] = keyboardPressed[key];

// Logica di rilevamento
public sealed override bool KeyboardCheckPressed(string key)
{
    if (!keyboardPressed.TryGetValue(key, out bool pressed))
        return false;
    if (!previousKeyboardPressed.TryGetValue(key, out bool previousPressed))
        return false;
    
    // Premuto adesso ma NON nel frame precedente
    return !previousPressed && pressed;
}
```

**Esempio pratico:**

| Frame | Key Down Event | `keyboardPressed["Space"]` | `previousKeyboardPressed["Space"]` | `CheckPressed` | `Check` |
|-------|----------------|----------------------------|------------------------------------|----------------|---------|
| 1     | ❌             | false                      | false                              | false          | false   |
| 2     | ✅ (evento)    | **true**                   | false                              | **TRUE**       | true    |
| 3     | (tenuto)       | true                       | **true**                           | false          | true    |
| 4     | (tenuto)       | true                       | true                               | false          | true    |
| 5     | ✅ (up event)  | **false**                  | true                               | false (released)| false   |

---

## 3. Sistema di GameObjects - Dependency Injection Avanzata

### Il Pattern di Istanziazione

L'engine usa `ActivatorUtilities` per creare GameObject con dependency injection:

```csharp
public sealed override T InstanceCreate<T>(float x, float y)
{
    // Crea l'istanza usando il service provider (DI automatica)
    var instance = ActivatorUtilities.CreateInstance<T>(serviceProvider, this);
    
    // Imposta le proprietà via reflection (per init-only properties)
    typeof(T).GetProperty(nameof(GameObject.OriginalX))!.SetValue(instance, x);
    typeof(T).GetProperty(nameof(GameObject.OriginalY))!.SetValue(instance, y);
    
    instance.X = x;
    instance.Y = y;
    
    // Aggiunge alla lista gestita
    instances.Add(instance);
    
    // Callback di inizializzazione
    instance.OnCreate();
    
    return instance;
}
```

**Perché usare Reflection per `OriginalX` e `OriginalY`?**

Queste proprietà sono `init`, quindi readonly dopo la costruzione:

```csharp
public float OriginalX { get; init; }
public float OriginalY { get; init; }
```

La reflection bypassa questa restrizione, permettendo di impostare i valori dopo la creazione ma mantenendoli immutabili per il codice utente.

### Esempio Pratico di GameObject

```csharp
public class Player : GameObject
{
    private readonly PlayerSpriteAsset sprite;
    
    // Il costrutor riceve dipendenze via DI
    public Player(Game game, PlayerSpriteAsset sprite) : base(game)
    {
        this.sprite = sprite;
        Sprite = sprite;
        BoundingBox = new BoundingBox(0, 0, 16, 32);
    }

    public override void OnCreate()
    {
        // Chiamato dopo l'istanziazione
        ImageSpeed = 0.2f;
    }

    public override async ValueTask OnStepAsync()
    {
        // Logica di movimento
        if (KeyboardCheck(KeyboardKeys.ArrowRight))
            HSpeed = 2;
        else if (KeyboardCheck(KeyboardKeys.ArrowLeft))
            HSpeed = -2;
        else
            HSpeed = 0;

        VAcceleration = 0.5f; // Gravità
    }
}
```

---

## 4. Sistema di Rendering - Canvas2D Extensions

### Asset System: Sprites e Immagini

L'engine distingue tra `ImageAsset` (immagini singole) e `SpriteAsset` (sprite sheets):

```csharp
public abstract record SpriteAsset : ImageAsset
{
    public int ImageCount { get; init; }      // Numero totale di frame
    public int ImagesPerRow { get; init; }    // Frame per riga
    public int OriginX { get; init; }         // Punto di rotazione/origine
    public int OriginY { get; init; }
    public int XGap { get; init; }            // Spaziatura tra frame
    public int YGap { get; init; }
    public int XOffset { get; init; }         // Offset iniziale
    public int YOffset { get; init; }
}
```

### Estrazione Frame da Sprite Sheet

Il metodo `DrawSpriteAsync` calcola le coordinate del frame:

```csharp
public static async ValueTask DrawSpriteAsync(
    this Canvas2DContext context, 
    SpriteAsset sprite, 
    int imageIndex, 
    double x, double y)
{
    // Calcola posizione del frame nello sprite sheet
    var imageX = (imageIndex % sprite.ImagesPerRow) * (sprite.Width + sprite.XGap) 
                 + sprite.XOffset;
    var imageY = (imageIndex / sprite.ImagesPerRow) * (sprite.Height + sprite.YGap) 
                 + sprite.YOffset;

    // Disegna solo la porzione del frame
    await context.DrawImageAsync(
        sprite.ElementReference,
        imageX, imageY,                           // Sorgente: dove prelevare
        sprite.Width, sprite.Height,              // Sorgente: dimensioni
        x - sprite.OriginX, y - sprite.OriginY,   // Destinazione: posizione
        sprite.Width, sprite.Height);              // Destinazione: dimensioni
}
```

**Esempio visivo:**

```
Sprite Sheet (64x64 totali, frame 16x16, 4 per riga):
┌────────┬────────┬────────┬────────┐
│ Frame0 │ Frame1 │ Frame2 │ Frame3 │  ← imageY = 0
├────────┼────────┼────────┼────────┤
│ Frame4 │ Frame5 │ Frame6 │ Frame7 │  ← imageY = 16
└────────┴────────┴────────┴────────┘
  ↑
  imageX varia: 0, 16, 32, 48...

Per Frame 5:
  imageIndex = 5
  imageX = (5 % 4) * 16 = 1 * 16 = 16
  imageY = (5 / 4) * 16 = 1 * 16 = 16
```

### Trasformazioni: Rotazione e Scala

```csharp
public static async ValueTask DrawSpriteAsync(
    this Canvas2DContext context, 
    SpriteAsset sprite, 
    int imageIndex, 
    double x, double y, 
    double scaleX = 1, 
    double scaleY = 1, 
    Angle rotation = default)
{
    // Ottimizzazione: skip se non serve trasformazione
    if (scaleX is 1 && scaleY is 1 && rotation == default)
    {
        await context.DrawSpriteAsync(sprite, imageIndex, x, y);
        return;
    }

    // 1. Reset trasformazione
    await context.SetTransformAsync(1, 0, 0, 1, 0, 0);
    
    // 2. Trasla all'origine desiderata
    await context.TranslateAsync(x, y);

    // 3. Scala (se necessario)
    if (scaleX is not 1 || scaleY is not 1)
        await context.ScaleAsync(scaleX, scaleY);

    // 4. Ruota (se necessario)
    if (rotation != default)
        await context.RotateAsync((float)rotation.ToRadians());

    // 5. Disegna con origine (0,0) - già trasformato
    await context.DrawSpriteAsync(sprite, imageIndex, 0, 0);
    
    // 6. Reset per non influenzare altri draw
    await context.SetTransformAsync(1, 0, 0, 1, 0, 0);
}
```

**Ordine delle trasformazioni (importante!):**
1. Translate → Scala → Rotate (ordine usato dall'engine)
2. Le trasformazioni si applicano in ordine INVERSO alla sequenza di chiamata

---

## 5. Sistema di Fisica e Collisioni

### Bounding Box e Collision Detection

```csharp
public bool IsCollidingWith(GameObject other)
{
    if (BoundingBox is null || other.BoundingBox is null)
        return false;

    // AABB (Axis-Aligned Bounding Box) collision
    return !(X + BoundingBox.X + BoundingBox.Width <= other.X + other.BoundingBox.X ||
             X + BoundingBox.X >= other.X + other.BoundingBox.X + other.BoundingBox.Width ||
             Y + BoundingBox.Y + BoundingBox.Height <= other.Y + other.BoundingBox.Y ||
             Y + BoundingBox.Y >= other.Y + other.BoundingBox.Y + other.BoundingBox.Height);
}
```

**Logica:** Due box collidono se **nessuna** delle seguenti condizioni è vera:
- Box1 è completamente a sinistra di Box2
- Box1 è completamente a destra di Box2
- Box1 è completamente sopra Box2
- Box1 è completamente sotto Box2

### Movement System: MoveContactSolid

Algoritmo per muovere un oggetto fermandosi al contatto con un solido:

```csharp
public bool MoveContactSolid(Angle direction, float maxDistance, float resolution = 1f)
{
    // Calcola incrementi per pixel
    float incrementX = direction.Cos();
    float incrementY = -direction.Sin();

    var (newX, newY) = (X, Y);
    var distance = 0f;
    
    // Avanza pixel per pixel (o con step personalizzato)
    while (distance <= maxDistance)
    {
        newX = X + incrementX * distance;
        newY = Y + incrementY * distance;

        // Fermati se la posizione non è libera
        if (!IsPointFree(newX, newY))
            break;

        distance += resolution;
    }
    
    // Se non ti sei mosso, fallimento
    if (distance <= 0)
        return false;

    // Limita alla distanza massima richiesta
    if (distance > maxDistance)
    {
        distance = maxDistance;
        newX = X + incrementX * distance;
        newY = Y + incrementY * distance;
    }

    // Applica il movimento
    X = newX;
    Y = newY;

    return true;
}
```

**Uso tipico:**
```csharp
public override async ValueTask OnStepAsync()
{
    // L'oggetto si muove, ma si ferma ai muri
    MoveContactSolid(HSpeed, VSpeed);
}
```

### MoveOutsideSolid: Espulsione da Collisioni

Usato per "spingere fuori" un oggetto intrappolato in un muro:

```csharp
public bool MoveOutsideSolid(Angle direction, float maxDistance, float resolution = 1f)
{
    float incrementX = direction.Cos();
    float incrementY = -direction.Sin();
    var (newX, newY) = (X, Y);
    var distance = 0f;
    
    // Muoviti finché NON sei più in collisione
    while (distance <= maxDistance)
    {
        newX = X + incrementX * distance;
        newY = Y + incrementY * distance;
        
        if (IsPointFree(newX, newY))  // Trovata posizione libera!
            break;
        
        distance += resolution;
    }
    
    // ... applica movimento
}
```

---

## 6. Il Componente GameView - Bridge Blazor/Canvas

### Struttura del Componente

```razor
@foreach (var asset in assets)
{
    @* Precarica immagini in elementi nascosti *@
    @if (asset is ImageAsset imageGameAsset)
    {
        <img src="@asset.Source" @ref=imageGameAsset.ElementReference 
             style="display:none;" loading="eager" />
    }
}

<div class="gameview" tabindex="0" @ref=canvasContainer
     @onmousemove="OnMouseMove"
     @onmousedown="OnMouseDown"
     @onmouseup="OnMouseUp"
     @onkeydown="OnKeyDown"
     @onkeyup="OnKeyUp">

    @if (Game is not null)
    {
        <svg viewBox="0 0 @(Game.ViewWidth) @(Game.ViewHeight)">
            <foreignObject x="0" y="0" width="@(Game.ViewWidth)" height="@(Game.ViewHeight)">
                <div style="left: @(-Game.ViewX)px; top:@(-Game.ViewY)px; position: absolute;">
                    <BECanvas @ref=canvas Width="Game.RoomWidth" Height="Game.RoomHeight" />
                </div>
            </foreignObject>
        </svg>
    }
</div>
```

**Tecnica della Camera/Viewport:**
- `RoomWidth`/`RoomHeight`: dimensioni totali del "mondo"
- `ViewWidth`/`ViewHeight`: dimensioni della "finestra" visibile
- `ViewX`/`ViewY`: posizione della camera nel mondo
- SVG `viewBox` + `foreignObject` per clipping automatico
- CSS `left`/`top` negativi per pan della camera

### Inizializzazione e Game Loop

```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        // 1. Crea il context Canvas2D
        context = await canvas.CreateCanvas2DAsync();
        
        // 2. Crea riferimento .NET per callback da JS
        var reference = DotNetObjectReference.Create(this);

        // 3. Inizializza il gioco
        await Game.OnStartAsync();

        // 4. Avvia il game loop JavaScript
        await (await moduleTask.Value).InvokeVoidAsync("setFPS", Fps);
        await (await moduleTask.Value).InvokeVoidAsync(
            "executeFrame", 
            reference, 
            nameof(OnAnimationFrameAsync));
    }
}

[JSInvokable]
public async Task OnAnimationFrameAsync()
{
    await canvasContainer.FocusAsync();  // Per catturare input tastiera

    await context.BeginBatchAsync();     // Batching per performance
    
    // Clear del canvas
    if (context is Canvas2DContext ctx)
        await ctx.ClearRectAsync(0, 0, canvas.Width, canvas.Height);

    // Esegui frame del gioco
    await Game.ExecuteFrameAsync(context);

    await context.EndBatchAsync();       // Esegui tutti i comandi batched
    
    StateHasChanged();                   // Aggiorna UI Blazor (viewport)
}
```

**Ottimizzazione: BeginBatchAsync/EndBatchAsync**
- Accumula chiamate Canvas2D
- Invia tutto in una volta a JavaScript
- Riduce drasticamente l'overhead di interop

---

## 7. Dependency Injection per Asset

### Registrazione di Sprites

```csharp
public static IServiceCollection AddSprite<TSprite>(this IServiceCollection services) 
    where TSprite : SpriteAsset
{
    services.AddScoped<TSprite>();
    services.AddScoped<SpriteAsset>(sp => sp.GetRequiredService<TSprite>());
    services.AddScoped<IGameAsset>(sp => sp.GetRequiredService<TSprite>());
    return services;
}
```

**Registrazione multipla:**
- Come tipo concreto `TSprite`
- Come tipo base `SpriteAsset`
- Come interfaccia `IGameAsset`

Questo permette di:
```csharp
// Iniettare per tipo specifico
public Player(Game game, PlayerSpriteAsset sprite) { }

// Enumerare tutti gli asset
public GameView(IEnumerable<IGameAsset> assets) { }
```

### Auto-discovery di Sprite

```csharp
public static IServiceCollection AddSprites(this IServiceCollection services, Assembly assembly)
{
    var spriteTypes = assembly.GetTypes()
        .Where(t => t.IsSubclassOf(typeof(SpriteAsset)) && !t.IsAbstract);
    
    foreach (var spriteType in spriteTypes)
        services.AddSprite(spriteType);
    
    return services;
}
```

**Uso:**
```csharp
// In Program.cs
builder.Services.AddSprites(typeof(Program).Assembly);
```

Registra automaticamente tutti gli sprite trovati nell'assembly!

---

## 8. Il Tipo Angle - Type Safety per Angoli

### Problema: Gradi vs Radianti

Il codice con `float` per angoli è ambiguo:
```csharp
Rotate(90);        // Gradi o radianti?
Rotate(1.57f);     // ???
```

### Soluzione: Strong Typing

```csharp
[DebuggerDisplay("{ToRadians()} rad | {ToDegrees()}°")]
public readonly struct Angle : IEquatable<Angle>, IComparable<Angle>
{
    private readonly float radians;
    private Angle(float radians) => this.radians = radians;

    // Factory methods espliciti
    public static Angle FromDegrees(float degrees) => new Angle(degrees * MathF.PI / 180);
    public static Angle FromRadians(float radians) => new Angle(radians);

    // Conversione esplicita
    public float ToDegrees() => radians * 180 / MathF.PI;
    public float ToRadians() => radians;

    // Operatori matematici
    public static Angle operator +(Angle a, Angle b) => new Angle(a.radians + b.radians);
    public static Angle operator *(Angle a, float b) => new Angle(a.radians * b);
    
    // Funzioni trigonometriche
    public float Sin() => MathF.Sin(radians);
    public float Cos() => MathF.Cos(radians);
    
    // Utility
    public static Angle Atan2(float y, float x) => new Angle(MathF.Atan2(y, x));
}
```

**Uso:**
```csharp
var angle = Angle.FromDegrees(90);
var direction = Angle.Atan2(targetY - Y, targetX - X);
float vx = direction.Cos() * speed;
float vy = direction.Sin() * speed;
```

---

## 9. Pattern "Abstract Game Entity"

### Gerarchia di Ereditarietà

```
IGameEntity (interface)
    ↑
GameEntity (abstract)
    ↑
    ├─── Game (abstract)
    └─── GameObject (abstract)
```

### Perché GameEntity?

Evita duplicazione di codice tra `Game` e `GameObject`:

```csharp
public abstract class GameEntity(Game game) : IGameEntity
{
    // Metodi delegati al Game
    public virtual bool KeyboardCheck(string key) => game.KeyboardCheck(key);
    public virtual bool MouseCheckButton(MouseButton button) => game.MouseCheckButton(button);
    public virtual T InstanceCreate<T>() where T : GameObject => game.InstanceCreate<T>();
    
    // ... altre deleghe
}
```

**Beneficio:** Sia `Game` che `GameObject` possono chiamare gli stessi metodi:
```csharp
// In Game.OnStepAsync()
if (KeyboardCheck(KeyboardKeys.Escape))
    // ...

// In GameObject.OnStepAsync()
if (KeyboardCheck(KeyboardKeys.Space))
    // ...
```

Senza duplicare l'implementazione!

---

## 10. Performance: Batch Rendering

### Il Problema dell'Interop

Ogni chiamata da C# a JavaScript ha overhead:
```csharp
// 1000 draw calls = 1000 round-trip C# ↔ JS
for (int i = 0; i < 1000; i++)
    await context.DrawImageAsync(...);
```

### La Soluzione: Batching

```csharp
await context.BeginBatchAsync();

// Queste chiamate vengono accumulate
for (int i = 0; i < 1000; i++)
    await context.DrawImageAsync(...);

// Un solo round-trip per tutte le operazioni
await context.EndBatchAsync();
```

**Implementazione interna (concettuale):**
```csharp
private List<Action> batchedCommands = new();

public async ValueTask DrawImageAsync(...)
{
    if (isBatching)
        batchedCommands.Add(() => jsRuntime.InvokeVoidAsync("drawImage", ...));
    else
        await jsRuntime.InvokeVoidAsync("drawImage", ...);
}

public async ValueTask EndBatchAsync()
{
    // Serializza tutti i comandi e invia in blocco
    await jsRuntime.InvokeVoidAsync("executeBatch", batchedCommands);
    batchedCommands.Clear();
}
```

---

## Considerazioni Finali

### Punti di Forza

1. **Type Safety**: Uso di record, struct readonly e generics per codice type-safe
2. **Dependency Injection**: Asset e GameObject integrati con il DI container
3. **Async/Await**: Tutto l'engine è completamente asincrono
4. **Batching**: Rendering ottimizzato con batching delle chiamate Canvas
5. **Familiar API**: API simile a GameMaker, facile per chi viene da altri engine 2D

### Limitazioni Architetturali

1. **No ECS**: Architettura orientata agli oggetti, non Entity-Component-System
2. **Single-threaded**: Blazor WebAssembly è single-thread
3. **Overhead Interop**: Ogni frame richiede comunicazione C# ↔ JavaScript
4. **Pixel-perfect Collision**: Usa solo AABB, no pixel-perfect o poligoni

### Possibili Evoluzioni

- **WebGL Context**: Supporto per rendering WebGL invece di Canvas2D
- **Object Pooling**: Riutilizzo GameObject per ridurre GC
- **Spatial Hashing**: Ottimizzazione collision detection per grandi quantità di oggetti
- **Asset Streaming**: Caricamento lazy degli asset invece che tutti all'avvio

---

## Esempio Completo: Mini Gioco

```csharp
// 1. Definisci gli sprite
public record PlayerSprite : SpriteAsset
{
    public PlayerSprite() : base("/res/player.png", 32, 32, imageCount: 4, imagesPerRow: 4) { }
}

// 2. Registra nel DI
builder.Services.AddSprite<PlayerSprite>();
builder.Services.AddScoped<MyGame>();

// 3. GameObject
public class Player : GameObject
{
    public Player(Game game, PlayerSprite sprite) : base(game)
    {
        Sprite = sprite;
        BoundingBox = new BoundingBox(0, 0, 32, 32);
    }

    public override async ValueTask OnStepAsync()
    {
        if (KeyboardCheck(KeyboardKeys.ArrowRight))
            X += 2;
        if (KeyboardCheck(KeyboardKeys.ArrowLeft))
            X -= 2;
    }
}

// 4. Game
public class MyGame : Game
{
    public MyGame(IServiceProvider sp) : base(sp) 
    {
        RoomWidth = 800;
        RoomHeight = 600;
    }

    public override ValueTask OnStartAsync()
    {
        InstanceCreate<Player>(400, 300);
        return ValueTask.CompletedTask;
    }
}

// 5. Razor Page
<GameView Game="game" style="width: 800px; height: 600px;" />

@code {
    [Inject] private MyGame game { get; set; } = default!;
}
```

---

**Fine della presentazione tecnica.**

Questa architettura dimostra come sia possibile costruire un game engine moderno sfruttando le capacità di Blazor, C# e HTML5 Canvas, mantenendo un'API familiare e type-safe.
