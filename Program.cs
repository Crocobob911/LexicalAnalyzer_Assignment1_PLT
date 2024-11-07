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
    private Token currentToken = 0;
    private Token nextToken = 0;
    private string currentLexeme = "";
    private string nextLexeme = "";

    private Dictionary<Token, int> tokenCount = new Dictionary<Token, int> {
        { Token.ID, 0 },
        { Token.Const, 0 },
        { Token.OP, 0 }
    };

    private Dictionary<string, int> idResult = new Dictionary<string, int>();
    private string opSymbols = "(+-*/();)";
    private bool errorFlag = false;
    private List<String> errorList = new List<string>();
    
    public void Analyze(string filePath) {
        MakeStringListFromFile(filePath);
        MakeLexemeList(inputStringList);
        
        Statements();
        if (errorFlag)      PrintStringList(errorList);

        // PrintAllCounts_debug();
        // PrintAllLexeme_debug();
        // PrintAllToken_debug();
    }

    // 아래는, Text 파일을 읽어와
    // Lexeme 리스트와 Token 리스트로 변환할 때 사용하는 메소드들
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

        // Console.WriteLine("Find Lexeme called : " + str);

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
            // Console.WriteLine("ID");
        }
        else if (str.All(char.IsDigit)) {
            // 렉심에 숫자만 있다면
            lexemeList.Add(str);
            tokenList.Add(Token.Const); // 상수로 판단
            // Console.WriteLine("const");
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
            // Console.WriteLine("assign op");
        }
        else if (str is ";") {
            lexemeList.Add(str);
            tokenList.Add(Token.SemiColon);
            // Console.WriteLine("semi colon");
        }
        else if (str is "+" or "-") {
            lexemeList.Add(str);
            tokenList.Add(Token.AddOp);
            // Console.WriteLine("add op");
        }
        else if (str is "*" or "/") {
            lexemeList.Add(str);
            tokenList.Add(Token.MultOp);
            // Console.WriteLine("mult op");
        }
        else if (str is "(") {
            lexemeList.Add(str);
            tokenList.Add(Token.LeftParen);
            // Console.WriteLine("left paren");
        }
        else if (str is ")") {
            lexemeList.Add(str);
            tokenList.Add(Token.RightParen);
            // Console.WriteLine("right paren");
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
                //Console.WriteLine(str[i]);
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

    // 만들어둔 Lexeme 리스트와 Token 리스트에서 다음 요소를 가져와
    // current와 next token, lexeme을 업데이트하는 함수
    private void Lexical() {
        // 택완이형은 이게 있던데...뭘까?
        //currentToken = nextToken;
        //currentLexeme = nextLexeme;
        if (tokenList.Count == 0) {
            nextToken = 0;
            nextLexeme = "";
            return;
        }
        
        nextToken = tokenList[0];
        nextLexeme = lexemeList[0];
        
        tokenList.RemoveAt(0);
        lexemeList.RemoveAt(0);
        
        Console.WriteLine("Next Token = " + nextToken + 
                          " | Next Lexeme = " + nextLexeme);
    }
    
    private void Statements() {
        Console.WriteLine("<< Statements enter >>");
        Statement();
        do {
            Lexical();
            if (nextToken == Token.SemiColon) { 
                Statement();
            }
        }while(nextToken is not 0);
        
        Console.WriteLine("<< Statements Exit >>");
    }
    
    private void Statement() {
        Console.WriteLine("<< Statement enter >>");
        Lexical();
        if (nextToken is Token.ID) {
            Lexical();
            if (nextToken is Token.AssignOp) {
                Lexical();
                Expression();
            }
            else {
                Console.WriteLine("(ERROR) Assign Symbol (:=) is expected.");
                // errorFlag = true;
                // errorList.Add("(ERROR) Assign Symbol (:=) is expected.");
            }
        }
        else {
            Console.WriteLine("(ERROR) Identifier is expected next.");
            // errorFlag = true;
            // errorList.Add("(ERROR) Identifier is expected next.");
        }
        Console.WriteLine("<< Statement Exit >>");
    }
      
    private void Expression() {
        Console.WriteLine("<< Expression Enter >>");
        Term();
        TermTail();
    }

    private void TermTail() {
        Console.WriteLine("<< TermTail Enter >>");
        if (nextToken is Token.AddOp) {
            Lexical();
            Term();
            TermTail();
        }
        else {
            // 입실론 처리
        }
    }

    private void Term() {
        Console.WriteLine("<< Term Enter >>");
        Factor();
        FactorTail();
    }
    
    private void FactorTail() {
        Console.WriteLine("<< FactorTail Enter >>");
        if (nextToken is Token.MultOp) {
            Lexical();
            Factor();
            FactorTail();
        }
        else {
            // 입실론 처리 -> 이거 어케 함
        }
    }
  
    private void Factor() {
        Console.WriteLine("<< Factor Enter >>");
        if (nextToken is (Token.ID or Token.Const)) {
            Lexical();
        }
        else {
            if (nextToken is Token.LeftParen) {
                Lexical();
                Expression();
                if (nextToken is Token.RightParen) {
                    Lexical();
                }
                else {
                    Console.WriteLine("(ERROR) RightParen expected for next symbol.");
                    // errorFlag = true;
                    // errorList.Add("(ERROR) RightParen expected for next symbol.");
                }
            }
        }
        Console.WriteLine("<< Factor Exit >>");
    }

    //--------- for Debug. Erase later ------------
    
    private void PrintStringList(List<String> list) {
        foreach (var str in list) {
            Console.Write(str + ' ');
        }

        Console.Write("\n");
    }
    public void PrintAllCounts_debug() {
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

}
