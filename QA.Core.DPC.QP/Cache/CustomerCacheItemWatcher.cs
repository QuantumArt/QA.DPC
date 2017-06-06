using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Transactions;
using QA.Core.Cache;
using QA.Core.Data;
using QA.Core.DPC.QP.Services;

namespace QA.Core.DPC.QP.Cache
{
    public class CustomerCacheItemWatcher : IDisposable, ICacheItemWatcher
    {
        private static readonly object _locker = new object();
        private static bool _disposing = false;
        private readonly Timer _timer;
        protected readonly string _connectionString;
        private Dictionary<int, ContentModification> _modifications = new Dictionary<int, ContentModification>();
        private Dictionary<string, TableModification> _tableModifications = new Dictionary<string, TableModification>();
        private readonly ConcurrentBag<CacheItemTracker> _trackers;
        private readonly IContentInvalidator _invalidator;
        private readonly InvalidationMode _mode;
        private readonly string _cmdText = @"SELECT [CONTENT_ID], [LIVE_MODIFIED], [STAGE_MODIFIED] FROM [CONTENT_MODIFICATION] WITH (NOLOCK)";

        private volatile bool _isBusy = false;
        private readonly TimeSpan _pollPeriod;
        private readonly TimeSpan _dueTime;

        private readonly Func<Tuple<int[], string[]>, bool> _onInvalidate;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mode">режим работы</param>
        /// <param name="invalidator">объект, инвалидирующий кеш</param>
        /// <param name="connectionName">имя строки подключения</param>
        public CustomerCacheItemWatcher(InvalidationMode mode, IContentInvalidator invalidator, IConnectionProvider connectionProvider)
            : this(mode, Timeout.InfiniteTimeSpan, invalidator, connectionProvider, 0, false)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mode">режим работы</param>
        /// <param name="pollPeriod">интервал опроса бд</param>
        /// <param name="invalidator">объект, инвалидирующий кеш</param>
        /// <param name="connectionProvider">провайдер строк подключения</param>
        /// <param name="dueTime">отложенный запуск (ms)</param>
        /// <param name="onInvalidate">вызывается при удалении старых записей</param>
        public CustomerCacheItemWatcher(InvalidationMode mode, TimeSpan pollPeriod, IContentInvalidator invalidator,
            IConnectionProvider connectionProvider,
            int dueTime = 0,
            bool useTimer = true,
            Func<Tuple<int[], string[]>, bool> onInvalidate = null)
        {
            Throws.IfArgumentNull(_ => connectionProvider);
            Throws.IfArgumentNull(_ => invalidator);

            _dueTime = TimeSpan.FromMilliseconds(dueTime);
            _pollPeriod = pollPeriod;
            if (useTimer)
            {
                _timer = new Timer(OnTick, null, 0, Timeout.Infinite);
            }

            _connectionString = connectionProvider.GetConnection();
            Throws.IfArgumentNullOrEmpty(_connectionString, nameof(_connectionString));

            _trackers = new ConcurrentBag<CacheItemTracker>();
            _mode = mode;
            _invalidator = invalidator;
            _onInvalidate = onInvalidate;
            Throws.IfArgumentNull(_ => _connectionString);
        }

        /// <summary>
        /// Запуск таймера
        /// </summary>
        public void Start()
        {
            if (_timer == null)
                throw new InvalidOperationException("Parameters for timer was not specified.");

            _timer.Change(_dueTime, _pollPeriod);
        }

        /// <summary>
        /// Подключение пользовательского анализатора изменений в БД
        /// </summary>
        /// <param name="tracker"></param>
        public void AttachTracker(CacheItemTracker tracker)
        {
            Throws.IfArgumentNull(tracker, _ => tracker);

            _trackers.Add(tracker);
        }

        public void TrackChanges()
        {
            TrackChangesInternal();
        }

        /// <summary>
        /// Обработчик таймера
        /// </summary>
        /// <param name="state"></param>
        protected virtual void OnTick(object state)
        {
            TrackChangesInternal();
        }

