using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AngleSharp.Io;
using System.Net.Http;
using Microsoft.ProgramSynthesis.Transformation.Text.Build.NodeTypes;
using Newtonsoft.Json;
using OpenAI_API;
using OpenAI_API.Models;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Net.Http.Json;
using OpenAI_API.Chat;

namespace FlashGPT3
{

    public static class MistralQueryRunner
    {

        internal static double temperature = 0.2;
        //internal static OpenAIAPI api = new OpenAIAPI(apiKeys: getAPI(), engine: "davinci-msft");
        //internal static OpenAIAPI api = new OpenAIAPI(apiKeys: getAPI(), engine: "davinci-002");
        //internal static OpenAIAPI api = new OpenAIAPI(apiKeys: getAPI());
        internal static Query builder = new MistralShortQuery();

        private static Dictionary<string, Dictionary<double, string[]>> _cache = new();


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
        public static string Run(Tuple<string, string>[] background,
                                 string question,
                                 bool? forceInput = null)
        {
            // build query
            string query = builder.Generate(background, question);
            //Console.WriteLine(query);
            // run if not in cache and have a key
            if ((!_cache.ContainsKey(query) ||
                 !_cache[query].ContainsKey(temperature)))
            {
                // ensure query in cache
                if (!_cache.ContainsKey(query))
                    _cache[query] = new Dictionary<double, string[]>(1);
                // check if temperature exists
                string url = getAPI() + "generate";

                string requestBody = @"{""model"": ""mixtral:8x22b-text"", ""prompt"": """ + query + @""",  ""stream"": false,  ""options"": {""seed"": 101, ""temperature"": 0.2, ""stop"": [""\n"", ""Q:""]}}";
                HttpContent content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                HttpClient client = new HttpClient();
                HttpResponseMessage task = client.PostAsync(url, content).Result;
                string responseData;
                if (task.IsSuccessStatusCode)
                {
                    // Read and parse the response body as JSON
                    string responseBody = task.Content.ReadAsStringAsync().Result;
                    JObject responseObject = JObject.Parse(responseBody);

                    // Extract the content in the "response" field
                    responseData = (string)responseObject["response"];
                    //Console.WriteLine(content);
                    //Console.WriteLine(responseData);
                    
                }
                else
                {
                    Console.WriteLine(task.StatusCode);
                    //Console.WriteLine(content);
                    responseData = "";
                }
                // save result to cache
                _cache[query][temperature] = new String[] { responseData };
            }
            // nothing to return
            if (!_cache.ContainsKey(query))
                return "";
            // verify whether need to constraint output if not explicitly given
            bool input = (forceInput == null) ? ConstrainOutput(background) :
                                                forceInput.GetValueOrDefault();
            // return input
            if (!input)
            {
                //Console.WriteLine(String.Join("\n", _cache[query][temperature]));
                return _cache[query][temperature][0].Trim();
            }
            else
            {
                //Console.WriteLine(String.Join("\n", _cache[query][temperature]));
                return (_cache[query][temperature].FirstOrDefault(
                    v => question.Contains(v.Trim(), StringComparison.OrdinalIgnoreCase)
                ) ?? "").Trim();
            }
        }

        /// <summary>
        /// Detect whether the output is a substring of the input.
        /// </summary>
        /// <param name="query">Query</param>
        /// <returns></returns>
        public static bool ConstrainOutput(Tuple<string, string>[] query)
        {
            return query.All(t => t.Item1.Contains(t.Item2));
        }

        /// <summary>
        /// Load data into static cache.
        /// </summary>
        /// <param name="file">Filename of json cache.</param>
        public static void LoadCache(string file, bool reset = false)
        {
            // reset cache
            if (reset)
                _cache = new Dictionary<string, Dictionary<double, string[]>>();
            // load from JSON file
            if (File.Exists(file))
            {
                Dictionary<string, Dictionary<double, string[]>> rawData =
                    JsonConvert.DeserializeObject<Dictionary<string, Dictionary<double, string[]>>>(
                        File.ReadAllText(file)
                    );
                // add to the cache
                foreach (var pair in rawData)
                    if (!_cache.ContainsKey(pair.Key))
                        _cache.Add(pair.Key, pair.Value);
            }
        }

        /// <summary>
        /// Save the cache.
        /// </summary>
        /// <param name="file"></param>
        public static void SaveCache(string file)
        {
            if (!Directory.GetParent(file).Exists)
                Directory.GetParent(file).Create();
            File.WriteAllText(file, JsonConvert.SerializeObject(_cache,
                                                                Formatting.Indented));
        }

    }

    public static class LLaMAQueryRunner
    {

        internal static double temperature = 0.2;
        //internal static OpenAIAPI api = new OpenAIAPI(apiKeys: getAPI(), engine: "davinci-msft");
        //internal static OpenAIAPI api = new OpenAIAPI(apiKeys: getAPI(), engine: "davinci-002");
        //internal static OpenAIAPI api = new OpenAIAPI(apiKeys: getAPI());
        internal static Query builder = new LLaMAShortQuery();

        private static Dictionary<string, Dictionary<double, string[]>> _cache = new();


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
        public static string Run(Tuple<string, string>[] background,
                                 string question,
                                 bool? forceInput = null)
        {
            // build query
            string query = builder.Generate(background, question);
            //Console.WriteLine(query);
            // run if not in cache and have a key
            if ((!_cache.ContainsKey(query) ||
                 !_cache[query].ContainsKey(temperature)))
            {
                // ensure query in cache
                if (!_cache.ContainsKey(query))
                    _cache[query] = new Dictionary<double, string[]>(1);
                // check if temperature exists
                string url = getAPI() + "generate";

                //string requestBody = "{\"model\": \"llama2\",\r\n  \"prompt: " + query + "\",\r\n  \"stream\": false,\r\n  \"format\": \"json\",\r\n  \"options\": {\r\n    \"seed\": 101,\r\n    \"temperature\": 0.2\r\n  }\r\n}";
                string requestBody = "{\"model\": \"llama2\",  \"prompt\": \"" + query + "\",  \"stream\": false,  \"format\": \"json\",  \"options\": {\"seed\": 101,\"temperature\": 0.2}}";
                //Console.WriteLine(requestBody);
                HttpContent content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                //var requestBody = new
                //{
                //    model = "llama2",
                //    prompt = query,
                //    stream = false,
                //    format = "json", 
                //    options = new
                //    {
                //        seed = 101,
                //        temperature = 0.2
                //    }
                //};
                //HttpContent content = new JsonContent(requestBody);
                HttpClient client = new HttpClient();
                HttpResponseMessage task = client.PostAsync(url, content).Result;
                string responseData;
                if (task.IsSuccessStatusCode)
                {
                    // Read and parse the response body as JSON
                    string responseBody = task.Content.ReadAsStringAsync().Result;
                    JObject responseObject = JObject.Parse(responseBody);

                    // Extract the content in the "response" field
                    responseData = (string)responseObject["response"];
                    //Console.WriteLine(responseData);
                }
                else
                {
                    responseData = "";
                    Console.WriteLine(task.StatusCode);
                }
                // save result to cache
                _cache[query][temperature] = new String[] { responseData };
            }
            // nothing to return
            if (!_cache.ContainsKey(query))
                return "";
            // verify whether need to constraint output if not explicitly given
            bool input = (forceInput == null) ? ConstrainOutput(background) :
                                                forceInput.GetValueOrDefault();
            // return input
            if (!input)
            {
                //Console.WriteLine(String.Join("\n", _cache[query][temperature]));
                return _cache[query][temperature][0].Trim();
            }
            else
            {
                //Console.WriteLine(String.Join("\n", _cache[query][temperature]));
                return (_cache[query][temperature].FirstOrDefault(
                    v => question.Contains(v.Trim(), StringComparison.OrdinalIgnoreCase)
                ) ?? "").Trim();
            }
        }

        /// <summary>
        /// Detect whether the output is a substring of the input.
        /// </summary>
        /// <param name="query">Query</param>
        /// <returns></returns>
        public static bool ConstrainOutput(Tuple<string, string>[] query)
        {
            return query.All(t => t.Item1.Contains(t.Item2));
        }

        /// <summary>
        /// Load data into static cache.
        /// </summary>
        /// <param name="file">Filename of json cache.</param>
        public static void LoadCache(string file, bool reset = false)
        {
            // reset cache
            if (reset)
                _cache = new Dictionary<string, Dictionary<double, string[]>>();
            // load from JSON file
            if (File.Exists(file))
            {
                Dictionary<string, Dictionary<double, string[]>> rawData =
                    JsonConvert.DeserializeObject<Dictionary<string, Dictionary<double, string[]>>>(
                        File.ReadAllText(file)
                    );
                // add to the cache
                foreach (var pair in rawData)
                    if (!_cache.ContainsKey(pair.Key))
                        _cache.Add(pair.Key, pair.Value);
            }
        }

        /// <summary>
        /// Save the cache.
        /// </summary>
        /// <param name="file"></param>
        public static void SaveCache(string file)
        {
            if (!Directory.GetParent(file).Exists)
                Directory.GetParent(file).Create();
            File.WriteAllText(file, JsonConvert.SerializeObject(_cache,
                                                                Formatting.Indented));
        }

    }

    public static class LLaMA3QueryRunner
    {

        internal static double temperature = 0.2;
        //internal static OpenAIAPI api = new OpenAIAPI(apiKeys: getAPI(), engine: "davinci-msft");
        //internal static OpenAIAPI api = new OpenAIAPI(apiKeys: getAPI(), engine: "davinci-002");
        //internal static OpenAIAPI api = new OpenAIAPI(apiKeys: getAPI());
        internal static Query builder = new LLaMAShortQuery();

        private static Dictionary<string, Dictionary<double, string[]>> _cache = new();


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
        public static string Run(Tuple<string, string>[] background,
                                 string question,
                                 bool? forceInput = null)
        {
            // build query
            string query = builder.Generate(background, question);
            query = "Only complete the given pattern, Do not generate additional text. The pattern is: " + query;
            //Console.WriteLine(query);
            // run if not in cache and have a key
            if ((!_cache.ContainsKey(query) ||
                 !_cache[query].ContainsKey(temperature)))
            {
                // ensure query in cache
                if (!_cache.ContainsKey(query))
                    _cache[query] = new Dictionary<double, string[]>(1);
                // check if temperature exists
                string url = getAPI() + "generate";

                //string requestBody = "{\"model\": \"llama2\",\r\n  \"prompt: " + query + "\",\r\n  \"stream\": false,\r\n  \"format\": \"json\",\r\n  \"options\": {\r\n    \"seed\": 101,\r\n    \"temperature\": 0.2\r\n  }\r\n}";
                string requestBody = @"{""model"": ""llama3:70b-text"",  ""prompt"": """ + query + @""",  ""stream"": false,  ""options"": {""seed"": 101,""temperature"": 0.2, ""stop"": [""\n"", ""Q:"", ""Question:""]}}";
                //Console.WriteLine(requestBody);
                HttpContent content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                //var requestBody = new
                //{
                //    model = "llama2",
                //    prompt = query,
                //    stream = false,
                //    format = "json", 
                //    options = new
                //    {
                //        seed = 101,
                //        temperature = 0.2
                //    }
                //};
                //HttpContent content = new JsonContent(requestBody);
                HttpClient client = new HttpClient();
                HttpResponseMessage task = client.PostAsync(url, content).Result;
                string responseData;
                if (task.IsSuccessStatusCode)
                {
                    // Read and parse the response body as JSON
                    string responseBody = task.Content.ReadAsStringAsync().Result;
                    JObject responseObject = JObject.Parse(responseBody);

                    // Extract the content in the "response" field
                    responseData = (string)responseObject["response"];
                    //Console.WriteLine(responseData);
                }
                else
                {
                    responseData = "";
                    Console.WriteLine(task.StatusCode);
                }
                // save result to cache
                _cache[query][temperature] = new String[] { responseData };
            }
            // nothing to return
            if (!_cache.ContainsKey(query))
                return "";
            // verify whether need to constraint output if not explicitly given
            bool input = (forceInput == null) ? ConstrainOutput(background) :
                                                forceInput.GetValueOrDefault();
            // return input
            if (!input)
            {
                //Console.WriteLine(String.Join("\n", _cache[query][temperature]));
                return _cache[query][temperature][0].Trim();
            }
            else
            {
                //Console.WriteLine(String.Join("\n", _cache[query][temperature]));
                return (_cache[query][temperature].FirstOrDefault(
                    v => question.Contains(v.Trim(), StringComparison.OrdinalIgnoreCase)
                ) ?? "").Trim();
            }
        }

        /// <summary>
        /// Detect whether the output is a substring of the input.
        /// </summary>
        /// <param name="query">Query</param>
        /// <returns></returns>
        public static bool ConstrainOutput(Tuple<string, string>[] query)
        {
            return query.All(t => t.Item1.Contains(t.Item2));
        }

        /// <summary>
        /// Load data into static cache.
        /// </summary>
        /// <param name="file">Filename of json cache.</param>
        public static void LoadCache(string file, bool reset = false)
        {
            // reset cache
            if (reset)
                _cache = new Dictionary<string, Dictionary<double, string[]>>();
            // load from JSON file
            if (File.Exists(file))
            {
                Dictionary<string, Dictionary<double, string[]>> rawData =
                    JsonConvert.DeserializeObject<Dictionary<string, Dictionary<double, string[]>>>(
                        File.ReadAllText(file)
                    );
                // add to the cache
                foreach (var pair in rawData)
                    if (!_cache.ContainsKey(pair.Key))
                        _cache.Add(pair.Key, pair.Value);
            }
        }

        /// <summary>
        /// Save the cache.
        /// </summary>
        /// <param name="file"></param>
        public static void SaveCache(string file)
        {
            if (!Directory.GetParent(file).Exists)
                Directory.GetParent(file).Create();
            File.WriteAllText(file, JsonConvert.SerializeObject(_cache,
                                                                Formatting.Indented));
        }

    }

    public static class OpenAIQueryRunner
    {

        internal static double temperature = 0.1;
        //internal static OpenAIAPI api = new OpenAIAPI(apiKeys: getAPI(), engine: "davinci-msft");
        //internal static OpenAIAPI api = new OpenAIAPI(apiKeys: getAPI(), engine: "davinci-002");
        internal static OpenAIAPI api = new OpenAIAPI(apiKeys: getAPI());
        internal static Query builder = new ShortQuery();

        private static Dictionary<string, Dictionary<double, string[]>> _cache = new();


        public static string getAPI()
        {
            string api;
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
        public static string Run(Tuple<string, string>[] background,
                                 string question,
                                 bool? forceInput = null)
        {
            // build query
            string query = builder.Generate(background, question);
            //Console.WriteLine(query);
            // run if not in cache and have a key
            if ((!_cache.ContainsKey(query) ||
                 !_cache[query].ContainsKey(temperature)) &&
                api.Auth != null)
            {
                // ensure query in cache
                if (!_cache.ContainsKey(query))
                    _cache[query] = new Dictionary<double, string[]>(1);
                #region GPT4
                //  if temperature exists
                var task = api.Chat.CreateChatCompletionAsync(
                    new ChatRequest()
                    {
                        //Model = Model.GPT4_Turbo,
                        Model = Model.GPT4_Turbo,
                        Temperature = temperature,
                        MaxTokens = 50,
                        Messages = new ChatMessage[] {
                             new ChatMessage(ChatMessageRole.System, "You only complete the given pattern without providing any extra tokens or repeating the given content."),
                             new ChatMessage(ChatMessageRole.User, query)
                        },
                        StopSequence = "Q"
                    }
                );
                _cache[query][temperature] = task.Result.Choices.Select(choice => choice.ToString()).ToArray();
                #endregion
                #region Davinci
                //var task = api.Completions.CreateCompletionAsync(
                //    query,
                //    temperature: temperature,
                //    max_tokens: 50,
                //    numOutputs: 25,
                //    stopSequences: new[] { "Q", "\n", "Question" },
                //    model: Model.Davinci
                //);
                //// save result to cache
                //_cache[query][temperature] = task.Result.Completions.Select(choice => choice.Text).ToArray();
                #endregion
            }
            // nothing to return
            if (!_cache.ContainsKey(query))
                return "";
            // verify whether need to constraint output if not explicitly given
            bool input = (forceInput == null) ? ConstrainOutput(background) :
                                                forceInput.GetValueOrDefault();
            // return input
            if (!input)
            {
                //Console.WriteLine(String.Join("\n", _cache[query][temperature]));
                return _cache[query][temperature][0].Trim();
            }
            else
            {
                //Console.WriteLine(String.Join("\n", _cache[query][temperature]));
                return (_cache[query][temperature].FirstOrDefault(
                    v => question.Contains(v.Trim(), StringComparison.OrdinalIgnoreCase)
                ) ?? "").Trim();
            }
        }

        /// <summary>
        /// Detect whether the output is a substring of the input.
        /// </summary>
        /// <param name="query">Query</param>
        /// <returns></returns>
        public static bool ConstrainOutput(Tuple<string, string>[] query)
        {
            return query.All(t => t.Item1.Contains(t.Item2));
        }

        /// <summary>
        /// Load data into static cache.
        /// </summary>
        /// <param name="file">Filename of json cache.</param>
        public static void LoadCache(string file, bool reset = false)
        {
            // reset cache
            if (reset)
                _cache = new Dictionary<string, Dictionary<double, string[]>>();
            // load from JSON file
            if (File.Exists(file))
            {
                Dictionary<string, Dictionary<double, string[]>> rawData =
                    JsonConvert.DeserializeObject<Dictionary<string, Dictionary<double, string[]>>>(
                        File.ReadAllText(file)
                    );
                // add to the cache
                foreach (var pair in rawData)
                    if (!_cache.ContainsKey(pair.Key))
                        _cache.Add(pair.Key, pair.Value);
            }
        }

        /// <summary>
        /// Save the cache.
        /// </summary>
        /// <param name="file"></param>
        public static void SaveCache(string file)
        {
            if (!Directory.GetParent(file).Exists)
                Directory.GetParent(file).Create();
            File.WriteAllText(file, JsonConvert.SerializeObject(_cache,
                                                                Formatting.Indented));
        }

    }


    /// <summary>
    /// Abstract query class.
    /// </summary>
    public abstract class Query
    {

        internal static string template = "Q: {0} A: {1} ";
        internal static string prompt = "Transformations: ";

        //public string Generate(Tuple<string, string>[] background, string question)
        //{
        //    var data = background.Append(Tuple.Create(question, ""));
        //    return prompt + "\n\n" +
        //           String.Join("\n\n",
        //                       data.Select(t => String.Format(template,
        //                                                      t.Item1, t.Item2)))
        //                 .TrimEnd();
        //}

        public string Generate(Tuple<string, string>[] background, string question)
        {
            var data = background.Append(Tuple.Create(question, ""));
            return prompt + "" +
                   String.Join(" ",
                               data.Select(t => String.Format(template,
                                                              t.Item1, t.Item2)))
                         .TrimEnd();
        }
    }

    /// <summary>
    /// Use short QA format.
    /// </summary>
    public class ShortQuery : Query
    {

    }

    /// <summary>
    /// Use long QA format.
    /// </summary>
    public class LongQuery : Query
    {
        internal static new string template = "Question: {0}\nAnswer: {1}";
        internal static new string prompt = "Transformations:";
    }

    /// <summary>
    /// Use arrow query.
    /// </summary>
    public class ArrowQuery : Query
    {
        internal static new string template = "{0} => {1}";
        internal static new string prompt = "Transformations:";
    }

    public class LLaMAShortQuery : Query
    {
        internal static new string template = "Q: {0} A: {1} ";
        internal static new string prompt = "Transformations:";
    }

    public class MistralShortQuery : Query
    {
        internal static new string template = "Q: {0} A: {1} ";
        internal static new string prompt = "Create a completion for the following pattern. Do not give additional answers:";
    }

}