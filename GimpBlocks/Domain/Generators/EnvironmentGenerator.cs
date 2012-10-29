using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GimpBlocks
{
    public interface IEnvironmentGenerator
    {
        void Generate(Chunk chunk);
    }

    public class EnvironmentGenerator : IEnvironmentGenerator
    {
        public void Generate(Chunk chunk)
        {
            var terrainGenerator = new TerrainGenerator1();
            terrainGenerator.GenerateTerrain(chunk);
        }
    }
}
