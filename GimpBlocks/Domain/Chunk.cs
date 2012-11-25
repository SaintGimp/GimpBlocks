using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
        public const int XDimension = 16;
        public const int Log2X = 4;
        public const int BitmaskX = XDimension - 1;
        
        // up-down
        public const int YDimension = 128;
        public const int Log2Y = 7;
        public const int BitmaskY = YDimension - 1;

        // in-out (positive toward viewer)
        public const int ZDimension = 16;
        public const int Log2Z = 4;
        public const int BitmaskZ = ZDimension - 1;

        public ChunkPosition Position { get; private set; }
        public BlockPosition OriginInWorld { get; private set; }
        readonly IEnumerable<ChunkPosition> neighborhoodPositions;
 
        readonly World world;
        readonly IEnvironmentGenerator environmentGenerator;
        readonly IChunkRenderer renderer;
        
        readonly BlockArray blockArray;
        readonly Array3<byte> lightArray;

        int highestVisibleBlock = -1;
        int lowestSunlitBlock = YDimension + 1;
        int lowestInvisibleBlock = YDimension + 1;

        public static int NumberOfLightPropagations = 0;

        public Chunk(World world, ChunkPosition position, IEnvironmentGenerator environmentGenerator, IChunkRenderer renderer, BlockPrototypeMap prototypeMap)
        {
            Position = position;
            this.world = world;
            this.environmentGenerator = environmentGenerator;
            this.renderer = renderer;

            blockArray = new BlockArray(prototypeMap, XDimension, YDimension, ZDimension);
            lightArray = new Array3<byte>(XDimension, YDimension, ZDimension);

            OriginInWorld = new BlockPosition(Position, new RelativeBlockPosition(0, 0, 0));
            neighborhoodPositions = new[]
            {
                Position,
                Position.Left,
                Position.Right,
                Position.Front,
                Position.Back
            };
        }

        public BlockPrototype GetBlockPrototype(RelativeBlockPosition position)
        {
            return blockArray[position.X, position.Y, position.Z];
        }

        public void SetBlockPrototype(RelativeBlockPosition position, BlockPrototype prototype)
        {
            blockArray[position.X, position.Y, position.Z] = prototype;

            if (prototype.CanBeSeen && position.Y > highestVisibleBlock)
            {
                highestVisibleBlock = position.Y;
            }

            if (!prototype.CanBeSeen && position.Y < lowestInvisibleBlock)
            {
                lowestInvisibleBlock = position.Y;
            }
        }

        public void Generate()
        {
            environmentGenerator.Generate(this);
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

            // We save iteration by only doing a slice of the chunk bounded at the top
            // by the highest solid block and at the bottom by the lowest non-solid block in the
            // neighborhood minus one. Anything above or below that isnt going to have geometry.
            // TODO: If we want to burn extra memory in order to optimize this even more aggressively,
            // we could keep track of lowest/highest for each colum in the chunk.

            var lowerTesselationLimit = Math.Max(GetLowestInvisibleBlockInNeighborhood() - 1, 0);
            var tessellator = new Tessellator(world);
            for (int x = 0; x < XDimension; x++)
            {
                for (int y = lowerTesselationLimit; y <= highestVisibleBlock; y++)
                {
                    for (int z = 0; z < ZDimension; z++)
                    {
                        var position = new RelativeBlockPosition(x, y, z);
                        var prototype = GetBlockPrototype(position);
                        if (prototype.CanBeSeen)
                        {
                            var worldBlockPosition = new BlockPosition(Position, position);
                            tessellator.TessellateBlock(vertexLists, indexLists, worldBlockPosition, position);
                        }
                    }
                }
            }

            // TODO: is the conversion causing extra work here?
            renderer.Initialize((Vector3)OriginInWorld, vertexLists, indexLists);
        }

        int GetLowestInvisibleBlockInNeighborhood()
        {
            var lowest = neighborhoodPositions.Select(position => world.GetChunkAt(position))
                .Where(chunk => chunk != null)
                .Min(chunk => chunk.lowestInvisibleBlock);
            return Math.Max(lowest, 0);
        }

        public void Draw(Vector3 cameraLocation, Matrix originBasedViewMatrix, Matrix projectionMatrix)
        {
            // TODO: frustum culling, and also skipping face VBs that are situation such that they
            // can never be seen from the current camera position.

            renderer.Draw(cameraLocation, originBasedViewMatrix, projectionMatrix);
        }

        public byte GetLightLevel(RelativeBlockPosition position)
        {
            return lightArray[position.X, position.Y, position.Z];
        }

        public void SetLightLevel(RelativeBlockPosition position, byte lightLevel)
        {
            lightArray[position.X, position.Y, position.Z] = lightLevel;
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
            var calculationLimit = GetHighestVisibleBlockInNeighborhood();
            var propagator = new LightPropagator();

            for (int x = 0; x < XDimension; x++)
            {
                for (int y = lowestSunlitBlock; y <= calculationLimit; y++)
                {
                    for (int z = 0; z < ZDimension; z++)
                    {
                        // TODO: For now we can propogate only if the light is full strength,
                        // but that won't work for light sources that are less than full strength.  Maybe have a source
                        // and destination light map so we don't have to deal with half-calculated data?
                        if (NeedsPropogation(x, y, z))
                        {
                            propagator.NumberOfRecursions = 0;
                            var relativeBlockPosition = new RelativeBlockPosition(x, y, z);
                            if (GetLightLevel(relativeBlockPosition) == World.MaximumLightLevel)
                            {
                                // TODO: because the propagator will happily move into neighboring chunks to do its work, we need to
                                // think about the implications for multi-threading and race conditions.
                                propagator.PropagateSunlightFromBlock(world, new BlockPosition(Position, relativeBlockPosition));
                                NumberOfLightPropagations++;
                            }
                        }
                    }
                }
            }
        }

        int GetHighestVisibleBlockInNeighborhood()
        {
            return neighborhoodPositions.Select(position => world.GetChunkAt(position))
                .Where(chunk => chunk != null)
                .Max(chunk => chunk.highestVisibleBlock);
        }

        bool NeedsPropogation(int x, int y, int z)
        {
            // When propagating sunlight, we actually only need to do x/z layers from the highest solid
            // block minus one down to the lowest sunlit block, plus all sunlit blocks on the outside edges up
            // to the highest solid block of any neighbors (because they might be adjacent to an overhang on the
            // next chunk over).

            if (y < highestVisibleBlock)
            {
                return true;
            }
            else if (x == 0 || x == XDimension - 1 || z == 0 || z == ZDimension - 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        void CastSunlight()
        {
            // TODO: would it be useful to keep track of everywhere that we
            // set sunlight and then just iterate over that list later when
            // we calculate light?  Maybe we could even then trace out the
            // edges of that volume and propogate sunlight from only those
            // blocks. Maybe more trouble than it's worth, but should think
            // about it.

            // TODO: could we use GetHighestVisibleBlockInNeighborhood to optimize the
            // amount of sunlight we need to cast? If not done carefully, though, I
            // guess we could have a problem with progagation leaking into the unset
            // area at the top and causing a lot more propagation work which kind of
            // defeats the purpose.

            for (int x = 0; x < XDimension; x++)
            {
                for (int z = 0; z < ZDimension; z++)
                {
                    int y = YDimension - 1;
                    while (y >= 0 && blockArray[x, y, z].CanPropagateLight)
                    {
                        lightArray[x, y, z] = World.MaximumLightLevel;

                        if (y < lowestSunlitBlock)
                        {
                            lowestSunlitBlock = y;
                        }

                        y--;
                    }

                    // Anything not in sunlight starts out completely dark
                    while (y >= 0)
                    {
                        lightArray[x, y, z] = 0;
                        y--;
                    }
                }
            }
        }

        bool disposed;

        public void Dispose()
        {
            if (!disposed)
            {
                ((IDisposable)renderer).Dispose();
                disposed = true;
            }
        }
    }
}
