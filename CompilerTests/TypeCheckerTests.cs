using JavaWhoCompiler;

namespace CompilerTests
{
    public class TypeCheckerTests
    {
        [Fact]
        public void EmptyTest()
        {
            Assert.Throws<TypeException>(() => TypeChecker.CheckType(null));
        }

        [Fact]
        public void IntAssignmentTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("Int a; a = 5;");
            AST root = Parser.Parse(tokens);

            TypeChecker.CheckType(root);
        }

        [Fact]
        public void StringAssignmentTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("String a; a = \"Hello World!\";");
            AST root = Parser.Parse(tokens);

            TypeChecker.CheckType(root);
        }

        [Fact]
        public void BooleanAssignmentTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("Boolean a; a = true;");
            AST root = Parser.Parse(tokens);

            TypeChecker.CheckType(root);
        }

        [Fact]
        public void ClassDeclarationTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyType {
                        init() {}
                    }
                    """);
            AST root = Parser.Parse(tokens);

            TypeChecker.CheckType(root);
        }

        [Fact]
        public void SubClassDeclarationTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyType {
                        init(Int x, Int y) {}
                    }

                    class SubType extends MyType {
                        init() {
                            super(5, 0);
                        }
                    }
                    """);
            AST root = Parser.Parse(tokens);

            TypeChecker.CheckType(root);
        }

        [Fact]
        public void ClassAssignmentTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyType {
                        init() {}
                    }
                    MyType a;
                    a = new MyType();
                    """);
            AST root = Parser.Parse(tokens);

            TypeChecker.CheckType(root);
        }
    }
}
