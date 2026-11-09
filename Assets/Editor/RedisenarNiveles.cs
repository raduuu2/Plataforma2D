using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Herramienta TEMPORAL de editor: Plataforma2D > Rediseñar niveles
/// Vuelve a colocar las plataformas, monedas, enemigos, power-ups, checkpoints y decoración
/// de Nivel1 y NivelLava. No toca la cámara, el HUD, el fondo, la música ni el jugador.
/// Cuando esté hecho, se puede borrar este archivo.
/// </summary>
public static class RedisenarNiveles
{
    const string Prefabs = "Assets/Prefabs/Juego/";
    const string Tileset = "Assets/Sprites/Tileset/";
    const float LimiteX = 9f;     // interior de la torre: de -9 a 9
    const float AltoPlataforma = 0.74f;

    static GameObject pfPlat, pfMovil, pfPared, pfMoneda, pfRoja, pfEnemigo, pfVel, pfSalto, pfEscudo, pfCheck, pfMeta, pfLava;
    static Transform tPlat, tMon, tEne, tPow, tChk, tDeco;
    static System.Random rnd;
    static float y;      // altura (centro) de la última plataforma
    static float x;      // x de la última plataforma
    static int lado = 1; // para alternar izquierda / derecha

    // Decoración: número de tile -> pivote (centro abajo del dibujo)
    static readonly Dictionary<int, Vector2> Pivotes = new Dictionary<int, Vector2>
    {
        { 18, new Vector2(0.516f, 0.035f) }, { 19, new Vector2(0.494f, 0.004f) }, { 20, new Vector2(0.504f, 0.004f) },
        { 21, new Vector2(0.5f, 0.004f) },   { 22, new Vector2(0.57f, 0.043f) },  { 23, new Vector2(0.539f, 0.043f) },
        { 24, new Vector2(0.504f, 0.043f) }, { 25, new Vector2(0.512f, 0.004f) }, { 26, new Vector2(0.52f, 0.004f) },
        { 29, new Vector2(0.547f, 0.301f) }, { 31, new Vector2(0.512f, 0.254f) }, { 32, new Vector2(0.582f, 0.004f) },
        { 33, new Vector2(0.498f, 0.002f) }, { 34, new Vector2(0.498f, 0.002f) }, { 39, new Vector2(0.5f, 0.002f) },
        { 40, new Vector2(0.5f, 0.002f) },
    };
    static readonly int[] DecoPequena = { 18, 22, 23, 24, 25, 26, 29, 31, 32 };
    static readonly int[] DecoGrande = { 19, 20, 21, 33, 34, 39, 40 };
    static Dictionary<int, Sprite> deco = new Dictionary<int, Sprite>();

    static readonly string[] ObjetosNivel = { "Plataformas", "Monedas", "Enemigos", "PowerUps", "Checkpoints", "Decoracion", "Meta", "Lava" };

    [MenuItem("Plataforma2D/Rediseñar niveles")]
    public static void Ejecutar()
    {
        if (!EditorUtility.DisplayDialog("Rediseñar niveles",
                "Se van a volver a colocar las plataformas, enemigos, monedas, power-ups, checkpoints y decoración de Nivel1 y NivelLava.\n\nLa cámara, el HUD, el fondo, la música y el jugador no se tocan.",
                "Adelante", "Cancelar"))
            return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        pfPlat = Cargar("Plataforma"); pfMovil = Cargar("PlataformaMovil"); pfPared = Cargar("Pared");
        pfMoneda = Cargar("Moneda"); pfRoja = Cargar("MonedaRoja"); pfEnemigo = Cargar("EnemigoVolador");
        pfVel = Cargar("PowerUp_Velocidad"); pfSalto = Cargar("PowerUp_Salto"); pfEscudo = Cargar("PowerUp_Escudo");
        pfCheck = Cargar("Checkpoint"); pfMeta = Cargar("Meta"); pfLava = Cargar("Lava");

        PrepararDecoracion();

        Rehacer("Assets/Scenes/Nivel1.unity", false);
        Rehacer("Assets/Scenes/NivelLava.unity", true);

        EditorSceneManager.OpenScene("Assets/Scenes/Nivel1.unity");
        Debug.Log("[RedisenarNiveles] ¡Listo! Ya puedes borrar Assets/Editor.");
    }

