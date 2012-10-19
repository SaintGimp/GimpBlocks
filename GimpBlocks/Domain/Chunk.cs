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
    public class Chunk : IDisposable
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
        public const int BitmaskX = XDimension - 1;
        
        // up-down
        public const int YDimension = 64;
        public const int Log2Y = 6;
        public const int BitmaskY = YDimension - 1;

        // in-out (positive toward viewer)
        public const int ZDimension = 32;
        public const int Log2Z = 5;
        public const int BitmaskZ = ZDimension - 1;

        public ChunkPosition Position { get; private set; }

        readonly World _world;
        readonly IChunkRenderer _renderer;
        readonly BlockPrototypeMap _prototypeMap;
        
        readonly BlockArray _blockArray;
        readonly Array3<byte> _lightArray;

        public Chunk(World world, ChunkPosition position, IChunkRenderer renderer, BlockPrototypeMap prototypeMap)
        {
            Position = position;
            _world = world;
            _renderer = renderer;
            _prototypeMap = prototypeMap;

            _blockArray = new BlockArray(prototypeMap, XDimension, YDimension, ZDimension);
            _lightArray = new Array3<byte>(XDimension, YDimension, ZDimension);
        }

        public BlockPrototype GetBlockPrototype(RelativeBlockPosition position)
        {
            return _blockArray[position.X, position.Y, position.Z];
        }

        public void SetBlockPrototype(RelativeBlockPosition position, BlockPrototype prototype)
        {
            _blockArray[position.X, position.Y, position.Z] = prototype;
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
                    float filterX = (XDimension * Position.X + x) / divisor;
                    float filterY = y / divisor;
                    float filterZ = (ZDimension * Position.Z + z) / divisor;
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
                    float filterX = (XDimension * Position.X + x) / divisor;
                    float filterY = y / divisor;
                    float filterZ = (ZDimension * Position.Z + z) / divisor;
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
                        float worldX = (XDimension * Position.X + x);
                        float worldY = y;
                        float worldZ = (ZDimension * Position.Z + z);
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
                // You'd think that setting a largeish initial capacity would save us some time growing
                // the lists, but it turns out to be the exact opposite in practice.  Not sure why.
                vertexLists[x] = new List<VertexPositionColorLighting>();
                indexLists[x] = new List<short>();
            }

            // TODO: we could save iteration by only doing a slice of the chunk bounded at the top
            // by the highest solid block and at the bottom by the lowest non-solid block minus one. Anything
            // above or below that isnt going to have geometry.  Could also use a variation on that for lighting
            // calcuations.  If we want to burn extra memory in order to optimize this even more aggressively,
            // we could keep track of lowest/highest for each colum in the chunk.
            var tessellator = new Tessellator(_world);
            _blockArray.ForEach((prototype, relativeBlockPosition) =>
            {
                if (!prototype.CanBeSeenThrough)
                {
                    var worldBlockPosition = new BlockPosition(Position, relativeBlockPosition);
                    tessellator.TessellateBlock(vertexLists, indexLists, worldBlockPosition, relativeBlockPosition);
                }
            });

            var chunkOriginInWorld = new Vector3(XDimension * Position.X, 0, ZDimension * Position.Z);
            // TODO: is the conversion causing extra work here?
            _renderer.Initialize(chunkOriginInWorld, vertexLists, indexLists);
        }

        public void Draw(Vector3 cameraLocation, Matrix originBasedViewMatrix, Matrix projectionMatrix)
        {
            // TODO: frustum culling, and also skipping face VBs that are situation such that they
            // can never be seen from the current camera position.

            _renderer.Draw(cameraLocation, originBasedViewMatrix, projectionMatrix);
        }

        public byte GetLightLevel(RelativeBlockPosition position)
        {
            return _lightArray[position.X, position.Y, position.Z];
        }

        public void SetLightLevel(RelativeBlockPosition position, byte lightLevel)
        {
            _lightArray[position.X, position.Y, position.Z] = lightLevel;
        }

        public void SetInitialLighting()
        {
            // This is separate from the calculate step because right now we create
            // an array of chunks and initialize them all at once.  In that case we
            // don't want light calculations from one chunk to spill over into a completely
            // unlighted chunk because there will be a lot of unnecessary recursion.
            CastSunlight();
        }

        // TODO: we should only calculate lighting once all neighbor chunks have geometry
        // and have sunlight casted

        public void CalculateLighting()
        {
            var propagator = new LightPropagator();
            _lightArray.ForEach((lightLevel, x, y, z) =>
            {
                // TODO: For now we can propogate only if the light is full strength,
                // but that won't work for light sources that are less than full strength.  Maybe have a source
                // and destination light map so we don't have to deal with half-calculated data?

                propagator.NumberOfRecursions = 0;
                var relativeBlockPosition = new RelativeBlockPosition(x, y, z);
                if (GetLightLevel(relativeBlockPosition) == World.MaximumLightLevel)
                {
                    // TODO: when propagating sunlight, we actually only need to do x/z layers from the highest solid
                    // block down to the lowest sunlit block, plus all sunlit blocks on the outside edges regardless
                    // of y height (because they might be adjacent to an overhang on the next chunk over).

                    // TODO: because the propagator will happily move into neighboring chunks to do its work, we need to
                    // think about the implications for multi-threading and race conditions.
                    propagator.PropagateSunlightFromBlock(_world, new BlockPosition(Position, relativeBlockPosition));
                }
                //Trace.WriteLine(string.Format("Number of light propogation recursions for block {1},{2},{3}: {0}", propagator.NumberOfRecursions, x, y, z));
            });
        }

        void CastSunlight()
        {
            // TODO: would it be useful to keep track of everywhere that we
            // set sunlight and then just iterate over that list later when
            // we calculate light?  Maybe we could even then trace out the
            // edges of that volume and propogate sunlight from only those
            // blocks. Maybe more trouble than it's worth, but should think
            // about it.

            for (int x = 0; x < XDimension; x++)
            {
                for (int z = 0; z < ZDimension; z++)
                {
                    int y = YDimension - 1;
                    while (y >= 0 && _blockArray[x, y, z].CanPropagateLight)
                    {
                        _lightArray[x, y, z] = World.MaximumLightLevel;
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

        bool _disposed;
        public void Dispose()
        {
            if (!_disposed)
            {
                ((IDisposable)_renderer).Dispose();
                _disposed = true;
            }
        }
    }
}
