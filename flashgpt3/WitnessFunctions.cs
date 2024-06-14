using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.Learning;
using Microsoft.ProgramSynthesis.Rules;
using Microsoft.ProgramSynthesis.Rules.Concepts;
using Microsoft.ProgramSynthesis.Specifications;
using Microsoft.ProgramSynthesis.Transformation.Formula.Build.RuleNodeTypes;
using Microsoft.ProgramSynthesis.Transformation.Formula.Semantics.Extensions;
using Microsoft.ProgramSynthesis.Transformation.Formula.Semantics.Learning.Models;
using Microsoft.ProgramSynthesis.Utils;
using Newtonsoft.Json;

namespace FlashGPT3
{
    public class WitnessFunctions : DomainLearningLogic
    {
        private static string ToStringExt<T>(List<T> list) => "[" + string.Join(", ", list) + "]";
        private static string ToStringExt<K, V>(KeyValuePair<K, V> kvp) => string.Format("[{0}] => {1}", kvp.Key, kvp.Value);
        public static string ToStringExt<K, V>(Dictionary<K, V> dic) => "{" + string.Join(", ", dic.Select((kvp) => ToStringExt(kvp))) + "}";

        internal List<string> GetValidSlices(string src, List<String> inputs)
        {
            const string toicache = @"../../../../FlashGPT3/cache/toi.json";
            Dictionary<string, Tuple<string, string>> toiDict = JsonConvert.DeserializeObject<Dictionary<string, Tuple<string, string>>>(File.ReadAllText(toicache));

            Tuple<string, string> toi = toiDict[src];
            List<string> currtoi_candidates_forward = StringUtils.GetAllSubstringForward(toi.Item2);
            List<string> currtoi_candidates_backward = StringUtils.GetAllSubStringBackward(toi.Item2);
            bool TOIHit = false;
            string prevY = null;

            List<string> strings = new List<string>();
            string firstinput = inputs[0];

            if (firstinput.EndsWith(toi.Item1) && !firstinput.Equals(toi.Item1))
            {
                int index = firstinput.IndexOf(toi.Item1);
                string stringbeforefirst = (index < 0)
                    ? firstinput
                    : firstinput.Remove(index, toi.Item1.Length);
                if (!currtoi_candidates_forward.Any(s => stringbeforefirst.StartsWith(s)))
                {
                    if (stringbeforefirst.EndsWith(" "))
                        strings.Add(stringbeforefirst.TrimEnd());
                    strings.Add(stringbeforefirst);
                }
                    
            }


            foreach (string input in inputs)
            {
                string currinput = input;
                if (currinput.Length == 0) { continue; }
                if (!currtoi_candidates_forward.Any(s => currinput.EndsWith(s)) &&
                    !((currtoi_candidates_backward.Any(s => currinput.StartsWith(s)) && currtoi_candidates_backward.Any(s => input.EndsWith(s)))))
                {
                    if (!TOIHit && !strings.Contains(currinput) && !currinput.IsNullOrEmpty()) strings.Add(currinput);
                    else
                    {
                        if (!strings.Contains(prevY) && !prevY.IsNullOrEmpty())
                        {
                            //if (prevY.EndsWith("  "))
                            //{
                            //    strings.Add(prevY.TrimEnd());
                            //    strings.Add(Regex.Replace(prevY, @"\s+", " "));
                            //}
                            //else if (prevY.StartsWith("  "))
                            //{
                            //    strings.Add(prevY.TrimStart());
                            //    strings.Add(Regex.Replace(prevY, @"\s+", " "));
                            //}
                            //else strings.Add(prevY);

                            //if (currinput.EndsWith("  "))
                            //{
                            //    strings.Add(currinput.TrimEnd());
                            //    strings.Add(Regex.Replace(prevY, @"\s+", " "));
                            //}
                            //else if (currinput.StartsWith("  "))
                            //{
                            //    strings.Add(currinput.TrimStart());
                            //    strings.Add(Regex.Replace(prevY, @"\s+", " "));
                            //}
                            //else strings.Add(currinput);
                            strings.Add(prevY);
                            strings.Add(currinput);
                        }
                        TOIHit = false;
                    }
                }
                else
                {
                    TOIHit = true;
                    prevY = currinput;
                }
            }
            if (TOIHit && !prevY.IsNullOrEmpty() && !strings.Contains(prevY))
            {
                if (prevY.EndsWith(" "))
                {
                    strings.Add(prevY.TrimEnd());
                    strings.Add(prevY);
                }
                else if (prevY.StartsWith(" "))
                {
                    strings.Add(prevY.TrimStart());
                    strings.Add(prevY);
                }
                else strings.Add(prevY);
            }
            return strings;

        }

