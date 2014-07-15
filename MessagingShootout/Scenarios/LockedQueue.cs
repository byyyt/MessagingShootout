using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingShootout
{
    internal class LockedQueue<T>
    {
        private Queue<T> q = new Queue<T>();

        public void Enqueue(T msg)
        {
            lock (q)
            {
                q.Enqueue(msg);
            }
        }

        public bool TryDequeue(out T msg)
        {
            lock (q)
            {
                if (q.Count == 0)
                {
                    msg = default(T);
                    return false;
                }
                msg = q.Dequeue();
                return true;
            }
        }
    }
}
