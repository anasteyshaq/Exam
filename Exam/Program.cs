//Напишіть Thread-safe (потоко-безпечну) реалізацію LIFO Queue 
//що зберігає дані у структурі array. 
//Продемонструйте працездатність при запуску у декількох потоках,
//що одночасно додають та виймають записи до queue.

using System;
using System.Threading;

namespace Practice1_2
{
    class Program
    {
        static void Main(string[] args)
        {
            var queue = new ConcurrLIFOQueue<int>();

            for (int i = 0; i < 10000; i++)
            {
                queue.Push(i);
            }
            ThreadPool.QueueUserWorkItem((o) => DequeueWhileExists(queue)); //добавляем метод DequeueWhileExists в очередь на выполнение. Метод выполнится когда поток из пула потоков станет доступным
            ThreadPool.QueueUserWorkItem((o) => DequeueWhileExists(queue));


            ThreadPool.QueueUserWorkItem((o) => PutThenPick(queue));
            ThreadPool.QueueUserWorkItem((o) => PutThenPick(queue));

            Thread.Sleep(2000);

            Console.ReadLine();
        }
        // добавляем и получаем
        static void PutThenPick(ConcurrLIFOQueue<int> queue)
        {
            int res;

            for (int i = 0; i < 10000; i++)
            {
                queue.Push(i);
                queue.TryPop(out res);
            }
            Console.WriteLine("done");
        }
        
        // удалить все элементы
        static void DequeueWhileExists(ConcurrLIFOQueue<int> queue)
        {
            int res;

            while (true)
            {
                if (queue.Count() > 0)
                {
                    queue.TryPop(out res);
                }
            }
        }
    }

    class ConcurrLIFOQueue<T>
    {
        private T[] holder; // массив объектов класса Т
        private int length; // длина стэка

        public ConcurrLIFOQueue()
        {
            length = 0;
            holder = new T[0];
        }
        // удалить объект 
        public bool TryPop(out T result)
        {
            lock (this)
            {
                if (length > 0)
                {
                    length--;
                    result = holder[length];
                    var newHolder = new T[length];

                    for (int i = 0; i < length - 1; i++)
                    {
                        newHolder[i] = holder[i];
                    }

                    holder = newHolder;
                    return true;
                }
                else
                {
                    result = default(T);
                    return false;
                }
            }
        }
        // вставляем элемент 
        public void Push(T item)
        {
            lock (this)
            {
                length++; 
                var newHolder = new T[length];

                for (int i = 0; i < length - 2; i++)
                {
                    newHolder[i] = holder[i]; 
                }

                newHolder[length - 1] = item;
                holder = newHolder;
            }
        }

        public int Count()
        {
            return length;
        }
    }
}
