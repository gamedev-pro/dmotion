using DOTSAnimation.Authoring;
using UnityEditor;
using UnityEngine;

namespace DOTSAnimation.Editor
{
    [CustomEditor(typeof(AnimationClipAsset))]
    public class AnimationClipAssetEditor : UnityEditor.Editor
    {
        private PreviewRenderUtility previewRenderUtility;
        private GameObject gameObject;
        private SkinnedMeshRenderer skinnedMeshRenderer;
        private Mesh previewMesh;
        private float sampleNormalizedTime;
        private GameObject AnimatorRoot => skinnedMeshRenderer.transform.root.gameObject;

        private AnimationClipAsset ClipTarget => (AnimationClipAsset)target;

        public override void OnInspectorGUI()
        {
            using (var c = new EditorGUI.ChangeCheckScope())
            {
                base.OnInspectorGUI();
                if (c.changed)
                {
                    RefreshPreviewObjects();
                }
            }
            using (var c = new EditorGUI.ChangeCheckScope())
            {
                gameObject = (GameObject) EditorGUILayout.ObjectField(gameObject, typeof(GameObject), true);
                if (c.changed)
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
                        DestroyPreviewInstance();
                    }
                }
            }
            
            sampleNormalizedTime = EditorGUILayout.Slider(sampleNormalizedTime, 0, 1);
        }


        private bool IsValidGameObject(GameObject obj)
        {
            return obj.GetComponentInChildren<SkinnedMeshRenderer>() != null;
        }

        private bool TryInstantiateSkinnedMesh(GameObject template)
        {
            if (!IsValidGameObject(template))
            {
                return false;
            }

            DestroyPreviewInstance();

            var instance = Instantiate(template, Vector3.zero, Quaternion.identity);
            instance.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild | HideFlags.HideInHierarchy |
                                 HideFlags.HideInInspector | HideFlags.NotEditable;
            skinnedMeshRenderer = instance.GetComponentInChildren<SkinnedMeshRenderer>();
            instance.SetActive(false);

            previewMesh = new Mesh();
            CreatePreviewUtility();
            
            return true;
        }

        private void DestroyPreviewInstance()
        {
            if (skinnedMeshRenderer != null)
            {
                DestroyImmediate(skinnedMeshRenderer.transform.root.gameObject);
                skinnedMeshRenderer = null;
                DestroyImmediate(previewMesh);
                previewMesh = null;
            }
        }

        private void CreatePreviewUtility()
        {
            previewRenderUtility?.Cleanup();
            previewRenderUtility = new PreviewRenderUtility();
            
            var bounds = skinnedMeshRenderer.bounds;
            var camPos = new Vector3(0f, bounds.size.y*3, 10f);
            previewRenderUtility.camera.transform.position = camPos;
            previewRenderUtility.camera.transform.rotation = Quaternion.LookRotation(bounds.center - camPos);
            previewRenderUtility.camera.nearClipPlane = 0.3f;
            previewRenderUtility.camera.farClipPlane = 3000f;
        }

        private void OnEnable()
        {
            RefreshPreviewObjects();
        }

        private void RefreshPreviewObjects()
        {
            if (gameObject != null)
            {
                TryInstantiateSkinnedMesh(gameObject);
            }
            else if (ClipTarget.Clip != null)
            {
                if (TryFindSkeletonFromClip(ClipTarget.Clip, out var armatureGo))
                {
                    if (TryInstantiateSkinnedMesh(armatureGo))
                    {
                        gameObject = armatureGo;
                    }
                }
            }
        }

        private bool TryFindSkeletonFromClip(AnimationClip clip, out GameObject armatureGo)
        {
            var path = AssetDatabase.GetAssetPath(ClipTarget.Clip);
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

        private void OnDisable()
        {
            previewRenderUtility?.Cleanup();
        }

        public override bool HasPreviewGUI()
        {
            return true;
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            if (skinnedMeshRenderer != null && ClipTarget.Clip != null)
            {
                AnimationMode.BeginSampling();
                AnimationMode.SampleAnimationClip(AnimatorRoot, ClipTarget.Clip, sampleNormalizedTime*ClipTarget.Clip.length);
                AnimationMode.EndSampling();

                {
                    skinnedMeshRenderer.BakeMesh(previewMesh);
                    previewRenderUtility.BeginPreview(r, background);

                    for (var i = 0; i < previewMesh.subMeshCount; i++)
                    {
                        previewRenderUtility.DrawMesh(previewMesh, Matrix4x4.identity, skinnedMeshRenderer.sharedMaterial, i);
                    }
                    previewRenderUtility.camera.Render();
                    
                    var resultRender = previewRenderUtility.EndPreview();
                    GUI.DrawTexture(r, resultRender, ScaleMode.StretchToFill, false);
                }
            }
        }
    }
}