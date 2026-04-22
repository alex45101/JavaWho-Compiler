using System.Collections;
using System.Collections.Immutable;
using System.Runtime.Serialization;
using System.Text;

namespace JavaWhoCompiler
{

    public class TypeException(string message, Position position) : Exception($"{position.Line}:{position.Column}: {message}");


    public record VarInfo(TypeBase Type, bool IsAssigned, bool IsField);

    public class Scope
    {
        public Scope Parent { get; init; }
        private readonly Dictionary<string, VarInfo> lookUp = new();

        public Scope(Scope parent)
        {
            Parent = parent;
        }


        public void Define(string name, TypeBase type, Position position, List<string> output)
        {
            if (!lookUp.TryAdd(name, new VarInfo(type, false, false)))
            {
                output.Add(new TypeException($"The variable {name} is already defined", position).ToString());
            }
        }

        public void DefineAssigned(string name, TypeBase type, Position position, List<string> output)
        {
            if (!lookUp.TryAdd(name, new VarInfo(type, true, false)))
            {
                output.Add(new TypeException($"The variable {name} is already defined", position).ToString());
            }
        }

        public void DefineField(string name, TypeBase type, Position position, List<string> output)
        {
            if (!lookUp.TryAdd(name, new VarInfo(type, false, true)))
            {
                output.Add(new TypeException($"The variable {name} is already defined", position).ToString());
            }
        }

        public void Assign(string name, TypeBase type, Position position, List<string> output)
        {
            if (lookUp.TryGetValue(name, out VarInfo info))
            {
                if (!type.CanBeAssignedTo(info.Type))
                {
                    output.Add(new TypeException($"Can not assign {type} to {info.Type}", position).ToString());
                }

                lookUp[name] = new VarInfo(info.Type, true, info.IsField);
            }
            else if (Parent != null)
            {
                Parent.Assign(name, type, position, output);
            }
            else
            {
                output.Add(new TypeException($"Undefined variable {name}", position).ToString());
            }
        }

        public VarInfo LookUp(string name, Position position, List<string> output)
        {
            if (lookUp.TryGetValue(name, out VarInfo info))
            {
                return info;
            }

            if (Parent != null)
            {
                return Parent.LookUp(name, position, output);
            }

            output.Add(new TypeException($"Undefined variable {name}", position).ToString());
            return null;
        }
    }

    public abstract class TypeBase(string name)
    {
        public string Name { get; } = name;
        public TypeBase Base;
        public int DistanceFromBase { get; protected set; } = 0;

        public abstract bool CanBeAssignedTo(TypeBase other);

        public override string ToString()
        {
            return Name;
        }


        // primitives
        public readonly static PrimitiveType IntPrimitive = new("Int");
        public readonly static PrimitiveType BooleanPrimitive = new("Boolean");
        public readonly static PrimitiveType VoidPrimitive = new("Void");
        public static readonly HashSet<PrimitiveType> Primitives = [
            IntPrimitive,
            BooleanPrimitive,
            VoidPrimitive
        ];


        // built ins
        public readonly static ClassType ObjectBuiltIn = new(
                new ClassDefinition(
                    new IdentifiedNode("Object", null),
                    null,
                    [], // vardecs
                    new Constructor([], null, [], null),
                    [], // methods
                    null
                )
                );

        public readonly static ClassType StringBuiltIn = new(
                new ClassDefinition(
                    new IdentifiedNode("String", null),
                    new IdentifiedNode("Object", null),
                    [], // vardecs
                    new Constructor([], null, [], null),
                    [], // methods
                    null
                ),
                ObjectBuiltIn // extending class
                );

        public readonly static HashSet<ClassType> BuiltIns = [
            ObjectBuiltIn,
            StringBuiltIn
        ];


        public readonly static HashSet<TypeBase> Predefined = new([
                ..Primitives,
                ..BuiltIns
        ]);
    }

    public class PrimitiveType : TypeBase
    {
        public PrimitiveType(string name) : base(name)
        {
            Base = this;
        }

        public override bool CanBeAssignedTo(TypeBase other)
        {
            return Equals(other);
        }
    }


