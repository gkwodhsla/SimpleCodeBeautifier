using System;
using Community.VisualStudio.Toolkit;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System.Collections.Generic;
using Task = System.Threading.Tasks.Task;

namespace InsertGuid.Commands
{
    [Command(PackageIds.MyCommand3)]
    internal sealed class MyCommand3 : BaseCommand<MyCommand3>
    {
        void MakeStringHasOneSpace(ref List<String> Strs)
        {
            List<String> retStr = new List<String>();
            foreach (String str in Strs)
            {
                String temp = str.Replace(" ", "");
                int idx = temp.IndexOf("//");
                retStr.Add(temp.Insert(idx, " ") );
            }

            Strs = retStr;
        }
        int GetLongestRemarkNum(ref List<String> Strs)
        {
            int longestRemarkNum = 0;
            foreach (String str in Strs)
            {
                int semiColonPos = -1;
                for (int i = 0; i < str.Length; ++i)
                {
                    if (str[i] == ';')
                    {
                        semiColonPos = i;

                        break;
                    }
                }

                int bias = 0;
                if (str[0] != '\t') bias = 1;

                int pos = str.IndexOf("//");
                longestRemarkNum = Math.Max(longestRemarkNum, pos + bias);
            }

            return longestRemarkNum;
        }
        void AligningStrs(ref List<String> Strs)
        {
            MakeStringHasOneSpace(ref Strs);
            int maxLen = GetLongestRemarkNum(ref Strs);

            List<String> retStrs = new List<String>();
            foreach (String str in Strs)
            {
                int spacePos = -1;
                for (int i = 0; i < str.Length; ++i)
                {
                    if (str[i] == ' ')
                    {
                        spacePos = i;

                        break;
                    }
                }

                String addedStr = "";
                int pos = str.IndexOf("//");
                int bias = 0;
                if (str[0] != '\t') bias = -1;

                for (int i = 0; i < maxLen - pos + bias; ++i)
                {
                    addedStr += ' ';
                }

                retStrs.Add(str.Insert(spacePos + 1, addedStr));
            }

            Strs = retStrs;
        }
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await Package.JoinableTaskFactory.SwitchToMainThreadAsync();
            DTE dte = await Package.GetServiceAsync(typeof(DTE)) as DTE;
            TextSelection selection = dte.ActiveDocument.Selection as TextSelection;

            string text = selection.Text;
            text = text.Replace('\r'.ToString(), String.Empty);
            List<String> strs = new List<String>();

            int startIdx = 0;
            //코드를 \n 단위로 자른다.
            for (int i = 0; i < text.Length; ++i)
            {
                if (text[i] == '\n')
                {
                    strs.Add(text.Substring(startIdx, i - startIdx));
                    startIdx = i + 1;
                }
            }
            strs.Add(text.Substring(startIdx, text.Length - startIdx));

            AligningStrs(ref strs);

            string result = String.Empty;

            foreach (String str in strs)
            {
                result += str + Environment.NewLine;
            }

            result = result.Remove(result.Length - 1);
            result = result.Remove(result.Length - 1);

            selection.Insert(result);
        }
    }
}