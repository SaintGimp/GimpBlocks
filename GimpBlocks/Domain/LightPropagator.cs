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

        public static int NumberOfInitialLightingOperations = 0;
        public static int NumberOfAdditionalLightingOperations = 0;

        public void EnqueueOperation(LightingOperation operation)
        {
            pendingOperations.Enqueue(operation);
            NumberOfInitialLightingOperations++;
        }

        public void Execute()
        {
            LightingOperation currentOperation;
            while (pendingOperations.TryDequeue(out currentOperation))
            {
                var newOperations = currentOperation.Propagate();
                foreach (var newOperation in newOperations)
                {
                    pendingOperations.Enqueue(newOperation);
                    NumberOfAdditionalLightingOperations++;
                }
            }
        }
    }
}
