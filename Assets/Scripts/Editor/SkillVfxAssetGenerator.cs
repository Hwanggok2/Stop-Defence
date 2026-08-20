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
        private const string MegaExplosionPrefabPath = PrefabPath + "/Skill_008_MegaExplosion.prefab";
        private const string FlashbangPrefabPath = PrefabPath + "/Skill_010_Flashbang.prefab";

        [MenuItem("Tools/Skill VFX/Generate Skill Effects")]
        public static void GenerateAll()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[Skill VFX] Exit Play Mode before generating skill effects.");
                return;
            }

            EnsureDirectories();
            CreateNailDrivingTextures();
            CreateFireballPixelTextures();
            CreatePlaguePixelTextures();
            CreateIceLanceTextures();
            CreateMegaExplosionTextures();
            CreateFlashbangTextures();
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
            GameObject megaExplosionPrefab = CreateMegaExplosionAssets(
                pixelGlow,
                pixelRing,
                pixelShard,
                pixelSmoke,
                pixelFlame);
            GameObject flashbangPrefab = CreateFlashbangAssets(pixelGlow, pixelRing, pixelShard);
            if (File.Exists(PreviewScenePath))
            {
                UpdatePreviewScene(
                    nailDrivingPrefab,
                    plagueMagicPrefab,
                    iceLancePrefab,
                    megaExplosionPrefab,
                    flashbangPrefab);
            }
            else
            {
                CreatePreviewScene(
                    fireballPrefab,
                    earthPrefab,
                    nailDrivingPrefab,
                    plagueMagicPrefab,
                    iceLancePrefab,
                    megaExplosionPrefab,
                    flashbangPrefab);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log(
                $"[Skill VFX] Generated {FireballPrefabPath}, {EarthPrefabPath}, " +
                $"{NailDrivingPrefabPath}, {PlagueMagicPrefabPath}, {IceLancePrefabPath}, " +
                $"{MegaExplosionPrefabPath}, {FlashbangPrefabPath}, and {PreviewScenePath}.");
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
                GameObject flashbangPrefab =
                    AssetDatabase.LoadAssetAtPath<GameObject>(FlashbangPrefabPath);
                GameObject megaExplosionPrefab =
                    AssetDatabase.LoadAssetAtPath<GameObject>(MegaExplosionPrefabPath);
                UpdatePreviewScene(
                    nailDrivingPrefab,
                    plagueMagicPrefab,
                    iceLancePrefab,
                    megaExplosionPrefab,
                    flashbangPrefab);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Skill VFX] Generated {IceLancePrefabPath} and updated the preview scene.");
        }

        [MenuItem("Tools/Skill VFX/Generate Mega Explosion Effect")]
        public static void GenerateMegaExplosion()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[Skill VFX] Exit Play Mode before generating skill effects.");
                return;
            }

            EnsureDirectories();
            if (LoadTexture("PixelGlow.png") == null ||
                LoadTexture("PixelRing.png") == null ||
                LoadTexture("PixelShard.png") == null ||
                LoadTexture("PixelSmoke.png") == null ||
                LoadTexture("PixelFlame.png") == null)
            {
                CreateNailDrivingTextures();
                CreateFireballPixelTextures();
            }

            CreateMegaExplosionTextures();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            Material pixelGlow = CreateMaterial(
                "PixelGlow_Additive.mat",
                LoadTexture("PixelGlow.png"),
                BlendMode.One);
            Material pixelRing = CreateMaterial(
                "PixelRing_Additive.mat",
                LoadTexture("PixelRing.png"),
                BlendMode.One);
            Material pixelShard = CreateMaterial(
                "PixelShard_Alpha.mat",
                LoadTexture("PixelShard.png"),
                BlendMode.OneMinusSrcAlpha);
            Material pixelSmoke = CreateMaterial(
                "PixelSmoke_Alpha.mat",
                LoadTexture("PixelSmoke.png"),
                BlendMode.OneMinusSrcAlpha);
            Material pixelFlame = CreateMaterial(
                "PixelFlame_Additive.mat",
                LoadTexture("PixelFlame.png"),
                BlendMode.One);

            GameObject megaExplosionPrefab = CreateMegaExplosionAssets(
                pixelGlow,
                pixelRing,
                pixelShard,
                pixelSmoke,
                pixelFlame);
            if (File.Exists(PreviewScenePath))
            {
                UpdatePreviewScene(
                    AssetDatabase.LoadAssetAtPath<GameObject>(NailDrivingPrefabPath),
                    AssetDatabase.LoadAssetAtPath<GameObject>(PlagueMagicPrefabPath),
                    AssetDatabase.LoadAssetAtPath<GameObject>(IceLancePrefabPath),
                    megaExplosionPrefab,
                    AssetDatabase.LoadAssetAtPath<GameObject>(FlashbangPrefabPath));
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log(
                $"[Skill VFX] Generated {MegaExplosionPrefabPath} and updated the preview scene.");
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

        private static GameObject CreateFlashbangAssets(
            Material pixelGlow,
            Material pixelRing,
            Material pixelShard)
        {
            Material radiantStar = CreateMaterial(
                "RadiantStar_Additive.mat",
                LoadTexture("RadiantStar.png"),
                BlendMode.One);
            Material lightBeam = CreateMaterial(
                "LightBeam_Additive.mat",
                LoadTexture("LightBeam.png"),
                BlendMode.One);
            Material bokeh = CreateMaterial(
                "GoldenBokeh_Alpha.mat",
                LoadTexture("GoldenBokeh.png"),
                BlendMode.OneMinusSrcAlpha);

            return CreateFlashbangPrefab(
                radiantStar,
                lightBeam,
                bokeh,
                pixelGlow,
                pixelRing,
                pixelShard);
        }

        private static GameObject CreateMegaExplosionAssets(
            Material pixelGlow,
            Material pixelRing,
            Material pixelShard,
            Material pixelSmoke,
            Material pixelFlame)
        {
            Material rune = CreateMaterial(
                "MegaRune_Alpha.mat",
                LoadTexture("MegaRune.png"),
                BlendMode.OneMinusSrcAlpha);
            Material beam = CreateMaterial(
                "MegaBeam_Alpha.mat",
                LoadTexture("MegaBeam.png"),
                BlendMode.OneMinusSrcAlpha);
            Material debris = CreateMaterial(
                "MegaDebris_Alpha.mat",
                LoadTexture("Rock.png"),
                BlendMode.OneMinusSrcAlpha);
            Material scorch = CreateMaterial(
                "MegaScorch_Alpha.mat",
                LoadTexture("MegaScorch.png"),
                BlendMode.OneMinusSrcAlpha);
            Material impactGlow = CreateMaterial(
                "MegaImpactGlow_Alpha.mat",
                LoadTexture("PixelGlow.png"),
                BlendMode.OneMinusSrcAlpha);

            return CreateMegaExplosionPrefab(
                rune,
                beam,
                debris,
                scorch,
                impactGlow,
                pixelGlow,
                pixelRing,
                pixelShard,
                pixelSmoke,
                pixelFlame);
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
            ConfigureBase(lanceGlow, 1.8f, 1.6f, 25f, 1f, 2, Color.white);
            ParticleSystem.MainModule lanceGlowMain = lanceGlow.main;
            lanceGlowMain.startLifetime = new ParticleSystem.MinMaxCurve(1.58f, 1.62f);
            lanceGlowMain.startSpeed = new ParticleSystem.MinMaxCurve(24.8f, 25.2f);
            SetStartSize3D(lanceGlow, 4.15f, 1.18f);
            ConfigureCone(
                lanceGlow,
                new Vector3(0f, 90f, 0f),
                0.35f,
                0.01f,
                new Vector3(-40f, 0.72f, 0f));
            SetBursts(lanceGlow, new ParticleSystem.Burst(0f, 1));
            SetColorOverLifetime(
                lanceGlow,
                new Color(0.66f, 0.9f, 1f),
                new Color(0.22f, 0.58f, 1f),
                0.58f,
                0f);
            SetDeterministic(lanceGlow, 7001);

            ParticleSystem lanceCore = CreateSystem(root.transform, "IceLanceCore", lanceAlpha, 15);
            ConfigureBase(lanceCore, 1.8f, 1.6f, 25f, 1f, 2, Color.white);
            ParticleSystem.MainModule lanceCoreMain = lanceCore.main;
            lanceCoreMain.startLifetime = new ParticleSystem.MinMaxCurve(1.58f, 1.62f);
            lanceCoreMain.startSpeed = new ParticleSystem.MinMaxCurve(24.8f, 25.2f);
            SetStartSize3D(lanceCore, 3.55f, 0.86f);
            ConfigureCone(
                lanceCore,
                new Vector3(0f, 90f, 0f),
                0.2f,
                0.01f,
                new Vector3(-40f, 0.72f, 0f));
            SetBursts(lanceCore, new ParticleSystem.Burst(0f, 1));
            SetColorOverLifetime(
                lanceCore,
                Color.white,
                new Color(0.45f, 0.78f, 1f),
                1f,
                0f);
            SetDeterministic(lanceCore, 7002);

            ParticleSystem lanceWake = CreateSystem(root.transform, "LanceWakeShards", pixelShard, 13);
            ConfigureBase(lanceWake, 1.9f, 1.6f, 25f, 0.25f, 36, Color.white);
            ParticleSystem.MainModule lanceWakeMain = lanceWake.main;
            lanceWakeMain.startLifetime = new ParticleSystem.MinMaxCurve(1.35f, 1.65f);
            lanceWakeMain.startSpeed = new ParticleSystem.MinMaxCurve(24f, 26f);
            lanceWakeMain.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.3f);
            lanceWakeMain.startRotation = new ParticleSystem.MinMaxCurve(-Mathf.PI, Mathf.PI);
            ConfigureCone(
                lanceWake,
                new Vector3(0f, 90f, 0f),
                8f,
                0.2f,
                new Vector3(-40f, 0.72f, 0f));
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
            frostMain.startDelay = 1.6f;
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
            backCrownMain.startDelay = 1.6f;
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
            frontCrownMain.startDelay = 1.62f;
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
            contactMain.startDelay = 1.59f;
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
            impactShardMain.startDelay = 1.6f;
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
            breakShardMain.startDelay = 2.54f;
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
            snowMain.startDelay = 1.8f;
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
            mistMain.startDelay = 1.66f;
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

        private static GameObject CreateFlashbangPrefab(
            Material radiantStar,
            Material lightBeam,
            Material bokeh,
            Material pixelGlow,
            Material pixelRing,
            Material pixelShard)
        {
            GameObject root = CreateEffectRoot("Skill_010_Flashbang", "skill_010");

            ParticleSystem focusStar = CreateSystem(root.transform, "FocusStar", radiantStar, 20);
            focusStar.transform.localPosition = new Vector3(0f, 0.22f, 0f);
            ConfigureBase(focusStar, 0.7f, 0.44f, 0f, 1.3f, 3, Color.white);
            ParticleSystem.MainModule focusMain = focusStar.main;
            focusMain.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.46f);
            focusMain.startSize = new ParticleSystem.MinMaxCurve(1.1f, 1.45f);
            SetBursts(focusStar, new ParticleSystem.Burst(0f, 1));
            SetColorOverLifetime(
                focusStar,
                new Color(1f, 0.98f, 0.84f),
                new Color(1f, 0.58f, 0.08f),
                1f,
                0f);
            SetSizeOverLifetime(
                focusStar,
                Curve((0f, 0.15f), (0.24f, 1f), (0.58f, 1.35f), (1f, 0.2f)));
            SetDeterministic(focusStar, 10001);

            ParticleSystem coreBurst = CreateSystem(root.transform, "SolarBurst", radiantStar, 19);
            coreBurst.transform.localPosition = new Vector3(0f, 0.2f, 0f);
            ConfigureBase(coreBurst, 0.9f, 0.38f, 0f, 5f, 6, Color.white);
            ParticleSystem.MainModule coreMain = coreBurst.main;
            coreMain.startDelay = 0.16f;
            coreMain.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.42f);
            coreMain.startSize = new ParticleSystem.MinMaxCurve(4.2f, 5.4f);
            coreMain.startRotation = new ParticleSystem.MinMaxCurve(-0.18f, 0.18f);
            SetBursts(coreBurst, new ParticleSystem.Burst(0f, 3));
            SetColorOverLifetime(
                coreBurst,
                new Color(1f, 1f, 0.94f),
                new Color(1f, 0.48f, 0.04f),
                1f,
                0f);
            SetSizeOverLifetime(coreBurst, Curve((0f, 0.04f), (0.12f, 1f), (1f, 0.14f)));
            SetDeterministic(coreBurst, 10002);

            ParticleSystem horizontalFlare =
                CreateSystem(root.transform, "HorizontalLensFlare", lightBeam, 18);
            horizontalFlare.transform.localPosition = new Vector3(0f, 0.26f, 0f);
            ConfigureBase(horizontalFlare, 0.9f, 0.42f, 0f, 1f, 2, Color.white);
            ParticleSystem.MainModule horizontalMain = horizontalFlare.main;
            horizontalMain.startDelay = 0.17f;
            horizontalMain.startLifetime = 0.42f;
            SetStartSize3D(horizontalFlare, 8.4f, 0.32f);
            SetBursts(horizontalFlare, new ParticleSystem.Burst(0f, 1));
            SetColorOverLifetime(
                horizontalFlare,
                new Color(1f, 0.94f, 0.68f),
                new Color(1f, 0.52f, 0.08f),
                0.9f,
                0f);
            SetSizeOverLifetime(
                horizontalFlare,
                Curve((0f, 0.08f), (0.16f, 1f), (0.66f, 0.75f), (1f, 0.1f)));
            SetDeterministic(horizontalFlare, 10003);

            ParticleSystem heavenColumn =
                CreateSystem(root.transform, "HeavenColumn", lightBeam, 12);
            heavenColumn.transform.localPosition = new Vector3(0f, 3f, 0f);
            ConfigureBase(heavenColumn, 1.8f, 1.15f, 0f, 1f, 4, Color.white);
            ParticleSystem.MainModule columnMain = heavenColumn.main;
            columnMain.startDelay = 0.21f;
            columnMain.startLifetime = new ParticleSystem.MinMaxCurve(0.95f, 1.18f);
            SetStartSize3D(heavenColumn, 2.15f, 8.2f);
            SetBursts(heavenColumn, new ParticleSystem.Burst(0f, 2));
            SetColorOverLifetime(
                heavenColumn,
                new Color(1f, 0.94f, 0.68f),
                new Color(1f, 0.57f, 0.08f),
                0.72f,
                0f);
            SetSizeOverLifetime(
                heavenColumn,
                Curve((0f, 0.08f), (0.12f, 1f), (0.72f, 0.72f), (1f, 0.18f)));
            SetDeterministic(heavenColumn, 10004);

            ParticleSystem halo = CreateSystem(root.transform, "RadiantHalo", pixelRing, 16);
            halo.transform.localScale = new Vector3(1f, 0.24f, 1f);
            ConfigureBase(halo, 1.4f, 0.72f, 0f, 6.4f, 8, Color.white);
            ParticleSystem.MainModule haloMain = halo.main;
            haloMain.startDelay = 0.19f;
            haloMain.startLifetime = new ParticleSystem.MinMaxCurve(0.56f, 0.78f);
            haloMain.startSize = new ParticleSystem.MinMaxCurve(5.5f, 6.7f);
            SetBursts(
                halo,
                new ParticleSystem.Burst(0f, 2),
                new ParticleSystem.Burst(0.2f, 1));
            SetColorOverLifetime(
                halo,
                new Color(1f, 0.88f, 0.45f),
                new Color(0.92f, 0.4f, 0.035f),
                0.88f,
                0f);
            SetSizeOverLifetime(halo, Curve((0f, 0.06f), (0.6f, 0.94f), (1f, 1.18f)));
            SetDeterministic(halo, 10005);

            ParticleSystem sunSpears = CreateSystem(root.transform, "SunSpears", pixelGlow, 17);
            sunSpears.transform.localPosition = new Vector3(0f, 0.2f, 0f);
            ConfigureBase(sunSpears, 1.2f, 0.62f, 8f, 0.12f, 120, Color.white);
            ParticleSystem.MainModule spearMain = sunSpears.main;
            spearMain.startDelay = 0.17f;
            spearMain.startLifetime = new ParticleSystem.MinMaxCurve(0.22f, 0.72f);
            spearMain.startSpeed = new ParticleSystem.MinMaxCurve(4.5f, 12f);
            spearMain.startSize = new ParticleSystem.MinMaxCurve(0.035f, 0.16f);
            ConfigureCircle(sunSpears, 0.16f);
            SetBursts(sunSpears, new ParticleSystem.Burst(0f, 84));
            SetColorOverLifetime(
                sunSpears,
                new Color(1f, 0.95f, 0.7f),
                new Color(1f, 0.43f, 0.025f),
                1f,
                0f);
            ParticleSystemRenderer spearRenderer = sunSpears.GetComponent<ParticleSystemRenderer>();
            spearRenderer.renderMode = ParticleSystemRenderMode.Stretch;
            spearRenderer.lengthScale = 3.2f;
            spearRenderer.velocityScale = 0.06f;
            SetDeterministic(sunSpears, 10006);

            ParticleSystem goldenShards =
                CreateSystem(root.transform, "GoldenGroundShards", pixelShard, 14);
            ConfigureBase(goldenShards, 1.7f, 0.95f, 5f, 0.32f, 100, Color.white);
            ParticleSystem.MainModule shardMain = goldenShards.main;
            shardMain.startDelay = 0.2f;
            shardMain.startLifetime = new ParticleSystem.MinMaxCurve(0.42f, 1.18f);
            shardMain.startSpeed = new ParticleSystem.MinMaxCurve(2.8f, 8f);
            shardMain.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.42f);
            shardMain.startRotation = new ParticleSystem.MinMaxCurve(-Mathf.PI, Mathf.PI);
            shardMain.gravityModifier = new ParticleSystem.MinMaxCurve(1.8f, 3.2f);
            ConfigureCone(goldenShards, new Vector3(-90f, 0f, 0f), 67f, 0.65f, Vector3.zero);
            SetBursts(goldenShards, new ParticleSystem.Burst(0f, 52));
            SetColorOverLifetime(
                goldenShards,
                new Color(1f, 0.8f, 0.34f),
                new Color(0.42f, 0.16f, 0.025f),
                1f,
                0f);
            ConfigureRotation(goldenShards, -5f, 5f);
            SetDeterministic(goldenShards, 10007);

            ParticleSystem lightRain = CreateSystem(root.transform, "FallingLight", pixelGlow, 13);
            ConfigureBase(lightRain, 2.1f, 0.9f, 0f, 0.1f, 100, Color.white);
            ParticleSystem.MainModule rainMain = lightRain.main;
            rainMain.startDelay = 0.32f;
            rainMain.startLifetime = new ParticleSystem.MinMaxCurve(0.48f, 1.02f);
            rainMain.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.13f);
            ParticleSystem.ShapeModule rainShape = lightRain.shape;
            rainShape.enabled = true;
            rainShape.shapeType = ParticleSystemShapeType.Box;
            rainShape.position = new Vector3(0f, 4f, 0f);
            rainShape.scale = new Vector3(5.2f, 0.2f, 0.1f);
            SetBursts(
                lightRain,
                new ParticleSystem.Burst(0f, 28),
                new ParticleSystem.Burst(0.45f, 22),
                new ParticleSystem.Burst(0.9f, 16));
            ParticleSystem.VelocityOverLifetimeModule rainVelocity = lightRain.velocityOverLifetime;
            rainVelocity.enabled = true;
            rainVelocity.space = ParticleSystemSimulationSpace.Local;
            rainVelocity.x = new ParticleSystem.MinMaxCurve(-0.28f, 0.28f);
            rainVelocity.y = new ParticleSystem.MinMaxCurve(-5.8f, -4.2f);
            rainVelocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);
            SetColorOverLifetime(
                lightRain,
                new Color(1f, 0.9f, 0.55f),
                new Color(1f, 0.45f, 0.04f),
                0.82f,
                0f);
            ParticleSystemRenderer rainRenderer = lightRain.GetComponent<ParticleSystemRenderer>();
            rainRenderer.renderMode = ParticleSystemRenderMode.Stretch;
            rainRenderer.lengthScale = 3.2f;
            rainRenderer.velocityScale = 0.035f;
            SetDeterministic(lightRain, 10008);

            ParticleSystem bokehDust = CreateSystem(root.transform, "GoldenBokeh", bokeh, 9);
            ConfigureBase(bokehDust, 2.8f, 1.55f, 1.25f, 0.75f, 70, Color.white);
            ParticleSystem.MainModule bokehMain = bokehDust.main;
            bokehMain.startDelay = 0.42f;
            bokehMain.startLifetime = new ParticleSystem.MinMaxCurve(0.85f, 2.15f);
            bokehMain.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 1.65f);
            bokehMain.startSize = new ParticleSystem.MinMaxCurve(0.2f, 1.05f);
            bokehMain.startRotation = new ParticleSystem.MinMaxCurve(-Mathf.PI, Mathf.PI);
            ConfigureCone(bokehDust, new Vector3(-90f, 0f, 0f), 52f, 2.2f, Vector3.zero);
            SetBursts(
                bokehDust,
                new ParticleSystem.Burst(0f, 28),
                new ParticleSystem.Burst(0.65f, 18));
            ConfigureNoise(bokehDust, 0.32f, 0.42f);
            SetColorOverLifetime(
                bokehDust,
                new Color(1f, 0.78f, 0.34f),
                new Color(0.52f, 0.18f, 0.025f),
                0.48f,
                0f);
            SetSizeOverLifetime(bokehDust, Curve((0f, 0.2f), (0.35f, 1f), (1f, 1.18f)));
            SetDeterministic(bokehDust, 10009);

            ParticleSystem afterglow = CreateSystem(root.transform, "GroundAfterglow", bokeh, 8);
            afterglow.transform.localScale = new Vector3(1f, 0.26f, 1f);
            ConfigureBase(afterglow, 2f, 1.4f, 0f, 5.5f, 4, Color.white);
            ParticleSystem.MainModule afterglowMain = afterglow.main;
            afterglowMain.startDelay = 0.2f;
            afterglowMain.startLifetime = new ParticleSystem.MinMaxCurve(1.15f, 1.5f);
            afterglowMain.startSize = new ParticleSystem.MinMaxCurve(4.8f, 5.8f);
            SetBursts(afterglow, new ParticleSystem.Burst(0f, 2));
            SetColorOverLifetime(
                afterglow,
                new Color(1f, 0.68f, 0.22f),
                new Color(0.45f, 0.12f, 0.015f),
                0.4f,
                0f);
            SetSizeOverLifetime(
                afterglow,
                Curve((0f, 0.1f), (0.18f, 1f), (0.76f, 0.9f), (1f, 0.7f)));
            SetDeterministic(afterglow, 10010);

            return SavePrefab(root, FlashbangPrefabPath);
        }

        private static GameObject CreateMegaExplosionPrefab(
            Material runeMaterial,
            Material beamMaterial,
            Material debrisMaterial,
            Material scorchMaterial,
            Material impactGlow,
            Material pixelGlow,
            Material pixelRing,
            Material pixelShard,
            Material pixelSmoke,
            Material pixelFlame)
        {
            GameObject root = CreateEffectRoot("Skill_008_MegaExplosion", "skill_008");

            ParticleSystem outerRune = CreateSystem(root.transform, "ChargeRuneOuter", runeMaterial, 2);
            outerRune.transform.localScale = new Vector3(1f, 0.72f, 1f);
            ConfigureBase(outerRune, 1f, 0.82f, 0f, 8.4f, 2, Color.white);
            ParticleSystem.MainModule outerRuneMain = outerRune.main;
            outerRuneMain.startColor = new Color(1f, 0.28f, 0.015f);
            SetBursts(outerRune, new ParticleSystem.Burst(0f, 1));
            SetSizeOverLifetime(
                outerRune,
                Curve((0f, 0.08f), (0.28f, 0.72f), (0.62f, 1f), (1f, 0.82f)));
            SetColorOverLifetime(
                outerRune,
                new Color(1f, 0.16f, 0.01f),
                new Color(1f, 0.72f, 0.12f),
                0.35f,
                0f);
            ConfigureRotation(outerRune, -0.55f, -0.55f);
            SetDeterministic(outerRune, 8001);

            ParticleSystem innerRune = CreateSystem(root.transform, "ChargeRuneInner", runeMaterial, 3);
            innerRune.transform.localPosition = new Vector3(0f, 0.08f, 0f);
            innerRune.transform.localScale = new Vector3(0.72f, 0.58f, 1f);
            ConfigureBase(innerRune, 1f, 0.7f, 0f, 6.8f, 2, Color.white);
            ParticleSystem.MainModule innerRuneMain = innerRune.main;
            innerRuneMain.startDelay = 0.12f;
            innerRuneMain.startColor = new Color(1f, 0.55f, 0.06f);
            SetBursts(innerRune, new ParticleSystem.Burst(0f, 1));
            SetSizeOverLifetime(
                innerRune,
                Curve((0f, 0.12f), (0.32f, 0.86f), (0.74f, 1f), (1f, 0.7f)));
            SetFade(innerRune, 0.9f, 0f);
            ConfigureRotation(innerRune, 0.85f, 0.85f);
            SetDeterministic(innerRune, 8002);

            ParticleSystem chargeSparks = CreateSystem(
                root.transform,
                "RisingChargeSparks",
                pixelShard,
                6);
            ConfigureBase(chargeSparks, 1.1f, 0.6f, 4f, 0.14f, 70, Color.white);
            ParticleSystem.MainModule chargeSparkMain = chargeSparks.main;
            chargeSparkMain.startDelay = 0.08f;
            chargeSparkMain.startLifetime = new ParticleSystem.MinMaxCurve(0.28f, 0.72f);
            chargeSparkMain.startSpeed = new ParticleSystem.MinMaxCurve(2.4f, 6.2f);
            chargeSparkMain.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.22f);
            chargeSparkMain.startColor = new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.35f, 0.02f),
                new Color(1f, 0.9f, 0.38f));
            ConfigureCone(
                chargeSparks,
                new Vector3(-90f, 0f, 0f),
                38f,
                2.1f,
                new Vector3(0f, 0.08f, 0f));
            SetBursts(
                chargeSparks,
                new ParticleSystem.Burst(0f, 18),
                new ParticleSystem.Burst(0.22f, 24),
                new ParticleSystem.Burst(0.44f, 28));
            SetFade(chargeSparks, 1f, 0f);
            chargeSparks.GetComponent<ParticleSystemRenderer>().alignment =
                ParticleSystemRenderSpace.Velocity;
            SetDeterministic(chargeSparks, 8003);

            ParticleSystem beamBack = CreateSystem(root.transform, "EnergyBeamBack", beamMaterial, 5);
            beamBack.transform.localPosition = new Vector3(0f, 3.4f, 0f);
            ConfigureBase(beamBack, 1.4f, 0.88f, 0f, 1f, 2, Color.white);
            ParticleSystem.MainModule beamBackMain = beamBack.main;
            beamBackMain.startDelay = 0.24f;
            SetStartSize3D(beamBack, 3.2f, 7.8f);
            beamBackMain.startColor = new Color(1f, 0.22f, 0.015f, 0.72f);
            SetBursts(beamBack, new ParticleSystem.Burst(0f, 1));
            SetSizeOverLifetime(
                beamBack,
                Curve((0f, 0.06f), (0.18f, 0.78f), (0.62f, 1f), (1f, 0.24f)));
            SetFade(beamBack, 0.78f, 0f);
            SetDeterministic(beamBack, 8004);

            ParticleSystem beamCore = CreateSystem(root.transform, "EnergyBeamCore", beamMaterial, 8);
            beamCore.transform.localPosition = new Vector3(0f, 3.55f, 0f);
            ConfigureBase(beamCore, 1.2f, 0.58f, 0f, 1f, 2, Color.white);
            ParticleSystem.MainModule beamCoreMain = beamCore.main;
            beamCoreMain.startDelay = 0.38f;
            SetStartSize3D(beamCore, 1.15f, 8.25f);
            beamCoreMain.startColor = new Color(1f, 0.94f, 0.6f);
            SetBursts(beamCore, new ParticleSystem.Burst(0f, 1));
            SetSizeOverLifetime(
                beamCore,
                Curve((0f, 0.08f), (0.15f, 1f), (0.7f, 0.82f), (1f, 0.05f)));
            SetFade(beamCore, 1f, 0f);
            SetDeterministic(beamCore, 8005);

            ParticleSystem beamHalos = CreateSystem(root.transform, "BeamOrbitHalos", pixelRing, 7);
            beamHalos.transform.localPosition = new Vector3(0f, 2.75f, 0f);
            beamHalos.transform.localScale = new Vector3(1f, 0.28f, 1f);
            ConfigureBase(beamHalos, 1.2f, 0.52f, 0f, 3.4f, 4, Color.white);
            ParticleSystem.MainModule haloMain = beamHalos.main;
            haloMain.startDelay = 0.28f;
            haloMain.startColor = new Color(1f, 0.52f, 0.08f);
            SetBursts(
                beamHalos,
                new ParticleSystem.Burst(0f, 1),
                new ParticleSystem.Burst(0.24f, 1));
            SetSizeOverLifetime(
                beamHalos,
                Curve((0f, 0.32f), (0.34f, 1f), (1f, 1.38f)));
            SetFade(beamHalos, 0.92f, 0f);
            ConfigureRotation(beamHalos, -1.3f, 1.3f);
            SetDeterministic(beamHalos, 8006);

            ParticleSystem impactFlash = CreateSystem(root.transform, "CataclysmFlash", impactGlow, 15);
            impactFlash.transform.localPosition = new Vector3(0f, 0.36f, 0f);
            ConfigureBase(impactFlash, 1.2f, 0.32f, 0f, 4.2f, 4, Color.white);
            ParticleSystem.MainModule impactFlashMain = impactFlash.main;
            impactFlashMain.startDelay = 0.68f;
            impactFlashMain.startSize = new ParticleSystem.MinMaxCurve(3.6f, 4.8f);
            impactFlashMain.startColor = new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.42f, 0.02f),
                new Color(1f, 1f, 0.76f));
            SetBursts(impactFlash, new ParticleSystem.Burst(0f, 3));
            SetSizeOverLifetime(
                impactFlash,
                Curve((0f, 0.08f), (0.14f, 1f), (0.5f, 0.82f), (1f, 0.08f)));
            SetFade(impactFlash, 1f, 0f);
            SetDeterministic(impactFlash, 8007);

            ParticleSystem shockwave = CreateSystem(root.transform, "GroundCataclysmRing", runeMaterial, 9);
            shockwave.transform.localScale = new Vector3(1f, 0.46f, 1f);
            ConfigureBase(shockwave, 1.2f, 0.46f, 0f, 7.2f, 2, Color.white);
            ParticleSystem.MainModule shockwaveMain = shockwave.main;
            shockwaveMain.startDelay = 0.68f;
            shockwaveMain.startColor = new Color(1f, 0.52f, 0.045f);
            SetBursts(shockwave, new ParticleSystem.Burst(0f, 1));
            SetSizeOverLifetime(
                shockwave,
                Curve((0f, 0.12f), (0.42f, 0.86f), (1f, 1.22f)));
            SetFade(shockwave, 0.95f, 0f);
            SetDeterministic(shockwave, 8008);

            ParticleSystem radialRays = CreateSystem(root.transform, "RadialBlastRays", beamMaterial, 14);
            radialRays.transform.localPosition = new Vector3(0f, 0.32f, 0f);
            ConfigureBase(radialRays, 1.3f, 0.34f, 10f, 0.12f, 100, Color.white);
            ParticleSystem.MainModule radialRayMain = radialRays.main;
            radialRayMain.startDelay = 0.67f;
            radialRayMain.startLifetime = new ParticleSystem.MinMaxCurve(0.16f, 0.42f);
            radialRayMain.startSpeed = new ParticleSystem.MinMaxCurve(7.5f, 14.5f);
            radialRayMain.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.16f);
            ConfigureCircle(radialRays, 0.18f);
            SetBursts(radialRays, new ParticleSystem.Burst(0f, 62));
            SetColorOverLifetime(
                radialRays,
                new Color(1f, 1f, 0.72f),
                new Color(1f, 0.24f, 0.01f),
                1f,
                0f);
            ParticleSystemRenderer radialRayRenderer =
                radialRays.GetComponent<ParticleSystemRenderer>();
            radialRayRenderer.renderMode = ParticleSystemRenderMode.Stretch;
            radialRayRenderer.lengthScale = 2.7f;
            radialRayRenderer.velocityScale = 0.055f;
            SetDeterministic(radialRays, 8009);

            ParticleSystem fireballs = CreateSystem(root.transform, "RollingFireBursts", pixelFlame, 12);
            fireballs.transform.localPosition = new Vector3(0f, 0.42f, 0f);
            ConfigureBase(fireballs, 2f, 0.68f, 3f, 0.8f, 100, Color.white);
            ParticleSystem.MainModule fireballMain = fireballs.main;
            fireballMain.startDelay = 0.69f;
            fireballMain.startLifetime = new ParticleSystem.MinMaxCurve(0.38f, 0.92f);
            fireballMain.startSpeed = new ParticleSystem.MinMaxCurve(1.2f, 4.8f);
            fireballMain.startSize = new ParticleSystem.MinMaxCurve(0.58f, 1.72f);
            fireballMain.startRotation = new ParticleSystem.MinMaxCurve(-Mathf.PI, Mathf.PI);
            fireballMain.startColor = new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.22f, 0.005f),
                new Color(1f, 0.88f, 0.2f));
            ConfigureCircle(fireballs, 0.72f);
            SetBursts(
                fireballs,
                new ParticleSystem.Burst(0f, 38),
                new ParticleSystem.Burst(0.16f, 24));
            ConfigureNoise(fireballs, 0.42f, 0.8f);
            ConfigureRotation(fireballs, -2.8f, 2.8f);
            SetColorOverLifetime(
                fireballs,
                new Color(1f, 0.9f, 0.24f),
                new Color(0.86f, 0.035f, 0.002f),
                1f,
                0f);
            SetSizeOverLifetime(
                fireballs,
                Curve((0f, 0.14f), (0.22f, 1f), (0.72f, 0.78f), (1f, 0.05f)));
            SetDeterministic(fireballs, 8010);

            ParticleSystem fireColumn = CreateSystem(root.transform, "ToweringFireColumn", pixelFlame, 13);
            ConfigureBase(fireColumn, 2.2f, 0.9f, 6f, 0.8f, 110, Color.white);
            ParticleSystem.MainModule fireMain = fireColumn.main;
            fireMain.startDelay = 0.7f;
            fireMain.startLifetime = new ParticleSystem.MinMaxCurve(0.42f, 1.08f);
            fireMain.startSpeed = new ParticleSystem.MinMaxCurve(4.4f, 10.8f);
            fireMain.startSize = new ParticleSystem.MinMaxCurve(0.36f, 1.28f);
            fireMain.startColor = new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.2f, 0.005f),
                new Color(1f, 0.9f, 0.28f));
            ConfigureCone(
                fireColumn,
                new Vector3(-90f, 0f, 0f),
                34f,
                0.82f,
                new Vector3(0f, 0.14f, 0f));
            SetBursts(
                fireColumn,
                new ParticleSystem.Burst(0f, 48),
                new ParticleSystem.Burst(0.16f, 30));
            ConfigureNoise(fireColumn, 0.36f, 0.75f);
            SetColorOverLifetime(
                fireColumn,
                new Color(1f, 0.86f, 0.2f),
                new Color(0.9f, 0.04f, 0f),
                1f,
                0f);
            SetSizeOverLifetime(
                fireColumn,
                Curve((0f, 0.18f), (0.18f, 1f), (0.72f, 0.82f), (1f, 0.08f)));
            fireColumn.GetComponent<ParticleSystemRenderer>().alignment =
                ParticleSystemRenderSpace.Velocity;
            SetDeterministic(fireColumn, 8011);

            ParticleSystem debris = CreateSystem(root.transform, "CataclysmDebris", debrisMaterial, 14);
            ConfigureBase(debris, 2.8f, 1.25f, 6f, 0.36f, 90, Color.white);
            ParticleSystem.MainModule debrisMain = debris.main;
            debrisMain.startDelay = 0.7f;
            debrisMain.startLifetime = new ParticleSystem.MinMaxCurve(0.65f, 1.8f);
            debrisMain.startSpeed = new ParticleSystem.MinMaxCurve(3.2f, 9.8f);
            debrisMain.startSize = new ParticleSystem.MinMaxCurve(0.16f, 0.58f);
            debrisMain.startRotation = new ParticleSystem.MinMaxCurve(-Mathf.PI, Mathf.PI);
            debrisMain.gravityModifier = new ParticleSystem.MinMaxCurve(1.8f, 3.2f);
            debrisMain.startColor = new ParticleSystem.MinMaxGradient(
                new Color(0.18f, 0.08f, 0.035f),
                new Color(0.58f, 0.2f, 0.055f));
            ConfigureCone(
                debris,
                new Vector3(-90f, 0f, 0f),
                76f,
                1.25f,
                new Vector3(0f, 0.18f, 0f));
            SetBursts(debris, new ParticleSystem.Burst(0f, 24));
            ConfigureRotation(debris, -7f, 7f);
            SetFade(debris, 1f, 0f);
            SetDeterministic(debris, 8012);

            ParticleSystem smoke = CreateSystem(root.transform, "MushroomSmoke", pixelSmoke, 7);
            smoke.transform.localPosition = new Vector3(0f, 0.4f, 0f);
            ConfigureBase(smoke, 3.6f, 2f, 2.4f, 1.2f, 110, Color.white);
            ParticleSystem.MainModule smokeMain = smoke.main;
            smokeMain.startDelay = 0.76f;
            smokeMain.startLifetime = new ParticleSystem.MinMaxCurve(0.95f, 2.45f);
            smokeMain.startSpeed = new ParticleSystem.MinMaxCurve(1.2f, 4.4f);
            smokeMain.startSize = new ParticleSystem.MinMaxCurve(0.7f, 2.05f);
            smokeMain.startRotation = new ParticleSystem.MinMaxCurve(-Mathf.PI, Mathf.PI);
            smokeMain.startColor = Color.white;
            ConfigureCone(
                smoke,
                new Vector3(-90f, 0f, 0f),
                52f,
                1.35f,
                new Vector3(0f, 0.2f, 0f));
            SetBursts(
                smoke,
                new ParticleSystem.Burst(0f, 30),
                new ParticleSystem.Burst(0.32f, 24),
                new ParticleSystem.Burst(0.72f, 18));
            ConfigureNoise(smoke, 0.5f, 0.42f);
            SetColorOverLifetime(
                smoke,
                new Color(0.32f, 0.13f, 0.055f),
                new Color(0.055f, 0.045f, 0.04f),
                0.94f,
                0f);
            SetSizeOverLifetime(
                smoke,
                Curve((0f, 0.35f), (0.44f, 1f), (1f, 1.42f)));
            SetDeterministic(smoke, 8013);

            ParticleSystem rollingSmoke = CreateSystem(
                root.transform,
                "RollingSmokeFront",
                pixelSmoke,
                11);
            rollingSmoke.transform.localPosition = new Vector3(0f, 0.46f, 0f);
            ConfigureBase(rollingSmoke, 3.2f, 1.25f, 2.2f, 1f, 80, Color.white);
            ParticleSystem.MainModule rollingSmokeMain = rollingSmoke.main;
            rollingSmokeMain.startDelay = 0.86f;
            rollingSmokeMain.startLifetime = new ParticleSystem.MinMaxCurve(0.78f, 1.8f);
            rollingSmokeMain.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3.8f);
            rollingSmokeMain.startSize = new ParticleSystem.MinMaxCurve(0.62f, 1.72f);
            rollingSmokeMain.startRotation = new ParticleSystem.MinMaxCurve(-Mathf.PI, Mathf.PI);
            rollingSmokeMain.startColor = Color.white;
            ConfigureCircle(rollingSmoke, 0.82f);
            SetBursts(
                rollingSmoke,
                new ParticleSystem.Burst(0f, 28),
                new ParticleSystem.Burst(0.42f, 18));
            ConfigureNoise(rollingSmoke, 0.55f, 0.48f);
            ConfigureRotation(rollingSmoke, -1.6f, 1.6f);
            SetColorOverLifetime(
                rollingSmoke,
                new Color(0.42f, 0.15f, 0.045f),
                new Color(0.045f, 0.038f, 0.035f),
                0.92f,
                0f);
            SetSizeOverLifetime(
                rollingSmoke,
                Curve((0f, 0.24f), (0.38f, 1f), (1f, 1.35f)));
            SetDeterministic(rollingSmoke, 8014);

            ParticleSystem embers = CreateSystem(root.transform, "LongLivedEmbers", pixelGlow, 16);
            ConfigureBase(embers, 3.8f, 2f, 6f, 0.1f, 170, Color.white);
            ParticleSystem.MainModule emberMain = embers.main;
            emberMain.startDelay = 0.72f;
            emberMain.startLifetime = new ParticleSystem.MinMaxCurve(0.75f, 2.8f);
            emberMain.startSpeed = new ParticleSystem.MinMaxCurve(2.4f, 9.5f);
            emberMain.startSize = new ParticleSystem.MinMaxCurve(0.035f, 0.18f);
            emberMain.gravityModifier = new ParticleSystem.MinMaxCurve(0.35f, 1.4f);
            emberMain.startColor = new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.18f, 0.01f),
                new Color(1f, 0.74f, 0.16f));
            ConfigureCone(
                embers,
                new Vector3(-90f, 0f, 0f),
                82f,
                1.7f,
                new Vector3(0f, 0.12f, 0f));
            SetBursts(
                embers,
                new ParticleSystem.Burst(0f, 92),
                new ParticleSystem.Burst(0.42f, 42));
            ConfigureNoise(embers, 0.42f, 1.1f);
            SetFade(embers, 1f, 0f);
            SetDeterministic(embers, 8015);

            ParticleSystem scorch = CreateSystem(root.transform, "BurningScorch", scorchMaterial, 1);
            scorch.transform.localPosition = new Vector3(0f, -0.03f, 0f);
            ConfigureBase(scorch, 3.8f, 2.9f, 0f, 1f, 2, Color.white);
            ParticleSystem.MainModule scorchMain = scorch.main;
            scorchMain.startDelay = 0.7f;
            SetStartSize3D(scorch, 8.4f, 0.82f);
            SetBursts(scorch, new ParticleSystem.Burst(0f, 1));
            SetSizeOverLifetime(
                scorch,
                Curve((0f, 0.08f), (0.1f, 1f), (0.82f, 1f), (1f, 0.94f)));
            SetFade(scorch, 1f, 0f);
            SetDeterministic(scorch, 8016);

            ParticleSystem groundFire = CreateSystem(root.transform, "GroundAfterburn", pixelFlame, 8);
            ConfigureBase(groundFire, 3.4f, 1.1f, 1.1f, 0.5f, 70, Color.white);
            ParticleSystem.MainModule groundFireMain = groundFire.main;
            groundFireMain.startDelay = 0.92f;
            groundFireMain.startLifetime = new ParticleSystem.MinMaxCurve(0.55f, 1.45f);
            groundFireMain.startSpeed = new ParticleSystem.MinMaxCurve(0.35f, 1.8f);
            groundFireMain.startSize = new ParticleSystem.MinMaxCurve(0.18f, 0.78f);
            groundFireMain.startColor = new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.1f, 0.005f),
                new Color(1f, 0.58f, 0.08f));
            ParticleSystem.ShapeModule groundFireShape = groundFire.shape;
            groundFireShape.enabled = true;
            groundFireShape.shapeType = ParticleSystemShapeType.Box;
            groundFireShape.position = new Vector3(0f, 0.1f, 0f);
            groundFireShape.scale = new Vector3(7.8f, 0.16f, 0.1f);
            SetBursts(
                groundFire,
                new ParticleSystem.Burst(0f, 28),
                new ParticleSystem.Burst(0.58f, 20));
            SetColorOverLifetime(
                groundFire,
                new Color(1f, 0.52f, 0.08f),
                new Color(0.65f, 0.025f, 0f),
                0.9f,
                0f);
            SetSizeOverLifetime(
                groundFire,
                Curve((0f, 0.18f), (0.2f, 1f), (1f, 0.05f)));
            groundFire.GetComponent<ParticleSystemRenderer>().alignment =
                ParticleSystemRenderSpace.Velocity;
            SetDeterministic(groundFire, 8017);

            return SavePrefab(root, MegaExplosionPrefabPath);
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
            particleSystem.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
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
            GameObject iceLancePrefab,
            GameObject megaExplosionPrefab,
            GameObject flashbangPrefab)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);

            GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0.8f, -10f);
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.008f, 0.012f, 0.025f);
            camera.orthographic = true;
            camera.orthographicSize = 8.8f;
            camera.allowHDR = true;

            GameObject fireballObject = (GameObject)PrefabUtility.InstantiatePrefab(fireballPrefab, scene);
            fireballObject.transform.position = new Vector3(-10f, 3.2f, 0f);
            SkillParticleEffect fireball = fireballObject.GetComponent<SkillParticleEffect>();
            DisableAutoDestroy(fireball);

            GameObject earthObject = (GameObject)PrefabUtility.InstantiatePrefab(earthPrefab, scene);
            earthObject.transform.position = new Vector3(-6f, 3.2f, 0f);
            SkillParticleEffect earth = earthObject.GetComponent<SkillParticleEffect>();
            DisableAutoDestroy(earth);

            GameObject nailDrivingObject =
                (GameObject)PrefabUtility.InstantiatePrefab(nailDrivingPrefab, scene);
            nailDrivingObject.transform.position = new Vector3(-2f, 3.2f, 0f);
            SkillParticleEffect nailDriving = nailDrivingObject.GetComponent<SkillParticleEffect>();
            DisableAutoDestroy(nailDriving);

            GameObject plagueMagicObject =
                (GameObject)PrefabUtility.InstantiatePrefab(plagueMagicPrefab, scene);
            plagueMagicObject.transform.position = new Vector3(2f, 3.2f, 0f);
            SkillParticleEffect plagueMagic = plagueMagicObject.GetComponent<SkillParticleEffect>();
            DisableAutoDestroy(plagueMagic);

            GameObject iceLanceObject =
                (GameObject)PrefabUtility.InstantiatePrefab(iceLancePrefab, scene);
            iceLanceObject.transform.position = new Vector3(6f, 3.2f, 0f);
            SkillParticleEffect iceLance = iceLanceObject.GetComponent<SkillParticleEffect>();
            DisableAutoDestroy(iceLance);

            GameObject flashbangObject =
                (GameObject)PrefabUtility.InstantiatePrefab(flashbangPrefab, scene);
            flashbangObject.transform.position = new Vector3(10f, 3.2f, 0f);
            SkillParticleEffect flashbang = flashbangObject.GetComponent<SkillParticleEffect>();
            DisableAutoDestroy(flashbang);

            GameObject megaExplosionObject =
                (GameObject)PrefabUtility.InstantiatePrefab(megaExplosionPrefab, scene);
            megaExplosionObject.transform.position = new Vector3(0f, -4.2f, 0f);
            SkillParticleEffect megaExplosion =
                megaExplosionObject.GetComponent<SkillParticleEffect>();
            DisableAutoDestroy(megaExplosion);

            GameObject controllerObject = new GameObject("Preview Controller");
            SkillVfxPreviewController controller = controllerObject.AddComponent<SkillVfxPreviewController>();
            SerializedObject serializedController = new SerializedObject(controller);
            serializedController.FindProperty("fireballExplosion").objectReferenceValue = fireball;
            serializedController.FindProperty("earthMagic").objectReferenceValue = earth;
            serializedController.FindProperty("nailDriving").objectReferenceValue = nailDriving;
            serializedController.FindProperty("plagueMagic").objectReferenceValue = plagueMagic;
            serializedController.FindProperty("iceLance").objectReferenceValue = iceLance;
            serializedController.FindProperty("megaExplosion").objectReferenceValue = megaExplosion;
            serializedController.FindProperty("flashbang").objectReferenceValue = flashbang;
            serializedController.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, PreviewScenePath);
            EditorSceneManager.CloseScene(scene, true);
        }

        private static void UpdatePreviewScene(
            GameObject nailDrivingPrefab,
            GameObject plagueMagicPrefab,
            GameObject iceLancePrefab,
            GameObject megaExplosionPrefab,
            GameObject flashbangPrefab)
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
            SkillParticleEffect megaExplosion = null;
            SkillParticleEffect flashbang = null;
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
                        case "skill_008":
                            megaExplosion = effect;
                            break;
                        case "skill_010":
                            flashbang = effect;
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

            if (flashbang == null && flashbangPrefab != null)
            {
                GameObject flashbangObject =
                    (GameObject)PrefabUtility.InstantiatePrefab(flashbangPrefab, scene);
                flashbang = flashbangObject.GetComponent<SkillParticleEffect>();
                DisableAutoDestroy(flashbang);
            }

            if (megaExplosion == null && megaExplosionPrefab != null)
            {
                GameObject megaExplosionObject =
                    (GameObject)PrefabUtility.InstantiatePrefab(megaExplosionPrefab, scene);
                megaExplosion = megaExplosionObject.GetComponent<SkillParticleEffect>();
                DisableAutoDestroy(megaExplosion);
            }

            if (fireball != null)
            {
                fireball.transform.position = new Vector3(-10f, 3.2f, 0f);
            }

            if (earth != null)
            {
                earth.transform.position = new Vector3(-6f, 3.2f, 0f);
            }

            nailDriving.transform.position = new Vector3(-2f, 3.2f, 0f);
            plagueMagic.transform.position = new Vector3(2f, 3.2f, 0f);
            iceLance.transform.position = new Vector3(6f, 3.2f, 0f);
            if (flashbang != null)
            {
                flashbang.transform.position = new Vector3(10f, 3.2f, 0f);
            }

            if (megaExplosion != null)
            {
                megaExplosion.transform.position = new Vector3(0f, -4.2f, 0f);
            }

            Camera previewCamera = null;
            foreach (GameObject rootObject in scene.GetRootGameObjects())
            {
                previewCamera ??= rootObject.GetComponent<Camera>();
            }

            if (previewCamera != null)
            {
                previewCamera.transform.position = new Vector3(0f, 0.8f, -10f);
                previewCamera.orthographicSize = 8.8f;
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
            serializedController.FindProperty("megaExplosion").objectReferenceValue = megaExplosion;
            serializedController.FindProperty("flashbang").objectReferenceValue = flashbang;
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

        private static void CreateMegaExplosionTextures()
        {
            WritePixelTexture("MegaRune.png", 64, 32, SampleMegaRunePixel);
            WritePixelTexture("MegaBeam.png", 32, 64, SampleMegaBeamPixel);
            WritePixelTexture("MegaBlastCloud.png", 128, 128, SampleMegaBlastPixel);
            WritePixelTexture("MegaScorch.png", 64, 16, SampleMegaScorchPixel);
        }

        private static void CreateFlashbangTextures()
        {
            WriteTexture("RadiantStar.png", (x, y) =>
            {
                float radius = Mathf.Sqrt(x * x + y * y);
                float core = Mathf.Pow(Mathf.Clamp01(1f - radius), 4f);
                float horizontal = Mathf.Clamp01(1f - Mathf.Abs(y) * 72f) *
                                   Mathf.Pow(Mathf.Clamp01(1f - Mathf.Abs(x)), 1.7f);
                float vertical = Mathf.Clamp01(1f - Mathf.Abs(x) * 72f) *
                                 Mathf.Pow(Mathf.Clamp01(1f - Mathf.Abs(y)), 1.7f);
                float diagonalA = Mathf.Clamp01(1f - Mathf.Abs(y - x) * 46f) *
                                  Mathf.Pow(Mathf.Clamp01(1f - radius), 2.2f);
                float diagonalB = Mathf.Clamp01(1f - Mathf.Abs(y + x) * 46f) *
                                  Mathf.Pow(Mathf.Clamp01(1f - radius), 2.2f);
                float alpha = Mathf.Clamp01(
                    core * 1.25f +
                    horizontal * 0.9f +
                    vertical * 0.9f +
                    diagonalA * 0.55f +
                    diagonalB * 0.55f);
                float value = Mathf.Lerp(0.68f, 1f, Mathf.Clamp01(core * 3f + alpha * 0.5f));
                return new Color(value, value, value, alpha);
            });

            WriteTexture("LightBeam.png", (x, y) =>
            {
                float center = Mathf.Pow(Mathf.Clamp01(1f - Mathf.Abs(x)), 6f);
                float shoulder = Mathf.Pow(Mathf.Clamp01(1f - Mathf.Abs(x) * 1.7f), 2f);
                float taper = Mathf.Clamp01((y + 1f) * 5f) * Mathf.Clamp01((1f - y) * 3f);
                float strands = 0.72f + 0.28f * Mathf.Sin((x + 1f) * 46f + y * 7f);
                float alpha = Mathf.Clamp01((center + shoulder * 0.42f) * taper * strands);
                float value = Mathf.Lerp(0.72f, 1f, center);
                return new Color(value, value, value, alpha);
            });

            WriteTexture("GoldenBokeh.png", (x, y) =>
            {
                float radius = Mathf.Sqrt(x * x + y * y);
                float body = Mathf.Pow(Mathf.Clamp01(1f - radius), 1.7f);
                float rim = Mathf.Clamp01(1f - Mathf.Abs(radius - 0.68f) / 0.16f);
                float alpha = Mathf.Clamp01(body * 0.42f + rim * rim * 0.72f);
                float value = Mathf.Lerp(0.65f, 1f, body + rim * 0.3f);
                return new Color(value, value, value, alpha);
            });
        }

        private static Color SampleMegaRunePixel(int x, int y)
        {
            float normalizedX = (x - 31.5f) / 30.5f;
            float normalizedY = (y - 15.5f) / 13.5f;
            float radius = Mathf.Sqrt(
                normalizedX * normalizedX + normalizedY * normalizedY);
            float angle = Mathf.Atan2(normalizedY, normalizedX);
            int sector = Mathf.FloorToInt((angle + Mathf.PI) / (Mathf.PI * 2f) * 32f);

            bool outerRing = Mathf.Abs(radius - 0.91f) <= 0.055f && sector % 5 != 0;
            bool middleRing = Mathf.Abs(radius - 0.68f) <= 0.045f;
            bool innerRing = Mathf.Abs(radius - 0.38f) <= 0.05f && sector % 4 != 1;
            bool cardinal = radius >= 0.42f && radius <= 0.96f &&
                            Mathf.Abs(Mathf.Sin(angle * 4f)) <= 0.075f;
            bool runes = radius >= 0.76f && radius <= 0.84f &&
                         ((x * 3 + y * 5) % 11 <= 2 || sector % 4 == 0);
            bool core = radius <= 0.13f ||
                        radius <= 0.28f && Mathf.Abs(normalizedX) <= 0.045f ||
                        radius <= 0.28f && Mathf.Abs(normalizedY) <= 0.075f;
            if (!outerRing && !middleRing && !innerRing && !cardinal && !runes && !core)
            {
                return Color.clear;
            }

            float value = core
                ? 1f
                : outerRing || middleRing
                    ? 0.88f
                    : runes
                        ? 0.72f
                        : 0.56f;
            return new Color(value, value, value, 1f);
        }

        private static Color SampleMegaBeamPixel(int x, int y)
        {
            const int center = 15;
            int distanceX = Math.Abs(x - center);
            int jaggedWidth = 8 + ((y * 7) % 5 == 0 ? 2 : 0);
            if (y > 52)
            {
                jaggedWidth = Math.Max(2, jaggedWidth - (y - 52) / 2);
            }

            bool body = y >= 1 && y <= 62 && distanceX <= jaggedWidth;
            bool detachedRay = y >= 6 && y <= 57 &&
                               (Math.Abs(x - 3 - y / 13) <= 1 ||
                                Math.Abs(x - 28 + y / 15) <= 1) &&
                               (x + y) % 4 != 0;
            if (!body && !detachedRay)
            {
                return Color.clear;
            }

            float value = detachedRay && !body
                ? 0.38f
                : distanceX <= 2
                    ? 1f
                    : distanceX <= 5
                        ? 0.78f
                        : 0.42f;
            if (body && (x * 5 + y * 3) % 23 == 0)
            {
                value *= 0.58f;
            }

            return new Color(value, value, value, 1f);
        }

        private static Color SampleMegaBlastPixel(int x, int y)
        {
            const float sourceScale = 63f / 127f;
            float sourceX = x * sourceScale;
            float sourceY = y * sourceScale;
            float noise = PixelHash01(x / 2, y / 2);
            bool smoke =
                InsidePixelCircle(sourceX, sourceY, 11f, 23f, 9f) ||
                InsidePixelCircle(sourceX, sourceY, 20f, 31f, 13f) ||
                InsidePixelCircle(sourceX, sourceY, 33f, 34f, 16f) ||
                InsidePixelCircle(sourceX, sourceY, 47f, 31f, 13f) ||
                InsidePixelCircle(sourceX, sourceY, 55f, 23f, 9f) ||
                InsidePixelCircle(sourceX, sourceY, 19f, 46f, 11f) ||
                InsidePixelCircle(sourceX, sourceY, 33f, 49f, 15f) ||
                InsidePixelCircle(sourceX, sourceY, 47f, 46f, 11f) ||
                InsidePixelCircle(sourceX, sourceY, 32f, 60f, 8f);

            bool flame =
                InsidePixelCircle(sourceX, sourceY, 32f, 9f, 18f) ||
                InsidePixelCircle(sourceX, sourceY, 16f, 15f, 11f) ||
                InsidePixelCircle(sourceX, sourceY, 49f, 16f, 12f) ||
                InsidePixelCircle(sourceX, sourceY, 23f, 24f, 13f) ||
                InsidePixelCircle(sourceX, sourceY, 42f, 26f, 14f) ||
                InsidePixelCircle(sourceX, sourceY, 31f, 37f, 13f) ||
                InsidePixelCircle(sourceX, sourceY, 24f, 48f, 7f) ||
                InsidePixelCircle(sourceX, sourceY, 39f, 48f, 9f) ||
                InsidePixelCircle(sourceX, sourceY, 33f, 57f, 6f);

            bool groundBurst = sourceY >= 2f && sourceY <= 10f &&
                               Mathf.Abs(sourceX - 31f) <= 29f - (sourceY - 2f) * 2f;
            if (smoke && sourceY > 16f && noise < 0.08f)
            {
                smoke = false;
            }

            if (flame && sourceY > 12f && noise < 0.035f)
            {
                flame = false;
            }

            if (!smoke && !flame && !groundBurst)
            {
                return Color.clear;
            }

            bool smokeOccludesFlame = smoke && sourceY > 24f && noise < 0.46f;
            if (smokeOccludesFlame || (smoke && !flame && !groundBurst))
            {
                if (sourceY < 36f && noise > 0.78f)
                {
                    return new Color32(126, 51, 23, 245);
                }

                if (noise < 0.28f)
                {
                    return new Color32(37, 31, 31, 240);
                }

                return noise < 0.68f
                    ? new Color32(67, 51, 48, 242)
                    : new Color32(91, 65, 57, 238);
            }

            if (flame || groundBurst)
            {
                float flameCenter = 32f + Mathf.Sin(sourceY * 0.33f) * 1.7f;
                float axisDistance = Mathf.Abs(sourceX - flameCenter);
                bool whiteCore = sourceY <= 13f && axisDistance <= 2.6f + noise * 1.4f;
                bool yellowCore = sourceY <= 36f &&
                                  axisDistance <= Mathf.Max(2.5f, 9f - sourceY * 0.15f + noise * 2.4f);
                if (whiteCore)
                {
                    return new Color32(255, 250, 190, 255);
                }

                if (yellowCore || noise > 0.82f)
                {
                    return new Color32(255, 173, 24, 255);
                }

                return noise < 0.23f
                    ? new Color32(188, 38, 5, 255)
                    : noise < 0.58f
                        ? new Color32(240, 61, 5, 255)
                        : new Color32(255, 101, 6, 255);
            }

            return Color.clear;
        }

        private static Color SampleMegaScorchPixel(int x, int y)
        {
            float normalizedX = (x - 31.5f) / 31f;
            float normalizedY = (y - 6.5f) / 5.5f;
            float distance = normalizedX * normalizedX + normalizedY * normalizedY;
            bool ground = y >= 1 && y <= 13 && distance <= 1f;
            if (ground && distance > 0.76f && (x * 5 + y * 7) % 9 <= 1)
            {
                ground = false;
            }

            if (!ground)
            {
                return Color.clear;
            }

            bool centerCrack = y >= 2 && y <= 10 &&
                               Math.Abs(x - 31 + y % 3 - 1) <= 1;
            bool radialCrack = Math.Abs(y - 6 - Math.Abs(x - 31) / 7) <= 1 ||
                               Math.Abs(y - 9 + Math.Abs(x - 31) / 9) <= 1;
            bool smallCrack = (x * 3 + y * 5) % 29 == 0;
            float ashNoise = PixelHash01(x, y);
            if (centerCrack)
            {
                return new Color32(255, 151, 12, 255);
            }

            if ((radialCrack || smallCrack) && ashNoise > 0.58f)
            {
                return ashNoise > 0.82f
                    ? new Color32(229, 54, 6, 220)
                    : new Color32(118, 38, 20, 165);
            }

            if (ashNoise < 0.17f && distance < 0.82f)
            {
                return ashNoise < 0.07f
                    ? new Color32(27, 22, 22, 150)
                    : new Color32(55, 33, 26, 115);
            }

            return Color.clear;
        }

        private static bool InsidePixelCircle(
            float x,
            float y,
            float centerX,
            float centerY,
            float radius)
        {
            float deltaX = x - centerX;
            float deltaY = y - centerY;
            return deltaX * deltaX + deltaY * deltaY <= radius * radius;
        }

        private static float PixelHash01(int x, int y)
        {
            unchecked
            {
                uint value = (uint)(x * 374761393 + y * 668265263);
                value = (value ^ (value >> 13)) * 1274126177u;
                return (value & 1023u) / 1023f;
            }
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
