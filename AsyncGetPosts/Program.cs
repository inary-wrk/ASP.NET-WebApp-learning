using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace AsyncGetPosts
{
    class Post
    {
        public int userid { get; set; }
        public int id { get; set; }
        public string title { get; set; }
        public string body { get; set; }
    }

    static class Program
    {
        private static readonly HttpClient _client = new();
        static async Task Main()
        {
            var postsTasks = Enumerable.Range(4, 10)
                .Select(async x => await GetPostAsync(x)).ToList();

            var file = File.Create("result.txt");
            using var fs = new StreamWriter(file);
            while (postsTasks.Count > 0)
            {
                var finishedTask = await Task.WhenAny(postsTasks);
                postsTasks.Remove(finishedTask);

                try
                {
                    var post = await finishedTask;
                    await fs.WriteLineAsync($"{post.userid}\n{post.id}\n{post.title}\n{post.body}\n");
                }
                catch (AggregateException exceptions)
                {
                    foreach (var exception in exceptions.InnerExceptions)
                    {
                        Console.WriteLine(exception.Message);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);

                }
            }
        }

        async static Task<Post> GetPostAsync(int id)
        {
                using var responce = await _client.GetAsync($"https://jsonplaceholder.typicode.com/posts/{id}");

                if (responce.StatusCode != System.Net.HttpStatusCode.OK) Console.WriteLine(responce.ReasonPhrase);

                var jsonResponce = await responce.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Post>(jsonResponce);
        }
    }
}
