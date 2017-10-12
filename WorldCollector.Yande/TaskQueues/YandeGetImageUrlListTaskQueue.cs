using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CsQuery;
using Microsoft.Extensions.Logging;
using TaskQueue;
using TaskQueue.CommonTaskQueues.DownloadTaskQueue;
using TaskQueue.CommonTaskQueues.SpiderTaskQueue;

namespace WorldCollector.Yande.TaskQueues
{
    public class
        YandeGetImageUrlListTaskQueue : SpiderTaskQueue<YandeGetImageUrlListTaskQueueOptions,
            YandeGetImageUrlListTaskData>
    {
        public YandeGetImageUrlListTaskQueue(YandeGetImageUrlListTaskQueueOptions options, ILoggerFactory loggerFactory)
            : base(options, loggerFactory)
        {
        }

        protected override async Task<List<TaskData>> ExecuteAsyncInternal(YandeGetImageUrlListTaskData taskData)
        {
            var client = await GetHttpClient();
            var listUrl = string.Format(Options.ListUrlTpl, taskData.Page);
            var html = await client.GetStringAsync(listUrl);
            var cQuery = new CQ(html);
            var posts = cQuery["#post-list-posts li"];
            var imageUrls = new List<string>();
            var newTaskData = new List<TaskData>();
            var stop = false;
            foreach (var post in posts)
            {
                var id = int.Parse(post.Attributes["id"].Substring(1));
                if (Options.LastTaskImageIds?.Contains(id) == true)
                {
                    stop = true;
                    break;
                }
                var url = post.Cq().Children("a").Attr("href");
                if (url.StartsWith("//"))
                {
                    url = "http:" + url;
                }
                imageUrls.Add(url);
                Options.CurrentTaskImageIds.Add(id);
            }
            if (!stop)
            {
                newTaskData.Add(new YandeGetImageUrlListTaskData {Page = taskData.Page + 1});
            }
            if (imageUrls.Any())
            {
                newTaskData.AddRange(imageUrls.Select(t =>
                {
                    var data = new DownloadImageTaskData {Url = t};
                    var filename = WebUtility.UrlDecode(t.Substring(t.LastIndexOf('/') + 1));
                    var regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
                    var r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
                    data.RelativeFilename = r.Replace(filename, "");
                    return data;
                }));
            }
            return newTaskData.Any() ? newTaskData : null;
        }
    }
}