using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;

namespace GimpBlocks.Specifications.WorldSpecs
{
    [Subject("Tessellation")]
    public class when_a_block_is_placed_then_destroyed : BasicWorldContext
    {
        Establish context = () =>
        {
            CreateWorld(1);
        };
    }
}
