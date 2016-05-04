﻿using System;
using System.Linq;
using System.Net;
using HtmlAgilityPack;
using RankOne.Business.Models;

namespace RankOne.Business.Analyzers.Speed
{
    public class JavascriptMinificationAnalyzer : BaseAnalyzer
    {
        public override AnalyzeResult Analyse(HtmlNode document, params object[] additionalValues)
        {
            var result = new AnalyzeResult
            {
                Alias = "javascriptminificationanalyzer"
            };

            var url = (Uri)additionalValues[0];

            var localCssFiles = HtmlHelper.GetElementsWithAttribute(document, "script", "src").
                Where(x =>
                        x.Attributes.Any(y => y.Name == "src" && y.Value.EndsWith("js") && ((y.Value.StartsWith("/") && !y.Value.StartsWith("//"))
                            || y.Value.StartsWith(url.Host)
                        ))
                );

            var webClient = new WebClient();

            foreach (var localCssFile in localCssFiles)
            {
                var address = HtmlHelper.GetAttribute(localCssFile, "src");

                if (address != null)
                {
                    var fullPath = address.Value;
                    if (fullPath.StartsWith("/"))
                    {
                        fullPath = string.Format("{0}://{1}{2}", url.Scheme, url.Host, fullPath);
                    }

                    var content = webClient.DownloadString(fullPath);

                    var totalCharacters = content.Length;
                    var lines = content.Count(x => x == '\n');

                    var ratio = totalCharacters / lines;

                    if (ratio < 200)
                    {
                        var resultRule = new ResultRule();
                        resultRule.Code = "javascriptminificationanalyzer_file_not_minified";
                        resultRule.Type = ResultType.Hint;
                        resultRule.Tokens.Add(address.Value);
                        result.ResultRules.Add(resultRule);
                    }

                }
            }
            if (!result.ResultRules.Any())
            {
                result.AddResultRule("javascriptminificationanalyzer_all_minified", ResultType.Success);
            }

            return result;
        }
    }
}