        internal List<string> GetValidSlices_SemPos(string src, List<String> inputs)
        {
            const string toicache = @"../../../../FlashGPT3/cache/toi.json";
            Dictionary<string, Tuple<string, string>> toiDict = JsonConvert.DeserializeObject<Dictionary<string, Tuple<string, string>>>(File.ReadAllText(toicache));

            Tuple<string, string> toi = toiDict[src];
            List<string> currtoi_candidates_forward = StringUtils.GetAllSubstringForward(toi.Item2);
            List<string> currtoi_candidates_backward = StringUtils.GetAllSubStringBackward(toi.Item2);
            bool TOIHit = false;
            string prevY = null;

            List<string> strings = new List<string>();
            string firstinput = inputs[0];

            if (firstinput.EndsWith(toi.Item1) && !firstinput.Equals(toi.Item1))
            {
                int index = firstinput.IndexOf(toi.Item1);
                string stringbeforefirst = (index < 0)
                    ? firstinput
                    : firstinput.Remove(index, toi.Item1.Length);
                strings.Add(stringbeforefirst);
            }

            string directafterTOI = src.Substring(src.IndexOf(toi.Item1) + toi.Item1.Length);
            if (firstinput.Equals(directafterTOI))
            {
                strings.Add(toi.Item1 + firstinput);
            }


            foreach (string input in inputs)
            {
                string currinput = input;
                if (currinput.Length == 0) { continue; }
                if (!currtoi_candidates_forward.Any(s => currinput.EndsWith(s)) &&
                    !((currtoi_candidates_backward.Any(s => currinput.StartsWith(s)) && currtoi_candidates_backward.Any(s => input.EndsWith(s)))))
                {
                    if (!TOIHit && !strings.Contains(currinput) && !currinput.IsNullOrEmpty()) strings.Add(currinput);
                    else
                    {
                        if (!strings.Contains(prevY) && !prevY.IsNullOrEmpty())
                        {
                            strings.Add(prevY);
                            strings.Add(currinput);
                        }
                        TOIHit = false;
                    }
                }
                else
                {
                    TOIHit = true;
                    prevY = currinput;
                }
            }
            if (TOIHit && !prevY.IsNullOrEmpty() && !strings.Contains(prevY))
            {
                strings.Add(prevY);
            }
            return strings;

        }

        public static bool Clustering = true;

        public WitnessFunctions(Grammar grammar) : base(grammar) { }

        /// <summary>
        /// Witness for first string of concat.
        ///
        /// This version only returns the longest contiguous
        /// substrings of the input.
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="spec"></param>
        /// <returns></returns>
        [WitnessFunction(nameof(Semantics.Concat), 0)]
        internal DisjunctiveExamplesSpec WitnessStringOne(GrammarRule rule,
                                                          DisjunctiveExamplesSpec spec)
        {
            #region Men
            const string toicache = @"../../../../FlashGPT3/cache/toi.json";
            Dictionary<string, Tuple<string, string>> toiDict = JsonConvert.DeserializeObject<Dictionary<string, Tuple<string, string>>>(File.ReadAllText(toicache));
            if (spec.ProvidedInputs.All(input => (!toiDict[(string)input.Bindings.Select(p => p.Value).FirstOrDefault()].Item1.IsNullOrEmpty()) &&
                                                  !toiDict[(string)input.Bindings.Select(p => p.Value).FirstOrDefault()].Item2.IsNullOrEmpty()))
            {
                var ex = new Dictionary<State, IEnumerable<object>>();
                foreach (State input in spec.ProvidedInputs)
                {
                    string v = (string)input.Bindings.Select(p => p.Value)
                                                  .FirstOrDefault();
                    List<string> tempstrs = new List<string>();
                    foreach (string str in spec.DisjunctiveExamples[input])
                    {
                        tempstrs.Add(str);
                    }
                    tempstrs = GetValidSlices(v, tempstrs);
                    ex[input] = tempstrs;
                }
                spec = DisjunctiveExamplesSpec.From(ex);
            }

            #endregion

            var examples = new Dictionary<State, IEnumerable<object>>();
            foreach (State input in spec.ProvidedInputs)
            {
                var v = (string)input.Bindings.Select(p => p.Value)
                                              .FirstOrDefault();
                // gather the interesting strings
                var interesting = new List<object>();
                foreach (string output in spec.DisjunctiveExamples[input])
                    interesting.AddRange(RegexUtils.InterestingStrings(output));
                // get the ones that are contiguous substrings
                var contiguous = interesting.Where(s => v.Contains((string)s))
                                            .Cast<string>();
                // get longest
                var longest = contiguous.Where(
                    s => !contiguous.Any(c => (c.Contains(s) && !s.Equals(c)))
                );
                // get strings that are not substrings
                var others = interesting.Where(s => !v.Contains((string)s));
                examples[input] = longest.Concat(others).ToList();
            }
            return DisjunctiveExamplesSpec.From(examples);
        }


        [WitnessFunction(nameof(Semantics.Concat), 1, DependsOnParameters = new[] { 0 })]
        internal DisjunctiveExamplesSpec WitnessStringTwo(GrammarRule rule,
                                                          DisjunctiveExamplesSpec spec,
                                                          ExampleSpec stringBinding)
        {
            #region Men
            const string toicache = @"../../../../FlashGPT3/cache/toi.json";
            Dictionary<string, Tuple<string, string>> toiDict = JsonConvert.DeserializeObject<Dictionary<string, Tuple<string, string>>>(File.ReadAllText(toicache));
            if (spec.ProvidedInputs.All(input => (!toiDict[(string)input.Bindings.Select(p => p.Value).FirstOrDefault()].Item1.IsNullOrEmpty()) &&
                                                  !toiDict[(string)input.Bindings.Select(p => p.Value).FirstOrDefault()].Item2.IsNullOrEmpty()))
            {
                var ex = new Dictionary<State, IEnumerable<object>>();
                foreach (State input in spec.ProvidedInputs)
                {
                    string v = (string)(string)input.Bindings.Select(p => p.Value)
                                                  .FirstOrDefault();
                    List<string> tempstrs = new List<string>();
                    foreach (string str in spec.DisjunctiveExamples[input])
                    {
                        tempstrs.Add(str);
                    }
                    tempstrs = GetValidSlices(v, tempstrs);
                    ex[input] = tempstrs;
                }
                spec = DisjunctiveExamplesSpec.From(ex);
            }
            #endregion

            var examples = new Dictionary<State, IEnumerable<object>>();
            foreach (State input in spec.ProvidedInputs)
            {
                // first string of concat
                var rest = new List<object>();
                // go over all of the possible left strings
                string one = (string)stringBinding.Examples[input];
                // go over all of the outputs
                foreach (string output in spec.DisjunctiveExamples[input])
                {
                    // test if the left part matches the first part
                    // of this output and add if doesn't exist yet
                    if (!one.IsNullOrEmpty() && output.StartsWith(one))
                    {
                        string s = output.Substring(one.Length, output.Length - one.Length);
                        if (!rest.Contains(s))
                            rest.Add(s);
                    }
                }
                //}
                examples[input] = rest;
            }
            return DisjunctiveExamplesSpec.From(examples);
        }

