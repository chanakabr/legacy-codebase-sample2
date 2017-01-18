using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using KLogMonitor;
using System.Reflection;
using KlogMonitorHelper;

namespace Core.Notification
{
    /// <summary>
    /// Responsible for consuming T objects from a queue(m_Queue member),
    /// and processing each object according to m_ActionQueueItem(of type Action<T>) member. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NotificationConsumer<T> where T : class
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region private Properties
        private bool processing = true;
        private ConcurrentQueue<T> m_Queue = null;
        private Action<T> m_ActionQueueItem;
        private int m_SleepNum = 0;
        private int m_SleepInidicatorNum = 0;
        #endregion

        #region Constructor
        public NotificationConsumer(ConcurrentQueue<T> queue, Action<T> actionQueueItem, int sleepNum, int sleepIndicatorNum)
        {
            m_ActionQueueItem = actionQueueItem;
            m_Queue = queue;
            m_SleepNum = sleepNum;
            m_SleepInidicatorNum = sleepIndicatorNum;
        }
        #endregion

        /// <summary>
        /// Starts consuming job by instantiate a Task object
        /// and start it by the Task.Factory.
        /// the job consuming job itself is implemented by the Process() method.
        /// </summary>
        /// <returns></returns>
        public Task Start(ContextData contextData)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                // load monitor and logs context data
                contextData.Load();

                Process();
            });
            return task;
        }

        /// <summary>
        /// In each iteration fetching T object from the queue(m_Queue member)
        /// and process it by calling the method responsible for processing the object(m_ActionQueueItem member). 
        /// </summary>
        private void Process()
        {
            int counter = 0;
            while (processing == true)
            {
                if (m_Queue.IsEmpty == false)
                {
                    T item;
                    bool result = m_Queue.TryDequeue(out item);
                    if (result == true)
                    {
                        try
                        {
                            m_ActionQueueItem(item);
                        }
                        catch (Exception ex)
                        {
                            log.Error("", ex);
                            //TBD:Handle exception                           
                        }
                        counter++;
                    }
                }
                else
                {
                    processing = false;
                }

                if (counter % m_SleepInidicatorNum == 0)
                {
                    Thread.Sleep(m_SleepNum);
                }
            }
        }

        #region Old code - handle threads manually

        private ManualResetEvent m_Handle;
        public NotificationConsumer(ConcurrentQueue<T> queue, Action<T> actionQueueItem, ManualResetEvent handle, int sleepNum, int sleepIndivatorNum)
        {
            m_ActionQueueItem = actionQueueItem;
            m_Queue = queue;
            m_Handle = handle;
            m_SleepNum = sleepNum;
            m_SleepInidicatorNum = sleepIndivatorNum;
        }

        public void StartByThread()
        {
            ThreadStart start = delegate { ProcessByThread(); };
            Thread workingThread = new Thread(start);
            workingThread.Start();
        }

        private void ProcessByThread()
        {
            int counter = 0;
            while (processing == true)
            {
                if (m_Queue.IsEmpty == false)
                {
                    T item;
                    bool result = m_Queue.TryDequeue(out item);
                    if (result == true)
                    {
                        m_ActionQueueItem(item);
                        counter++;
                    }
                }
                else
                {
                    processing = false;
                }

                if (counter % m_SleepInidicatorNum == 0)
                {
                    Thread.Sleep(m_SleepNum);
                }
            }
            m_Handle.Set();
        }

        #endregion
    }
}
