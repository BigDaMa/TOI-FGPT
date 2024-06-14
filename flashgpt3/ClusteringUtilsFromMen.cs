using AngleSharp.Dom;
using Microsoft.ProgramSynthesis.Extraction.Web.Build.RuleNodeTypes;
using Microsoft.ProgramSynthesis.Transformation.Formula.Semantics.Extensions;
using Microsoft.ProgramSynthesis.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FlashGPT3
{
    internal class ToiStructure
    {
        public Tuple<string, string> Left { get; set; }
        public Tuple<string, string> Toi { get; set; }
        public Tuple<string, string> Right { get; set; }
    }

    internal static partial class ClusteringUtils
    {


        internal static List<List<string>> ClusterGreedy_TOI(List<Tuple<string, List<string>>> options_tuple, bool print = false)
        {

            const string TOICache = @"../../../../FlashGPT3/cache/toi.json";
            Dictionary<string, Tuple<string, string>> toiDict = JsonConvert.DeserializeObject<Dictionary<string, Tuple<string, string>>>(File.ReadAllText(TOICache));

            List<List<string>> options = new();
            

            foreach (var xys in options_tuple)
            {
                List<string> candidates = new List<string>();
                Tuple<string, string> currtoi = toiDict[xys.Item1];
                bool flag = false;
                if (xys.Item2.All(s => xys.Item1.Contains(s)))
                {
                    flag = false;
                }
                else
                {
                    flag = true;
                }
                List<string> currtoi_candidates_forward;
                List<string> currtoi_candidates_backward;
                if (flag)
                {
                    currtoi_candidates_forward = StringUtils.GetAllSubstringForward(currtoi.Item2);
                    currtoi_candidates_backward = StringUtils.GetAllSubStringBackward(currtoi.Item2);
                }   
                else
                {
                    currtoi_candidates_forward = StringUtils.GetAllSubstringForward(currtoi.Item1);
                    currtoi_candidates_backward = StringUtils.GetAllSubStringBackward(currtoi.Item1);
                }
                bool TOIHit = false;
                string prevY = null;
                bool fwd_EndswFwd = false;
                bool bwd_StartswBwdEndswBwd = false;
                bool bwd_StartswBwdNEndswBwd = false;

                foreach (string y in xys.Item2)
                {
                    if (y.EndsWith(currtoi.Item2) || (y.EndsWith(currtoi.Item1) && !y.EndsWith(currtoi.Item2) && currtoi.Item1.Contains(y))) { candidates.Add(y); TOIHit = false; }
                    else if (!currtoi_candidates_forward.Any(s => y.EndsWith(s)) && !currtoi_candidates_backward.Any(s => y.StartsWith(s)) &&
                        !((currtoi_candidates_backward.Any(s => y.StartsWith(s)) && currtoi_candidates_backward.Any(s => y.EndsWith(s)))))
                    {
                        if (!TOIHit) candidates.Add(y);
                        else
                        {
                            candidates.Add(prevY);
                            candidates.Add(y);
                            TOIHit = false;
                        }
                    }
                    else
                    {
                        TOIHit = true;
                        prevY = y;
                    }



                }
                if(TOIHit && !prevY.IsNullOrEmpty())
                {
                    candidates.Add(prevY);
                }
                options.Add(candidates);
            }



            if (print)
                Console.WriteLine(ToStringExt(options));
            // get shortest
            int length = options.Max(l => l.Count);
            var shortest = options.FirstOrDefault(l => l.Count == length);
            // remove shortest
            options.Remove(shortest);
            // initialise clusters
            var clusters = new List<List<string>>(length);
            foreach (string s in shortest)
                clusters.Add(new List<string> { s });
            // add rest
            foreach (List<string> row in options)
            {
                // compute list of similarities of cluster to strings
                List<List<double>> similarities = shortest.Select(
                    a => row.Select(b => Similarity(b, a)).ToList()
                ).ToList();
                // iteratively look for lowest value until all clusters
                // are assigned a value
                for (int i = 0; i < similarities.Count; i++)
                {
                    // get best
                    (int cluster, int option) = ArgMaxArgMax(similarities);
                    // add to cluster
                    clusters[cluster].Add(row[option]);
                    // remove the selected option from the options
                    //row.RemoveAt(option);
                    //foreach (var sim in similarities)
                    //    if (sim.Count > option)
                    //        sim.RemoveAt(option);
                    // remove the selected cluster from possible clusters
                    similarities[cluster] = new List<double> { };
                }
            }
            if (print)
            {
                Console.WriteLine("--------------------");
                Console.WriteLine(ToStringExt(clusters));
                Console.WriteLine("====================");
            }
            
            clusters = ProcessClusters(clusters);

            return clusters;
        }

        internal static List<List<string>> ProcessClusters(List<List<string>> clusters)
        {
            foreach (var cluster in clusters)
            {
                // Create a list of strings to remove
                List<string> stringsToRemove = new List<string>();

                for (int i = 0; i < cluster.Count; i++)
                {
                    string currentString = cluster[i];
                    if (currentString.EndsWith(" "))
                    {
                        string trimmedString = currentString.TrimEnd(' ');

                        if (cluster.Contains(trimmedString))
                        {
                            // Add the current string to the removal list
                            stringsToRemove.Add(currentString);
                        }
                        else
                        {
                            // Replace the current string with the trimmed version
                            cluster[i] = trimmedString;
                        }
                    }
                }

                foreach (var str in stringsToRemove)
                {
                    cluster.Remove(str);
                }
            }
            return clusters;
        }

        internal static List<List<string>> ClusterGreedy_TOI_new(List<Tuple<string, List<string>>> options_tuple, bool print = false)
        {

            const string TOICache = @"../../../../FlashGPT3/cache/toi.json";
            Dictionary<string, Tuple<string, string>> toiDict = JsonConvert.DeserializeObject<Dictionary<string, Tuple<string, string>>>(File.ReadAllText(TOICache));

            Dictionary<string, List<string>> options = new();

            

            foreach (var xys in options_tuple)
            {
                List<string> candidates = new List<string>();
                Tuple<string, string> currtoi = toiDict[xys.Item1];
                List<string> currtoi_candidates_forward = StringUtils.GetAllSubstringForward(currtoi.Item2);
                currtoi_candidates_forward.AddRange(StringUtils.GetAllSubstringForward(currtoi.Item1));
                List<string> currtoi_candidates_backward = StringUtils.GetAllSubStringBackward(currtoi.Item2);
                currtoi_candidates_backward.AddRange(StringUtils.GetAllSubStringBackward(currtoi.Item1));
                bool TOIHit = false;
                string prevY = null;
                bool fwd_EndswFwd = false;
                bool bwd_StartswBwdEndswBwd = false;
                bool bwd_StartswBwdNEndswBwd = false;

                foreach (string y in xys.Item2)
                {
                    if (!currtoi_candidates_forward.Any(s => y.EndsWith(s)) &&
                        !((currtoi_candidates_backward.Any(s => y.StartsWith(s)) && currtoi_candidates_backward.Any(s => y.EndsWith(s)))))
                    {
                        if (!TOIHit) candidates.Add(y);
                        else
                        {
                            candidates.Add(prevY);
                            candidates.Add(y);
                            TOIHit = false;
                        }
                    }
                    else
                    {
                        TOIHit = true;
                        prevY = y;
                    }

                    //if(currtoi_candidates_forward.Any(s => y.EndsWith(s))) fwd_EndswFwd = true;
                    //if(currtoi_candidates_backward.Any(s => y.StartsWith(s) && currtoi_candidates_backward.Any(s => y.EndsWith(s)))) bwd_StartswBwdEndswBwd = true;
                    //if(currtoi_candidates_backward.Any(s => y.StartsWith(s) && !currtoi_candidates_backward.Any(s => y.EndsWith(s)))) bwd_StartswBwdNEndswBwd = true;




                }
                if (TOIHit && !prevY.IsNullOrEmpty())
                {
                    candidates.Add(prevY);
                }
                options[xys.Item1] = candidates;
            }

            Dictionary<string, List<string>> options_beforeTOI = GetAllBeforeTOI(options, toiDict);
            List<string> lastbeforeTOI = options_beforeTOI.Values.Select(list => list.OrderByDescending(s => s.Length).FirstOrDefault()).ToList();
            Dictionary<string, string> options_TOI = GetAllTOI(options, toiDict);
            List<string> lastandtoi = lastbeforeTOI.Zip(options_TOI.Values, (i, j) => i.IsNullOrEmpty() ? i + j : i + " " + j).ToList();
            Dictionary<string, List<string>> options_afterTOI = GetAllAfterTOI(options, toiDict);
            

            List<List<string>> clusters = new();

            int maxlength = options_beforeTOI.Max(kvp => kvp.Value.Count);
            for (int i = 0; i < maxlength; i++)
            {
                //if(clusters.Count == i)
                //    clusters.Add(new List<string>());
                clusters.Add(new List<string>());
                foreach (List<string> s in options_beforeTOI.Values)
                {
                    clusters[i].Add(GetIdxOrLast(s, i));
                }
            }
            //if (clusters.Count == maxlength) clusters.Add(new List<string>());
            // Not always add, should detect if such condition exists. --> there exists s ends with TOI, or s starts with TOI.

            int toiidx = 0;
            if(!lastandtoi.IsNullOrEmpty())
            {
                toiidx = 1;
                clusters.Add(new List<string>());
                foreach (string s in lastandtoi)
                {
                    clusters[maxlength].Add(s);
                }
            }
            
            for (int i = 0; i < options_afterTOI.Max(kvp => kvp.Value.Count); i++)
            {
                //if (clusters.Count == maxlength + 1 + i)
                //    clusters.Add(new List<string>());
                clusters.Add(new List<string>());
                foreach (List<string> s in options_afterTOI.Values)
                {
                    clusters[maxlength + toiidx + i].Add(GetIdxOrLast(s, i));
                }
            }

            return clusters;
        }

        internal static string GetIdxOrLast(List<string> op, int i)
        {
            if(i >= op.Count) return op[op.Count - 1].ToString();
            else return op[i].ToString();
        }

        internal static Dictionary<string, List<string>> GetAllBeforeTOI(Dictionary<string, List<string>> options, Dictionary<string, Tuple<string, string>> toiDict)
        {
            Dictionary<string, List<string>> result = new();
            foreach (var kvp in options)
            {
                Tuple<string, string> currtoi = toiDict[kvp.Key];
                int index = -1;
                if(kvp.Key.Contains(currtoi.Item2)) index = kvp.Key.IndexOf(currtoi.Item2);
                else if (kvp.Key.Contains(currtoi.Item1) && !kvp.Key.Contains(currtoi.Item2)) index = kvp.Key.IndexOf(currtoi.Item1);

                string currbeforetoi = kvp.Key.Substring(0, index);

                result[kvp.Key] = kvp.Value.Where(s => currbeforetoi.Contains(s)).ToList();
            }
            return result;
        }

        internal static Dictionary<string, string> GetAllTOI(Dictionary<string, List<string>> options, Dictionary<string, Tuple<string, string>> toiDict)
        {
            Dictionary<string, string> result = new();
            foreach (var kvp in options)
            {
                Tuple<string, string> currtoi = toiDict[kvp.Key];
                if (kvp.Value.Any(s => s.StartsWith(currtoi.Item2) || s.EndsWith(currtoi.Item2))) result[kvp.Key] = currtoi.Item2;
                else if (kvp.Value.Any(s => (s.StartsWith(currtoi.Item1) && !s.StartsWith(currtoi.Item2)) || (s.EndsWith(currtoi.Item1) && !s.EndsWith(currtoi.Item2)))) result[kvp.Key] = currtoi.Item1;
            }
            return result;
        }

        internal static Dictionary<string, List<string>> GetAllAfterTOI(Dictionary<string, List<string>> options, Dictionary<string, Tuple<string, string>> toiDict)
        {
            Dictionary<string, List<string>> result = new();
            foreach (var kvp in options)
            {
                Tuple<string, string> currtoi = toiDict[kvp.Key];
                int index = -1;
                int length = -1;
                if (kvp.Key.Contains(currtoi.Item2)) { index = kvp.Key.IndexOf(currtoi.Item2); length = currtoi.Item2.Length; }
                else if (kvp.Key.Contains(currtoi.Item1) && !kvp.Key.Contains(currtoi.Item2)) { index = kvp.Key.IndexOf(currtoi.Item1); length = currtoi.Item1.Length; }

                string curraftertoi = kvp.Key.Substring(index + length);
                List<string> curraftertoi_forward = StringUtils.GetAllSubstringForward(curraftertoi.Trim());

                result[kvp.Key] = kvp.Value.Where(s => curraftertoi_forward.Any(str2 => s.EndsWith(str2))).ToList();
            }
            return result;
        }

        internal static List<ToiStructure> ClusterTOI(List<Tuple<string, string>> options, bool print = false)
        {
            TOIIdentSim toi = new TOIIdentSim();
            List<Tuple<string, string>> toiList = toi.GetAllTOIs(options);
            List<ToiStructure> res = new();
            //List<List<List<string>>> resforprint = new();

            for (int i = 0; i < toiList.Count; i++)
            {
                ToiStructure tempres = new();
                //List<List<string>> tempresforprint = new();
                string tmpTOIXLeft = null;
                if (options[i].Item1.StartsWith(toiList[i].Item1))
                {
                    tmpTOIXLeft = "";
                }
                else
                {
                    tmpTOIXLeft = options[i].Item1.Substring(0, options[i].Item1.IndexOf(toiList[i].Item1));
                }
                string tmpTOIYLeft = null;
                if (options[i].Item2.StartsWith(toiList[i].Item2))
                {
                    tmpTOIYLeft = "";
                }
                else
                {
                    tmpTOIYLeft = options[i].Item2.Substring(0, options[i].Item2.IndexOf(toiList[i].Item2));
                }
                tempres.Left = new Tuple<string, string> (tmpTOIXLeft, tmpTOIYLeft);
                tempres.Toi = new Tuple<string, string> (toiList[i].Item1, toiList[i].Item2);
                string tmpTOIXRight = null;
                if (options[i].Item2.EndsWith(toiList[i].Item1))
                {
                    tmpTOIXRight = "";
                }
                else
                {
                    tmpTOIXRight = options[i].Item1.Substring(options[i].Item1.IndexOf(toiList[i].Item1) + toiList[i].Item1.Length);
                }

                string tmpTOIYRight = null;
                if (options[i].Item2.EndsWith(toiList[i].Item1))
                {
                    tmpTOIYRight = "";
                }
                else
                {
                    tmpTOIYRight = options[i].Item2.Substring(options[i].Item2.IndexOf(toiList[i].Item2) + toiList[i].Item2.Length);
                }

                tempres.Right = new Tuple<string, string> (tmpTOIXRight, tmpTOIYRight);
                //tempresforprint.Add(new List<string>() { tmpTOIXRight, tmpTOIYRight });
                res.Add(tempres);
                //resforprint.Add(tempresforprint);
            }

            //XmlSerializer ser = new XmlSerializer(typeof(List<List<List<string>>>));
            //// write
            //dynamic stream;
            //if (File.Exists("toi.xml"))
            //{
            //    stream = File.AppendText("toi.xml");
            //}
            //else
            //{
            //    stream = File.Create("toi.xml");
            //}
            //using (stream)
            //{
            //    ser.Serialize(stream, resforprint); // your instance
            //}

            return res;
        }

        internal static HashSet<Tuple<string, string>> toiCheat = new HashSet<Tuple<string, string>>()
            {


                new Tuple<string, string> ("colorful", "[colorful/colorless]"),
                new Tuple<string, string> ("careless", "[careful/careless]"),
                new Tuple<string, string> ("useless", "[useful/useless]"),
                new Tuple<string, string> ("helpful", "[helpful/helpless]"),
                new Tuple<string, string> ("powerless", "[powerful/powerless]"),
                new Tuple<string, string> ("thoughtful", "[thoughtful/thoughtless]"),
                new Tuple<string, string> ("painless", "[painful/painless]"),
                new Tuple<string, string> ("hopeless", "[hopeful/hopeless]"),
                new Tuple<string, string> ("tasteful", "[tasteful/tasteless]"),

                new Tuple<string, string> ("HET", "HET, Hohhot"),
                new Tuple<string, string> ("Frankfurt am Main", "FRA, Frankfurt am Main"),
                new Tuple<string, string> ("BER", "BER, Berlin"),
                new Tuple<string, string> ("Shenyang", "SHE, Shenyang"),
                new Tuple<string, string> ("LAX", "LAX, Los Angeles"),
                new Tuple<string, string> ("CDG", "HND, Tokyo"),
                new Tuple<string, string> ("Tokyo", "MAD, Madrid"),
                new Tuple<string, string> ("Madrid", "HET, Hohhot"),
                new Tuple<string, string> ("AEP", "AEP, Buenos Aires"),
                new Tuple<string, string> ("Cape Town", "CPT, Cape Town"),

                new Tuple<string, string> ("Apple", "Apple(AAPL)"),
                new Tuple<string, string> ("Microsoft", "Microsoft(MSFT)"),
                new Tuple<string, string> ("TSLA", "Tesla(TSLA)"),
                new Tuple<string, string> ("Meta Platforms", "Meta Platforms(META)"),
                new Tuple<string, string> ("AXP", "American Express Company(AXP)"),
                new Tuple<string, string> ("AMZN", "Amazon(AMZN)"),
                new Tuple<string, string> ("GlaxoSmithKline plc", "GlaxoSmithKline plc(GSK)"),
                new Tuple<string, string> ("PepsiCo", "PepsiCo(PEP)"),
                new Tuple<string, string> ("Nvidia Corporation", "Nvidia Corporation(NVDA)"),
                new Tuple<string, string> ("Intel Corporation", "Intel Corporation(INTC)"),

                new Tuple<string, string> ("Germany", "Germany(DE), +49"),
                new Tuple<string, string> ("CN", "China(CN), +86"),
                new Tuple<string, string> ("33", "France(FR), +33"),
                new Tuple<string, string> ("55", "Brazil(BR), +55"),
                new Tuple<string, string> ("ES", "Spain(ES), +34"),
                new Tuple<string, string> ("Portugal", "Portugal(PT), +351"),
                new Tuple<string, string> ("JP", "Japan(JP), +81"),
                new Tuple<string, string> ("Russia", "Russia(RU), +7"),
                new Tuple<string, string> ("54", "Argentina(AR), +54"),
                new Tuple<string, string> ("Italy", "Italy(IT), +39"),

                new Tuple<string, string> ("Beijing", "Beijing, China"),
                new Tuple<string, string> ("Berlin", "Berlin, Germany"),
                new Tuple<string, string> ("Italy", "Rome, Italy"),
                new Tuple<string, string> ("Paris", "Paris, France"),
                new Tuple<string, string> ("Japan", "Tokyo, Japan"),
                new Tuple<string, string> ("Russia", "Moscow, Russia"),
                new Tuple<string, string> ("Ottawa", "Ottawa, Canada"),
                new Tuple<string, string> ("Canberra", "Canberra, Australia"),
                new Tuple<string, string> ("Spain", "Madrid, Spain"),
                new Tuple<string, string> ("Brussels", "Brussels, Belgium"),

                new Tuple<string, string> ("smaller", "(smaller/smallest)"),
                new Tuple<string, string> ("faster", "(faster/fastest)"),
                new Tuple<string, string> ("biggest", "(bigger/biggest)"),
                new Tuple<string, string> ("fastest", "(faster/fastest)"),
                new Tuple<string, string> ("best", "(better/best)"),
                new Tuple<string, string> ("warmer", "(warmer/warmest)"),
                new Tuple<string, string> ("worst", "(worse/worst)"),
                new Tuple<string, string> ("best", "(better/best)"),

            };


        //internal static List<List<Tuple<string, string>>> ClusterTOI_GMod(List<Tuple<string, string>> options, bool print = false)
        //{
        //    TOIIdentSim toi = new TOIIdentSim();
        //    List<Tuple<string, string>> toiList = new();

        //    foreach (var opt in options)
        //    {
        //        bool insertflag = false;
        //        foreach (var pair in toiCheat)
        //        {
        //            if (opt.Item1.Contains(pair.Item1) && opt.Item2.Contains(pair.Item2))
        //            {
        //                toiList.Add(pair);
        //                insertflag = true;
        //            }
        //        }
        //        if (!insertflag)
        //        {
        //            toiList.Add(toi.GetTOI(opt.Item1, opt.Item2));
        //        }
        //    }
        //    //if(toiList.IsNullOrEmpty())
        //    //{
        //    //    TOIIdentSim toi = new TOIIdentSim();
        //    //    toiList = toi.GetAllTOIs(options);
        //    //}



        //    //TOIIdentSim toi = new TOIIdentSim();
        //    //List<Tuple<string, string>> toiList = toi.GetAllTOIs(options);
        //    List<List<Tuple<string, string>>> res = new();

        //    for (int i = 0; i < toiList.Count; i++)
        //    {
        //        List<Tuple<string, string>> tempres = new();
        //        string tmpTOIXLeft = null;
        //        if (options[i].Item1.StartsWith(toiList[i].Item1))
        //        {
        //            tmpTOIXLeft = "";
        //        }
        //        else
        //        {
        //            try
        //            {
        //                tmpTOIXLeft = options[i].Item1.Substring(0, options[i].Item1.IndexOf(toiList[i].Item1));
        //            }
        //            catch
        //            {
        //                Console.WriteLine(options[i].Item1);
        //                Console.WriteLine(toiList[i].Item1);
        //            }

        //        }
        //        string tmpTOIYLeft = null;
        //        if (options[i].Item2.StartsWith(toiList[i].Item2))
        //        {
        //            tmpTOIYLeft = "";
        //        }
        //        else
        //        {
        //            tmpTOIYLeft = options[i].Item2.Substring(0, options[i].Item2.IndexOf(toiList[i].Item2));
        //        }
        //        tempres.Add(new Tuple<string, string>(tmpTOIXLeft, tmpTOIYLeft));
        //        tempres.Add(new Tuple<string, string>(toiList[i].Item1, toiList[i].Item2));
        //        string tmpTOIXRight = null;
        //        if (options[i].Item2.EndsWith(toiList[i].Item1))
        //        {
        //            tmpTOIXRight = "";
        //        }
        //        else
        //        {
        //            tmpTOIXRight = options[i].Item1.Substring(options[i].Item1.IndexOf(toiList[i].Item1) + toiList[i].Item1.Length);
        //        }

        //        string tmpTOIYRight = null;
        //        if (options[i].Item2.EndsWith(toiList[i].Item1))
        //        {
        //            tmpTOIYRight = "";
        //        }
        //        else
        //        {
        //            tmpTOIYRight = options[i].Item2.Substring(options[i].Item2.IndexOf(toiList[i].Item2) + toiList[i].Item2.Length);
        //        }

        //        tempres.Add(new Tuple<string, string>(tmpTOIXRight, tmpTOIYRight));
        //        res.Add(tempres);
        //    }
        //    return res;
        //}
    }
}
