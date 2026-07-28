using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace TryVR.Editor
{
    public static class StudioApartmentShellBuilder
    {
        [MenuItem("Tools/Build Studio Apartment Shell")]
        public static string Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject shellRoot = new GameObject("StudioApartment_ArchitecturalShell");

            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null) litShader = Shader.Find("Standard");

            Material CreateMat(string name, Color color, float smoothness = 0.3f, float metallic = 0.0f, bool isTransparent = false)
            {
                Material mat = new Material(litShader);
                mat.name = name;
                mat.color = color;
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
                if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);

                if (isTransparent)
                {
                    if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1);
                    mat.SetOverrideTag("RenderType", "Transparent");
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.SetInt("_ZWrite", 0);
                    mat.DisableKeyword("_ALPHATEST_ON");
                    mat.EnableKeyword("_ALPHABLEND_ON");
                    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                }

                if (!AssetDatabase.IsValidFolder("Assets/Materials"))
                    AssetDatabase.CreateFolder("Assets", "Materials");

                string matPath = $"Assets/Materials/{name}.mat";
                Material existing = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if (existing != null)
                {
                    existing.color = color;
                    if (existing.HasProperty("_BaseColor")) existing.SetColor("_BaseColor", color);
                    EditorUtility.SetDirty(existing);
                    return existing;
                }

                AssetDatabase.CreateAsset(mat, matPath);
                return mat;
            }

            // Material Palette for Architectural Shell
            Material matOakFloor = CreateMat("Mat_OakFloor_Shell", new Color(0.82f, 0.74f, 0.65f), 0.35f, 0.0f);
            Material matMarbleTile = CreateMat("Mat_MarbleTile_Shell", new Color(0.92f, 0.92f, 0.91f), 0.75f, 0.1f);
            Material matWallPaint = CreateMat("Mat_WallPaint_Shell", new Color(0.88f, 0.86f, 0.83f), 0.15f, 0.0f);
            Material matDarkWood = CreateMat("Mat_DarkWood_Shell", new Color(0.18f, 0.17f, 0.16f), 0.3f, 0.0f);
            Material matBlackFrame = CreateMat("Mat_BlackFrame_Shell", new Color(0.12f, 0.12f, 0.12f), 0.5f, 0.7f);
            Material matGlass = CreateMat("Mat_Glass_Shell", new Color(0.65f, 0.85f, 0.95f, 0.35f), 0.95f, 0.1f, true);

            GameObject CreatePrim(PrimitiveType type, string name, Vector3 pos, Vector3 scale, Quaternion rot, Material mat, Transform parent)
            {
                GameObject obj = GameObject.CreatePrimitive(type);
                obj.name = name;
                obj.transform.position = pos;
                obj.transform.localScale = scale;
                obj.transform.rotation = rot;
                if (parent != null) obj.transform.SetParent(parent);
                if (mat != null) obj.GetComponent<Renderer>().sharedMaterial = mat;
                return obj;
            }

            // 1. Sun Lighting
            GameObject sun = new GameObject("Sun Light");
            Light sunComp = sun.AddComponent<Light>();
            sunComp.type = LightType.Directional;
            sunComp.color = new Color(1.0f, 0.96f, 0.88f);
            sunComp.intensity = 1.3f;
            sunComp.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(38f, -42f, 0);
            sun.transform.SetParent(shellRoot.transform);

            // 2. Flooring (Light Oak Wood + Bathroom Marble)
            Transform floorGroup = new GameObject("Floors").transform;
            floorGroup.SetParent(shellRoot.transform);

            // Main Oak Wood Floor
            CreatePrim(PrimitiveType.Cube, "Floor_LightOak", new Vector3(0, -0.05f, 0), new Vector3(8.0f, 0.1f, 11.6f), Quaternion.identity, matOakFloor, floorGroup);
            // Bathroom White Marble Tile Floor
            CreatePrim(PrimitiveType.Cube, "Floor_BathroomMarble", new Vector3(-2.1f, -0.04f, -1.0f), new Vector3(3.6f, 0.1f, 3.8f), Quaternion.identity, matMarbleTile, floorGroup);

            // 3. Perimeter & Partition Walls (Ceiling Height = 3.4m)
            Transform wallGroup = new GameObject("Walls").transform;
            wallGroup.SetParent(shellRoot.transform);

            // Back Wall (Entry, Storage & Laundry back boundary)
            CreatePrim(PrimitiveType.Cube, "Wall_Back_Left", new Vector3(-3.5f, 1.7f, -5.85f), new Vector3(1.0f, 3.4f, 0.15f), Quaternion.identity, matWallPaint, wallGroup);
            CreatePrim(PrimitiveType.Cube, "Wall_Back_Right", new Vector3(1.5f, 1.7f, -5.85f), new Vector3(5.0f, 3.4f, 0.15f), Quaternion.identity, matWallPaint, wallGroup);
            CreatePrim(PrimitiveType.Cube, "Wall_Back_DoorHeader", new Vector3(-2.8f, 3.1f, -5.85f), new Vector3(1.1f, 0.6f, 0.15f), Quaternion.identity, matWallPaint, wallGroup);

            // Left Outer Wall
            CreatePrim(PrimitiveType.Cube, "Wall_Left_Outer", new Vector3(-4.05f, 1.7f, 0), new Vector3(0.15f, 3.4f, 11.6f), Quaternion.identity, matWallPaint, wallGroup);

            // Right Outer Glass Window Wall (Floor-to-Ceiling Windows)
            CreatePrim(PrimitiveType.Cube, "WindowWall_FrameBottom", new Vector3(4.05f, 0.05f, 0), new Vector3(0.12f, 0.1f, 11.6f), Quaternion.identity, matBlackFrame, wallGroup);
            CreatePrim(PrimitiveType.Cube, "WindowWall_FrameTop", new Vector3(4.05f, 3.35f, 0), new Vector3(0.12f, 0.1f, 11.6f), Quaternion.identity, matBlackFrame, wallGroup);
            CreatePrim(PrimitiveType.Cube, "WindowWall_Glass", new Vector3(4.05f, 1.7f, 0), new Vector3(0.04f, 3.2f, 11.55f), Quaternion.identity, matGlass, wallGroup);

            // Front Wall Header & Balcony Glass Door Frame
            CreatePrim(PrimitiveType.Cube, "Wall_Front_RightHeader", new Vector3(2.0f, 3.25f, 5.8f), new Vector3(4.0f, 0.3f, 0.1f), Quaternion.identity, matWallPaint, wallGroup);
            CreatePrim(PrimitiveType.Cube, "Wall_Front_RightGlass", new Vector3(2.0f, 1.55f, 5.8f), new Vector3(3.9f, 3.1f, 0.04f), Quaternion.identity, matGlass, wallGroup);

            // Interior Partition Walls
            CreatePrim(PrimitiveType.Cube, "Wall_Bed_Living_Divider", new Vector3(-0.3f, 1.7f, 3.1f), new Vector3(0.15f, 3.4f, 5.4f), Quaternion.identity, matWallPaint, wallGroup);
            CreatePrim(PrimitiveType.Cube, "Wall_Bed_Bath_Divider", new Vector3(-2.1f, 1.7f, 0.9f), new Vector3(3.6f, 3.4f, 0.15f), Quaternion.identity, matWallPaint, wallGroup);
            CreatePrim(PrimitiveType.Cube, "Wall_Bath_Hall_Divider", new Vector3(-0.3f, 1.7f, -1.0f), new Vector3(0.15f, 3.4f, 3.8f), Quaternion.identity, matWallPaint, wallGroup);
            CreatePrim(PrimitiveType.Cube, "Wall_Bath_Kitchen_Divider", new Vector3(-2.1f, 1.7f, -2.9f), new Vector3(3.6f, 3.4f, 0.15f), Quaternion.identity, matWallPaint, wallGroup);

            // Bathroom Marble Wall Lining
            CreatePrim(PrimitiveType.Cube, "Bath_MarbleWall_Back", new Vector3(-2.1f, 1.7f, -2.81f), new Vector3(3.5f, 3.4f, 0.02f), Quaternion.identity, matMarbleTile, wallGroup);
            CreatePrim(PrimitiveType.Cube, "Bath_MarbleWall_Left", new Vector3(-3.96f, 1.7f, -1.0f), new Vector3(0.02f, 3.4f, 3.7f), Quaternion.identity, matMarbleTile, wallGroup);

            // 4. Doors & Frames
            Transform doorGroup = new GameObject("DoorsAndOpenings").transform;
            doorGroup.SetParent(shellRoot.transform);

            // Main Entrance Door (Back Wall)
            CreatePrim(PrimitiveType.Cube, "MainEntrance_DoorFrame", new Vector3(-2.8f, 1.4f, -5.85f), new Vector3(1.15f, 2.85f, 0.16f), Quaternion.identity, matBlackFrame, doorGroup);
            CreatePrim(PrimitiveType.Cube, "MainEntrance_DoorPanel", new Vector3(-2.8f, 1.4f, -5.85f), new Vector3(1.08f, 2.78f, 0.08f), Quaternion.identity, matDarkWood, doorGroup);

            // Bathroom Privacy Door Frame & Panel
            CreatePrim(PrimitiveType.Cube, "Bathroom_DoorFrame", new Vector3(-0.3f, 1.4f, -2.7f), new Vector3(0.17f, 2.85f, 0.95f), Quaternion.identity, matBlackFrame, doorGroup);
            CreatePrim(PrimitiveType.Cube, "Bathroom_DoorPanel", new Vector3(-0.3f, 1.4f, -2.7f), new Vector3(0.06f, 2.78f, 0.88f), Quaternion.identity, matDarkWood, doorGroup);

            // Bedroom Front Balcony Sliding Glass Door
            CreatePrim(PrimitiveType.Cube, "Bedroom_BalconyDoorFrame", new Vector3(-2.1f, 1.6f, 5.8f), new Vector3(3.6f, 3.1f, 0.12f), Quaternion.identity, matBlackFrame, doorGroup);
            CreatePrim(PrimitiveType.Cube, "Bedroom_BalconyGlass", new Vector3(-2.1f, 1.6f, 5.8f), new Vector3(3.4f, 2.95f, 0.04f), Quaternion.identity, matGlass, doorGroup);

            // 5. Skylight / Glass Roof Section above Bedroom Area (Upper Left)
            Transform skylightGroup = new GameObject("BedroomSkylight").transform;
            skylightGroup.SetParent(shellRoot.transform);

            // Ceiling Main Slab (for living/kitchen/bath/entry)
            CreatePrim(PrimitiveType.Cube, "Ceiling_MainSlab", new Vector3(1.8f, 3.45f, 0.0f), new Vector3(4.4f, 0.1f, 11.6f), Quaternion.identity, matWallPaint, skylightGroup);
            CreatePrim(PrimitiveType.Cube, "Ceiling_LowerSlab", new Vector3(-2.1f, 3.45f, -2.5f), new Vector3(3.6f, 0.1f, 6.6f), Quaternion.identity, matWallPaint, skylightGroup);

            // Bedroom Glass Roof Frame & Glass Panels (Above X: -3.8 to -0.4, Z: 1.0 to 5.6, Y: 3.4m)
            CreatePrim(PrimitiveType.Cube, "Skylight_FrameOuter", new Vector3(-2.1f, 3.45f, 3.3f), new Vector3(3.6f, 0.12f, 4.6f), Quaternion.identity, matBlackFrame, skylightGroup);
            CreatePrim(PrimitiveType.Cube, "Skylight_GlassPanel", new Vector3(-2.1f, 3.45f, 3.3f), new Vector3(3.4f, 0.04f, 4.4f), Quaternion.identity, matGlass, skylightGroup);
            // Skylight Grid Mullions
            CreatePrim(PrimitiveType.Cube, "Skylight_MullionV1", new Vector3(-3.0f, 3.46f, 3.3f), new Vector3(0.06f, 0.08f, 4.4f), Quaternion.identity, matBlackFrame, skylightGroup);
            CreatePrim(PrimitiveType.Cube, "Skylight_MullionV2", new Vector3(-1.2f, 3.46f, 3.3f), new Vector3(0.06f, 0.08f, 4.4f), Quaternion.identity, matBlackFrame, skylightGroup);
            CreatePrim(PrimitiveType.Cube, "Skylight_MullionH1", new Vector3(-2.1f, 3.46f, 2.0f), new Vector3(3.4f, 0.08f, 0.06f), Quaternion.identity, matBlackFrame, skylightGroup);
            CreatePrim(PrimitiveType.Cube, "Skylight_MullionH2", new Vector3(-2.1f, 3.46f, 4.6f), new Vector3(3.4f, 0.08f, 0.06f), Quaternion.identity, matBlackFrame, skylightGroup);

            // 6. Cameras Setup
            Transform camsGroup = new GameObject("Cameras").transform;
            camsGroup.SetParent(shellRoot.transform);

            // Top-Down Floor Plan Camera
            GameObject camTop = new GameObject("Camera_TopDown");
            Camera cTop = camTop.AddComponent<Camera>();
            cTop.orthographic = true;
            cTop.orthographicSize = 6.2f;
            camTop.transform.position = new Vector3(0, 12.0f, 0);
            camTop.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            camTop.transform.SetParent(camsGroup);

            // Main Isometric View Camera
            GameObject camMain = new GameObject("Main Camera");
            Camera cMain = camMain.AddComponent<Camera>();
            cMain.clearFlags = CameraClearFlags.Skybox;
            cMain.backgroundColor = new Color(0.8f, 0.85f, 0.9f);
            camMain.tag = "MainCamera";
            camMain.transform.position = new Vector3(7.5f, 7.0f, -8.5f);
            camMain.transform.rotation = Quaternion.Euler(32f, -38f, 0f);
            camMain.transform.SetParent(camsGroup);

            // Save Scene
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");

            string scenePath = "Assets/Scenes/StudioApartmentShellScene.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.Refresh();

            return $"Successfully built 3D Studio Apartment Architectural Shell in Unity! Saved to: {scenePath}";
        }
    }
}
