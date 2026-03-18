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
        public void SubTypeInClassConstructorDefTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyType {
                        init(Int x, Int y) {}
                    }

                    class SubType extends MyType {
                        init() {
                            super(5, 0);
                        }
                    }

                    class TestType {
                        init(MyType m) {}
                    }

                    class SubTestType extends TestType {
                        init(SubType s) { super(s); }
                    }
                    """);
            AST root = Parser.Parse(tokens);

            TypeChecker.CheckType(root);
        }

        [Fact]
        public void MethodsInClassDefTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyType {
                        init(Int x, Int y) {}

                        method a(Int y) Void {
                            Int x;
                            x = y;
                        }
                    }

                    class OtherType {
                        init() {}
                        
                        method b(Int y) Int {
                            Int x;
                            x = y;
                            return x;
                        }
                    }
                    """);
            AST root = Parser.Parse(tokens);

            TypeChecker.CheckType(root);
        }

        [Fact]
        public void AdHocClassDefTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyType {
                        init(Int x, Int y) {}
                    }

                    class SubType extends MyType {
                        init() {
                            super(5, 0);
                        }
                    }

                    class TestType {
                        init(MyType m) {}

                        method a(MyType m) Void {}
                        method a(Int x) Void {}
                    }

                    class SubTestType extends TestType {
                        init(SubType s) { super(s); }

                        method a(Boolean y) Void {}
                    }
                    """);
            AST root = Parser.Parse(tokens);

            TypeChecker.CheckType(root);
        }

        [Fact]
        public void OverridingClassDefTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyType {
                        init(Int x, Int y) {}
                    }

                    class SubType extends MyType {
                        init() {
                            super(5, 0);
                        }
                    }

                    class TestType {
                        init(MyType m) {}

                        method a(MyType m) Void {}
                    }

                    class SubTestType extends TestType {
                        init(SubType s) { super(s); }

                        method a(MyType m) Void {}
                    }
                    """);
            AST root = Parser.Parse(tokens);

            TypeChecker.CheckType(root);
        }

        [Fact]
        public void InvalidOverridingClassDefTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class TestType {
                        init() {}

                        method a(Int x) Void {}
                        method a(Int x) Int { return x; }
                    }

                    class SubTestType extends TestType {
                        init() { super(); }

                        method a(Int x) Int { return x; }
                    }
                    """);
            AST root = Parser.Parse(tokens);

            TypeChecker.CheckType(root);
        }

        [Fact]
        public void CyclicInheritanceTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyType extends OtherType {
                        init() { super(); }
                    }
                    class OtherType extends MyType {
                        init() { super(); }
                    }
                    """);
            AST root = Parser.Parse(tokens);

            Assert.Throws<TypeException>(() => TypeChecker.CheckType(root));
        }

        [Fact]
        public void UnecessarySuperCallTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyType {
                        init() { super(5, "string"); }
                    }
                    """);
            AST root = Parser.Parse(tokens);

            Assert.Throws<TypeException>(() => TypeChecker.CheckType(root));
        }

        [Fact]
        public void MismachSuperCallTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyType extends OtherType {
                        init() { super(5, "string", 1); }
                    }

                    class OtherType {
                        init(Int x, String y) {}
                    }
                    """);
            AST root = Parser.Parse(tokens);

            Assert.Throws<TypeException>(() => TypeChecker.CheckType(root));
        }
    }
}
