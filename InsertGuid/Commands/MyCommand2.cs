using System;
using Community.VisualStudio.Toolkit;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System.Collections.Generic;
using Task = System.Threading.Tasks.Task;

namespace InsertGuid.Commands
{
    [Command(PackageIds.MyCommand2)]
    internal sealed class MyCommand2 : BaseCommand<MyCommand2>
    {
        const int INDEX_NONE = -1;
        string Swap(string s, char a, char b, char unused)
        {
            return s.Replace(a, unused).Replace(b, a).Replace(unused, b);
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

            string standardStr = Microsoft.VisualBasic.Interaction.InputBox("기준 문자열을 입력하세요", "beautify", "", 0, 0);

            List<int> poses = new List<int>();
            foreach (String str in strs)
            {
                poses.Add(str.IndexOf(standardStr, 0));
            }

            int maxLineNum = 0;
            foreach(int pos in poses)
            {
                if(pos == INDEX_NONE)
                {
                    VS.MessageBox.Show("beautify", "다시 입력해주세요.");
                    return;
                }

                maxLineNum = Math.Max(maxLineNum, pos);
            }

            int idx = 0;
            foreach (int pos in poses)
            {
                for(int i=0;i<maxLineNum - pos;++i)
                {
                    strs[idx] = strs[idx].Insert(pos, " ");
                }
                ++idx;
            }

            string result = String.Empty;

            foreach (String str in strs)
            {
                result += str + "\r\n";
            }

            result = result.Remove(result.Length - 1);
            result = result.Remove(result.Length - 1);

            selection.Insert(result);
        }
    }
}
