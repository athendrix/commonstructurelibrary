using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSL.SQL
{
    public class AutoClosingEnumerable<T> : IEnumerable<T>, IDisposable
    {
        private readonly IEnumerable<T> innerEnumerable;
        private readonly IDisposable toDispose;

        public AutoClosingEnumerable(IEnumerable<T> innerEnumerable, IDisposable toDispose)
        {
            this.innerEnumerable = innerEnumerable;
            this.toDispose = toDispose;
        }

        public void Dispose() => toDispose.Dispose();
        public IEnumerator<T> GetEnumerator() => innerEnumerable.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => innerEnumerable.GetEnumerator();
    }
}
