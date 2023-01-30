using System;
using Community.VisualStudio.Toolkit;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System.Collections.Generic;
using Task = System.Threading.Tasks.Task;
using System.Text.RegularExpressions;

namespace InsertGuid
{
    [ Command( PackageIds.MyCommand ) ]
    internal sealed class MyCommand : BaseCommand< MyCommand >
    {
        // 코드를 줄 단위로 추출한다.
        void ExtractCodeByEachLine( ref String BaseCode, ref List< String > Codes )
        {
            int startIdx = 0;
            for ( int i = 0; i < BaseCode.Length; ++i )
            {
                if ( BaseCode[i] == '\n' )
                {
                    Codes.Add( BaseCode.Substring( startIdx, i - startIdx ) );
                    startIdx = i + 1;
                }
            }

            Codes.Add( BaseCode.Substring( startIdx, BaseCode.Length - startIdx ) );
        }

        // 코드에서 입력받은 문자의 위치를 반환한다.
        int GetInputCharPosInCode( String Code, char ToFindChar, bool FindLastPos )
        {
            int charColonPos = -1;
            for ( int i = 0; i < Code.Length; ++i )
            {
                if ( Code[ i ] == ToFindChar )
                {
                    charColonPos = i;

                    if( !FindLastPos ) break;
                }
            }

            return charColonPos;
        }

        // 문자열에 알파벳이 있는지 검사한다.
        bool HasAlphabet( String Str )
        {
            return Regex.IsMatch( Str, @"[a-zA-Z]" );
        }

        // 각 줄의 코드가 타입과 이름 사이에 하나의 공백만 가지도록 수정한다.
        bool MakeEachCodeLineHasOneSpaceBetTypeAndName( ref List< String > Codes )
        {
            List< String > oneSpaceStrs = new List< String >();
            
            foreach( String code in Codes )
            {
                int semiColonPos    = GetInputCharPosInCode( code, ';', false );
                int firstSpacePos   = GetInputCharPosInCode( code, ' ', false );
                int lastTemplatePos = GetInputCharPosInCode( code, '>', true  );

                if( !HasAlphabet( code ) )
                {
                    VS.MessageBox.Show( "beautify", "빈 문자열이 있습니다. 빈 문자열을 없애주세요." );

                    return false;
                }
                else if ( semiColonPos == -1 )
                {
                    VS.MessageBox.Show( "beautify", "변수에 세미콜론이 없습니다." );
                    
                    return false;
                }
                else if ( semiColonPos > 0 && code[ semiColonPos - 1 ] == ' ')
                {
                    VS.MessageBox.Show( "beautify", "세미콜론 앞에 공백이 있으면 안됩니다." );
                    
                    return false;
                }
                else if( lastTemplatePos == -1 && firstSpacePos == -1 )
                {
                    VS.MessageBox.Show( "beautify", "변수 형식이 잘못되었습니다 [자료형] [이름]; 형식을 지켜주세요" );
                }
                else
                {
                    // 템플릿인 경우 처리
                    if( lastTemplatePos != -1 )
                    {
                        int spaceCount = 0;
                        while( true )
                        {
                            if ( code[ lastTemplatePos + spaceCount + 1 ] != ' ' ) break;
                            else ++spaceCount;
                        }

                        if ( spaceCount == 0 )
                        {
                            oneSpaceStrs.Add( code.Insert( lastTemplatePos + 1, " ".ToString() ) );
                        }
                        else
                        {
                            oneSpaceStrs.Add( code.Remove( lastTemplatePos + 1, spaceCount - 1 ) );
                        }
                    }
                    // 일반 변수인 경우 처리
                    else
                    {
                        int spaceCount = 0;
                        while ( true )
                        {
                            if ( code[ firstSpacePos + spaceCount ] != ' ' ) break;
                            else ++spaceCount;
                        }

                        oneSpaceStrs.Add( code.Remove( firstSpacePos, spaceCount - 1 ) );
                    }
                }
            }

            Codes = oneSpaceStrs;

            return true;
        }

        // 가장 긴 타입의 길이를 반환한다.
        int GetLongestTypeNum( ref List< String > Codes )
        {
            int longestNum = 0;
            foreach ( String code in Codes )
            {
                int firstSpacePos   = GetInputCharPosInCode( code, ' ', false );
                int lastTemplatePos = GetInputCharPosInCode( code, '>', false );

                // 템플릿인 경우 처리
                if ( lastTemplatePos != -1 )
                {
                    int bias = 0;
                    if ( code[ 0 ] == '\t' ) bias = -1;

                    longestNum = Math.Max( longestNum, lastTemplatePos + 1 + bias );
                }
                // 일반 변수인 경우 처리
                else
                {
                    int bias = 0;
                    if ( code[ 0 ] == '\t' ) bias = -1;

                    longestNum = Math.Max( longestNum, firstSpacePos + bias );
                }
            }

            return longestNum;
        }