        // Left position of SubStr
        [WitnessFunction(nameof(Semantics.SubStr), 1)]
        internal DisjunctiveExamplesSpec WitnessLeftPosition(GrammarRule rule,
                                                             DisjunctiveExamplesSpec spec)
        {
            #region Men
            const string toicache = @"../../../../FlashGPT3/cache/toi.json";
            Dictionary<string, Tuple<string, string>> toiDict = JsonConvert.DeserializeObject<Dictionary<string, Tuple<string, string>>>(File.ReadAllText(toicache));
            if (spec.ProvidedInputs.All(input => (!toiDict[(string)input.Bindings.Select(p => p.Value).FirstOrDefault()].Item1.IsNullOrEmpty()) &&
                                                  !toiDict[(string)input.Bindings.Select(p => p.Value).FirstOrDefault()].Item2.IsNullOrEmpty()))
            {
                var ex = new Dictionary<State, IEnumerable<object>>();
                foreach (State input in spec.ProvidedInputs)
                {
                    string v = (string)(string)input.Bindings.Select(p => p.Value)
                                                  .FirstOrDefault();
                    List<string> tempstrs = new List<string>();
                    foreach (string str in spec.DisjunctiveExamples[input])
                    {
                        tempstrs.Add(str);
                    }
                    tempstrs = GetValidSlices(v, tempstrs);
                    ex[input] = tempstrs;
                }
                spec = DisjunctiveExamplesSpec.From(ex);
            }
            #endregion

            Dictionary<State, List<string>> disjunctiveexamples = new();
            foreach (var item in spec.DisjunctiveExamples)
            {
                string src = (string)item.Key[rule.Body[0]];
                //List<string> validSlices = GetValidSlices(src, item.Value.ToList() as List<string>);
            }


            var lExamples = new Dictionary<State, IEnumerable<object>>();
            foreach (State input in spec.ProvidedInputs)
            {
                // input string
                var v = (string)input[rule.Body[0]];
                // get all occurrences of all disjunctive
                // examples in the input string
                var occurrences = new List<object>();
                foreach (string output in spec.DisjunctiveExamples[input])
                {
                    if (String.IsNullOrEmpty(output)) continue;
                    foreach (int occurence in v.AllIndexesOf(output))
                        occurrences.Add(occurence);
                }

                lExamples[input] = occurrences.Distinct().ToList();
            }
            return DisjunctiveExamplesSpec.From(lExamples);
        }

        // Right position of SubStr
        [WitnessFunction(nameof(Semantics.SubStr), 2, DependsOnParameters = new[] { 1 })]
        internal DisjunctiveExamplesSpec WitnessRightPosition(GrammarRule rule,
                                                              DisjunctiveExamplesSpec spec,
                                                              ExampleSpec leftBinding)
        {
            #region Men
            const string toicache = @"../../../../FlashGPT3/cache/toi.json";
            Dictionary<string, Tuple<string, string>> toiDict = JsonConvert.DeserializeObject<Dictionary<string, Tuple<string, string>>>(File.ReadAllText(toicache));
            if (spec.ProvidedInputs.All(input => (!toiDict[(string)input.Bindings.Select(p => p.Value).FirstOrDefault()].Item1.IsNullOrEmpty()) &&
                                                  !toiDict[(string)input.Bindings.Select(p => p.Value).FirstOrDefault()].Item2.IsNullOrEmpty()))
            {
                var ex = new Dictionary<State, IEnumerable<object>>();
                foreach (State input in spec.ProvidedInputs)
                {
                    string v = (string)input.Bindings.Select(p => p.Value)
                                                  .FirstOrDefault();
                    List<string> tempstrs = new List<string>();
                    foreach (string str in spec.DisjunctiveExamples[input])
                    {
                        tempstrs.Add(str);
                    }
                    tempstrs = GetValidSlices(v, tempstrs);
                    ex[input] = tempstrs;
                }
                spec = DisjunctiveExamplesSpec.From(ex);
            }
            #endregion

            var rExamples = new Dictionary<State, IEnumerable<object>>();
            foreach (State input in spec.ProvidedInputs)
            {
                // get left index
                int left = (int)leftBinding.Examples[input];
                // get the string
                var v = (string)input[rule.Body[0]];
                // given the left and the string, compute the
                // right part
                var occurences = new List<object>();
                foreach (string output in spec.DisjunctiveExamples[input])
                {
                    // compute right position
                    int p = left + output.Length;
                    // check if within  bounds
                    if (left + output.Length <= v.Length)
                    {
                        // check if output is found in v at the left position
                        int match = v.IndexOf(output, left, output.Length);
                        if (match == left && !occurences.Contains(p))
                            occurences.Add(p);
                    }
                }
                rExamples[input] = occurences.Distinct().ToList();
            }
            return DisjunctiveExamplesSpec.From(rExamples);
        }

