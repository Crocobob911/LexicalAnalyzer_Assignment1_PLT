using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;


enum Token {
    ID = 1,
    Const = 2,
    AssignOp = 11, // :=
    SemiColon = 12, // ;
    AddOp = 13, // +
    MultOp = 14, // *
    LeftParen = 15, // (
    RightParen = 16 // )
}


class Program {
    static void Main(string[] args) {
        LexicalAnalyzer lexAnalyzer = new LexicalAnalyzer();
        
        lexAnalyzer.Analyze("C:\\Users\\mmung\\Desktop\\대학\\프밍언\\프로젝트\\LLParser\\code1.txt");

        // lexAnalyzer.PrintAllLexeme_debug();
        // lexAnalyzer.PrintAllToken_debug();
    }
}




class LexicalAnalyzer {
    private List<string> inputStringList = new List<string>();
    private List<string> lexemeList = new List<string>();
    private List<Token> tokenList = new List<Token>();
    private string opSymbols = "(+-*/();)";

    public void PrintAllToken_debug() {
        Console.WriteLine("*****************");
        Console.WriteLine("Print all tokens");
        foreach (var token in tokenList) {
            Console.WriteLine(token);
        }
        Console.WriteLine(" ");
        Console.WriteLine("*****************");
    }

    public void PrintAllLexeme_debug() {
        Console.WriteLine("*****************");
        Console.WriteLine("Print all lexemes");
        foreach (var lexeme in lexemeList) {
            Console.WriteLine(lexeme);
        }

        Console.WriteLine(" ");
        Console.WriteLine("*****************");
    }
    
    public void Analyze(string filePath) {
        MakeStringListFromFile(filePath);
        MakeLexemeList(inputStringList);
    }

    private void MakeStringListFromFile(string codeFilePath) {
        var readFile = File.ReadAllLines(codeFilePath);
        foreach (var str in readFile) {
            var splitedStr = str.Split(' ');
            foreach (var word in splitedStr) {
                // Console.WriteLine(word);
                inputStringList.Add(word);
            }        
        }

        // foreach (var str in inputStringList) {
        //     Console.WriteLine(str);
        // }
        // Console.WriteLine(" ");
    }


    private void MakeLexemeList(List<string> list) {
        foreach (var lexeme in list) {
            LookUpToken(lexeme);
        }
    }
    private void LookUpToken(string str) {
        Console.WriteLine("Find Lexeme called : " + str);
            
        if (char.IsLetter(str[0]) || str[0] == '_') {
            foreach (var c in str) {
                if (char.IsLetterOrDigit(c) || c == '_') { }
                else { // letter or digit이 등장한 경우 
                    LookUpOperatorToken(str); // 다른 토큰을 붙여주기 위해 lookup 호출
                    return;
                }
            }
            lexemeList.Add(str);
            tokenList.Add(Token.ID); // Ident라고 판단
            Console.WriteLine("ID");
        }
        else if (str.All(char.IsDigit)) { // 렉심에 숫자만 있다면
            lexemeList.Add(str);
            tokenList.Add(Token.Const); // 상수로 판단
            Console.WriteLine("const");
        }        
        else { // ident와 const 모두 아닌 경우
            LookUpOperatorToken(str); // 특수문자의 토큰을 찾기 위해 lookup 호출
        }
    }
    
    private void LookUpOperatorToken(string str) {
        if (str is ":=" or "=" or ":") {
            lexemeList.Add(str);
            tokenList.Add(Token.AssignOp);
            Console.WriteLine(Token.AssignOp);
        }else if (str is ";") {
            lexemeList.Add(str);
            tokenList.Add(Token.SemiColon);
            Console.WriteLine(Token.SemiColon);
        }else if (str is "+" or "-") {
            lexemeList.Add(str);
            tokenList.Add(Token.AddOp);
            Console.WriteLine(Token.AddOp);
        }else if (str is "*" or "/") {
            lexemeList.Add(str);
            tokenList.Add(Token.MultOp);
            Console.WriteLine(Token.MultOp);
        }else if (str is "(") {
            lexemeList.Add(str);
            tokenList.Add(Token.LeftParen);
            Console.WriteLine(Token.LeftParen);
        }else if (str is ")") {
            lexemeList.Add(str);
            tokenList.Add(Token.RightParen);
            Console.WriteLine(Token.RightParen);
        }
        else {
            // 띄어쓰기가 안 되어있거나, 이상한 문자가 입력된 경우
            FixString(str);
        }
    }
    
    private void FixString(string str) {
        var splitStr = new List<String>();
        for(int i=0; i<str.Length; i++) {
            if (str[i] is '+' or '-' or '*' or '/' or '(' or ')' or ';') {
                Console.WriteLine(str[i]);
                splitStr.Add(str[..i]);
                splitStr.Add(str[i].ToString());
                splitStr.Add(str[(i + 1)..]);
                break;
            }
            if (str[i] <= 32) {
                splitStr.AddRange(str.Split(opSymbols));
                break;
            }
        }
        
        foreach (var word in splitStr) {
            // 고친 String의 Token을 다시 찾음
            LookUpToken(word);
        }  
    }

    // 오류나면 실행시키려고 만들어둔 메서드
    private void ProgramExit() {
        Environment.Exit(0);
    }
}