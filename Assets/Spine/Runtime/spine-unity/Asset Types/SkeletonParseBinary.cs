using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Spine.Unity
{
    public class SkeletonParseBinary
    {

        #region 合并 SkeletonData

        /// <summary>
        ///  将拆分的 Skeleton 数据组装成一个
        /// </summary>
        public static SkeletonData CombineSkeleton(AttachmentLoader loader, float scale, params byte[][] skelBytes)
        {
            var skelData = new SkeletonData[skelBytes.Length];
            for (int i = 0; i < skelBytes.Length; i++)
            {
                using (var stream = new System.IO.MemoryStream(skelBytes[i]))
                {
                    using (var reader = new System.IO.BinaryReader(stream))
                    {
                        skelData[i] = ParseSkeletonData(reader, loader, scale);
                    }
                }
            }

            return CombineSkeleton(skelData);
        }

        /// <summary>
        ///  合并多个 SkeletonData 数据
        ///     TODO:  一边读二进制文件一边合并,  而不是 创建完所有 SkeletonData 再合并
        /// </summary>
        public static SkeletonData CombineSkeleton(SkeletonData[] skelDatas)
        {
            if (skelDatas.Length <= 0) return null;

            // 将所有数据合并到第一个 SkeletonData中
            var skelData = new SkeletonData();
            skelData.Hash = skelDatas[0].Hash;
            skelData.Version = skelDatas[0].Version;
            skelData.X = skelDatas[0].X;
            skelData.Y = skelDatas[0].Y;
            skelData.Width = skelDatas[0].Width;
            skelData.Height = skelDatas[0].Height;
            skelData.ReferenceScale = skelDatas[0].ReferenceScale;
            skelData.Fps = skelDatas[0].Fps;
            skelData.ImagesPath = skelDatas[0].ImagesPath;
            skelData.AudioPath = skelDatas[0].AudioPath;


            // 骨骼
            var boneCount = skelDatas[0].Bones.Count;
            skelData.Bones.Resize(boneCount);
            var bones = skelData.Bones.Items;
            for (int i = 0; i < boneCount; i++)
            {
                if (bones[i] != null) continue;

                for (int j = 0; j < skelDatas.Length; j++)
                {
                    var tbs = skelDatas[j].Bones.Items;
                    if (i < tbs.Length && tbs[i] != null && i == tbs[i].Index)
                    {
                        var parent = tbs[i].Parent == null ? null : skelData.FindBone(tbs[i].Parent.Name);
                        var bone = new BoneData(i, tbs[i].Name, parent);
                        bone.Length = tbs[i].Length;
                        bone.X = tbs[i].X;
                        bone.Y = tbs[i].Y;
                        bone.Rotation = tbs[i].Rotation;
                        bone.ScaleX = tbs[i].ScaleX;
                        bone.ScaleY = tbs[i].ScaleY;
                        bone.ShearX = tbs[i].ShearX;
                        bone.ShearY = tbs[i].ShearY;
                        bone.Inherit = tbs[i].Inherit;
                        bone.SkinRequired = tbs[i].SkinRequired;
                        bones[i] = bone;
                        break;
                    }
                }
            }

            // ik.   所有皮肤都有ik数据
            var ikCount = skelDatas[0].IkConstraints.Count;
            var ikDatas = skelDatas[0].IkConstraints.Items;
            for (int i = 0; i < ikCount; i++)
            {
                var ikData = new IkConstraintData(ikDatas[i].Name);
                ikData.Order = ikDatas[i].Order;
                ikData.SkinRequired = ikDatas[i].SkinRequired;
                foreach (var rawIkBone in ikDatas[i].Bones)
                {
                    ikData.Bones.Add(skelData.FindBone(rawIkBone.Name));
                }
                ikData.Target = skelData.FindBone(ikDatas[i].Target.Name);
                ikData.Mix = ikDatas[i].Mix;
                ikData.Softness = ikDatas[i].Softness;
                ikData.BendDirection = ikDatas[i].BendDirection;
                ikData.Compress = ikDatas[i].Compress;
                ikData.Stretch = ikDatas[i].Stretch;
                ikData.Uniform = ikDatas[i].Uniform;

                skelData.IkConstraints.Add(ikData);
            }

            // slot
            var slotCount = skelDatas[0].Slots.Count;
            skelData.Slots.Resize(slotCount);
            var slots = skelData.Slots.Items;
            for (int i = 0; i < slotCount; i++)
            {
                if (slots[i] != null) continue;

                for (int j = 0; j < skelDatas.Length; j++)
                {
                    var tss = skelDatas[j].Slots.Items;
                    if (i < tss.Length && tss[i] != null && i == tss[i].Index)
                    {
                        var boneName = tss[i].BoneData.Name;
                        var boneData = skelData.FindBone(boneName);
                        var slot = new SlotData(i, tss[i].Name, boneData);
                        slot.R = tss[i].R;
                        slot.G = tss[i].G;
                        slot.B = tss[i].B;
                        slot.A = tss[i].A;

                        slot.R2 = tss[i].R2;
                        slot.G2 = tss[i].G2;
                        slot.B2 = tss[i].B2;

                        slot.AttachmentName = tss[i].AttachmentName;
                        slot.BlendMode = tss[i].BlendMode;
                        slots[i] = slot;
                        break;
                    }
                }
            }

            // skin
            var skin = new Skin(skelDatas[0].DefaultSkin.Name); // 合并后  使用第一个皮肤的名称
            skelData.Skins.Add(skin);
            skelData.DefaultSkin = skin;
            for (int j = 0; j < skelDatas.Length; j++)
            {
                AddSkin(skelDatas[j].DefaultSkin, skelData.DefaultSkin, skelData);
            }

            // aniamtion
            var animDict = new Dictionary<string, List<Timeline>>();
            var animTypeBoneIndexDict = new Dictionary<string, HashSet<(int, int)>>();
            for (int j = 0; j < skelDatas.Length; j++)
            {
                var anims = skelDatas[j].Animations.Items;
                foreach (var a in anims)
                {
                    HashSet<(int, int)> boneIndexTypes = null;
                    if (!animDict.TryGetValue(a.Name, out var aList))
                    {
                        aList = new List<Timeline>();
                        animDict[a.Name] = aList;

                        boneIndexTypes = new HashSet<(int, int)>();
                        animTypeBoneIndexDict[a.Name] = boneIndexTypes;
                    }else
                        boneIndexTypes = animTypeBoneIndexDict[a.Name];

                    // 过滤重复骨骼
                    foreach (var tl in a.Timelines)
                    {
                        // 过滤重复的Bone动画
                        if (tl is IBoneTimeline boneTimeline)
                        {
                            var tlTypeIndex = (tl.GetType().GetHashCode(), boneTimeline.BoneIndex);
                            if (!boneIndexTypes.Contains(tlTypeIndex))
                            {
                                aList.Add(tl);
                                boneIndexTypes.Add(tlTypeIndex);
                            }
//#if UNITY_EDITOR
//                            else
//                                Debug.Log($"CombineSkeleton  过滤掉的动画:  {tl.GetType()}--{(tl as IBoneTimeline).BoneIndex}");
//#endif
                            continue;
                        }
                        else
                            aList.Add(tl);
                    }
                }
            }
            var animations = new ExposedList<Animation>();
            foreach (var item in animDict)
            {
                var timelines = new ExposedList<Timeline>();
                timelines.AddRange(item.Value);
                var duration = 0f;
                foreach (var a in timelines)
                {
                    duration = Mathf.Max(duration, a.Duration);
                }
                //animations.Add(new Animation(item.Key, timelines, duration));

                var anim = new Animation(item.Key, timelines, duration);
                animations.Add(anim);
            }
            skelData.Animations = animations;

            skelData.Animations.TrimExcess();
            skelData.IkConstraints.TrimExcess();


            return skelData;
        }

        private static void AddSkin(Skin src, Skin dst, SkeletonData skel)
        {
            // 在 新的SkeletonData中查找皮肤 使用的骨骼引用
            foreach (BoneData data in src.Bones)
            {
                if (!dst.Bones.Exists(b => b != null && b.Index == data.Index))
                {
                    dst.Bones.Add(skel.FindBone(data.Name));
                }
            }

            foreach (Skin.SkinEntry item in src.Attachments)
            {
                dst.SetAttachment(item.SlotIndex, item.Name, item.Attachment);
            }
        }

        #endregion


        #region 加载 SkeletonData

        /// <summary>
        ///  自定义二进制文件加载 Skeleton数据
        /// </summary>
        public static SkeletonData ParseSkeletonData(System.IO.BinaryReader reader, AttachmentLoader loader, float scale)
        {
            var skeletonData = new SkeletonData();

            skeletonData.Hash = reader.ReadString();
            skeletonData.Version = reader.ReadString();
            skeletonData.X = reader.ReadSingle();
            skeletonData.Y = reader.ReadSingle();
            skeletonData.Width = reader.ReadSingle();
            skeletonData.Height = reader.ReadSingle();
            skeletonData.ReferenceScale = reader.ReadSingle();
            skeletonData.Fps = reader.ReadSingle();
            skeletonData.ImagesPath = reader.ReadString();
            skeletonData.AudioPath = reader.ReadString();

            var maxBoneCount = reader.ReadInt32();
            var maxSlotCount = reader.ReadInt32();

            // bones
            var boneCount = reader.ReadInt32();
            var boneIndex = reader.ReadInt32(); // 读取骨骼索引
            var readIndex = 0;
            for (int i = 0; i < maxBoneCount; i++)
            {
                if (i == boneIndex)
                {
                    var boneName = reader.ReadString();
                    var parentName = reader.ReadString();
                    var parent = string.IsNullOrEmpty(parentName) ? null : skeletonData.FindBone(parentName);
                    var bone = new BoneData(boneIndex, boneName, parent);
                    bone.Length = reader.ReadSingle() * scale;
                    bone.X = reader.ReadSingle() * scale;
                    bone.Y = reader.ReadSingle() * scale;
                    bone.Rotation = reader.ReadSingle();
                    bone.ScaleX = reader.ReadSingle();
                    bone.ScaleY = reader.ReadSingle();
                    bone.ShearX = reader.ReadSingle();
                    bone.ShearY = reader.ReadSingle();
                    bone.Inherit = (Inherit)Enum.Parse(typeof(Inherit), reader.ReadString(), true);
                    bone.SkinRequired = reader.ReadBoolean();
                    skeletonData.Bones.Add(bone);

                    // 读取下一个 骨骼数据
                    readIndex++;
                    if (readIndex < boneCount)
                        boneIndex = reader.ReadInt32(); // 读取骨骼索引
                    else
                        boneIndex = -1;
                }
                else
                    skeletonData.Bones.Add(null);
            }

            // slots
            var slotCount = reader.ReadInt32();
            var slotIndex = reader.ReadInt32();
            readIndex = 0;
            for (int i = 0; i < maxSlotCount; i++)
            {
                if (i == slotIndex)
                {
                    var slotName = reader.ReadString();
                    int slash = slotName.LastIndexOf('/');
                    if (slash != -1)
                    {
                        slotName = slotName.Substring(slash + 1);
                    }
                    var boneName = reader.ReadString();
                    var boneData = skeletonData.FindBone(boneName);
                    var slot = new SlotData(slotIndex, slotName, boneData);

                    var color = reader.ReadString();
                    slot.R = ToColor(color, 0);
                    slot.G = ToColor(color, 1);
                    slot.B = ToColor(color, 2);
                    slot.A = ToColor(color, 3);

                    var dark = reader.ReadString();
                    slot.R2 = ToColor(dark, 0, 6);
                    slot.G2 = ToColor(dark, 1, 6);
                    slot.B2 = ToColor(dark, 2, 6);

                    slot.AttachmentName = reader.ReadString();
                    slot.BlendMode = (BlendMode)Enum.Parse(typeof(BlendMode), reader.ReadString(), true);

                    skeletonData.Slots.Add(slot);

                    // 读取下一个 数据
                    readIndex++;
                    if (readIndex < slotCount)
                        slotIndex = reader.ReadInt32(); // 读取索引
                    else
                        slotIndex = -1;
                }
                else
                    skeletonData.Slots.Add(null);
            }

            // ik
            var ikCount = reader.ReadInt32();
            for (int i = 0; i < ikCount; i++)
            {
                IkConstraintData ikData = new IkConstraintData(reader.ReadString());
                ikData.Order = reader.ReadInt32();
                ikData.SkinRequired = reader.ReadBoolean();
                var ikBonesCount = reader.ReadInt32();
                for (int j = 0; j < ikBonesCount; j++)
                {
                    var ikBone = skeletonData.FindBone(reader.ReadString());
                    ikData.Bones.Add(ikBone);
                }
                ikData.Target = skeletonData.FindBone(reader.ReadString());
                ikData.Mix = reader.ReadSingle();
                ikData.Softness = reader.ReadSingle() * scale;
                ikData.BendDirection = reader.ReadBoolean() ? 1 : -1;
                ikData.Compress = reader.ReadBoolean();
                ikData.Stretch = reader.ReadBoolean();
                ikData.Uniform = reader.ReadBoolean();

                skeletonData.IkConstraints.Add(ikData);
            }

            // skin
            var skinName = reader.ReadString();
            var skin = new Skin(skinName);
            var skinBoneCount = reader.ReadInt32();
            for (int i = 0; i < skinBoneCount; i++)
            {
                var skinBone = skeletonData.FindBone(reader.ReadString());
                skin.Bones.Add(skinBone);
            }
            skin.Bones.TrimExcess();

            var attachmentCount = reader.ReadInt32();
            for (int i = 0; i < attachmentCount; i++)
            {
                var slot = skeletonData.FindSlot(reader.ReadString());

                var slotAttachCount = reader.ReadInt32();
                for (int j = 0; j < slotAttachCount; j++)
                {
                    var slotAttachName = reader.ReadString(); // 不是真正的AttachmentName
                    var attachment = ReadAttachment(reader, skin, slot.Index, skeletonData, scale, loader);
                    skin.SetAttachment(slot.Index, slotAttachName, attachment);
                }
            }
            skeletonData.Skins.Add(skin);
            //if (skin.Name == "default") skeletonData.DefaultSkin = skin;
            skeletonData.DefaultSkin = skin; // 指定为默认皮肤


            // animation
            var animationCount = reader.ReadInt32();
            if (animationCount > 0)
            {
                for (int i = 0; i < animationCount; i++)
                {
                    ReadAnimation(reader, skeletonData, scale);
                }
            }


            skeletonData.Bones.TrimExcess();
            skeletonData.Slots.TrimExcess();
            skeletonData.Skins.TrimExcess();
            //skeletonData.Events.TrimExcess();
            skeletonData.Animations.TrimExcess();
            skeletonData.IkConstraints.TrimExcess();

            return skeletonData;
        }


        private static Attachment ReadAttachment(System.IO.BinaryReader reader, Skin skin, int slotIndex, SkeletonData skel, float scale, AttachmentLoader loader)
        {
            var attachmentName = reader.ReadString();
            var type = (AttachmentType)Enum.Parse(typeof(AttachmentType), reader.ReadString(), true);
            string path = null;
            Sequence sequence = null;

            switch (type)
            {
                case AttachmentType.Region:
                    path = reader.ReadString();
                    var x = reader.ReadSingle() * scale;
                    var y = reader.ReadSingle() * scale;
                    var scaleX = reader.ReadSingle();
                    var scaleY = reader.ReadSingle();
                    var rotation = reader.ReadSingle();
                    var width = reader.ReadSingle() * scale;
                    var height = reader.ReadSingle() * scale;
                    var color = reader.ReadString();

                    // sequence
                    //sequence = ParseSequence(data.sequence);
                    var region = loader.NewRegionAttachment(skin, attachmentName, path, sequence);
                    if (region == null) return null;
                    region.Path = path;
                    region.X = x;
                    region.Y = y;
                    region.ScaleX = scaleX;
                    region.ScaleY = scaleY;
                    region.Rotation = rotation;
                    region.Width = width;
                    region.Height = height;
                    region.R = ToColor(color, 0);
                    region.G = ToColor(color, 1);
                    region.B = ToColor(color, 2);
                    region.A = ToColor(color, 3);
                    if (region.Region != null) region.UpdateRegion();

                    return region;
                case AttachmentType.Mesh:
                case AttachmentType.Linkedmesh:
                    path = reader.ReadString();
                    color = reader.ReadString();
                    width = reader.ReadSingle() * scale;
                    height = reader.ReadSingle() * scale;
                    var parent = reader.ReadString();
                    var uvs = ReadFloatArray(reader, 1);
                    var vertices = ReadFloatArray(reader, 1);
                    var triangles = ReadIntArray(reader);
                    var hull = reader.ReadInt32();
                    var edges = ReadIntArray(reader);

                    // sequence
                    //sequence = ParseSequence(data.sequence);
                    var mesh = loader.NewMeshAttachment(skin, attachmentName, path, sequence);
                    if (mesh == null) return null;

                    mesh.Path = path;
                    mesh.R = ToColor(color, 0);
                    mesh.G = ToColor(color, 1);
                    mesh.B = ToColor(color, 2);
                    mesh.A = ToColor(color, 3);
                    mesh.Width = width;
                    mesh.Height = height;

                    // LinkedMesh
                    //if (!string.IsNullOrEmpty(parent))
                    //{
                    //    m_linkedMeshs.Add(new LinkedMesh(mesh, data.skin, slotIndex, data.parent, data.timelines));
                    //    return mesh;
                    //}

                    ReadVertices(vertices, mesh, uvs.Length, scale);
                    mesh.Triangles = triangles;
                    mesh.RegionUVs = uvs;
                    if (mesh.Region != null) mesh.UpdateRegion();
                    mesh.HullLength = hull;
                    mesh.Edges = edges;
                    return mesh;
            }

            return null;
        }

        private static void ReadAnimation(System.IO.BinaryReader reader, SkeletonData skel, float scale)
        {
            var timelines = new ExposedList<Timeline>();
            var animName = reader.ReadString();

            // slot
            var slotAnimCount = reader.ReadInt32();

            // bone
            var boneAnimCount = reader.ReadInt32();
            if (boneAnimCount > 0)
            {
                for (var i = 0; i < boneAnimCount; i++)
                {
                    var boneName = reader.ReadString();
                    var boneIndex = skel.FindBone(boneName).Index;

                    var timelineCount = reader.ReadInt32();
                    for (int j = 0; j < timelineCount; j++)
                    {
                        var valueCount = reader.ReadInt32();
                        var timelineName = reader.ReadString();
                        var frames = valueCount;
                        if (timelineName == "rotate")
                            timelines.Add(ReadTimeline(reader, valueCount, new RotateTimeline(frames, frames, boneIndex), 1));
                        else if (timelineName == "translate")
                        {
                            TranslateTimeline timeline = new TranslateTimeline(frames, frames << 1, boneIndex);
                            timelines.Add(ReadTimeline(reader, valueCount, timeline, scale));
                        }
                        else if (timelineName == "translatex")
                            timelines.Add(ReadTimeline(reader, valueCount, new TranslateXTimeline(frames, frames, boneIndex), scale));
                        else if (timelineName == "translatey")
                            timelines.Add(ReadTimeline(reader, valueCount, new TranslateYTimeline(frames, frames, boneIndex), scale));
                        else if (timelineName == "scale")
                        {
                            ScaleTimeline timeline = new ScaleTimeline(frames, frames << 1, boneIndex);
                            timelines.Add(ReadTimeline(reader, valueCount, timeline, 1));
                        }
                        else if (timelineName == "scalex")
                            timelines.Add(ReadTimeline(reader, valueCount, new ScaleXTimeline(frames, frames, boneIndex), 1));
                        else if (timelineName == "scaley")
                            timelines.Add(ReadTimeline(reader, valueCount, new ScaleYTimeline(frames, frames, boneIndex), 1));
                        else if (timelineName == "shear")
                        {
                            ShearTimeline timeline = new ShearTimeline(frames, frames << 1, boneIndex);
                            timelines.Add(ReadTimeline(reader, valueCount, timeline, 1));
                        }
                        else if (timelineName == "shearx")
                            timelines.Add(ReadTimeline(reader, valueCount, new ShearXTimeline(frames, frames, boneIndex), 1));
                        else if (timelineName == "sheary")
                            timelines.Add(ReadTimeline(reader, valueCount, new ShearYTimeline(frames, frames, boneIndex), 1));
                        else if (timelineName == "inherit")
                        {
                            InheritTimeline timeline = new InheritTimeline(frames, boneIndex);
                            for (int frame = 0; frame < valueCount; frame++)
                            {
                                float time = reader.ReadSingle();
                                var inherit = (Inherit)Enum.Parse(typeof(Inherit), reader.ReadString(), true);
                                timeline.SetFrame(frame, time, inherit);
                            }
                            timelines.Add(timeline);
                        }
                    }
                }
            }

            // ik

            // transform

            // path

            // physics

            // attachment
            var attachmentAnimCount = reader.ReadInt32(); // 按单个皮肤拆分数据.  所以此处最多一个
            if (attachmentAnimCount > 0)
            {
                var skinName = reader.ReadString();
                var skin = skel.FindSkin(skinName);

                var aaSlotCount = reader.ReadInt32(); // slot数量
                for (int i = 0; i < aaSlotCount; i++)
                {
                    var slotName = reader.ReadString();
                    var slotData = skel.FindSlot(slotName);
                    var aaAttachmentCount = reader.ReadInt32();
                    for (int j = 0; j < aaAttachmentCount; j++)
                    {
                        var attachmentName = reader.ReadString();
                        var attachment = skin.GetAttachment(slotData.Index, attachmentName);
                        var timelineCount = reader.ReadInt32();
                        for (int k = 0; k < timelineCount; k++)
                        {
                            var timelineName = reader.ReadString();
                            if (timelineName == "deform")
                            {
                                var vertexAttachment = (VertexAttachment)attachment;
                                bool weighted = vertexAttachment.Bones != null;
                                float[] vertices = vertexAttachment.Vertices;
                                int deformLength = weighted ? (vertices.Length / 3) << 1 : vertices.Length;

                                var frames = reader.ReadInt32();
                                var timeline = new DeformTimeline(frames, frames, slotData.Index, vertexAttachment);
                                var deformValTime = reader.ReadSingle();
                                var deformValOffset = reader.ReadInt32();
                                for (int frame = 0, bezier = 0; ; frame++)
                                {
                                    float[] deform;
                                    var deformVertexCount = reader.ReadInt32();
                                    if (deformVertexCount <= 0)
                                        deform = weighted ? new float[deformLength] : vertices;
                                    else
                                    {
                                        deform = new float[deformLength];
                                        float[] verticesValue = new float[deformVertexCount];
                                        for (int t = 0; t < deformVertexCount; t++) { verticesValue[t] = reader.ReadSingle(); }
                                        Array.Copy(verticesValue, 0, deform, deformValOffset, verticesValue.Length);
                                        if (scale != 1)
                                            for (int w = deformValOffset, n = w + verticesValue.Length; w < n; w++) deform[w] *= scale;

                                        if (!weighted)
                                            for (int w = 0; w < deformLength; w++) deform[w] += vertices[w];

                                    }

                                    ReadCurve(reader, out var curveState, out var curveStr, out var curveVals);
                                    timeline.SetFrame(frame, deformValTime, deform);
                                    if (frame == frames - 1)
                                    {
                                        timeline.Shrink(bezier);
                                        break;
                                    }

                                    var deformValTime2 = reader.ReadSingle();
                                    var deformValOffset2 = reader.ReadInt32();

                                    if (curveState > 0)
                                    {
                                        bezier = SetCurve(curveState == 1, curveStr, curveVals, timeline, bezier, frame, 0, deformValTime, deformValTime2, 0, 1, 1);
                                    }

                                    deformValTime = deformValTime2;
                                    deformValOffset = deformValOffset2;
                                }

                                timelines.Add(timeline);
                            }
                            else if (timelineName == "sequence")
                            {
                                // 无测试数据, 暂不支持
                            }
                        }
                    }
                }
            }

            // draw order

            // event

            timelines.TrimExcess();
            float duration = 0;
            Timeline[] items = timelines.Items;
            for (int i = 0, n = timelines.Count; i < n; i++)
                duration = Math.Max(duration, items[i].Duration);
            skel.Animations.Add(new Animation(animName, timelines, duration));
        }

        private static void ReadVertices(float[] vertices, VertexAttachment attachment, int verticesLength, float scale)
        {
            attachment.WorldVerticesLength = verticesLength;
            if (verticesLength == vertices.Length)
            {
                if (scale != 1)
                {
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        vertices[i] *= scale;
                    }
                }
                attachment.Vertices = vertices;
                return;
            }
            ExposedList<float> weights = new ExposedList<float>(verticesLength * 3 * 3);
            ExposedList<int> bones = new ExposedList<int>(verticesLength * 3);
            for (int i = 0, n = vertices.Length; i < n;)
            {
                int boneCount = (int)vertices[i++];
                bones.Add(boneCount);
                for (int nn = i + (boneCount << 2); i < nn; i += 4)
                {
                    bones.Add((int)vertices[i]);
                    weights.Add(vertices[i + 1] * scale);
                    weights.Add(vertices[i + 2] * scale);
                    weights.Add(vertices[i + 3]);
                }
            }
            attachment.Bones = bones.ToArray();
            attachment.Vertices = weights.ToArray();
        }

        private static Timeline ReadTimeline(System.IO.BinaryReader reader, int count, CurveTimeline1 timeline, float scale)
        {
            float time = reader.ReadSingle();
            float value = reader.ReadSingle() * scale;
            ReadCurve(reader, out var curveState, out var curveStr, out var curveVals);
            for (int frame = 0, bezier = 0; ; frame++)
            {
                timeline.SetFrame(frame, time, value);
                if (frame == count - 1)
                {
                    timeline.Shrink(bezier);
                    return timeline;
                }

                float time2 = reader.ReadSingle();
                float value2 = reader.ReadSingle() * scale;

                if (curveState > 0)
                {
                    bezier = SetCurve(curveState == 1, curveStr, curveVals, timeline, bezier, frame, 0, time, time2, value, value2, scale);
                }

                time = time2;
                value = value2;
                ReadCurve(reader, out curveState, out curveStr, out curveVals);
            }
        }

        private static Timeline ReadTimeline(System.IO.BinaryReader reader, int count, CurveTimeline2 timeline, float scale)
        {
            float time = reader.ReadSingle();
            float value1 = reader.ReadSingle() * scale;
            float value2 = reader.ReadSingle() * scale;
            ReadCurve(reader, out var curveState, out var curveStr, out var curveVals);
            for (int frame = 0, bezier = 0; ; frame++)
            {
                timeline.SetFrame(frame, time, value1, value2);
                if (frame == count - 1)
                {
                    timeline.Shrink(bezier);
                    return timeline;
                }

                float time2 = reader.ReadSingle();
                float nvalue1 = reader.ReadSingle() * scale;
                float nvalue2 = reader.ReadSingle() * scale;

                if (curveState > 0)
                {
                    bezier = SetCurve(curveState == 1, curveStr, curveVals, timeline, bezier, frame, 0, time, time2, value1, nvalue1, scale);
                    bezier = SetCurve(curveState == 1, curveStr, curveVals, timeline, bezier, frame, 1, time, time2, value2, nvalue2, scale);
                }

                time = time2;
                value1 = nvalue1;
                value2 = nvalue2;
                ReadCurve(reader, out curveState, out curveStr, out curveVals);
            }
        }


        private static void ReadCurve(System.IO.BinaryReader reader, out int curveState, out string curverStr, out float[] curveVals)
        {
            curverStr = null;
            curveVals = null;

            curveState = reader.ReadInt32();
            if (curveState == 1) curverStr = reader.ReadString();
            else if (curveState == 2) curveVals = ReadFloatArray(reader, 1);
        }

        private static int SetCurve(bool isCurveStr, string curveStr, float[] curveValues, CurveTimeline timeline, int bezier, int frame, int value, float time1, float time2,
            float value1, float value2, float scale)
        {
            if (isCurveStr)
            {
                if (curveStr == "stepped") timeline.SetStepped(frame);
                return bezier;
            }
            int i = value << 2;
            float cx1 = curveValues[i];
            float cy1 = curveValues[i + 1] * scale;
            float cx2 = curveValues[i + 2];
            float cy2 = curveValues[i + 3] * scale;
            SetBezier(timeline, frame, value, bezier, time1, value1, cx1, cy1, cx2, cy2, time2, value2);
            return bezier + 1;
        }

        private static void SetBezier(CurveTimeline timeline, int frame, int value, int bezier, float time1, float value1, float cx1, float cy1,
            float cx2, float cy2, float time2, float value2)
        {
            timeline.SetBezier(bezier, frame, value, time1, value1, cx1, cy1, cx2, cy2, time2, value2);
        }

        private static int[] ReadIntArray(System.IO.BinaryReader reader)
        {
            var count = reader.ReadInt32();
            var array = new int[count];
            for (int i = 0; i < count; i++) array[i] = reader.ReadInt32();

            return array;
        }

        private static float[] ReadFloatArray(System.IO.BinaryReader reader, float scale)
        {
            var count = reader.ReadInt32();
            var array = new float[count];
            for (int i = 0; i < count; i++) array[i] = reader.ReadSingle() * scale;

            return array;
        }

        private static float ToColor(string hexString, int colorIndex, int expectedLength = 8)
        {
            if (hexString.Length < expectedLength)
                throw new ArgumentException("Color hexadecimal length must be " + expectedLength + ", received: " + hexString, "hexString");
            return Convert.ToInt32(hexString.Substring(colorIndex * 2, 2), 16) / (float)255;
        }

        #endregion


        #region 材质名

        /// <summary>
        ///  读取 Attachment 对应的材质名
        /// </summary>
        /// <param name="reader"></param>
        public static Dictionary<int, Dictionary<string, string>> GetAttachmentMatName(System.IO.BinaryReader reader, Dictionary<int, Dictionary<string, string>> dict)
        {
            if (dict == null) dict = new Dictionary<int, Dictionary<string, string>>();
            var matCount = reader.ReadInt32();
            if (matCount > 0)
            {
                for (int i = 0;i < matCount; i++)
                {
                    var matName = reader.ReadString();
                    var attachCount = reader.ReadInt32();
                    for (int j = 0;j < attachCount; j++)
                    {
                        var slotIndex = reader.ReadInt32();
                        var attachName = reader.ReadString();

                        if (!dict.TryGetValue(slotIndex, out var nameDict))
                        {
                            nameDict = new Dictionary<string, string>();
                            dict[slotIndex] = nameDict;
                        }
                        nameDict[attachName] = matName;
                    }
                }
            }
            return dict;
        }
        #endregion

    }
}