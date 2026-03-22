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
        public void SubClassDeclarationBeforeBaseTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class SubType extends MyType {
                        init() {
                            super(5, 0);
                        }
                    }

                    class MyType {
                        init(Int x, Int y) {}
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
        public void OverridingMethodWithCovarianceTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class TestType {
                        init(Int x) {}

                        method a(Boolean y) Object { return new Object(); }
                    }

                    class SubTestType extends TestType {
                        init() { super(5); }

                        method a(Boolean z) String { return "hello world"; }
                    }
                    """);
            AST root = Parser.Parse(tokens);

            TypeChecker.CheckType(root);
        }

        [Fact]
        public void OverridingMethodTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class TestType {
                        init(Int x) {}

                        method a(Boolean y) Void { }
                    }

                    class SubTestType extends TestType {
                        init() { super(5); }

                        method a(Boolean z) Void { }
                    }
                    """);
            AST root = Parser.Parse(tokens);

            TypeChecker.CheckType(root);
        }

        [Fact]
        public void BasicMethodCallTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class Math {
                        init() { }

                        method reflect(Int x) Int { return x; }
                    }

                    Math m;
                    m = new Math();

                    Int x;
                    x = m.reflect(1);
                    """);
            AST root = Parser.Parse(tokens);

            TypeChecker.CheckType(root);
        }

        [Fact]
        public void BasicBadMethodCallTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class Math {
                        init() { }

                        method reflect(Int x) Int { return x; }
                    }

                    Math m;
                    m = new Math();

                    Boolean x;
                    x = m.reflect(1);
                    """);
            AST root = Parser.Parse(tokens);

            Assert.Throws<TypeException>(() => TypeChecker.CheckType(root));
        }

        [Fact]
        public void MethodCallWithCovarianceTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class TestType {
                        init(Int x) {}

                        method a(Boolean y) Object { return new Object(); }
                    }

                    class SubTestType extends TestType {
                        init() { super(5); }

                        method a(Boolean z) String { return "hello world"; }
                    }

                    SubTestType s;
                    s = new SubTestType();

                    String str;
                    str = s.a(true);

                    Object obj;
                    obj = s.a(true);
                    """);
            AST root = Parser.Parse(tokens);

            TypeChecker.CheckType(root);
        }

        [Fact]
        public void MethodCallWithOverloadingTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyType {
                        init() {}
                    }

                    class SubType extends MyType {
                        init() {
                            super();
                        }
                    }

                    class TestType {
                        init(MyType m) {}

                        method a(MyType m) Int { return 5; }
                        method a(Int x) Boolean { return true; }
                    }

                    class SubTestType extends TestType {
                        init(SubType s) { super(s); }

                        method a(Boolean y) Boolean { return y; }
                        method a(SubType s) SubType { return s; }
                    }
                    
                    SubTestType s;
                    s = new SubTestType(new SubType());
                    
                    Int a;
                    a = s.a(new MyType());

                    Boolean b;
                    b = s.a(5);

                    Boolean c;
                    c = s.a(true);

                    SubType sub;
                    sub = s.a(new SubType());
                    """);
            AST root = Parser.Parse(tokens);

            TypeChecker.CheckType(root);
        }

        [Fact]
        public void MethodCallWithThisTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyType {
                        init() {}
                        method reflectInt(Int x) Int { return x; }
                    }

                    class SubType extends MyType {
                        init() {
                            super();
                        }

                        method reflectBool(Boolean y) Boolean { return y; }

                        method test() Int {
                            Boolean y;
                            y = this.reflectBool(true);

                            return this.reflectInt(5);
                        }
                    }
                    """);
            AST root = Parser.Parse(tokens);

            TypeChecker.CheckType(root);
        }

        [Fact]
        public void AmbiguousMethodCallTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyType {
                        init() {}
                    }

                    class SubType extends MyType {
                        init() {
                            super();
                        }
                    }

                    class TestType {
                        init() {}

                        method a(MyType m, SubType s) Int { return 5; }
                        method a(SubType s, MyType m) Int { return 5; }
                    }

                    TestType t;
                    t = new TestType();

                    Int x;
                    x = t.a(new SubType(), new SubType());
                    """);
            AST root = Parser.Parse(tokens);

            Assert.Throws<TypeException>(() => TypeChecker.CheckType(root));
        }

        [Fact]
        public void InvalidOverridingMethodTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class TestType {
                        init() {}

                        method a(Int x) Void {}
                    }

                    class SubTestType extends TestType {
                        init() { super(); }

                        method a(Int x) Int { return x; }
                    }
                    """);
            AST root = Parser.Parse(tokens);

            Assert.Throws<TypeException>(() => TypeChecker.CheckType(root));
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
        public void MismatchSuperCallTest() {
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
        
        [Fact]
        public void MismatchConstructorCallTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyType {
                        init(Int x, String y) {}
                    }

                    MyType m;
                    m = new MyType(5, new Object());
                    """);
            AST root = Parser.Parse(tokens);

            Assert.Throws<TypeException>(() => TypeChecker.CheckType(root));
        }
    }
}