        private void TrackChangesInternal()
        {
            if (_isBusy)
                return;

            lock (_locker)
            {
                if (_disposing)
                {
                    throw new InvalidOperationException("obj is being disposed!");
                }

                if (_isBusy)
                    return;

                try
                {
                    _isBusy = true;
                    Dictionary<int, ContentModification> newValues = new Dictionary<int, ContentModification>();
                    Dictionary<string, TableModification> tableChanges = new Dictionary<string, TableModification>();


                    GetData(newValues);

                    var trackers = _trackers.ToList();

                    foreach (var tracker in trackers)
                    {
                        tracker.TrackChanges(tableChanges);
                    }

                    List<int> itemsIds = new List<int>();
                    List<string> itemsTables = new List<string>();
                    if (_modifications != null && _modifications.Count > 0)
                    {

                        // check for updates
                        Compare(_modifications, newValues, itemsIds);

                        // invalidate
                        if (itemsIds.Count > 0)
                        {
                            _invalidator.InvalidateIds(_mode, itemsIds.ToArray());
                            ObjectFactoryBase.Logger.Debug(_ => ("Invalidating a set of ids " + string.Join(", ", itemsIds)));
                        }
                    }

                    if (_tableModifications != null && _tableModifications.Count > 0)
                    {

                        // check for updates
                        Compare(_tableModifications, tableChanges, itemsTables);

                        // invalidate
                        if (itemsTables.Count > 0)
                        {
                            _invalidator.InvalidateTables(_mode, itemsTables.ToArray());
                            ObjectFactoryBase.Logger.Debug(_ => ("Invalidating a set of tables " + string.Join(", ", itemsTables)));
                        }
                    }

                    if (_onInvalidate != null && (itemsIds.Count > 0 || itemsTables.Count > 0))
                    {
                        _onInvalidate(new Tuple<int[], string[]>(itemsIds.ToArray(), itemsTables.ToArray()));
                    }

                    if (newValues != null && newValues.Count > 0)
                    {
                        _modifications = newValues;
                    }

                    if (tableChanges != null && tableChanges.Count > 0)
                    {
                        _tableModifications = tableChanges;
                    }

                    newValues = null;
                    tableChanges = null;
                }
                catch (Exception ex)
                {
                    ObjectFactoryBase.Logger.ErrorException("qp watcher error", ex);
                    ObjectFactoryBase.Logger.Error("qp watcher error StackTrace: {0}", ex.StackTrace);
                }
                finally
                {
                    _isBusy = false;
                }
            }
        }

        protected virtual void GetData(Dictionary<int, ContentModification> newValues)
        {
            // при возникновении исключения в базе, даже если его перехватить
            // родительская транзакция все равно откатывается, и дальнейшая работа с базой будет вызывать ошибки.
            // чтобы этого не было, выполняем код вне родительской транзакции (TransactionScopeOption.Suppress). 
            using (var tsSuppressed = new TransactionScope(TransactionScopeOption.Suppress))
            {
                if (newValues == null) throw new ArgumentNullException(nameof(newValues));

                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(_cmdText, con))
                    {
                        cmd.CommandType = CommandType.Text;
                        con.Open();

                        try
                        {
                            // производим запрос - без этого не будет работать dependency
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var item = new ContentModification
                                    {
                                        ContentId = Convert.ToInt32(reader["CONTENT_ID"]),
                                        LiveModified = Convert.ToDateTime(reader["LIVE_MODIFIED"]),
                                        StageModified = Convert.ToDateTime(reader["STAGE_MODIFIED"])
                                    };

                                    newValues[item.ContentId] = item;
                                }
                            }
                        }
                        finally
                        {
                            con.Close();
                        }
                    }
                }
                tsSuppressed.Complete();
            }
        }

        private void Compare<T, V>(Dictionary<T, V> modifications,
            Dictionary<T, V> newValues,
            List<T> idsToUpdate)
                where V : TableModification
        {
            foreach (var item in newValues)
            {
                if (!modifications.ContainsKey(item.Key))
                {
                    idsToUpdate.Add(item.Key);
                    continue;
                }

                var old = modifications[item.Key];

                if ((_mode == InvalidationMode.Live || _mode == InvalidationMode.All)
                    && (old.LiveModified < item.Value.LiveModified))
                {
                    idsToUpdate.Add(item.Key);
                }

                if ((_mode == InvalidationMode.Stage || _mode == InvalidationMode.All)
                    && (old.StageModified < item.Value.StageModified))
                {
                    idsToUpdate.Add(item.Key);
                }
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            _disposing = true;
            if (_timer != null)
                _timer.Dispose();
        }

        #endregion

        /// <summary>
        /// Класс, описывающий последнее изменение
        /// </summary>
        protected class ContentModification : TableModification
        {
            public int ContentId { get; set; }
            //    public DateTime LiveModified { get; set; }
            //    public DateTime StageModified { get; set; }
        }
    }
}
