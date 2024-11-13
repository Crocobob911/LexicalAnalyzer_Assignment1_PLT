using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;


// c#에서는 전처리기 지시문으로 상수를 선언할 수 없으므로
// token 값을 의미하는 열거형을 사용
enum Token {
    ID = 1,
    Const = 2,
    OP = 3,
    AssignOp = 11, // :=
    SemiColon = 12, // ;
    AddOp = 13, // + -
    MultOp = 14, // * /
    LeftParen = 15, // (
    RightParen = 16 // )
}

class Program {
    static void Main(string[] args) {
        //string filePath = args[0];  -- exe 파일 빌드할 때, 파일 이름을 같이 전달해줌.
        string filePath = "code1.txt";
        Console.WriteLine(filePath);
        
        LexicalAnalyzer lexAnalyzer = new LexicalAnalyzer();
        
        lexAnalyzer.Analyze(filePath);
        
        // Console.ReadKey();
        // exe 파일 빌드해서 실행하면 자꾸 켜지자마자 꺼져서 넣어둠.
        // -- 빌드할 때 주석 해제할 것!!!
    }
}

class LexicalAnalyzer {

    bool debug = false;

    private List<string> inputStringList = new List<string>();
    private List<string> lexemeList = new List<string>();
    private List<Token> tokenList = new List<Token>();
    private Token nextToken = 0;
    private string nextLexeme = "";

    private List<string> statementString = new List<string>();
    private Dictionary<string, int?> variableStorage = new Dictionary<string, int?>();

    private Dictionary<Token, int> tokenCount = new Dictionary<Token, int> {
        { Token.ID, 0 },
        { Token.Const, 0 },
        { Token.OP, 0 }
    }; // -- 요소의 개수를 저장하는 딕셔너리
    
    private string opSymbols = "(+-*/();)";
    //private bool errorFlag = false;
    //private List<string> errorList = new List<string>();
    
    public void Analyze(string filePath) {
        MakeStringListFromFile(filePath);
        MakeTokenList(inputStringList);
        
        Statements();
        //PrintAllCounts_debug();
        PrintResult();
        //if (errorFlag)      PrintElementsOfStringList(errorList);
    }

