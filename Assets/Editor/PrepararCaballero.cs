using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;

/// <summary>
/// Herramienta de editor (no va en el juego final).
/// Menú: Plataforma2D > Preparar caballero
///  1. Configura y corta las hojas de sprites (mismo tamaño, pixel art nítido, pivote en los pies).
///  2. Crea las animaciones Idle / Run / Jump / Fall y el Animator Controller.
///  3. Pone el caballero en el objeto con el script Player de la escena abierta.
/// Se puede ejecutar varias veces sin problema.
/// </summary>
public static class PrepararCaballero
{
    const int PixelsPorUnidad = 32;
    const string CarpetaJugador = "Assets/Sprites/Player/";
    const string CarpetaEnemigos = "Assets/Sprites/Enemigos/";
    const string CarpetaAnim = "Assets/Animations/Caballero/";

    // Cada cuadro del caballero mide 112x96 px y los pies están a 11 px del borde inferior.
    static readonly Vector2 PivoteCaballero = new Vector2(0.5f, 11f / 96f);

    [MenuItem("Plataforma2D/Preparar caballero")]
    public static void Ejecutar()
    {
        // ---- 1. Sprites ----
        string[] hojasCaballero = { "IDLE", "RUN", "JUMP-START", "JUMP-TRANSITION", "JUMP-FALL", "ATTACK 1", "HURT", "DEATH" };
        foreach (var hoja in hojasCaballero)
            CortarEnRejilla(CarpetaJugador + hoja + ".png", 112, 96, SpriteAlignment.Custom, PivoteCaballero, PixelsPorUnidad);

        // Enemigo volador (lo usará el compañero): cuadros de 81x71 px
        CortarEnRejilla(CarpetaEnemigos + "Demon_FLYING.png", 81, 71, SpriteAlignment.Center, new Vector2(0.5f, 0.5f), 48);
        CortarEnRejilla(CarpetaEnemigos + "Demon_DEATH.png", 81, 71, SpriteAlignment.Center, new Vector2(0.5f, 0.5f), 48);

        AssetDatabase.Refresh();

        // ---- 2. Animaciones ----
        Directory.CreateDirectory(CarpetaAnim);
        var idle = CrearClip("Idle", CargarSprites(CarpetaJugador + "IDLE.png"), 10, true);
        var run = CrearClip("Run", CargarSprites(CarpetaJugador + "RUN.png"), 12, true);
        var jump = CrearClip("Jump", CargarSprites(CarpetaJugador + "JUMP-START.png"), 10, true);
        var fall = CrearClip("Fall", CargarSprites(CarpetaJugador + "JUMP-FALL.png"), 10, true);
        if (idle == null || run == null || jump == null || fall == null)
        {
            Debug.LogError("[PrepararCaballero] Faltan sprites del caballero. ¿Están en Assets/Sprites/Player?");
            return;
        }
        var controller = CrearAnimator(idle, run, jump, fall);

        // ---- 3. Jugador de la escena ----
        ColocarEnJugador(controller, CargarSprites(CarpetaJugador + "IDLE.png")[0]);

        AssetDatabase.SaveAssets();
        Debug.Log("[PrepararCaballero] ¡Listo! Guarda la escena con Ctrl+S.");
    }

