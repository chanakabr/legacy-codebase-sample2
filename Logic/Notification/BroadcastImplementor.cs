using ApiObjects.Notification;
using ConfigurationManager;
using DAL;
using KlogMonitorHelper;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Notification
{
    /// <summary>
    /// Responsible for handling broadcast type notifications,
    /// this class designed to handle a big number of messages using multithreaded ablities.
    /// </summary>
    public class BroadcastImplementor : BaseImplementor
    {

        #region Constructor
        public BroadcastImplementor(NotificationRequest request)
            : base(request)
        {
        }
        #endregion

        #region protected methods
        /// <summary>
        /// Send the messages in the messagesList param,
        /// if the messages count is below or equal to MinMessagesNumberForTasks property(the value is taken from config file)
        /// the messages are sent synchronically, otherwise the messages inserted into a queue and a multithreaded operation
        /// that open several tasks objects access this queue simultaneously and process the messages.
        /// (The tasks number is determined according to MaxTasksNumber property that its value taken from config file).  
        /// ater all messages procesed the messages are saved to db in a bulk operation.
        /// </summary>
        /// <param name="messagesList"></param>
        protected override void SendMessages(List<NotificationMessage> messagesList, bool bIsInsertBulkOfMessagesToDB)
        {

            if (messagesList != null && messagesList.Count > 0)
            {
                int messagesCount = messagesList.Count;
                if (messagesCount <= this.MinMessagesNumberForTasks) //No need for tasks openning
                {
                    SendMessagesSync(messagesList);
                }
                else
                {
                    SendMessagesBroadCast(messagesList);
                    //SendMessagesBroadCastParallel(messagesList);
                    //SendMessagesBroadCastByThreads(messagesList);
                }
                if (bIsInsertBulkOfMessagesToDB)
                    Task.Run(() => { InsertMessagesBulkCopy(messagesList); });
            }
        }

        /// <summary>
        /// Concrete implementation of the GetUsersDevices() method,
        /// here the users devices is fetched by group id.
        /// </summary>
        /// <param name="groupID"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        protected override DataTable GetUsersDevices(long groupID, long? userID, NotificationMessageType eType)
        {
            DataTable res = null;
            switch (eType)
            {
                case NotificationMessageType.Push:
                    {
                        // unlike other notifications, push notifications should be sent only to devices the user is currently using,
                        // hence we use a different SP for that.
                        res = UsersDal.GetDevicesToUsers(groupID, userID, true);
                        break;
                    }
                default:
                    {
                        res = UsersDal.GetDevicesToUsers(groupID, userID);
                        break;
                    }
            }

            return res;
            //DataTable dtUsersDevices = UsersDal.GetDevicesToUsers(groupID , null);
            //return dtUsersDevices;
        }



        #endregion

        #region private methods
        /// <summary>
        /// Send the messages in the messagesList param synchronically.
        /// </summary>
        /// <param name="messagesList"></param>
        private void SendMessagesSync(List<NotificationMessage> messagesList)
        {
            int messagesCount = messagesList.Count;
            for (int i = 0; i < messagesCount; i++)
            {
                try
                {
                    ProcessOneMessage(messagesList[i]);
                }
                catch
                {
                    //TBD:Take care of failures
                }
            }
        }
        /// <summary>
        /// Send the messages in the messagesList param simultaneously,
        /// The messages inserted into a queue the message proceesing is done multithreaded 
        /// by opening several tasks objects that access this queue simultaneously.
        /// </summary>
        /// <param name="messagesList"></param>
        private void SendMessagesBroadCast(List<NotificationMessage> messagesList)
        {
            ConcurrentQueue<NotificationMessage> messagesQueue = new ConcurrentQueue<NotificationMessage>(messagesList);
            int tasksNumber = this.MaxTasksNumber;
            Task[] tasks = new Task[tasksNumber];

            int threadSleepNum = this.ThreadSleepNumber;
            int threadSleepIndicator = this.ThreadSleepIndicator;

            // save monitor and logs context data
            ContextData contextData = new ContextData();

            for (int i = 0; i < tasksNumber; i++)
            {
                NotificationConsumer<NotificationMessage> consumer = new NotificationConsumer<NotificationMessage>(messagesQueue, ProcessOneMessage, threadSleepNum, threadSleepIndicator);
                tasks[i] = consumer.Start(contextData);
            }
            Task.WaitAll(tasks);
        }


        public void SendMessagesBroadCastParallel(List<NotificationMessage> messagesList)
        {
            ConcurrentQueue<NotificationMessage> messagesQueue = new ConcurrentQueue<NotificationMessage>(messagesList);
            Parallel.ForEach(messagesQueue, new ParallelOptions { MaxDegreeOfParallelism = 10 },
                            (item) =>
                            {
                                ProcessOneMessage(item);
                            });

        }



        #endregion


        #region private properties

        /// <summary>
        /// Determine the number of tasks to open in order to 
        /// proccess big nummber of messages simultaneously.        
        /// </summary>
        private int MaxTasksNumber
        {
            get
            {
                int ret = 10;
                if (ApplicationConfiguration.NotificationConfiguration.BroadcastTasksNum.IntValue > 0)
                {
                    return ApplicationConfiguration.NotificationConfiguration.BroadcastTasksNum.IntValue;
                }
                return ret;
            }
        }

        /// <summary>
        /// Determine the minimum number of messages that is needed  
        /// to proccess the messages in a multithreaded way (implemented by tasks openning).
        /// </summary>
        private int MinMessagesNumberForTasks
        {
            get
            {
                int ret = 100;
                if (ApplicationConfiguration.NotificationConfiguration.BroadcastMinMessagesNumForTasks.IntValue > 0)
                {
                    return ApplicationConfiguration.NotificationConfiguration.BroadcastMinMessagesNumForTasks.IntValue;
                }
                return ret;
            }
        }
        /// <summary>
        /// Determine the time in milliseconds that each task will "sleep"  
        /// when processing the  the messages in a multithreaded way.
        /// </summary>
        private int ThreadSleepNumber
        {
            get
            {
                int ret = 50;
                if (ApplicationConfiguration.NotificationConfiguration.BroadcastThreadSleep.IntValue > 0)
                {
                    return ApplicationConfiguration.NotificationConfiguration.BroadcastThreadSleep.IntValue;
                }
                return ret;
            }
        }
        /// <summary>
        /// Determine the number of messages to be processed sequentially  by each task
        /// before call "sleeping" the task.        
        /// </summary>
        private int ThreadSleepIndicator
        {
            get
            {
                int ret = 100;
                if (ApplicationConfiguration.NotificationConfiguration.BroadcastThreadSleepIndicator.IntValue > 0)
                {
                    return ApplicationConfiguration.NotificationConfiguration.BroadcastThreadSleepIndicator.IntValue;
                }
                return ret;
            }
        }
        #endregion


        #region Old code, processing messages by threads instead of tasks
        public void SendMessagesByThreads(List<NotificationMessage> messagesList)
        {
            if (messagesList != null && messagesList.Count > 0)
            {
                SendMessagesBroadCastByThreads(messagesList);
            }
        }

        private void SendMessagesBroadCastByThreads(List<NotificationMessage> messagesList)
        {
            ConcurrentQueue<NotificationMessage> messagesQueue = new ConcurrentQueue<NotificationMessage>(messagesList);
            ManualResetEvent[] handles = new ManualResetEvent[MaxTasksNumber];
            int threadSleepNum = 25;
            int threadSleepIndicator = 100;

            for (int i = 0; i < MaxTasksNumber; i++)
            {
                handles[i] = new ManualResetEvent(false);
                NotificationConsumer<NotificationMessage> consumer = new NotificationConsumer<NotificationMessage>(messagesQueue, ProcessOneMessage, handles[i], threadSleepNum, threadSleepIndicator);
                consumer.StartByThread();
            }
            WaitHandle.WaitAll(handles);
        }


        #endregion
    }
}