    static GameObject Cargar(string nombre)
    {
        var go = AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + nombre + ".prefab");
        if (go == null) Debug.LogError("[RedisenarNiveles] No encuentro el prefab " + nombre);
        return go;
    }

    // Configura los tiles de decoración como sprites sueltos con el pivote en la base
    static void PrepararDecoracion()
    {
        deco.Clear();
        foreach (var par in Pivotes)
        {
            string ruta = Tileset + "Tile_" + par.Key + ".png";
            var imp = AssetImporter.GetAtPath(ruta) as TextureImporter;
            if (imp == null) continue;

            var ajustes = new TextureImporterSettings();
            imp.ReadTextureSettings(ajustes);
            bool cambiar = imp.spriteImportMode != SpriteImportMode.Single || imp.spritePixelsPerUnit != 200f
                           || ajustes.spriteAlignment != (int)SpriteAlignment.Custom || ajustes.spritePivot != par.Value;
            if (cambiar)
            {
                imp.textureType = TextureImporterType.Sprite;
                imp.spriteImportMode = SpriteImportMode.Single;
                imp.spritePixelsPerUnit = 200f;
                imp.ReadTextureSettings(ajustes);
                ajustes.spriteAlignment = (int)SpriteAlignment.Custom;
                ajustes.spritePivot = par.Value;
                imp.SetTextureSettings(ajustes);
                imp.SaveAndReimport();
            }
            var s = AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
            if (s != null) deco[par.Key] = s;
        }
    }

    // =====================================================================

    static void Rehacer(string ruta, bool conLava)
    {
        var escena = EditorSceneManager.OpenScene(ruta, OpenSceneMode.Single);

        // Quitamos el contenido viejo del nivel
        var borrar = new List<string>(ObjetosNivel);
        foreach (var go in escena.GetRootGameObjects())
            if (borrar.Contains(go.name)) Object.DestroyImmediate(go);

        var player = Object.FindAnyObjectByType<Player>();
        player.transform.position = new Vector3(0f, 0.3f, 0f);
        PrefabUtility.RecordPrefabInstancePropertyModifications(player.transform);

        tPlat = new GameObject("Plataformas").transform;
        tMon = new GameObject("Monedas").transform;
        tEne = new GameObject("Enemigos").transform;
        tPow = new GameObject("PowerUps").transform;
        tChk = new GameObject("Checkpoints").transform;
        tDeco = new GameObject("Decoracion").transform;

        rnd = new System.Random(conLava ? 2024 : 1987);
        y = 0f; x = 0f; lado = 1;

        if (conLava) DisenoLava(); else DisenoNivel1();

        // La cima: plataforma ancha y el cofre
        y += 2.6f;
        Plataforma(0f, y, 12f, false).name = "Cima";
        Poner(pfMeta, null, new Vector3(0f, y + 1.8f, 0f));
        for (int k = -2; k <= 2; k++)
            if (k != 0) Poner(pfMoneda, tMon, new Vector3(k * 1.6f, y + 1f, 0f));
        Decorar(DecoGrande, -4.5f, y);
        Decorar(DecoGrande, 4.5f, y);

        // Suelo y paredes de la torre
        float alto = y + 40f;
        Bloque(pfPared, 0f, -1.75f, LimiteX * 2f + 8f, 4f).name = "Suelo";
        Bloque(pfPared, -LimiteX - 2f, alto / 2f - 5f, 4f, alto).name = "ParedIzquierda";
        Bloque(pfPared, LimiteX + 2f, alto / 2f - 5f, 4f, alto).name = "ParedDerecha";
        Decorar(DecoGrande, -6f, -0.25f);
        Decorar(DecoPequena, 5f, -0.25f);

        if (conLava)
        {
            var lava = Poner(pfLava, null, new Vector3(0f, -24f, 0f)); // la superficie queda en y = -4
            var l = lava.GetComponent<Lava>();
            l.player = player;
            l.velocidad = 0.45f;
        }

        var gm = Object.FindAnyObjectByType<GameManager>();
        gm.lives = 5;
        EditorUtility.SetDirty(gm);

        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
        Debug.Log("[RedisenarNiveles] " + escena.name + ": cima a " + y.ToString("0") + " m");
    }

    // =====================================================================
    //  DISEÑO DE LOS NIVELES
    // =====================================================================

    static void DisenoNivel1()
    {
        // 1. La base: plataformas anchas para aprender
        Escalera(8, 4f, 5f, 0.85f, false);

        // 2. El zigzag: de pared a pared
        Checkpoint(pfSalto);
        Zigzag(12, 2.6f, 1);

        // 3. Los transbordadores: plataformas móviles horizontales
        Checkpoint(pfVel);
        Transbordadores(7, 2f, 3);

        // 4. El nido de demonios: dos caminos y muchos demonios
        Checkpoint(pfEscudo);
        Nido(12, 5);

        // 5. Los ascensores
        Checkpoint(null);
        Ascensores(5, 1.4f);

        // 6. La torre estrecha: saltos de precisión
        Checkpoint(pfSalto);
        Estrecha(12, 2);

        // 7. La recta final: un poco de todo
        Checkpoint(pfVel);
        Mixta(14, 4);
    }

    static void DisenoLava()
    {
        // La lava empuja: zonas más directas pero con riesgo
        Escalera(7, 3.5f, 4.5f, 0.7f, false);

        Checkpoint(null);
        Transbordadores(6, 2.4f, 1);

        Checkpoint(pfSalto);
        Ascensores(4, 1.6f);

        Checkpoint(null);
        Zigzag(12, 2.6f, 2);

        Checkpoint(pfEscudo);
        Nido(10, 4);

        Checkpoint(null);
        Estrecha(10, 2);

        Checkpoint(pfVel);
        Mixta(12, 3);
    }

    // =====================================================================
    //  ZONAS
    // =====================================================================

    // Escalones normales hacia arriba
    static void Escalera(int n, float anchoMin, float anchoMax, float probMonedas, bool enemigos)
    {
        for (int i = 0; i < n; i++)
        {
            float w = Azar(anchoMin, anchoMax);
            float nx = SiguienteX(w, 3f, 5.5f);
            y += Azar(2.4f, 2.7f);
            Plataforma(nx, y, w, true);
            if (Azar01() < probMonedas) Monedas(nx, y, Mathf.Min(3, (int)(w / 1.3f)));
            if (enemigos && i % 3 == 2) Enemigo(-nx * 0.5f, y + 1.4f, 3f, 1.8f);
            x = nx;
        }
    }

    // De pared a pared por carriles fijos
    static void Zigzag(int n, float w, int enemigos)
    {
        float[] carriles = { -6.6f, -2.2f, 2.2f, 6.6f };
        int c = x < 0 ? 0 : 3;
        int dir = c == 0 ? 1 : -1;
        int cadaEnemigo = enemigos > 0 ? n / (enemigos + 1) : 999;
        for (int i = 0; i < n; i++)
        {
            c += dir;
            if (c >= carriles.Length) { c = carriles.Length - 2; dir = -1; }
            if (c < 0) { c = 1; dir = 1; }
            y += 2.5f;
            Plataforma(carriles[c], y, w, true);
            Monedas(carriles[c], y, 1);
            if (i > 0 && i % cadaEnemigo == 0)
                Enemigo(-carriles[c] * 0.6f, y + 1.3f, 2.5f, 1.6f);
            x = carriles[c];
        }
        // Moneda roja en el borde del último carril
        Poner(pfRoja, tMon, new Vector3(x, y + 2.2f, 0f));
    }

    // Lado fijo -> plataforma móvil en el centro -> otro lado fijo
    static void Transbordadores(int n, float velocidad, int enemigos)
    {
        int cadaEnemigo = enemigos > 0 ? Mathf.Max(1, n / enemigos) : 999;
        float sx = x < 0 ? -6.8f : 6.8f;
        for (int i = 0; i < n; i++)
        {
            y += 2.4f;
            Movil(0f, y, 2.6f, false, 3.6f, velocidad + Azar(-0.2f, 0.3f));
            Monedas(0f, y + 0.6f, 1);
            sx = -sx;
            y += 2.4f;
            Plataforma(sx, y, 3.2f, true);
            if (i % 2 == 1) Monedas(sx, y, 2);
            if (i % cadaEnemigo == cadaEnemigo - 1)
                Enemigo(0f, y + 1.5f, 4f, 1.8f);
            x = sx;
        }
    }

    // Dos caminos (izquierda y derecha) con demonios en medio
    static void Nido(int n, int enemigos)
    {
        // Plataforma de entrada en el centro: desde aquí se puede ir por la izquierda o por la derecha
        y += 2.4f;
        Plataforma(0f, y, 3f, true);

        int puestos = 0;
        for (int i = 0; i < n; i++)
        {
            y += 2.6f;
            float desfase = (i % 2 == 0) ? 0.8f : -0.8f;
            Plataforma(-5.6f + desfase, y, 3.4f, i % 3 == 0);        // camino izquierdo
            Plataforma(5.6f + desfase, y - 1.3f, 3.4f, i % 3 == 1);  // camino derecho (a media altura)
            if (i % 4 == 2) Plataforma(0f, y + 1.3f, 2.4f, false);   // puente para cambiar de camino
            if (i % 2 == 0) Monedas(-5.6f + desfase, y, 2);
            else Monedas(5.6f + desfase, y - 1.3f, 2);
            if (puestos < enemigos && i % 2 == 1)
            {
                Enemigo(Azar(-1.5f, 1.5f), y + 1.9f, 3.5f, 2f);
                puestos++;
            }
            if (i == n / 2) Poner(pfRoja, tMon, new Vector3(0f, y + 2.6f, 0f)); // vigilada por los demonios
        }
        x = -5.6f; // se sale por el camino izquierdo, que es el más alto
    }

    // Plataformas verticales que te suben
    static void Ascensores(int n, float velocidad)
    {
        for (int i = 0; i < n; i++)
        {
            // El ascensor sale cerca de la plataforma anterior, hacia el centro
            lado = x > 0 ? -1 : 1;
            float ex = Mathf.Clamp(x + lado * Azar(3.5f, 4.5f), -6.5f, 6.5f);
            float dist = Azar(1.8f, 2.3f);
            // El ascensor va de y+2.4 (abajo) a y+2.4+2*dist (arriba)
            Movil(ex, y + 2.4f + dist, 2.6f, true, dist, velocidad);
            float arriba = y + 2.4f + dist * 2f;
            Poner(pfMoneda, tMon, new Vector3(ex, arriba + 1.2f, 0f));
            // Repisa al lado de lo más alto del ascensor
            float rx = Mathf.Clamp(ex - lado * 4.2f, -LimiteX + 1.5f, LimiteX - 1.5f);
            y = arriba + 1.6f;
            Plataforma(rx, y, 2.8f, true);
            if (i == n - 2) Enemigo(ex, y + 2f, 2.5f, 1.5f);
            x = rx;
        }
    }

    // Plataformas pequeñas y saltos largos
    static void Estrecha(int n, int enemigos)
    {
        int cadaEnemigo = enemigos > 0 ? n / (enemigos + 1) : 999;
        for (int i = 0; i < n; i++)
        {
            float nx = SiguienteX(2.2f, 3.2f, 4.4f);
            y += Azar(2.6f, 2.85f);
            Plataforma(nx, y, 2.2f, false);
            if (i % 2 == 0) Monedas(nx, y, 1);
            if (i > 0 && i % cadaEnemigo == 0) Enemigo(-nx * 0.5f, y + 1.2f, 2.5f, 2f);
            x = nx;
        }
        Poner(pfRoja, tMon, new Vector3(x, y + 2.4f, 0f)); // hay que saltar para cogerla
    }

    // Mezcla de fijas, móviles y enemigos
    static void Mixta(int n, int enemigos)
    {
        int puestos = 0;
        for (int i = 0; i < n; i++)
        {
            int tipo = i % 4;
            if (tipo == 1)
            {
                y += 2.4f;
                Movil(0f, y, 2.4f, false, 4.2f, 2.8f);
                Monedas(0f, y + 0.6f, 1);
                x = Azar01() < 0.5f ? -1f : 1f;
            }
            else if (tipo == 3)
            {
                // El ascensor sale cerca de la plataforma anterior
                lado = x > 0 ? -1 : 1;
                float ex = Mathf.Clamp(x + lado * 4f, -6.5f, 6.5f);
                float dist = 1.8f;
                Movil(ex, y + 2.4f + dist, 2.4f, true, dist, 2f);
                y += 2.4f + dist * 2f + 1.6f;
                float rx = ex - lado * 4.3f;
                Plataforma(rx, y, 2.4f, false);
                x = rx;
            }
            else
            {
                float nx = SiguienteX(2.6f, 3f, 5f);
                y += Azar(2.4f, 2.7f);
                Plataforma(nx, y, 2.6f, true);
                Monedas(nx, y, 2);
                x = nx;
            }
            if (puestos < enemigos && i % 3 == 2)
            {
                Enemigo(Azar(-4f, 4f), y + 1.6f, 3f, 2.2f);
                puestos++;
            }
        }
    }

    // Plataforma ancha con checkpoint y, si se pasa, un power-up
    static void Checkpoint(GameObject powerUp)
    {
        float nx = SiguienteX(5f, 2.5f, 4f);
        y += 2.5f;
        Plataforma(nx, y, 5f, false);
        Poner(pfCheck, tChk, new Vector3(nx - 1.6f, y + 0.25f, 0f));
        if (powerUp != null) Poner(powerUp, tPow, new Vector3(nx + 1.3f, y + 1.1f, 0f));
        x = nx;
    }

    // =====================================================================
    //  PIEZAS
    // =====================================================================

    static GameObject Plataforma(float px, float py, float w, bool decorar)
    {
        px = Mathf.Clamp(px, -LimiteX + w / 2f, LimiteX - w / 2f);
        var go = Bloque(pfPlat, px, py, w, 0.5f);
        if (decorar)
        {
            float r = Azar01();
            if (w >= 4f && r < 0.35f) Decorar(DecoGrande, px + Azar(-w / 2f + 0.8f, w / 2f - 0.8f), py);
            else if (r < 0.7f) Decorar(DecoPequena, px + Azar(-w / 2f + 0.5f, w / 2f - 0.5f), py);
        }
        return go;
    }

    static void Movil(float px, float py, float w, bool vertical, float distancia, float velocidad)
    {
        var go = Bloque(pfMovil, px, py, w, 0.5f);
        var pm = go.GetComponent<PlataformaMovil>();
        pm.vertical = vertical;
        pm.distancia = distancia;
        pm.velocidad = velocidad;
    }

    static GameObject Bloque(GameObject prefab, float px, float py, float w, float h)
    {
        var go = Poner(prefab, tPlat, new Vector3(px, py, 0f));
        var sr = go.GetComponent<SpriteRenderer>();
        float altoDibujo = prefab == pfPared ? h : AltoPlataforma;
        sr.size = new Vector2(w, altoDibujo);
        go.GetComponent<BoxCollider2D>().size = new Vector2(w, h);
        return go;
    }

    static void Monedas(float cx, float py, int n)
    {
        for (int k = 0; k < n; k++)
            Poner(pfMoneda, tMon, new Vector3(cx + (k - (n - 1) / 2f) * 0.9f, py + 0.95f, 0f));
    }

    static void Enemigo(float ex, float ey, float distancia, float velocidad)
    {
        ex = Mathf.Clamp(ex, -LimiteX + distancia + 0.5f, LimiteX - distancia - 0.5f);
        var e = Poner(pfEnemigo, tEne, new Vector3(ex, ey, 0f)).GetComponent<EnemigoVolador>();
        e.distancia = distancia;
        e.velocidad = velocidad;
    }

    static void Decorar(int[] lista, float dx, float plataformaY)
    {
        int tile = lista[rnd.Next(lista.Length)];
        if (!deco.ContainsKey(tile)) return;
        var go = new GameObject("Deco_" + tile);
        go.transform.SetParent(tDeco, false);
        go.transform.position = new Vector3(dx, plataformaY + 0.2f, 0f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = deco[tile];
        sr.flipX = Azar01() < 0.5f;
        // Lo grande va detrás de las plataformas; lo pequeño delante (pero detrás del jugador)
        sr.sortingOrder = System.Array.IndexOf(DecoGrande, tile) >= 0 ? -1 : 2;
    }

    static GameObject Poner(GameObject prefab, Transform padre, Vector3 pos)
    {
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, padre);
        go.transform.position = pos;
        return go;
    }

    // Siguiente x a cierta distancia de la anterior, sin salirse de la torre
    static float SiguienteX(float w, float dMin, float dMax)
    {
        float limite = LimiteX - w / 2f - 0.2f;
        float dx = Azar(dMin, dMax) * (Azar01() < 0.5f ? -1f : 1f);
        float nx = x + dx;
        if (nx > limite || nx < -limite) nx = x - dx;
        return Mathf.Clamp(nx, -limite, limite);
    }

    static float Azar01() { return (float)rnd.NextDouble(); }
    static float Azar(float a, float b) { return a + (b - a) * (float)rnd.NextDouble(); }
}