    // ------------------------------------------------------------------
    static void CortarEnRejilla(string ruta, int anchoCuadro, int altoCuadro, SpriteAlignment alineacion, Vector2 pivote, int ppu)
    {
        var importer = AssetImporter.GetAtPath(ruta) as TextureImporter;
        if (importer == null)
        {
            Debug.LogWarning("[PrepararCaballero] No encontrado: " + ruta);
            return;
        }

        // Ajustes de importación para pixel art
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = ppu;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.SaveAndReimport();

        // Cortar con el Sprite Editor Data Provider
        var factory = new SpriteDataProviderFactories();
        factory.Init();
        var provider = factory.GetSpriteEditorDataProviderFromObject(importer);
        if (provider == null)
        {
            Debug.LogError("[PrepararCaballero] El importador no permite editar sprites: " + ruta);
            return;
        }
        provider.InitSpriteEditorDataProvider();

        var capacidad = provider.GetDataProvider<ISpriteFrameEditCapability>();
        if (capacidad != null)
        {
            var cap = capacidad.GetEditCapability();
            if (!cap.HasCapability(EEditCapability.CreateAndDeleteSprite) || !cap.HasCapability(EEditCapability.EditPivot))
            {
                Debug.LogError("[PrepararCaballero] El importador no permite cortar sprites: " + ruta);
                return;
            }
        }

        var texturaInfo = provider.GetDataProvider<ITextureDataProvider>();
        int ancho, alto;
        texturaInfo.GetTextureActualWidthAndHeight(out ancho, out alto);

        string nombreBase = Path.GetFileNameWithoutExtension(ruta).Replace(" ", "_");
        var existentes = provider.GetSpriteRects().ToDictionary(r => r.name, r => r.spriteID);

        int cuadros = ancho / anchoCuadro;
        var rects = new List<SpriteRect>();
        for (int i = 0; i < cuadros; i++)
        {
            string nombre = nombreBase + "_" + i;
            rects.Add(new SpriteRect
            {
                name = nombre,
                rect = new Rect(i * anchoCuadro, alto - altoCuadro, anchoCuadro, altoCuadro),
                alignment = alineacion,
                pivot = pivote,
                // Reutilizamos el ID si ya existía, para no romper referencias
                spriteID = existentes.TryGetValue(nombre, out var id) ? id : GUID.Generate()
            });
        }
        provider.SetSpriteRects(rects.ToArray());

        var nombresIds = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
        if (nombresIds != null)
            nombresIds.SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)).ToList());

        provider.Apply();
        importer.SaveAndReimport();
    }

    static Sprite[] CargarSprites(string ruta)
    {
        return AssetDatabase.LoadAllAssetsAtPath(ruta)
            .OfType<Sprite>()
            .OrderBy(s => int.TryParse(s.name.Substring(s.name.LastIndexOf('_') + 1), out var n) ? n : 0)
            .ToArray();
    }

    static AnimationClip CrearClip(string nombre, Sprite[] sprites, float fps, bool bucle)
    {
        if (sprites == null || sprites.Length == 0) return null;

        string ruta = CarpetaAnim + nombre + ".anim";
        AssetDatabase.DeleteAsset(ruta);

        var clip = new AnimationClip { frameRate = fps };
        var binding = new EditorCurveBinding { type = typeof(SpriteRenderer), path = "", propertyName = "m_Sprite" };

        var claves = new ObjectReferenceKeyframe[sprites.Length + 1];
        for (int i = 0; i < sprites.Length; i++)
            claves[i] = new ObjectReferenceKeyframe { time = i / fps, value = sprites[i] };
        // Repetimos el último cuadro para que dure lo mismo que los demás
        claves[sprites.Length] = new ObjectReferenceKeyframe { time = sprites.Length / fps, value = sprites[sprites.Length - 1] };
        AnimationUtility.SetObjectReferenceCurve(clip, binding, claves);

        var ajustes = AnimationUtility.GetAnimationClipSettings(clip);
        ajustes.loopTime = bucle;
        AnimationUtility.SetAnimationClipSettings(clip, ajustes);

        AssetDatabase.CreateAsset(clip, ruta);
        return clip;
    }

    static AnimatorController CrearAnimator(AnimationClip idle, AnimationClip run, AnimationClip jump, AnimationClip fall)
    {
        string ruta = CarpetaAnim + "Caballero.controller";
        AssetDatabase.DeleteAsset(ruta);
        var ac = AnimatorController.CreateAnimatorControllerAtPath(ruta);

        // Mismos nombres que usa Player.cs
        ac.AddParameter("movement", AnimatorControllerParameterType.Float);
        ac.AddParameter("enSuelo", AnimatorControllerParameterType.Bool);
        ac.AddParameter("velocidadY", AnimatorControllerParameterType.Float);

        var sm = ac.layers[0].stateMachine;
        var sIdle = sm.AddState("Idle", new Vector3(300, 0));
        var sRun = sm.AddState("Run", new Vector3(300, 120));
        var sJump = sm.AddState("Jump", new Vector3(600, 0));
        var sFall = sm.AddState("Fall", new Vector3(600, 120));
        sIdle.motion = idle; sRun.motion = run; sJump.motion = jump; sFall.motion = fall;
        sm.defaultState = sIdle;

        // En el suelo: Idle <-> Run
        Transicion(sIdle, sRun).AddCondition(AnimatorConditionMode.Greater, 0.1f, "movement");
        Transicion(sRun, sIdle).AddCondition(AnimatorConditionMode.Less, 0.1f, "movement");

        // Despegar: subiendo -> Jump, cayendo -> Fall
        foreach (var desde in new[] { sIdle, sRun })
        {
            var t = Transicion(desde, sJump);
            t.AddCondition(AnimatorConditionMode.IfNot, 0, "enSuelo");
            t.AddCondition(AnimatorConditionMode.Greater, 0.1f, "velocidadY");
            t = Transicion(desde, sFall);
            t.AddCondition(AnimatorConditionMode.IfNot, 0, "enSuelo");
            t.AddCondition(AnimatorConditionMode.Less, -0.1f, "velocidadY");
        }

        // En el aire: Jump -> Fall al empezar a bajar
        Transicion(sJump, sFall).AddCondition(AnimatorConditionMode.Less, 0f, "velocidadY");

        // Aterrizar
        foreach (var desde in new[] { sJump, sFall })
        {
            var t = Transicion(desde, sIdle);
            t.AddCondition(AnimatorConditionMode.If, 0, "enSuelo");
            t.AddCondition(AnimatorConditionMode.Less, 0.1f, "movement");
            t = Transicion(desde, sRun);
            t.AddCondition(AnimatorConditionMode.If, 0, "enSuelo");
            t.AddCondition(AnimatorConditionMode.Greater, 0.1f, "movement");
        }

        return ac;
    }

    static AnimatorStateTransition Transicion(AnimatorState desde, AnimatorState hacia)
    {
        var t = desde.AddTransition(hacia);
        t.hasExitTime = false;
        t.duration = 0f;
        return t;
    }

    static void ColocarEnJugador(AnimatorController controller, Sprite spriteInicial)
    {
        var jugador = Object.FindAnyObjectByType<Player>();
        if (jugador == null)
        {
            Debug.LogWarning("[PrepararCaballero] No hay ningún objeto con el script Player en la escena abierta. " +
                             "Las animaciones se han creado igualmente en " + CarpetaAnim);
            return;
        }

        var go = jugador.gameObject;
        Undo.RecordObject(go.transform, "Preparar caballero");
        go.transform.localScale = Vector3.one; // Player.cs usa la escala para girarse

        var sr = go.GetComponent<SpriteRenderer>();
        if (sr == null) sr = Undo.AddComponent<SpriteRenderer>(go);
        Undo.RecordObject(sr, "Preparar caballero");
        sr.sprite = spriteInicial;
        sr.color = Color.white;
        sr.sortingOrder = 10; // por delante de las plataformas

        var anim = go.GetComponent<Animator>();
        if (anim == null) anim = Undo.AddComponent<Animator>(go);
        Undo.RecordObject(anim, "Preparar caballero");
        anim.runtimeAnimatorController = controller;

        // Collider ajustado al cuerpo del caballero (el pivote está en los pies)
        var box = go.GetComponent<BoxCollider2D>();
        if (box != null)
        {
            Undo.RecordObject(box, "Preparar caballero");
            box.size = new Vector2(0.6f, 1.1f);
            box.offset = new Vector2(0f, 0.55f);
        }

        EditorSceneManager.MarkSceneDirty(go.scene);
        Selection.activeGameObject = go;
    }
}
