using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machine.Specifications;

namespace GimpBlocks.Specifications
{
    public class FakeEventListener
    {
        readonly List<object> receivedMessages = new List<object>();

        protected void RecordMessage<T>(T message)
        {
            receivedMessages.Add(message);
        }

        public IEnumerable<T> Messages<T>()
        {
            return receivedMessages.OfType<T>();
        }
        public T FirstMessage<T>()
        {
            return Messages<T>().FirstOrDefault();
        }

        public void ShouldReceive<T>()
        {
            FirstMessage<T>().ShouldNotBeNull();
        }

        public void ShouldNotReceive<T>()
        {
            FirstMessage<T>().ShouldBeNull();
        }
    }
}
