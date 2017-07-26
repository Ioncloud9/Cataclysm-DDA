using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace VOX
{
    public struct Voxel
    {
        public byte x, y, z, i;
    }

    public struct Material
    {
        public Color color;
        public int type;
        public int weight;
        public float plastic;
        public float roughness;
        public float specular;
        public float ior;
        public float attenuation;
        public float power;
        public float glow;
        public float isTotalPower;
    }

    public class Model
    {
        public float SCALE = 1f;
        public Material[] materials = null;
        public byte[] voxels;
        public string path;
        public int version;
        public int frames = 1; // only one animation frame for now
        public int sizeX = 0, sizeY = 0, sizeZ = 0;

        public Model(string path)
        {
            this.path = path;
            try
            {
				byte[] data = System.IO.File.ReadAllBytes(path);
                using (MemoryStream ms = new MemoryStream(data, false)) // read-only
                {
                    BinaryReader br = new BinaryReader(ms);
                    string fileHeader = Encoding.ASCII.GetString(br.ReadBytes(4));
                    if (fileHeader != "VOX ")
                    {
                        Debug.LogError("[VOXLoader] Invalid header in VOX file");
                        return;
                    }
                    version = br.ReadInt32();

                    string chunkHeader = Encoding.ASCII.GetString(br.ReadBytes(4));
                    if (chunkHeader != "MAIN")
                    {
                        Debug.LogError("[VOXLoader] Invalid chunk, MAIN expected");
                        return;
                    }
                    int mainModelSize = br.ReadInt32();
                    int bytesLeft = br.ReadInt32();
                    br.ReadBytes(mainModelSize); // should be empty

                    int chunkSize;
                    int childrenSize;

                    while (bytesLeft > 0)
                    {
                        chunkHeader = Encoding.ASCII.GetString(br.ReadBytes(4));
                        switch (chunkHeader)
                        {
                            case "PACK":
                                chunkSize = br.ReadInt32();
                                childrenSize = br.ReadInt32();
                                frames = br.ReadInt32();
                                if (frames > 1)
                                {
                                    Debug.LogError("[VOXLoader] Animated vox files are not supported");
                                    return;
                                }
                                if (childrenSize > 0)
                                {
                                    br.ReadBytes(childrenSize);
                                    Debug.LogWarning("[VOXLoader] Nested chunk for PACK not supported");
                                }
                                bytesLeft -= chunkSize + childrenSize + 4 * 3; // 4 for HEADER, 4 for size, 4 for childrenSize
                                break;
                            case "SIZE":
                                chunkSize = br.ReadInt32();
                                childrenSize = br.ReadInt32();

                                sizeX = br.ReadInt32();
                                sizeZ = br.ReadInt32();
                                sizeY = br.ReadInt32();

                                if (childrenSize > 0)
                                {
                                    br.ReadBytes(childrenSize);
                                    Debug.LogWarning("[VOXLoader] Nested chunk for SIZE not supported");
                                }
                                bytesLeft -= chunkSize + childrenSize + 4 * 3;
                                voxels = new byte[sizeX * sizeY * sizeZ];
                                break;
                            case "XYZI":
                                chunkSize = br.ReadInt32();
                                childrenSize = br.ReadInt32();
                                int n = br.ReadInt32();

                                for (int i = 0; i < n; i++)
                                {
                                    byte x = br.ReadByte();
                                    byte z = br.ReadByte();
                                    byte y = br.ReadByte();
                                    byte m = br.ReadByte();
                                    voxels[sizeX * sizeY * z + sizeX * y + x] = m;
                                }
                                if (childrenSize > 0)
                                {
                                    br.ReadBytes(childrenSize);
                                    Debug.LogWarning("[VOXLoader] Nested chunk for XYZI not supported");
                                }
                                bytesLeft -= chunkSize + childrenSize + 4 * 3;
                                break;
                            case "RGBA":
                                materials = new Material[256];
                                chunkSize = br.ReadInt32();
                                childrenSize = br.ReadInt32();
                                materials[0].color = Color.black;
                                
                                for (int i = 1; i < 256; i++)
                                {
                                    materials[i].color.r = br.ReadByte() / 255.0f;
                                    materials[i].color.g = br.ReadByte() / 255.0f;
                                    materials[i].color.b = br.ReadByte() / 255.0f;
                                    materials[i].color.a = br.ReadByte() / 255.0f;
                                }
                                if (childrenSize > 0)
                                {
                                    br.ReadBytes(childrenSize);
                                    Debug.LogWarning("[VOXLoader] Nested chunk for RGBA not supported");
                                }
                                bytesLeft -= chunkSize + childrenSize + 4 * 3;
                                break;
                            case "MATT":
                                if (materials == null)
                                    LoadDefaultPalette();
                                chunkSize = br.ReadInt32();
                                childrenSize = br.ReadInt32();

                                int id = br.ReadInt32();

                                materials[id].type = br.ReadInt32();
                                materials[id].weight = br.ReadInt32();
                                int propBits = br.ReadInt32();

                                if ((propBits & 1) != 0) materials[id].plastic = br.ReadSingle();
                                if ((propBits & 2) != 0) materials[id].roughness = br.ReadSingle();
                                if ((propBits & 4) != 0) materials[id].specular = br.ReadSingle();
                                if ((propBits & 8) != 0) materials[id].ior = br.ReadSingle();
                                if ((propBits & 16) != 0) materials[id].attenuation = br.ReadSingle();
                                if ((propBits & 32) != 0) materials[id].power = br.ReadSingle();
                                if ((propBits & 64) != 0) materials[id].glow = br.ReadSingle();
                                if ((propBits & 128) != 0) materials[id].isTotalPower = br.ReadSingle();

                                if (childrenSize > 0)
                                {
                                    br.ReadBytes(childrenSize);
                                    Debug.LogWarning("[VOXLoader] Nested chunk for materials [id]T not supported");
                                }

                                bytesLeft -= chunkSize + childrenSize + 4 * 3;
                                break;
                        }
                    }

                    if (materials == null)
                        LoadDefaultPalette();
                }
            }
            catch (IOException)
            {

            }
        }

        public Nullable<Material> VoxelAt(int x, int y, int z) {
            if (x < 0 || y < 0 || z < 0 || x >= sizeX || y >= sizeY || z >= sizeZ) return null;
            byte m = voxels[sizeX * sizeY * z + sizeX * y + x];
            if (m == 0) return null;
            return materials[m];
        }

        public Nullable<Material> VoxelAt(Vector3Int v) {
            return VoxelAt(v.x, v.y, v.z);
        }

        public byte MaterialIDAt(int x, int y, int z) {
            return voxels[sizeX * sizeY * z + sizeX * y + x];
        }

        public void LoadDefaultPalette()
        {
            #region defaultPalette
            Color[] colors = {
                new Color(1.000000f, 1.000000f, 1.000000f),
                new Color(1.000000f, 1.000000f, 0.800000f),
                new Color(1.000000f, 1.000000f, 0.600000f),
                new Color(1.000000f, 1.000000f, 0.400000f),
                new Color(1.000000f, 1.000000f, 0.200000f),
                new Color(1.000000f, 1.000000f, 0.000000f),
                new Color(1.000000f, 0.800000f, 1.000000f),
                new Color(1.000000f, 0.800000f, 0.800000f),
                new Color(1.000000f, 0.800000f, 0.600000f),
                new Color(1.000000f, 0.800000f, 0.400000f),
                new Color(1.000000f, 0.800000f, 0.200000f),
                new Color(1.000000f, 0.800000f, 0.000000f),
                new Color(1.000000f, 0.600000f, 1.000000f),
                new Color(1.000000f, 0.600000f, 0.800000f),
                new Color(1.000000f, 0.600000f, 0.600000f),
                new Color(1.000000f, 0.600000f, 0.400000f),
                new Color(1.000000f, 0.600000f, 0.200000f),
                new Color(1.000000f, 0.600000f, 0.000000f),
                new Color(1.000000f, 0.400000f, 1.000000f),
                new Color(1.000000f, 0.400000f, 0.800000f),
                new Color(1.000000f, 0.400000f, 0.600000f),
                new Color(1.000000f, 0.400000f, 0.400000f),
                new Color(1.000000f, 0.400000f, 0.200000f),
                new Color(1.000000f, 0.400000f, 0.000000f),
                new Color(1.000000f, 0.200000f, 1.000000f),
                new Color(1.000000f, 0.200000f, 0.800000f),
                new Color(1.000000f, 0.200000f, 0.600000f),
                new Color(1.000000f, 0.200000f, 0.400000f),
                new Color(1.000000f, 0.200000f, 0.200000f),
                new Color(1.000000f, 0.200000f, 0.000000f),
                new Color(1.000000f, 0.000000f, 1.000000f),
                new Color(1.000000f, 0.000000f, 0.800000f),
                new Color(1.000000f, 0.000000f, 0.600000f),
                new Color(1.000000f, 0.000000f, 0.400000f),
                new Color(1.000000f, 0.000000f, 0.200000f),
                new Color(1.000000f, 0.000000f, 0.000000f),
                new Color(0.800000f, 1.000000f, 1.000000f),
                new Color(0.800000f, 1.000000f, 0.800000f),
                new Color(0.800000f, 1.000000f, 0.600000f),
                new Color(0.800000f, 1.000000f, 0.400000f),
                new Color(0.800000f, 1.000000f, 0.200000f),
                new Color(0.800000f, 1.000000f, 0.000000f),
                new Color(0.800000f, 0.800000f, 1.000000f),
                new Color(0.800000f, 0.800000f, 0.800000f),
                new Color(0.800000f, 0.800000f, 0.600000f),
                new Color(0.800000f, 0.800000f, 0.400000f),
                new Color(0.800000f, 0.800000f, 0.200000f),
                new Color(0.800000f, 0.800000f, 0.000000f),
                new Color(0.800000f, 0.600000f, 1.000000f),
                new Color(0.800000f, 0.600000f, 0.800000f),
                new Color(0.800000f, 0.600000f, 0.600000f),
                new Color(0.800000f, 0.600000f, 0.400000f),
                new Color(0.800000f, 0.600000f, 0.200000f),
                new Color(0.800000f, 0.600000f, 0.000000f),
                new Color(0.800000f, 0.400000f, 1.000000f),
                new Color(0.800000f, 0.400000f, 0.800000f),
                new Color(0.800000f, 0.400000f, 0.600000f),
                new Color(0.800000f, 0.400000f, 0.400000f),
                new Color(0.800000f, 0.400000f, 0.200000f),
                new Color(0.800000f, 0.400000f, 0.000000f),
                new Color(0.800000f, 0.200000f, 1.000000f),
                new Color(0.800000f, 0.200000f, 0.800000f),
                new Color(0.800000f, 0.200000f, 0.600000f),
                new Color(0.800000f, 0.200000f, 0.400000f),
                new Color(0.800000f, 0.200000f, 0.200000f),
                new Color(0.800000f, 0.200000f, 0.000000f),
                new Color(0.800000f, 0.000000f, 1.000000f),
                new Color(0.800000f, 0.000000f, 0.800000f),
                new Color(0.800000f, 0.000000f, 0.600000f),
                new Color(0.800000f, 0.000000f, 0.400000f),
                new Color(0.800000f, 0.000000f, 0.200000f),
                new Color(0.800000f, 0.000000f, 0.000000f),
                new Color(0.600000f, 1.000000f, 1.000000f),
                new Color(0.600000f, 1.000000f, 0.800000f),
                new Color(0.600000f, 1.000000f, 0.600000f),
                new Color(0.600000f, 1.000000f, 0.400000f),
                new Color(0.600000f, 1.000000f, 0.200000f),
                new Color(0.600000f, 1.000000f, 0.000000f),
                new Color(0.600000f, 0.800000f, 1.000000f),
                new Color(0.600000f, 0.800000f, 0.800000f),
                new Color(0.600000f, 0.800000f, 0.600000f),
                new Color(0.600000f, 0.800000f, 0.400000f),
                new Color(0.600000f, 0.800000f, 0.200000f),
                new Color(0.600000f, 0.800000f, 0.000000f),
                new Color(0.600000f, 0.600000f, 1.000000f),
                new Color(0.600000f, 0.600000f, 0.800000f),
                new Color(0.600000f, 0.600000f, 0.600000f),
                new Color(0.600000f, 0.600000f, 0.400000f),
                new Color(0.600000f, 0.600000f, 0.200000f),
                new Color(0.600000f, 0.600000f, 0.000000f),
                new Color(0.600000f, 0.400000f, 1.000000f),
                new Color(0.600000f, 0.400000f, 0.800000f),
                new Color(0.600000f, 0.400000f, 0.600000f),
                new Color(0.600000f, 0.400000f, 0.400000f),
                new Color(0.600000f, 0.400000f, 0.200000f),
                new Color(0.600000f, 0.400000f, 0.000000f),
                new Color(0.600000f, 0.200000f, 1.000000f),
                new Color(0.600000f, 0.200000f, 0.800000f),
                new Color(0.600000f, 0.200000f, 0.600000f),
                new Color(0.600000f, 0.200000f, 0.400000f),
                new Color(0.600000f, 0.200000f, 0.200000f),
                new Color(0.600000f, 0.200000f, 0.000000f),
                new Color(0.600000f, 0.000000f, 1.000000f),
                new Color(0.600000f, 0.000000f, 0.800000f),
                new Color(0.600000f, 0.000000f, 0.600000f),
                new Color(0.600000f, 0.000000f, 0.400000f),
                new Color(0.600000f, 0.000000f, 0.200000f),
                new Color(0.600000f, 0.000000f, 0.000000f),
                new Color(0.400000f, 1.000000f, 1.000000f),
                new Color(0.400000f, 1.000000f, 0.800000f),
                new Color(0.400000f, 1.000000f, 0.600000f),
                new Color(0.400000f, 1.000000f, 0.400000f),
                new Color(0.400000f, 1.000000f, 0.200000f),
                new Color(0.400000f, 1.000000f, 0.000000f),
                new Color(0.400000f, 0.800000f, 1.000000f),
                new Color(0.400000f, 0.800000f, 0.800000f),
                new Color(0.400000f, 0.800000f, 0.600000f),
                new Color(0.400000f, 0.800000f, 0.400000f),
                new Color(0.400000f, 0.800000f, 0.200000f),
                new Color(0.400000f, 0.800000f, 0.000000f),
                new Color(0.400000f, 0.600000f, 1.000000f),
                new Color(0.400000f, 0.600000f, 0.800000f),
                new Color(0.400000f, 0.600000f, 0.600000f),
                new Color(0.400000f, 0.600000f, 0.400000f),
                new Color(0.400000f, 0.600000f, 0.200000f),
                new Color(0.400000f, 0.600000f, 0.000000f),
                new Color(0.400000f, 0.400000f, 1.000000f),
                new Color(0.400000f, 0.400000f, 0.800000f),
                new Color(0.400000f, 0.400000f, 0.600000f),
                new Color(0.400000f, 0.400000f, 0.400000f),
                new Color(0.400000f, 0.400000f, 0.200000f),
                new Color(0.400000f, 0.400000f, 0.000000f),
                new Color(0.400000f, 0.200000f, 1.000000f),
                new Color(0.400000f, 0.200000f, 0.800000f),
                new Color(0.400000f, 0.200000f, 0.600000f),
                new Color(0.400000f, 0.200000f, 0.400000f),
                new Color(0.400000f, 0.200000f, 0.200000f),
                new Color(0.400000f, 0.200000f, 0.000000f),
                new Color(0.400000f, 0.000000f, 1.000000f),
                new Color(0.400000f, 0.000000f, 0.800000f),
                new Color(0.400000f, 0.000000f, 0.600000f),
                new Color(0.400000f, 0.000000f, 0.400000f),
                new Color(0.400000f, 0.000000f, 0.200000f),
                new Color(0.400000f, 0.000000f, 0.000000f),
                new Color(0.200000f, 1.000000f, 1.000000f),
                new Color(0.200000f, 1.000000f, 0.800000f),
                new Color(0.200000f, 1.000000f, 0.600000f),
                new Color(0.200000f, 1.000000f, 0.400000f),
                new Color(0.200000f, 1.000000f, 0.200000f),
                new Color(0.200000f, 1.000000f, 0.000000f),
                new Color(0.200000f, 0.800000f, 1.000000f),
                new Color(0.200000f, 0.800000f, 0.800000f),
                new Color(0.200000f, 0.800000f, 0.600000f),
                new Color(0.200000f, 0.800000f, 0.400000f),
                new Color(0.200000f, 0.800000f, 0.200000f),
                new Color(0.200000f, 0.800000f, 0.000000f),
                new Color(0.200000f, 0.600000f, 1.000000f),
                new Color(0.200000f, 0.600000f, 0.800000f),
                new Color(0.200000f, 0.600000f, 0.600000f),
                new Color(0.200000f, 0.600000f, 0.400000f),
                new Color(0.200000f, 0.600000f, 0.200000f),
                new Color(0.200000f, 0.600000f, 0.000000f),
                new Color(0.200000f, 0.400000f, 1.000000f),
                new Color(0.200000f, 0.400000f, 0.800000f),
                new Color(0.200000f, 0.400000f, 0.600000f),
                new Color(0.200000f, 0.400000f, 0.400000f),
                new Color(0.200000f, 0.400000f, 0.200000f),
                new Color(0.200000f, 0.400000f, 0.000000f),
                new Color(0.200000f, 0.200000f, 1.000000f),
                new Color(0.200000f, 0.200000f, 0.800000f),
                new Color(0.200000f, 0.200000f, 0.600000f),
                new Color(0.200000f, 0.200000f, 0.400000f),
                new Color(0.200000f, 0.200000f, 0.200000f),
                new Color(0.200000f, 0.200000f, 0.000000f),
                new Color(0.200000f, 0.000000f, 1.000000f),
                new Color(0.200000f, 0.000000f, 0.800000f),
                new Color(0.200000f, 0.000000f, 0.600000f),
                new Color(0.200000f, 0.000000f, 0.400000f),
                new Color(0.200000f, 0.000000f, 0.200000f),
                new Color(0.200000f, 0.000000f, 0.000000f),
                new Color(0.000000f, 1.000000f, 1.000000f),
                new Color(0.000000f, 1.000000f, 0.800000f),
                new Color(0.000000f, 1.000000f, 0.600000f),
                new Color(0.000000f, 1.000000f, 0.400000f),
                new Color(0.000000f, 1.000000f, 0.200000f),
                new Color(0.000000f, 1.000000f, 0.000000f),
                new Color(0.000000f, 0.800000f, 1.000000f),
                new Color(0.000000f, 0.800000f, 0.800000f),
                new Color(0.000000f, 0.800000f, 0.600000f),
                new Color(0.000000f, 0.800000f, 0.400000f),
                new Color(0.000000f, 0.800000f, 0.200000f),
                new Color(0.000000f, 0.800000f, 0.000000f),
                new Color(0.000000f, 0.600000f, 1.000000f),
                new Color(0.000000f, 0.600000f, 0.800000f),
                new Color(0.000000f, 0.600000f, 0.600000f),
                new Color(0.000000f, 0.600000f, 0.400000f),
                new Color(0.000000f, 0.600000f, 0.200000f),
                new Color(0.000000f, 0.600000f, 0.000000f),
                new Color(0.000000f, 0.400000f, 1.000000f),
                new Color(0.000000f, 0.400000f, 0.800000f),
                new Color(0.000000f, 0.400000f, 0.600000f),
                new Color(0.000000f, 0.400000f, 0.400000f),
                new Color(0.000000f, 0.400000f, 0.200000f),
                new Color(0.000000f, 0.400000f, 0.000000f),
                new Color(0.000000f, 0.200000f, 1.000000f),
                new Color(0.000000f, 0.200000f, 0.800000f),
                new Color(0.000000f, 0.200000f, 0.600000f),
                new Color(0.000000f, 0.200000f, 0.400000f),
                new Color(0.000000f, 0.200000f, 0.200000f),
                new Color(0.000000f, 0.200000f, 0.000000f),
                new Color(0.000000f, 0.000000f, 1.000000f),
                new Color(0.000000f, 0.000000f, 0.800000f),
                new Color(0.000000f, 0.000000f, 0.600000f),
                new Color(0.000000f, 0.000000f, 0.400000f),
                new Color(0.000000f, 0.000000f, 0.200000f),
                new Color(0.933333f, 0.000000f, 0.000000f),
                new Color(0.866667f, 0.000000f, 0.000000f),
                new Color(0.733333f, 0.000000f, 0.000000f),
                new Color(0.666667f, 0.000000f, 0.000000f),
                new Color(0.533333f, 0.000000f, 0.000000f),
                new Color(0.466667f, 0.000000f, 0.000000f),
                new Color(0.333333f, 0.000000f, 0.000000f),
                new Color(0.266667f, 0.000000f, 0.000000f),
                new Color(0.133333f, 0.000000f, 0.000000f),
                new Color(0.066667f, 0.000000f, 0.000000f),
                new Color(0.000000f, 0.933333f, 0.000000f),
                new Color(0.000000f, 0.866667f, 0.000000f),
                new Color(0.000000f, 0.733333f, 0.000000f),
                new Color(0.000000f, 0.666667f, 0.000000f),
                new Color(0.000000f, 0.533333f, 0.000000f),
                new Color(0.000000f, 0.466667f, 0.000000f),
                new Color(0.000000f, 0.333333f, 0.000000f),
                new Color(0.000000f, 0.266667f, 0.000000f),
                new Color(0.000000f, 0.133333f, 0.000000f),
                new Color(0.000000f, 0.066667f, 0.000000f),
                new Color(0.000000f, 0.000000f, 0.933333f),
                new Color(0.000000f, 0.000000f, 0.866667f),
                new Color(0.000000f, 0.000000f, 0.733333f),
                new Color(0.000000f, 0.000000f, 0.666667f),
                new Color(0.000000f, 0.000000f, 0.533333f),
                new Color(0.000000f, 0.000000f, 0.466667f),
                new Color(0.000000f, 0.000000f, 0.333333f),
                new Color(0.000000f, 0.000000f, 0.266667f),
                new Color(0.000000f, 0.000000f, 0.133333f),
                new Color(0.000000f, 0.000000f, 0.066667f),
                new Color(0.933333f, 0.933333f, 0.933333f),
                new Color(0.866667f, 0.866667f, 0.866667f),
                new Color(0.733333f, 0.733333f, 0.733333f),
                new Color(0.666667f, 0.666667f, 0.666667f),
                new Color(0.533333f, 0.533333f, 0.533333f),
                new Color(0.466667f, 0.466667f, 0.466667f),
                new Color(0.333333f, 0.333333f, 0.333333f),
                new Color(0.266667f, 0.266667f, 0.266667f),
                new Color(0.133333f, 0.133333f, 0.133333f),
                new Color(0.066667f, 0.066667f, 0.066667f),
                new Color(0.000000f, 0.000000f, 0.000000f)
            };
            #endregion
            materials = new Material[256];
            for (int i = 0; i < 256; i++)
            {
                materials[i+1].color = colors[i];
            }
        }
    }
}