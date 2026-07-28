using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace TryVR.Editor
{
    public static class HouseBuilder
    {
        [MenuItem("Tools/Build 3D House Scene")]
        public static string Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            
            GameObject houseRoot = new GameObject("HouseScene");

            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null) litShader = Shader.Find("Standard");

            Material CreateMat(string name, Color color, float smoothness = 0.2f, float metallic = 0.0f, bool isTransparent = false)
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

            Material matGrass = CreateMat("Mat_Grass", new Color(0.28f, 0.52f, 0.24f), 0.1f, 0.0f);
            Material matPath = CreateMat("Mat_Pathway", new Color(0.55f, 0.53f, 0.50f), 0.3f, 0.0f);
            Material matMulch = CreateMat("Mat_Mulch", new Color(0.28f, 0.20f, 0.14f), 0.1f, 0.0f);
            Material matFoundation = CreateMat("Mat_Foundation", new Color(0.25f, 0.26f, 0.28f), 0.2f, 0.0f);
            Material matWallWhite = CreateMat("Mat_WallWhite", new Color(0.92f, 0.90f, 0.86f), 0.2f, 0.0f);
            Material matWallAccent = CreateMat("Mat_WallAccent", new Color(0.22f, 0.28f, 0.35f), 0.3f, 0.0f);
            Material matRoof = CreateMat("Mat_Roof", new Color(0.18f, 0.19f, 0.21f), 0.4f, 0.0f);
            Material matWood = CreateMat("Mat_Wood", new Color(0.48f, 0.30f, 0.16f), 0.4f, 0.0f);
            Material matTrim = CreateMat("Mat_WhiteTrim", new Color(0.96f, 0.96f, 0.96f), 0.5f, 0.0f);
            Material matGlass = CreateMat("Mat_Glass", new Color(0.60f, 0.82f, 0.92f, 0.45f), 0.9f, 0.1f, true);
            Material matChimney = CreateMat("Mat_ChimneyBrick", new Color(0.52f, 0.26f, 0.20f), 0.2f, 0.0f);
            Material matFoliage = CreateMat("Mat_Foliage", new Color(0.18f, 0.38f, 0.18f), 0.1f, 0.0f);
            Material matTrunk = CreateMat("Mat_Trunk", new Color(0.35f, 0.22f, 0.12f), 0.1f, 0.0f);
            Material matLamp = CreateMat("Mat_LampGlow", new Color(1.0f, 0.85f, 0.4f), 0.8f, 0.0f);

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

            GameObject sun = new GameObject("Directional Light");
            Light lightComp = sun.AddComponent<Light>();
            lightComp.type = LightType.Directional;
            lightComp.color = new Color(1.0f, 0.95f, 0.85f);
            lightComp.intensity = 1.4f;
            lightComp.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(45, -35, 0);
            sun.transform.SetParent(houseRoot.transform);

            GameObject mainCam = new GameObject("Main Camera");
            Camera cam = mainCam.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.backgroundColor = new Color(0.6f, 0.75f, 0.9f);
            mainCam.tag = "MainCamera";
            mainCam.transform.position = new Vector3(16f, 10f, -18f);
            mainCam.transform.rotation = Quaternion.Euler(22f, -40f, 0f);
            mainCam.transform.SetParent(houseRoot.transform);

            Transform envGroup = new GameObject("Environment").transform;
            envGroup.SetParent(houseRoot.transform);

            CreatePrim(PrimitiveType.Plane, "Lawn", new Vector3(0, -0.01f, 0), new Vector3(3.5f, 1f, 3.5f), Quaternion.identity, matGrass, envGroup);
            CreatePrim(PrimitiveType.Cube, "Pathway", new Vector3(0, 0.01f, -9f), new Vector3(2.2f, 0.04f, 8f), Quaternion.identity, matPath, envGroup);
            CreatePrim(PrimitiveType.Cube, "MulchBed", new Vector3(0, 0.01f, -4.5f), new Vector3(12f, 0.05f, 2f), Quaternion.identity, matMulch, envGroup);

            Transform bldgGroup = new GameObject("Building").transform;
            bldgGroup.SetParent(houseRoot.transform);

            CreatePrim(PrimitiveType.Cube, "Foundation", new Vector3(0, 0.3f, 0), new Vector3(10.2f, 0.6f, 8.2f), Quaternion.identity, matFoundation, bldgGroup);
            CreatePrim(PrimitiveType.Cube, "GroundFloor", new Vector3(0, 2.1f, 0), new Vector3(10f, 3f, 8f), Quaternion.identity, matWallWhite, bldgGroup);
            CreatePrim(PrimitiveType.Cube, "SecondFloor", new Vector3(0, 4.8f, 0), new Vector3(9.4f, 2.4f, 7.4f), Quaternion.identity, matWallAccent, bldgGroup);
            CreatePrim(PrimitiveType.Cube, "MidTrim", new Vector3(0, 3.6f, 0), new Vector3(10.4f, 0.2f, 8.4f), Quaternion.identity, matTrim, bldgGroup);

            Transform roofGroup = new GameObject("RoofAssembly").transform;
            roofGroup.SetParent(bldgGroup);

            CreatePrim(PrimitiveType.Cube, "RoofSlant_Left", new Vector3(-2.6f, 6.7f, 0), new Vector3(6.2f, 0.3f, 9.2f), Quaternion.Euler(0, 0, 32), matRoof, roofGroup);
            CreatePrim(PrimitiveType.Cube, "RoofSlant_Right", new Vector3(2.6f, 6.7f, 0), new Vector3(6.2f, 0.3f, 9.2f), Quaternion.Euler(0, 0, -32), matRoof, roofGroup);
            CreatePrim(PrimitiveType.Cube, "RoofRidgeCap", new Vector3(0, 8.25f, 0), new Vector3(0.5f, 0.3f, 9.3f), Quaternion.identity, matRoof, roofGroup);

            CreatePrim(PrimitiveType.Cube, "GableFill_Front", new Vector3(0, 6.4f, 3.7f), new Vector3(7.5f, 1.6f, 0.1f), Quaternion.identity, matWallAccent, roofGroup);
            CreatePrim(PrimitiveType.Cube, "GableFill_Back", new Vector3(0, 6.4f, -3.7f), new Vector3(7.5f, 1.6f, 0.1f), Quaternion.identity, matWallAccent, roofGroup);

            CreatePrim(PrimitiveType.Cube, "Fascia_FrontLeft", new Vector3(-2.6f, 6.6f, 4.55f), new Vector3(6.25f, 0.35f, 0.15f), Quaternion.Euler(0, 0, 32), matTrim, roofGroup);
            CreatePrim(PrimitiveType.Cube, "Fascia_FrontRight", new Vector3(2.6f, 6.6f, 4.55f), new Vector3(6.25f, 0.35f, 0.15f), Quaternion.Euler(0, 0, -32), matTrim, roofGroup);
            CreatePrim(PrimitiveType.Cube, "Fascia_BackLeft", new Vector3(-2.6f, 6.6f, -4.55f), new Vector3(6.25f, 0.35f, 0.15f), Quaternion.Euler(0, 0, 32), matTrim, roofGroup);
            CreatePrim(PrimitiveType.Cube, "Fascia_BackRight", new Vector3(2.6f, 6.6f, -4.55f), new Vector3(6.25f, 0.35f, 0.15f), Quaternion.Euler(0, 0, -32), matTrim, roofGroup);

            Transform porchGroup = new GameObject("FrontPorch").transform;
            porchGroup.SetParent(bldgGroup);

            CreatePrim(PrimitiveType.Cube, "PorchDeck", new Vector3(0, 0.5f, -4.8f), new Vector3(5f, 0.2f, 2f), Quaternion.identity, matWood, porchGroup);
            CreatePrim(PrimitiveType.Cube, "PorchStep", new Vector3(0, 0.25f, -5.9f), new Vector3(3f, 0.2f, 0.6f), Quaternion.identity, matWood, porchGroup);

            CreatePrim(PrimitiveType.Cylinder, "Pillar_Left", new Vector3(-2.2f, 2.1f, -5.6f), new Vector3(0.25f, 1.4f, 0.25f), Quaternion.identity, matTrim, porchGroup);
            CreatePrim(PrimitiveType.Cylinder, "Pillar_Right", new Vector3(2.2f, 2.1f, -5.6f), new Vector3(0.25f, 1.4f, 0.25f), Quaternion.identity, matTrim, porchGroup);

            CreatePrim(PrimitiveType.Cube, "PorchRoof", new Vector3(0, 3.6f, -4.9f), new Vector3(5.4f, 0.25f, 2.2f), Quaternion.identity, matRoof, porchGroup);
            CreatePrim(PrimitiveType.Cube, "PorchRoofTrim", new Vector3(0, 3.55f, -4.9f), new Vector3(5.5f, 0.15f, 2.3f), Quaternion.identity, matTrim, porchGroup);

            Transform doorGroup = new GameObject("FrontDoor").transform;
            doorGroup.SetParent(porchGroup);

            CreatePrim(PrimitiveType.Cube, "DoorFrame", new Vector3(0, 1.9f, -4.01f), new Vector3(1.6f, 2.5f, 0.1f), Quaternion.identity, matTrim, doorGroup);
            CreatePrim(PrimitiveType.Cube, "DoorPanel", new Vector3(0, 1.85f, -4.03f), new Vector3(1.3f, 2.3f, 0.08f), Quaternion.identity, matWood, doorGroup);
            CreatePrim(PrimitiveType.Sphere, "DoorHandle", new Vector3(0.5f, 1.85f, -4.1f), new Vector3(0.12f, 0.12f, 0.12f), Quaternion.identity, matTrim, doorGroup);

            CreatePrim(PrimitiveType.Cube, "Sconce_Left", new Vector3(-1.2f, 2.4f, -4.05f), new Vector3(0.15f, 0.3f, 0.15f), Quaternion.identity, matLamp, porchGroup);
            CreatePrim(PrimitiveType.Cube, "Sconce_Right", new Vector3(1.2f, 2.4f, -4.05f), new Vector3(0.15f, 0.3f, 0.15f), Quaternion.identity, matLamp, porchGroup);

            Transform windowGroup = new GameObject("Windows").transform;
            windowGroup.SetParent(bldgGroup);

            void CreateWindow(string winName, Vector3 pos, Vector3 size)
            {
                Transform winRoot = new GameObject(winName).transform;
                winRoot.SetParent(windowGroup);
                winRoot.position = pos;

                CreatePrim(PrimitiveType.Cube, "Frame", pos, size + new Vector3(0.2f, 0.2f, 0.02f), Quaternion.identity, matTrim, winRoot);
                CreatePrim(PrimitiveType.Cube, "Glass", pos + new Vector3(0,0,-0.02f), size, Quaternion.identity, matGlass, winRoot);
                CreatePrim(PrimitiveType.Cube, "Sill", pos + new Vector3(0, -size.y*0.5f - 0.05f, -0.06f), new Vector3(size.x + 0.3f, 0.12f, 0.2f), Quaternion.identity, matTrim, winRoot);
                CreatePrim(PrimitiveType.Cube, "MullionV", pos + new Vector3(0,0,-0.03f), new Vector3(0.06f, size.y, 0.04f), Quaternion.identity, matTrim, winRoot);
                CreatePrim(PrimitiveType.Cube, "MullionH", pos + new Vector3(0,0,-0.03f), new Vector3(size.x, 0.06f, 0.04f), Quaternion.identity, matTrim, winRoot);
            }

            CreateWindow("Win_GF_Left", new Vector3(-3.2f, 2.1f, -4.01f), new Vector3(1.6f, 1.8f, 0.06f));
            CreateWindow("Win_GF_Right", new Vector3(3.2f, 2.1f, -4.01f), new Vector3(1.6f, 1.8f, 0.06f));
            CreateWindow("Win_FF_Left", new Vector3(-2.8f, 4.8f, -3.71f), new Vector3(1.4f, 1.5f, 0.06f));
            CreateWindow("Win_FF_Center", new Vector3(0f, 4.8f, -3.71f), new Vector3(1.4f, 1.5f, 0.06f));
            CreateWindow("Win_FF_Right", new Vector3(2.8f, 4.8f, -3.71f), new Vector3(1.4f, 1.5f, 0.06f));

            Transform chimneyGroup = new GameObject("Chimney").transform;
            chimneyGroup.SetParent(bldgGroup);
            CreatePrim(PrimitiveType.Cube, "ChimneyStack", new Vector3(3.3f, 6.2f, 1.2f), new Vector3(1.2f, 4.5f, 1.2f), Quaternion.identity, matChimney, chimneyGroup);
            CreatePrim(PrimitiveType.Cube, "ChimneyCap", new Vector3(3.3f, 8.55f, 1.2f), new Vector3(1.4f, 0.2f, 1.4f), Quaternion.identity, matFoundation, chimneyGroup);

            Transform garageGroup = new GameObject("GarageWing").transform;
            garageGroup.SetParent(bldgGroup);
            CreatePrim(PrimitiveType.Cube, "GarageBody", new Vector3(7.2f, 1.8f, 0.5f), new Vector3(5.5f, 2.8f, 6.5f), Quaternion.identity, matWallWhite, garageGroup);
            CreatePrim(PrimitiveType.Cube, "GarageRoofL", new Vector3(5.8f, 3.7f, 0.5f), new Vector3(3.5f, 0.25f, 7f), Quaternion.Euler(0, 0, 25), matRoof, garageGroup);
            CreatePrim(PrimitiveType.Cube, "GarageRoofR", new Vector3(8.6f, 3.7f, 0.5f), new Vector3(3.5f, 0.25f, 7f), Quaternion.Euler(0, 0, -25), matRoof, garageGroup);
            CreatePrim(PrimitiveType.Cube, "GarageDoorFrame", new Vector3(7.2f, 1.5f, -2.76f), new Vector3(4.2f, 2.2f, 0.08f), Quaternion.identity, matTrim, garageGroup);
            CreatePrim(PrimitiveType.Cube, "GarageDoorPanels", new Vector3(7.2f, 1.45f, -2.78f), new Vector3(3.9f, 2.0f, 0.06f), Quaternion.identity, matWallWhite, garageGroup);

            Transform treesGroup = new GameObject("TreesAndFoliage").transform;
            treesGroup.SetParent(envGroup);

            void CreatePineTree(string treeName, Vector3 pos)
            {
                Transform treeRoot = new GameObject(treeName).transform;
                treeRoot.SetParent(treesGroup);
                treeRoot.position = pos;

                CreatePrim(PrimitiveType.Cylinder, "Trunk", pos + new Vector3(0, 1.2f, 0), new Vector3(0.5f, 1.2f, 0.5f), Quaternion.identity, matTrunk, treeRoot);
                CreatePrim(PrimitiveType.Cylinder, "FoliageLower", pos + new Vector3(0, 2.8f, 0), new Vector3(2.5f, 1.2f, 2.5f), Quaternion.identity, matFoliage, treeRoot);
                CreatePrim(PrimitiveType.Cylinder, "FoliageMid", pos + new Vector3(0, 3.8f, 0), new Vector3(2.0f, 1.1f, 2.0f), Quaternion.identity, matFoliage, treeRoot);
                CreatePrim(PrimitiveType.Cylinder, "FoliageTop", pos + new Vector3(0, 4.7f, 0), new Vector3(1.3f, 1.0f, 1.3f), Quaternion.identity, matFoliage, treeRoot);
            }

            CreatePineTree("Tree_FrontLeft", new Vector3(-9.5f, 0, -6f));
            CreatePineTree("Tree_FrontFarLeft", new Vector3(-12.5f, 0, -3f));
            CreatePineTree("Tree_BackRight", new Vector3(11.5f, 0, 8f));
            CreatePineTree("Tree_BackLeft", new Vector3(-8f, 0, 9f));

            void CreateBush(string bName, Vector3 pos, Vector3 scale)
            {
                CreatePrim(PrimitiveType.Sphere, bName, pos, scale, Quaternion.identity, matFoliage, envGroup);
            }
            CreateBush("Bush_1", new Vector3(-4.2f, 0.4f, -4.5f), new Vector3(0.9f, 0.8f, 0.9f));
            CreateBush("Bush_2", new Vector3(-3.2f, 0.35f, -4.5f), new Vector3(0.75f, 0.7f, 0.75f));
            CreateBush("Bush_3", new Vector3(3.2f, 0.35f, -4.5f), new Vector3(0.75f, 0.7f, 0.75f));
            CreateBush("Bush_4", new Vector3(4.2f, 0.4f, -4.5f), new Vector3(0.9f, 0.8f, 0.9f));

            Transform fenceGroup = new GameObject("FrontFence").transform;
            fenceGroup.SetParent(envGroup);

            float startX = -14f;
            float endX = -2.5f;
            for (float x = startX; x <= endX; x += 0.8f)
            {
                CreatePrim(PrimitiveType.Cube, "Picket", new Vector3(x, 0.6f, -12f), new Vector3(0.12f, 1.2f, 0.05f), Quaternion.identity, matTrim, fenceGroup);
            }
            CreatePrim(PrimitiveType.Cube, "FenceRailTop", new Vector3((startX+endX)*0.5f, 0.9f, -12f), new Vector3(endX - startX, 0.08f, 0.06f), Quaternion.identity, matTrim, fenceGroup);
            CreatePrim(PrimitiveType.Cube, "FenceRailBottom", new Vector3((startX+endX)*0.5f, 0.35f, -12f), new Vector3(endX - startX, 0.08f, 0.06f), Quaternion.identity, matTrim, fenceGroup);

            float rStartX = 2.5f;
            float rEndX = 14f;
            for (float x = rStartX; x <= rEndX; x += 0.8f)
            {
                CreatePrim(PrimitiveType.Cube, "Picket", new Vector3(x, 0.6f, -12f), new Vector3(0.12f, 1.2f, 0.05f), Quaternion.identity, matTrim, fenceGroup);
            }
            CreatePrim(PrimitiveType.Cube, "FenceRailTopR", new Vector3((rStartX+rEndX)*0.5f, 0.9f, -12f), new Vector3(rEndX - rStartX, 0.08f, 0.06f), Quaternion.identity, matTrim, fenceGroup);
            CreatePrim(PrimitiveType.Cube, "FenceRailBottomR", new Vector3((rStartX+rEndX)*0.5f, 0.35f, -12f), new Vector3(rEndX - rStartX, 0.08f, 0.06f), Quaternion.identity, matTrim, fenceGroup);

            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");
            
            string scenePath = "Assets/Scenes/HouseScene.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.Refresh();

            return $"Successfully built 3D House Scene in Unity! Saved to: {scenePath}";
        }
    }
}
