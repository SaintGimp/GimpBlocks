using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibNoise;
using LibNoise.Filter;
using LibNoise.Modifier;
using LibNoise.Primitive;
using LibNoise.Tranformer;

namespace GimpBlocks
{
    public class TerrainGenerator1
    {
        // Setting the horizontal sampe rate to 4 smoothes things a bit, 16 smoothes a lot,
        // looking much more gentle and not very sensitive to detail in the noise.
        readonly int horizontalSampleRate = 4;
        readonly int verticalSampleRate = 4;

        public void GenerateTerrain(Chunk chunk)
        {
            var densityMap = GenerateDensityMap(chunk);
            InitializeChunk(chunk, densityMap);
        }

        double[,,] GenerateDensityMap(Chunk chunk)
        {
            var primitive = new SimplexPerlin
            {
                Quality = NoiseQuality.Best,
                Seed = 1
            };

            var terrainFilter = new MultiFractal
            {
                Primitive3D = primitive,
                Frequency = 1,
                Gain = 3f,
                Lacunarity = 2,
                OctaveCount = 4,
                Offset = 1,
                SpectralExponent = 0.25f
            };

            // MultiFractal output seems to vary from 0 to 3ish
            var outputScaler = new ScaleBias
            {
                SourceModule = terrainFilter,
                Scale = 2 / 3f,
                Bias = -1f
            };

            // The terrace seems to be useful for smoothing the lower parts while still allowing
            // for dramatic mountains

            var terrace = new Terrace()
            {
                SourceModule = outputScaler,
            };
            terrace.AddControlPoint(-1f);
            //terrace.AddControlPoint(-0.5f);
            //terrace.AddControlPoint(0f);
            //terrace.AddControlPoint(0.5f);
            terrace.AddControlPoint(2f);
            //terrace.AddControlPoint(0.7f, 0.8f);

            var inputScaler = new ScalePoint
            {
                SourceModule = terrace,
                XScale = 0.01f,
                YScale = 0.01f,
                ZScale = 0.01f
            };

            var densityMap = new double[Chunk.XDimension + 1,Chunk.YDimension + 1,Chunk.ZDimension + 1];
            for (int x = 0; x <= Chunk.XDimension; x += horizontalSampleRate)
            {
                for (int y = 0; y <= Chunk.YDimension; y += verticalSampleRate)
                {
                    for (int z = 0; z <= Chunk.ZDimension; z += horizontalSampleRate)
                    {
                        float worldX = chunk.OriginInWorld.X + x;
                        float worldY = chunk.OriginInWorld.Y + y;
                        float worldZ = chunk.OriginInWorld.Z + z;
                        var noise = inputScaler.GetValue(worldX, worldY, worldZ);

                        // This applies a gradient to the noise based on height.
                        // The smaller the gradient value, the longer it takes to drive
                        // the entire fractal below zero and the more overhang/hole stuff
                        // we get.
                        var density = noise - (y / 10f) + 3;
                        densityMap[x, y, z] = density;
                    }
                }
            }

            TriLerp(densityMap);

            return densityMap;
        }

        void TriLerp(double[,,] densityMap)
        {
            for (int x = 0; x < Chunk.XDimension; x++)
            {
                for (int y = 0; y < Chunk.YDimension; y++)
                {
                    for (int z = 0; z < Chunk.ZDimension; z++)
                    {
                        if (x % horizontalSampleRate != 0 || y % verticalSampleRate != 0 || z % horizontalSampleRate != 0)
                        {
                            int offsetX = (x / horizontalSampleRate) * horizontalSampleRate;
                            int offsetY = (y / verticalSampleRate) * verticalSampleRate;
                            int offsetZ = (z / horizontalSampleRate) * horizontalSampleRate;
                            densityMap[x, y, z] = GimpMath.TriLerp(x, y, z,
                                densityMap[offsetX, offsetY, offsetZ],
                                densityMap[offsetX, verticalSampleRate + offsetY, offsetZ],
                                densityMap[offsetX, offsetY, offsetZ + horizontalSampleRate],
                                densityMap[offsetX, offsetY + verticalSampleRate, offsetZ + horizontalSampleRate],
                                densityMap[horizontalSampleRate + offsetX, offsetY, offsetZ],
                                densityMap[horizontalSampleRate + offsetX, offsetY + verticalSampleRate, offsetZ],
                                densityMap[horizontalSampleRate + offsetX, offsetY, offsetZ + horizontalSampleRate],
                                densityMap[horizontalSampleRate + offsetX, offsetY + verticalSampleRate, offsetZ + horizontalSampleRate],
                                offsetX, horizontalSampleRate + offsetX, offsetY, verticalSampleRate + offsetY, offsetZ,
                                offsetZ + horizontalSampleRate);
                        }
                    }
                }
            }
        }

