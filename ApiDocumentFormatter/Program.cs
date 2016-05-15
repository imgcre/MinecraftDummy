using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static ApiDocumentFormatter.Properties.Resources;


namespace ApiDocumentFormatter
{
    class Program
    {
        const string FileName = @"protocol1_8_8.html";
        const string ProtocolUri = @"http://wiki.vg/index.php?oldid=7018";

        static void Main(string[] args)
        {
            //获得网页
            //（正则内容位于资源中，免去\n的麻烦）
            //先将网页保存到文件中，观察网页的内容
            //在资源中构造资源表达式，对每个子表达式进行单元测试
            //将结果输出到apiDocument.txt中

            var httpContent = default(string);
            
            //如果文件存在，则直接使用文件
            if (File.Exists(FileName))
            {
                httpContent = File.ReadAllText(FileName);
            }
            else
            {
                using (var sr = new StreamReader(WebRequest.CreateHttp(ProtocolUri).GetResponse().GetResponseStream()))
                    httpContent = sr.ReadToEnd();

                File.WriteAllText(FileName, httpContent);
            }

            //构建正则表达式
            var regex = new Regex(ApiExpression.Replace("\n", string.Empty).Replace("\r", string.Empty), RegexOptions.Singleline);
            Console.WriteLine(regex.ToString());
            var matches = regex.Matches(httpContent.Replace("\n", string.Empty));


            //脚手架：检测未被匹配的封包
            var standardNames = File.ReadAllLines("apiName.txt");
            var currentNames = new List<string>();
            var unmatched = standardNames.Except(currentNames);
            foreach (Match match in matches)
                currentNames.Add(match.Groups[1].ToString());
            foreach (var name in unmatched)
                Console.Write("{0}, ", name);
            Console.WriteLine("未被匹配的封包: {0}", unmatched.Count() == 0 ? "无" : string.Empty);

            //生成摘要
            // id 状态 绑定至
            //绑定至 id 状态 名称

            var summary = from Match match in matches select new
            {
                BoundTo = match.Groups[2].Captures[2].ToString(),
                ID = match.Groups[2].Captures[0].ToString(),
                State = match.Groups[2].Captures[1].ToString(),
                Name = match.Groups[1].ToString()
            };

            //var grouped = from entry in summary group entry by entry.BoundTo into bound from entry in bound group entry by entry.State into state from entry in state group entry by entry.ID;

            //var grouped = from entry in summary orderby entry.ID group entry by entry.State into state from entry in state group entry by entry.BoundTo;

            var sb = new StringBuilder();
            foreach (var line in from entry in summary where entry.BoundTo == "Client" select entry)
            {
                sb.AppendLine(string.Format("{0} {1} {2}", line.ID, line.State, line.Name));
            }

            File.WriteAllText("apiSummaryBoundToClient.txt", sb.ToString());

            Console.ReadLine();


        }
    }
}
