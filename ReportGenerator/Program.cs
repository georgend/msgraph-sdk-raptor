using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;
using MsGraphSDKSnippetsCompiler.Models;
using TestsCommon;

namespace ReportGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            // pass in first argument as the path to the root directory
            // validate arguments
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: ReportGenerator.exe <path to root directory>");
                return;
            }
            var rootDirectory = args[0];
            V1ExecutionKnownIssues(rootDirectory);
        }

        // create known issue visualization for v1 execution tests
        private static void V1ExecutionKnownIssues(string rootDirectory)
        {
            var version = Versions.V1;
            var issues = CSharpKnownIssues.GetCSharpExecutionKnownIssues(version);
            var counter = new Dictionary<string, int>();
            foreach (KeyValuePair<string, KnownIssue> kv in issues)
            {
                if (!kv.Key.EndsWith($"{version}-executes"))
                {
                    continue;
                }

                if (counter.ContainsKey(kv.Value.Owner))
                {
                    counter[kv.Value.Owner]++;
                }
                else
                {
                    counter.Add(kv.Value.Owner, 1);
                }
            }

            var counterList = counter.ToList();
            var ordered = counterList.OrderByDescending(x => x.Value);

            Console.WriteLine($"{version} execution known issues");
            foreach (KeyValuePair<string, int> kv in ordered)
            {
                Console.WriteLine($"{kv.Key}: {kv.Value}");
            }

            var fileName = Path.Combine(rootDirectory, "report", $"{version}-execution-known-issues-report.html",);
            VisualizeData(ordered, fileName, version);
        }

        // visualize data using chart.js
        // https://www.chartjs.org/docs/latest/charts/bar.html
        public static void VisualizeData(IOrderedEnumerable<KeyValuePair<string, int>> data, string fileName, Versions version = Versions.V1)
        {
            var labels = new List<string>();
            var values = new List<int>();
            foreach (KeyValuePair<string, int> kv in data)
            {
                labels.Add(kv.Key);
                values.Add(kv.Value);
            }

            // create the HTML
            var html = @"<!DOCTYPE html>
<html>
<head>
    <title>{0}</title>
    <script src=""https://cdn.jsdelivr.net/npm/chart.js@3.6.0/dist/chart.min.js""></script>
</head>
<body>

    <div style=""width: 800px; height: 600px;"">
        <canvas id=""myChart""></canvas>
    </div>

    <script>
        var ctx = document.getElementById('myChart').getContext('2d');
        var myChart = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: [{1}],
                datasets: [{
                    label: '# of issues',
                    data: [{2}],
                    backgroundColor: [
                        'rgba(255, 99, 132, 0.2)',
                        'rgba(54, 162, 235, 0.2)',
                        'rgba(255, 206, 86, 0.2)',
                        'rgba(75, 192, 192, 0.2)',
                        'rgba(153, 102, 255, 0.2)',
                        'rgba(255, 159, 64, 0.2)'
                    ],
                    borderColor: [
                        'rgba(255, 99, 132, 1)',
                        'rgba(54, 162, 235, 1)',
                        'rgba(255, 206, 86, 1)',
                        'rgba(75, 192, 192, 1)',
                        'rgba(153, 102, 255, 1)',
                        'rgba(255, 159, 64, 1)'
                    ],
                    borderWidth: 1
                }]
            },
            options: {
                scales: {
                    yAxes: [{
                        ticks: {
                            beginAtZero: true
                        }
                    }]
                }
            }
        });
    </script>
</body>
</html>";

            // replace the placeholders
            html = string.Format(html, $"{version} execution known issues",
                string.Join(",", labels.Select(x => $"'{x}'")),
                string.Join(",", values));

            // write the HTML to a file
            Console.WriteLine($"Writing report to {fileName}");
            File.WriteAllText(fileName, html);
        }
    }
}
