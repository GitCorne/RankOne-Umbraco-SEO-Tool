﻿using System;
using System.Text;
using HtmlAgilityPack;
using RankOne.Business.Models;

namespace RankOne.Business.Analyzers.Speed
{
    public class HtmlSizeAnalyzer : BaseAnalyzer
    {
        public override AnalyzeResult Analyse(HtmlNode document, params object[] additionalValues)
        {
            var htmlSizeAnalysis = new AnalyzeResult
            {
                Alias = "htmlsizeanalyzer"
            };
            var byteCount = Encoding.Unicode.GetByteCount(document.InnerHtml);
            var htmlSizeResultRule = new ResultRule();
            if (byteCount < (33 * 1024))
            {
                htmlSizeResultRule.Code = "htmlsizeanalyzer_html_size_small";
                htmlSizeResultRule.Type = ResultType.Success;
            }
            else
            {
                htmlSizeResultRule.Code = "htmlsizeanalyzer_html_size_too_large";
                htmlSizeResultRule.Type = ResultType.Warning;
            }
            htmlSizeResultRule.Tokens.Add(SizeSuffix(byteCount));
            htmlSizeAnalysis.ResultRules.Add(htmlSizeResultRule);

            return htmlSizeAnalysis;
        }

        static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        static string SizeSuffix(int value)
        {
            if (value < 0) { return "-" + SizeSuffix(-value); }
            if (value == 0) { return "0.0 bytes"; }

            var mag = (int)Math.Log(value, 1024);
            var adjustedSize = (decimal)value / (1L << (mag * 10));

            return string.Format("{0:n1} {1}", adjustedSize, SizeSuffixes[mag]);
        }
    }
}
