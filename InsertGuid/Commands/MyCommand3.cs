using System;
using Community.VisualStudio.Toolkit;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System.Collections.Generic;
using Task = System.Threading.Tasks.Task;

namespace InsertGuid.Commands
{
    [ Command( PackageIds.MyCommand3 ) ]
    internal sealed class MyCommand3 : BaseCommand< MyCommand3 >
    {
        // 전체 코드를 각 줄 단위로 추출한다.
        void ExtractCodeByEachLine( ref String BaseCode, ref List< String > Codes )
        {
            int startIdx = 0;
            for ( int i = 0; i < BaseCode.Length; ++i )
            {
                if (BaseCode[ i ] == '\n')
                {
                    Codes.Add( BaseCode.Substring( startIdx, i - startIdx ) );
                    startIdx = i + 1;
                }
            }

            Codes.Add( BaseCode.Substring( startIdx, BaseCode.Length - startIdx ) );
        }

        // 문자열이 하나의 공백만 가지도록 만든다.
        void MakeStringHasOneSpace( ref List< String > Strs )
        {
            List< String > retStr = new List< String >();
            foreach ( String str in Strs )
            {
                String temp = str.Replace(" ", "");
                int idx = temp.IndexOf("//");
                retStr.Add(temp.Insert(idx, " ") );
            }

            Strs = retStr;
        }
        
        // 가장 긴 주석의 길이를 반환한다.
        int GetLongestCommentNum( ref List< String > Strs )
        {
            int longestCommentNum = 0;
            foreach ( String str in Strs )
            {
                int semiColonPos = -1;
                for ( int i = 0; i < str.Length; ++i )
                {
                    if ( str[i] == ';' )
                    {
                        semiColonPos = i;

                        break;
                    }
                }

                int bias = 0;
                if ( str[0] != '\t' ) bias = 1;

                int pos = str.IndexOf( "//" );
                longestCommentNum = Math.Max( longestCommentNum, pos + bias );
            }

            return longestCommentNum;
        }
        
        // 정렬된 문자열을 반환한다.
        String GetAlignedStrs( ref List< String > Strs )
        {
            MakeStringHasOneSpace( ref Strs );
            int maxLen = GetLongestCommentNum( ref Strs );

            List< String > retStrs = new List< String >();
            foreach ( String str in Strs )
            {
                int spacePos = -1;
                for ( int i = 0; i < str.Length; ++i )
                {
                    if ( str[i] == ' ' )
                    {
                        spacePos = i;

                        break;
                    }
                }

                String addedStr = "";
                int pos = str.IndexOf( "//" );
                int bias = 0;
                if ( str[0] != '\t' ) bias = -1;

                for ( int i = 0; i < maxLen - pos + bias; ++i )
                {
                    addedStr += ' ';
                }

                retStrs.Add( str.Insert( spacePos + 1, addedStr ) );
            }

            String retStr = String.Empty;
            foreach ( String str in retStrs )
            {
                retStr += str + Environment.NewLine;
            }

            retStr = retStr.Remove( retStr.Length - 1);
            retStr = retStr.Remove( retStr.Length - 1);

            return retStr;
        }
        protected override async Task ExecuteAsync( OleMenuCmdEventArgs e )
        {
            await Package.JoinableTaskFactory.SwitchToMainThreadAsync();
            DTE dte = await Package.GetServiceAsync( typeof( DTE ) ) as DTE;
            TextSelection selection = dte.ActiveDocument.Selection as TextSelection;

            string baseSourceCode = selection.Text;
            baseSourceCode = baseSourceCode.Replace( '\r'.ToString(), String.Empty );
            
            List< String > strs = new List< String >();
            ExtractCodeByEachLine( ref baseSourceCode, ref strs );

            String result = GetAlignedStrs( ref strs );
            if( result != String.Empty )
            {
                selection.Insert( result );
            }
        }
    }
}