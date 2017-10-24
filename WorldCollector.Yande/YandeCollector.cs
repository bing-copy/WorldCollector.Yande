using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskQueue;
using TaskQueue.CommonTaskQueues.DownloadTaskQueue;
using WorldCollector.Yande.Models;
using WorldCollector.Yande.TaskQueues;

namespace WorldCollector.Yande
{
    public class YandeCollector : TaskQueuePool
    {
        private readonly YandeCollectorOptions _options;
        private const string ProxyPurpose = "Yande";
        private List<int> _lastImageIds = new List<int>();
        private readonly List<int> _currentImageIds = new List<int>();

        public YandeCollector(YandeCollectorOptions options, ILoggerFactory loggerFactory) : base(
            new TaskQueuePoolOptions {MaxThreads = options.MaxThreads, MinInterval = options.MinInterval},
            loggerFactory)
        {
            _options = options;
        }

        public override void Stop()
        {
            base.Stop();
            _lastImageIds.Clear();
            _currentImageIds.Clear();
        }

        public override object GetState()
        {
            return $"{base.GetState()}{Environment.NewLine}Last Ids: {_lastImageIds.Count}";
        }

        public override async Task Start()
        {
            if (Status == TaskQueuePoolStatus.Idle)
            {
                var db = new YandeDbContext(new DbContextOptionsBuilder<YandeDbContext>()
                    .UseMySql(_options.DbConnectionString).Options);
                var lastRecord = await db.CollectRecords.OrderByDescending(t => t.CollectDt)
                    .FirstOrDefaultAsync(t => t.Site == _options.Site);
                if (!string.IsNullOrEmpty(lastRecord?.Ids))
                {
                    _lastImageIds = lastRecord.Ids.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                        .Select(int.Parse).ToList();
                }
                _currentImageIds.Clear();
                if (!this.Any())
                {
                    Add(new YandeGetImageUrlListTaskQueue(new YandeGetImageUrlListTaskQueueOptions
                    {
                        MaxThreads = _options.ListThreads,
                        Interval = _options.ListInterval,
                        ListUrlTpl = _options.ListUrlTemplate,
                        CurrentTaskImageIds = _currentImageIds,
                        LastTaskImageIds = _lastImageIds,
                        Purpose = ProxyPurpose
                    }, LoggerFactory));

                    Add(new DownloadImageTaskQueue(new DownloadImageTaskQueueOptions
                    {
                        MaxThreads = _options.DownloadThreads,
                        Interval = _options.DownloadInterval,
                        DownloadPath = _options.DownloadPath,
                        Purpose = ProxyPurpose
                    }, LoggerFactory));
                }
                Enqueue(new YandeGetImageUrlListTaskData {Page = 1});
                await base.Start();

                if (this.All(t => t.Completed) && _currentImageIds.Any())
                {
                    var record = new CollectRecord
                    {
                        CollectDt = DateTime.Now,
                        Ids = string.Join(",", _currentImageIds),
                        Site = _options.Site
                    };
                    db.Add(record);
                    await db.SaveChangesAsync();
                }
            }
        }
    }
}