    public class TypeMap : IEnumerable<TypeBase>
    {
        private readonly Dictionary<string, TypeBase> types = new();

        public TypeMap() : this([]) { }

        public TypeMap(IEnumerable<TypeBase> predefined)
        {
            foreach (TypeBase type in predefined)
            {
                types.Add(type.Name, type);
            }
        }

        public void DefineType(TypeBase classDefinition, Position position, List<string> output)
        {
            AssertNotDefined(classDefinition.Name, position, output);

            types.Add(classDefinition.Name, classDefinition);
        }

        public bool TypeDefined(string type)
        {
            return types.ContainsKey(type);
        }

        public void AssertNotDefined(string type, Position position, List<string> output)
        {
            if (TypeDefined(type))
            {
                output.Add(new TypeException($"Type {type} is already defined", position).ToString());
            }
        }

        public bool AssertDefined(string type, Position position, List<string> output)
        {
            if (!TypeDefined(type))
            {
                output.Add(new TypeException($"Type {type} is not defined", position).ToString());
                return false;
            }

            return true;
        }

        public TypeBase GetType(IdentifiedNode node, List<string> output)
        {
            return GetType(node.Value, node.Position, output);
        }

        public TypeBase GetType(string typeName, Position position, List<string> output)
        {
            if (!AssertDefined(typeName, position, output))
            {
                return null;
            }

            TypeBase type = types[typeName];
            if (type is ClassType classType)
            {
                classType.PopulateWithTypeMap(this, output);
            }

            return type;
        }

        public T GetTypeAs<T>(string type, Position position, List<string> output)
            where T : TypeBase
        {
            TypeBase typeObj = GetType(type, position, output);
            return typeObj switch
            {
                T classType => classType,
                _ => ReturnNullAndAddError<T>(new TypeException($"Type {type} is not a {typeof(T)} type", position).ToString(), output)
            };
        }

        private T ReturnNullAndAddError<T>(string message, List<string> output)
            where T : TypeBase
        {
            output.Add(message);
            return null;
        }

