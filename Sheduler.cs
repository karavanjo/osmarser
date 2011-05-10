using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace OsmImportToSqlServer.Utilites
{
    public class Scheduler
    {
        /// <summary>
        /// Объект синхронизации потоков
        /// </summary>
        private AutoResetEvent _event = new AutoResetEvent(false);

        /// <summary>
        /// Устанавливается в null, если управляемый объектом Scheduler ресурс не занят.
        /// </summary>
        private Thread _runningThread;

        /// <summary>
        /// Потоки и их запросы ожидающие выполнения
        /// </summary>
        private Dictionary<Thread, ISchedulerOrdering> _waiting = new Dictionary<Thread, ISchedulerOrdering>();

        /// <summary>
        /// Метод <see cref="Enter"/> вызывается перед тем, как поток начнет использовать уравляемый ресурс.
        /// Метод не выполняется до тех пор пока управляемый ресур не освободиться и объект <see cref="Sheduler"/>
        /// не примет решение, что подошла очередь выполнения этого запроса
        /// </summary>
        /// <param name="s"></param>
        public void Enter(ISchedulerOrdering s)
        {
            var thisThread = Thread.CurrentThread;

            lock (this)
            {
                // Определяем не занят ли планировщик
                if (_runningThread == null)
                {
                    // Немедленно начинаем выполнение поступившего запроса
                    _runningThread = thisThread;
                    return;
                }
                _waiting.Add(thisThread, s);
            }

            lock (thisThread)
            {
                //Блокируем поток до тех пор, пока планировщик не решит сделать его текущим
                while (thisThread != _runningThread)
                {
                    _event.WaitOne();
                    _event.Set();   // даем возможность другим потокам проверить своё состояние
                    Thread.Sleep(1);
                }
                _event.Reset();
            }

            lock (this)
            {
                _waiting.Remove(thisThread);
            }
        }

        /// <summary>
        /// Вызов метода <see cref="Done"/> указывает на то, что текущий поток завершил работу
        /// и управляемый ресурс освободился
        /// </summary>
        public void Done()
        {
            lock (this)
            {
                if (_runningThread != Thread.CurrentThread)
                    throw new ThreadStateException(@"Wrong Thread");

                Int32 waitCount = _waiting.Count;
                if (waitCount <= 0)
                {
                    _runningThread = null;
                }
                else if (waitCount == 1)
                {
                    _runningThread = _waiting.First().Key;
                    _waiting.Remove(_runningThread);
                    _event.Set();
                }
                else
                {
                    var next = _waiting.First();
                    foreach (var wait in _waiting)
                    {
                        if (wait.Value.ScheduleBefore(next.Value))
                        {
                            next = wait;
                        }
                    }

                    _runningThread = next.Key;
                    _event.Set();
                }
            }
        }
    }

    public interface ISchedulerOrdering
    {
        Boolean ScheduleBefore(ISchedulerOrdering s);
    }

    /// <summary>
    /// Вспомогательный класс
    /// </summary>
    static partial class ConvertTo
    {
        /// <summary>
        /// Получить первый элемент коллекции
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static KeyValuePair<Thread, ISchedulerOrdering> First(this Dictionary<Thread, ISchedulerOrdering> collection)
        {
            foreach (var item in collection)
            {
                return item;
            }
            throw new ArgumentException();
        }
    }
}
