using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;


// c#에서는 전처리기 지시문으로 상수를 선언할 수 없는 관계로
// token 값을 의미하는 열거형을 사용하였습니다.
enum Token {
    ID = 1,
    Const = 2,
    OP = 3,
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
        
        lexAnalyzer.Analyze("C:\\Users\\mmung\\Desktop\\대학\\프밍언\\프로젝트\\LexicalAnalizer_Assignment1_PLT\\code1.txt");
    }
}


class LexicalAnalyzer {

    private List<string> inputStringList = new List<string>();
    private List<string> lexemeList = new List<string>();
    private List<Token> tokenList = new List<Token>();
    private List<String> lineList = new List<string>();

    private Dictionary<Token, int> tokenCount = new Dictionary<Token, int> {
        { Token.ID, 0 },
        { Token.Const, 0 },
        { Token.OP, 0 }
    };

    private Dictionary<string, int> idResult = new Dictionary<string, int>();
    private string opSymbols = "(+-*/();)";
    private bool errorFlag = false;
    private List<String> errorList = new List<string>();

    public void PrintAllCounts() {
        Console.WriteLine("Id count : " + tokenCount[Token.ID] + "Const Count : " + tokenCount[Token.Const] + "\n" +
                          "Op Count : " + tokenCount[Token.OP]);
    }

    public void PrintAllToken_debug() {
        Console.WriteLine("-----------------");
        Console.WriteLine(" << Print all tokens >>");
        foreach (var token in tokenList) {
            Console.WriteLine(token);
        }

        Console.WriteLine("-----------------");
    }

    public void PrintAllLexeme_debug() {
        Console.WriteLine("-----------------");
        Console.WriteLine(" << Print all lexemes >>");
        foreach (var lexeme in lexemeList) {
            Console.WriteLine(lexeme);
        }

        Console.WriteLine("-----------------");
    }

    public void Analyze(string filePath) {
        MakeStringListFromFile(filePath);
        MakeLexemeList(inputStringList);

        PrintAllCounts();
        PrintAllLexeme_debug();
        PrintAllToken_debug();

        // statements(tokenList);
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
        if (string.IsNullOrEmpty(str)) return;

        Console.WriteLine("Find Lexeme called : " + str);

        if (char.IsLetter(str[0]) || str[0] == '_') {
            foreach (var c in str) {
                if (char.IsLetterOrDigit(c) || c == '_') { }
                else {
                    // letter or digit이 등장한 경우 
                    LookUpOperatorToken(str); // 다른 토큰을 붙여주기 위해 lookup 호출
                    return;
                }
            }

            lexemeList.Add(str);
            tokenList.Add(Token.ID); // Ident라고 판단
            Console.WriteLine("ID");
        }
        else if (str.All(char.IsDigit)) {
            // 렉심에 숫자만 있다면
            lexemeList.Add(str);
            tokenList.Add(Token.Const); // 상수로 판단
            Console.WriteLine("const");
        }
        else {
            // ident와 const 모두 아닌 경우
            LookUpOperatorToken(str); // 특수문자의 토큰을 찾기 위해 lookup 호출
        }
    }

    private void LookUpOperatorToken(string str) {
        if (str is ":=") {
            lexemeList.Add(str);
            tokenList.Add(Token.AssignOp);
            Console.WriteLine("assign op");
        }
        else if (str is ";") {
            lexemeList.Add(str);
            tokenList.Add(Token.SemiColon);
            Console.WriteLine("semi colon");
        }
        else if (str is "+" or "-") {
            lexemeList.Add(str);
            tokenList.Add(Token.AddOp);
            Console.WriteLine("add op");
        }
        else if (str is "*" or "/") {
            lexemeList.Add(str);
            tokenList.Add(Token.MultOp);
            Console.WriteLine("mult op");
        }
        else if (str is "(") {
            lexemeList.Add(str);
            tokenList.Add(Token.LeftParen);
            Console.WriteLine("left paren");
        }
        else if (str is ")") {
            lexemeList.Add(str);
            tokenList.Add(Token.RightParen);
            Console.WriteLine("right paren");
        }
        else {
            // 띄어쓰기가 안 되어있거나, 이상한 문자가 입력된 경우
            FixString(str);
        }
    }

    private void FixString(string str) {
        var splitStr = new List<String>();
        for (int i = 0; i < str.Length; i++) {
            if (str[i] is '+' or '-' or '*' or '/' or '(' or ')' or ';') {
                Console.WriteLine(str[i]);
                splitStr.Add(str[..i]);
                splitStr.Add(str[i].ToString());
                splitStr.Add(str[(i + 1)..]);
                break;
            }

            if (str[i] == ':' && str[i + 1] == '=') {
                splitStr.Add(str[..i]);
                splitStr.Add(":=");
                splitStr.Add(str[(i + 2)..]);
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

    private void PrintStringList(List<String> list) {
        foreach (var str in list) {
            Console.Write(str + ' ');
        }

        Console.Write("\n");
    }

    private void expectMatch(char current, char expect) {
        if (current == expect) { }
        else {
            errorFlag = true;
            errorList.Add("(ERROR) " + expect + "가 필요하지만 불필요한 기호 " + current + "가 입력되었습니다.");
        }
    }

    private void advance() { }

private void Statements(List<Token> tList) {
        tList = statement(tList);
        if (tList.Count == 0) {
            PrintStringList(lineList);
            Console.Write('\n');
            Console.WriteLine("ID = " + tokenCount[Token.ID] + ", Const = " + tokenCount[Token.Const] + ", Op = " +
                              tokenCount[Token.OP]);
            if (errorFlag) {
                PrintStringList(errorList);
            }
            else {
                Console.WriteLine("(OK)");
            }

            Console.Write("Result ==> ");
            foreach (var pair in idResult) {
                Console.Write("{0}: {1}", pair.Key, pair.Value);
            }

            Console.Write("\n");
        }
        else if (tList[0] == Token.SemiColon) {
            if (tList[1] is not (Token.ID or Token.Const)) {
                errorFlag = true;
                errorList.Add("(Warning) 잘못된 기호 " + tList[1] + " 제거");
                tList.RemoveAt(1);
                lexemeList.RemoveAt(1);
            }

            lineList.Add(lexemeList[0]);
            tList.RemoveAt(0);
            lineList.RemoveAt(0);
        }
    }
    
    List<Token> statement(List<Token> tList) {
        if (tList[0] == Token.ID) {
            tList.RemoveAt(0);
            tokenCount[Token.ID]++;
        }

        return null;
    }
}
