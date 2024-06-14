using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenAI_API;
using OpenAI_API.Models;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using AngleSharp.Common;

namespace FlashGPT3
{

    public static class LLAMA3EmbeddingQueryRunner
    {

        internal static double temperature = 0.1;
        //internal static OpenAIAPI api = new OpenAIAPI(apiKeys: getAPI(), engine: "davinci-msft");
        //internal static OpenAIAPI api = new OpenAIAPI(apiKeys: getAPI(), engine: "davinci-002");
        //internal static OpenAI_API.OpenAIAPI api = new OpenAI_API.OpenAIAPI(apiKeys: getAPI());
        internal static Query builder = new ShortQuery();
        private static Dictionary<string, Dictionary<double, string[]>> _cache = new();
        //private static Dictionary<string, Dictionary<double, string[]>> _cache = new();


        public static string getAPI()
        {
            string api;
            try
            {
                //Pass the file path and file name to the StreamReader constructor
                StreamReader sr = new StreamReader("../../../ollamaapi.txt");
                //Read the first line of text
                api = sr.ReadLine();
                sr.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                api = null;
            }
            return api;
        }
        /// <summary>
        /// Run a query that consists of a background and a question.
        ///
        /// Uses the default <c>QueryRunner.builder</c> and <c>QueryRunner.api</c>.
        /// </summary>
        /// <param name="background">Examples for few-shot learning a function f(x).</param>
        /// <param name="question">The input to give to the learned function.</param>
        /// <param name="forceInput">Whether to force the output to be a substring of the input.</param>
        /// <returns></returns>
        public static string[]? Run(string text, bool? forceInput = null)
        {
            string query = text;
            if ((!_cache.ContainsKey(query) ||
                 !_cache[query].ContainsKey(temperature)))
            {
                if (!_cache.ContainsKey(text))
                    _cache[query] = new Dictionary<double, string[]>(1);
                string url = getAPI() + "embeddings";

                string requestBody = @"{""model"": ""llama3:70b-text"",  ""prompt"": """ + query + @"""}";
                //Console.WriteLine(requestBody);
                HttpContent content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                HttpClient client = new HttpClient();
                HttpResponseMessage task = client.PostAsync(url, content).Result;
                Dictionary<string, List<float>> responseData;
                if (task.IsSuccessStatusCode)
                {
                    // Read and parse the response body as JSON
                    string responseBody = task.Content.ReadAsStringAsync().Result;
                    JObject responseObject = JObject.Parse(responseBody);

                    // Extract the content in the "response" field
                    responseData = responseObject.ToObject<Dictionary<string, List<float>>>();
                    //Console.WriteLine(responseData);
                }
                else
                {
                    responseData = new Dictionary<string, List<float>>();
                    Console.WriteLine(task.StatusCode);
                }
                // save result to cache
                //Console.WriteLine(responseData);
                _cache[query][temperature] = new String[] { "[" + String.Join(",", responseData["embedding"]) + "]" };
            }
            // verify whether need to constraint output if not explicitly given
            //Console.WriteLine(_cache[query][temperature]);
            int cnt = 0;
            //foreach (string str in _cache[query][temperature]) { Console.WriteLine(str); cnt++; if (cnt >= 5) break; }
            return _cache[query][temperature];
        }

    }

    public static class MxbaiEmbeddingQueryRunner
    {

        internal static double temperature = 0.1;
        //internal static OpenAIAPI api = new OpenAIAPI(apiKeys: getAPI(), engine: "davinci-msft");
        //internal static OpenAIAPI api = new OpenAIAPI(apiKeys: getAPI(), engine: "davinci-002");
        //internal static OpenAI_API.OpenAIAPI api = new OpenAI_API.OpenAIAPI(apiKeys: getAPI());
        internal static Query builder = new ShortQuery();
        private static Dictionary<string, Dictionary<double, string[]>> _cache = new();
        //private static Dictionary<string, Dictionary<double, string[]>> _cache = new();


        public static string getAPI()
        {
            string api;
            try
            {
                //Pass the file path and file name to the StreamReader constructor
                StreamReader sr = new StreamReader("../../../ollamaapi.txt");
                //Read the first line of text
                api = sr.ReadLine();
                sr.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                api = null;
            }
            return api;
        }
        /// <summary>
        /// Run a query that consists of a background and a question.
        ///
        /// Uses the default <c>QueryRunner.builder</c> and <c>QueryRunner.api</c>.
        /// </summary>
        /// <param name="background">Examples for few-shot learning a function f(x).</param>
        /// <param name="question">The input to give to the learned function.</param>
        /// <param name="forceInput">Whether to force the output to be a substring of the input.</param>
        /// <returns></returns>
        public static string[]? Run(string text, bool? forceInput = null)
        {
            string query = text;
            if ((!_cache.ContainsKey(query) ||
                 !_cache[query].ContainsKey(temperature)))
            {
                if (!_cache.ContainsKey(text))
                    _cache[query] = new Dictionary<double, string[]>(1);
                string url = getAPI() + "embeddings";

                string requestBody = @"{""model"": ""mxbai-embed-large"", ""prompt"": """ + query + @"""}";
                //Console.WriteLine(requestBody);
                HttpContent content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                HttpClient client = new HttpClient();
                HttpResponseMessage task = client.PostAsync(url, content).Result;
                Dictionary<string, List<float>> responseData;
                if (task.IsSuccessStatusCode)
                {
                    // Read and parse the response body as JSON
                    string responseBody = task.Content.ReadAsStringAsync().Result;
                    JObject responseObject = JObject.Parse(responseBody);

                    // Extract the content in the "response" field
                    responseData = responseObject.ToObject<Dictionary<string, List<float>>>();
                    //Console.WriteLine(responseData);
                }
                else
                {
                    responseData = new Dictionary<string, List<float>>();
                    Console.WriteLine(task.StatusCode);
                }
                // save result to cache
                //Console.WriteLine(responseData);
                _cache[query][temperature] = new String[] { "[" + String.Join(",", responseData["embedding"]) + "]" };
            }
            // verify whether need to constraint output if not explicitly given
            //Console.WriteLine(_cache[query][temperature]);
            int cnt = 0;
            //foreach (string str in _cache[query][temperature]) { Console.WriteLine(str); cnt++; if (cnt >= 5) break; }
            return _cache[query][temperature];
        }

    }

    public static class EmbeddingQueryRunner
    {

        internal static double temperature = 0.1;
        //internal static OpenAIAPI api = new OpenAIAPI(apiKeys: getAPI(), engine: "davinci-msft");
        //internal static OpenAIAPI api = new OpenAIAPI(apiKeys: getAPI(), engine: "davinci-002");
        internal static OpenAI_API.OpenAIAPI api = new OpenAI_API.OpenAIAPI(apiKeys: getAPI());
        internal static Query builder = new ShortQuery();
        private static Dictionary<string, Dictionary<double, List<OpenAI_API.Embedding.Data>>> _cache = new();
        //private static Dictionary<string, Dictionary<double, string[]>> _cache = new();


        public static string? getAPI()
        {
            string? api;
            try
            {
                //Pass the file path and file name to the StreamReader constructor
                StreamReader sr = new StreamReader("../../../openaikey.txt");
                //Read the first line of text
                api = sr.ReadLine();
                sr.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                api = null;
            }
            return api;
        }
        /// <summary>
        /// Run a query that consists of a background and a question.
        ///
        /// Uses the default <c>QueryRunner.builder</c> and <c>QueryRunner.api</c>.
        /// </summary>
        /// <param name="background">Examples for few-shot learning a function f(x).</param>
        /// <param name="question">The input to give to the learned function.</param>
        /// <param name="forceInput">Whether to force the output to be a substring of the input.</param>
        /// <returns></returns>
        public static List<OpenAI_API.Embedding.Data>? Run(string text,
                                                      bool? forceInput = null)
        {
            // build query
            //string query = builder.Generate(background, question);
            OpenAI_API.Embedding.EmbeddingRequest query = new OpenAI_API.Embedding.EmbeddingRequest(Model.TextEmbedding3Large, text);
            //Console.WriteLine(query);
            // run if not in cache and have a key
            if ((!_cache.ContainsKey(text) ||
                 !_cache[text].ContainsKey(temperature)) &&
                api.Auth != null)
            {
                // ensure query in cache
                if (!_cache.ContainsKey(text))
                    _cache[text] = new Dictionary<double, List<OpenAI_API.Embedding.Data>>(1);
                // check if temperature exists
                var task = api.Embeddings.CreateEmbeddingAsync(query);
                // save result to cache
                _cache[text][temperature] = task.Result.Data.ToList();
            }
            // nothing to return
            if (!_cache.ContainsKey(text))
                return null;
            // verify whether need to constraint output if not explicitly given
            //bool input = (forceInput == null) ? ConstrainOutput(text) :
            //                                    forceInput.GetValueOrDefault();\
            return _cache[text][temperature];
            //bool input = false;
            // return input
            //if (!input)
            //{
            //Console.WriteLine(String.Join("\n", _cache[query][temperature]));
            //    return _cache[text][temperature];
            //}
            //else
            //{
            //Console.WriteLine(String.Join("\n", _cache[query][temperature]));
            //return (_cache[text][temperature].FirstOrDefault(
            //    v => text.Contains(v.Trim(), StringComparison.OrdinalIgnoreCase)
            //) ?? "").Trim();
            //}
        }
        

    }

    /// <summary>
    /// Abstract query class.
    /// </summary>
    public abstract class EmbeddingQuery
    {

        internal static string template = "Q: {0}\nA: {1}";

        public string Generate(Tuple<string, string>[] background, string question)
        {
            var data = background.Append(Tuple.Create(question, ""));
            return "Transformations:\n\n" +
                   String.Join("\n\n",
                               data.Select(t => String.Format(template,
                                                              t.Item1, t.Item2)))
                         .TrimEnd();
        }
    }

    /// <summary>
    /// Use short QA format.
    /// </summary>
    public class ShortEmbeddingQuery : Query
    {

    }

    /// <summary>
    /// Use long QA format.
    /// </summary>
    public class LongEmbeddingQuery : Query
    {
        internal static new string template = "Question: {0}\nAnswer: {1}";
    }

    /// <summary>
    /// Use arrow query.
    /// </summary>
    public class ArrowEmbeddingQuery : Query
    {
        internal static new string template = "{0} => {1}";
    }
}