        // Constant for ConstStr is just the constant itself.
        [WitnessFunction(nameof(Semantics.ConstStr), 0)]
        internal DisjunctiveExamplesSpec WitnessConstant(GrammarRule rule,
                                                         DisjunctiveExamplesSpec spec)
        {
            #region Men
            const string toicache = @"../../../../FlashGPT3/cache/toi.json";
            Dictionary<string, Tuple<string, string>> toiDict = JsonConvert.DeserializeObject<Dictionary<string, Tuple<string, string>>>(File.ReadAllText(toicache));
            if (spec.ProvidedInputs.All(input => (!toiDict[(string)input.Bindings.Select(p => p.Value).FirstOrDefault()].Item1.IsNullOrEmpty()) &&
                                                  !toiDict[(string)input.Bindings.Select(p => p.Value).FirstOrDefault()].Item2.IsNullOrEmpty()))
            {
                var ex = new Dictionary<State, IEnumerable<object>>();
                foreach (State input in spec.ProvidedInputs)
                {
                    string v = (string)(string)input.Bindings.Select(p => p.Value)
                                                  .FirstOrDefault();
                    List<string> tempstrs = new List<string>();
                    foreach (string str in spec.DisjunctiveExamples[input])
                    {
                        tempstrs.Add(str);
                    }
                    tempstrs = GetValidSlices(v, tempstrs);
                    ex[input] = tempstrs;
                }
                spec = DisjunctiveExamplesSpec.From(ex);
            }
            #endregion

            // get all values
            var examples = new Dictionary<State, IEnumerable<object>>();
            foreach (State input in spec.ProvidedInputs)
                examples[input] = spec.DisjunctiveExamples[input];
            // itersect
            return DisjunctiveExamplesSpec.From(LearningUtils.Intersect(examples));
        }


        [WitnessFunction(nameof(Semantics.AbsPos), 1)]
        internal DisjunctiveExamplesSpec WitnessK(GrammarRule rule, DisjunctiveExamplesSpec spec)
        {
            // collect all positions
            var examples = new Dictionary<State, IEnumerable<object>>();
            foreach (State input in spec.ProvidedInputs)
            {
                var v = (string)input[rule.Body[0]];
                var p = new List<object>();
                foreach (int pos in spec.DisjunctiveExamples[input])
                {
                    p.Add(pos);
                    if (pos > 0 & pos <= v.Length)
                        p.Add(pos - v.Length - 1);
                }
                examples[input] = p;
            }
            return DisjunctiveExamplesSpec.From(LearningUtils.Intersect(examples));
        }

        // Left match
        [WitnessFunction(nameof(Semantics.RegPos), 1)]
        DisjunctiveExamplesSpec WitnessLeftRegex(GrammarRule rule, DisjunctiveExamplesSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();
            foreach (var example in spec.DisjunctiveExamples)
            {
                State inputState = example.Key;
                var x = (string)inputState[rule.Body[0]];
                // Get all positions, are cached anyway.
                RegexUtils.BuildStringMatches(x,
                                              out List<Tuple<System.Text.RegularExpressions.Match, Regex>>[] leftMatches,
                                              out List<Tuple<System.Text.RegularExpressions.Match, Regex>>[] rightMatches);
                var regexes = new List<Regex>();
                foreach (int? pos in example.Value)
                    regexes.AddRange(leftMatches[pos.Value].Select(t => t.Item2));
                if (regexes.Count == 0)
                    return null;
                result[inputState] = regexes;
            }
            return new DisjunctiveExamplesSpec(LearningUtils.Intersect(result));
        }

        // Right match, independent of the other one
        [WitnessFunction(nameof(Semantics.RegPos), 2)]
        static DisjunctiveExamplesSpec WitnessRightRegex(GrammarRule rule, DisjunctiveExamplesSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();
            foreach (var example in spec.DisjunctiveExamples)
            {
                State inputState = example.Key;
                var x = (string)inputState[rule.Body[0]];
                // Get all positions, are cached anyway.
                RegexUtils.BuildStringMatches(x,
                                              out List<Tuple<System.Text.RegularExpressions.Match, Regex>>[] leftMatches,
                                              out List<Tuple<System.Text.RegularExpressions.Match, Regex>>[] rightMatches);
                var regexes = new List<Regex>();
                foreach (int? pos in example.Value)
                    regexes.AddRange(rightMatches[pos.Value].Select(t => t.Item2));
                result[inputState] = regexes;
            }
            return new DisjunctiveExamplesSpec(LearningUtils.Intersect(result));
        }

        // Copied from PROSE website.
        [WitnessFunction(nameof(Semantics.RegPos), 3, DependsOnParameters = new[] { 1, 2 })]
        DisjunctiveExamplesSpec WitnessKForRegexPair(GrammarRule rule, DisjunctiveExamplesSpec spec,
                                                     ExampleSpec lSpec, ExampleSpec rSpec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();
            foreach (var example in spec.DisjunctiveExamples)
            {
                State inputState = example.Key;
                var left = (Regex)lSpec.Examples[inputState];
                var right = (Regex)rSpec.Examples[inputState];
                if (left.ToString() == "" && right.ToString() == "")
                {
                    result[inputState] = new List<object>();
                }
                else
                {
                    var x = (string)inputState[rule.Body[0]];
                    var rightMatches = right.Matches(x).Cast<System.Text.RegularExpressions.Match>().ToDictionary(m => m.Index);
                    var matchPositions = new List<int>();
                    foreach (System.Text.RegularExpressions.Match m in left.Matches(x))
                    {
                        if (rightMatches.ContainsKey(m.Index + m.Length))
                            matchPositions.Add(m.Index + m.Length);
                    }
                    var ks = new HashSet<int?>();
                    foreach (int? pos in example.Value)
                    {
                        int occurrence = matchPositions.BinarySearch(pos.Value);
                        if (occurrence < 0) continue;
                        ks.Add(occurrence);
                        ks.Add(occurrence - matchPositions.Count);
                    }
                    if (ks.Count == 0)
                        return null;
                    result[inputState] = ks.Cast<object>().ToList();
                }
            }
            return new DisjunctiveExamplesSpec(LearningUtils.Intersect(result));
        }

