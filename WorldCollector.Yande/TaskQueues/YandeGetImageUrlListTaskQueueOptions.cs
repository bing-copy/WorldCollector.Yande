using System.Collections.Generic;
using TaskQueue.CommonTaskQueues.SpiderTaskQueue;

namespace WorldCollector.Yande.TaskQueues
{
    public class YandeGetImageUrlListTaskQueueOptions : SpiderTaskQueueOptions
    {
        public string ListUrlTpl { get; set; }
        public List<int> LastTaskImageIds { get; set; } = new List<int>();
        public List<int> CurrentTaskImageIds { get; set; } = new List<int>();
    }
}
