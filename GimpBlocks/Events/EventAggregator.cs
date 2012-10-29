using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GimpBlocks
{
    public class EventAggregator
    {
        readonly object lockObject = new object();
        readonly List<WeakReference> listeners = new List<WeakReference>();

        static EventAggregator()
        {
            Instance = new EventAggregator();
        }

        public static EventAggregator Instance { get; private set; }

        public void SendMessage<T>(T message)
        {
            IEnumerable<IListener<T>> recipients;
            lock (lockObject)
            {
                recipients = FindEligibleListeners<T>();
            }

            SendMessageToRecipients(message, recipients);
        }

        private void SendMessageToRecipients<T>(T message, IEnumerable<IListener<T>> recipients)
        {
            foreach (var recipient in recipients)
            {
                recipient.Handle(message);
            }
        }

        private IEnumerable<IListener<T>> FindEligibleListeners<T>()
        {
            var eligibleListeners = new List<IListener<T>>();
            foreach (var weakReference in listeners)
            {
                // We need to create a strong reference before testing aliveness
                // so that the GC doesn't yank it out from under us.  Don't convert
                // this to a LINQ expression, despite what Resharper says, because
                // a LINQ expression doesn't guarentee that behavior
                var strongReference = weakReference.Target as IListener<T>;
                if (strongReference != null)
                {
                    eligibleListeners.Add(strongReference);
                }
            }

            return eligibleListeners;
        }

        public void AddListener(object listener)
        {
            lock (lockObject)
            {
                PruneDeadReferences();

                if (!IsRegistered(listener))
                {
                    listeners.Add(new WeakReference((listener)));
                }
            }
        }

        protected bool IsRegistered(object listener)
        {
            return listeners.Exists(x => x.Target == listener);
        }

        private void PruneDeadReferences()
        {
            listeners.RemoveAll(x => x.Target == null);
        }
    }
}
