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
    public class Chunk
    {
        // Design tradeoffs: We could have one array of structs that contain all block information.  The advantage there
        // is that if we need to access multiple pieces of information about a block simultaneously, we only need to do one
        // array lookup.  Or we could store each type of information in a separate array which requires a separate array
        // lookup for each piece of information, but in the case where we need to iterate a lot over one type of information,
        // we would get fewer cache misses.  With that approach we could also optimize storage for each type of information separately, so
        // something that's usually sparse could be stored in a sparse octtree where something else that varies a lot could
        // be an an array.  It's not clear which strategy is best in the long term.

        // TODO: I read a rumor that the XNA Vector3 class is able to use SSE instructions to operate on all three fields
        // simultaneously.  If that's true, could we take advantage of that in any way? Maybe in noise generation?
        // http://techcraft.codeplex.com/discussions/253344


        // left-right
        public static readonly int XDimension = 32;
        // up-down
        public static readonly int YDimension = 64;
        // in-out (positive toward viewer)
        public static readonly int ZDimension = 32;

        public static readonly byte MaximumLightLevel = 15;

        readonly int _chunkX;
        readonly int _chunkZ;
        readonly IChunkRenderer _renderer;
        readonly BlockArray _blockArray;
        readonly Array3<byte> _lightArray;
        readonly BlockPrototypeMap _prototypeMap;

        public Chunk(int chunkX, int chunkZ, IChunkRenderer renderer, BlockPrototypeMap prototypeMap)
        {
            _chunkX = chunkX;
            _chunkZ = chunkZ;
            _renderer = renderer;
            _blockArray = new BlockArray(prototypeMap, XDimension, YDimension, ZDimension);
            _lightArray = new Array3<byte>(XDimension, YDimension, ZDimension);
            _prototypeMap = prototypeMap;
        }

        public void SetBlock(ChunkBlockPosition position, BlockPrototype prototype)
        {
            _blockArray[position] = prototype;
        }

        public void Generate()
        {
            //GenerateRandom();
            //GenerateSlab();
            GenerateTerrain1();
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
                    float filterX = (XDimension * _chunkX + x) / divisor;
                    float filterY = y / divisor;
                    float filterZ = (ZDimension * _chunkZ + z) / divisor;
                    return filter.GetValue(filterX, filterY, filterZ) < 1.4 ? _prototypeMap[1] : _prototypeMap[0];
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
                    float filterX = (XDimension * _chunkX + x) / divisor;
                    float filterY = y / divisor;
                    float filterZ = (ZDimension * _chunkZ + z) / divisor;
                    return filter.GetValue(filterX, filterY, filterZ) < 1.5 ? _prototypeMap[1] : _prototypeMap[0];
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

            // Setting the horizontal sampe rate to 4 smoothes things a bit, 16 smoothes a lot,
            // looking much more gentle and not very sensitive to detail in the noise.

            var horizontalSampleRate = 4;
            var verticalSampleRate = 4;

            var maxNoise = float.MinValue;
            var minNoise = float.MaxValue;
            var densityMap = new double[_blockArray.XDimension + 1, _blockArray.YDimension + 1, _blockArray.ZDimension + 1];
            for (int x = 0; x < _blockArray.XDimension; x += horizontalSampleRate)
            {
                for (int y = 0; y < _blockArray.YDimension; y += verticalSampleRate)
                {
                    for (int z = 0; z < _blockArray.ZDimension; z += horizontalSampleRate)
                    {
                        float worldX = (XDimension * _chunkX + x);
                        float worldY = y;
                        float worldZ = (ZDimension * _chunkZ + z);
                        var noise = inputScaler.GetValue(worldX, worldY, worldZ);

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
                        densityMap[x, y, z] = GimpMath.TriLerp(x, y, z,
                            densityMap[offsetX, offsetY, offsetZ],
                            densityMap[offsetX, verticalSampleRate + offsetY, offsetZ],
                            densityMap[offsetX, offsetY, offsetZ + horizontalSampleRate],
                            densityMap[offsetX, offsetY + verticalSampleRate, offsetZ + horizontalSampleRate],
                            densityMap[horizontalSampleRate + offsetX, offsetY, offsetZ],
                            densityMap[horizontalSampleRate + offsetX, offsetY + verticalSampleRate, offsetZ],
                            densityMap[horizontalSampleRate + offsetX, offsetY, offsetZ + horizontalSampleRate],
                            densityMap[horizontalSampleRate + offsetX, offsetY + verticalSampleRate, offsetZ + horizontalSampleRate],
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

        public void BuildGeometry()
        {
            var vertexLists = new List<VertexPositionColorLighting>[6];
            var indexLists = new List<short>[6];
            for (int x = 0; x < 6; x++)
            {
                vertexLists[x] = new List<VertexPositionColorLighting>();
                indexLists[x] = new List<short>();
            }

            // TODO: we could save iteration by only doing a slice of the chunk bounded at the top
            // by the highest solid block and at the bottom by the lowest non-solid block minus one. Anything
            // above or below that isnt going to have geometry.  Could also use a variation on that for lighting
            // calcuations.  If we want to burn extra memory in order to optimize this even more aggressively,
            // we could keep track of lowest/highest for each colum in the chunk.
            _blockArray.ForEach(block =>
            {
                if (block.Prototype.IsSolid)
                {
                    BuildQuads(vertexLists, indexLists, block.Position);
                }
            });

            var worldLocation = new Vector3(XDimension * _chunkX, 0, ZDimension * _chunkZ);
            _renderer.Initialize(worldLocation, vertexLists, indexLists);
        }

        void BuildQuads(List<VertexPositionColorLighting>[] vertexLists, List<short>[] indexLists, ChunkBlockPosition blockPosition)
        {
            BuildLeftQuad(vertexLists[Face.Left], indexLists[Face.Left], blockPosition);
            BuildRightQuad(vertexLists[Face.Right], indexLists[Face.Right], blockPosition);
            BuildFrontQuad(vertexLists[Face.Front], indexLists[Face.Front], blockPosition);
            BuildBackQuad(vertexLists[Face.Back], indexLists[Face.Back], blockPosition);
            BuildTopQuad(vertexLists[Face.Top], indexLists[Face.Top], blockPosition);
            BuildBottomQuad(vertexLists[Face.Bottom], indexLists[Face.Bottom], blockPosition);
        }

        // TODO: could maybe generalize these six methods into one once we're happy with the behavior

        void BuildLeftQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, ChunkBlockPosition blockPosition)
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

        void BuildRightQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, ChunkBlockPosition blockPosition)
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

        void BuildBackQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, ChunkBlockPosition blockPosition)
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

        void BuildFrontQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, ChunkBlockPosition blockPosition)
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

        void BuildTopQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, ChunkBlockPosition blockPosition)
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

        void BuildBottomQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, ChunkBlockPosition blockPosition)
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

        // Light propogation between chunks: once the chunk has its internal lighting calculated, we can take all of the edge blocks in a chunk
        // and just propogate that current light level over to the neighboring chunk. The recursion will be naturally limited by the light level.

        Vector3 AverageLightingOver(ChunkBlockPosition adjacent, ChunkBlockPosition edge1, ChunkBlockPosition edge2, ChunkBlockPosition diagonal, float limit)
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

            var percentage = Math.Min(average / MaximumLightLevel, limit);

            return new Vector3(percentage);
        }

        public void Draw(Vector3 cameraLocation, Matrix originBasedViewMatrix, Matrix projectionMatrix)
        {
            // TODO: frustum culling, and also skipping face VBs that are situation such that they
            // can never be seen from the current camera position.

            _renderer.Draw(cameraLocation, originBasedViewMatrix, projectionMatrix);
        }

        public bool IsSolid(ChunkBlockPosition blockPosition)
        {
            return _blockArray[blockPosition].IsSolid;
        }

        public byte GetLightLevel(ChunkBlockPosition blockPosition)
        {
            return _lightArray[blockPosition];
        }

        public void SetLightLevel(ChunkBlockPosition blockPosition, byte lightLevel)
        {
            _lightArray[blockPosition] = lightLevel;
        }

        public void CalculateInternalLighting()
        {
            CastSunlight();
            var propogator = new LightPropagator();
            _lightArray.ForEach((lightLevel, x, y, z) =>
            {
                // TODO: we don't need to propogate light from blocks that contain only light that's already
                // been propogated from elsewhere. For now we can propogate only if the light is full strength,
                // but that won't work for light sources that are less than full strength.  Maybe have a source
                // and destination light map so we don't have to deal with half-calculated data?

                var blockPosition = new ChunkBlockPosition(x, y, z);
                if (GetLightLevel(blockPosition) == MaximumLightLevel)
                {
                    propogator.PropagateLightInChunk(this, blockPosition);
                }
            });
        }

        void CastSunlight()
        {
            for (int x = 0; x < XDimension; x++)
            {
                for (int z = 0; z < ZDimension; z++)
                {
                    int y = YDimension - 1;
                    while (y >= 0 && !_blockArray[x, y, z].IsSolid)
                    {
                        _lightArray[x, y, z] = MaximumLightLevel;
                        y--;
                    }

                    // Anything not in sunlight starts out completely dark
                    while (y >= 0)
                    {
                        _lightArray[x, y, z] = 0;
                        y--;
                    }
                }
            }
        }
    }
}
