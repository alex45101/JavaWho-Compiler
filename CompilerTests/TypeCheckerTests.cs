using JavaWhoCompiler;
using System.ComponentModel;

namespace CompilerTests
{
    public class TypeCheckerTests
    {
        public static IEnumerable<object[]> BuiltInTypeData()
        {
            foreach(TypeBase type in TypeBase.BuiltIns) {
                yield return new object[] {
                    type.Name
                };
            }
        }

        public static IEnumerable<object[]> AssignablePrimitiveTypeData()
        {
            // Void type in an assignment context won't parse
            foreach(TypeBase type in TypeBase.Primitives.Where(t => t != TypeBase.VoidPrimitive)) {
                yield return new object[] {
                    type.Name
                };
            }
        }

        [Fact]
        [Trait("Category", "Empty")]
        public void EmptyTest()
        {
            List<string> errors = TypeChecker.CheckType(null);
            Assert.Single(errors); //give error of null input
        }

        [Fact]
        [Trait("Category", "Assignment")]
        public void IntAssignmentTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("Int a; a = 5;");
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "Assignment")]
        public void StringAssignmentTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("String a; a = \"Hello World!\";");
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "Assignment")]
        public void BooleanAssignmentTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("Boolean a; a = true;");
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "ClassDeclaration")]
        public void ClassDeclarationTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyType {
                        init() {}
                    }
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "ClassFields")]
        public void ClassVardecTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyType {
                        Int x;
                        Boolean y;
                        init() {}
                    }
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "ClassFields")]
        public void ClassUseVardecTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyType {
                        Int x;
                        Boolean y;
                        init() {
                            x = 5;
                            y = true;
                        }
                    }
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "ClassInheritence")]
        public void ClassUseInheritedVardecTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyType {
                        Int x;
                        Boolean y;
                        init() {}
                    }

                    class SubType extends MyType {
                        String z;
                        init(Int _x, Boolean _y, String _z) {
                            super();
                            x = _x;
                            y = _y;
                            z = _z;
                        }
                    }
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "Class")]
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

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "ClassInheritence")]
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

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "ClassInheritence")]
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

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "ClassInheritence")]
        public void ObjectSubClassAssignmentTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyType {
                        init(Int x, Int y) {}
                    }

                    class SubType extends MyType {
                        init() {
                            super(5, 0);
                        }
                    }

                    Object m;
                    m = new MyType(5, 4);

                    Object s;
                    s = new SubType();
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "ClassInheritence")]
        public void ExtendObjectTest() {
            // with and without super call
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyType extends Object {
                        init() { super(); }
                    }

                    class OtherType extends Object {
                        init() {}
                    }

                    Object m;
                    m = new MyType();

                    Object s;
                    s = new OtherType();
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "ClassInheritence")]
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

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "ClassMethods")]
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

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "ClassMethods")]
        public void MalformedMethodCallTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyType {
                        init() {}

                        method a(Nope y) Void {}
                    }

                    MyType m;
                    m = new MyType();
                    m.a("?");
                    m.a(doesntexist);
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.NotEmpty(errors);
        }

        [Fact]
        [Trait("Category", "ClassMethods")]
        public void MalformedMethodReturnTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyType {
                        init() {}

                        method a(Nope y) Void {
                            return whatisthis;
                        }

                        method b(Nope y) Boolean {
                            return whatisthis;
                        }

                        method c(Nope y) what {
                            return nothing;
                        }
                    }
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.NotEmpty(errors);
        }

        [Fact]
        [Trait("Category", "ClassInheritence")]
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

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "MethodOverloading")]
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

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "MethodOverloading")]
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

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "ClassMethods")]
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

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "ClassMethods")]
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

            List<string> errors = TypeChecker.CheckType(root);

            Assert.NotEmpty(errors);
        }

        [Fact]
        [Trait("Category", "MethodOverloading")]
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

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "MethodOverloading")]
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

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "ClassMethods")]
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

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "ClassMethods")]
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

            List<string> errors = TypeChecker.CheckType(root);

            Assert.NotEmpty(errors);
        }

        [Fact]
        [Trait("Category", "MethodOverloading")]
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

            List<string> errors = TypeChecker.CheckType(root);

            Assert.NotEmpty(errors);
        }

        [Fact]
        [Trait("Category", "ClassInheritence")]
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

            List<string> errors = TypeChecker.CheckType(root);

            Assert.NotEmpty(errors);
        }

        [Fact]
        [Trait("Category", "ClassInheritence")]
        public void UnecessarySuperCallTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyType {
                        init() { super(5, "string"); }
                    }
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.NotEmpty(errors);
        }

        [Fact]
        [Trait("Category", "ClassInheritence")]
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

            List<string> errors = TypeChecker.CheckType(root);

            Assert.NotEmpty(errors);
        }
        
        [Fact]
        [Trait("Category", "ClassDeclaration")]
        public void MismatchConstructorCallTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyType {
                        init(Int x, String y) {}
                    }

                    MyType m;
                    m = new MyType(5, new Object());
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.NotEmpty(errors);
        }

        [Fact]
        [Trait("Category", "ClassDeclaration")]
        public void MalformedConstructorCallTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyType {
                        init(Nope x, No y) {}
                    }

                    MyType m;
                    m = new MyType(5, "5");
                    m = new MyType(5, doesntexist);
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.NotEmpty(errors);
        }

        [Fact]
        [Trait("Category", "ClassDecleration")]
        public void RedefineClassTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyType {
                        init() {}
                    }

                    class MyType {
                        Int y;
                        init(Int x) {}
                    }
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.NotEmpty(errors);
        }

        [Fact]
        [Trait("Category", "Assignment")]
        public void UseUnassignedVarTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    Int x;
                    Int y;

                    x = y;
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.NotEmpty(errors);
        }

        [Fact]
        [Trait("Category", "Assignment")]
        public void IntBooleanVarAssignmentTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                Int x;
                Boolean y;

                x = 5;
                y = true;

                x = y;
                """);

            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.NotEmpty(errors);
        }

        [Fact]
        [Trait("Category", "Assignment")]
        public void UseUndefinedVarTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    Int x;
                    x = y;
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.NotEmpty(errors);
        }


        [Theory]
        [Trait("Category", "Class")]
        [MemberData(nameof(BuiltInTypeData))]
        public void RedefineBuiltInTest(string builtInName) {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize($$"""
                    class {{builtInName}} {
                        init() {}
                    }
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.NotEmpty(errors);
        }

        [Theory]
        [Trait("Category", "Class")]
        [MemberData(nameof(AssignablePrimitiveTypeData))]
        public void RedefinePrimitiveTest(string primitiveName) {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize($$"""
                    class {{primitiveName}} {
                        init() {}
                    }
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.NotEmpty(errors);
        }

        [Theory]
        [Trait("Category", "Class")]
        [MemberData(nameof(AssignablePrimitiveTypeData))]
        public void ExtendPrimitiveTest(string primitiveName) {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize($$"""
                    class MyType extends {{primitiveName}} {
                        init() {}
                    }
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.NotEmpty(errors);
        }

        [Fact]
        [Trait("Category", "Declaration")]
        public void DeclareVarWithUndefinedTypeTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("DoesntExist d;");
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.NotEmpty(errors);
        }

        [Fact]
        [Trait("Category", "Annotation")]
        public void AnnotatedMethodDefNameTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class OtherType { init() {} }
                    class MyType {
                        init() {}

                        method a() Int { return 5; }
                        method a(Int x) Int { return 5; }
                        method a(Boolean x, OtherType y) Int { return 5; }
                        method b(Boolean x, OtherType y) Int { return 5; }
                    }
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);

            ProgramNode program = (ProgramNode)root;

            ClassDefinition myTypeClass = (ClassDefinition)program.Classes[1];

            List<MethodDefinition> methodDefinitions = myTypeClass.MethodDefinitions.Select(ast => (MethodDefinition)ast).ToList();

            Assert.Equal("Empty_a_Int", methodDefinitions[0].AnnotatedMethodName);
            Assert.Equal("Int_a_Int", methodDefinitions[1].AnnotatedMethodName);
            Assert.Equal("Boolean_OtherType_a_Int", methodDefinitions[2].AnnotatedMethodName);
            Assert.Equal("Boolean_OtherType_b_Int", methodDefinitions[3].AnnotatedMethodName);
        }

        [Fact]
        [Trait("Category", "Annotation")]
        public void AnnotatedMethodCallNameTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class OtherType { init() {} }
                    class MyType {
                        init() {}

                        method a() Int { return 5; }
                        method a(Int x) Int { return 5; }
                        method a(Boolean x, OtherType y) Int { return 5; }
                        method b(Boolean x, OtherType y) Int { return 5; }
                    }

                    MyType m;
                    m = new MyType();

                    Int x;
                    x = m.a();
                    x = m.a(5);
                    x = m.a(true, new OtherType());
                    x = m.b(true, new OtherType());
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);

            ProgramNode program = (ProgramNode)root;

            List<MethodCallExpression> methodCalls = program.Statements
                                                            .Where(s => s is AssignmentStatement)
                                                            .Skip(1) // skip m = new MyType();
                                                            .Select(s => (AssignmentStatement)s)
                                                            .Select(s => (MethodCallExpression)s.Val).ToList();

            Assert.Equal("Empty_a_Int", methodCalls[0].AnnotatedMethodName);
            Assert.Equal("Int_a_Int", methodCalls[1].AnnotatedMethodName);
            Assert.Equal("Boolean_OtherType_a_Int", methodCalls[2].AnnotatedMethodName);
            Assert.Equal("Boolean_OtherType_b_Int", methodCalls[3].AnnotatedMethodName);
        }

        [Fact]
        [Trait("Category", "Annotation")]
        public void AnnotatedIdentifiedNodeThisTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyType {
                        Int x;
                        init() { x = 5; }

                        method a(Int y) Void { 
                            Int z;
                            z = x;
                            z = y;
                        }
                    }
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);

            ProgramNode program = (ProgramNode)root;

            ClassDefinition myTypeClass = (ClassDefinition)program.Classes[0];
            MethodDefinition methodDefinition = (MethodDefinition)myTypeClass.MethodDefinitions.First();
            List<IdentifiedNode> identifiedNodes = ((BlockStatement)methodDefinition.Body).Statements
                                                        .Where(s => s is AssignmentStatement)
                                                        .Select(s => (AssignmentStatement)s)
                                                        .Select(s => (IdentifiedNode)s.Val)
                                                        .ToList();

            Assert.True(identifiedNodes[0].IsField);
            Assert.False(identifiedNodes[1].IsField);
        }

        [Fact]
        [Trait("Category", "IfStatement")]
        public void IfTrueTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                if(true)
                {
                
                }
                """);

            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "IfStatement")]
        public void IfElseLinkTests()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                if(true)
                {
                
                }
                else if(true)
                {
                
                }
                else
                {
                
                }
                """);

            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "IfStatement")]
        public void IfFalseTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                if(false)
                {
                
                }
                """);

            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "IfStatement")]
        public void IfBlockEqualityTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                Int x;
                Int y;

                x = 5;
                y = 5;

                if(x == y)
                {
                
                }
                """);

            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "IfStatement")]
        public void IfSimpleBlockScopeTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                if(true)
                {
                    Int x;
                    x = 5;
                }

                x = 7;

                """);

            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.NotEmpty(errors);
        }

        [Fact]
        [Trait("Category", "IfStatement")]
        public void IfUsingVarsParentScopeTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                Int x;
                x = 5;
                
                if(x < 6)
                {
                    Int y;
                    y = 7;

                    x = y;
                }
                """);

            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "IfStatement")]
        public void IntIfElseTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                Int x;
                Int y;

                x = 5;

                if(x < 5)
                {
                    y = 0;
                }
                else if(x == 4)
                {
                    y = 1;
                }
                else if(x == 3)
                {
                    y = 2;
                }
                else
                {
                    y = 3;
                }
                """);

            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "WhileStatement")]
        public void WhileTrueTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                while(true)
                {
                
                }
                """);

            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "WhileStatement")]
        public void WhileFalseTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                while(false)
                {
                
                }
                """);

            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "Class")]
        public void SimpleClassTypeEqualityTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyType 
                    {
                        init(){}
                    }
                    
                    MyType a;
                    MyType b;

                    a = new MyType();
                    b = new MyType();

                    if(a == b)
                    {
                    
                    }
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);
            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "Assignment")]
        public void ClassInheritenceAssignemntTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                class MyType 
                {
                    init(){}
                }
                class SubType extends MyType 
                {
                    init(){ super(); }
                }

                MyType a;
                SubType b;

                b = new SubType();

                a = b;

                """);
        }

        [Fact]
        [Trait("Category", "Class")]
        public void InheritenceClassTypeEqualityTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyType 
                    {
                        init(){}
                    }
                    class SubType extends MyType 
                    {
                        init(){ super(); }
                    }
                    
                    MyType a;
                    SubType b;

                    a = new MyType();
                    b = new SubType();

                    if(a == b)
                    {
                    
                    }

                    if(b == a)
                    {
                    
                    }
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);
            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "Class")]
        public void ObjectClassTypeEqualityTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyType 
                    {
                        init(){}
                    }

                    MyType a;
                    Object b;

                    a = new MyType();
                    b = new Object();

                    if(a == b)
                    {
                    
                    }
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);
            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "Methods")]
        public void DeadCodePathMethodTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyType {
                        init(Int x, Int y) {}

                        method a(Int y) Int {
                            Int x;
                            x = y;
                            return x;

                            x = 10;
                            Boolean z;
                            z = true;
                        }
                    }
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.NotEmpty(errors);
        }

        [Fact]
        [Trait("Category", "Methods")]
        public void EmptyCodePathMethodTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyType {
                        init(Int x, Int y) {}

                        method a(Int y) Int {
                        }
                    }
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.NotEmpty(errors);
        }

        [Fact]
        [Trait("Category", "Methods")]
        public void CodePathsReturnWithSingleNonReturnMethodTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyType {
                        init(Int x, Int y) {}

                        method a(Int y) Int {
                            Int x;
                        }
                    }
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.NotEmpty(errors);
        }


        [Fact]
        [Trait("Category", "Methods")]
        public void CodePathsReturnWithNotTopLevelReturnMethodTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyType {
                        init(Int x, Int y) {}

                        method a(Int y) Int {
                            if(y == 5) {
                                return 5;
                            } else {
                                return 6;
                            }
                        }
                    }
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "Methods")]
        public void CodePathsReturnWithIfElseIfMethodTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyType {
                        init(Int x, Int y) {}

                        method a(Int y) Int {
                            if(y == 5) {
                                return 5;
                            } else if(y == 8) {
                                return 6;
                            } else {
                                return 9;
                            }
                        }
                    }
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "Methods")]
        public void CodePathsReturnWithIfNoElseMethodTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyType {
                        init(Int x, Int y) {}

                        method a(Int y) Int {
                            if(y == 5) {
                                return 5;
                            }

                            Int x;
                            x = 5;
                            return x;
                        }
                    }
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "Methods")]
        public void DeadCodeInIfNoElseMethodTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyType {
                        init(Int x, Int y) {}

                        method a(Int y) Int {
                            if(y == 5) {
                                return 5;
                            }

                            Int x;
                            x = 5;
                        }
                    }
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.NotEmpty(errors);
        }

        [Fact]
        [Trait("Category", "Methods")]
        public void DeadCodeInIfNoBlockMethodTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyType {
                        init(Int x, Int y) {}

                        method a(Int y) Int {
                            if(y == 5)
                                return 5;

                            Int x;
                            x = 5;
                        }
                    }
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.NotEmpty(errors);
        }

        [Fact]
        [Trait("Category", "Methods")]
        public void CodePathsReturnWithIfNoBlockMethodTest() {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                    class MyType {
                        init(Int x, Int y) {}

                        method a(Int y) Int {
                            if(y == 5)
                                return 5;

                            Int x;
                            x = 5;
                            return x;
                        }
                    }
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "Methods")]
        public void SimpleVoidMethodEmptyReturnTests()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize($$"""
                    class Test {
                        init() {}
                        method a() Void {                       
                            return;
                        }
                    }
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "Methods")]
        public void DifferentScopeReturnTests()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize($$"""
                    class Test {
                        init() {}
                        method a() Void {                       

                            if (true)
                            {
                                return;
                            }

                            return;
                        }
                    }
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "Methods")]
        public void OneReturnInIfInsideVoidMethodTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize($$"""
                    class Test {
                        init() {}
                        method a() Void {                       

                            if (true)
                            {
                                return;
                            }
                        }
                    }
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "Methods")]
        public void CodePathsReturnWithIfTrueTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize($$"""
                    class Test {
                        init() {}
                        method a() Int {                       

                            if (true)
                            {
                                return 5;
                            }
                        }
                    }
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "Methods")]
        public void DeadCodeWithIfTrueTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize($$"""
                    class Test {
                        init() {}
                        method a() Int {                       

                            if (true)
                            {
                                return 5;
                            }

                            Int x;
                        }
                    }
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.NotEmpty(errors);
        }

        [Fact]
        [Trait("Category", "Methods")]
        public void DeadCodeWithIfFalseTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize($$"""
                    class Test {
                        init() {}
                        method a() Int {                       

                            if (false)
                            {
                                return 5;
                            }

                            return 4;
                        }
                    }
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.NotEmpty(errors);
        }

        [Fact]
        [Trait("Category", "Methods")]
        public void CodePathsReturnWithIfElseTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize($$"""
                    class Test {
                        init() {}
                        method a(Int x) Int {                       

                            if (x == 8)
                            {
                                return 5;
                            }
                            else
                            {
                                return 4;
                            }
                        }
                    }
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "Methods")]
        public void CodePathsReturnWithWhileTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize($$"""
                    class Test {
                        init() {}
                        method a(Int x) Int {                       
                            while(x == 5)
                            {
                                return x;
                            }

                            while(x == 5) 
                            {

                            }

                            return x;
                        }
                    }
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "Methods")]
        public void CodePathsReturnWithWhileTrueTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize($$"""
                    class Test {
                        init() {}
                        method a(Int x) Int {                       
                            while(true)
                            {
                                return x;
                            }
                        }
                    }
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "Methods")]
        public void DeadCodeWithWhileFalseTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize($$"""
                    class Test {
                        init() {}
                        method a(Int x) Int {                       
                            while(false)
                            {
                                return x;
                            }

                            return 5;
                        }
                    }
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.NotEmpty(errors);
        }

        [Fact]
        [Trait("Category", "Methods")]
        public void CodePathsReturnWithIfInWhileTrueTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize($$"""
                    class Test {
                        init() {}
                        method a(Int x) Int {                       
                            while(true)
                            {
                                if(x == 5)
                                { 
                                    return 5;
                                }
                                else
                                {
                                    return 8;
                                }
                            }

                        }
                    }
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "Methods")]
        public void CodePathsReturnWithWhileInIfTrueTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize($$"""
                    class Test {
                        init() {}
                        method a(Int x) Int {                       
                            if(true)
                            {
                                while(true)
                                    return 8;
                            }

                        }
                    }
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "BuiltInFunctions")]
        public void PrintLnTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize($$"""
                    println("Hello, world!");
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "BuiltInFunctions")]
        public void PrintLnInClassTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize($$"""
                    class Test {
                        init() {
                            println("initializing");
                        }
                        method a() Void {
                            println("method a");
                        }
                    }
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "BuiltInFunctions")]
        public void PrintLnAsReturnTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize($$"""
                    class Test {
                        init() {}
                        method a() Void {
                            return println("method a");
                        }
                    }
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.NotEmpty(errors);
        }

        [Fact]
        [Trait("Category", "BuiltInFunctions")]
        public void PrintLnAsValueTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize($$"""
                    Int x;
                    x = println("no no no");
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.NotEmpty(errors);
        }

        [Fact]
        [Trait("Category", "BuiltInFunctions")]
        public void PrintLnWithNonStringArgTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize($$"""
                    Object o;
                    o = new Object();

                    println(5);
                    println(o);
                    """);
            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "BinaryExpressions")]
        public void SimpleAddExpressionTest()
        { 
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                Int x;
                Int y;

                x = 5;
                y = 7;

                Int result;
                result = x + y;
                """);

            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "BinaryExpressions")]
        public void SimpleSubtractExpressionTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                Int x;
                Int y;

                x = 5;
                y = 7;

                Int result;
                result = x - y;
                """);

            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "BinaryExpressions")]
        public void SimpleMultiplyExpressionTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                Int x;
                Int y;

                x = 5;
                y = 7;

                Int result;
                result = x * y;
                """);

            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "BinaryExpressions")]
        public void SimpleDivisionExpressionTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                Int x;
                Int y;

                x = 5;
                y = 7;

                Int result;
                result = x / y;
                """);

            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "BinaryExpressions")]
        public void SimpleInvalidAddExpressionTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                Int x;
                Boolean y;

                x = 5;
                y = true;

                Int result;
                result = x + y;
                """);

            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.NotEmpty(errors);
        }

        [Fact]
        [Trait("Category", "BinaryExpressions")]
        public void SimpleInvalidSubtractExpressionTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                Int x;
                Boolean y;
                
                x = 5;
                y = true;
                
                Int result;
                result = x - y;
                """);

            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.NotEmpty(errors);
        }

        [Fact]
        [Trait("Category", "BinaryExpressions")]
        public void SimpleInvalidMultiplyExpressionTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                Int x;
                Boolean y;
                
                x = 5;
                y = true;
                
                Int result;
                result = x * y;
                """);

            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.NotEmpty(errors);
        }

        [Fact]
        [Trait("Category", "BinaryExpressions")]
        public void SimpleInvalidDivisionExpressionTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                Int x;
                Boolean y;
                
                x = 5;
                y = true;
                
                Int result;
                result = x / y;
                """);

            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.NotEmpty(errors);
        }

        [Fact]
        [Trait("Category", "BinaryExpressions")]
        public void SimpleLessThanExpressionTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                Int x;
                Int y;
                
                x = 5;
                y = 7;
                
                Boolean result;
                result = x < y;
                """);

            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "BinaryExpressions")]
        public void SimpleInvalidLessThanExpressionTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                Int x;
                Boolean y;
                
                x = 5;
                y = true;
                
                Boolean result;
                result = x < y;
                """);

            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.NotEmpty(errors);
        }

        [Fact]
        [Trait("Category", "IfStatement")]
        public void MultiNestedIfScopeTest()
        { 
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                Int x;
                
                x = 5;

                if(x < 8)
                {
                    Int y;
                    y = 7;

                    if(x == y)
                    {
                        Int z;
                        z = x + y;
                    }
                }
                else if(true)
                {
                    x = 6;
                }
                else
                {
                    if (x == 2)
                    {
                        Int y;
                        y = 2;
                    }
                }
                """);

            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "IfStatement")]
        public void InvalidMultiNestedIfScopeTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                Int x;
                
                x = 5;

                if(x < 8)
                {
                    Int y;
                    y = 7;

                    if(x == y)
                    {
                        Int z;
                        z = x + y;
                    }
                }
                else if(true)
                {
                    z = 5;
                    x = 6;
                }
                else
                {
                    if (x == 2)
                    {
                        Int y;                        
                    }

                    y = 2;
                }
                """);

            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.NotEmpty(errors);
        }

        [Fact]
        [Trait("Category", "IfStatement")]
        public void NestedBinaryExpressionInIfStatementTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                Int x;
                Int y;

                x = 2;
                y = 7;

                if(((x + 7) < (y + 3)) == false)
                {
                
                }
                """);

            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "IfStatement")]
        public void InvalidNestedBinaryExpressionInIfStatementTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                Int x;
                Int y;

                x = 2;
                y = 7;

                if(((x + 7) < (y + 3)) == (1 + 2))
                {
                
                }
                """);

            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.NotEmpty(errors);
        }

        [Fact]
        [Trait("Category", "WhileStatement")]
        public void NestedBinaryExpressionInWhileStatementTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                Int x;
                Int y;

                x = 2;
                y = 7;

                while(((x + 7) < (y + 3)) == false)
                {
                
                }
                """);

            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "WhileStatement")]
        public void InvalidNestedBinaryExpressionInWhileStatementTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                Int x;
                Int y;

                x = 2;
                y = 7;

                if(((x + 7) < (y + 3)) == (1 + 2))
                {
                
                }
                """);

            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.NotEmpty(errors);
        }

        [Fact]
        [Trait("Category", "ExpressionStatement")]
        public void SimpleInvalidExpressionStatementTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                5;
                """);

            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.NotEmpty(errors);
        }

        [Fact]
        [Trait("Category", "ExpressionStatement")]
        public void SimpleMethodExpressionStatementTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                println(5);
                """);

            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.Empty(errors);
        }

        [Fact]
        [Trait("Category", "ExpressionStatement")]
        public void InvalidLessThanExpressionStatementTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                5 < 7;
                """);

            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.NotEmpty(errors);
        }

        [Fact]
        [Trait("Category", "ExpressionStatement")]
        public void InvalidAdditionExpressionStatementTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                5 + 7;
                """);

            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.NotEmpty(errors);
        }

        [Fact]
        [Trait("Category", "ExpressionStatement")]
        public void InvalidSubtractionExpressionStatementTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                5 - 7;
                """);

            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.NotEmpty(errors);
        }

        [Fact]
        [Trait("Category", "ExpressionStatement")]
        public void InvalidMultiplicationExpressionStatementTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                5 * 7;
                """);

            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.NotEmpty(errors);
        }

        [Fact]
        [Trait("Category", "ExpressionStatement")]
        public void InvalidDivisionExpressionStatementTest()
        {
            IEnumerable<IToken> tokens = Tokenizer.Tokenize("""
                5 / 7;
                """);

            AST root = Parser.Parse(tokens);

            List<string> errors = TypeChecker.CheckType(root);

            Assert.NotEmpty(errors);
        }
    }
}
