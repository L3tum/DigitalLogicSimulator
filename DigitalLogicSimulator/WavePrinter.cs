using System;
using System.Collections.Generic;
using System.Linq;

namespace DigitalLogicSimulator
{
    public static class WavePrinter
    {
        public static void PrintWave(Dictionary<long, List<Sample>> samples)
        {
            var lines = new List<string>();
            var nextNodeLine = 0;
            var nodeIds = samples.SelectMany(kvp => kvp.Value.Select(sample => sample.NodeId)).Distinct().ToList();

            foreach (var nodeId in nodeIds)
            {
                var numberOfOutputs = samples[0].First(sample => sample.NodeId == nodeId).OutputValues.Count;
                var nodeType = samples[0].First(sample => sample.NodeId == nodeId).NodeType.Name;

                AddLineHeader(nodeType, nodeId, numberOfOutputs, ref lines);

                foreach (var sample in samples.Select(kvp => kvp.Value.First(sample => sample.NodeId == nodeId)))
                {
                    for (var index = 0; index < sample.OutputValues.Count; index++)
                    {
                        var outputValue = sample.OutputValues[index];
                        var lineNumber = nextNodeLine + index * 3;

                        if (outputValue == 1)
                        {
                            if (lines[lineNumber].Last() == '-' || lines[lineNumber].Last() == '┌' ||
                                lines[lineNumber + 2].Last() == ' ')
                            {
                                lines[lineNumber] += "-";
                                lines[lineNumber + 1] += " ";
                                lines[lineNumber + 2] += " ";
                            }
                            else
                            {
                                lines[lineNumber] += "┌";
                                lines[lineNumber + 1] += "|";
                                lines[lineNumber + 2] += "┘";
                            }
                        }
                        else
                        {
                            if (lines[lineNumber + 2].Last() == '-' || lines[lineNumber + 2].Last() == '└' ||
                                lines[lineNumber].Last() == ' ')
                            {
                                lines[lineNumber] += " ";
                                lines[lineNumber + 1] += " ";
                                lines[lineNumber + 2] += "-";
                            }
                            else
                            {
                                lines[lineNumber] += "┐";
                                lines[lineNumber + 1] += "|";
                                lines[lineNumber + 2] += "└";
                            }
                        }
                    }
                }

                nextNodeLine += numberOfOutputs * 3;
            }

            for (var index = 0; index < lines.Count; index++)
            {
                if (index % 3 == 0)
                {
                    Console.WriteLine();
                }

                var line = lines[index];
                Console.WriteLine(line);
            }
        }

        private static void AddLineHeader(string nodeType, int nodeId, int numberOfOutputs, ref List<string> lines)
        {
            for (var i = 0; i < numberOfOutputs; i++)
            {
                lines.Add(
                    $"{string.Join("", Enumerable.Repeat(" ", nodeType.Length))} {string.Join("", Enumerable.Repeat(" ", nodeId.ToString().Length))} {string.Join("", Enumerable.Repeat(" ", i.ToString().Length))}");
                lines.Add($"{nodeType}-{nodeId}-{i}");
                lines.Add(
                    $"{string.Join("", Enumerable.Repeat(" ", nodeType.Length))} {string.Join("", Enumerable.Repeat(" ", nodeId.ToString().Length))} {string.Join("", Enumerable.Repeat(" ", i.ToString().Length))}");

                for (var j = 1; j < 4; j++)
                {
                    if (lines[lines.Count - j].Length < 20)
                    {
                        lines[lines.Count - j] += string.Join("", Enumerable.Repeat(" ", 20 - lines[lines.Count - j].Length));
                        lines[lines.Count - j] += "|\t ";
                    }
                }
            }
        }
    }
}