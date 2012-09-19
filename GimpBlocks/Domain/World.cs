using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using LibNoise;
using LibNoise.Filter;
using LibNoise.Modifier;
using LibNoise.Primitive;
using LibNoise.Tranformer;
using Microsoft.Xna.Framework;

namespace GimpBlocks
{
    public class World
        : IListener<PlaceBlock>,
        IListener<DestroyBlock>
    {
        readonly IWorldRenderer _renderer;
        readonly BlockArray _blockArray;
        readonly LightArray _lightArray;
        readonly BlockPrototypeMap _prototypeMap;
        readonly BlockPicker _blockPicker;

        public World(IWorldRenderer renderer, BlockArray blockArray, LightArray lightArray, BlockPrototypeMap prototypeMap, BlockPicker blockPicker)
        {
            _renderer = renderer;
            _blockArray = blockArray;
            _lightArray = lightArray;
            _prototypeMap = prototypeMap;
            _blockPicker = blockPicker;
        }

        public void Generate()
        {
            //GenerateRandom();
            //GenerateSlab();
            //GenerateCaves2();
            GenerateTerrain1();

            Rebuild();
        }

        private void GenerateCaves1()
        {
            var primitive = new SimplexPerlin
            {
                Quality = NoiseQuality.Best,
                Seed = 1
            };

            var filter = new HeterogeneousMultiFractal()
            {
                Primitive3D = primitive,
                Frequency = 1,
                Gain = 2,
                Lacunarity = 2,
                OctaveCount = 1,
                Offset = 1,
                SpectralExponent = 0.9f
            };

            _blockArray.Initialize((x, y, z) =>
            {
                if (x > 0 && x < _blockArray.XDimension - 1 && y > 0 && y < _blockArray.YDimension - 1 && z > 0 && z < _blockArray.ZDimension - 1)
                {
                    float divisor = 20;
                    return filter.GetValue(x / divisor, y / divisor, z / divisor) < 1.4 ? _prototypeMap[1] : _prototypeMap[0];
                }
                else
                {
                    return _prototypeMap[0];
                }
            });
        }

        private void GenerateCaves2()
        {
            var primitive = new SimplexPerlin
            {
                Quality = NoiseQuality.Best,
                Seed = 1
            };

            var filter = new HybridMultiFractal()
            {
                Primitive3D = primitive,
                Frequency = 1,
                Gain = 2,
                Lacunarity = 2,
                OctaveCount = 1,
                Offset = 1,
                SpectralExponent = 0.9f
            };

            _blockArray.Initialize((x, y, z) =>
            {
                if (x > 0 && x < _blockArray.XDimension - 1 && y > 0 && y < _blockArray.YDimension - 1 && z > 0 && z < _blockArray.ZDimension - 1)
                {
                    float divisor = 20;
                    return filter.GetValue(x / divisor, y / divisor, z / divisor) < 1.5 ? _prototypeMap[1] : _prototypeMap[0];
                }
                else
                {
                    return _prototypeMap[0];
                }
            });
        }

        private void GenerateTerrain1()
        {
            var primitive = new SimplexPerlin
            {
                Quality = NoiseQuality.Best,
                Seed = 1
            };

            var terrainFilter = new RidgedMultiFractal
            {
                Primitive3D = primitive,
                Frequency = 1,
                Gain = 3f,
                Lacunarity = 2,
                OctaveCount = 4,
                Offset = 1,
                SpectralExponent = 0.7f
            };

            // MultiFractal output seems to vary from 0 to 3ish
            var outputScaler = new ScaleBias
            {
                SourceModule = terrainFilter,
                Scale = 1f,
                Bias = 0f
            };

            // The terrace seems to be useful for smoothing the lower parts while still allowing
            // for dramatic mountains

            var terrace = new Terrace()
            {
                SourceModule = outputScaler,
            };
            terrace.AddControlPoint(-0.5f);
            //terrace.AddControlPoint(-0.5f);
            terrace.AddControlPoint(0f);
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

            // Setting the horizontal sampe rate to 4 smoothes things a bit, 16 smoothes a lot,
            // looking much more gentle and not very sensitive to detail in the noise.

            var horizontalSampleRate = 4;
            var verticalSampleRate = 4;

            var maxNoise = float.MinValue;
            var minNoise = float.MaxValue;
            var densityMap = new double[_blockArray.XDimension + 1,_blockArray.YDimension + 1,_blockArray.ZDimension + 1];
            for (int x = 0; x < _blockArray.XDimension; x += horizontalSampleRate)
            {
                for (int y = 0; y < _blockArray.YDimension; y += verticalSampleRate)
                {
                    for (int z = 0; z < _blockArray.ZDimension; z += horizontalSampleRate)
                    {
                        var noise = inputScaler.GetValue(x, y, z);

                        if (noise > maxNoise)
                        {
                            maxNoise = noise;
                        }
                        if (noise < minNoise)
                        {
                            minNoise = noise;
                        }

                        // This applies a gradient to the noise based on height.
                        // The smaller the gradient value, the longer it takes to drive
                        // the entire fractal below zero and the more overhang/hole stuff
                        // we get.
                        var density = noise - (y / 10f) + 3;
                        //var density = noise - (y / 20f) + 3;
                        densityMap[x, y, z] = density;
                    }
                }
            }

            Debug.WriteLine("Max noise: {0}, min noise: {1}", maxNoise, minNoise);

            _blockArray.Initialize((x, y, z) =>
            {
                if (x > 0 && x < _blockArray.XDimension - 1 && y > 0 && y < _blockArray.YDimension - 1 && z > 0 && z < _blockArray.ZDimension - 1)
                {
                    if (!(x % horizontalSampleRate == 0 && y % verticalSampleRate == 0 && z % horizontalSampleRate == 0))
                    {
                        int offsetX = (x / horizontalSampleRate) * horizontalSampleRate;
                        int offsetY = (y / verticalSampleRate) * verticalSampleRate;
                        int offsetZ = (z / horizontalSampleRate) * horizontalSampleRate;
                        densityMap[x,y,z] = TriLerp(x, y, z,
                            densityMap[offsetX,offsetY,offsetZ],
                            densityMap[offsetX,verticalSampleRate + offsetY,offsetZ],
                            densityMap[offsetX,offsetY,offsetZ + horizontalSampleRate],
                            densityMap[offsetX,offsetY + verticalSampleRate,offsetZ + horizontalSampleRate],
                            densityMap[horizontalSampleRate + offsetX,offsetY,offsetZ], 
                            densityMap[horizontalSampleRate + offsetX,offsetY + verticalSampleRate,offsetZ],
                            densityMap[horizontalSampleRate + offsetX,offsetY,offsetZ + horizontalSampleRate],
                            densityMap[horizontalSampleRate + offsetX,offsetY + verticalSampleRate,offsetZ + horizontalSampleRate],
                            offsetX, horizontalSampleRate + offsetX, offsetY, verticalSampleRate + offsetY, offsetZ, offsetZ + horizontalSampleRate);
                    }
                    return densityMap[x, y, z] > 0 ? _prototypeMap[1] : _prototypeMap[0];
                }
                else
                {
                    return _prototypeMap[0];
                }
            });
        }

        public double TriLerp(double x, double y, double z, double q000, double q001, double q010, double q011, double q100, double q101, double q110, double q111, double x1, double x2, double y1, double y2, double z1, double z2)
        {
            double x00 = Lerp(x, x1, x2, q000, q100);
            double x10 = Lerp(x, x1, x2, q010, q110);
            double x01 = Lerp(x, x1, x2, q001, q101);
            double x11 = Lerp(x, x1, x2, q011, q111);
            double r0 = Lerp(y, y1, y2, x00, x01);
            double r1 = Lerp(y, y1, y2, x10, x11);
            return Lerp(z, z1, z2, r0, r1);
        }

        public double Lerp(double x, double x1, double x2, double q00, double q01)
        {
            return ((x2 - x) / (x2 - x1)) * q00 + ((x - x1) / (x2 - x1)) * q01;
        }

        void GenerateSlab()
        {
            for (int x = 1; x < _blockArray.XDimension - 1; x++)
            {
                for (int z = 1; z < _blockArray.ZDimension - 1; z++)
                {
                    _blockArray[x, 1, z] = _prototypeMap[1];
                }
            }
        }

        void GenerateRandom()
        {
            var random = new Random(123);

            _blockArray.Initialize((x, y, z) =>
            {
                if (x > 0 && x < _blockArray.XDimension - 1 && y > 0 && y < _blockArray.YDimension - 1 && z > 0 && z < _blockArray.ZDimension - 1)
                {
                    return random.Next(4) > 0 ? _prototypeMap[0] : _prototypeMap[1];
                }
                else
                {
                    return _prototypeMap[0];
                }
            });
        }

        void Rebuild()
        {
            var overallStopWatch = new Stopwatch();
            overallStopWatch.Start();
            
            var stopWatch = new Stopwatch();

            var lightingTime = stopWatch.Measure(() =>
            {
                _lightArray.Calculate();
            });

            var vertexLists = new List<VertexPositionColorLighting>[6];
            var indexLists = new List<short>[6];
            for (int x = 0; x < 6; x++)
            {
                vertexLists[x] = new List<VertexPositionColorLighting>();
                indexLists[x] = new List<short>();
            }
            
            var quadTime = stopWatch.Measure(() =>
            {
                _blockArray.ForEach(block =>
                {
                    if (block.Prototype.IsSolid)
                    {
                        BuildQuads(vertexLists, indexLists, block.Position);
                    }
                });
            });

            var renderTime = stopWatch.Measure(() =>
            {
                _renderer.Initialize(vertexLists, indexLists);
            });

            overallStopWatch.Stop();
            Trace.WriteLine("Lighting calcuation time: " + lightingTime + " ms");
            Trace.WriteLine("Quad building time: " + quadTime + " ms");
            Trace.WriteLine("Renderer initialization time: " + renderTime + " ms");
            Trace.WriteLine("Total chunk rebuild time: " + overallStopWatch.ElapsedMilliseconds + " ms");
            
            EventAggregator.Instance.SendMessage(new ChunkRebuilt());
        }

        void BuildQuads(List<VertexPositionColorLighting>[] vertexLists, List<short>[] indexLists, BlockPosition blockPosition)
        {
            BuildLeftQuad(vertexLists[Face.Left], indexLists[Face.Left], blockPosition);
            BuildRightQuad(vertexLists[Face.Right], indexLists[Face.Right], blockPosition);
            BuildFrontQuad(vertexLists[Face.Front], indexLists[Face.Front], blockPosition);
            BuildBackQuad(vertexLists[Face.Back], indexLists[Face.Back], blockPosition);
            BuildTopQuad(vertexLists[Face.Top], indexLists[Face.Top], blockPosition);
            BuildBottomQuad(vertexLists[Face.Bottom], indexLists[Face.Bottom], blockPosition);
        }

        void BuildLeftQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, BlockPosition blockPosition)
        {
            if (_blockArray[blockPosition.Left].IsSolid)
            {
                return;
            }

            var topLeftBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition.Up,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Left, blockPosition.Left.Up, blockPosition.Left.Back, blockPosition.Left.Up.Back, 0.85f)
            });

            var topLeftFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition.Up.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Left, blockPosition.Left.Up, blockPosition.Left.Front, blockPosition.Left.Up.Front, 0.85f)
            });

            var bottomLeftFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Left, blockPosition.Left.Down, blockPosition.Left.Front, blockPosition.Left.Down.Front, 0.85f)
            });

            var bottomLeftBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Left, blockPosition.Left.Down, blockPosition.Left.Back, blockPosition.Left.Down.Back, 0.85f)
            });

            indexList.Add(topLeftBackIndex);
            indexList.Add(topLeftFrontIndex);
            indexList.Add(bottomLeftFrontIndex);
            indexList.Add(topLeftBackIndex);
            indexList.Add(bottomLeftFrontIndex);
            indexList.Add(bottomLeftBackIndex);
        }

        void BuildRightQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, BlockPosition blockPosition)
        {
            if (_blockArray[blockPosition.Right].IsSolid)
            {
                return;
            }

            var topRightFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition.Up.Right.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Right, blockPosition.Right.Up, blockPosition.Right.Front, blockPosition.Right.Up.Front, 0.85f)
            });

            var topRightBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition.Up.Right,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Right, blockPosition.Right.Up, blockPosition.Right.Back, blockPosition.Right.Up.Back, 0.85f)
            });

            var bottomRightBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition.Right,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Right, blockPosition.Right.Down, blockPosition.Right.Back, blockPosition.Right.Down.Back, 0.85f)
            });

            var bottomRightFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition.Right.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Right, blockPosition.Right.Down, blockPosition.Right.Front, blockPosition.Right.Down.Front, 0.85f)
            });

            indexList.Add(topRightFrontIndex);
            indexList.Add(topRightBackIndex);
            indexList.Add(bottomRightBackIndex);
            indexList.Add(topRightFrontIndex);
            indexList.Add(bottomRightBackIndex);
            indexList.Add(bottomRightFrontIndex);
        }

        void BuildBackQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, BlockPosition blockPosition)
        {
            if (_blockArray[blockPosition.Back].IsSolid)
            {
                return;
            }

            var topRightBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition.Right.Up,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Back, blockPosition.Back.Up, blockPosition.Back.Right, blockPosition.Back.Up.Right, 0.85f)
            });

            var topLeftBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition.Up,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Back, blockPosition.Back.Up, blockPosition.Back.Left, blockPosition.Back.Up.Left, 0.85f)
            });

            var bottomLeftBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Back, blockPosition.Back.Down, blockPosition.Back.Left, blockPosition.Back.Down.Left, 0.85f)
            });

            var bottomRightBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition.Right,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Back, blockPosition.Back.Down, blockPosition.Back.Right, blockPosition.Back.Down.Right, 0.85f)
            });

            indexList.Add(topRightBackIndex);
            indexList.Add(topLeftBackIndex);
            indexList.Add(bottomLeftBackIndex);
            indexList.Add(topRightBackIndex);
            indexList.Add(bottomLeftBackIndex);
            indexList.Add(bottomRightBackIndex);
        }

        void BuildFrontQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, BlockPosition blockPosition)
        {
            if (_blockArray[blockPosition.Front].IsSolid)
            {
                return;
            }

            var topLeftFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition.Up.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Front, blockPosition.Front.Up, blockPosition.Front.Left, blockPosition.Front.Up.Left, 0.85f)
            });

            var topRightFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition.Right.Up.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Front, blockPosition.Front.Up, blockPosition.Front.Right, blockPosition.Front.Up.Right, 0.85f)
            });

            var bottomRightFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition.Right.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Front, blockPosition.Front.Down, blockPosition.Front.Right, blockPosition.Front.Down.Right, 0.85f)
            });

            var bottomLeftFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Front, blockPosition.Front.Down, blockPosition.Front.Left, blockPosition.Front.Down.Left, 0.85f)
            });

            indexList.Add(topLeftFrontIndex);
            indexList.Add(topRightFrontIndex);
            indexList.Add(bottomRightFrontIndex);
            indexList.Add(topLeftFrontIndex);
            indexList.Add(bottomRightFrontIndex);
            indexList.Add(bottomLeftFrontIndex);
        }

        void BuildTopQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, BlockPosition blockPosition)
        {
            if (_blockArray[blockPosition.Up].IsSolid)
            {
                return;
            }

            var topLeftBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition.Up,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Up, blockPosition.Up.Left, blockPosition.Up.Back, blockPosition.Up.Left.Back, 1f)
            });

            var topRightBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition.Right.Up,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Up, blockPosition.Up.Right, blockPosition.Up.Back, blockPosition.Up.Right.Back, 1f)
            });

            var topRightFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition.Right.Up.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Up, blockPosition.Up.Right, blockPosition.Up.Front, blockPosition.Up.Right.Front, 1f)
            });

            var topLeftFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition.Up.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Up, blockPosition.Up.Left, blockPosition.Up.Front, blockPosition.Up.Left.Front, 1f)
            });

            indexList.Add(topLeftBackIndex);
            indexList.Add(topRightBackIndex);
            indexList.Add(topRightFrontIndex);
            indexList.Add(topLeftBackIndex);
            indexList.Add(topRightFrontIndex);
            indexList.Add(topLeftFrontIndex);
        }

        void BuildBottomQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, BlockPosition blockPosition)
        {
            if (_blockArray[blockPosition.Down].IsSolid)
            {
                return;
            }

            var bottomLeftFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Down, blockPosition.Down.Left, blockPosition.Down.Front, blockPosition.Down.Left.Front, 0.70f)
            });

            var bottomRightFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition.Right.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Down, blockPosition.Down.Right, blockPosition.Down.Front, blockPosition.Down.Right.Front, 0.70f)
            });

            var bottomRightBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition.Right,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Down, blockPosition.Down.Right, blockPosition.Down.Back, blockPosition.Down.Right.Back, 0.70f)
            });

            var bottomLeftBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = blockPosition,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(blockPosition.Down, blockPosition.Down.Left, blockPosition.Down.Back, blockPosition.Down.Left.Back, 0.70f)
            });

            indexList.Add(bottomLeftFrontIndex);
            indexList.Add(bottomRightFrontIndex);
            indexList.Add(bottomRightBackIndex);
            indexList.Add(bottomLeftFrontIndex);
            indexList.Add(bottomRightBackIndex);
            indexList.Add(bottomLeftBackIndex);
        }

        Vector3 AverageLightingOver(BlockPosition adjacent, BlockPosition edge1, BlockPosition edge2, BlockPosition diagonal, float limit)
        {
            // For each vertex we examine four voxels grouped around the vertex in the plane of the face that the vertex belongs to.
            // The voxels we're interested in for a particular vertex are:
            //   The voxel adjacent to the face we're calculating
            //   One voxel along the edge of the face we're calculating
            //   The other voxel along the edge of the face we're calculating 
            //   The voxel diagonal to the face we're calculating

            float average;
            if (_blockArray[edge1].IsSolid && _blockArray[edge2].IsSolid)
            {
                // If the two edge voxels are solid then light can't get from the diagonal to the vertex we're calculating
                // so we don't include it in the average
                average = (_lightArray[adjacent] + _lightArray[edge1] + _lightArray[edge2]) / 3f;
            }
            else
            {
                average = (_lightArray[adjacent] + _lightArray[edge1] + _lightArray[edge2] + _lightArray[diagonal]) / 4f;
            }

            var percentage = Math.Min(average / _lightArray.MaximumLightLevel, limit);

            return new Vector3(percentage);
        }

        public void Handle(PlaceBlock message)
        {
            if (_blockPicker.SelectedBlock != null)
            {
                _blockArray[_blockPicker.SelectedPlacePosition] = _prototypeMap[1];

                Rebuild();
            }
        }

        public void Handle(DestroyBlock message)
        {
            if (_blockPicker.SelectedBlock != null)
            {
                _blockArray[_blockPicker.SelectedBlock.Position] = _prototypeMap[0];

                Rebuild();
            }
        }
    }
}
