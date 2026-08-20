using System;
using System.IO;
using StopDefence.Vfx;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace StopDefence.Editor
{
    public static class SkillVfxAssetGenerator
    {
        private const string RootPath = "Assets/Particle/SkillVFX";
        private const string TexturePath = RootPath + "/Textures";
        private const string MaterialPath = RootPath + "/Materials";
        private const string PrefabPath = "Assets/Prefabs/SkillVFX";
        private const string ShaderPath = RootPath + "/SkillParticle.shader";
        private const string PreviewScenePath = "Assets/Scenes/SkillVFXPreview.unity";

        private const string FireballPrefabPath = PrefabPath + "/Skill_001_FireballExplosion.prefab";
        private const string EarthPrefabPath = PrefabPath + "/Skill_002_EarthMagic.prefab";
        private const string NailDrivingPrefabPath = PrefabPath + "/Skill_004_NailDriving.prefab";
        private const string PlagueMagicPrefabPath = PrefabPath + "/Skill_006_PlagueMagic.prefab";
        private const string IceLancePrefabPath = PrefabPath + "/Skill_007_IceLance.prefab";

        [MenuItem("Tools/Skill VFX/Generate Skill Effects")]
        public static void GenerateAll()
        {
            EnsureDirectories();
            CreateNailDrivingTextures();
            CreateFireballPixelTextures();
            CreatePlaguePixelTextures();
            CreateIceLanceTextures();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            Texture2D nailTexture = LoadTexture("Nail.png");
            Texture2D pixelGlowTexture = LoadTexture("PixelGlow.png");
            Texture2D pixelRingTexture = LoadTexture("PixelRing.png");
            Texture2D pixelShardTexture = LoadTexture("PixelShard.png");
            Texture2D pixelSmokeTexture = LoadTexture("PixelSmoke.png");
            Texture2D pixelMeteorTexture = LoadTexture("PixelMeteor.png");
            Texture2D pixelFlameTexture = LoadTexture("PixelFlame.png");
            Texture2D skullTexture = LoadTexture("Skull.png");
            Texture2D plaguePoolTexture = LoadTexture("PlaguePool.png");

            Material nail = CreateMaterial("Nail_Alpha.mat", nailTexture, BlendMode.OneMinusSrcAlpha);
            Material pixelGlow = CreateMaterial("PixelGlow_Additive.mat", pixelGlowTexture, BlendMode.One);
            Material pixelRing = CreateMaterial("PixelRing_Additive.mat", pixelRingTexture, BlendMode.One);
            Material pixelShard =
                CreateMaterial("PixelShard_Alpha.mat", pixelShardTexture, BlendMode.OneMinusSrcAlpha);
            Material pixelSmoke =
                CreateMaterial("PixelSmoke_Alpha.mat", pixelSmokeTexture, BlendMode.OneMinusSrcAlpha);
            Material pixelMeteor =
                CreateMaterial("PixelMeteor_Alpha.mat", pixelMeteorTexture, BlendMode.OneMinusSrcAlpha);
            Material pixelFlame =
                CreateMaterial("PixelFlame_Additive.mat", pixelFlameTexture, BlendMode.One);
            Material skull = CreateMaterial("Skull_Additive.mat", skullTexture, BlendMode.One);
            Material plaguePool =
                CreateMaterial("PlaguePool_Additive.mat", plaguePoolTexture, BlendMode.One);

            GameObject fireballPrefab = CreateFireballPrefab(
                pixelMeteor,
                pixelFlame,
                pixelGlow,
                pixelRing,
                pixelShard,
                pixelSmoke);
            GameObject earthPrefab =
                CreateEarthPrefab(pixelGlow, pixelRing, pixelShard, pixelSmoke);
            GameObject nailDrivingPrefab =
                CreateNailDrivingPrefab(pixelGlow, pixelRing, pixelShard, pixelSmoke, nail);
            GameObject plagueMagicPrefab =
                CreatePlagueMagicPrefab(pixelGlow, pixelRing, pixelSmoke, skull, plaguePool);
            GameObject iceLancePrefab = CreateIceLanceAssets();
            if (File.Exists(PreviewScenePath))
            {
                UpdatePreviewScene(nailDrivingPrefab, plagueMagicPrefab, iceLancePrefab);
            }
            else
            {
                CreatePreviewScene(
                    fireballPrefab,
                    earthPrefab,
                    nailDrivingPrefab,
                    plagueMagicPrefab,
                    iceLancePrefab);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log(
                $"[Skill VFX] Generated {FireballPrefabPath}, {EarthPrefabPath}, " +
                $"{NailDrivingPrefabPath}, {PlagueMagicPrefabPath}, {IceLancePrefabPath}, " +
                $"and {PreviewScenePath}.");
        }

        [MenuItem("Tools/Skill VFX/Generate Ice Lance Effect")]
        public static void GenerateIceLance()
        {
            EnsureDirectories();
            if (LoadTexture("PixelGlow.png") == null ||
                LoadTexture("PixelShard.png") == null ||
                LoadTexture("PixelSmoke.png") == null)
            {
                CreateNailDrivingTextures();
            }

            CreateIceLanceTextures();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            GameObject iceLancePrefab = CreateIceLanceAssets();
            if (File.Exists(PreviewScenePath))
            {
                GameObject nailDrivingPrefab =
                    AssetDatabase.LoadAssetAtPath<GameObject>(NailDrivingPrefabPath);
                GameObject plagueMagicPrefab =
                    AssetDatabase.LoadAssetAtPath<GameObject>(PlagueMagicPrefabPath);
                UpdatePreviewScene(nailDrivingPrefab, plagueMagicPrefab, iceLancePrefab);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Skill VFX] Generated {IceLancePrefabPath} and updated the preview scene.");
        }

        [MenuItem("Tools/Skill VFX/Open Preview Scene")]
        private static void OpenPreviewScene()
        {
            if (!File.Exists(PreviewScenePath))
            {
                GenerateAll();
            }

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(PreviewScenePath);
            }
        }

        private static GameObject CreateIceLanceAssets()
        {
            Material lanceAlpha = CreateMaterial(
                "IceLance_Alpha.mat",
                LoadTexture("IceLance.png"),
                BlendMode.OneMinusSrcAlpha);
            Material lanceAdditive = CreateMaterial(
                "IceLance_Additive.mat",
                LoadTexture("IceLance.png"),
                BlendMode.One);
            Material crownBack = CreateMaterial(
                "IceCrownBack_Alpha.mat",
                LoadTexture("IceCrownBack.png"),
                BlendMode.OneMinusSrcAlpha);
            Material crownFront = CreateMaterial(
                "IceCrownFront_Alpha.mat",
                LoadTexture("IceCrownFront.png"),
                BlendMode.OneMinusSrcAlpha);
            Material frostPatch = CreateMaterial(
                "FrostPatch_Alpha.mat",
                LoadTexture("FrostPatch.png"),
                BlendMode.OneMinusSrcAlpha);
            Material snowflake = CreateMaterial(
                "Snowflake_Additive.mat",
                LoadTexture("Snowflake.png"),
                BlendMode.One);
            Material pixelGlow = CreateMaterial(
                "PixelGlow_Additive.mat",
                LoadTexture("PixelGlow.png"),
                BlendMode.One);
            Material pixelShard = CreateMaterial(
                "PixelShard_Alpha.mat",
                LoadTexture("PixelShard.png"),
                BlendMode.OneMinusSrcAlpha);
            Material pixelSmoke = CreateMaterial(
                "PixelSmoke_Alpha.mat",
                LoadTexture("PixelSmoke.png"),
                BlendMode.OneMinusSrcAlpha);

            return CreateIceLancePrefab(
                lanceAlpha,
                lanceAdditive,
                crownBack,
                crownFront,
                frostPatch,
                snowflake,
                pixelGlow,
                pixelShard,
                pixelSmoke);
        }

        private static GameObject CreateFireballPrefab(
            Material meteorMaterial,
            Material flameMaterial,
            Material glow,
            Material ring,
            Material shard,
            Material smoke)
        {
            GameObject root = CreateEffectRoot("Skill_001_FireballExplosion", "skill_001");

            ParticleSystem meteor = CreateSystem(root.transform, "MeteorCore", meteorMaterial, 10);
            ConfigureBase(meteor, 0.7f, 0.6f, 8f, 0.8f, 4, Color.white);
            ParticleSystem.MainModule meteorMain = meteor.main;
            meteorMain.startLifetime = new ParticleSystem.MinMaxCurve(0.56f, 0.62f);
            meteorMain.startSpeed = new ParticleSystem.MinMaxCurve(7.4f, 8.2f);
            SetStartSize3D(meteor, 1.15f, 2.9f);
            meteorMain.startColor = new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.95f, 0.55f),
                new Color(1f, 0.28f, 0.02f));
            ConfigureCone(meteor, new Vector3(90f, 0f, 0f), 1.5f, 0.02f, new Vector3(0f, 4.4f, 0f));
            SetBursts(meteor, new ParticleSystem.Burst(0f, 1));
            SetColorOverLifetime(
                meteor,
                new Color(1f, 1f, 0.8f),
                new Color(1f, 0.18f, 0.01f),
                1f,
                0.15f);
            ParticleSystemRenderer meteorRenderer = meteor.GetComponent<ParticleSystemRenderer>();
            meteorRenderer.renderMode = ParticleSystemRenderMode.Billboard;

            ParticleSystem fragments = CreateSystem(root.transform, "MeteorFragments", shard, 9);
            ConfigureBase(fragments, 0.7f, 0.55f, 7.5f, 0.18f, 20, new Color(1f, 0.4f, 0.04f));
            ParticleSystem.MainModule fragmentMain = fragments.main;
            fragmentMain.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.62f);
            fragmentMain.startSpeed = new ParticleSystem.MinMaxCurve(6.5f, 8.4f);
            fragmentMain.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.25f);
            ConfigureCone(fragments, new Vector3(90f, 0f, 0f), 8f, 0.18f, new Vector3(0f, 4.25f, 0f));
            SetBursts(fragments, new ParticleSystem.Burst(0f, 12));
            ConfigureNoise(fragments, 0.35f, 1.1f);
            SetFade(fragments, 0.9f, 0f);

            ParticleSystem impactFlash = CreateSystem(root.transform, "ImpactFlash", glow, 8);
            ConfigureBase(impactFlash, 0.8f, 0.28f, 0f, 3.2f, 4, new Color(1f, 0.75f, 0.18f));
            ParticleSystem.MainModule flashMain = impactFlash.main;
            flashMain.startDelay = 0.48f;
            flashMain.startSize = new ParticleSystem.MinMaxCurve(2.4f, 3.4f);
            SetBursts(impactFlash, new ParticleSystem.Burst(0f, 2));
            SetSizeOverLifetime(impactFlash, Curve((0f, 0.15f), (0.18f, 1f), (1f, 0.35f)));
            SetFade(impactFlash, 1f, 0f);

            ParticleSystem shockwave = CreateSystem(root.transform, "FireShockwave", ring, 7);
            shockwave.transform.localScale = new Vector3(1f, 0.28f, 1f);
            ConfigureBase(shockwave, 1.2f, 0.68f, 0f, 5.5f, 4, new Color(1f, 0.34f, 0.02f));
            ParticleSystem.MainModule shockMain = shockwave.main;
            shockMain.startDelay = 0.48f;
            shockMain.startLifetime = new ParticleSystem.MinMaxCurve(0.52f, 0.72f);
            shockMain.startSize = new ParticleSystem.MinMaxCurve(4.5f, 5.6f);
            SetBursts(
                shockwave,
                new ParticleSystem.Burst(0f, 1),
                new ParticleSystem.Burst(0.12f, 1));
            SetSizeOverLifetime(shockwave, Curve((0f, 0.08f), (0.6f, 0.95f), (1f, 1.2f)));
            SetFade(shockwave, 0.9f, 0f);

            ParticleSystem flames = CreateSystem(root.transform, "FlameBurst", flameMaterial, 6);
            ConfigureBase(flames, 1.4f, 0.8f, 4f, 0.55f, 60, new Color(1f, 0.42f, 0.03f));
            ParticleSystem.MainModule flameMain = flames.main;
            flameMain.startDelay = 0.49f;
            flameMain.startLifetime = new ParticleSystem.MinMaxCurve(0.38f, 0.95f);
            flameMain.startSpeed = new ParticleSystem.MinMaxCurve(2.2f, 6.5f);
            flameMain.startSize = new ParticleSystem.MinMaxCurve(0.38f, 1.05f);
            ConfigureCone(flames, new Vector3(-90f, 0f, 0f), 42f, 0.55f, Vector3.zero);
            SetBursts(flames, new ParticleSystem.Burst(0f, 38));
            ConfigureNoise(flames, 0.55f, 0.75f);
            SetColorOverLifetime(
                flames,
                new Color(1f, 0.9f, 0.25f),
                new Color(0.9f, 0.04f, 0f),
                1f,
                0f);
            SetSizeOverLifetime(flames, Curve((0f, 0.2f), (0.2f, 1f), (1f, 0.08f)));
            ParticleSystemRenderer flameRenderer = flames.GetComponent<ParticleSystemRenderer>();
            flameRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            flameRenderer.alignment = ParticleSystemRenderSpace.Velocity;

            ParticleSystem embers = CreateSystem(root.transform, "Embers", glow, 5);
            ConfigureBase(embers, 1.8f, 1.1f, 5f, 0.12f, 90, new Color(1f, 0.3f, 0.01f));
            ParticleSystem.MainModule emberMain = embers.main;
            emberMain.startDelay = 0.5f;
            emberMain.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 1.35f);
            emberMain.startSpeed = new ParticleSystem.MinMaxCurve(2.5f, 7.5f);
            emberMain.startSize = new ParticleSystem.MinMaxCurve(0.035f, 0.14f);
            emberMain.gravityModifier = new ParticleSystem.MinMaxCurve(1.1f);
            ConfigureCircle(embers, 0.25f);
            SetBursts(embers, new ParticleSystem.Burst(0f, 55));
            SetFade(embers, 1f, 0f);

            ParticleSystem smokePuffs = CreateSystem(root.transform, "Smoke", smoke, 4);
            ConfigureBase(smokePuffs, 2.1f, 1.4f, 1.5f, 1f, 36, new Color(0.32f, 0.12f, 0.05f, 0.68f));
            ParticleSystem.MainModule smokeMain = smokePuffs.main;
            smokeMain.startDelay = 0.58f;
            smokeMain.startLifetime = new ParticleSystem.MinMaxCurve(0.85f, 1.65f);
            smokeMain.startSpeed = new ParticleSystem.MinMaxCurve(0.7f, 2.1f);
            smokeMain.startSize = new ParticleSystem.MinMaxCurve(0.55f, 1.35f);
            smokeMain.startRotation = new ParticleSystem.MinMaxCurve(-Mathf.PI, Mathf.PI);
            ConfigureCone(smokePuffs, new Vector3(-90f, 0f, 0f), 50f, 0.7f, Vector3.zero);
            SetBursts(smokePuffs, new ParticleSystem.Burst(0f, 20));
            ConfigureNoise(smokePuffs, 0.42f, 0.45f);
            SetColorOverLifetime(
                smokePuffs,
                new Color(0.45f, 0.16f, 0.05f),
                new Color(0.08f, 0.045f, 0.035f),
                0.62f,
                0f);
            SetSizeOverLifetime(smokePuffs, Curve((0f, 0.35f), (0.55f, 1f), (1f, 1.35f)));

            return SavePrefab(root, FireballPrefabPath);
        }

        private static GameObject CreateEarthPrefab(
            Material glow,
            Material ring,
            Material rock,
            Material smoke)
        {
            GameObject root = CreateEffectRoot("Skill_002_EarthMagic", "skill_002");

            ParticleSystem flash = CreateSystem(root.transform, "GroundFlash", glow, 8);
            ConfigureBase(flash, 0.8f, 0.35f, 0f, 3.4f, 5, new Color(1f, 0.72f, 0.28f));
            ParticleSystem.MainModule flashMain = flash.main;
            flashMain.startSize = new ParticleSystem.MinMaxCurve(2.2f, 3.6f);
            SetBursts(flash, new ParticleSystem.Burst(0f, 3));
            SetSizeOverLifetime(flash, Curve((0f, 0.08f), (0.16f, 1f), (1f, 0.1f)));
            SetFade(flash, 1f, 0f);

            ParticleSystem shockwave = CreateSystem(root.transform, "EarthShockwave", ring, 7);
            shockwave.transform.localScale = new Vector3(1f, 0.26f, 1f);
            ConfigureBase(shockwave, 1.3f, 0.8f, 0f, 6f, 5, new Color(1f, 0.56f, 0.16f));
            ParticleSystem.MainModule shockMain = shockwave.main;
            shockMain.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 0.85f);
            shockMain.startSize = new ParticleSystem.MinMaxCurve(5.2f, 6.2f);
            SetBursts(
                shockwave,
                new ParticleSystem.Burst(0f, 1),
                new ParticleSystem.Burst(0.16f, 1));
            SetSizeOverLifetime(shockwave, Curve((0f, 0.08f), (0.55f, 0.9f), (1f, 1.15f)));
            SetFade(shockwave, 0.85f, 0f);

            ParticleSystem spikes = CreateSystem(root.transform, "RockSpikes", rock, 6);
            ConfigureBase(spikes, 1.4f, 1f, 3.5f, 0.9f, 40, new Color(0.58f, 0.29f, 0.09f));
            ParticleSystem.MainModule spikeMain = spikes.main;
            spikeMain.startLifetime = new ParticleSystem.MinMaxCurve(0.65f, 1.15f);
            spikeMain.startSpeed = new ParticleSystem.MinMaxCurve(1.8f, 5.2f);
            spikeMain.startSize = new ParticleSystem.MinMaxCurve(0.55f, 1.25f);
            spikeMain.startRotation = new ParticleSystem.MinMaxCurve(-0.35f, 0.35f);
            spikeMain.gravityModifier = new ParticleSystem.MinMaxCurve(2.8f, 4.2f);
            ConfigureCone(spikes, new Vector3(-90f, 0f, 0f), 48f, 0.5f, Vector3.zero);
            SetBursts(spikes, new ParticleSystem.Burst(0.02f, 24));
            SetColorOverLifetime(
                spikes,
                new Color(1f, 0.68f, 0.26f),
                new Color(0.22f, 0.1f, 0.035f),
                1f,
                0f);
            SetSizeOverLifetime(spikes, Curve((0f, 0.15f), (0.12f, 1f), (0.78f, 0.9f), (1f, 0f)));
            ParticleSystemRenderer spikeRenderer = spikes.GetComponent<ParticleSystemRenderer>();
            spikeRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            spikeRenderer.alignment = ParticleSystemRenderSpace.Velocity;

            ParticleSystem pillars = CreateSystem(root.transform, "CentralPillars", rock, 5);
            ConfigureBase(pillars, 1.25f, 1.05f, 1.2f, 1.8f, 16, new Color(0.48f, 0.22f, 0.07f));
            ParticleSystem.MainModule pillarMain = pillars.main;
            pillarMain.startLifetime = new ParticleSystem.MinMaxCurve(0.85f, 1.15f);
            pillarMain.startSpeed = new ParticleSystem.MinMaxCurve(0.65f, 1.3f);
            pillarMain.startSize = new ParticleSystem.MinMaxCurve(1.25f, 2.05f);
            pillarMain.startRotation = new ParticleSystem.MinMaxCurve(-0.25f, 0.25f);
            ConfigureCone(pillars, new Vector3(-90f, 0f, 0f), 22f, 0.45f, Vector3.zero);
            SetBursts(pillars, new ParticleSystem.Burst(0f, 9));
            SetColorOverLifetime(
                pillars,
                new Color(0.95f, 0.55f, 0.2f),
                new Color(0.26f, 0.12f, 0.04f),
                1f,
                0f);
            SetSizeOverLifetime(pillars, Curve((0f, 0f), (0.12f, 1f), (0.8f, 0.95f), (1f, 0f)));
            ParticleSystemRenderer pillarRenderer = pillars.GetComponent<ParticleSystemRenderer>();
            pillarRenderer.renderMode = ParticleSystemRenderMode.Billboard;

            ParticleSystem debris = CreateSystem(root.transform, "Debris", rock, 4);
            ConfigureBase(debris, 1.8f, 1f, 5f, 0.24f, 100, new Color(0.42f, 0.2f, 0.06f));
            ParticleSystem.MainModule debrisMain = debris.main;
            debrisMain.startLifetime = new ParticleSystem.MinMaxCurve(0.55f, 1.45f);
            debrisMain.startSpeed = new ParticleSystem.MinMaxCurve(2.2f, 7f);
            debrisMain.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.32f);
            debrisMain.startRotation = new ParticleSystem.MinMaxCurve(-Mathf.PI, Mathf.PI);
            debrisMain.gravityModifier = new ParticleSystem.MinMaxCurve(2.2f);
            ConfigureCone(debris, new Vector3(-90f, 0f, 0f), 65f, 0.65f, Vector3.zero);
            SetBursts(debris, new ParticleSystem.Burst(0.02f, 58));
            SetFade(debris, 1f, 0f);
            ConfigureRotation(debris, -4f, 4f);

            ParticleSystem dust = CreateSystem(root.transform, "Dust", smoke, 3);
            ConfigureBase(dust, 1.9f, 1.2f, 1.5f, 1.1f, 55, new Color(0.32f, 0.18f, 0.08f, 0.72f));
            ParticleSystem.MainModule dustMain = dust.main;
            dustMain.startDelay = 0.04f;
            dustMain.startLifetime = new ParticleSystem.MinMaxCurve(0.7f, 1.55f);
            dustMain.startSpeed = new ParticleSystem.MinMaxCurve(0.7f, 2.4f);
            dustMain.startSize = new ParticleSystem.MinMaxCurve(0.5f, 1.4f);
            dustMain.startRotation = new ParticleSystem.MinMaxCurve(-Mathf.PI, Mathf.PI);
            ConfigureCircle(dust, 0.55f);
            SetBursts(dust, new ParticleSystem.Burst(0f, 32));
            ConfigureNoise(dust, 0.35f, 0.5f);
            SetColorOverLifetime(
                dust,
                new Color(0.48f, 0.27f, 0.12f),
                new Color(0.12f, 0.075f, 0.045f),
                0.55f,
                0f);
            SetSizeOverLifetime(dust, Curve((0f, 0.2f), (0.5f, 1f), (1f, 1.3f)));

            ParticleSystem glints = CreateSystem(root.transform, "ImpactGlints", glow, 9);
            ConfigureBase(glints, 1f, 0.55f, 5f, 0.11f, 40, new Color(1f, 0.76f, 0.3f));
            ParticleSystem.MainModule glintMain = glints.main;
            glintMain.startLifetime = new ParticleSystem.MinMaxCurve(0.22f, 0.7f);
            glintMain.startSpeed = new ParticleSystem.MinMaxCurve(3f, 8f);
            glintMain.startSize = new ParticleSystem.MinMaxCurve(0.035f, 0.16f);
            glintMain.gravityModifier = new ParticleSystem.MinMaxCurve(1.4f);
            ConfigureCircle(glints, 0.3f);
            SetBursts(glints, new ParticleSystem.Burst(0f, 34));
            SetFade(glints, 1f, 0f);

            return SavePrefab(root, EarthPrefabPath);
        }

        private static GameObject CreateNailDrivingPrefab(
            Material pixelGlow,
            Material pixelRing,
            Material pixelShard,
            Material pixelSmoke,
            Material nail)
        {
            GameObject root = CreateEffectRoot("Skill_004_NailDriving", "skill_004");

            ParticleSystem fallingNail = CreateSystem(root.transform, "FallingNail", nail, 12);
            ConfigureBase(fallingNail, 0.7f, 0.52f, 8.8f, 0.9f, 2, new Color(0.54f, 0.16f, 0.86f));
            ParticleSystem.MainModule fallingNailMain = fallingNail.main;
            fallingNailMain.startLifetime = new ParticleSystem.MinMaxCurve(0.49f, 0.52f);
            fallingNailMain.startSpeed = new ParticleSystem.MinMaxCurve(8.6f, 8.9f);
            fallingNailMain.startSize3D = true;
            fallingNailMain.startSizeX = new ParticleSystem.MinMaxCurve(0.9f, 1.05f);
            fallingNailMain.startSizeY = new ParticleSystem.MinMaxCurve(3f, 3.25f);
            fallingNailMain.startSizeZ = 1f;
            ConfigureCone(
                fallingNail,
                new Vector3(90f, 0f, 0f),
                0.6f,
                0.02f,
                new Vector3(0f, 4.45f, 0f));
            SetBursts(fallingNail, new ParticleSystem.Burst(0f, 1));
            SetColorOverLifetime(
                fallingNail,
                new Color(0.86f, 0.54f, 1f),
                new Color(0.2f, 0.02f, 0.34f),
                1f,
                1f);

            ParticleSystem descentAura = CreateSystem(root.transform, "DescentAura", pixelGlow, 11);
            ConfigureBase(descentAura, 0.7f, 0.46f, 8.7f, 0.42f, 8, new Color(0.72f, 0.25f, 1f));
            ParticleSystem.MainModule descentAuraMain = descentAura.main;
            descentAuraMain.startLifetime = new ParticleSystem.MinMaxCurve(0.32f, 0.48f);
            descentAuraMain.startSpeed = new ParticleSystem.MinMaxCurve(8.2f, 9.2f);
            descentAuraMain.startSize = new ParticleSystem.MinMaxCurve(0.18f, 0.48f);
            ConfigureCone(
                descentAura,
                new Vector3(90f, 0f, 0f),
                4f,
                0.12f,
                new Vector3(0f, 4.35f, 0f));
            SetBursts(descentAura, new ParticleSystem.Burst(0f, 8));
            ConfigureNoise(descentAura, 0.2f, 1.1f);
            SetFade(descentAura, 0.8f, 0f);
            ParticleSystemRenderer descentAuraRenderer = descentAura.GetComponent<ParticleSystemRenderer>();
            descentAuraRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            descentAuraRenderer.alignment = ParticleSystemRenderSpace.Velocity;

            ParticleSystem embeddedNail = CreateSystem(root.transform, "EmbeddedNail", nail, 10);
            embeddedNail.transform.localPosition = new Vector3(0f, 0.82f, 0f);
            ConfigureBase(embeddedNail, 1.6f, 0.92f, 0f, 2.65f, 2, new Color(0.36f, 0.07f, 0.58f));
            ParticleSystem.MainModule embeddedNailMain = embeddedNail.main;
            embeddedNailMain.startDelay = 0.47f;
            embeddedNailMain.startLifetime = 0.92f;
            SetBursts(embeddedNail, new ParticleSystem.Burst(0f, 1));
            SetColorOverLifetime(
                embeddedNail,
                new Color(0.88f, 0.62f, 1f),
                new Color(0.12f, 0.015f, 0.2f),
                1f,
                0f);
            SetSizeOverLifetime(
                embeddedNail,
                Curve((0f, 0.55f), (0.08f, 1f), (0.78f, 0.96f), (1f, 0.84f)));

            ParticleSystem impactFlash = CreateSystem(root.transform, "VoidImpactFlash", pixelGlow, 9);
            ConfigureBase(impactFlash, 0.9f, 0.32f, 0f, 3.5f, 6, new Color(0.78f, 0.34f, 1f));
            ParticleSystem.MainModule impactFlashMain = impactFlash.main;
            impactFlashMain.startDelay = 0.45f;
            impactFlashMain.startSize = new ParticleSystem.MinMaxCurve(2.8f, 4.2f);
            SetBursts(impactFlash, new ParticleSystem.Burst(0f, 4));
            SetColorOverLifetime(
                impactFlash,
                new Color(1f, 0.82f, 1f),
                new Color(0.37f, 0.03f, 0.7f),
                1f,
                0f);
            SetSizeOverLifetime(impactFlash, Curve((0f, 0.08f), (0.14f, 1f), (1f, 0.16f)));

            ParticleSystem shockwave = CreateSystem(root.transform, "VoidShockwave", pixelRing, 8);
            shockwave.transform.localScale = new Vector3(1f, 0.24f, 1f);
            ConfigureBase(shockwave, 1.3f, 0.72f, 0f, 6f, 4, new Color(0.63f, 0.16f, 1f));
            ParticleSystem.MainModule shockwaveMain = shockwave.main;
            shockwaveMain.startDelay = 0.46f;
            shockwaveMain.startLifetime = new ParticleSystem.MinMaxCurve(0.55f, 0.74f);
            shockwaveMain.startSize = new ParticleSystem.MinMaxCurve(5.2f, 6.4f);
            SetBursts(
                shockwave,
                new ParticleSystem.Burst(0f, 1),
                new ParticleSystem.Burst(0.14f, 1));
            SetColorOverLifetime(
                shockwave,
                new Color(0.92f, 0.62f, 1f),
                new Color(0.24f, 0.02f, 0.48f),
                0.9f,
                0f);
            SetSizeOverLifetime(shockwave, Curve((0f, 0.05f), (0.52f, 0.92f), (1f, 1.18f)));

            ParticleSystem groundSpikes = CreateSystem(root.transform, "VoidGroundSpikes", pixelShard, 7);
            ConfigureBase(groundSpikes, 1.4f, 0.95f, 3.8f, 0.9f, 28, new Color(0.34f, 0.055f, 0.52f));
            ParticleSystem.MainModule groundSpikesMain = groundSpikes.main;
            groundSpikesMain.startDelay = 0.46f;
            groundSpikesMain.startLifetime = new ParticleSystem.MinMaxCurve(0.62f, 1.02f);
            groundSpikesMain.startSpeed = new ParticleSystem.MinMaxCurve(1.8f, 5.4f);
            groundSpikesMain.startSize = new ParticleSystem.MinMaxCurve(0.62f, 1.45f);
            groundSpikesMain.startRotation = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);
            groundSpikesMain.gravityModifier = new ParticleSystem.MinMaxCurve(2.2f, 3.6f);
            ConfigureCone(groundSpikes, new Vector3(-90f, 0f, 0f), 54f, 0.5f, Vector3.zero);
            SetBursts(groundSpikes, new ParticleSystem.Burst(0f, 16));
            SetColorOverLifetime(
                groundSpikes,
                new Color(0.91f, 0.55f, 1f),
                new Color(0.12f, 0.015f, 0.19f),
                1f,
                0f);
            SetSizeOverLifetime(
                groundSpikes,
                Curve((0f, 0.08f), (0.12f, 1f), (0.78f, 0.88f), (1f, 0f)));
            ParticleSystemRenderer groundSpikesRenderer = groundSpikes.GetComponent<ParticleSystemRenderer>();
            groundSpikesRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            groundSpikesRenderer.alignment = ParticleSystemRenderSpace.Velocity;

            ParticleSystem shards = CreateSystem(root.transform, "VoidShards", pixelShard, 6);
            ConfigureBase(shards, 1.7f, 1f, 5.5f, 0.3f, 64, new Color(0.27f, 0.035f, 0.4f));
            ParticleSystem.MainModule shardsMain = shards.main;
            shardsMain.startDelay = 0.47f;
            shardsMain.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.25f);
            shardsMain.startSpeed = new ParticleSystem.MinMaxCurve(2.8f, 7.6f);
            shardsMain.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.46f);
            shardsMain.startRotation = new ParticleSystem.MinMaxCurve(-Mathf.PI, Mathf.PI);
            shardsMain.gravityModifier = new ParticleSystem.MinMaxCurve(2.4f);
            ConfigureCone(shards, new Vector3(-90f, 0f, 0f), 68f, 0.58f, Vector3.zero);
            SetBursts(shards, new ParticleSystem.Burst(0f, 34));
            SetColorOverLifetime(
                shards,
                new Color(0.72f, 0.27f, 1f),
                new Color(0.08f, 0.01f, 0.12f),
                1f,
                0f);
            ConfigureRotation(shards, -5f, 5f);

            ParticleSystem sparks = CreateSystem(root.transform, "ArcSparks", pixelGlow, 13);
            ConfigureBase(sparks, 1.2f, 0.7f, 7f, 0.14f, 40, new Color(0.83f, 0.46f, 1f));
            ParticleSystem.MainModule sparksMain = sparks.main;
            sparksMain.startDelay = 0.45f;
            sparksMain.startLifetime = new ParticleSystem.MinMaxCurve(0.24f, 0.72f);
            sparksMain.startSpeed = new ParticleSystem.MinMaxCurve(3.5f, 8.5f);
            sparksMain.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.22f);
            sparksMain.gravityModifier = new ParticleSystem.MinMaxCurve(1.2f);
            ConfigureCone(sparks, new Vector3(-90f, 0f, 0f), 72f, 0.32f, Vector3.zero);
            SetBursts(sparks, new ParticleSystem.Burst(0f, 26));
            ConfigureNoise(sparks, 0.32f, 1.4f);
            SetFade(sparks, 1f, 0f);
            ParticleSystemRenderer sparksRenderer = sparks.GetComponent<ParticleSystemRenderer>();
            sparksRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            sparksRenderer.alignment = ParticleSystemRenderSpace.Velocity;

            ParticleSystem voidSmoke = CreateSystem(root.transform, "VoidSmoke", pixelSmoke, 5);
            ConfigureBase(voidSmoke, 2.2f, 1.5f, 1.7f, 1.25f, 28, new Color(0.15f, 0.035f, 0.2f, 0.78f));
            ParticleSystem.MainModule voidSmokeMain = voidSmoke.main;
            voidSmokeMain.startDelay = 0.5f;
            voidSmokeMain.startLifetime = new ParticleSystem.MinMaxCurve(0.85f, 1.65f);
            voidSmokeMain.startSpeed = new ParticleSystem.MinMaxCurve(0.7f, 2.2f);
            voidSmokeMain.startSize = new ParticleSystem.MinMaxCurve(0.72f, 1.65f);
            voidSmokeMain.startRotation = new ParticleSystem.MinMaxCurve(-Mathf.PI, Mathf.PI);
            ConfigureCircle(voidSmoke, 0.65f);
            SetBursts(voidSmoke, new ParticleSystem.Burst(0f, 18));
            ConfigureNoise(voidSmoke, 0.5f, 0.45f);
            SetColorOverLifetime(
                voidSmoke,
                new Color(0.28f, 0.07f, 0.4f),
                new Color(0.025f, 0.01f, 0.035f),
                0.68f,
                0f);
            SetSizeOverLifetime(voidSmoke, Curve((0f, 0.22f), (0.52f, 1f), (1f, 1.35f)));

            return SavePrefab(root, NailDrivingPrefabPath);
        }

        private static GameObject CreatePlagueMagicPrefab(
            Material glow,
            Material ring,
            Material smoke,
            Material skull,
            Material plaguePool)
        {
            GameObject root = CreateEffectRoot("Skill_006_PlagueMagic", "skill_006");

            ParticleSystem comet = CreateSystem(root.transform, "CarrionComet", skull, 15);
            comet.transform.localPosition = new Vector3(-3.8f, 2.75f, 0f);
            ConfigureBase(comet, 0.9f, 0.58f, 0f, 1.05f, 3, Color.white);
            ParticleSystem.MainModule cometMain = comet.main;
            cometMain.startLifetime = new ParticleSystem.MinMaxCurve(0.56f, 0.6f);
            cometMain.startSize = new ParticleSystem.MinMaxCurve(0.92f, 1.16f);
            cometMain.startRotation = new ParticleSystem.MinMaxCurve(-0.2f, 0.12f);
            SetBursts(comet, new ParticleSystem.Burst(0f, 1));
            SetColorOverLifetime(
                comet,
                new Color(0.9f, 1f, 0.52f),
                new Color(0.44f, 0.92f, 0.015f),
                1f,
                0.35f);
            SetSizeOverLifetime(comet, Curve((0f, 0.62f), (0.12f, 1f), (0.82f, 0.9f), (1f, 0.3f)));
            ConfigureVelocity(comet, 6.7f, -4.85f);

            ParticleSystemRenderer cometRenderer = comet.GetComponent<ParticleSystemRenderer>();
            cometRenderer.renderMode = ParticleSystemRenderMode.Billboard;

            ParticleSystem cometHaze = CreateSystem(root.transform, "CometMiasma", glow, 14);
            cometHaze.transform.localPosition = new Vector3(-3.8f, 2.75f, 0f);
            ConfigureBase(cometHaze, 0.9f, 0.52f, 0f, 0.35f, 40, Color.white);
            ParticleSystem.MainModule hazeMain = cometHaze.main;
            hazeMain.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.72f);
            hazeMain.startSize = new ParticleSystem.MinMaxCurve(0.14f, 0.5f);
            ConfigureCircle(cometHaze, 0.24f);
            SetBursts(cometHaze, new ParticleSystem.Burst(0f, 24));
            ParticleSystem.VelocityOverLifetimeModule hazeVelocity = cometHaze.velocityOverLifetime;
            hazeVelocity.enabled = true;
            hazeVelocity.space = ParticleSystemSimulationSpace.Local;
            hazeVelocity.x = new ParticleSystem.MinMaxCurve(5.4f, 6.8f);
            hazeVelocity.y = new ParticleSystem.MinMaxCurve(-5.2f, -4.1f);
            hazeVelocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);
            ConfigureNoise(cometHaze, 0.42f, 1.2f);
            SetColorOverLifetime(
                cometHaze,
                new Color(0.68f, 1f, 0.05f),
                new Color(0.28f, 0.015f, 0.46f),
                0.9f,
                0f);
            ParticleSystemRenderer hazeRenderer = cometHaze.GetComponent<ParticleSystemRenderer>();
            hazeRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            hazeRenderer.alignment = ParticleSystemRenderSpace.Velocity;

            ParticleSystem impactFlash = CreateSystem(root.transform, "CorrosiveImpact", glow, 13);
            ConfigureBase(impactFlash, 0.9f, 0.34f, 0f, 4f, 6, Color.white);
            ParticleSystem.MainModule impactMain = impactFlash.main;
            impactMain.startDelay = 0.54f;
            impactMain.startSize = new ParticleSystem.MinMaxCurve(3f, 4.5f);
            SetBursts(impactFlash, new ParticleSystem.Burst(0f, 4));
            SetColorOverLifetime(
                impactFlash,
                new Color(0.88f, 1f, 0.35f),
                new Color(0.36f, 0.04f, 0.58f),
                1f,
                0f);
            SetSizeOverLifetime(impactFlash, Curve((0f, 0.06f), (0.14f, 1f), (1f, 0.12f)));

            ParticleSystem pool = CreateSystem(root.transform, "LivingPlaguePool", plaguePool, 6);
            pool.transform.localScale = new Vector3(1f, 0.34f, 1f);
            ConfigureBase(pool, 3.1f, 2.45f, 0f, 6.4f, 3, Color.white);
            ParticleSystem.MainModule poolMain = pool.main;
            poolMain.startDelay = 0.56f;
            poolMain.startLifetime = new ParticleSystem.MinMaxCurve(2.2f, 2.5f);
            poolMain.startSize = new ParticleSystem.MinMaxCurve(5.7f, 6.5f);
            poolMain.startRotation = new ParticleSystem.MinMaxCurve(-0.16f, 0.16f);
            SetBursts(pool, new ParticleSystem.Burst(0f, 2));
            SetColorOverLifetime(
                pool,
                new Color(0.56f, 1f, 0.015f),
                new Color(0.18f, 0.025f, 0.28f),
                0.85f,
                0f);
            SetSizeOverLifetime(pool, Curve((0f, 0.04f), (0.12f, 1f), (0.8f, 1.05f), (1f, 0.9f)));
            ConfigureRotation(pool, -0.08f, 0.08f);

            ParticleSystem pulseRings = CreateSystem(root.transform, "InfectionPulse", ring, 12);
            pulseRings.transform.localScale = new Vector3(1f, 0.24f, 1f);
            ConfigureBase(pulseRings, 2.5f, 0.72f, 0f, 6.2f, 8, Color.white);
            ParticleSystem.MainModule pulseMain = pulseRings.main;
            pulseMain.startDelay = 0.55f;
            pulseMain.startLifetime = new ParticleSystem.MinMaxCurve(0.58f, 0.76f);
            pulseMain.startSize = new ParticleSystem.MinMaxCurve(5.2f, 6.4f);
            SetBursts(
                pulseRings,
                new ParticleSystem.Burst(0f, 1),
                new ParticleSystem.Burst(0.58f, 1),
                new ParticleSystem.Burst(1.16f, 1));
            SetColorOverLifetime(
                pulseRings,
                new Color(0.76f, 1f, 0.18f),
                new Color(0.42f, 0.02f, 0.62f),
                0.9f,
                0f);
            SetSizeOverLifetime(pulseRings, Curve((0f, 0.08f), (0.58f, 0.9f), (1f, 1.18f)));

            ParticleSystem splash = CreateSystem(root.transform, "AcidSplash", glow, 11);
            ConfigureBase(splash, 1.7f, 0.9f, 5f, 0.16f, 90, Color.white);
            ParticleSystem.MainModule splashMain = splash.main;
            splashMain.startDelay = 0.54f;
            splashMain.startLifetime = new ParticleSystem.MinMaxCurve(0.38f, 1.15f);
            splashMain.startSpeed = new ParticleSystem.MinMaxCurve(2.8f, 7.8f);
            splashMain.startSize = new ParticleSystem.MinMaxCurve(0.045f, 0.2f);
            splashMain.gravityModifier = new ParticleSystem.MinMaxCurve(1.4f, 2.4f);
            ConfigureCone(splash, new Vector3(-90f, 0f, 0f), 68f, 0.7f, Vector3.zero);
            SetBursts(splash, new ParticleSystem.Burst(0f, 56));
            SetColorOverLifetime(
                splash,
                new Color(0.86f, 1f, 0.24f),
                new Color(0.26f, 0.015f, 0.43f),
                1f,
                0f);
            ParticleSystemRenderer splashRenderer = splash.GetComponent<ParticleSystemRenderer>();
            splashRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            splashRenderer.alignment = ParticleSystemRenderSpace.Velocity;

            ParticleSystem wraiths = CreateSystem(root.transform, "WraithSkulls", skull, 10);
            ConfigureBase(wraiths, 2.3f, 1.35f, 1.8f, 0.58f, 24, Color.white);
            ParticleSystem.MainModule wraithMain = wraiths.main;
            wraithMain.startDelay = 0.68f;
            wraithMain.startLifetime = new ParticleSystem.MinMaxCurve(0.9f, 1.65f);
            wraithMain.startSpeed = new ParticleSystem.MinMaxCurve(0.8f, 2.4f);
            wraithMain.startSize = new ParticleSystem.MinMaxCurve(0.3f, 0.82f);
            wraithMain.startRotation = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);
            ConfigureCone(wraiths, new Vector3(-90f, 0f, 0f), 32f, 1.8f, Vector3.zero);
            SetBursts(
                wraiths,
                new ParticleSystem.Burst(0f, 7),
                new ParticleSystem.Burst(0.7f, 5));
            ConfigureNoise(wraiths, 0.48f, 0.62f);
            ConfigureRotation(wraiths, -0.6f, 0.6f);
            SetColorOverLifetime(
                wraiths,
                new Color(0.72f, 1f, 0.12f),
                new Color(0.34f, 0.035f, 0.5f),
                0.92f,
                0f);
            SetSizeOverLifetime(wraiths, Curve((0f, 0.35f), (0.2f, 1f), (0.75f, 0.82f), (1f, 0.25f)));

            ParticleSystem spores = CreateSystem(root.transform, "PlagueSpores", glow, 9);
            ConfigureBase(spores, 2.6f, 0.85f, 1.3f, 0.14f, 100, Color.white);
            ParticleSystem.MainModule sporeMain = spores.main;
            sporeMain.startDelay = 0.66f;
            sporeMain.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 1.15f);
            sporeMain.startSpeed = new ParticleSystem.MinMaxCurve(0.45f, 1.85f);
            sporeMain.startSize = new ParticleSystem.MinMaxCurve(0.045f, 0.23f);
            ConfigureCone(spores, new Vector3(-90f, 0f, 0f), 58f, 2f, Vector3.zero);
            SetBursts(
                spores,
                new ParticleSystem.Burst(0f, 24),
                new ParticleSystem.Burst(0.52f, 18),
                new ParticleSystem.Burst(1.04f, 14));
            ConfigureNoise(spores, 0.38f, 0.8f);
            SetColorOverLifetime(
                spores,
                new Color(0.73f, 1f, 0.08f),
                new Color(0.32f, 0.03f, 0.48f),
                1f,
                0f);

            ParticleSystem miasma = CreateSystem(root.transform, "MiasmaCloud", smoke, 7);
            ConfigureBase(miasma, 2.8f, 1.65f, 1.5f, 1.25f, 60, Color.white);
            ParticleSystem.MainModule miasmaMain = miasma.main;
            miasmaMain.startDelay = 0.6f;
            miasmaMain.startLifetime = new ParticleSystem.MinMaxCurve(1.1f, 2f);
            miasmaMain.startSpeed = new ParticleSystem.MinMaxCurve(0.65f, 2f);
            miasmaMain.startSize = new ParticleSystem.MinMaxCurve(0.65f, 1.8f);
            miasmaMain.startRotation = new ParticleSystem.MinMaxCurve(-Mathf.PI, Mathf.PI);
            ConfigureCone(miasma, new Vector3(-90f, 0f, 0f), 48f, 1.45f, Vector3.zero);
            SetBursts(miasma, new ParticleSystem.Burst(0f, 34));
            ConfigureNoise(miasma, 0.56f, 0.42f);
            SetColorOverLifetime(
                miasma,
                new Color(0.31f, 0.52f, 0.035f),
                new Color(0.12f, 0.018f, 0.18f),
                0.58f,
                0f);
            SetSizeOverLifetime(miasma, Curve((0f, 0.18f), (0.5f, 1f), (1f, 1.32f)));

            ParticleSystem rain = CreateSystem(root.transform, "VirulentRain", glow, 8);
            ConfigureBase(rain, 2.3f, 0.72f, 0f, 0.1f, 80, Color.white);
            ParticleSystem.MainModule rainMain = rain.main;
            rainMain.startDelay = 0.78f;
            rainMain.startLifetime = new ParticleSystem.MinMaxCurve(0.48f, 0.82f);
            rainMain.startSize = new ParticleSystem.MinMaxCurve(0.035f, 0.12f);
            ParticleSystem.ShapeModule rainShape = rain.shape;
            rainShape.enabled = true;
            rainShape.shapeType = ParticleSystemShapeType.Box;
            rainShape.position = new Vector3(0f, 2.8f, 0f);
            rainShape.scale = new Vector3(4.8f, 0.2f, 0.1f);
            SetBursts(
                rain,
                new ParticleSystem.Burst(0f, 20),
                new ParticleSystem.Burst(0.48f, 18),
                new ParticleSystem.Burst(0.96f, 14));
            ParticleSystem.VelocityOverLifetimeModule rainVelocity = rain.velocityOverLifetime;
            rainVelocity.enabled = true;
            rainVelocity.space = ParticleSystemSimulationSpace.Local;
            rainVelocity.x = new ParticleSystem.MinMaxCurve(-0.3f, 0.3f);
            rainVelocity.y = new ParticleSystem.MinMaxCurve(-5.2f, -4.2f);
            rainVelocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);
            SetColorOverLifetime(
                rain,
                new Color(0.72f, 1f, 0.08f),
                new Color(0.37f, 0.03f, 0.54f),
                0.85f,
                0f);
            ParticleSystemRenderer rainRenderer = rain.GetComponent<ParticleSystemRenderer>();
            rainRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            rainRenderer.alignment = ParticleSystemRenderSpace.Velocity;

            return SavePrefab(root, PlagueMagicPrefabPath);
        }

        private static GameObject CreateIceLancePrefab(
            Material lanceAlpha,
            Material lanceAdditive,
            Material crownBack,
            Material crownFront,
            Material frostPatch,
            Material snowflake,
            Material pixelGlow,
            Material pixelShard,
            Material pixelSmoke)
        {
            GameObject root = CreateEffectRoot("Skill_007_IceLance", "skill_007");

            ParticleSystem lanceGlow = CreateSystem(root.transform, "IceLanceGlow", lanceAdditive, 14);
            ConfigureBase(lanceGlow, 0.7f, 0.51f, 9.1f, 1f, 2, Color.white);
            ParticleSystem.MainModule lanceGlowMain = lanceGlow.main;
            lanceGlowMain.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.52f);
            lanceGlowMain.startSpeed = new ParticleSystem.MinMaxCurve(8.95f, 9.15f);
            SetStartSize3D(lanceGlow, 4.15f, 1.18f);
            ConfigureCone(
                lanceGlow,
                new Vector3(0f, 90f, 0f),
                0.35f,
                0.01f,
                new Vector3(-4.6f, 0.72f, 0f));
            SetBursts(lanceGlow, new ParticleSystem.Burst(0f, 1));
            SetColorOverLifetime(
                lanceGlow,
                new Color(0.66f, 0.9f, 1f),
                new Color(0.22f, 0.58f, 1f),
                0.58f,
                0f);
            SetDeterministic(lanceGlow, 7001);

            ParticleSystem lanceCore = CreateSystem(root.transform, "IceLanceCore", lanceAlpha, 15);
            ConfigureBase(lanceCore, 0.7f, 0.51f, 9.1f, 1f, 2, Color.white);
            ParticleSystem.MainModule lanceCoreMain = lanceCore.main;
            lanceCoreMain.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.52f);
            lanceCoreMain.startSpeed = new ParticleSystem.MinMaxCurve(8.95f, 9.15f);
            SetStartSize3D(lanceCore, 3.55f, 0.86f);
            ConfigureCone(
                lanceCore,
                new Vector3(0f, 90f, 0f),
                0.2f,
                0.01f,
                new Vector3(-4.6f, 0.72f, 0f));
            SetBursts(lanceCore, new ParticleSystem.Burst(0f, 1));
            SetColorOverLifetime(
                lanceCore,
                Color.white,
                new Color(0.45f, 0.78f, 1f),
                1f,
                0f);
            SetDeterministic(lanceCore, 7002);

            ParticleSystem lanceWake = CreateSystem(root.transform, "LanceWakeShards", pixelShard, 13);
            ConfigureBase(lanceWake, 0.8f, 0.4f, 7.6f, 0.25f, 36, Color.white);
            ParticleSystem.MainModule lanceWakeMain = lanceWake.main;
            lanceWakeMain.startLifetime = new ParticleSystem.MinMaxCurve(0.18f, 0.46f);
            lanceWakeMain.startSpeed = new ParticleSystem.MinMaxCurve(6.5f, 8.8f);
            lanceWakeMain.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.3f);
            lanceWakeMain.startRotation = new ParticleSystem.MinMaxCurve(-Mathf.PI, Mathf.PI);
            ConfigureCone(
                lanceWake,
                new Vector3(0f, 90f, 0f),
                8f,
                0.2f,
                new Vector3(-4.45f, 0.72f, 0f));
            SetBursts(lanceWake, new ParticleSystem.Burst(0f, 20));
            SetColorOverLifetime(
                lanceWake,
                new Color(0.72f, 0.9f, 1f),
                new Color(0.08f, 0.28f, 0.58f),
                0.9f,
                0f);
            ConfigureRotation(lanceWake, -5f, 5f);
            SetDeterministic(lanceWake, 7003);

            ParticleSystem frost = CreateSystem(root.transform, "GroundFrost", frostPatch, -5);
            frost.transform.localPosition = new Vector3(0f, 0.1f, 0f);
            ConfigureBase(frost, 2.7f, 2.05f, 0f, 1f, 2, Color.white);
            ParticleSystem.MainModule frostMain = frost.main;
            frostMain.startDelay = 0.48f;
            frostMain.startLifetime = 2.05f;
            SetStartSize3D(frost, 4.15f, 0.82f);
            SetBursts(frost, new ParticleSystem.Burst(0f, 1));
            SetSizeOverLifetime(
                frost,
                Curve((0f, 0.12f), (0.08f, 1f), (0.86f, 1f), (1f, 0.96f)));
            SetColorOverLifetime(
                frost,
                Color.white,
                new Color(0.2f, 0.48f, 0.78f),
                0.92f,
                0f);
            SetDeterministic(frost, 7004);

            ParticleSystem backCrown = CreateSystem(root.transform, "IceCrownBack", crownBack, -3);
            backCrown.transform.localPosition = new Vector3(0f, 1.24f, 0f);
            ConfigureBase(backCrown, 1.9f, 1.15f, 0f, 1f, 2, Color.white);
            ParticleSystem.MainModule backCrownMain = backCrown.main;
            backCrownMain.startDelay = 0.48f;
            backCrownMain.startLifetime = 1.15f;
            SetStartSize3D(backCrown, 3.7f, 2.75f);
            SetBursts(backCrown, new ParticleSystem.Burst(0f, 1));
            SetSizeOverLifetime(
                backCrown,
                Curve((0f, 0.08f), (0.12f, 1f), (0.78f, 1f), (1f, 0.94f)));
            SetColorOverLifetime(
                backCrown,
                Color.white,
                new Color(0.32f, 0.65f, 0.92f),
                1f,
                0f);
            SetDeterministic(backCrown, 7005);

            ParticleSystem frontCrown = CreateSystem(root.transform, "IceCrownFront", crownFront, 8);
            frontCrown.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            ConfigureBase(frontCrown, 1.95f, 1.2f, 0f, 1f, 2, Color.white);
            ParticleSystem.MainModule frontCrownMain = frontCrown.main;
            frontCrownMain.startDelay = 0.5f;
            frontCrownMain.startLifetime = 1.2f;
            SetStartSize3D(frontCrown, 3.55f, 1.35f);
            SetBursts(frontCrown, new ParticleSystem.Burst(0f, 1));
            SetSizeOverLifetime(
                frontCrown,
                Curve((0f, 0.06f), (0.11f, 1f), (0.76f, 1f), (1f, 0.92f)));
            SetColorOverLifetime(
                frontCrown,
                Color.white,
                new Color(0.28f, 0.58f, 0.86f),
                1f,
                0f);
            SetDeterministic(frontCrown, 7006);

            ParticleSystem contact = CreateSystem(root.transform, "FrozenContactGlint", pixelGlow, 16);
            contact.transform.localPosition = new Vector3(0f, 0.52f, 0f);
            ConfigureBase(contact, 0.8f, 0.2f, 0f, 1f, 5, Color.white);
            ParticleSystem.MainModule contactMain = contact.main;
            contactMain.startDelay = 0.47f;
            contactMain.startLifetime = new ParticleSystem.MinMaxCurve(0.12f, 0.2f);
            contactMain.startSize = new ParticleSystem.MinMaxCurve(0.6f, 1.25f);
            SetBursts(contact, new ParticleSystem.Burst(0f, 3));
            SetSizeOverLifetime(contact, Curve((0f, 0.12f), (0.22f, 1f), (1f, 0.1f)));
            SetColorOverLifetime(
                contact,
                Color.white,
                new Color(0.35f, 0.72f, 1f),
                1f,
                0f);
            SetDeterministic(contact, 7007);

            ParticleSystem impactShards = CreateSystem(root.transform, "ImpactIceNeedles", pixelShard, 12);
            ConfigureBase(impactShards, 1.2f, 0.55f, 5f, 0.28f, 60, Color.white);
            ParticleSystem.MainModule impactShardMain = impactShards.main;
            impactShardMain.startDelay = 0.48f;
            impactShardMain.startLifetime = new ParticleSystem.MinMaxCurve(0.28f, 0.68f);
            impactShardMain.startSpeed = new ParticleSystem.MinMaxCurve(2.8f, 7.4f);
            impactShardMain.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.48f);
            impactShardMain.startRotation = new ParticleSystem.MinMaxCurve(-0.6f, 0.6f);
            impactShardMain.gravityModifier = new ParticleSystem.MinMaxCurve(1.2f, 2.1f);
            ConfigureCone(impactShards, new Vector3(-90f, 0f, 0f), 70f, 0.36f, Vector3.zero);
            SetBursts(impactShards, new ParticleSystem.Burst(0f, 34));
            SetColorOverLifetime(
                impactShards,
                new Color(0.78f, 0.94f, 1f),
                new Color(0.08f, 0.3f, 0.62f),
                1f,
                0f);
            impactShards.GetComponent<ParticleSystemRenderer>().alignment =
                ParticleSystemRenderSpace.Velocity;
            SetDeterministic(impactShards, 7008);

            ParticleSystem breakShards = CreateSystem(root.transform, "BreakingIceShards", pixelShard, 11);
            ConfigureBase(breakShards, 2.4f, 0.72f, 3.3f, 0.32f, 40, Color.white);
            ParticleSystem.MainModule breakShardMain = breakShards.main;
            breakShardMain.startDelay = 1.42f;
            breakShardMain.startLifetime = new ParticleSystem.MinMaxCurve(0.42f, 0.9f);
            breakShardMain.startSpeed = new ParticleSystem.MinMaxCurve(1.6f, 4.8f);
            breakShardMain.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.58f);
            breakShardMain.startRotation = new ParticleSystem.MinMaxCurve(-Mathf.PI, Mathf.PI);
            breakShardMain.gravityModifier = new ParticleSystem.MinMaxCurve(2f, 3.2f);
            ConfigureCone(
                breakShards,
                new Vector3(-90f, 0f, 0f),
                76f,
                1.05f,
                new Vector3(0f, 0.35f, 0f));
            SetBursts(breakShards, new ParticleSystem.Burst(0f, 20));
            SetColorOverLifetime(
                breakShards,
                new Color(0.68f, 0.88f, 1f),
                new Color(0.05f, 0.18f, 0.4f),
                1f,
                0f);
            ConfigureRotation(breakShards, -6f, 6f);
            SetDeterministic(breakShards, 7009);

            ParticleSystem snow = CreateSystem(root.transform, "SnowCrystals", snowflake, 10);
            ConfigureBase(snow, 2.7f, 1.1f, 0f, 0.2f, 36, Color.white);
            ParticleSystem.MainModule snowMain = snow.main;
            snowMain.startDelay = 0.68f;
            snowMain.startLifetime = new ParticleSystem.MinMaxCurve(0.72f, 1.35f);
            snowMain.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.3f);
            snowMain.startRotation = new ParticleSystem.MinMaxCurve(-Mathf.PI, Mathf.PI);
            snowMain.gravityModifier = new ParticleSystem.MinMaxCurve(0.03f, 0.12f);
            ParticleSystem.ShapeModule snowShape = snow.shape;
            snowShape.enabled = true;
            snowShape.shapeType = ParticleSystemShapeType.Box;
            snowShape.position = new Vector3(0f, 1.25f, 0f);
            snowShape.scale = new Vector3(3.4f, 2.2f, 0.1f);
            SetBursts(
                snow,
                new ParticleSystem.Burst(0f, 8),
                new ParticleSystem.Burst(0.38f, 7),
                new ParticleSystem.Burst(0.76f, 5));
            ParticleSystem.VelocityOverLifetimeModule snowVelocity = snow.velocityOverLifetime;
            snowVelocity.enabled = true;
            snowVelocity.space = ParticleSystemSimulationSpace.Local;
            snowVelocity.x = new ParticleSystem.MinMaxCurve(-0.24f, 0.24f);
            snowVelocity.y = new ParticleSystem.MinMaxCurve(-0.34f, -0.08f);
            snowVelocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);
            ConfigureRotation(snow, -0.8f, 0.8f);
            SetColorOverLifetime(
                snow,
                Color.white,
                new Color(0.28f, 0.62f, 0.95f),
                0.9f,
                0f);
            SetDeterministic(snow, 7010);

            ParticleSystem mist = CreateSystem(root.transform, "PixelColdMist", pixelSmoke, -4);
            mist.transform.localPosition = new Vector3(0f, 1.25f, 0f);
            ConfigureBase(mist, 2.5f, 1.35f, 0.55f, 0.9f, 30, Color.white);
            ParticleSystem.MainModule mistMain = mist.main;
            mistMain.startDelay = 0.54f;
            mistMain.startLifetime = new ParticleSystem.MinMaxCurve(0.85f, 1.6f);
            mistMain.startSpeed = new ParticleSystem.MinMaxCurve(0.18f, 0.72f);
            mistMain.startSize = new ParticleSystem.MinMaxCurve(0.38f, 0.9f);
            mistMain.startRotation = new ParticleSystem.MinMaxCurve(-Mathf.PI, Mathf.PI);
            ConfigureCircle(mist, 0.8f);
            SetBursts(mist, new ParticleSystem.Burst(0f, 16));
            ConfigureNoise(mist, 0.22f, 0.52f);
            SetColorOverLifetime(
                mist,
                new Color(0.12f, 0.32f, 0.58f),
                new Color(0.025f, 0.08f, 0.18f),
                0.34f,
                0f);
            SetSizeOverLifetime(mist, Curve((0f, 0.45f), (0.48f, 1f), (1f, 1.15f)));
            SetDeterministic(mist, 7011);

            return SavePrefab(root, IceLancePrefabPath);
        }

        private static GameObject CreateEffectRoot(string objectName, string skillId)
        {
            GameObject root = new GameObject(objectName);
            SkillParticleEffect effect = root.AddComponent<SkillParticleEffect>();
            SerializedObject serializedEffect = new SerializedObject(effect);
            serializedEffect.FindProperty("skillId").stringValue = skillId;
            serializedEffect.FindProperty("playOnEnable").boolValue = true;
            serializedEffect.FindProperty("destroyWhenFinished").boolValue = true;
            serializedEffect.ApplyModifiedPropertiesWithoutUndo();
            return root;
        }

        private static ParticleSystem CreateSystem(
            Transform parent,
            string objectName,
            Material material,
            int sortingOrder)
        {
            GameObject child = new GameObject(objectName, typeof(ParticleSystem));
            child.transform.SetParent(parent, false);

            ParticleSystem particleSystem = child.GetComponent<ParticleSystem>();
            ParticleSystemRenderer renderer = child.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.sortingOrder = sortingOrder;
            return particleSystem;
        }

        private static void ConfigureBase(
            ParticleSystem system,
            float duration,
            float lifetime,
            float speed,
            float size,
            int maxParticles,
            Color color)
        {
            ParticleSystem.MainModule main = system.main;
            main.duration = duration;
            main.loop = false;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            main.startLifetime = lifetime;
            main.startSpeed = speed;
            main.startSize = size;
            main.startColor = color;
            main.maxParticles = maxParticles;
            main.stopAction = ParticleSystemStopAction.None;

            ParticleSystem.EmissionModule emission = system.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;

            ParticleSystem.ShapeModule shape = system.shape;
            shape.enabled = false;
        }

        private static void ConfigureCone(
            ParticleSystem system,
            Vector3 rotation,
            float angle,
            float radius,
            Vector3 position)
        {
            ParticleSystem.ShapeModule shape = system.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = angle;
            shape.radius = radius;
            shape.rotation = rotation;
            shape.position = position;
        }

        private static void ConfigureCircle(ParticleSystem system, float radius)
        {
            ParticleSystem.ShapeModule shape = system.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = radius;
            shape.arc = 360f;
            shape.randomDirectionAmount = 0.18f;
        }

        private static void ConfigureNoise(ParticleSystem system, float strength, float frequency)
        {
            ParticleSystem.NoiseModule noise = system.noise;
            noise.enabled = true;
            noise.quality = ParticleSystemNoiseQuality.High;
            noise.strength = strength;
            noise.frequency = frequency;
            noise.scrollSpeed = 0.35f;
            noise.damping = true;
        }

        private static void ConfigureRotation(ParticleSystem system, float minimum, float maximum)
        {
            ParticleSystem.RotationOverLifetimeModule rotation = system.rotationOverLifetime;
            rotation.enabled = true;
            rotation.z = new ParticleSystem.MinMaxCurve(minimum, maximum);
        }

        private static void ConfigureVelocity(ParticleSystem system, float x, float y)
        {
            ParticleSystem.VelocityOverLifetimeModule velocity = system.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            velocity.x = x;
            velocity.y = y;
        }

        private static void SetStartSize3D(ParticleSystem system, float width, float height)
        {
            ParticleSystem.MainModule main = system.main;
            main.startSize3D = true;
            main.startSizeX = width;
            main.startSizeY = height;
            main.startSizeZ = 1f;
        }

        private static void SetDeterministic(ParticleSystem system, uint seed)
        {
            system.useAutoRandomSeed = false;
            system.randomSeed = seed;
        }

        private static void SetBursts(ParticleSystem system, params ParticleSystem.Burst[] bursts)
        {
            ParticleSystem.EmissionModule emission = system.emission;
            emission.SetBursts(bursts);
        }

        private static void SetFade(ParticleSystem system, float startAlpha, float endAlpha)
        {
            Color start = system.main.startColor.color;
            SetColorOverLifetime(system, start, start, startAlpha, endAlpha);
        }

        private static void SetColorOverLifetime(
            ParticleSystem system,
            Color start,
            Color end,
            float startAlpha,
            float endAlpha)
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(start, 0f),
                    new GradientColorKey(end, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(startAlpha, 0f),
                    new GradientAlphaKey(startAlpha, 0.62f),
                    new GradientAlphaKey(endAlpha, 1f)
                });

            ParticleSystem.ColorOverLifetimeModule color = system.colorOverLifetime;
            color.enabled = true;
            color.color = new ParticleSystem.MinMaxGradient(gradient);
        }

        private static void SetSizeOverLifetime(ParticleSystem system, AnimationCurve curve)
        {
            ParticleSystem.SizeOverLifetimeModule size = system.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, curve);
        }

        private static AnimationCurve Curve(params (float time, float value)[] keys)
        {
            Keyframe[] keyframes = new Keyframe[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                keyframes[i] = new Keyframe(keys[i].time, keys[i].value);
            }

            return new AnimationCurve(keyframes);
        }

        private static GameObject SavePrefab(GameObject root, string path)
        {
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static void CreatePreviewScene(
            GameObject fireballPrefab,
            GameObject earthPrefab,
            GameObject nailDrivingPrefab,
            GameObject plagueMagicPrefab,
            GameObject iceLancePrefab)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);

            GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 1.1f, -10f);
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.008f, 0.012f, 0.025f);
            camera.orthographic = true;
            camera.orthographicSize = 6.2f;
            camera.allowHDR = true;

            GameObject fireballObject = (GameObject)PrefabUtility.InstantiatePrefab(fireballPrefab, scene);
            fireballObject.transform.position = new Vector3(-8f, -0.7f, 0f);
            SkillParticleEffect fireball = fireballObject.GetComponent<SkillParticleEffect>();
            DisableAutoDestroy(fireball);

            GameObject earthObject = (GameObject)PrefabUtility.InstantiatePrefab(earthPrefab, scene);
            earthObject.transform.position = new Vector3(-4f, -0.7f, 0f);
            SkillParticleEffect earth = earthObject.GetComponent<SkillParticleEffect>();
            DisableAutoDestroy(earth);

            GameObject nailDrivingObject =
                (GameObject)PrefabUtility.InstantiatePrefab(nailDrivingPrefab, scene);
            nailDrivingObject.transform.position = new Vector3(0f, -0.7f, 0f);
            SkillParticleEffect nailDriving = nailDrivingObject.GetComponent<SkillParticleEffect>();
            DisableAutoDestroy(nailDriving);

            GameObject plagueMagicObject =
                (GameObject)PrefabUtility.InstantiatePrefab(plagueMagicPrefab, scene);
            plagueMagicObject.transform.position = new Vector3(4f, -0.7f, 0f);
            SkillParticleEffect plagueMagic = plagueMagicObject.GetComponent<SkillParticleEffect>();
            DisableAutoDestroy(plagueMagic);

            GameObject iceLanceObject =
                (GameObject)PrefabUtility.InstantiatePrefab(iceLancePrefab, scene);
            iceLanceObject.transform.position = new Vector3(8f, -0.7f, 0f);
            SkillParticleEffect iceLance = iceLanceObject.GetComponent<SkillParticleEffect>();
            DisableAutoDestroy(iceLance);

            GameObject controllerObject = new GameObject("Preview Controller");
            SkillVfxPreviewController controller = controllerObject.AddComponent<SkillVfxPreviewController>();
            SerializedObject serializedController = new SerializedObject(controller);
            serializedController.FindProperty("fireballExplosion").objectReferenceValue = fireball;
            serializedController.FindProperty("earthMagic").objectReferenceValue = earth;
            serializedController.FindProperty("nailDriving").objectReferenceValue = nailDriving;
            serializedController.FindProperty("plagueMagic").objectReferenceValue = plagueMagic;
            serializedController.FindProperty("iceLance").objectReferenceValue = iceLance;
            serializedController.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, PreviewScenePath);
            EditorSceneManager.CloseScene(scene, true);
        }

        private static void UpdatePreviewScene(
            GameObject nailDrivingPrefab,
            GameObject plagueMagicPrefab,
            GameObject iceLancePrefab)
        {
            Scene scene = SceneManager.GetSceneByPath(PreviewScenePath);
            bool closeWhenFinished = !scene.IsValid() || !scene.isLoaded;
            if (closeWhenFinished)
            {
                scene = EditorSceneManager.OpenScene(PreviewScenePath, OpenSceneMode.Additive);
            }

            SkillParticleEffect fireball = null;
            SkillParticleEffect earth = null;
            SkillParticleEffect nailDriving = null;
            SkillParticleEffect plagueMagic = null;
            SkillParticleEffect iceLance = null;
            SkillVfxPreviewController controller = null;

            foreach (GameObject rootObject in scene.GetRootGameObjects())
            {
                SkillParticleEffect effect = rootObject.GetComponent<SkillParticleEffect>();
                if (effect != null)
                {
                    switch (effect.SkillId)
                    {
                        case "skill_001":
                            fireball = effect;
                            break;
                        case "skill_002":
                            earth = effect;
                            break;
                        case "skill_004":
                            nailDriving = effect;
                            break;
                        case "skill_006":
                            plagueMagic = effect;
                            break;
                        case "skill_007":
                            iceLance = effect;
                            break;
                    }
                }

                controller ??= rootObject.GetComponent<SkillVfxPreviewController>();
            }

            if (nailDriving == null)
            {
                GameObject nailDrivingObject =
                    (GameObject)PrefabUtility.InstantiatePrefab(nailDrivingPrefab, scene);
                nailDriving = nailDrivingObject.GetComponent<SkillParticleEffect>();
                DisableAutoDestroy(nailDriving);
            }

            if (plagueMagic == null)
            {
                GameObject plagueMagicObject =
                    (GameObject)PrefabUtility.InstantiatePrefab(plagueMagicPrefab, scene);
                plagueMagic = plagueMagicObject.GetComponent<SkillParticleEffect>();
                DisableAutoDestroy(plagueMagic);
            }

            if (iceLance == null)
            {
                GameObject iceLanceObject =
                    (GameObject)PrefabUtility.InstantiatePrefab(iceLancePrefab, scene);
                iceLance = iceLanceObject.GetComponent<SkillParticleEffect>();
                DisableAutoDestroy(iceLance);
            }

            if (fireball != null)
            {
                fireball.transform.position = new Vector3(-8f, -0.7f, 0f);
            }

            if (earth != null)
            {
                earth.transform.position = new Vector3(-4f, -0.7f, 0f);
            }

            nailDriving.transform.position = new Vector3(0f, -0.7f, 0f);
            plagueMagic.transform.position = new Vector3(4f, -0.7f, 0f);
            iceLance.transform.position = new Vector3(8f, -0.7f, 0f);

            Camera previewCamera = null;
            foreach (GameObject rootObject in scene.GetRootGameObjects())
            {
                previewCamera ??= rootObject.GetComponent<Camera>();
            }

            if (previewCamera != null)
            {
                previewCamera.orthographicSize = 6.2f;
            }

            if (controller == null)
            {
                GameObject controllerObject = new GameObject("Preview Controller");
                SceneManager.MoveGameObjectToScene(controllerObject, scene);
                controller = controllerObject.AddComponent<SkillVfxPreviewController>();
            }

            SerializedObject serializedController = new SerializedObject(controller);
            serializedController.FindProperty("fireballExplosion").objectReferenceValue = fireball;
            serializedController.FindProperty("earthMagic").objectReferenceValue = earth;
            serializedController.FindProperty("nailDriving").objectReferenceValue = nailDriving;
            serializedController.FindProperty("plagueMagic").objectReferenceValue = plagueMagic;
            serializedController.FindProperty("iceLance").objectReferenceValue = iceLance;
            serializedController.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            if (closeWhenFinished)
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static void DisableAutoDestroy(SkillParticleEffect effect)
        {
            SerializedObject serializedEffect = new SerializedObject(effect);
            serializedEffect.FindProperty("destroyWhenFinished").boolValue = false;
            serializedEffect.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Material CreateMaterial(string fileName, Texture2D texture, BlendMode destinationBlend)
        {
            string path = MaterialPath + "/" + fileName;
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(ShaderPath);
            if (shader == null)
            {
                throw new InvalidOperationException($"Skill particle shader was not found at '{ShaderPath}'.");
            }

            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.shader = shader;
            material.SetTexture("_MainTex", texture);
            material.SetColor("_Color", Color.white);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)destinationBlend);
            material.renderQueue = (int)RenderQueue.Transparent;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void CreateTextures()
        {
            WriteTexture("Glow.png", (x, y) =>
            {
                float radius = Mathf.Sqrt(x * x + y * y);
                float alpha = Mathf.Pow(Mathf.Clamp01(1f - radius), 2.2f);
                return new Color(1f, 1f, 1f, alpha);
            });

            WriteTexture("Ring.png", (x, y) =>
            {
                float radius = Mathf.Sqrt(x * x + y * y);
                float alpha = Mathf.Clamp01(1f - Mathf.Abs(radius - 0.68f) / 0.11f);
                alpha *= alpha;
                return new Color(1f, 1f, 1f, alpha);
            });

            WriteTexture("Rock.png", (x, y) =>
            {
                float halfWidth = Mathf.Lerp(0.08f, 0.55f, 1f - Mathf.Abs(y));
                float edge = Mathf.Clamp01((halfWidth - Mathf.Abs(x)) * 28f);
                float cap = Mathf.Clamp01((1f - Mathf.Abs(y)) * 24f);
                float alpha = edge * cap;
                float facet = x < -0.08f ? 0.72f : x < 0.18f ? 1f : 0.52f;
                return new Color(facet, facet, facet, alpha);
            });

            WriteTexture("Smoke.png", (x, y) =>
            {
                float alpha = Mathf.Max(
                    Blob(x, y, -0.28f, -0.05f, 0.56f),
                    Blob(x, y, 0.25f, -0.08f, 0.58f));
                alpha = Mathf.Max(alpha, Blob(x, y, -0.05f, 0.26f, 0.62f));
                alpha *= Mathf.Clamp01(1f - Mathf.Sqrt(x * x + y * y) * 0.72f);
                return new Color(1f, 1f, 1f, alpha * 0.82f);
            });

            WriteTexture("Skull.png", (x, y) =>
            {
                float cranium = SoftEllipse(x, y - 0.2f, 0.58f, 0.68f, 18f);
                float jawWidth = Mathf.Lerp(0.22f, 0.4f, Mathf.InverseLerp(-0.72f, -0.12f, y));
                float jaw = Mathf.Clamp01((jawWidth - Mathf.Abs(x)) * 28f);
                jaw *= Mathf.Clamp01((y + 0.76f) * 24f);
                jaw *= Mathf.Clamp01((-0.06f - y) * 22f);

                float silhouette = Mathf.Max(cranium, jaw);
                float eyes = Mathf.Max(
                    SoftEllipse(x + 0.21f, y - 0.18f, 0.15f, 0.18f, 30f),
                    SoftEllipse(x - 0.21f, y - 0.18f, 0.15f, 0.18f, 30f));
                float nose = SoftEllipse(x, y + 0.06f, 0.085f, 0.13f, 35f);
                float holes = Mathf.Max(eyes, nose);
                float alpha = silhouette * (1f - holes * 0.96f);

                float teeth = y < -0.28f
                    ? Mathf.Lerp(0.62f, 1f, Mathf.Abs(Mathf.Sin((x + 0.5f) * 36f)))
                    : 1f;
                float facet = x < -0.08f ? 0.66f : x < 0.14f ? 1f : 0.74f;
                return new Color(facet * teeth, facet * teeth, facet * teeth, alpha);
            });

            WriteTexture("PlaguePool.png", (x, y) =>
            {
                float radius = Mathf.Sqrt(x * x + y * y);
                float angle = Mathf.Atan2(y, x);
                float edgeRadius = 0.72f +
                                   Mathf.Sin(angle * 5f + 0.6f) * 0.08f +
                                   Mathf.Sin(angle * 9f - 0.4f) * 0.045f +
                                   Mathf.Sin(angle * 13f) * 0.025f;
                float alpha = Mathf.Clamp01((edgeRadius - radius) * 13f);
                float bubbles = Mathf.Max(
                    Blob(x, y, -0.28f, 0.08f, 0.13f),
                    Blob(x, y, 0.24f, -0.18f, 0.1f));
                bubbles = Mathf.Max(bubbles, Blob(x, y, 0.1f, 0.25f, 0.08f));
                alpha *= 1f - bubbles * 0.72f;
                float vein = 0.68f + 0.32f * Mathf.Sin(angle * 7f + radius * 24f);
                return new Color(vein, vein, vein, alpha);
            });
        }

        private static void CreateFireballPixelTextures()
        {
            WritePixelTexture("PixelMeteor.png", 32, 64, (x, y) =>
            {
                const int centerX = 15;
                int distanceX = Math.Abs(x - centerX);
                bool meteorHead = false;
                int headHalfWidth = 0;
                if (y >= 2 && y <= 27)
                {
                    headHalfWidth = y < 9
                        ? 2 + (y - 2) / 2
                        : y < 19
                            ? 9 - ((x + y) % 11 == 0 ? 1 : 0)
                            : Math.Max(2, 8 - (y - 19) / 2);
                    meteorHead = distanceX <= headHalfWidth;
                }

                int tailShift = y < 42 ? (y / 9) % 2 : -((y / 7) % 2);
                int tailHalfWidth = y < 34
                    ? 5
                    : y < 47
                        ? 4
                        : Math.Max(0, (64 - y) / 5);
                bool centerTail = y >= 23 && y <= 62 &&
                                  Math.Abs(x - centerX - tailShift) <= tailHalfWidth;
                bool leftTongue = y >= 24 && y <= 52 &&
                                  Math.Abs(x - (10 - (y - 24) / 7)) <= 1 &&
                                  (x + y) % 5 != 0;
                bool rightTongue = y >= 25 && y <= 57 &&
                                   Math.Abs(x - (21 + (y - 25) / 9)) <= 1 &&
                                   (x * 2 + y) % 6 != 0;
                if (!meteorHead && !centerTail && !leftTongue && !rightTongue)
                {
                    return Color.clear;
                }

                float value = 0.48f;
                if (meteorHead)
                {
                    value = distanceX <= Math.Max(1, headHalfWidth - 5)
                        ? 1f
                        : distanceX <= Math.Max(1, headHalfWidth - 2)
                            ? 0.82f
                            : 0.5f;
                }
                else if (centerTail && distanceX <= 1)
                {
                    value = 0.9f;
                }
                else if ((x + y) % 4 == 0)
                {
                    value = 0.68f;
                }

                return new Color(value, value, value, 1f);
            });

            WritePixelTexture("PixelFlame.png", (x, y) =>
            {
                int centerX = 15 + (y > 18 ? (y - 18) / 7 : 0);
                int halfWidth = y < 3
                    ? -1
                    : y < 9
                        ? 8 - (y % 3 == 0 ? 1 : 0)
                        : y < 20
                            ? Math.Max(3, 8 - (y - 9) / 2)
                            : Math.Max(0, (31 - y) / 3);
                bool body = halfWidth >= 0 && Math.Abs(x - centerX) <= halfWidth;
                bool sideTongue = y >= 7 && y <= 19 &&
                                  Math.Abs(x - (8 - (y - 7) / 5)) <= 1;
                if (!body && !sideTongue)
                {
                    return Color.clear;
                }

                int distanceX = Math.Abs(x - centerX);
                float value = distanceX <= Math.Max(1, halfWidth - 4)
                    ? 1f
                    : distanceX <= Math.Max(1, halfWidth - 2)
                        ? 0.78f
                        : 0.46f;
                if ((x * 3 + y) % 13 == 0)
                {
                    value *= 0.72f;
                }

                return new Color(value, value, value, 1f);
            });
        }

        private static void CreatePlaguePixelTextures()
        {
            WritePixelTexture("Skull.png", (x, y) =>
            {
                const int centerX = 15;
                int distanceX = Math.Abs(x - centerX);
                bool cranium = y >= 10 && y <= 28 &&
                                distanceX <= (y < 14 ? 7 + (y - 10) : 11);
                bool templeCut = y >= 10 && y <= 14 && distanceX >= 8;
                bool jaw = y >= 3 && y <= 13 && distanceX <= (y < 7 ? 5 : 7);
                bool eye = y >= 15 && y <= 20 &&
                           (Math.Abs(x - 10) <= 2 || Math.Abs(x - 21) <= 2);
                bool nose = y >= 10 && y <= 14 && Math.Abs(x - centerX) <= 1;
                bool toothGap = y >= 4 && y <= 8 && (x - 6) % 3 == 0;
                if ((!cranium || templeCut) && !jaw || eye || nose || toothGap)
                {
                    return Color.clear;
                }

                float value = x < centerX - 2 ? 0.48f : x <= centerX + 2 ? 1f : 0.7f;
                if ((x + y) % 9 == 0)
                {
                    value *= 0.68f;
                }

                return new Color(value, value, value, 1f);
            });

            WritePixelTexture("PlaguePool.png", 64, 32, (x, y) =>
            {
                float nx = (x - 31.5f) / 31.5f;
                float ny = (y - 13.5f) / 12.5f;
                float radius = nx * nx + ny * ny;
                float jaggedEdge = 0.94f + ((x * 7 + y * 11) % 9 - 4) * 0.018f;
                bool pool = radius <= jaggedEdge && y >= 2 && y <= 27;
                bool bubbleHole =
                    ((x - 18) * (x - 18) + (y - 15) * (y - 15) <= 7) ||
                    ((x - 44) * (x - 44) + (y - 10) * (y - 10) <= 5) ||
                    ((x - 35) * (x - 35) + (y - 20) * (y - 20) <= 3);
                if (!pool || bubbleHole)
                {
                    return Color.clear;
                }

                bool rim = radius >= 0.68f;
                bool vein = (x + y * 3) % 11 <= 1 || (x * 2 - y) % 17 == 0;
                float value = rim ? 0.56f : vein ? 1f : 0.74f;
                return new Color(value, value, value, 1f);
            });
        }

        private static void CreateNailDrivingTextures()
        {
            WritePixelTexture("Nail.png", (x, y) =>
            {
                const int center = 15;
                int distanceX = Math.Abs(x - center);
                int bodyHalfWidth = y < 4 ? 0 : y < 9 ? 1 : y < 17 ? 2 : 3;
                bool body = y >= 1 && y <= 21 && distanceX <= bodyHalfWidth;

                int headHalfWidth = y == 20 ? 5 : y == 21 ? 8 : y == 22 ? 10 : y == 23 ? 8 : 5;
                bool head = y >= 20 && y <= 24 && distanceX <= headHalfWidth;

                int crownHalfWidth = y >= 25 ? Math.Max(0, (31 - y) / 2) : -1;
                bool crown = y >= 25 && y <= 31 && distanceX <= crownHalfWidth;
                if (!body && !head && !crown)
                {
                    return Color.clear;
                }

                float facet = x < center - 1 ? 0.42f : x <= center ? 1f : 0.64f;
                if ((x + y) % 7 == 0)
                {
                    facet *= 0.72f;
                }

                return new Color(facet, facet, facet, 1f);
            });

            WritePixelTexture("PixelGlow.png", (x, y) =>
            {
                const int center = 15;
                int distanceX = Math.Abs(x - center);
                int distanceY = Math.Abs(y - center);
                float alpha = 0f;

                if (distanceX <= 2 && distanceY <= 2)
                {
                    alpha = 1f;
                }
                else if ((distanceX <= 1 && distanceY <= 10) ||
                         (distanceY <= 1 && distanceX <= 10))
                {
                    alpha = 0.82f;
                }
                else if (distanceX == distanceY && distanceX <= 6)
                {
                    alpha = 0.58f;
                }
                else if (distanceX + distanceY == 11 && (x + y) % 3 == 0)
                {
                    alpha = 0.4f;
                }

                return new Color(1f, 1f, 1f, alpha);
            });

            WritePixelTexture("PixelRing.png", (x, y) =>
            {
                float distanceX = x - 15.5f;
                float distanceY = y - 15.5f;
                float squaredDistance = distanceX * distanceX + distanceY * distanceY;
                bool ring = squaredDistance >= 88f && squaredDistance <= 145f;
                bool gap = (x + y * 2) % 13 == 0 || (x * 3 + y) % 17 == 0;
                float alpha = ring && !gap ? 0.9f : 0f;
                return new Color(1f, 1f, 1f, alpha);
            });

            WritePixelTexture("PixelShard.png", (x, y) =>
            {
                int center = y < 10 ? 14 : y < 22 ? 15 : 16;
                int halfWidth = y < 3
                    ? 0
                    : y < 7
                        ? 1 + (y - 3) / 2
                        : y < 20
                            ? 5 - (y % 3 == 0 ? 1 : 0)
                            : Math.Max(0, (30 - y) / 2);
                if (y < 1 || y > 30 || Math.Abs(x - center) > halfWidth)
                {
                    return Color.clear;
                }

                float facet = x < center - 1 ? 0.38f : x <= center + 1 ? 1f : 0.58f;
                if ((x + y) % 5 == 0)
                {
                    facet *= 0.68f;
                }

                return new Color(facet, facet, facet, 1f);
            });

            WritePixelTexture("PixelSmoke.png", (x, y) =>
            {
                int left = Math.Abs(x - 10) + Math.Abs(y - 12);
                int right = Math.Abs(x - 22) + Math.Abs(y - 11);
                int top = Math.Abs(x - 16) + Math.Abs(y - 21);
                int distance = Math.Min(left, Math.Min(right, top));
                float alpha = distance <= 5 ? 0.82f : distance <= 8 ? 0.52f : 0f;
                if (alpha > 0f && (x + y) % 6 == 0)
                {
                    alpha *= 0.55f;
                }

                return new Color(1f, 1f, 1f, alpha);
            });
        }

        private static void CreateIceLanceTextures()
        {
            WritePixelTexture("IceLance.png", 64, 32, SampleIceLancePixel);
            WritePixelTexture("IceCrownBack.png", 64, 64, SampleIceCrownBackPixel);
            WritePixelTexture("IceCrownFront.png", 64, 48, SampleIceCrownFrontPixel);
            WritePixelTexture("FrostPatch.png", 64, 32, SampleFrostPatchPixel);
            WritePixelTexture("Snowflake.png", SampleSnowflakePixel);
        }

        private static Color SampleIceLancePixel(int x, int y)
        {
            const int centerY = 15;
            int distanceY = Math.Abs(y - centerY);
            int halfHeight = -1;
            if (x >= 10 && x <= 63)
            {
                halfHeight = x < 22
                    ? 2 + (x - 10) / 3
                    : x < 43
                        ? 6 + (x % 8 == 0 ? 1 : 0)
                        : Math.Max(0, (63 - x + 2) / 3);
            }

            bool body = halfHeight >= 0 && distanceY <= halfHeight;
            if (body && distanceY == halfHeight && (x * 3 + y) % 7 == 0)
            {
                body = false;
            }

            bool upperFeather = x <= 34 &&
                                Math.Abs(y - (3 + x / 3)) <= (x < 12 ? 0 : 1) &&
                                (x + y) % 5 != 0;
            bool lowerFeather = x <= 34 &&
                                Math.Abs(y - (28 - x / 3)) <= (x < 12 ? 0 : 1) &&
                                (x * 2 + y) % 6 != 0;
            bool upperInner = x >= 3 && x <= 38 &&
                              Math.Abs(y - (8 + x / 6)) <= 1 &&
                              (x + y * 2) % 7 != 0;
            bool lowerInner = x >= 3 && x <= 38 &&
                              Math.Abs(y - (22 - x / 6)) <= 1 &&
                              (x * 3 + y) % 8 != 0;
            bool detachedChip = x < 26 &&
                                ((x * 5 + y * 3) % 23 == 0 ||
                                 (x * 7 + y * 2) % 29 == 0) &&
                                distanceY <= 13;

            if (!body && !upperFeather && !lowerFeather && !upperInner && !lowerInner && !detachedChip)
            {
                return Color.clear;
            }

            if (!body)
            {
                return detachedChip
                    ? new Color32(47, 120, 183, 255)
                    : new Color32(23, 71, 117, 255);
            }

            bool outline = distanceY >= Math.Max(0, halfHeight - 1) || x == 10;
            if (outline)
            {
                return y >= centerY
                    ? new Color32(11, 41, 69, 255)
                    : new Color32(112, 185, 246, 255);
            }

            if (distanceY <= 1)
            {
                return new Color32(232, 248, 255, 255);
            }

            if (y > centerY)
            {
                return distanceY <= 3
                    ? new Color32(168, 221, 255, 255)
                    : new Color32(112, 185, 246, 255);
            }

            return distanceY <= 3
                ? new Color32(47, 120, 183, 255)
                : new Color32(23, 71, 117, 255);
        }

        private static Color SampleIceCrownBackPixel(int x, int y)
        {
            Color pixel = Color.clear;
            pixel = OverlayPixel(pixel, SampleIceCrystalPixel(x, y, 8, 2, 1, 34, 7));
            pixel = OverlayPixel(pixel, SampleIceCrystalPixel(x, y, 55, 2, 63, 38, 7));
            pixel = OverlayPixel(pixel, SampleIceCrystalPixel(x, y, 18, 2, 10, 49, 8));
            pixel = OverlayPixel(pixel, SampleIceCrystalPixel(x, y, 46, 2, 54, 51, 8));
            pixel = OverlayPixel(pixel, SampleIceCrystalPixel(x, y, 26, 2, 23, 43, 6));
            pixel = OverlayPixel(pixel, SampleIceCrystalPixel(x, y, 39, 2, 42, 45, 6));
            pixel = OverlayPixel(pixel, SampleIceCrystalPixel(x, y, 32, 2, 32, 62, 10));
            return pixel;
        }

        private static Color SampleIceCrownFrontPixel(int x, int y)
        {
            Color pixel = Color.clear;
            pixel = OverlayPixel(pixel, SampleIceCrystalPixel(x, y, 5, 2, 0, 20, 5));
            pixel = OverlayPixel(pixel, SampleIceCrystalPixel(x, y, 58, 2, 63, 22, 5));
            pixel = OverlayPixel(pixel, SampleIceCrystalPixel(x, y, 13, 2, 9, 31, 7));
            pixel = OverlayPixel(pixel, SampleIceCrystalPixel(x, y, 51, 2, 56, 33, 7));
            pixel = OverlayPixel(pixel, SampleIceCrystalPixel(x, y, 23, 2, 20, 38, 8));
            pixel = OverlayPixel(pixel, SampleIceCrystalPixel(x, y, 42, 2, 45, 36, 8));
            pixel = OverlayPixel(pixel, SampleIceCrystalPixel(x, y, 32, 2, 32, 24, 7));
            return pixel;
        }

        private static Color SampleFrostPatchPixel(int x, int y)
        {
            float normalizedX = (x - 31.5f) / 30f;
            float normalizedY = (y - 7f) / 5.5f;
            float distance = normalizedX * normalizedX + normalizedY * normalizedY;
            bool patch = y >= 2 && y <= 14 && distance <= 1f;
            if (patch && distance > 0.72f && (x * 5 + y * 3) % 8 == 0)
            {
                patch = false;
            }

            Color pixel = Color.clear;
            if (patch)
            {
                bool edge = distance > 0.78f || y <= 3;
                bool crack = (Math.Abs(x - 31) + y * 2) % 17 <= 1;
                pixel = edge
                    ? new Color32(11, 41, 69, 255)
                    : crack
                        ? new Color32(232, 248, 255, 255)
                        : new Color32(47, 120, 183, 255);
            }

            pixel = OverlayPixel(pixel, SampleIceCrystalPixel(x, y, 8, 5, 3, 19, 4));
            pixel = OverlayPixel(pixel, SampleIceCrystalPixel(x, y, 20, 5, 17, 23, 5));
            pixel = OverlayPixel(pixel, SampleIceCrystalPixel(x, y, 44, 5, 47, 22, 5));
            pixel = OverlayPixel(pixel, SampleIceCrystalPixel(x, y, 56, 5, 62, 18, 4));
            return pixel;
        }

        private static Color SampleSnowflakePixel(int x, int y)
        {
            const int center = 15;
            int dx = x - center;
            int dy = y - center;
            int absoluteX = Math.Abs(dx);
            int absoluteY = Math.Abs(dy);
            bool centerCore = absoluteX <= 1 && absoluteY <= 1;
            bool primaryArm = (absoluteX <= 1 && absoluteY <= 12) ||
                              (absoluteY <= 1 && absoluteX <= 12) ||
                              (absoluteX == absoluteY && absoluteX <= 9);
            bool branch = (absoluteY == 5 || absoluteY == 9) &&
                          absoluteX >= 2 && absoluteX <= 4 ||
                          (absoluteX == 5 || absoluteX == 9) &&
                          absoluteY >= 2 && absoluteY <= 4;
            if (!centerCore && !primaryArm && !branch)
            {
                return Color.clear;
            }

            if (centerCore || absoluteX + absoluteY <= 3)
            {
                return new Color32(232, 248, 255, 255);
            }

            return (x + y) % 3 == 0
                ? new Color32(112, 185, 246, 255)
                : new Color32(168, 221, 255, 255);
        }

        private static Color SampleIceCrystalPixel(
            int x,
            int y,
            int baseX,
            int baseY,
            int tipX,
            int tipY,
            int baseHalfWidth)
        {
            if (y < baseY || y > tipY)
            {
                return Color.clear;
            }

            float progress = Mathf.InverseLerp(baseY, tipY, y);
            float centerX = Mathf.Lerp(baseX, tipX, progress);
            int halfWidth = Mathf.Max(0, Mathf.CeilToInt(baseHalfWidth * (1f - progress)));
            float delta = x - centerX;
            if (Mathf.Abs(delta) > halfWidth)
            {
                return Color.clear;
            }

            bool outline = Mathf.Abs(delta) >= Math.Max(0, halfWidth - 1) || y <= baseY + 1;
            if (outline)
            {
                return delta < 0f
                    ? new Color32(23, 71, 117, 255)
                    : new Color32(11, 41, 69, 255);
            }

            if (Mathf.Abs(delta) <= 1f && progress > 0.16f)
            {
                return new Color32(232, 248, 255, 255);
            }

            return delta < 0f
                ? new Color32(112, 185, 246, 255)
                : new Color32(47, 120, 183, 255);
        }

        private static Color OverlayPixel(Color current, Color next)
        {
            return next.a > 0f ? next : current;
        }

        private static float Blob(float x, float y, float centerX, float centerY, float radius)
        {
            float distance = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));
            return Mathf.Pow(Mathf.Clamp01(1f - distance / radius), 1.7f);
        }

        private static float SoftEllipse(
            float x,
            float y,
            float radiusX,
            float radiusY,
            float softness)
        {
            float distance = Mathf.Sqrt(
                x * x / (radiusX * radiusX) +
                y * y / (radiusY * radiusY));
            return Mathf.Clamp01((1f - distance) * softness);
        }

        private static void WriteTexture(string fileName, Func<float, float, Color> sample)
        {
            const int size = 128;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false, true);
            Color[] pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float normalizedX = x / (size - 1f) * 2f - 1f;
                    float normalizedY = y / (size - 1f) * 2f - 1f;
                    pixels[y * size + x] = sample(normalizedX, normalizedY);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(false, false);

            string assetPath = TexturePath + "/" + fileName;
            File.WriteAllBytes(Path.GetFullPath(assetPath), texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            importer.textureType = TextureImporterType.Default;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        private static void WritePixelTexture(string fileName, Func<int, int, Color> sample)
        {
            WritePixelTexture(fileName, 32, 32, sample);
        }

        private static void WritePixelTexture(
            string fileName,
            int width,
            int height,
            Func<int, int, Color> sample)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false, true);
            Color[] pixels = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    pixels[y * width + x] = sample(x, y);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(false, false);

            string assetPath = TexturePath + "/" + fileName;
            File.WriteAllBytes(Path.GetFullPath(assetPath), texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            importer.textureType = TextureImporterType.Default;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        private static Texture2D LoadTexture(string fileName)
        {
            return AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath + "/" + fileName);
        }

        private static void EnsureDirectories()
        {
            Directory.CreateDirectory(TexturePath);
            Directory.CreateDirectory(MaterialPath);
            Directory.CreateDirectory(PrefabPath);
        }
    }
}
