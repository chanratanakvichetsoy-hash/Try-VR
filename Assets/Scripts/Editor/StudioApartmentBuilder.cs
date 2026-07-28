using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace TryVR.Editor
{
    public static class StudioApartmentBuilder
    {
        [MenuItem("Tools/Build Studio Apartment Scene")]
        public static string Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject aptRoot = new GameObject("StudioApartment");

            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null) litShader = Shader.Find("Standard");

            Material CreateMat(string name, Color color, float smoothness = 0.3f, float metallic = 0.0f, bool isTransparent = false, Color? emissive = null)
            {
                Material mat = new Material(litShader);
                mat.name = name;
                mat.color = color;
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
                if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);

                if (emissive.HasValue)
                {
                    mat.EnableKeyword("_EMISSION");
                    if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", emissive.Value);
                }

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
                    if (emissive.HasValue && existing.HasProperty("_EmissionColor")) existing.SetColor("_EmissionColor", emissive.Value);
                    EditorUtility.SetDirty(existing);
                    return existing;
                }

                AssetDatabase.CreateAsset(mat, matPath);
                return mat;
            }

            // Material Palette
            Material matOakFloor = CreateMat("Mat_OakFloor", new Color(0.82f, 0.74f, 0.65f), 0.35f, 0.0f);
            Material matMarbleFloor = CreateMat("Mat_MarbleTile", new Color(0.92f, 0.92f, 0.91f), 0.75f, 0.1f);
            Material matWallPaint = CreateMat("Mat_WallPaint", new Color(0.88f, 0.86f, 0.83f), 0.15f, 0.0f);
            Material matDarkWood = CreateMat("Mat_DarkWood", new Color(0.18f, 0.17f, 0.16f), 0.3f, 0.0f);
            Material matWhiteCabinet = CreateMat("Mat_WhiteCabinet", new Color(0.94f, 0.94f, 0.93f), 0.4f, 0.0f);
            Material matBlackMetal = CreateMat("Mat_BlackMetal", new Color(0.12f, 0.12f, 0.12f), 0.5f, 0.7f);
            Material matGlass = CreateMat("Mat_GlassPanels", new Color(0.65f, 0.85f, 0.95f, 0.35f), 0.95f, 0.1f, true);
            Material matSmokedGlass = CreateMat("Mat_SmokedGlass", new Color(0.2f, 0.2f, 0.22f, 0.6f), 0.9f, 0.2f, true);
            Material matSofaFabric = CreateMat("Mat_SofaFabric", new Color(0.72f, 0.70f, 0.67f), 0.1f, 0.0f);
            Material matBedLinen = CreateMat("Mat_BedLinen", new Color(0.91f, 0.90f, 0.87f), 0.1f, 0.0f);
            Material matRug = CreateMat("Mat_GrayRug", new Color(0.38f, 0.37f, 0.36f), 0.05f, 0.0f);
            Material matLedWarm = CreateMat("Mat_LedWarmGlow", new Color(1.0f, 0.88f, 0.68f), 0.9f, 0.0f, false, new Color(1.0f, 0.85f, 0.6f) * 2.5f);
            Material matMirror = CreateMat("Mat_MirrorReflect", new Color(0.85f, 0.90f, 0.95f), 0.95f, 0.9f);
            Material matCurtainWhite = CreateMat("Mat_CurtainSheer", new Color(0.95f, 0.95f, 0.93f, 0.7f), 0.1f, 0.0f, true);
            Material matCurtainTaupe = CreateMat("Mat_CurtainTaupe", new Color(0.55f, 0.50f, 0.45f), 0.15f, 0.0f);

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

            // 1. Lighting Setup
            GameObject sun = new GameObject("Sun Light");
            Light sunComp = sun.AddComponent<Light>();
            sunComp.type = LightType.Directional;
            sunComp.color = new Color(1.0f, 0.96f, 0.88f);
            sunComp.intensity = 1.3f;
            sunComp.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(38f, -42f, 0);
            sun.transform.SetParent(aptRoot.transform);

            void CreatePointLight(string name, Vector3 pos, Color col, float intensity, float range, Transform parent)
            {
                GameObject lightObj = new GameObject(name);
                lightObj.transform.position = pos;
                if (parent != null) lightObj.transform.SetParent(parent);
                Light l = lightObj.AddComponent<Light>();
                l.type = LightType.Point;
                l.color = col;
                l.intensity = intensity;
                l.range = range;
            }

            Transform lightsGroup = new GameObject("InteriorLights").transform;
            lightsGroup.SetParent(aptRoot.transform);
            Color warmLightColor = new Color(1.0f, 0.86f, 0.65f);

            CreatePointLight("Light_Bedroom", new Vector3(-2.1f, 3.0f, 3.5f), warmLightColor, 1.8f, 6.0f, lightsGroup);
            CreatePointLight("Light_Living", new Vector3(2.0f, 3.0f, 3.5f), warmLightColor, 2.0f, 7.0f, lightsGroup);
            CreatePointLight("Light_Kitchen", new Vector3(2.0f, 3.0f, -1.5f), warmLightColor, 1.8f, 6.0f, lightsGroup);
            CreatePointLight("Light_Bathroom", new Vector3(-2.1f, 3.0f, -1.0f), warmLightColor, 1.8f, 5.0f, lightsGroup);
            CreatePointLight("Light_Entry", new Vector3(0.0f, 3.0f, -4.5f), warmLightColor, 1.6f, 5.0f, lightsGroup);

            // 2. Shell & Walls
            Transform shellGroup = new GameObject("BuildingShell").transform;
            shellGroup.SetParent(aptRoot.transform);

            // Main Floor (Oak Wood)
            CreatePrim(PrimitiveType.Cube, "Floor_OakWood", new Vector3(0, -0.05f, 0), new Vector3(8.0f, 0.1f, 11.6f), Quaternion.identity, matOakFloor, shellGroup);
            // Bathroom Floor (White Marble Tile)
            CreatePrim(PrimitiveType.Cube, "Floor_BathroomMarble", new Vector3(-2.1f, -0.04f, -1.0f), new Vector3(3.6f, 0.1f, 3.8f), Quaternion.identity, matMarbleFloor, shellGroup);

            // Perimeter Walls (Height = 3.4m)
            CreatePrim(PrimitiveType.Cube, "Wall_Back_Entry", new Vector3(0, 1.7f, -5.85f), new Vector3(8.0f, 3.4f, 0.15f), Quaternion.identity, matWallPaint, shellGroup);
            CreatePrim(PrimitiveType.Cube, "Wall_Left_Outer", new Vector3(-4.05f, 1.7f, 0), new Vector3(0.15f, 3.4f, 11.6f), Quaternion.identity, matWallPaint, shellGroup);
            CreatePrim(PrimitiveType.Cube, "Wall_Right_Outer", new Vector3(4.05f, 1.7f, 0), new Vector3(0.15f, 3.4f, 11.6f), Quaternion.identity, matWallPaint, shellGroup);

            // Front Window Wall (Living & Bedroom Window/Balcony Glass)
            CreatePrim(PrimitiveType.Cube, "WindowWall_FrameTop", new Vector3(0, 3.25f, 5.8f), new Vector3(8.0f, 0.3f, 0.1f), Quaternion.identity, matBlackMetal, shellGroup);
            CreatePrim(PrimitiveType.Cube, "WindowWall_Glass", new Vector3(0, 1.6f, 5.8f), new Vector3(7.9f, 3.0f, 0.05f), Quaternion.identity, matGlass, shellGroup);

            // Interior Partition Walls
            CreatePrim(PrimitiveType.Cube, "Wall_Bed_Living_Divider", new Vector3(-0.3f, 1.7f, 2.8f), new Vector3(0.15f, 3.4f, 6.0f), Quaternion.identity, matWallPaint, shellGroup);
            CreatePrim(PrimitiveType.Cube, "Wall_Bed_Bath_Divider", new Vector3(-2.1f, 1.7f, 0.9f), new Vector3(3.6f, 3.4f, 0.15f), Quaternion.identity, matWallPaint, shellGroup);
            CreatePrim(PrimitiveType.Cube, "Wall_Bath_Hall_Divider", new Vector3(-0.3f, 1.7f, -1.0f), new Vector3(0.15f, 3.4f, 3.8f), Quaternion.identity, matWallPaint, shellGroup);
            CreatePrim(PrimitiveType.Cube, "Wall_Bath_Kitchen_Divider", new Vector3(-2.1f, 1.7f, -2.9f), new Vector3(3.6f, 3.4f, 0.15f), Quaternion.identity, matWallPaint, shellGroup);

            // Bathroom Marble Wall Lining
            CreatePrim(PrimitiveType.Cube, "Bath_MarbleWall_Back", new Vector3(-2.1f, 1.7f, -2.81f), new Vector3(3.5f, 3.4f, 0.02f), Quaternion.identity, matMarbleFloor, shellGroup);
            CreatePrim(PrimitiveType.Cube, "Bath_MarbleWall_Left", new Vector3(-3.96f, 1.7f, -1.0f), new Vector3(0.02f, 3.4f, 3.7f), Quaternion.identity, matMarbleFloor, shellGroup);

            // 3. Bedroom Zone (Upper Left)
            Transform bedGroup = new GameObject("Zone_Bedroom").transform;
            bedGroup.SetParent(aptRoot.transform);

            // Rug
            CreatePrim(PrimitiveType.Cube, "Bed_Rug", new Vector3(-2.1f, 0.01f, 3.8f), new Vector3(3.2f, 0.02f, 3.0f), Quaternion.identity, matRug, bedGroup);

            // Platform Bed
            CreatePrim(PrimitiveType.Cube, "Bed_PlatformBase", new Vector3(-2.2f, 0.2f, 3.8f), new Vector3(2.1f, 0.35f, 2.2f), Quaternion.identity, matDarkWood, bedGroup);
            CreatePrim(PrimitiveType.Cube, "Bed_Mattress", new Vector3(-2.2f, 0.5f, 3.8f), new Vector3(1.9f, 0.3f, 2.0f), Quaternion.identity, matBedLinen, bedGroup);
            CreatePrim(PrimitiveType.Cube, "Bed_Duvet", new Vector3(-2.2f, 0.58f, 3.4f), new Vector3(1.92f, 0.18f, 1.3f), Quaternion.identity, matBedLinen, bedGroup);
            CreatePrim(PrimitiveType.Cube, "Bed_ThrowBlanket", new Vector3(-2.2f, 0.68f, 3.0f), new Vector3(1.94f, 0.04f, 0.5f), Quaternion.identity, matRug, bedGroup);

            // Pillows (4x)
            CreatePrim(PrimitiveType.Cube, "Pillow_1", new Vector3(-2.6f, 0.72f, 4.5f), new Vector3(0.7f, 0.15f, 0.45f), Quaternion.Euler(15, 0, 0), matBedLinen, bedGroup);
            CreatePrim(PrimitiveType.Cube, "Pillow_2", new Vector3(-1.8f, 0.72f, 4.5f), new Vector3(0.7f, 0.15f, 0.45f), Quaternion.Euler(15, 0, 0), matBedLinen, bedGroup);

            // Headboard Accent Board & LED Strip
            CreatePrim(PrimitiveType.Cube, "Bed_Headboard", new Vector3(-2.2f, 1.4f, 4.82f), new Vector3(2.5f, 1.4f, 0.08f), Quaternion.identity, matDarkWood, bedGroup);
            CreatePrim(PrimitiveType.Cube, "Bed_HeadboardLED", new Vector3(-2.2f, 2.12f, 4.81f), new Vector3(2.5f, 0.05f, 0.04f), Quaternion.identity, matLedWarm, bedGroup);

            // Built-in Wardrobe (Left wall)
            CreatePrim(PrimitiveType.Cube, "Bed_WardrobeFrame", new Vector3(-3.5f, 1.7f, 4.4f), new Vector3(0.8f, 3.4f, 1.6f), Quaternion.identity, matDarkWood, bedGroup);
            CreatePrim(PrimitiveType.Cube, "Bed_WardrobeGlassDoor", new Vector3(-3.08f, 1.7f, 4.4f), new Vector3(0.04f, 3.3f, 1.55f), Quaternion.identity, matSmokedGlass, bedGroup);

            // Desk & Chair (Right wall of bedroom)
            CreatePrim(PrimitiveType.Cube, "Bed_DeskTop", new Vector3(-1.0f, 0.75f, 1.8f), new Vector3(1.1f, 0.06f, 0.6f), Quaternion.identity, matDarkWood, bedGroup);
            CreatePrim(PrimitiveType.Cube, "Bed_DeskLegs", new Vector3(-1.0f, 0.37f, 1.8f), new Vector3(1.08f, 0.7f, 0.58f), Quaternion.identity, matDarkWood, bedGroup);
            CreatePrim(PrimitiveType.Cube, "Bed_DeskChairSeat", new Vector3(-1.0f, 0.48f, 1.2f), new Vector3(0.5f, 0.08f, 0.5f), Quaternion.identity, matBlackMetal, bedGroup);
            CreatePrim(PrimitiveType.Cube, "Bed_DeskChairBack", new Vector3(-1.0f, 0.85f, 0.98f), new Vector3(0.48f, 0.45f, 0.06f), Quaternion.identity, matBlackMetal, bedGroup);
            CreatePrim(PrimitiveType.Cube, "Bed_Laptop", new Vector3(-1.0f, 0.8f, 1.8f), new Vector3(0.35f, 0.02f, 0.25f), Quaternion.identity, matBlackMetal, bedGroup);

            // Glass Balcony Sliding Door
            CreatePrim(PrimitiveType.Cube, "Bed_BalconyDoorFrame", new Vector3(-2.1f, 1.6f, 5.75f), new Vector3(3.6f, 3.0f, 0.08f), Quaternion.identity, matBlackMetal, bedGroup);
            CreatePrim(PrimitiveType.Cube, "Bed_BalconyDoorGlass", new Vector3(-2.1f, 1.6f, 5.75f), new Vector3(3.4f, 2.9f, 0.04f), Quaternion.identity, matGlass, bedGroup);

            // 4. Living & Dining Zone (Center & Right)
            Transform livingGroup = new GameObject("Zone_LivingDining").transform;
            livingGroup.SetParent(aptRoot.transform);

            // L-Shaped Sofa
            CreatePrim(PrimitiveType.Cube, "Sofa_MainBase", new Vector3(2.4f, 0.22f, 4.4f), new Vector3(2.8f, 0.4f, 1.0f), Quaternion.identity, matSofaFabric, livingGroup);
            CreatePrim(PrimitiveType.Cube, "Sofa_ChaiseBase", new Vector3(3.3f, 0.22f, 3.3f), new Vector3(1.0f, 0.4f, 1.2f), Quaternion.identity, matSofaFabric, livingGroup);
            CreatePrim(PrimitiveType.Cube, "Sofa_Backrest", new Vector3(2.4f, 0.65f, 4.75f), new Vector3(2.8f, 0.55f, 0.3f), Quaternion.identity, matSofaFabric, livingGroup);
            CreatePrim(PrimitiveType.Cube, "Sofa_SideArm", new Vector3(3.85f, 0.55f, 3.85f), new Vector3(0.25f, 0.45f, 2.1f), Quaternion.identity, matSofaFabric, livingGroup);

            // Round Coffee Table
            CreatePrim(PrimitiveType.Cylinder, "CoffeeTableTop", new Vector3(1.6f, 0.38f, 3.4f), new Vector3(0.8f, 0.04f, 0.8f), Quaternion.identity, matBlackMetal, livingGroup);
            CreatePrim(PrimitiveType.Cylinder, "CoffeeTableBase", new Vector3(1.6f, 0.18f, 3.4f), new Vector3(0.4f, 0.36f, 0.4f), Quaternion.identity, matBlackMetal, livingGroup);

            // Dining Table
            CreatePrim(PrimitiveType.Cube, "DiningTableTop", new Vector3(2.0f, 0.75f, 1.2f), new Vector3(1.6f, 0.06f, 1.0f), Quaternion.identity, matMarbleFloor, livingGroup);
            CreatePrim(PrimitiveType.Cube, "DiningTableBase", new Vector3(2.0f, 0.36f, 1.2f), new Vector3(1.0f, 0.7f, 0.6f), Quaternion.identity, matDarkWood, livingGroup);

            // Dining Chairs (4x)
            void CreateDiningChair(string cName, Vector3 pos, float rotY)
            {
                Transform cRoot = new GameObject(cName).transform;
                cRoot.SetParent(livingGroup);
                cRoot.position = pos;
                cRoot.rotation = Quaternion.Euler(0, rotY, 0);

                CreatePrim(PrimitiveType.Cube, "Seat", pos + new Vector3(0, 0.45f, 0), new Vector3(0.45f, 0.06f, 0.45f), Quaternion.Euler(0, rotY, 0), matBlackMetal, cRoot);
                CreatePrim(PrimitiveType.Cube, "Back", pos + new Vector3(0, 0.75f, -0.2f), new Vector3(0.44f, 0.35f, 0.05f), Quaternion.Euler(0, rotY, 0), matBlackMetal, cRoot);
                CreatePrim(PrimitiveType.Cylinder, "Leg1", pos + new Vector3(-0.18f, 0.22f, -0.18f), new Vector3(0.04f, 0.44f, 0.04f), Quaternion.identity, matBlackMetal, cRoot);
                CreatePrim(PrimitiveType.Cylinder, "Leg2", pos + new Vector3(0.18f, 0.22f, -0.18f), new Vector3(0.04f, 0.44f, 0.04f), Quaternion.identity, matBlackMetal, cRoot);
            }

            CreateDiningChair("Chair_1", new Vector3(1.4f, 0, 0.7f), 0);
            CreateDiningChair("Chair_2", new Vector3(2.6f, 0, 0.7f), 0);
            CreateDiningChair("Chair_3", new Vector3(1.4f, 0, 1.7f), 180);
            CreateDiningChair("Chair_4", new Vector3(2.6f, 0, 1.7f), 180);

            // Floor-to-Ceiling Curtains (Right wall)
            CreatePrim(PrimitiveType.Cube, "Curtain_Sheer", new Vector3(3.92f, 1.7f, 3.0f), new Vector3(0.04f, 3.4f, 5.0f), Quaternion.identity, matCurtainWhite, livingGroup);
            CreatePrim(PrimitiveType.Cube, "Curtain_TaupeLeft", new Vector3(3.88f, 1.7f, 5.2f), new Vector3(0.12f, 3.4f, 0.8f), Quaternion.identity, matCurtainTaupe, livingGroup);
            CreatePrim(PrimitiveType.Cube, "Curtain_TaupeRight", new Vector3(3.88f, 1.7f, 0.8f), new Vector3(0.12f, 3.4f, 0.8f), Quaternion.identity, matCurtainTaupe, livingGroup);

            // Wall Sconce
            CreatePrim(PrimitiveType.Cube, "Living_WallSconce", new Vector3(2.0f, 2.2f, 4.92f), new Vector3(0.6f, 0.12f, 0.06f), Quaternion.identity, matLedWarm, livingGroup);

            // 5. Kitchen Zone (Lower Right)
            Transform kitchenGroup = new GameObject("Zone_Kitchen").transform;
            kitchenGroup.SetParent(aptRoot.transform);

            // Lower Cabinets & Countertop
            CreatePrim(PrimitiveType.Cube, "Kitchen_LowerCab", new Vector3(2.0f, 0.45f, -1.5f), new Vector3(3.6f, 0.9f, 0.8f), Quaternion.identity, matDarkWood, kitchenGroup);
            CreatePrim(PrimitiveType.Cube, "Kitchen_Countertop", new Vector3(2.0f, 0.92f, -1.5f), new Vector3(3.65f, 0.05f, 0.85f), Quaternion.identity, matMarbleFloor, kitchenGroup);

            // Cooktop
            CreatePrim(PrimitiveType.Cube, "Kitchen_Hob", new Vector3(3.0f, 0.95f, -1.5f), new Vector3(0.65f, 0.02f, 0.5f), Quaternion.identity, matBlackMetal, kitchenGroup);

            // Undermount Sink & Faucet
            CreatePrim(PrimitiveType.Cube, "Kitchen_Sink", new Vector3(1.2f, 0.93f, -1.5f), new Vector3(0.6f, 0.02f, 0.45f), Quaternion.identity, matBlackMetal, kitchenGroup);
            CreatePrim(PrimitiveType.Cylinder, "Kitchen_Faucet", new Vector3(1.2f, 1.15f, -1.3f), new Vector3(0.04f, 0.25f, 0.04f), Quaternion.identity, matBlackMetal, kitchenGroup);

            // Upper Open Shelving & Cabinets
            CreatePrim(PrimitiveType.Cube, "Kitchen_UpperShelving", new Vector3(2.0f, 2.2f, -1.5f), new Vector3(3.6f, 0.6f, 0.4f), Quaternion.identity, matDarkWood, kitchenGroup);
            CreatePrim(PrimitiveType.Cube, "Kitchen_UnderCabinetLED", new Vector3(2.0f, 1.88f, -1.5f), new Vector3(3.6f, 0.04f, 0.38f), Quaternion.identity, matLedWarm, kitchenGroup);

            // Integrated Refrigerator (Tall unit on right wall)
            CreatePrim(PrimitiveType.Cube, "Kitchen_Fridge", new Vector3(3.4f, 1.1f, -4.8f), new Vector3(0.9f, 2.2f, 0.8f), Quaternion.identity, matDarkWood, kitchenGroup);

            // 6. Bathroom Zone (Lower Left)
            Transform bathGroup = new GameObject("Zone_Bathroom").transform;
            bathGroup.SetParent(aptRoot.transform);

            // Glass Shower Enclosure
            CreatePrim(PrimitiveType.Cube, "Shower_GlassFront", new Vector3(-2.8f, 1.3f, -0.1f), new Vector3(1.6f, 2.6f, 0.04f), Quaternion.identity, matGlass, bathGroup);
            CreatePrim(PrimitiveType.Cube, "Shower_GlassSide", new Vector3(-2.0f, 1.3f, -0.9f), new Vector3(0.04f, 2.6f, 1.6f), Quaternion.identity, matGlass, bathGroup);
            CreatePrim(PrimitiveType.Cube, "Shower_FrameBlack", new Vector3(-2.8f, 2.6f, -0.1f), new Vector3(1.65f, 0.06f, 0.06f), Quaternion.identity, matBlackMetal, bathGroup);
            CreatePrim(PrimitiveType.Cylinder, "Shower_Head", new Vector3(-3.2f, 2.5f, -0.9f), new Vector3(0.25f, 0.04f, 0.25f), Quaternion.identity, matBlackMetal, bathGroup);
            CreatePrim(PrimitiveType.Cube, "Shower_NicheLED", new Vector3(-3.94f, 1.5f, -0.9f), new Vector3(0.05f, 0.4f, 0.6f), Quaternion.identity, matLedWarm, bathGroup);

            // Toilet
            CreatePrim(PrimitiveType.Cube, "Bath_ToiletBase", new Vector3(-1.6f, 0.25f, -0.5f), new Vector3(0.45f, 0.5f, 0.65f), Quaternion.identity, matWhiteCabinet, bathGroup);
            CreatePrim(PrimitiveType.Cube, "Bath_ToiletTank", new Vector3(-1.6f, 0.65f, -0.2f), new Vector3(0.45f, 0.5f, 0.25f), Quaternion.identity, matWhiteCabinet, bathGroup);

            // Floating Vanity
            CreatePrim(PrimitiveType.Cube, "Bath_VanityCab", new Vector3(-1.6f, 0.55f, -2.2f), new Vector3(0.55f, 0.5f, 1.2f), Quaternion.identity, matDarkWood, bathGroup);
            CreatePrim(PrimitiveType.Cube, "Bath_SinkVessel", new Vector3(-1.6f, 0.85f, -2.2f), new Vector3(0.45f, 0.12f, 0.7f), Quaternion.identity, matWhiteCabinet, bathGroup);

            // Backlit Oval Mirror
            CreatePrim(PrimitiveType.Cube, "Bath_MirrorGlass", new Vector3(-0.4f, 1.8f, -2.2f), new Vector3(0.04f, 1.1f, 0.85f), Quaternion.identity, matMirror, bathGroup);
            CreatePrim(PrimitiveType.Cube, "Bath_MirrorHaloLED", new Vector3(-0.38f, 1.8f, -2.2f), new Vector3(0.03f, 1.16f, 0.91f), Quaternion.identity, matLedWarm, bathGroup);

            // Privacy Door
            CreatePrim(PrimitiveType.Cube, "Bath_DoorFrame", new Vector3(-0.38f, 1.4f, -2.7f), new Vector3(0.12f, 2.8f, 0.95f), Quaternion.identity, matDarkWood, bathGroup);

            // 7. Entry, Storage & Laundry Zone (Back Wall)
            Transform entryGroup = new GameObject("Zone_EntryLaundry").transform;
            entryGroup.SetParent(aptRoot.transform);

            // Main Entrance Door
            CreatePrim(PrimitiveType.Cube, "Entry_MainDoor", new Vector3(-2.8f, 1.4f, -5.78f), new Vector3(1.1f, 2.8f, 0.08f), Quaternion.identity, matDarkWood, entryGroup);
            CreatePrim(PrimitiveType.Cube, "Entry_DoorHandle", new Vector3(-2.35f, 1.4f, -5.72f), new Vector3(0.06f, 0.25f, 0.08f), Quaternion.identity, matBlackMetal, entryGroup);

            // Entry Vanity / Console Table
            CreatePrim(PrimitiveType.Cube, "Entry_ConsoleTable", new Vector3(-1.4f, 0.45f, -5.4f), new Vector3(1.0f, 0.8f, 0.4f), Quaternion.identity, matDarkWood, entryGroup);
            CreatePrim(PrimitiveType.Cylinder, "Entry_Stool", new Vector3(-0.7f, 0.22f, -5.4f), new Vector3(0.35f, 0.42f, 0.35f), Quaternion.identity, matSofaFabric, entryGroup);
            CreatePrim(PrimitiveType.Cylinder, "Entry_MirrorRound", new Vector3(-1.4f, 1.6f, -5.76f), new Vector3(0.6f, 0.02f, 0.6f), Quaternion.Euler(90, 0, 0), matMirror, entryGroup);
            CreatePrim(PrimitiveType.Cube, "Entry_WallSconce", new Vector3(-1.4f, 2.2f, -5.75f), new Vector3(0.08f, 0.4f, 0.08f), Quaternion.identity, matLedWarm, entryGroup);

            // Tall Storage Wardrobe Closets
            CreatePrim(PrimitiveType.Cube, "Entry_TallCloset", new Vector3(0.4f, 1.7f, -5.4f), new Vector3(1.5f, 3.4f, 0.65f), Quaternion.identity, matDarkWood, entryGroup);

            // Laundry Alcove
            CreatePrim(PrimitiveType.Cube, "Laundry_WasherMachine", new Vector3(1.8f, 0.45f, -5.4f), new Vector3(0.7f, 0.9f, 0.65f), Quaternion.identity, matWhiteCabinet, entryGroup);
            CreatePrim(PrimitiveType.Cylinder, "Laundry_WasherPorthole", new Vector3(1.8f, 0.45f, -5.06f), new Vector3(0.45f, 0.04f, 0.45f), Quaternion.Euler(90, 0, 0), matBlackMetal, entryGroup);
            CreatePrim(PrimitiveType.Cube, "Laundry_HangingRod", new Vector3(1.8f, 2.1f, -5.4f), new Vector3(0.7f, 0.04f, 0.04f), Quaternion.identity, matBlackMetal, entryGroup);
            CreatePrim(PrimitiveType.Cube, "Laundry_UpperShelf", new Vector3(1.8f, 2.7f, -5.4f), new Vector3(0.75f, 0.05f, 0.65f), Quaternion.identity, matDarkWood, entryGroup);

            // 8. Cameras Setup (Multiple Viewpoints matching references)
            Transform camsGroup = new GameObject("Cameras").transform;
            camsGroup.SetParent(aptRoot.transform);

            // Top-Down Floor Plan Camera
            GameObject camTop = new GameObject("Camera_TopDown");
            Camera cTop = camTop.AddComponent<Camera>();
            cTop.orthographic = true;
            cTop.orthographicSize = 6.2f;
            camTop.transform.position = new Vector3(0, 12.0f, 0);
            camTop.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            camTop.transform.SetParent(camsGroup);

            // Main Isometric Overview Camera
            GameObject camMain = new GameObject("Main Camera");
            Camera cMain = camMain.AddComponent<Camera>();
            cMain.clearFlags = CameraClearFlags.Skybox;
            cMain.backgroundColor = new Color(0.8f, 0.85f, 0.9f);
            camMain.tag = "MainCamera";
            camMain.transform.position = new Vector3(7.5f, 7.0f, -8.5f);
            camMain.transform.rotation = Quaternion.Euler(32f, -38f, 0f);
            camMain.transform.SetParent(camsGroup);

            // Front Wall Camera (Living Room & Kitchen)
            GameObject camFront = new GameObject("Camera_FrontWall");
            Camera cFront = camFront.AddComponent<Camera>();
            camFront.transform.position = new Vector3(2.0f, 1.7f, -4.5f);
            camFront.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            camFront.transform.SetParent(camsGroup);

            // Left Wall Camera (Bedroom)
            GameObject camLeft = new GameObject("Camera_LeftWall");
            Camera cLeft = camLeft.AddComponent<Camera>();
            camLeft.transform.position = new Vector3(1.2f, 1.7f, 3.8f);
            camLeft.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
            camLeft.transform.SetParent(camsGroup);

            // Right Wall Camera (Bathroom)
            GameObject camRight = new GameObject("Camera_RightWall");
            Camera cRight = camRight.AddComponent<Camera>();
            camRight.transform.position = new Vector3(1.2f, 1.7f, -1.0f);
            camRight.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
            camRight.transform.SetParent(camsGroup);

            // Back Wall Camera (Entry, Storage & Laundry)
            GameObject camBack = new GameObject("Camera_BackWall");
            Camera cBack = camBack.AddComponent<Camera>();
            camBack.transform.position = new Vector3(0.0f, 1.7f, -1.0f);
            camBack.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            camBack.transform.SetParent(camsGroup);

            // Save Scene
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");

            string scenePath = "Assets/Scenes/StudioApartmentScene.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.Refresh();

            return $"Successfully built 3D Studio Apartment Scene in Unity! Saved to: {scenePath}";
        }
    }
}