        //[WitnessFunction(nameof(Semantics.SemPos), 2)]
        //internal DisjunctiveExamplesSpec WitnessDirection(GrammarRule rule, DisjunctiveExamplesSpec spec)
        //{
        //    var kExamples = new Dictionary<State, IEnumerable<object>>();
        //    foreach (var example in spec.DisjunctiveExamples)
        //    {
        //        State inputState = example.Key;
        //        int n = ((string)inputState[rule.Body[0]]).Length;
        //        var ks = new List<object>();
        //        if (example.Value.Any(p => (int)p > 0)) ks.Add("L");
        //        if (example.Value.Any(p => (int)p < n)) ks.Add("R");
        //        kExamples[inputState] = ks;
        //    }
        //    return DisjunctiveExamplesSpec.From(kExamples);
        //}
        public static string[] DirGen = { "R", "L" };

        [WitnessFunction(nameof(Semantics.SemPos), 1, DependsOnParameters = new[] { 2 })]
        internal DisjunctiveExamplesSpec WitnessPosQuery(GrammarRule rule, DisjunctiveExamplesSpec spec,
                                                         ExampleSpec directionBinding)
        {

            const string TOICache = @"../../../../FlashGPT3/cache/toi.json";
            Dictionary<string, Tuple<string, string>> toiDict = JsonConvert.DeserializeObject<Dictionary<string, Tuple<string, string>>>(File.ReadAllText(TOICache));
            bool flag = true;
            // If direction not consistent, return nothing,
            // else get the direction.
            var directions = directionBinding.Examples.Values.Distinct();
            //Console.WriteLine(directionBinding);
            //foreach ( var d in directions )
            //{
            //    Console.Write(d);
            //    Console.Write(" // ");
            //}
            //Console.WriteLine("=========");

            // A bird is smaller than a dog.

            if (directions.Count() != 1)
            {
                // Queries for each state are the same.
                var eExamples = new Dictionary<State, IEnumerable<object>>();
                foreach (var example in spec.DisjunctiveExamples)
                    eExamples[example.Key] = new List<object>();
                return DisjunctiveExamplesSpec.From(eExamples);
            }

            string direction = (string)directions.First();


            // First, we gather all the sides.
            var sides = new Dictionary<string, List<string>>();
            List<string> srcs = new();
            foreach (var example in spec.DisjunctiveExamples)
            {
                State inputState = example.Key;
                string inputString = (string)inputState[rule.Body[0]];
                srcs.Add(inputString);

                #region MS
                //if (direction == "L") sides[inputString] = example.Value.Select(p => inputString.Substring(Convert.ToInt32(p))).ToList();
                //if (direction == "R") sides[inputString] = example.Value.Select(p => inputString.Substring(0, Convert.ToInt32(p))).ToList();
                #endregion

                #region Men

                if (spec.ProvidedInputs.All(input => (!toiDict[(string)input.Bindings.Select(p => p.Value).FirstOrDefault()].Item1.IsNullOrEmpty()) &&
                                                      !toiDict[(string)input.Bindings.Select(p => p.Value).FirstOrDefault()].Item2.IsNullOrEmpty()))
                {

                    if (direction == "L")
                    {
                        List<string> tempside = example.Value.Select(p => inputString.Substring(Convert.ToInt32(p))).ToList();
                        for (int i = 0; i < tempside.Count; i++)
                        {
                            string s = tempside[i];
                            Tuple<string, string> currtoi = toiDict[inputString];
                            if (inputString.Contains(currtoi.Item1) && !inputString.Contains(currtoi.Item2) && s.Equals(inputString.Substring(inputString.IndexOf(currtoi.Item1) + currtoi.Item1.Length)))
                            {
                                s = currtoi.Item1 + " " + s;
                                tempside[i] = s;
                            }
                        }
                        sides[inputString] = tempside;
                    }
                    if (direction == "R")
                    {
                        List<string> tempside = example.Value.Select(p => inputString.Substring(0, Convert.ToInt32(p))).ToList();
                        for (int i = 0; i < tempside.Count; i++)
                        {
                            string s = tempside[i];
                            Tuple<string, string> currtoi = toiDict[inputString];
                            if (inputString.Contains(currtoi.Item2) && s.Equals(inputString.Substring(0, inputString.IndexOf(currtoi.Item2) + currtoi.Item2.Length)))
                            {
                                s = s + " " + currtoi.Item2;
                                tempside[i] = s;
                            }
                        }
                        sides[inputString] = tempside;
                    }
                }
                else
                {
                    flag = false;
                    if (direction == "L") sides[inputString] = example.Value.Select(p => inputString.Substring(Convert.ToInt32(p))).ToList();
                    if (direction == "R") sides[inputString] = example.Value.Select(p => inputString.Substring(0, Convert.ToInt32(p))).ToList();
                }
                #endregion
            }
            //foreach (string K in sides.Keys)
            //{
            //    Console.WriteLine(K);
            //    foreach (string V in sides[K]) Console.WriteLine(V);
            //    Console.WriteLine("---------");
            //}

            // Get interesting positions.
            // The 
            // The rainbow
            // The rainbow builds
            // The rainbow builds a
            // The rainbow builds a colorful
            // The rainbow boilds a colorful picture
            var positions = new Dictionary<string, List<string>>();
            foreach (var side in sides)
            {
                Tuple<string, string> currtoi = toiDict[side.Key];
                #region MS
                positions[side.Key] = side.Value.Select(s => RegexUtils.DirectedInterestingStrings(s, direction)
                                                                       .OrderBy(x => x.Length))
                                                .SelectMany(x => x)
                                                .ToList();
                #endregion

                #region Men
                //positions[side.Key] = side.Value.Select(s => RegexUtils.DirectedInterestingStrings(s, direction, currtoi)
                //                                                       .OrderBy(x => x.Length))
                //                                .SelectMany(x => x)
                //                                .ToList();
                #endregion
            }
            //foreach (string K in positions.Keys)
            //{
            //    Console.WriteLine(K);
            //    foreach (string V in positions[K]) Console.WriteLine(V);
            //    Console.WriteLine("=========");
            //}

            // some position no
            if (positions.Values.Any(p => p.Count == 0))
                return null;

            #region from Men_TOI
            //List<Tuple<string, string>[]> queries = new();
            //List<List<string>> clusters;
            ////List<List<string>> tempmaps = positions.Values.Select(x => x.ToList()).ToList();
            //List<Tuple<string, List<string>>> tempmaps = positions.Select(kvp => new Tuple<string, List<string>>(kvp.Key, kvp.Value.ToList())).ToList();
            //if (Clustering)
            //    clusters = ClusteringUtils.ClusterGreedy_TOI(tempmaps);
            //else
            //    clusters = ClusteringUtils.ClusterAll(positions.Values.Select(x => x.ToList()).ToList());

            //// convert back to queries
            ////List<object> queries = new(clusters.Count);
            //foreach (List<string> cluster in clusters)
            //    queries.Add(ClusteringUtils.BuildQuery(cluster, positions));
            #endregion

            #region from ms
            // cluster copy
            List<List<string>> clusters;


            List<Tuple<string, List<string>>> temppositions = positions.Select(kvp => new Tuple<string, List<string>>(kvp.Key, kvp.Value.ToList())).ToList();

            if (Clustering)
                if (flag) clusters = ClusteringUtils.ClusterGreedy_TOI(temppositions);
                else clusters = ClusteringUtils.ClusterGreedy(positions.Values.Select(x => x.ToList()).ToList());

            //clusters = ClusteringUtils.ClusterGreedy(positions.Values.Select(x => x.ToList()).ToList());
            else
                clusters = ClusteringUtils.ClusterAll(positions.Values.Select(x => x.ToList()).ToList());

            //foreach (var cluster in clusters)
            //{
            //    foreach (var s in cluster)
            //        Console.WriteLine(s);
            //    Console.WriteLine("-----");
            //}
            //Console.WriteLine("========");



            List<object> queries = new(clusters.Count);
            foreach (List<string> cluster in clusters)
                queries.Add(ClusteringUtils.BuildQuery(cluster, positions));

            #endregion

            #region from men
            //List<Tuple<string, string>> clusterpair = new();
            //foreach (string k in positions.Keys)
            //{
            //    string Y = positions[k].OrderByDescending(s => s.Length).FirstOrDefault();
            //    Tuple<string, string> pair = new Tuple<string, string>(k, Y);
            //    clusterpair.Add(pair);
            //}

            //dynamic clusters;
            //if (Clustering)
            //    clusters = ClusteringUtils.ClusterTOI(clusterpair); //List<ToiStructure>
            ////clusters = ClusteringUtils.ClusterTOI_GMod(clusterpair);
            //else
            //    clusters = ClusteringUtils.ClusterAll(positions.Values.Select(x => x.ToList()).ToList());

            //List<Tuple<string, string>[]> queries = new();
            //if (Clustering)
            //{
            //    // create queries based on new ds
            //    List<Tuple<string, string>> q = new();
            //    int i = 0;
            //    foreach (ToiStructure pairList in clusters)
            //    {
            //        string src = srcs[i];
            //        q.Add(new Tuple<string, string>(src, pairList.Toi.Item1));
            //        i++;
            //    }
            //    queries.Add(q.ToArray());
            //    //foreach (Tuple<string, string>[] qpairarr in queries)
            //    //{
            //    //    foreach (var qpair in qpairarr)
            //    //    {
            //    //        Console.WriteLine($"{qpair.Item1}, {qpair.Item2}");
            //    //    }
            //    //}
            //}
            //else
            //{
            //    foreach (List<string> cluster in clusters)
            //    {
            //        //Console.WriteLine(ToStringExt(cluster));
            //        //Console.WriteLine("----");
            //        Tuple<string, string>[] q = ClusteringUtils.BuildQuery(cluster, positions);
            //        //foreach (var i in q)
            //        //{
            //        //    Console.WriteLine(i.Item1);
            //        //    Console.WriteLine(i.Item2);
            //        //    Console.WriteLine("====");
            //        //}
            //        queries.Add(q);
            //    }
            //}
            #endregion

            // Queries for each state are the same.
            var kExamples = new Dictionary<State, IEnumerable<object>>();
            foreach (State state in spec.DisjunctiveExamples.Keys)
                kExamples[state] = queries;

            return DisjunctiveExamplesSpec.From(kExamples);
        }

