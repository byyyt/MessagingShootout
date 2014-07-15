using System.Collections.Generic;

namespace MessagingShootout.Scenarios.LockedQueue
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
