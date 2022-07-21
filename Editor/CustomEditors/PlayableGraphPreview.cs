using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace DOTSAnimation.Editor
{
    public abstract class PlayableGraphPreview : IDisposable
    {
        private PreviewRenderUtility previewRenderUtility;
        private GameObject gameObject;
        protected Animator animator;
        private SkinnedMeshRenderer skinnedMeshRenderer;
        private Mesh previewMesh;
        private PlayableGraph playableGraph;
        public GameObject GameObject
        {
            get => gameObject;
            set => SetGameObjectPreview(value);
        }
        
        protected abstract PlayableGraph BuildGraph();
        protected abstract IEnumerable<AnimationClip> Clips { get; }
        protected abstract float SampleTime { get; }

        public void Initialize()
        {
            AnimationMode.StartAnimationMode();
            RefreshPreviewObjects();
        }
        private void SetGameObjectPreview(GameObject newValue)
        {
            if (gameObject == newValue)
            {
                return;
            }
            gameObject = newValue;
            if (gameObject != null)
            {
                if (!TryInstantiateSkinnedMesh(gameObject))
                {
                    DestroyPreviewInstance();
                    gameObject = null;
                }
            }
            else
            {
                DestroyPreviewInstance();
            }
        }
        private bool IsValidGameObject(GameObject obj)
        {
            return obj.GetComponentInChildren<Animator>() != null;
        }
        private void DestroyPreviewInstance()
        {
            if (skinnedMeshRenderer != null)
            {
                Object.DestroyImmediate(skinnedMeshRenderer.transform.root.gameObject);
                skinnedMeshRenderer = null;
                Object.DestroyImmediate(previewMesh);
                previewMesh = null;
            }
        }


        private bool TryInstantiateSkinnedMesh(GameObject template)
        {
            if (!IsValidGameObject(template))
            {
                return false;
            }

            DestroyPreviewInstance();

            var instance = Object.Instantiate(template, Vector3.zero, Quaternion.identity);
            
            AnimatorUtility.DeoptimizeTransformHierarchy(instance);
            
            instance.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild | HideFlags.HideInHierarchy |
                                 HideFlags.HideInInspector | HideFlags.NotEditable;
            // leaving this here for debug purposes
            // instance.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            animator = instance.GetComponentInChildren<Animator>();
            skinnedMeshRenderer = instance.GetComponentInChildren<SkinnedMeshRenderer>();
            instance.SetActive(false);

            previewMesh = new Mesh();

            if (playableGraph.IsValid())
            {
                playableGraph.Destroy();
            }

            playableGraph = BuildGraph();
            
            CreatePreviewUtility();

            return true;
        }

        private void CreatePreviewUtility()
        {
            previewRenderUtility?.Cleanup();
            previewRenderUtility = new PreviewRenderUtility();

            var bounds = skinnedMeshRenderer.bounds;
            var camPos = new Vector3(0f, bounds.size.y * 3, 10f);
            previewRenderUtility.camera.transform.position = camPos;
            var camRot = Quaternion.LookRotation(bounds.center - camPos);
            previewRenderUtility.camera.transform.rotation = camRot;
            previewRenderUtility.camera.nearClipPlane = 0.3f;
            previewRenderUtility.camera.farClipPlane = 3000f;

            var light1 = previewRenderUtility.lights[0];
            light1.type = LightType.Directional;
            light1.color = Color.white;
            light1.intensity = 1;
            light1.transform.rotation = camRot;
        }
        public void RefreshPreviewObjects()
        {
            if (gameObject != null)
            {
                if (!TryInstantiateSkinnedMesh(gameObject))
                {
                    gameObject = null;
                }
            }
            else
            {
                foreach (var clip in Clips)
                {
                    if (TryFindSkeletonFromClip(clip, out var armatureGo))
                    {
                        if (TryInstantiateSkinnedMesh(armatureGo))
                        {
                            gameObject = armatureGo;
                            break;
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            AnimationMode.StopAnimationMode();
            previewRenderUtility?.Cleanup();
            DestroyPreviewInstance();
        }
        
        private bool TryFindSkeletonFromClip(AnimationClip Clip, out GameObject armatureGo)
        {
            var path = AssetDatabase.GetAssetPath(Clip);
            var owner = AssetDatabase.LoadMainAssetAtPath(path);
            var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(owner)) as ModelImporter;
            if (importer != null && importer.sourceAvatar != null)
            {
                var avatarPath = AssetDatabase.GetAssetPath(importer.sourceAvatar);
                var avatarOwner = AssetDatabase.LoadMainAssetAtPath(avatarPath);
                if (avatarOwner is GameObject go)
                {
                    armatureGo = go;
                    return IsValidGameObject(go);
                }
            }

            armatureGo = null;
            return false;
        }
        
        public void DrawPreview(Rect r, GUIStyle background)
        {
            if (skinnedMeshRenderer != null && playableGraph.IsValid())
            {
                Assert.IsTrue(AnimationMode.InAnimationMode(), "AnimationMode disabled, make sure to call Initialize");
                AnimationMode.BeginSampling();
                AnimationMode.SamplePlayableGraph(playableGraph, 0, SampleTime);
                AnimationMode.EndSampling();
                {
                    skinnedMeshRenderer.BakeMesh(previewMesh);
                    previewRenderUtility.BeginPreview(r, background);

                    for (var i = 0; i < previewMesh.subMeshCount; i++)
                    {
                        previewRenderUtility.DrawMesh(previewMesh, Matrix4x4.identity,
                            skinnedMeshRenderer.sharedMaterial, i);
                    }

                    previewRenderUtility.camera.Render();

                    var resultRender = previewRenderUtility.EndPreview();
                    GUI.DrawTexture(r, resultRender, ScaleMode.StretchToFill, false);
                }
            }
        }
    }
}