    // Text File에서 String들을 읽어와
    // Lexeme List와 Token List를 생성하는 메서드들
    private void MakeStringListFromFile(string codeFilePath) {
        // Text File에서 띄어쓰기 단위로 읽어와 Input String List에 저장해둠.
        var readFile = File.ReadAllLines(codeFilePath);
        foreach (var str in readFile) {
            var splitedStr = str.Split(' ');
            foreach (var word in splitedStr) {
                // Console.WriteLine(word);  -- 단어가 제대로 들어갔나 확인용. 나중에 지우기
                inputStringList.Add(word);
            }
        }

        // --,단어가 모두 잘 들어갔나 확인용. 나중에 지우기.
        // foreach (var str in inputStringList) {
        //     Console.WriteLine(str);
        // }
        // Console.WriteLine(" ");
    }
    private void MakeTokenList(List<string> list) {
        // 만들어진 Input String List에서 렉심을 하나씩 가져와
        // 올바른 Token을 찾아 Token list에 추가하는 메서드
        
        foreach (var lexeme in list) {
            LookUpToken(lexeme);
        }
    }
    private void LookUpToken(string lexeme) {
        // 주어진 렉심의 Token을 판별하고
        // 그것을 Token List에 추가하는 메서드
        
        if (string.IsNullOrEmpty(lexeme)) return;

        if (char.IsLetter(lexeme[0]) || lexeme[0] == '_') {
            foreach (var c in lexeme) {
                if (char.IsLetterOrDigit(c) || c == '_') { }
                else {
                    LookUpOperatorToken(lexeme); // 연산자일 것이라고 판단
                    return;
                }
            }
            lexemeList.Add(lexeme);
            tokenList.Add(Token.ID); // Ident라고 판단
            // Console.WriteLine("ID");   -- 디버깅용. 나중에 지우기
        }
        else if (lexeme.All(char.IsDigit)) {
            // 렉심에 숫자만 있다면
            lexemeList.Add(lexeme);
            tokenList.Add(Token.Const); // 상수로 판단
            // Console.WriteLine("const");   -- 디버깅용. 나중에 지우기
        }
        else {
            // ident와 const 모두 아닌 경우
            LookUpOperatorToken(lexeme); // 특수문자의 토큰을 찾기 위해 lookup 호출
        }
    }
    private void LookUpOperatorToken(string str) {
        // Identifier나 const가 아닌
        // + - * := ( ) 등의 연산자에 토큰을 판별해야할 때 사용하는 메서드
        
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
        // 띄어쓰기가 안 되어있거나, ASCII 값 32 이하의 char이 입력된 것을 정리하고, 제거해주는 메서드
        
        var splitStr = new List<String>();
        for (int i = 0; i < str.Length; i++) {
            if (str[i] is '+' or '-' or '*' or '/' or '(' or ')' or ';') {
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


    private void Lexical() {
        // 만들어둔 Lexeme 리스트와 Token 리스트에서 다음 요소를 가져와
        // next token, next lexeme을 업데이트하는 함수
        
        if (tokenList.Count == 0) {
            nextToken = 0;
            nextLexeme = "";
            return;
        }
        
        nextToken = tokenList[0];
        nextLexeme = lexemeList[0];
        
        tokenList.RemoveAt(0);
        lexemeList.RemoveAt(0);

        statementString.Add(nextLexeme);
        
        if(debug) Console.WriteLine("Next Token = " + nextToken + 
                          " | Next Lexeme = " + nextLexeme);
    }
    
    private void Statements() {
        if (debug) Console.WriteLine("<< Statements enter >>");
        Statement();
        do {
            //Lexical();
            if (nextToken == Token.SemiColon) {
                //Console.WriteLine("\t\t\t 작동을 하지 않아요!");
                
                Statement();
            }
        }while(nextToken is not 0);

        if (debug) Console.WriteLine("<< Statements Exit >>");
    }
    
    private void Statement() {
        if (debug) Console.WriteLine("<< Statement enter >>");
        Lexical();
        if (nextToken is Token.ID) {
            tokenCount[Token.ID] += 1;

            string targetVariable = nextLexeme;
            int? value = 0;

            Lexical();
            if (nextToken is Token.AssignOp) {      //assign 은 더하지 않더군요
                //tokenCount[Token.OP] += 1; 
                Lexical();
                value = Expression();
            }
            else {
                Console.WriteLine("(ERROR) Assign Symbol (:=) is expected.");
                // errorFlag = true;
                // errorList.Add("(ERROR) Assign Symbol (:=) is expected.");
            }

            variableStorage.Add(targetVariable, value);
        }
        else {
            Console.WriteLine("(ERROR) Identifier is expected next.");
            // errorFlag = true;
            // errorList.Add("(ERROR) Identifier is expected next.");
        }
        FlushStatementString();
        FlushElements();
        if (debug) Console.WriteLine("<< Statement ----------- Exit >>");
    }
      
    private int? Expression() {
        if (debug) Console.WriteLine("<< Expression Enter >>");
        int? termValue = Term();
        int? tailValue = TermTail();
        if (debug) Console.WriteLine("<< Expression Exit >>");

        return termValue + tailValue;
    }

    private int? TermTail() {
        if (debug) Console.WriteLine("<< TermTail Enter >>");

        int? result;

        if (nextToken is Token.AddOp) {
            var op = nextLexeme;
            tokenCount[Token.OP] += 1;

            Lexical();
            
            
            int? termValue = Term();
            int? tailValue = TermTail();

            if (op == "-") termValue *= -1;

            
            result = termValue + tailValue;
        }
        else {
            
            result = 0;
        }

        if (debug) Console.WriteLine("<< TermTail Exit >>");
        return result;
    }

    private int? Term() {
        if (debug) Console.WriteLine("<< Term Enter >>");
        int? factValue = Factor();
        (int?, bool) tailValue = FactorTail();
        if (debug) Console.WriteLine("<< Term Exit >>");

        //Console.WriteLine("result : " + factValue + " / " + tailValue);
        if(tailValue.Item2) return factValue * tailValue.Item1;
        return factValue / tailValue.Item1;
    }
    
    private (int?, bool) FactorTail() {
        if (debug) Console.WriteLine("<< FactorTail Enter >>");
        int? result = 1; bool isMul = true;
        if (nextToken is Token.MultOp) {
            tokenCount[Token.OP] += 1;
            if (nextLexeme == "/") isMul = false;
            Lexical();
            int? factValue = Factor();
            (int?, bool) tailValue = FactorTail();



            if (tailValue.Item2) result = factValue * tailValue.Item1;
            else result = factValue / tailValue.Item1;
        }
        else {
            result = 1;
            // 입실론 처리 -> 이거 어케 함 ->> 네 해드렸습니다
        }
        
        if (debug) Console.WriteLine("<< FactorTail Exit >>");
        return (result, isMul);
    }
  
    private int? Factor() {
        if (debug) Console.WriteLine("<< Factor Enter >>");

        int? result = 1;
        if (nextToken is Token.ID ) {
            tokenCount[Token.ID] += 1;
            result = variableStorage[nextLexeme];
            Lexical();
            
            ////
        }
        else if (nextToken is Token.Const) {
            tokenCount[Token.Const] += 1;
            result = int.Parse(nextLexeme);
            Lexical();
            
        }
        else {
            if (nextToken is Token.LeftParen) {
                Lexical();
                result = Expression();
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
        if (debug) Console.WriteLine("<< Factor Exit >>");

        return result;
    }

    
    private void FlushStatementString()
    {
        StringBuilder stringBuilder = new StringBuilder();
        foreach(var str in statementString)
        {
            stringBuilder.Append(str);
            stringBuilder.Append(' ');
        }
        //stringBuilder.Append(';');
        Console.WriteLine(stringBuilder.ToString());
        statementString.Clear();
    }
    private void FlushElements()
    {
        Console.WriteLine("ID: " + tokenCount[Token.ID] + "; CONST: " + tokenCount[Token.Const] + " OP: " + tokenCount[Token.OP] + ";");
        foreach (var key in tokenCount.Keys.ToList())
        {
            tokenCount[key] = 0;
        }
        if (true) Console.WriteLine("(OK)");
    }

    private void PrintResult()
    {
        foreach(var key in variableStorage.Keys)
        {
            Console.WriteLine($"ID : {key} , value : {variableStorage[key]}");
        }
    }

    //--------- 디버깅용. 나중에 지우기 ------------

    private void PrintElementsOfStringList(List<String> list) {
        foreach (var str in list) {
            Console.Write(str + ' ');
        }

        Console.Write("\n");
    }
    public void PrintAllCounts_debug() {
        Console.WriteLine("Id count : " + tokenCount[Token.ID] + "\nConst Count : " + tokenCount[Token.Const] + "\n" +
                          "Op Count : " + tokenCount[Token.OP]);
    }
    public void PrintAllToken_debug() {
        Console.WriteLine("-----------------");
        Console.WriteLine(" << Print all tokens >>");
        foreach (var token in tokenList) {
            Console.Write(token + " ");
        }
        Console.Write("\n");
        Console.WriteLine("-----------------");
    }
    public void PrintAllLexeme_debug() {
        Console.WriteLine("-----------------");
        Console.WriteLine(" << Print all lexemes >>");
        foreach (var lexeme in lexemeList) {
            Console.Write(lexeme + " ");
        }
        Console.Write("\n");
        Console.WriteLine("-----------------");
    }
}
