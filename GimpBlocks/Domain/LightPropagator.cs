using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace GimpBlocks
{
    public class LightPropagator
    {
        private ConcurrentQueue<LightingOperation> pendingOperations = new ConcurrentQueue<LightingOperation>();

        public static int NumberOfLightingOperations = 0;

        public void EnqueueOperation(LightingOperation operation)
        {
            pendingOperations.Enqueue(operation);
        }

        public void Execute()
        {
            LightingOperation currentOperation;
            while (pendingOperations.TryDequeue(out currentOperation))
            {
                NumberOfLightingOperations++;

                var newOperations = currentOperation.Propagate();
                foreach (var newOperation in newOperations)
                {
                    pendingOperations.Enqueue(newOperation);
                }
            }
        }
    }
}