        void InitializeChunk(Chunk chunk, double[,,] densityMap)
        {
            for (int x = 0; x < Chunk.XDimension; x++)
            {
                for (int y = 0; y < Chunk.YDimension; y++)
                {
                    for (int z = 0; z < Chunk.ZDimension; z++)
                    {
                        var prototype = densityMap[x, y, z] > 0 ? BlockPrototype.StoneBlock : BlockPrototype.AirBlock;
                        chunk.SetBlockPrototype(new RelativeBlockPosition(x, y, z), prototype);
                    }
                }
            }
        }

        //private void GenerateCaves1()
        //{
        //    var primitive = new SimplexPerlin
        //    {
        //        Quality = NoiseQuality.Best,
        //        Seed = 1
        //    };

        //    var filter = new HeterogeneousMultiFractal()
        //    {
        //        Primitive3D = primitive,
        //        Frequency = 1,
        //        Gain = 2,
        //        Lacunarity = 2,
        //        OctaveCount = 1,
        //        Offset = 1,
        //        SpectralExponent = 0.9f
        //    };

        //    _blockArray.Initialize((x, y, z) =>
        //    {
        //        if (x > 0 && x < _blockArray.XDimension - 1 && y > 0 && y < _blockArray.YDimension - 1 && z > 0 && z < _blockArray.ZDimension - 1)
        //        {
        //            float divisor = 20;
        //            float filterX = (XDimension * Position.X + x) / divisor;
        //            float filterY = y / divisor;
        //            float filterZ = (ZDimension * Position.Z + z) / divisor;
        //            return filter.GetValue(filterX, filterY, filterZ) < 1.4 ? _prototypeMap[1] : _prototypeMap[0];
        //        }
        //        else
        //        {
        //            return _prototypeMap[0];
        //        }
        //    });
        //}

        //private void GenerateCaves2()
        //{
        //    var primitive = new SimplexPerlin
        //    {
        //        Quality = NoiseQuality.Best,
        //        Seed = 1
        //    };

        //    var filter = new HybridMultiFractal()
        //    {
        //        Primitive3D = primitive,
        //        Frequency = 1,
        //        Gain = 2,
        //        Lacunarity = 2,
        //        OctaveCount = 1,
        //        Offset = 1,
        //        SpectralExponent = 0.9f
        //    };

        //    _blockArray.Initialize((x, y, z) =>
        //    {
        //        if (x > 0 && x < _blockArray.XDimension - 1 && y > 0 && y < _blockArray.YDimension - 1 && z > 0 && z < _blockArray.ZDimension - 1)
        //        {
        //            float divisor = 20;
        //            float filterX = (XDimension * Position.X + x) / divisor;
        //            float filterY = y / divisor;
        //            float filterZ = (ZDimension * Position.Z + z) / divisor;
        //            return filter.GetValue(filterX, filterY, filterZ) < 1.5 ? _prototypeMap[1] : _prototypeMap[0];
        //        }
        //        else
        //        {
        //            return _prototypeMap[0];
        //        }
        //    });
        //}

        void GenerateSlab(Chunk chunk)
        {
            for (int x = 0; x < Chunk.XDimension; x++)
            {
                for (int z = 0; z < Chunk.ZDimension; z++)
                {
                    chunk.SetBlockPrototype(new RelativeBlockPosition(x, 0, z), BlockPrototype.StoneBlock);
                }
            }
        }
    }
}