        [WitnessFunction(nameof(Semantics.SemMap), 1)]
        internal DisjunctiveExamplesSpec WitnessMapQuery(GrammarRule rule,
                                                         DisjunctiveExamplesSpec spec)
        {

            // convert into mapping from state to
            // list of strings
            Dictionary<string, List<string>> maps = spec.DisjunctiveExamples.ToDictionary(
                item => (string)item.Key[rule.Body[0]],
                item => item.Value.Cast<string>().ToList()
            );

            //input: A bird is smaller than a dog.
            //calling: A bird is smaller than a dog.
            //          [A, A bird, A bird is, ...]
            // K: A bird is smaller than a dog.
            // [A bird is smaller than a dog.]
            // ====----====
            // K: A bird is smaller than a dog.
            // [A, A bird, A bird is, ...]
            // ====----====

            //foreach (KeyValuePair<string, List<string>> kvp in maps)
            //{
            //    Console.WriteLine("K: " + kvp.Key);
            //    Console.WriteLine(ToStringExt(kvp.Value));
            //}
            //Console.WriteLine("====----====----====");

            #region from men
            //List<Tuple<string, string>> clusterpair = new();
            //foreach (string k in maps.Keys)
            //{
            //    string Y = maps[k].OrderByDescending(s => s.Length).FirstOrDefault();
            //    Tuple<string, string> pair = new Tuple<string, string>(k, Y);
            //    clusterpair.Add(pair);
            //}

            //dynamic clusters;
            //if (Clustering)
            //    clusters = ClusteringUtils.ClusterTOI(clusterpair);
            ////clusters = ClusteringUtils.ClusterTOI_GMod(clusterpair);
            //else
            //    clusters = ClusteringUtils.ClusterAll(maps.Values.Select(x => x.ToList()).ToList());

            //List<Tuple<string, string>[]> queries = new();
            //if (Clustering)
            //{
            //    // create queries based on new ds
            //    List<Tuple<string, string>> q = new();
            //    foreach (ToiStructure pairList in clusters)
            //    {
            //        q.Add(pairList.Toi);
            //    }
            //    queries.Add(q.ToArray());
            //}
            //else
            //{
            //    foreach (List<string> cluster in clusters)
            //    {
            //        //Console.WriteLine(ToStringExt(cluster));
            //        //Console.WriteLine("----");
            //        Tuple<string, string>[] q = ClusteringUtils.BuildQuery(cluster, maps);
            //        //foreach (var i in q)
            //        //{
            //        //    Console.WriteLine(i.Item1);
            //        //    Console.WriteLine(i.Item2);
            //        //    Console.WriteLine("====");
            //        //}
            //        queries.Add(q);
            //    }
            //}
            #endregion

            #region from Men_TOI
            List<Tuple<string, string>[]> queries = new();
            List<List<string>> clusters;
            //List<List<string>> tempmaps = maps.Values.Select(x => x.ToList()).ToList();
            List<Tuple<string, List<string>>> tempmaps = maps.Select(kvp => new Tuple<string, List<string>>(kvp.Key, kvp.Value.ToList())).ToList();
            const string TOICache = @"../../../../FlashGPT3/cache/toi.json";
            Dictionary<string, Tuple<string, string>> toiDict = JsonConvert.DeserializeObject<Dictionary<string, Tuple<string, string>>>(File.ReadAllText(TOICache));
            if (Clustering)
            {
                bool greedyortoi = spec.ProvidedInputs.All(input => (!toiDict[(string)input.Bindings.Select(p => p.Value).FirstOrDefault()].Item1.IsNullOrEmpty()) &&
                                                      !toiDict[(string)input.Bindings.Select(p => p.Value).FirstOrDefault()].Item2.IsNullOrEmpty());
                if (greedyortoi)
                    clusters = ClusteringUtils.ClusterGreedy_TOI(tempmaps);
                else
                    clusters = ClusteringUtils.ClusterGreedy(maps.Values.Select(x => x.ToList()).ToList());
            }
                
            else
                clusters = ClusteringUtils.ClusterAll(maps.Values.Select(x => x.ToList()).ToList());

            // convert back to queries
            //List<object> queries = new(clusters.Count);
            foreach (List<string> cluster in clusters)
                queries.Add(ClusteringUtils.BuildQuery(cluster, maps));
            #endregion


            //queries.Add(tempquery);
            //queries.Add(new() { (toisrc, toitgt) });

            //skip clustering

            // run clustering on (deep) copy of the list

            #region from MS
            //List<Tuple<string, string>[]> queries = new();
            //List<List<string>> clusters;
            //if (Clustering)
            //    clusters = ClusteringUtils.ClusterGreedy(maps.Values.Select(x => x.ToList()).ToList());
            //else
            //    clusters = ClusteringUtils.ClusterAll(maps.Values.Select(x => x.ToList()).ToList());

            //// convert back to queries
            ////List<object> queries = new(clusters.Count);
            //foreach (List<string> cluster in clusters)
            //    queries.Add(ClusteringUtils.BuildQuery(cluster, maps));
            #endregion

            //Queries for each state are the same.
            var kExamples = new Dictionary<State, IEnumerable<object>>();
            foreach (State state in spec.DisjunctiveExamples.Keys)
                kExamples[state] = queries;

            // check where is different in queries
            DisjunctiveExamplesSpec res = DisjunctiveExamplesSpec.From(kExamples);

            //foreach (KeyValuePair<string, List<string>> kvp in tempmaps)
            //{
            //    Console.WriteLine("K: " + kvp.Key);
            //    Console.WriteLine(ToStringExt(kvp.Value));
            //}
            //Console.WriteLine(res.ToString());

            //return DisjunctiveExamplesSpec.From(kExamples);
            return res;
        }

