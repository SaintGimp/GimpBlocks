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
        public const int XDimension = 32;
        public const int Log2X = 5;
        public const int BitMaskX = XDimension - 1;
        
        // up-down
        public const int YDimension = 64;
        public const int Log2Y = 6;
        public const int BitmaskY = YDimension - 1;

        // in-out (positive toward viewer)
        public const int ZDimension = 32;
        public const int Log2Z = 5;
        public const int BitMaskZ = ZDimension - 1;

        public static readonly byte MaximumLightLevel = 15;

        readonly int _chunkX;
        readonly int _chunkZ;
        readonly World _world;
        readonly IChunkRenderer _renderer;
        readonly BlockPrototypeMap _prototypeMap;
        
        readonly BlockArray _blockArray;
        readonly Array3<byte> _lightArray;
        readonly int _baseBlockX;
        readonly int _baseBlockZ;

        public Chunk(int chunkX, int chunkZ, World world, IChunkRenderer renderer, BlockPrototypeMap prototypeMap)
        {
            _chunkX = chunkX;
            _chunkZ = chunkZ;
            _world = world;
            _renderer = renderer;
            _prototypeMap = prototypeMap;

            _blockArray = new BlockArray(prototypeMap, XDimension, YDimension, ZDimension);
            _lightArray = new Array3<byte>(XDimension, YDimension, ZDimension);
            _baseBlockX = _chunkX * XDimension;
            _baseBlockZ = _chunkZ * ZDimension;
        }

        public BlockPrototype GetBlockPrototype(int x, int y, int z)
        {
            return _blockArray[x, y, z];
        }

        public void SetBlockPrototype(BlockPosition position, BlockPrototype prototype)
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
            for (int x = 0; x <= _blockArray.XDimension; x += horizontalSampleRate)
            {
                for (int y = 0; y <= _blockArray.YDimension; y += verticalSampleRate)
                {
                    for (int z = 0; z <= _blockArray.ZDimension; z += horizontalSampleRate)
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
                return densityMap[x, y, z] > 0 ? BlockPrototype.StoneBlock : BlockPrototype.AirBlock;
            });
        }

        void GenerateSlab()
        {
            for (int x = 1; x < _blockArray.XDimension - 1; x++)
            {
                for (int z = 1; z < _blockArray.ZDimension - 1; z++)
                {
                    _blockArray[x, 1, z] = BlockPrototype.StoneBlock;
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
                    return random.Next(4) > 0 ? BlockPrototype.StoneBlock : BlockPrototype.AirBlock;
                }
                else
                {
                    return _prototypeMap[0];
                }
            });
        }

        public void Tessellate()
        {
            // TODO: I guess DX9 can be CPU-bound by the number of draw calls so we might
            // want to switch back to a single VB/IB per chunk.
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
            _blockArray.ForEach((prototype, x, y, z) =>
            {
                if (!prototype.CanBeSeenThrough)
                {
                    var worldBlockPosition = BlockPositionFor(x, y, z);
                    var relativeBlockPosition = new ChunkBlockPosition(x, y, z);
                    BuildQuads(vertexLists, indexLists, worldBlockPosition, relativeBlockPosition);
                }
            });

            var worldLocation = new Vector3(XDimension * _chunkX, 0, ZDimension * _chunkZ);
            // TODO: is the conversion causing extra work here?
            _renderer.Initialize(worldLocation, vertexLists, indexLists);
        }

        void BuildQuads(List<VertexPositionColorLighting>[] vertexLists, List<short>[] indexLists, BlockPosition worldBlockPosition, ChunkBlockPosition relativeBlockPosition)
        {
            BuildLeftQuad(vertexLists[Face.Left], indexLists[Face.Left], worldBlockPosition, relativeBlockPosition);
            BuildRightQuad(vertexLists[Face.Right], indexLists[Face.Right], worldBlockPosition, relativeBlockPosition);
            BuildFrontQuad(vertexLists[Face.Front], indexLists[Face.Front], worldBlockPosition, relativeBlockPosition);
            BuildBackQuad(vertexLists[Face.Back], indexLists[Face.Back], worldBlockPosition, relativeBlockPosition);
            BuildTopQuad(vertexLists[Face.Top], indexLists[Face.Top], worldBlockPosition, relativeBlockPosition);
            BuildBottomQuad(vertexLists[Face.Bottom], indexLists[Face.Bottom], worldBlockPosition, relativeBlockPosition);
        }

        // TODO: could maybe generalize these six methods into one once we're happy with the behavior

        // TODO: could maybe collapse these methods into one and lookup all neighboring blocks in one shot so
        // we don't duplicate lookups.  We're looking up many of these multiple times, maybe with different names.


        void BuildLeftQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, BlockPosition worldBlockPosition, ChunkBlockPosition relativeBlockPosition)
        {
            var leftBlock = _world.GetBlockAt(worldBlockPosition.Left);
            if (!leftBlock.CanBeSeenThrough)
            {
                return;
            }

            var leftUpBlock = _world.GetBlockAt(worldBlockPosition.Left.Up);
            var leftBackBlock = _world.GetBlockAt(worldBlockPosition.Left.Back);
            var leftUpBackBlock = _world.GetBlockAt(worldBlockPosition.Left.Up.Back);
            var leftFrontBlock = _world.GetBlockAt(worldBlockPosition.Left.Front);
            var leftUpFrontBlock = _world.GetBlockAt(worldBlockPosition.Left.Up.Front);
            var leftDownBlock = _world.GetBlockAt(worldBlockPosition.Left.Down);
            var leftDownFrontBlock = _world.GetBlockAt(worldBlockPosition.Left.Down.Front);
            var leftDownBackBlock = _world.GetBlockAt(worldBlockPosition.Left.Down.Back);

            var topLeftBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition.Up,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(leftBlock, leftUpBlock, leftBackBlock, leftUpBackBlock, 0.85f)
            });

            var topLeftFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition.Up.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(leftBlock, leftUpBlock, leftFrontBlock, leftUpFrontBlock, 0.85f)
            });

            var bottomLeftFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(leftBlock, leftDownBlock, leftFrontBlock, leftDownFrontBlock, 0.85f)
            });

            var bottomLeftBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(leftBlock, leftDownBlock, leftBackBlock, leftDownBackBlock, 0.85f)
            });

            indexList.Add(topLeftBackIndex);
            indexList.Add(topLeftFrontIndex);
            indexList.Add(bottomLeftFrontIndex);
            indexList.Add(topLeftBackIndex);
            indexList.Add(bottomLeftFrontIndex);
            indexList.Add(bottomLeftBackIndex);
        }

        void BuildRightQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, BlockPosition worldBlockPosition, ChunkBlockPosition relativeBlockPosition)
        {
            var rightBlock = _world.GetBlockAt(worldBlockPosition.Right);
            if (!rightBlock.CanBeSeenThrough)
            {
                return;
            }

            var rightUpBlock = _world.GetBlockAt(worldBlockPosition.Right.Up);
            var rightFrontBlock = _world.GetBlockAt(worldBlockPosition.Right.Front);
            var rightUpFrontBlock = _world.GetBlockAt(worldBlockPosition.Right.Up.Front);
            var rightBackBlock = _world.GetBlockAt(worldBlockPosition.Right.Back);
            var rightUpBackBlock = _world.GetBlockAt(worldBlockPosition.Right.Up.Back);
            var rightDownBlock = _world.GetBlockAt(worldBlockPosition.Right.Down);
            var rightDownBackBlock = _world.GetBlockAt(worldBlockPosition.Right.Down.Back);
            var rightDownFrontBlock = _world.GetBlockAt(worldBlockPosition.Right.Down.Front);

            var topRightFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition.Up.Right.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(rightBlock, rightUpBlock, rightFrontBlock, rightUpFrontBlock, 0.85f)
            });

            var topRightBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition.Up.Right,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(rightBlock, rightUpBlock, rightBackBlock, rightUpBackBlock, 0.85f)
            });

            var bottomRightBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition.Right,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(rightBlock, rightDownBlock, rightBackBlock, rightDownBackBlock, 0.85f)
            });

            var bottomRightFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition.Right.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(rightBlock, rightDownBlock, rightFrontBlock, rightDownFrontBlock, 0.85f)
            });

            indexList.Add(topRightFrontIndex);
            indexList.Add(topRightBackIndex);
            indexList.Add(bottomRightBackIndex);
            indexList.Add(topRightFrontIndex);
            indexList.Add(bottomRightBackIndex);
            indexList.Add(bottomRightFrontIndex);
        }

        void BuildBackQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, BlockPosition worldBlockPosition, ChunkBlockPosition relativeBlockPosition)
        {
            var backBlock = _world.GetBlockAt(worldBlockPosition.Back);
            if (!backBlock.CanBeSeenThrough)
            {
                return;
            }

            var backUpBlock = _world.GetBlockAt(worldBlockPosition.Back.Up);
            var backRightBlock = _world.GetBlockAt(worldBlockPosition.Back.Right);
            var backUpRightBlock = _world.GetBlockAt(worldBlockPosition.Back.Up.Right);
            var backLeftBlock = _world.GetBlockAt(worldBlockPosition.Back.Left);
            var backUpLeftBlock = _world.GetBlockAt(worldBlockPosition.Back.Up.Left);
            var backDownBlock = _world.GetBlockAt(worldBlockPosition.Back.Down);
            var backDownLeftBlock = _world.GetBlockAt(worldBlockPosition.Back.Down.Left);
            var backDownRightBlock = _world.GetBlockAt(worldBlockPosition.Back.Down.Right);

            var topRightBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition.Right.Up,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(backBlock, backUpBlock, backRightBlock, backUpRightBlock, 0.85f)
            });

            var topLeftBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition.Up,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(backBlock, backUpBlock, backLeftBlock, backUpLeftBlock, 0.85f)
            });

            var bottomLeftBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(backBlock, backDownBlock, backLeftBlock, backDownLeftBlock, 0.85f)
            });

            var bottomRightBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition.Right,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(backBlock, backDownBlock, backRightBlock, backDownRightBlock, 0.85f)
            });

            indexList.Add(topRightBackIndex);
            indexList.Add(topLeftBackIndex);
            indexList.Add(bottomLeftBackIndex);
            indexList.Add(topRightBackIndex);
            indexList.Add(bottomLeftBackIndex);
            indexList.Add(bottomRightBackIndex);
        }

        void BuildFrontQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, BlockPosition worldBlockPosition, ChunkBlockPosition relativeBlockPosition)
        {
            var frontBlock = _world.GetBlockAt(worldBlockPosition.Front);
            if (!frontBlock.CanBeSeenThrough)
            {
                return;
            }

            var frontUpBlock = _world.GetBlockAt(worldBlockPosition.Front.Up);
            var frontLeftBlock = _world.GetBlockAt(worldBlockPosition.Front.Left);
            var frontUpLeftBlock = _world.GetBlockAt(worldBlockPosition.Front.Up.Left);
            var frontRightBlock = _world.GetBlockAt(worldBlockPosition.Front.Right);
            var frontUpRightBlock = _world.GetBlockAt(worldBlockPosition.Front.Up.Right);
            var frontDownBlock = _world.GetBlockAt(worldBlockPosition.Front.Down);
            var frontDownRightBlock = _world.GetBlockAt(worldBlockPosition.Front.Down.Right);
            var frontDownLeftBlock = _world.GetBlockAt(worldBlockPosition.Front.Down.Left);

            var topLeftFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition.Up.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(frontBlock, frontUpBlock, frontLeftBlock, frontUpLeftBlock, 0.85f)
            });

            var topRightFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition.Right.Up.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(frontBlock, frontUpBlock, frontRightBlock, frontUpRightBlock, 0.85f)
            });

            var bottomRightFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition.Right.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(frontBlock, frontDownBlock, frontRightBlock, frontDownRightBlock, 0.85f)
            });

            var bottomLeftFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(frontBlock, frontDownBlock, frontLeftBlock, frontDownLeftBlock, 0.85f)
            });

            indexList.Add(topLeftFrontIndex);
            indexList.Add(topRightFrontIndex);
            indexList.Add(bottomRightFrontIndex);
            indexList.Add(topLeftFrontIndex);
            indexList.Add(bottomRightFrontIndex);
            indexList.Add(bottomLeftFrontIndex);
        }

        void BuildTopQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, BlockPosition worldBlockPosition, ChunkBlockPosition relativeBlockPosition)
        {
            var upBlock = _world.GetBlockAt(worldBlockPosition.Up);
            if (!upBlock.CanBeSeenThrough)
            {
                return;
            }

            var upLeftBlock = _world.GetBlockAt(worldBlockPosition.Up.Left);
            var upBackBlock = _world.GetBlockAt(worldBlockPosition.Up.Back);
            var upLeftBackBlock = _world.GetBlockAt(worldBlockPosition.Up.Left.Back);
            var upRightBlock = _world.GetBlockAt(worldBlockPosition.Up.Right);
            var upRightBackBlock = _world.GetBlockAt(worldBlockPosition.Up.Right.Back);
            var upFrontBlock = _world.GetBlockAt(worldBlockPosition.Up.Front);
            var upRightFrontBlock = _world.GetBlockAt(worldBlockPosition.Up.Right.Front);
            var upLeftFrontBlock = _world.GetBlockAt(worldBlockPosition.Up.Left.Front);

            var topLeftBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition.Up,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(upBlock, upLeftBlock, upBackBlock, upLeftBackBlock, 1f)
            });

            var topRightBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition.Right.Up,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(upBlock, upRightBlock, upBackBlock, upRightBackBlock, 1f)
            });

            var topRightFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition.Right.Up.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(upBlock, upRightBlock, upFrontBlock, upRightFrontBlock, 1f)
            });

            var topLeftFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition.Up.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(upBlock, upLeftBlock, upFrontBlock, upLeftFrontBlock, 1f)
            });

            indexList.Add(topLeftBackIndex);
            indexList.Add(topRightBackIndex);
            indexList.Add(topRightFrontIndex);
            indexList.Add(topLeftBackIndex);
            indexList.Add(topRightFrontIndex);
            indexList.Add(topLeftFrontIndex);
        }

        void BuildBottomQuad(List<VertexPositionColorLighting> vertexList, List<short> indexList, BlockPosition worldBlockPosition, ChunkBlockPosition relativeBlockPosition)
        {
            var downBlock = _world.GetBlockAt(worldBlockPosition.Down);
            if (!downBlock.CanBeSeenThrough)
            {
                return;
            }

            var downLeftBlock = _world.GetBlockAt(worldBlockPosition.Down.Left);
            var downFrontBlock = _world.GetBlockAt(worldBlockPosition.Down.Front);
            var downLeftFrontBlock = _world.GetBlockAt(worldBlockPosition.Down.Left.Front);
            var downRightBlock = _world.GetBlockAt(worldBlockPosition.Down.Right);
            var downRightFrontBlock = _world.GetBlockAt(worldBlockPosition.Down.Right.Front);
            var downBackBlock = _world.GetBlockAt(worldBlockPosition.Down.Back);
            var downRightBackBlock = _world.GetBlockAt(worldBlockPosition.Down.Right.Back);
            var downLeftBackBlock = _world.GetBlockAt(worldBlockPosition.Down.Left.Back);

            var bottomLeftFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(downBlock, downLeftBlock, downFrontBlock, downLeftFrontBlock, 0.70f)
            });

            var bottomRightFrontIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition.Right.Front,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(downBlock, downRightBlock, downFrontBlock, downRightFrontBlock, 0.70f)
            });

            var bottomRightBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition.Right,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(downBlock, downRightBlock, downBackBlock, downRightBackBlock, 0.70f)
            });

            var bottomLeftBackIndex = (short)vertexList.Count;
            vertexList.Add(new VertexPositionColorLighting
            {
                Position = relativeBlockPosition,
                Color = Color.LightGray,
                Lighting = AverageLightingOver(downBlock, downLeftBlock, downBackBlock, downLeftBackBlock, 0.70f)
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

        Vector3 AverageLightingOver(Block adjacent, Block edge1, Block edge2, Block diagonal, float limit)
        {
            // For each vertex we examine four voxels grouped around the vertex in the plane of the face that the vertex belongs to.
            // The voxels we're interested in for a particular vertex are:
            //   The voxel adjacent to the face we're calculating
            //   One voxel along the edge of the face we're calculating
            //   The other voxel along the edge of the face we're calculating 
            //   The voxel diagonal to the face we're calculating

            float average;
            if (!edge1.CanPropagateLight && !edge2.CanPropagateLight)
            {
                // If the two edge voxels are not transparent then light can't get from the diagonal to the vertex we're calculating
                // so we don't include it in the average
                average = (adjacent.LightLevel + edge1.LightLevel + edge2.LightLevel) / 3f;
            }
            else
            {
                average = (adjacent.LightLevel + edge1.LightLevel + edge2.LightLevel + diagonal.LightLevel) / 4f;
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

        public byte GetLightLevel(int x, int y, int z)
        {
            return _lightArray[x, y, z];
        }

        public void SetLightLevel(int x, int y, int z, byte lightLevel)
        {
            _lightArray[x, y, z] = lightLevel;
        }

        // TODO: we should only generate lighting once all neighbor chunks have geometry

        // TODO: we could use a hybrid system where we use an internal chunk position
        // to traverse the interior of the chunk then switch to a world position
        // to do the edges of the chunk, which can easily travel to other chunks at the
        // expense of slower perf.  Maybe.  Or we could just use world position everywhere
        // and not constrain light propogation the boundaries of chunks - just let it go
        // where it may.

        public void SetInitialLighting()
        {
            // This is separate from the calculate step because right now we create
            // an array of chunks and initialize them all at once.  In that case we
            // don't want light calculations from one chunk to spill over into a completely
            // unlighted chunk because there will be a lot of unnecessary recursion.
            CastSunlight();
        }

        public void CalculateLighting()
        {
            var propagator = new LightPropagator();
            _lightArray.ForEach((lightLevel, x, y, z) =>
            {
                // TODO: we don't need to propogate light from blocks that contain only light that's already
                // been propogated from elsewhere. For now we can propogate only if the light is full strength,
                // but that won't work for light sources that are less than full strength.  Maybe have a source
                // and destination light map so we don't have to deal with half-calculated data?

                if (GetLightLevel(x, y, z) == MaximumLightLevel)
                {
                    propagator.PropagateLightFromBlock(_world, BlockPositionFor(x, y, z));
                }
                
                //Trace.WriteLine(string.Format("Number of light propogation recursions for block {1},{2},{3}: {0}", propagator.NumberOfRecursions, x, y, z));
            });


        }

        BlockPosition BlockPositionFor(int x, int y, int z)
        {
            return new BlockPosition(_baseBlockX + x, y, _baseBlockZ + z);
        }

        void CastSunlight()
        {
            for (int x = 0; x < XDimension; x++)
            {
                for (int z = 0; z < ZDimension; z++)
                {
                    int y = YDimension - 1;
                    while (y >= 0 && _blockArray[x, y, z].CanPropagateLight)
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
