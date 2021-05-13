using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace EF.RPC.Impl
{
    /// <summary>
    /// 多线程安全的kv map集合
    /// 比上一版本减少了三个object的占用
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    public class LinkMap<K, V> : MapInterface<K, V>
    {
        LinkMapNode<K, V> head;
        LinkMapNode<K, V> tail;
        volatile int size = 0;
        public LinkMap()
        {
            this.head = new LinkMapNode<K, V>();
            this.tail = new LinkMapNode<K, V>();
            this.head.prev = this.head.next = tail;
            this.tail.prev = this.tail.next = head;
        }
        public void clear()
        {
            this.head.prev = this.head.next = tail;
            this.tail.prev = this.tail.next = head;
        }

        public bool containsKey(K key)
        {
            LinkMapNode<K, V> temp = head.next;
            for (; ; )
            {
                if (temp == tail) return false;
                if (key.Equals(temp.getK())) return true;
                temp = temp.next;
            }

        }


        public bool containsValue(V value)
        {
            LinkMapNode<K, V> temp = head.next;
            for (; ; )
            {
                if (temp == tail) return false;
                if (value.Equals(temp.getV())) return true;
                temp = temp.next;
            }
        }

        public V get(K key)
        {
            LinkMapNode<K, V> temp = head.next;
            for (; ; )
            {
                if (temp == tail) return default(V);
                if (key.Equals(temp.getK())&& temp!=tail&& temp != head) return temp.getV();
                temp = temp.next;
            }
        }
        public LinkMapNode<K, V> getNode(K key)
        {
            LinkMapNode<K, V> temp = tail.prev;
            for (; ; )
            {
                if (temp == head) return null;
                if (key.Equals(temp.getK())) return temp;
                temp = temp.prev;
            }
        }
        public IEnumerable<LinkMapNode<K, V>> getNodeOneByOne()
        {
            LinkMapNode<K, V> temp = tail.prev;
            for (; ; )
            {
                if (temp == head) break;
                yield return temp;
                temp = temp.prev;
            }
        }
        public bool isEmpty()
        {
            return this.head.next == tail;
        }

        public Collection<K> keys()
        {
            Collection<K> ks = new Collection<K>();
            LinkMapNode<K, V> temp = head.next;
            for (; ; )
            {
                if (temp == tail) return ks;
                ks.Add(temp.getK());
                temp = temp.next;
            }

        }

        public V put(K key, V value)
        {
            V v = default(V);
            LinkMapNode<K, V> temp = getNode(key);
            if (null == temp)
            {
                LinkMapNode<K, V> node = new LinkMapNode<K, V>(key, value);
                lock (tail.prev)
                {
                    temp = getNode(key);
                    if (null == temp)
                    {                      
                        node.next = tail;
                        tail.prev.next = node;
                        node.prev = tail.prev;
                        tail.prev = node;
                        //lock (sizeLock)
                        //{
                            size++;
                        //}
                    }
                    else
                    {
                        v = temp.setV(value);
                    }
                }

            }
            else
            {
                ///保证后赢者赢
                lock (temp)
                {
                    v = temp.setV(value);
                }

            }
            return v;

        }

        public V remove(K key)
        {
            V v = default(V);
            if (containsKey(key))
            {
                lock (tail)
                {
                    LinkMapNode<K, V> temp = getNode(key);
                    if (null != temp)
                    {
                        temp.prev.next = temp.next;
                        temp.next.prev = temp.prev;
                        //lock (sizeLock)
                        //{
                            size--;
                        //}
                    }
                }

            }
            return v;
        }

        public int Size()
        {
            return size;
        }

        public Collection<V> values()
        {
            Collection<V> vs = new Collection<V>();
            LinkMapNode<K, V> temp = head.next;
            for (; ; )
            {
                if (temp == tail) return vs;
                vs.Add(temp.getV());
                temp = temp.next;
            }
        }

    }
    public interface MapInterface<K, V>
    {
        int Size();
        bool isEmpty();
        bool containsKey(K key);
        bool containsValue(V value);
        V get(K key);
        V put(K key, V value);
        V remove(K key);
        void clear();

        Collection<V> values();
        Collection<K> keys();
    }
    public class LinkMapNode<K, V> : MapNode<K, V>
    {
        public LinkMapNode<K, V> next;
        public LinkMapNode<K, V> prev;
        public LinkMapNode(K key, V vel) : base(key, vel)
        {

        }
        public LinkMapNode()
        {

        }

    }
    public abstract class MapNode<K, V>
    {

        protected K key;
        protected V vel;
        protected MapNode(K key, V vel)
        {
            this.key = key;
            this.vel = vel;
        }
        protected MapNode()
        {

        }

        public V getV()
        {
            return vel;
        }
        public V setV(V vel)
        {
            try
            {
                return vel;
            }
            finally
            {
                this.vel = vel;
            }

        }
        public K getK()
        {
            return key;
        }


    }
}
