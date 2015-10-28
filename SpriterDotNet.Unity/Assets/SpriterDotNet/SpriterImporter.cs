﻿// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

#if UNITY_EDITOR

using SpriterDotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SpriterDotNetUnity
{
    public class SpriterImporter : AssetPostprocessor
    {
        public static event Action<GameObject> EntityImported = e => { };

        private static readonly string[] ScmlExtensions = new string[] { ".scml" };
        private static readonly float DeltaZ = -0.001f;

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromPath)
        {
            foreach (string asset in importedAssets)
            {
                if (!IsScml(asset)) continue;
                CreateSpriter(asset);
            }

            foreach (string asset in deletedAssets)
            {
                if (!IsScml(asset)) continue;
            }

            for (int i = 0; i < movedAssets.Length; i++)
            {
                string asset = movedAssets[i];
                if (!IsScml(asset)) continue;
            }
        }

        private static bool IsScml(string path)
        {
            return ScmlExtensions.Any(path.EndsWith);
        }

        private static void CreateSpriter(string path)
        {
            string data = File.ReadAllText(path);
            Spriter spriter = SpriterParser.Parse(data);
            string rootFolder = Path.GetDirectoryName(path);

            string name = Path.GetFileNameWithoutExtension(path);
            SpriterData spriterData = CreateSpriterData(spriter, rootFolder, name);

            foreach (SpriterEntity entity in spriter.Entities)
            {
                GameObject go = new GameObject(entity.Name, typeof(AudioSource), typeof(SpriterDotNetBehaviour));
                GameObject sprites = new GameObject("Sprites");
                GameObject metadata = new GameObject("Metadata");

                sprites.SetParent(go);
                metadata.SetParent(go);

                ChildData cd = new ChildData();
                CreateSprites(entity, cd, spriter, sprites);
                CreateCollisionRectangles(entity, cd, spriter, metadata);
                CreatePoints(entity, cd, spriter, metadata);

                SpriterDotNetBehaviour behaviour = go.GetComponent<SpriterDotNetBehaviour>();
                behaviour.EntityIndex = entity.Id;
                behaviour.enabled = true;
                behaviour.SpriterData = spriterData;
                behaviour.ChildData = cd;

                CreatePrefab(go, rootFolder);

                EntityImported(go);
            }

            CreateTags(spriter);
        }

        private static SpriterData CreateSpriterData(Spriter spriter, string rootFolder, string name)
        {
            SpriterData data = ScriptableObject.CreateInstance<SpriterData>();
            data.Spriter = spriter;
            data.FileEntries = LoadAssets(spriter, rootFolder).ToArray();

            AssetDatabase.CreateAsset(data, rootFolder + "/" + name + ".asset");
            AssetDatabase.SaveAssets();

            return data;
        }

        private static void CreatePrefab(GameObject go, string folder)
        {
            string prefabPath = folder + "/" + go.name + ".prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (existing != null) PrefabUtility.ReplacePrefab(go, existing, ReplacePrefabOptions.Default);
            else PrefabUtility.CreatePrefab(prefabPath, go, ReplacePrefabOptions.Default);

            GameObject.DestroyImmediate(go);
        }

        private static void CreateSprites(SpriterEntity entity, ChildData cd, Spriter spriter, GameObject parent)
        {
            int maxObjects = GetDrawablesCount(entity);

            cd.Sprites = new GameObject[maxObjects];
            cd.SpritePivots = new GameObject[maxObjects];

            for (int i = 0; i < maxObjects; ++i)
            {
                GameObject pivot = new GameObject("Pivot " + i);
                GameObject child = new GameObject("Sprite " + i);

                pivot.SetParent(parent);
                child.SetParent(pivot);

                cd.SpritePivots[i] = pivot;
                cd.Sprites[i] = child;

                child.transform.localPosition = new Vector3(0, 0, DeltaZ * i);

                child.AddComponent<SpriteRenderer>();
            }
        }

        private static void CreateCollisionRectangles(SpriterEntity entity, ChildData cd, Spriter spriter, GameObject parent)
        {
            if (entity.ObjectInfos == null) return;
            var boxes = entity.ObjectInfos.Where(o => o.ObjectType == SpriterObjectType.Box).ToList();
            if (boxes.Count == 0) return;

            GameObject boxRoot = new GameObject("Boxes");
            boxRoot.SetParent(parent);

            cd.BoxPivots = new GameObject[boxes.Count];
            cd.Boxes = new GameObject[boxes.Count];

            for (int i = 0; i < boxes.Count; ++i)
            {
                GameObject pivot = new GameObject("Pivot " + i);
                GameObject child = new GameObject("Box " + i);

                pivot.SetParent(boxRoot);
                child.SetParent(pivot);

                cd.BoxPivots[i] = pivot;
                cd.Boxes[i] = child;

                child.AddComponent<BoxCollider2D>();
            }
        }

        private static void CreatePoints(SpriterEntity entity, ChildData cd, Spriter spriter, GameObject parent)
        {
            GameObject pointRoot = new GameObject("Points");
            pointRoot.SetParent(parent);

            int count = GetPointsCount(entity);

            cd.Points = new GameObject[count];

            for (int i = 0; i < count; ++i)
            {
                GameObject point = new GameObject("Point " + i);
                point.SetParent(pointRoot);
                cd.Points[i] = point;
            }
        }

        private static IEnumerable<SdnFileEntry> LoadAssets(Spriter spriter, string rootFolder)
        {
            for (int i = 0; i < spriter.Folders.Length; ++i)
            {
                SpriterFolder folder = spriter.Folders[i];

                for (int j = 0; j < folder.Files.Length; ++j)
                {
                    SpriterFile file = folder.Files[j];
                    string path = rootFolder;
                    path += "/";
                    path += file.Name;

                    SdnFileEntry entry = new SdnFileEntry
                    {
                        FolderId = folder.Id,
                        FileId = file.Id
                    };

                    if (file.Type == SpriterFileType.Sound) entry.Sound = LoadContent<AudioClip>(path);
                    else entry.Sprite = LoadContent<Sprite>(path);

                    yield return entry;
                }
            }
        }

        private static T LoadContent<T>(string path) where T : UnityEngine.Object
        {
            T asset;
            asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null) Debug.Log("Missing Asset: " + path);

            return asset;
        }

        private static void CreateTags(Spriter spriter)
        {
            if (spriter.Tags == null) return;

            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tags = tagManager.FindProperty("tags");

            foreach(SpriterElement tag in spriter.Tags)
            {
                for (int i = 0; i < tags.arraySize; i++)
                {
                    SerializedProperty t = tags.GetArrayElementAtIndex(i);
                    if (t.stringValue.Equals(tag.Name)) continue;
                }

                ++tags.arraySize;
                SerializedProperty newEntry = tags.GetArrayElementAtIndex(tags.arraySize - 1);
                newEntry.stringValue = tag.Name;
            }

            tagManager.ApplyModifiedProperties();
        }

        public static void AddTag(SerializedProperty tags, string value)
        {
           
        }

        public static int GetDrawablesCount(SpriterEntity entity)
        {
            int drawablesCount = 0;

            foreach (SpriterAnimation animation in entity.Animations)
            {
                int count = GetDrawablesCount(animation);
                drawablesCount = Math.Max(drawablesCount, count);
            }

            return drawablesCount;
        }

        public static int GetDrawablesCount(SpriterAnimation animation)
        {
            int drawablesCount = 0;

            foreach (SpriterMainlineKey key in animation.MainlineKeys)
            {
                int countForKey = GetDrawablesCount(animation, key);
                drawablesCount = Math.Max(drawablesCount, countForKey);
            }

            return drawablesCount;
        }

        private static int GetDrawablesCount(SpriterAnimation animation, SpriterMainlineKey key)
        {
            int drawablesCount = 0;

            foreach (SpriterObjectRef obj in key.ObjectRefs)
            {
                SpriterTimeline timeline = animation.Timelines[obj.TimelineId];
                if (timeline.ObjectType == SpriterObjectType.Sprite) ++drawablesCount;
                else if (timeline.ObjectType == SpriterObjectType.Entity)
                {
                    Spriter spriter = animation.Entity.Spriter;
                    HashSet<SpriterAnimation> animations = new HashSet<SpriterAnimation>();
                    foreach (SpriterTimelineKey timelineKey in timeline.Keys)
                    {
                        SpriterObject spriterObject = timelineKey.ObjectInfo;
                        SpriterAnimation newAnim = spriter.Entities[spriterObject.EntityId].Animations[spriterObject.AnimationId];
                        if (!animations.Contains(newAnim)) animations.Add(newAnim);
                    }
                    IEnumerable<int> drawableCount = animations.Select<SpriterAnimation, int>(GetDrawablesCount);
                    drawablesCount += drawableCount.Max();
                }
            }

            return drawablesCount;
        }

        private static int GetPointsCount(SpriterEntity entity)
        {
            int count = 0;

            foreach (SpriterAnimation animation in entity.Animations)
            {
                int countForAnim = animation.Timelines.Where(t => t.ObjectType == SpriterObjectType.Point).Count();
                count = Math.Max(count, countForAnim);
            }

            return count;
        }
    }

    internal static class SpriterImporterUtil
    {
        public static void SetParent(this GameObject child, GameObject parent)
        {
            child.transform.SetParent(parent.transform);
        }
    }
}
#endif