        //[WitnessFunction(nameof(Semantics.SemMap), 1)]
        //internal DisjunctiveExamplesSpec WitnessMapQuery(GrammarRule rule,
        //                                                 DisjunctiveExamplesSpec spec)
        //{

        //    // convert into mapping from state to
        //    // list of strings
        //    Dictionary<string, List<string>> maps = spec.DisjunctiveExamples.ToDictionary(
        //        item => (string)item.Key[rule.Body[0]],
        //        item => item.Value.Cast<string>().ToList()
        //    );

        //    foreach (KeyValuePair<string, List<string>> kvp in maps)
        //    {
        //        Console.WriteLine("K: " + kvp.Key);
        //        Console.WriteLine(ToStringExt(kvp.Value));
        //    }
        //    Console.WriteLine("----");


        //    // run clustering on (deep) copy of the list
        //    List<List<string>> clusters;
        //    if (Clustering)
        //        clusters = ClusteringUtils.ClusterGreedy(maps.Values.Select(x => x.ToList()).ToList(), false);
        //    else
        //        clusters = ClusteringUtils.ClusterAll(maps.Values.Select(x => x.ToList()).ToList());

        //    // convert back to queries
        //    List<object> queries = new(clusters.Count);
        //    foreach (List<string> cluster in clusters)
        //    {
        //        //Console.WriteLine(ToStringExt(cluster));
        //        //Console.WriteLine("----");
        //        Tuple<string, string>[] q = ClusteringUtils.BuildQuery(cluster, maps);
        //        //foreach (var i in q)
        //        //{
        //        //    Console.WriteLine(i.Item1);
        //        //    Console.WriteLine(i.Item2);
        //        //    Console.WriteLine("====");
        //        //}
        //        queries.Add(q);
        //    }