        // 타입을 정렬한다.
        bool AligningType( ref List< String > Codes )
        {
            if ( !MakeEachCodeLineHasOneSpaceBetTypeAndName( ref Codes ) ) return false;

            int longestTypeNum = GetLongestTypeNum( ref Codes );
            if ( longestTypeNum == -1 ) return false;

            List< String > retStrs = new List< String >();
            foreach ( String code in Codes )
            {
                int firstSpacePos   = GetInputCharPosInCode( code, ' ', false );
                int lastTemplatePos = GetInputCharPosInCode( code, '>', false );
                
                // 템플릿인 경우 처리
                if ( lastTemplatePos != -1 )
                {
                    int bias = 0;
                    if ( code[ 0 ] != '\t' ) bias = -1;
                    
                    String addedStr = "";
                    for ( int i = 0; i < longestTypeNum - lastTemplatePos + bias; ++i )
                    {
                        addedStr += ' ';
                    }

                    retStrs.Add( code.Insert( lastTemplatePos + 1, addedStr ) );
                }
                // 일반 변수인 경우 처리
                else
                {
                    int bias = 0;
                    if ( code[ 0 ] != '\t' ) bias = -1;

                    String addedStr = "";
                    for ( int i = 0; i < longestTypeNum - firstSpacePos + 1 + bias; ++i )
                    {
                        addedStr += ' ';
                    }

                    retStrs.Add( code.Insert( firstSpacePos + 1, addedStr ) );
                }
            }

            Codes = retStrs;

            return true;
        }

        // 주석이 없다면 빈 주석을 하나 추가한다.
        void AddEmptyCommentIfNoComment( ref List< String > Codes )
        {
            List< String > addedStrs = new List< String >();
            foreach ( String code in Codes )
            {
                if ( code.Contains( "//" ) )
                {
                    addedStrs.Add( code );

                    continue;
                }

                int semiColonPos = GetInputCharPosInCode( code, ';', false );

                addedStrs.Add( code.Insert( semiColonPos + 1, " //".ToString() ) );
            }

            Codes = addedStrs;
        }

        // 주석 앞에 하나의 스페이스만 있도록 코드를 수정한다.
        void MakeCodeHasOneSpaceBefComment( ref List< String > Codes )
        {
            AddEmptyCommentIfNoComment( ref Codes );

            List< String > retStr = new List< String >();
            foreach ( String code in Codes )
            {
                int semiColonPos = GetInputCharPosInCode( code, ';', false );
                int spaceCount   = code.IndexOf( "//" ) - semiColonPos - 1;
                
                retStr.Add( code.Remove( semiColonPos + 1, spaceCount - 1 ) );
            }

            Codes = retStr;
        }

        // 가장 긴 주석의 길이를 반환한다.
        int GetLongestCommentNum( ref List< String > Codes )
        {
            int longestRemarkNum = 0;
            foreach ( String code in Codes )
            {
                int semiColonPos = GetInputCharPosInCode( code, ';', false );

                int bias = 0;
                if ( code[ 0 ] != '\t' ) bias = 1;

                int pos = code.IndexOf( "//" );
                longestRemarkNum = Math.Max( longestRemarkNum, pos + bias );
            }

            return longestRemarkNum;
        }

        // 주석을 정렬한다.
        void AligningComment( ref List< String > Codes )
        {
            MakeCodeHasOneSpaceBefComment( ref Codes );
            int longestCommentNum = GetLongestCommentNum( ref Codes );

            List< String > retStrs = new List< String >();
            foreach ( String code in Codes )
            {
                int semiColonPos = GetInputCharPosInCode( code, ';', false );

                String addedStr = "";
                int commentBegpos = code.IndexOf("//");
                int bias = 0;
                if ( code[ 0 ] != '\t' ) bias = -1;

                for ( int i = 0; i < longestCommentNum - commentBegpos + bias; ++i )
                {
                    addedStr += ' ';
                }

                retStrs.Add( code.Insert( semiColonPos+1, addedStr ) );
            }

            Codes = retStrs;
        }

        protected override async Task ExecuteAsync( OleMenuCmdEventArgs e )
        {
            await Package.JoinableTaskFactory.SwitchToMainThreadAsync();
            DTE dte = await Package.GetServiceAsync( typeof( DTE ) ) as DTE;
            TextSelection selection = dte.ActiveDocument.Selection as TextSelection;
            
            string targetSourceCode = selection.Text;
            targetSourceCode = targetSourceCode.Replace( '\r'.ToString(), String.Empty );
            List< String > codes = new List< String >();

            ExtractCodeByEachLine( ref targetSourceCode, ref codes );
            
            if ( !AligningType( ref codes ) ) return;

            AligningComment( ref codes );

            string result = String.Empty;

            foreach( String str in codes )
            {
                result += str+"\r\n";
            }

            result = result.Remove( result.Length - 1 );
            result = result.Remove( result.Length - 1 );

            selection.Insert(result);
        }
    }
}