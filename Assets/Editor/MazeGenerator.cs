using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class MazeGeneratorWindow : EditorWindow
{
    [MenuItem("Maze/Maze Generator Window")]
    public static void ShowWindow()
    {
        GetWindow<MazeGeneratorWindow>("Maze Generator");
    }

    [MenuItem("Maze/Generate Maze In Current Scene")]
    public static void GenerateFromMenu()
    {
        MazeGeneratorLogic.GenerateMaze(8, 8, 42, true, true, true, true, true);
    }

    [MenuItem("Maze/Clear Maze In Current Scene")]
    public static void ClearFromMenu()
    {
        MazeGeneratorLogic.ClearExistingMaze();
    }

    private int width = 8;
    private int height = 8;
    private int seed = 42;
    private bool addPillars = true;
    private bool addTorches = true;
    private bool addTrees = true;
    private bool addGoalSwitch = true;
    private bool setupCameraAndLight = true;

    private void OnGUI()
    {
        GUILayout.Label("Maze Generation Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        width = EditorGUILayout.IntSlider("Width (Cells)", width, 4, 20);
        height = EditorGUILayout.IntSlider("Height (Cells)", height, 4, 20);
        
        EditorGUILayout.BeginHorizontal();
        seed = EditorGUILayout.IntField("Random Seed", seed);
        if (GUILayout.Button("Randomize", GUILayout.Width(80)))
        {
            seed = UnityEngine.Random.Range(1, 999999);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);
        GUILayout.Label("Features", EditorStyles.boldLabel);
        addPillars = EditorGUILayout.Toggle("Add Corner Pillars", addPillars);
        addTorches = EditorGUILayout.Toggle("Add Wall Torches", addTorches);
        addTrees = EditorGUILayout.Toggle("Add Surrounding Trees", addTrees);
        addGoalSwitch = EditorGUILayout.Toggle("Add Goal Switch", addGoalSwitch);
        setupCameraAndLight = EditorGUILayout.Toggle("Adjust Camera & Light", setupCameraAndLight);

        EditorGUILayout.Space(10);
        if (GUILayout.Button("Generate Maze", GUILayout.Height(35)))
        {
            MazeGeneratorLogic.GenerateMaze(width, height, seed, addPillars, addTorches, addTrees, addGoalSwitch, setupCameraAndLight);
        }

        if (GUILayout.Button("Clear Maze", GUILayout.Height(25)))
        {
            MazeGeneratorLogic.ClearExistingMaze();
        }
    }
}

public static class MazeGeneratorLogic
{
    private const float CellSize = 3f;

    // Prefab GUIDs
    private const string FloorGuid = "42471d84493492140af70847f296bd30";     // Floor_3M
    private const string WallGuid = "54d08ba60661ea943a645c9fdbb4d353";      // Wall_3M
    private const string WallDoorGuid = "a822b738299005140bf66e8205f0fa39";  // Wall_Door
    private const string PillarGuid = "623bb50d8d0b3d54aa4a8f6566798d70";    // Pilar
    private const string TorchGuid = "52854b7453417924faa50fb7d021a14d";     // Wall_Light
    private const string LampGuid = "c0907f21c8d187b43829eb485f4489d7";      // Wall_Lamp
    private const string SwitchGuid = "0aa37b6666ba4d642921aa823a3add65";    // Switch
    private const string GapGuid = "5a1b1bec0f03d1348a3ae23ffaecb6a2";       // Gap
    private static readonly string[] TreeGuids = new string[]
    {
        "7d3229d05242c074a9369511c81961ab", // Tree_1_V1
        "88d93d503cd32814a88b57ca365eedcc", // Tree_1_V2
        "faa8fddb50fd6eb489905462ca588a22", // Tree_2_V1
        "c962f204ebe1284408382b56980e220d", // Tree_2_V2
        "73f70af9d9a62974c825d662f67408cd", // Tree_3_V1
        "c617f1716d4fb6f4babd33e9d45ec7a2", // Tree_3_V2
        "d3f4a10d436b6cd4db9a8861784327b3", // Tree_4_V1
        "e213e39257a3b0e4492e17f9fe253453"  // Tree_4_V2
    };

    public static void ClearExistingMaze()
    {
        GameObject existing = GameObject.Find("Maze");
        if (existing != null)
        {
            Undo.DestroyObjectImmediate(existing);
            Debug.Log("[MazeGenerator] Cleared existing Maze hierarchy.");
        }
    }

    public static void GenerateMaze(int width, int height, int seed, bool addPillars, bool addTorches, bool addTrees, bool addGoalSwitch, bool setupCamera)
    {
        ClearExistingMaze();

        GameObject mazeRoot = new GameObject("Maze");
        Undo.RegisterCreatedObjectUndo(mazeRoot, "Generate Maze");

        GameObject floorsParent = new GameObject("Floors");
        floorsParent.transform.SetParent(mazeRoot.transform, false);

        GameObject wallsParent = new GameObject("Walls");
        wallsParent.transform.SetParent(mazeRoot.transform, false);

        GameObject pillarsParent = new GameObject("Pillars");
        pillarsParent.transform.SetParent(mazeRoot.transform, false);

        GameObject propsParent = new GameObject("Props_and_Torches");
        propsParent.transform.SetParent(mazeRoot.transform, false);

        GameObject treesParent = new GameObject("Trees_Perimeter");
        treesParent.transform.SetParent(mazeRoot.transform, false);

        // Load Prefabs
        GameObject floorPrefab = LoadPrefab(FloorGuid);
        GameObject wallPrefab = LoadPrefab(WallGuid);
        GameObject doorPrefab = LoadPrefab(WallDoorGuid);
        GameObject pillarPrefab = LoadPrefab(PillarGuid);
        GameObject torchPrefab = LoadPrefab(TorchGuid);
        GameObject lampPrefab = LoadPrefab(LampGuid);
        GameObject switchPrefab = LoadPrefab(SwitchGuid);
        GameObject gapPrefab = LoadPrefab(GapGuid);

        List<GameObject> treePrefabs = new List<GameObject>();
        foreach (var guid in TreeGuids)
        {
            var p = LoadPrefab(guid);
            if (p != null) treePrefabs.Add(p);
        }

        // Maze DFS Generation
        System.Random rand = new System.Random(seed);
        bool[,] horiz = new bool[height + 1, width];
        bool[,] vert = new bool[height, width + 1];

        for (int r = 0; r <= height; r++)
            for (int c = 0; c < width; c++)
                horiz[r, c] = true;

        for (int r = 0; r < height; r++)
            for (int c = 0; c <= width; c++)
                vert[r, c] = true;

        bool[,] visited = new bool[height, width];
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        visited[0, 0] = true;
        stack.Push(new Vector2Int(0, 0));

        while (stack.Count > 0)
        {
            Vector2Int current = stack.Peek();
            List<(int dir, Vector2Int neighbor)> neighbors = new List<(int, Vector2Int)>();

            // 0: North, 1: South, 2: East, 3: West
            if (current.y + 1 < height && !visited[current.y + 1, current.x])
                neighbors.Add((0, new Vector2Int(current.x, current.y + 1)));
            if (current.y - 1 >= 0 && !visited[current.y - 1, current.x])
                neighbors.Add((1, new Vector2Int(current.x, current.y - 1)));
            if (current.x + 1 < width && !visited[current.y, current.x + 1])
                neighbors.Add((2, new Vector2Int(current.x + 1, current.y)));
            if (current.x - 1 >= 0 && !visited[current.y, current.x - 1])
                neighbors.Add((3, new Vector2Int(current.x - 1, current.y)));

            if (neighbors.Count > 0)
            {
                var chosen = neighbors[rand.Next(neighbors.Count)];
                if (chosen.dir == 0) // North
                    horiz[current.y + 1, current.x] = false;
                else if (chosen.dir == 1) // South
                    horiz[current.y, current.x] = false;
                else if (chosen.dir == 2) // East
                    vert[current.y, current.x + 1] = false;
                else if (chosen.dir == 3) // West
                    vert[current.y, current.x] = false;

                visited[chosen.neighbor.y, chosen.neighbor.x] = true;
                stack.Push(chosen.neighbor);
            }
            else
            {
                stack.Pop();
            }
        }

        // Add braid loops (3-5 extra openings for interesting paths)
        int extraLoops = Mathf.Max(2, width * height / 16);
        for (int i = 0; i < extraLoops; i++)
        {
            int rx = rand.Next(1, width - 1);
            int ry = rand.Next(1, height - 1);
            if (rand.Next(2) == 0)
                horiz[ry, rx] = false;
            else
                vert[ry, rx] = false;
        }

        // 1. Instantiate Floors
        for (int r = 0; r < height; r++)
        {
            for (int c = 0; c < width; c++)
            {
                Vector3 pos = new Vector3((c + 1) * CellSize, 0, r * CellSize);
                // Check if this is a hazard trap tile (rare dead-end)
                if (gapPrefab != null && r == height - 1 && c == 0)
                {
                    InstantiateInstance(gapPrefab, pos, Quaternion.identity, floorsParent.transform, $"Gap_{c}_{r}");
                }
                else if (floorPrefab != null)
                {
                    InstantiateInstance(floorPrefab, pos, Quaternion.identity, floorsParent.transform, $"Floor_{c}_{r}");
                }
            }
        }

        // 2. Instantiate Horizontal Walls
        for (int r = 0; r <= height; r++)
        {
            for (int c = 0; c < width; c++)
            {
                Vector3 pos = new Vector3((c + 1) * CellSize, 0, r * CellSize);
                Quaternion rot = Quaternion.identity;

                // Entrance at (0, 0) South wall
                if (r == 0 && c == 0)
                {
                    if (doorPrefab != null)
                        InstantiateInstance(doorPrefab, pos, rot, wallsParent.transform, "Entrance_Door");
                    continue;
                }

                // Exit at (width-1, height) North wall
                if (r == height && c == width - 1)
                {
                    if (doorPrefab != null)
                        InstantiateInstance(doorPrefab, pos, rot, wallsParent.transform, "Exit_Door");
                    continue;
                }

                if (horiz[r, c] && wallPrefab != null)
                {
                    GameObject w = InstantiateInstance(wallPrefab, pos, rot, wallsParent.transform, $"Wall_H_{c}_{r}");
                    // Place torches on select interior walls
                    if (addTorches && torchPrefab != null && (c + r) % 3 == 0 && rand.Next(100) < 60)
                    {
                        Vector3 torchPos = pos + new Vector3(-1.5f, 2.5f, 0.2f);
                        InstantiateInstance(torchPrefab, torchPos, rot, propsParent.transform, $"Torch_H_{c}_{r}");
                    }
                }
            }
        }

        // 3. Instantiate Vertical Walls
        for (int r = 0; r < height; r++)
        {
            for (int c = 0; c <= width; c++)
            {
                if (vert[r, c] && wallPrefab != null)
                {
                    Vector3 pos = new Vector3(c * CellSize, 0, r * CellSize);
                    Quaternion rot = Quaternion.Euler(0, 90, 0);
                    GameObject w = InstantiateInstance(wallPrefab, pos, rot, wallsParent.transform, $"Wall_V_{c}_{r}");

                    // Place torches on select vertical walls
                    if (addTorches && torchPrefab != null && (c + r) % 3 == 1 && rand.Next(100) < 60)
                    {
                        Vector3 torchPos = pos + new Vector3(-0.2f, 2.5f, 1.5f);
                        InstantiateInstance(torchPrefab, torchPos, rot, propsParent.transform, $"Torch_V_{c}_{r}");
                    }
                }
            }
        }

        // 4. Instantiate Pillars at all wall corners
        if (addPillars && pillarPrefab != null)
        {
            for (int r = 0; r <= height; r++)
            {
                for (int c = 0; c <= width; c++)
                {
                    Vector3 pos = new Vector3(c * CellSize, 0, r * CellSize);
                    InstantiateInstance(pillarPrefab, pos, Quaternion.identity, pillarsParent.transform, $"Pillar_{c}_{r}");
                }
            }
        }

        // 5. Goal Switch at exit cell
        if (addGoalSwitch && switchPrefab != null)
        {
            Vector3 switchPos = new Vector3((width - 1) * CellSize + 1.5f, 1.2f, (height - 1) * CellSize + 2.8f);
            InstantiateInstance(switchPrefab, switchPos, Quaternion.Euler(0, 180, 0), propsParent.transform, "Goal_Switch");
        }

        // 6. Surround the perimeter with trees
        if (addTrees && treePrefabs.Count > 0)
        {
            float margin = 4.5f;
            float totalW = width * CellSize;
            float totalH = height * CellSize;

            for (float x = -margin; x <= totalW + margin; x += 4f)
            {
                // South row (skip right at entrance)
                if (Mathf.Abs(x - 1.5f) > 3f)
                {
                    var tp = treePrefabs[rand.Next(treePrefabs.Count)];
                    InstantiateInstance(tp, new Vector3(x, 0, -margin), Quaternion.Euler(0, rand.Next(360), 0), treesParent.transform, "Tree_S");
                }
                // North row (skip right at exit)
                if (Mathf.Abs(x - (totalW - 1.5f)) > 3f)
                {
                    var tp = treePrefabs[rand.Next(treePrefabs.Count)];
                    InstantiateInstance(tp, new Vector3(x, 0, totalH + margin), Quaternion.Euler(0, rand.Next(360), 0), treesParent.transform, "Tree_N");
                }
            }

            for (float z = 0; z <= totalH; z += 4f)
            {
                var tp1 = treePrefabs[rand.Next(treePrefabs.Count)];
                InstantiateInstance(tp1, new Vector3(-margin, 0, z), Quaternion.Euler(0, rand.Next(360), 0), treesParent.transform, "Tree_W");

                var tp2 = treePrefabs[rand.Next(treePrefabs.Count)];
                InstantiateInstance(tp2, new Vector3(totalW + margin, 0, z), Quaternion.Euler(0, rand.Next(360), 0), treesParent.transform, "Tree_E");
            }
        }

        // 7. Adjust Main Camera & Directional Light
        if (setupCamera)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.transform.position = new Vector3(width * CellSize * 0.5f, width * CellSize * 1.1f, -CellSize * 2.5f);
                cam.transform.rotation = Quaternion.Euler(60f, 0f, 0f);
                Debug.Log("[MazeGenerator] Configured Camera position and angle.");
            }

            Light dirLight = Object.FindFirstObjectByType<Light>();
            if (dirLight != null && dirLight.type == LightType.Directional)
            {
                dirLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
                dirLight.color = new Color(1f, 0.96f, 0.84f);
                dirLight.intensity = 1.2f;
                dirLight.shadows = LightShadows.Soft;
            }
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log($"[MazeGenerator] Successfully generated {width}x{height} maze! (Seed: {seed})");
    }

    private static GameObject LoadPrefab(string guid)
    {
        string path = AssetDatabase.GUIDToAssetPath(guid);
        if (string.IsNullOrEmpty(path)) return null;
        return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }

    private static GameObject InstantiateInstance(GameObject prefab, Vector3 pos, Quaternion rot, Transform parent, string name)
    {
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        if (instance != null)
        {
            instance.name = name;
            instance.transform.localPosition = pos;
            instance.transform.localRotation = rot;
            Undo.RegisterCreatedObjectUndo(instance, "Create " + name);
        }
        return instance;
    }
}