        //    //Get interesting positions.
        //    //var positions = new Dictionary<State, List<string>>();
        //    //foreach (var side in sides)
        //    //     positions[side.Key] = side.Value.Select(s => RegexUtils.DirectedInterestingStrings(s, direction))
        //    //                                    .SelectMany(x => x).ToList();

        //    //Generate a list of combinations of interesting positions
        //    // and turn it into
        //    //var combinations = CollectionUtils.CartesianProduct(
        //    //    maps.Select(p => p.Value.Select(i => Tuple.Create(p.Key, i)))
        //    //);

        //    // Convert the positions back into queries.
        //    //var queries = combinations.Select(
        //    //    c => c.Select(p => Tuple.Create(((string)p.Item1[rule.Body[0]]), p.Item2))
        //    //          .ToArray()
        //    //);

        //    //Queries for each state are the same.
        //    var kExamples = new Dictionary<State, IEnumerable<object>>();
        //    foreach (State state in spec.DisjunctiveExamples.Keys)
        //        kExamples[state] = queries;

        //    DisjunctiveExamplesSpec res = DisjunctiveExamplesSpec.From(kExamples);

        //    //Dictionary<string, List<string>> tempmaps = res.DisjunctiveExamples.ToDictionary(
        //    //    item => (string)item.Key[rule.Body[0]],
        //    //    item => item.Value.Cast<string>().ToList()
        //    //);

        //    //foreach (KeyValuePair<string, List<string>> kvp in tempmaps)
        //    //{
        //    //    Console.WriteLine("K: " + kvp.Key);
        //    //    Console.WriteLine(ToStringExt(kvp.Value));
        //    //}
        //    //Console.WriteLine(res.ToString());

        //    //return DisjunctiveExamplesSpec.From(kExamples);
        //    return res;
        //}

        private Dictionary<string, Tuple<string, string>> LoadCache(string file)
        {
            Dictionary<string, Tuple<string, string>> _cache = new Dictionary<string, Tuple<string, string>>();

            // load from JSON file
            if (File.Exists(file))
            {
                _cache =
                    JsonConvert.DeserializeObject<Dictionary<string, Tuple<string, string>>>(
                        File.ReadAllText(file)
                    );
            }
            return _cache;
        }

        /// <summary>
        /// Save the cache.
        /// </summary>
        /// <param name="file"></param>
        private void SaveCache(string file, dynamic _cache)
        {
            if (!Directory.GetParent(file).Exists)
                Directory.GetParent(file).Create();
            File.WriteAllText(file, JsonConvert.SerializeObject(_cache,
                                                                Formatting.Indented));
        }

    }

}


//[WitnessFunction(nameof(Semantics.RegPos), 1)]
//internal DisjunctiveExamplesSpec WitnessRegexPair(GrammarRule rule, DisjunctiveExamplesSpec spec)
//{
//    var rrExamples = new Dictionary<State, IEnumerable<object>>();
//    foreach (State input in spec.ProvidedInputs)
//    {
//        var v = (StringRegion)input[rule.Body[0]];
//        var regexes = new List<object>();
//        foreach (uint pos in spec.DisjunctiveExamples[input])
//        {
//            UnboundedCache<Token, TokenMatch> rightMatches;
//            if (!v.Cache.TryGetAllMatchesStartingAt(pos, out rightMatches)) continue;
//            UnboundedCache<Token, TokenMatch> leftMatches;
//            if (!v.Cache.TryGetAllMatchesEndingAt(pos, out leftMatches)) continue;
//            var leftRegexes = RegularExpression.LearnLeftMatches(v, pos, RegularExpression.DefaultTokenCount);
//            var rightRegexes = RegularExpression.LearnRightMatches(v, pos, RegularExpression.DefaultTokenCount);
//            var regexPairs =
//                from l in leftRegexes from r in rightRegexes select (object)Record.Create(l, r);
//            regexes.AddRange(regexPairs);
//        }
//        rrExamples[input] = regexes;
//    }
//    return DisjunctiveExamplesSpec.From(rrExamples);
//}
//[WitnessFunction(nameof(Semantics.RegPos), 2, DependsOnParameters = new[] { 1 })]
//internal DisjunctiveExamplesSpec WitnessRegexCount(GrammarRule rule, DisjunctiveExamplesSpec spec,
//                                                   ExampleSpec regexBinding)
//{
//    var kExamples = new Dictionary<State, IEnumerable<object>>();
//    foreach (State input in spec.ProvidedInputs)
//    {
//        var v = (string)input[rule.Body[0]];
//        var rr = (Record<RegularExpression, RegularExpression>)regexBinding.Examples[input];
//        var ks = new List<object>();
//        foreach (uint pos in spec.DisjunctiveExamples[input])
//        {
//            var ms = rr.Item1.Run(v).Where(m => rr.Item2.MatchesAt(v, m.Right)).ToArray();
//            int index = ms.BinarySearchBy(m => m.Right.CompareTo(pos));
//            if (index < 0) return null;
//            ks.Add(index + 1);
//            ks.Add(index - ms.Length);
//        }
//        kExamples[input] = ks;
//    }
//    return DisjunctiveExamplesSpec.From(kExamples);
//}