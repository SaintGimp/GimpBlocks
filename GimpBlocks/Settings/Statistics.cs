using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GimpBlocks
{
    public class Statistics
    {
        public float FrameRate;

        public int NumberOfQuadNodes;

        public int[] NumberOfQuadNodesAtLevel = new int[30];

        public float CameraAltitude;

        public int NumberOfQuadMeshesRendered;

        public int PreviousNumberOfQuadMeshesRendered;

        public int NumberOfPendingSplits;

        public int NumberOfPendingMerges;

        public int NumberOfSplitsScheduledPerInterval;

        public int NumberOfSplitsCanceledPerInterval;

        public void Flush()
        {
            PreviousNumberOfQuadMeshesRendered = NumberOfQuadMeshesRendered;
            NumberOfQuadMeshesRendered = 0;
        }
    }
}
