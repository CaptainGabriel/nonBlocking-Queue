using System;
using System.Threading;

namespace space_goes_here
{
    class ConcurrentQueue<T>
    {
        private class Node
        {
            public Node Next;
            public readonly T Value;

            public Node(T val)
            {
                Value = val;
                Next = null;
            }
        }

        private Node _head, _tail;
        private int _size;

        public ConcurrentQueue()
        {
            Node dummy = new Node(default(T));
            _head = _tail = dummy;
            _size = 0;
        }

        public void Put(T elem)
        {
            Node newNode = new Node(elem);
            Node tail, next;
            while (true)
            {
                tail = _tail;
                next = tail.Next;
                if (tail == _tail)
                {
                    if (next == null)
                    {
                        if (Cas(ref tail.Next, next, newNode))
                        {
                            Interlocked.Increment(ref _size);
                            break;
                        }
                    }else
                    {
                        Cas(ref _tail, tail, next);
                    }  
                }
            }
            Cas(ref _tail, tail, newNode);
        }

        public T TryPush()
        {
            while (true)
            {
                Node head = _head;
                Node tail = _tail;
                Node next = head.Next;
                if (head == _head)
                {
                    if (head == tail)
                    {
                        if (next == null)
                        {
                            return default(T);
                        }
                        Cas(ref _tail, tail, next);
                    }else
                    {
                        T val = next.Value;
                        if (Cas(ref _head, head, next))
                        {
                            Interlocked.Decrement(ref _size);
                            return val;
                        }
                    }
                }
            }
        }

        public Boolean IsEmpty()
        {
            return _head == _tail && _head.Next == null;
        }

        public int Size()
        {
            return _size;
        }

        private bool Cas(ref Node destination, Node compared, Node exchange)
        {
            return compared == Interlocked.CompareExchange(ref destination, exchange, compared);
        }
    }
}
