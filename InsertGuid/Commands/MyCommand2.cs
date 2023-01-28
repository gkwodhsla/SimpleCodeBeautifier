using System;
using Community.VisualStudio.Toolkit;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System.Collections.Generic;
using Task = System.Threading.Tasks.Task;
using System.Text.RegularExpressions;

namespace InsertGuid.Commands
{
    [ Command( PackageIds.MyCommand2 ) ]
    internal sealed class MyCommand2 : BaseCommand< MyCommand2 >
    {
        const int INDEX_NONE = -1;

        // 전체 코드를 각 줄 단위로 추출한다.
        void ExtractCodeByEachLine( ref String BaseCode, ref List< String > Codes )
        {
            int startIdx = 0;
            for ( int i = 0; i < BaseCode.Length; ++i )
            {
                if ( BaseCode[ i ] == '\n' )
                {
                    Codes.Add( BaseCode.Substring( startIdx, i - startIdx ) );
                    startIdx = i + 1;
                }
            }

            Codes.Add( BaseCode.Substring( startIdx, BaseCode.Length - startIdx ) );
        }

        // 사용자로부터 기준 문자열을 입력받는다.
        String InputStandardStrFromUser()
        {
            String standardStr = Microsoft.VisualBasic.Interaction.InputBox( "기준 문자열을 입력하세요", "beautify", "", 0, 0 );

            return standardStr;
        }

        // 문자열에 알파벳이 있는지 검사한다.
        bool HasAlphabet( String Str )
        {
            return Regex.IsMatch( Str, @"[a-zA-Z]" );
        }

        // 코드 라인중에 기준 문자열이 없는 라인이 있는지 검사한다.
        bool CheckCodeLineHasStandardStr( ref List< String > CodeLines, ref String StandardStr )
        {
            bool bHas = true;
            foreach ( String str in CodeLines )
            {
                if ( HasAlphabet( str ) && str.IndexOf( StandardStr, 0 ) == INDEX_NONE )
                {
                    bHas = false;

                    break;
                }
            }

            return bHas;
        }

        // 코드를 입력받은 문자열 기준으로 정렬한다.
        String GetAlignedCodeByInputStr( ref List< String > Codes, ref String StandardStr )
        {
            List< int > standardStrPoses = new List< int >();
            foreach ( String str in Codes)
            {
                standardStrPoses.Add( str.IndexOf( StandardStr, 0 ) );
            }

            int maxLineNum = 0;
            foreach ( int pos in standardStrPoses )
            {
                maxLineNum = Math.Max( maxLineNum, pos );
            }

            int idx = -1;
            foreach ( int pos in standardStrPoses )
            {
                ++idx;

                if ( pos == INDEX_NONE ) continue;

                for (int i = 0; i < maxLineNum - pos; ++i)
                {
                    Codes[ idx ] = Codes[ idx ].Insert( pos, " " );
                }
            }

            String result = String.Empty;

            foreach ( String str in Codes )
            {
                result += str + "\r\n";
            }

            result = result.Remove( result.Length - 1 );
            result = result.Remove( result.Length - 1 );

            return result;
        }

        protected override async Task ExecuteAsync( OleMenuCmdEventArgs e )
        {
            await Package.JoinableTaskFactory.SwitchToMainThreadAsync();
            DTE dte = await Package.GetServiceAsync( typeof( DTE ) ) as DTE;
            TextSelection selection = dte.ActiveDocument.Selection as TextSelection;

            String targetSourceCode = selection.Text;
            targetSourceCode = targetSourceCode.Replace( '\r'.ToString(), String.Empty );
            
            List< String > codes = new List< String >();
            ExtractCodeByEachLine( ref targetSourceCode, ref codes );

            String standardStr = InputStandardStrFromUser();
            if ( !CheckCodeLineHasStandardStr( ref codes, ref standardStr ) )
            {
                VS.MessageBox.Show( "beautify", "다시 입력해주세요." );
            }
            else 
            {
                String result = GetAlignedCodeByInputStr( ref codes, ref standardStr );

                selection.Insert( result );
            }
        }
    }
}
