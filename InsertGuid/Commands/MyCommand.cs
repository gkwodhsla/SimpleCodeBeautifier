using System;
using Community.VisualStudio.Toolkit;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System.Collections.Generic;
using Task = System.Threading.Tasks.Task;

namespace InsertGuid
{
    [Command(PackageIds.MyCommand)]
    internal sealed class MyCommand : BaseCommand<MyCommand>
    {
        bool MakeStringHasOneSpace(ref List<String> Strs)
        {
            List<String> oneSpaceStrs = new List<String>();
            int maxSpace = 0;

            foreach(String str in Strs)
            {
                int semiColonPos    = -1;
                int firstSpacePos   = -1;
                int lastTemplatePos = -1;
                for(int i=0; i<str.Length; ++i)
                {
                    if (str[i] == ';')
                    {
                        semiColonPos = i;
                    }

                    if(firstSpacePos==-1 && str[i] == ' ')
                    {
                        firstSpacePos = i;
                    }

                    if( str[i] == '>' )
                    {
                        lastTemplatePos = i;
                    }
                }

                if (semiColonPos == -1)
                {
                    VS.MessageBox.Show("beautify", "변수에 세미콜론이 없습니다.");
                    return false;
                }
                else if (semiColonPos > 0 && str[semiColonPos - 1] == ' ')
                {
                    VS.MessageBox.Show("beautify", "세미콜론 앞에 공백이 있으면 안됩니다.");
                    return false;
                }
                else if(lastTemplatePos == -1 && firstSpacePos == -1)
                {
                    VS.MessageBox.Show("beautify", "변수 형식이 잘못되었습니다 [자료형] [이름]; 형식을 지켜주세요");
                }
                else
                {
                    // 템플릿인 경우 처리
                    if(lastTemplatePos != -1)
                    {
                        int spaceCount = 0;
                        while(true)
                        {
                            if (str[lastTemplatePos + spaceCount + 1] != ' ') break;
                            else ++spaceCount;
                        }

                        if (spaceCount == 0)
                        {
                            oneSpaceStrs.Add(str.Insert(lastTemplatePos + 1, " ".ToString()));
                        }
                        else
                        {
                            oneSpaceStrs.Add(str.Remove(lastTemplatePos + 1, spaceCount - 1));
                        }
                    }
                    // 일반 변수인 경우 처리
                    else
                    {
                        int spaceCount = 0;
                        while (true)
                        {
                            if (str[firstSpacePos + spaceCount] != ' ') break;
                            else ++spaceCount;
                        }

                        oneSpaceStrs.Add(str.Remove(firstSpacePos, spaceCount - 1));
                    }
                }
            }

            Strs = oneSpaceStrs;

            return true;
        }

        int GetLongestValNum(ref List<String> Strs)
        {
            bool bSuccess = MakeStringHasOneSpace(ref Strs);
            if (!bSuccess) return -1;

            int longestCount = 0;
            foreach (String str in Strs)
            {
                int firstSpacePos = -1;
                int lastTemplatePos = -1;
                for (int i = 0; i < str.Length; ++i)
                {
                    if (firstSpacePos == -1 && str[i] == ' ')
                    {
                        firstSpacePos = i;
                    }

                    if (str[i] == '>')
                    {
                        lastTemplatePos = i;
                    }
                }

                // 템플릿인 경우 처리
                if (lastTemplatePos != -1)
                {
                    int bias = 0;
                    if (str[0] == '\t') bias = -1;
                    
                    longestCount = Math.Max(longestCount, lastTemplatePos + 1 + bias);
                }
                // 일반 변수인 경우 처리
                else
                {
                    int bias = 0;
                    if (str[0] == '\t') bias = -1;

                    longestCount = Math.Max(longestCount, firstSpacePos + bias);
                }
            }

            return longestCount;
        }

        bool AligningStrs(ref List<String> Strs)
        {
            int longestNum = GetLongestValNum(ref Strs);
            if (longestNum == -1) return false;

            List<String> retStrs = new List<String>();
            foreach (String str in Strs)
            {
                int firstSpacePos = -1;
                int lastTemplatePos = -1;
                for (int i = 0; i < str.Length; ++i)
                {
                    if (firstSpacePos == -1 && str[i] == ' ')
                    {
                        firstSpacePos = i;
                    }

                    if (str[i] == '>')
                    {
                        lastTemplatePos = i;
                    }
                }

                // 템플릿인 경우 처리
                if (lastTemplatePos != -1)
                {
                    int bias = 0;
                    if (str[0] != '\t') bias = -1;
                    
                    String addedStr = "";
                    for (int i=0;i< longestNum - lastTemplatePos + bias;++i)
                    {
                        addedStr += ' ';
                    }

                    retStrs.Add(str.Insert(lastTemplatePos + 1, addedStr));
                }
                // 일반 변수인 경우 처리
                else
                {
                    int bias = 0;
                    if (str[0] != '\t') bias = -1;

                    String addedStr = "";
                    for (int i = 0; i < longestNum - firstSpacePos + 1 + bias; ++i)
                    {
                        addedStr += ' ';
                    }

                    retStrs.Add(str.Insert(firstSpacePos + 1, addedStr));
                }
            }

            Strs = retStrs;

            return true;
        }

        void AddEmptyRemark(ref List<String> Strs)
        {
            List<String> addedStrs = new List<String>();
            foreach (String str in Strs)
            {
                if (str.Contains("//"))
                {
                    addedStrs.Add(str);

                    continue;
                }

                int semiColonPos = -1;
                for (int i = 0; i < str.Length; ++i)
                {
                    if (str[i] == ';')
                    {
                        semiColonPos = i;
                    }
                }

                addedStrs.Add(str.Insert(semiColonPos + 1, " //".ToString()));
            }

            Strs = addedStrs;
        }

        void MakeStringHasOneSpaceBefRemark(ref List<String> Strs)
        {
            // 주석이 없다면 반드시 하나 달아준다. ( 변수에 주석 안다는 일이 없도록! )
            AddEmptyRemark(ref Strs);

            List<String> retStr = new List<String>();
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

                int spaceCount = str.IndexOf("//") - semiColonPos - 1;
                retStr.Add(str.Remove(semiColonPos + 1, spaceCount - 1));
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

        void AligningRemark(ref List<String> Strs)
        {
            MakeStringHasOneSpaceBefRemark(ref Strs);
            int longestRemarkNum = GetLongestRemarkNum(ref Strs);

            List<String> retStrs = new List<String>();
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

                String addedStr = "";
                int pos = str.IndexOf("//");
                int bias = 0;
                if (str[0] != '\t') bias = -1;

                for (int i=0;i<longestRemarkNum - pos + bias;++i)
                {
                    addedStr += ' ';
                }

                retStrs.Add(str.Insert(semiColonPos+1, addedStr));
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
            for (int i=0;i<text.Length;++i)
            {
                if(text[i] == '\n')
                {
                    strs.Add(text.Substring(startIdx, i - startIdx ));
                    startIdx = i + 1;
                }
            }
            strs.Add(text.Substring(startIdx, text.Length - startIdx));

            if (!AligningStrs(ref strs)) return;

            AligningRemark(ref strs);

            string result = String.Empty;

            foreach(String str in strs)
            {
                result += str+"\r\n";
            }

            result = result.Remove(result.Length - 1);
            result = result.Remove(result.Length - 1);

            selection.Insert(result);
            //selection.Text = result;
        }
    }
}