        public IEnumerator<TypeBase> GetEnumerator()
        {
            foreach ((_, TypeBase type) in types)
            {
                yield return type;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }


    public sealed record TypeList(ImmutableList<TypeBase> Types)
    {
        private int savedHashCode;
        private bool hashCodeCalculated = false;

        private string UnderscoreSeperatedString = null;

        public bool AreSubtypesOf(TypeList other)
        {
            return Types.SequenceEqual(other.Types, EqualityComparer<TypeBase>.Create((thisType, otherType) =>
                        thisType.CanBeAssignedTo(otherType)
                        ));
        }

        public TypeList ToBaseList()
        {
            return new(Types.Select(type => type.Base).ToImmutableList());
        }

        public bool IsMorePreciseThan(TypeList other)
        {
            return Types.SequenceEqual(other.Types, EqualityComparer<TypeBase>.Create((thisType, otherType) =>
                        thisType.DistanceFromBase > otherType.DistanceFromBase
                ));
        }

        public enum MorePreciseResult
        {
            True,
            False,
            Ambigious
        }

        public MorePreciseResult IsMorePreciseThanNonAmbiguous(TypeList other)
        {
            if (Types.Count != other.Types.Count)
            {
                return MorePreciseResult.False;
            }

            int numLT = 0;
            int numGT = 0;
            for (int i = 0; i < Types.Count; i++)
            {
                int thisPrecision = Types[i].DistanceFromBase;
                int otherPrecision = other.Types[i].DistanceFromBase;

                if (thisPrecision > otherPrecision)
                {
                    numGT++;
                }
                else if (thisPrecision < otherPrecision)
                {
                    numLT++;
                }
            }

            if (numGT == Types.Count) return MorePreciseResult.True;

            if (numLT > 0 && numGT > 0)
            {
                return MorePreciseResult.Ambigious;
            }

            return MorePreciseResult.False;
        }

        public bool Equals(TypeList other)
        {
            return Types.SequenceEqual(other.Types);
        }

        public override int GetHashCode()
        {
            if (hashCodeCalculated) return savedHashCode;
            HashCode hashCode = new();
            foreach (TypeBase type in Types)
            {
                hashCode.Add(type);
            }

            hashCodeCalculated = true;
            savedHashCode = hashCode.ToHashCode();

            return savedHashCode;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder("(");
            for (int i = 0; i < Types.Count - 1; i++)
            {
                TypeBase type = Types[i];
                stringBuilder.Append($"{type}, ");
            }

            stringBuilder.Append($"{Types[^1]})");

            return stringBuilder.ToString();
        }

        public string ToUnderscoreSeperated()
        {
            if (UnderscoreSeperatedString != null) return UnderscoreSeperatedString;

            if (Types.Count == 0)
            {
                UnderscoreSeperatedString = "Empty";
                return UnderscoreSeperatedString;
            }

            StringBuilder s = new(Types[0].ToString());

            for (int i = 1; i < Types.Count; i++)
            {
                s.Append('_');
                s.Append(Types[i].ToString());
            }

            UnderscoreSeperatedString = s.ToString();
            return UnderscoreSeperatedString;
        }
    }

    public sealed record MethodSignature(string Name, TypeList ParamTypes, TypeBase ReturnType, Position Position)
    {
        public string MethodName = $"{ParamTypes.ToUnderscoreSeperated()}_{Name}_{ReturnType}";

        public bool Equals(MethodSignature other)
        {
            // signatures will be considered unique by (Name + ParamTypes)
            return Name == other.Name &&
                ParamTypes.Equals(other.ParamTypes);
        }

        // called when two method signatures are 'equal' (name + param types)
        public bool CanOverride(MethodSignature other)
        {
            return ReturnType.CanBeAssignedTo(other.ReturnType);
        }

        public override int GetHashCode()
        {
            HashCode hashCode = new();
            hashCode.Add(Name);
            hashCode.Add(ParamTypes);
            return hashCode.ToHashCode();
        }

        public override string ToString()
        {
            return $"{Name}{ParamTypes} {ReturnType}";
        }
    }

    public class ClassType : TypeBase
    {
        private List<AST> VariableDeclarations;
        private List<AST> MethodDefinitions;
        private Constructor Constructor;


        public ClassType ParentClassType { get; }

        // name to base type list to signature set
        public Dictionary<string, Dictionary<TypeList, HashSet<MethodSignature>>> MethodSignatures { get; } = new();

        public Dictionary<string, (TypeBase, Position)> Fields { get; private set; }

        public TypeList ConstructorTypes { get; private set; }


        private bool isChecked = false;

        // constructor for built ins (avoid defaulting to Object inheritance)
        public ClassType(
                ClassDefinition classDefinition
                ) : base(classDefinition.Name.Value)
        {
            Base = this;
            DistanceFromBase = 0;

            VariableDeclarations = classDefinition.VariableDeclarations;
            MethodDefinitions = classDefinition.MethodDefinitions;

            Constructor = (Constructor)classDefinition.Constructor;
        }

        //This constructor is for built in types, as in StringBuiltIn variable
        public ClassType(
                ClassDefinition classDefinition,
                TypeBase parentClassType
            )
            : this(classDefinition, parentClassType, null)
        { }

        public ClassType(
                ClassDefinition classDefinition,
                TypeBase parentClassType,
                List<string> output
                )
                : base(classDefinition.Name.Value)
        {
            // default to inheriting from Object
            Base = TypeBase.ObjectBuiltIn;
            ParentClassType = TypeBase.ObjectBuiltIn;

            if (parentClassType is not null && ValidateParentClass(classDefinition, parentClassType, output))
            {
                ParentClassType = parentClassType as ClassType;
                Base = parentClassType.Base;
                DistanceFromBase = parentClassType.DistanceFromBase + 1;
            }

            VariableDeclarations = classDefinition.VariableDeclarations;
            MethodDefinitions = classDefinition.MethodDefinitions;

            Constructor = (Constructor)classDefinition.Constructor;
        }

        private bool ValidateParentClass(ClassDefinition classDefinition, TypeBase parentClassType, List<string> output)
        {
            if (parentClassType is PrimitiveType primitiveType)
            {
                if (output is null)
                {
                    throw new ArgumentNullException(nameof(output), "should not be null... something went horribly wrong");
                }

                output.Add(new TypeException($"Cannot extend class by primitive type {primitiveType.Name}", classDefinition.ExtendsName.Position).ToString());
                return false;
            }

            return true;
        }

        public override bool CanBeAssignedTo(TypeBase other)
        {
            switch (other)
            {
                case ClassType classType:
                    {
                        if (classType.Name == Name) return true;

                        if (ParentClassType is null) return false;

                        return ParentClassType.CanBeAssignedTo(classType);
                    }

                default:
                    return false;
            }
        }

        public bool CanAccessField(string fieldName)
        {
            return Fields.ContainsKey(fieldName) ||
                   (
                    ParentClassType is not null &&
                    ParentClassType.CanAccessField(fieldName)
                   );
        }

        public void PopulateWithTypeMap(TypeMap typeMap, List<string> output)
        {
            if (isChecked) return;

            // populate parent class first
            if (ParentClassType is not null)
            {
                ParentClassType.PopulateWithTypeMap(typeMap, output);
            }

            InitializeFields(typeMap, output);

            InitializeConstructor(typeMap, output);

            InitializeLocalMethodSignatures(typeMap, output);
            CheckInheritedMethods(output);

            isChecked = true;
        }

        private void InitializeConstructor(TypeMap typeMap, List<string> output)
        {
            ConstructorTypes = new TypeList(
                Constructor.Parameters.Select(
                    param => typeMap.GetType(((VariableDeclaration)param).Type, output)
                    ).ToImmutableList()
            );
        }

        private void CheckMatchingParentMethodSet(Dictionary<TypeList, HashSet<MethodSignature>> parentMethodDict, Dictionary<TypeList, HashSet<MethodSignature>> localMethodDict, List<string> output)
        {
            // local class has matching method name to parent
            HashSet<MethodSignature> localMethodSet = null;
            HashSet<MethodSignature> parentMethodSet = null;

            TypeList matchingBaseTypeList = parentMethodDict.Keys.SingleOrDefault(baseTypeList => localMethodDict.ContainsKey(baseTypeList), null);
            if (matchingBaseTypeList is null)
            {
                // nothing to check
                return;
            }

            localMethodSet = localMethodDict[matchingBaseTypeList];
            parentMethodSet = parentMethodDict[matchingBaseTypeList];


            foreach (MethodSignature parentMethodSignature in parentMethodSet)
            {
                if (!localMethodSet.TryGetValue(parentMethodSignature, out MethodSignature localMethodSignature))
                {
                    // local method set isnt trying to override parent method
                    continue;
                }

                // a local method is trying to override a parent method
                if (!localMethodSignature.CanOverride(parentMethodSignature))
                {
                    output.Add(new TypeException($"Overriding method {localMethodSignature.Name}'s return type " +
                            $"{localMethodSignature.ReturnType} is not a subtype of the parent method's " +
                            $"return type {parentMethodSignature.ReturnType}", localMethodSignature.Position).ToString());
                }
            }
        }

        private void CheckInheritedMethods(List<string> output)
        {
            if (ParentClassType is null) return;

            foreach ((string parentMethodName, Dictionary<TypeList, HashSet<MethodSignature>> parentMethodDict) in ParentClassType.MethodSignatures)
            {
                if (MethodSignatures.TryGetValue(parentMethodName, out Dictionary<TypeList, HashSet<MethodSignature>> localMethodSet))
                {
                    CheckMatchingParentMethodSet(parentMethodDict, localMethodSet, output);
                }
            }
        }

        private void InitializeLocalMethodSignatures(TypeMap typeMap, List<string> output)
        {
            foreach (MethodDefinition methodDefinition in MethodDefinitions)
            {
                TypeBase newMethodReturnType = TypeBase.VoidPrimitive;
                if (methodDefinition.ReturnType is not null)
                {
                    newMethodReturnType = typeMap.GetType(methodDefinition.ReturnType.Value,
                                                        methodDefinition.ReturnType.Position,
                                                        output);
                }

                TypeList paramTypes = new(methodDefinition.Parameters.Select(
                        param => typeMap.GetType(((VariableDeclaration)param).Type, output)
                        ).ToImmutableList());
                TypeList baseParamTypes = new(methodDefinition.Parameters.Select(
                        param => typeMap.GetType(((VariableDeclaration)param).Type, output).Base
                        ).ToImmutableList());


                MethodSignature newMethodSignature = new(
                    methodDefinition.Name.Value,
                    paramTypes,
                    newMethodReturnType,
                    methodDefinition.Position
                );

                methodDefinition.Annotate(newMethodSignature);

                if (!MethodSignatures.ContainsKey(newMethodSignature.Name))
                {
                    MethodSignatures.Add(
                        newMethodSignature.Name,
                        new Dictionary<TypeList, HashSet<MethodSignature>>{
                            {
                                baseParamTypes,
                                new HashSet<MethodSignature>([newMethodSignature])
                            }
                            }
                        );
                    continue;
                }

                Dictionary<TypeList, HashSet<MethodSignature>> methodBaseTypeDict = MethodSignatures[newMethodSignature.Name];

                if (!methodBaseTypeDict.ContainsKey(baseParamTypes))
                {
                    methodBaseTypeDict.Add(baseParamTypes, new HashSet<MethodSignature>([newMethodSignature]));
                    continue;
                }

                HashSet<MethodSignature> methodSet = methodBaseTypeDict[baseParamTypes];

                if (methodSet.Contains(newMethodSignature))
                {
                    // exact signature match, local redeclaration
                    output.Add(new TypeException($"Redeclaration of method {newMethodSignature}", newMethodSignature.Position).ToString());
                }

                methodSet.Add(newMethodSignature);
            }
        }

        private void InitializeFields(TypeMap typeMap, List<string> output)
        {
            Fields = ParentClassType is not null ? new(ParentClassType.Fields) : new();

            foreach (VariableDeclaration variableDeclaration in VariableDeclarations)
            {
                if (Fields.ContainsKey(variableDeclaration.Var.Value))
                {
                    output.Add(new TypeException($"Redeclaration of field {variableDeclaration.Var.Value}", variableDeclaration.Position).ToString());
                }

                Fields.Add(
                        variableDeclaration.Var.Value,
                        (typeMap.GetType(variableDeclaration.Type.Value, variableDeclaration.Type.Position, output),
                        variableDeclaration.Type.Position)
                        );
            }
        }


        public MethodSignature GetMatchingSignature(string queryMethodName, TypeList queryMethodArguments, Position position, List<string> output)
        {
            if (!MethodSignatures.TryGetValue(queryMethodName, out Dictionary<TypeList, HashSet<MethodSignature>> baseDict))
            {
                if (ParentClassType is null)
                {
                    output.Add(new TypeException($"Class {Name} does not contain a method ${queryMethodName}", position).ToString());
                }

                return ParentClassType.GetMatchingSignature(queryMethodName, queryMethodArguments, position, output);
            }

            if (!baseDict.TryGetValue(queryMethodArguments.ToBaseList(), out HashSet<MethodSignature> methodSet))
            {
                if (ParentClassType is null)
                {
                    output.Add(new TypeException($"Class {Name} does not contain a method {queryMethodName} that matches the argument types {queryMethodArguments}", position).ToString());
                }

                return ParentClassType.GetMatchingSignature(queryMethodName, queryMethodArguments, position, output);
            }

            // avoid exhaustive search if exact match is found
            if (methodSet.TryGetValue(new MethodSignature(queryMethodName, queryMethodArguments, null, null), out MethodSignature exactMatch))
            {
                return exactMatch;
            }

            // do the comparison here to find most precise method or throw ambiguous error
            MethodSignature mostPrecise = null;
            foreach (MethodSignature methodSignature in methodSet)
            {
                if (mostPrecise is null)
                {
                    // look for method signature is usable with given type list
                    if (!methodSignature.ParamTypes.IsMorePreciseThan(queryMethodArguments))
                    {
                        mostPrecise = methodSignature;
                    }

                    continue;
                }

                mostPrecise = methodSignature.ParamTypes.IsMorePreciseThanNonAmbiguous(mostPrecise.ParamTypes) switch
                {
                    TypeList.MorePreciseResult.True => methodSignature,
                    TypeList.MorePreciseResult.False => mostPrecise,
                    TypeList.MorePreciseResult.Ambigious => ReturnNullAndAddMessage(new TypeException(
                            $"Ambiguous method call with types {queryMethodArguments}\n" +
                            $"Given types do not distinctly match {methodSignature} or {mostPrecise}"
                            , position).ToString(), output),
                    _ => throw new TypeException($"Unexpected error", position)
                };
            }

            if (mostPrecise is null)
            {
                if (ParentClassType is null)
                {
                    output.Add(new TypeException($"Class {Name} does not contain a method ${queryMethodName} that matches the argument types {queryMethodArguments}", position).ToString());
                }

                mostPrecise = ParentClassType.GetMatchingSignature(queryMethodName, queryMethodArguments, position, output);
            }

            return mostPrecise;
        }

        private MethodSignature ReturnNullAndAddMessage(string message, List<string> output)
        {
            output.Add(message);
            return null;
        }

    }

    public class TypeChecker
    {
        private Scope scope = new(null);
        private TypeMap Types = new(TypeBase.Predefined);


        private void CreateClassType(
            string className,
            Dictionary<string, ClassDefinition> definedClasses,
            HashSet<string> workingTree,
            List<string> output)
        {
            if (workingTree.Contains(className))
            {
                // cyclic inheritance
                output.Add(new TypeException($"Class {className} is part of an inheritance cycle", definedClasses[className].Position).ToString());
            }

            if (Types.TypeDefined(className))
            {
                return;
            }

            if (!definedClasses.TryGetValue(className, out ClassDefinition classDefinition))
            {
                output.Add(new TypeException($"Class {className} is not defined", new Position(1, 1)).ToString());
            }


            TypeBase extendingClassType = null;
            if (classDefinition.ExtendsName is (string extendsName, Position extendsPosition))
            {
                if (!definedClasses.ContainsKey(extendsName) && !Types.TypeDefined(extendsName))
                {
                    output.Add(new TypeException($"Inherited class {extendsName} is not defined", extendsPosition).ToString());
                }

                workingTree.Add(className);
                CreateClassType(extendsName, definedClasses, workingTree, output);
                extendingClassType = Types.GetType(extendsName, extendsPosition, output);
            }

            Types.DefineType(
                new ClassType(
                    classDefinition,
                    extendingClassType,
                    output
                ),
                classDefinition.Position,
                output
            );
        }

        private void CreateAndInitializeClassTypes(List<AST> classes, List<string> output)
        {
            Dictionary<string, ClassDefinition> definedClasses = new();

            // first pass: add classes to dictionary
            foreach (ClassDefinition classDefinition in classes)
            {
                // check built ins
                Types.AssertNotDefined(classDefinition.Name.Value, classDefinition.Position, output);

                // check user defined classes
                if (definedClasses.ContainsKey(classDefinition.Name.Value))
                {
                    output.Add(new TypeException($"Class {classDefinition.Name.Value} defined more than once", classDefinition.Position).ToString());
                    continue;
                }

                definedClasses.Add(classDefinition.Name.Value, classDefinition);
            }

            // second pass: convert ClassDefinitions into ClassTypes
            foreach (ClassDefinition classDefinition in classes)
            {
                HashSet<string> workingTree = new();
                CreateClassType(classDefinition.Name.Value, definedClasses, workingTree, output);
            }
        }

        public static List<string> CheckType(AST node)
        {
            TypeChecker typeChecker = new TypeChecker();

            List<string> output = new();

            typeChecker.CheckTypeHelper(node, output);

            return output;
        }

        private void CheckTypeHelper(AST node, List<string> output)
        {
            switch (node)
            {
                case ProgramNode prog:
                    CreateAndInitializeClassTypes(prog.Classes, output);

                    foreach (AST classDefinition in prog.Classes)
                    {
                        CheckTypeHelper(classDefinition, output);
                    }

                    foreach (AST statement in prog.Statements)
                    {
                        CheckTypeHelper(statement, output);
                    }
                    break;
                case ClassDefinition classDefinition:
                    CheckClass(classDefinition, output);

                    break;
                case VariableDeclaration varDec:
                    scope.Define(varDec.Var.Value, Types.GetType(varDec.Type, output), varDec.Position, output);

                    break;
                case AssignmentStatement assignmentStatement:
                    TypeBase rightType = GetExpressionType(assignmentStatement.Val, output);

                    scope.Assign(assignmentStatement.Var.Value, rightType, assignmentStatement.Position, output);

                    break;
                case null:
                    output.Add(new TypeException("Null node given", new Position(1, 1)).ToString());
                    break;
                default:
                    output.Add(new TypeException($"Type is not supported: {node.GetType()}", node.Position).ToString());
                    break;
            }
        }

        private void CheckClass(ClassDefinition classDefinition, List<string> output)
        {
            ClassType classType = Types.GetTypeAs<ClassType>(classDefinition.Name.Value, classDefinition.Name.Position, output);

            // enter class scope
            EnterScope();

            // hacky way of defining the type of "this"
            scope.DefineAssigned("this", classType, classDefinition.Position, output);

            // add fields to scope
            foreach ((string name, (TypeBase type, Position position)) in classType.Fields)
            {
                scope.DefineField(name, type, position, output);
            }

            Constructor constructor = (Constructor)classDefinition.Constructor;
            CheckClassConstructor(constructor, classType, output);

            foreach (MethodDefinition methodDefinition in classDefinition.MethodDefinitions)
            {
                CheckClassMethod(methodDefinition, output);
            }

            // exit class scope
            ExitScope();
        }

        private void CheckClassMethod(MethodDefinition methodDefinition, List<string> output)
        {
            EnterScope();

            AddParamsToScope(methodDefinition.Parameters, output);

            BlockStatement body = methodDefinition.Body as BlockStatement;

            TypeBase methodReturnType = TypeBase.VoidPrimitive;
            if (methodDefinition.ReturnType is not null)
            {
                methodReturnType = Types.GetType(methodDefinition.ReturnType.Value, methodDefinition.ReturnType.Position, output);
            }

            bool returned = false;
            for (int i = 0; i < body.Statements.Count; i++)
            {
                AST statement = body.Statements[i];
                if (statement is ReturnStatement returnStatement)
                {
                    if (i < body.Statements.Count - 1)
                    {
                        output.Add(new TypeException($"Unreachable code after return in method {methodDefinition.Name.Value}", methodDefinition.Position).ToString());
                    }

                    TypeBase returnExpressionType = TypeBase.VoidPrimitive;
                    if (returnStatement.Val is not null)
                    {
                        returnExpressionType = GetExpressionType(returnStatement.Val, output);

                    }

                    if (!returnExpressionType.CanBeAssignedTo(methodReturnType))
                    {
                        output.Add(new TypeException($"Method {methodDefinition.Name.Value} cannot return type {returnExpressionType}", returnStatement.Val.Position).ToString());
                    }

                    returned = true;
                }
                else
                {
                    CheckTypeHelper(statement, output);
                }
            }

            if (methodReturnType != TypeBase.VoidPrimitive && !returned)
            {
                output.Add(new TypeException($"Method {methodDefinition.Name.Value} expects return value of type {methodReturnType} but got none", methodDefinition.Position).ToString());
            }

            ExitScope();
        }

        private void CheckClassConstructor(Constructor constructor, ClassType classType, List<string> output)
        {
            // enter constructor scope
            EnterScope();

            AddParamsToScope(constructor.Parameters, output);

            // check super call
            if (classType.ParentClassType is not null)
            {
                // insert empty super call if a super isnt provided and parent is Object
                List<AST> superArguments = constructor.SuperArguments is null && classType.ParentClassType == TypeBase.ObjectBuiltIn
                                           ? []
                                           : constructor.SuperArguments;

                if (superArguments is null)
                {
                    output.Add(new TypeException($"Constructor for class {classType.Name} is missing super call", constructor.Position).ToString());
                }

                TypeList superCallTypes = GetExpressionTypeList(superArguments, output);
                if (!superCallTypes.AreSubtypesOf(classType.ParentClassType.ConstructorTypes))
                {
                    output.Add(new TypeException($"Super call arguments in class {classType.Name} are not compatible with parent class {classType.ParentClassType} constructor", constructor.Position).ToString());
                }
            }
            else if (constructor.SuperArguments is not null)
            {
                output.Add(new TypeException($"Class {classType} attempts to call a super constructor when it does not inherit any class", constructor.Position).ToString());
            }

            foreach (AST statement in constructor.Statements)
            {
                CheckTypeHelper(statement, output);
            }

            // exit constructor scope
            ExitScope();
        }

        private void AddParamsToScope(List<AST> astVariableDeclarations, List<string> output)
        {
            foreach (AST astVariableDeclaration in astVariableDeclarations)
            {
                VariableDeclaration variableDeclaration = (VariableDeclaration)astVariableDeclaration;

                scope.DefineAssigned(variableDeclaration.Var.Value, Types.GetType(variableDeclaration.Type, output), variableDeclaration.Position, output);
            }
        }

        private void EnterScope()
        {
            scope = new Scope(scope);
        }

        private void ExitScope()
        {
            scope = scope.Parent;
        }

        private TypeBase GetExpressionType(AST node, List<string> output)
        {
            return node switch
            {
                IntLiteral => TypeBase.IntPrimitive,
                StringLiteral => TypeBase.StringBuiltIn,
                BooleanLiteral => TypeBase.BooleanPrimitive,
                IdentifiedNode identifiedNode => DeriveIdentifiedNodeExpressionType(identifiedNode, output),
                NewObjectExpression newObjectExpression => DeriveNewObjectExpressionType(newObjectExpression, output),
                ThisExpression(Position position) => scope.LookUp("this", position, output).Type,
                MethodCallExpression methodCallExpression => DeriveMethodCallExpressionType(methodCallExpression, output),
                _ => AddAndReturnNull(output,
                    new TypeException($"Cannot obtain type of {node}", node.Position).ToString())
            };
        }

        private TypeBase AddAndReturnNull(List<string> output, string message)
        {
            output.Add(message);
            return null;
        }

        private TypeList GetExpressionTypeList(List<AST> nodes, List<string> output)
        {
            return new TypeList(nodes.Select(n => GetExpressionType(n, output)).ToImmutableList());
        }


        private TypeBase DeriveIdentifiedNodeExpressionType(IdentifiedNode identifiedNode, List<string> output)
        {
            VarInfo varInfo = scope.LookUp(identifiedNode.Value, identifiedNode.Position, output);
            if (!varInfo.IsAssigned)
            {
                output.Add(new TypeException($"Cannot use unassigned variable {identifiedNode.Value} in an expression", identifiedNode.Position).ToString());
            }

            identifiedNode.IsField = varInfo.IsField;

            return varInfo.Type;
        }

        private TypeBase DeriveNewObjectExpressionType(NewObjectExpression newObjectExpression, List<string> output)
        {
            ClassType classType = Types.GetTypeAs<ClassType>(newObjectExpression.ClassName.Value, newObjectExpression.ClassName.Position, output);

            // check if params are compatible with constructor
            TypeList arguments = GetExpressionTypeList(newObjectExpression.Arguments, output);
            if (!arguments.AreSubtypesOf(classType.ConstructorTypes))
            {
                Position expPosition = newObjectExpression.Position;
                output.Add(new TypeException($"Arguments for new {classType.Name} object do not match constructor types", expPosition).ToString());
            }

            return classType;
        }

        private TypeBase DeriveMethodCallExpressionType(MethodCallExpression methodCallExpression, List<string> output)
        {
            TypeBase targetType = GetExpressionType(methodCallExpression.Target, output);

            if (targetType is ClassType targetClassType)
            {
                MethodSignature matchingSignature = targetClassType.GetMatchingSignature(
                    methodCallExpression.Name,
                    GetExpressionTypeList(methodCallExpression.Arguments, output),
                    methodCallExpression.Position,
                    output
                );

                methodCallExpression.Annotate(matchingSignature);

                return matchingSignature.ReturnType;
            }

            Position targetPosition = methodCallExpression.Target.Position;
            output.Add(new TypeException($"Cannot call methods on primitive type {targetType}", targetPosition).ToString());

            return null; //might be a bad idea to return null
        }
    }
}
