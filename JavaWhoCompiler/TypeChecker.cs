using System.Collections;
using System.Collections.Immutable;
using System.Text;

namespace JavaWhoCompiler
{

    public class TypeException(string message, Position position) : Exception($"{position.Line}:{position.Column}: {message}");


    public record VarInfo(TypeBase Type, bool IsField);

    public class Scope
    {
        public Scope Parent { get; init; }
        public TypeBase ReturnType { get; init; }
        public bool InLoop { get; init; }
        public bool HasBreak { get; set; }
        public bool Initializing { get; init; }
        private readonly Dictionary<string, VarInfo> lookUp = new();

        public Scope(Scope parent, TypeBase returnType = null, bool inLoop = false, bool initializing = false)
        {
            Parent = parent;
            ReturnType = returnType;
            InLoop = inLoop;
            HasBreak = false;
            Initializing = initializing;
        }

        public void Define(string name, TypeBase type, Position position, List<string> output)
        {
            if (Initializing)
            {
                DefineWhileInitializing(name, type, position, output);
            }
            else
            {
                DefineStandard(name, type, position, output);
            }
        }

        private void DefineStandard(string name, TypeBase type, Position position, List<string> output)
        {
            if (!lookUp.TryAdd(name, new VarInfo(type, false)))
            {
                output.Add(new TypeException($"The variable {name} is already defined", position).ToString());
            }
        }

        private void DefineWhileInitializing(string name, TypeBase type, Position position, List<string> output)
        {
            // ignore output on look up
            VarInfo varInfo = LookUp(name, position, []);

            if (varInfo is not null && varInfo.IsField)
            {
                output.Add(new TypeException("Cannot shadow local or inherited class fields while initializing class", position).ToString());
            }
            else
            {
                DefineStandard(name, type, position, output);
            }
        }

        public void DefineField(string name, TypeBase type, Position position, List<string> output)
        {
            if (!lookUp.TryAdd(name, new VarInfo(type, true)))
            {
                output.Add(new TypeException($"The variable {name} is already defined", position).ToString());
            }
        }

        public void Assign(IdentifiedNode varNode, TypeBase type, Position position, List<string> output)
        {
            string name = varNode.Value;
            if (lookUp.TryGetValue(name, out VarInfo info))
            {
                if (!type.CanBeAssignedTo(info.Type))
                {
                    output.Add(new TypeException($"Can not assign {type} to {info.Type}", position).ToString());
                }

                lookUp[name] = new VarInfo(info.Type, info.IsField);

                // annotate varNode
                varNode.IsField = info.IsField;
            }
            else if (Parent != null)
            {
                Parent.Assign(varNode, type, position, output);
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
        public int DistanceFromBase { get; protected set; } = 0;
        public abstract IReadOnlyDictionary<OperatorType, Dictionary<TypeBase, TypeBase>> CompatibleOperatorTypes { get; }

        public abstract bool CanBeAssignedTo(TypeBase other);

        public override string ToString()
        {
            return Name;
        }


        // primitives
        public readonly static PrimitiveType IntPrimitive;          
        public readonly static PrimitiveType BooleanPrimitive;          
        public readonly static PrimitiveType VoidPrimitive;
        public static readonly HashSet<PrimitiveType> Primitives;


        // built ins
        public readonly static ClassType ObjectBuiltIn;
        public readonly static ClassType StringBuiltIn;

        public readonly static HashSet<ClassType> BuiltIns;
        public readonly static HashSet<TypeBase> Predefined;

        static TypeBase()
        {
            IntPrimitive = new("Int",
                new Dictionary<OperatorType, Dictionary<TypeBase, TypeBase>>()
            );

            BooleanPrimitive = new("Boolean",
                new Dictionary<OperatorType, Dictionary<TypeBase, TypeBase>>()
            );
            VoidPrimitive = new("Void", new Dictionary<OperatorType, Dictionary<TypeBase, TypeBase>>());
            Primitives = [
                IntPrimitive,
                BooleanPrimitive,
                VoidPrimitive
            ];


            // built ins
            ObjectBuiltIn = new(
                    new ClassDefinition(
                        new IdentifiedNode("Object", null),
                        null,
                        [], // vardecs
                        new Constructor([], null, [], null),
                        [], // methods
                        null
                    )
            );

            StringBuiltIn = new(
                    new ClassDefinition(
                        new IdentifiedNode("String", null),
                        new IdentifiedNode("Object", null),
                        [], // vardecs
                        new Constructor(
                            [
                                new VariableDeclaration(
                                    new IdentifiedNode("String", null),
                                    new IdentifiedNode("value", null),
                                    null
                                )
                            ], 
                            null, 
                            [], 
                            null
                        ),
                        [], // methods
                        null
                    ),
                    ObjectBuiltIn, // extending class
                    new Dictionary<OperatorType, Dictionary<TypeBase, TypeBase>>()
            );

            IntPrimitive.SetOperator(OperatorType.Add, 
                (IntPrimitive, IntPrimitive), 
                (StringBuiltIn, StringBuiltIn)
            );
            IntPrimitive.SetOperator(OperatorType.Subtract, 
                (IntPrimitive, IntPrimitive)
            );
            IntPrimitive.SetOperator(OperatorType.Multiply, 
                (IntPrimitive, IntPrimitive)
            );
            IntPrimitive.SetOperator(OperatorType.Divide, 
                (IntPrimitive, IntPrimitive)
            );
            IntPrimitive.SetOperator(OperatorType.LessThan, 
                (IntPrimitive, BooleanPrimitive)
            );

            BooleanPrimitive.SetOperator(OperatorType.Add,
                (StringBuiltIn, StringBuiltIn)
            );

            StringBuiltIn.SetOperator(OperatorType.Add, 
                (IntPrimitive, StringBuiltIn),
                (BooleanPrimitive, StringBuiltIn),
                (StringBuiltIn, StringBuiltIn)
            );

            BuiltIns = [
                ObjectBuiltIn,
            StringBuiltIn
            ];


            Predefined = new([
                    ..Primitives,
                ..BuiltIns
            ]);
        }
    }

    public class PrimitiveType : TypeBase
    {
        public override IReadOnlyDictionary<OperatorType, Dictionary<TypeBase, TypeBase>> CompatibleOperatorTypes => compOperatorTypes;

        private Dictionary<OperatorType, Dictionary<TypeBase, TypeBase>> compOperatorTypes;

        public PrimitiveType(string name, Dictionary<OperatorType, Dictionary<TypeBase, TypeBase>> compatibleOperatorTypes) : base(name)
        {
            compOperatorTypes = compatibleOperatorTypes;
        }

        internal void SetOperator(OperatorType op, params (TypeBase Other, TypeBase Result)[] mappings)
        => compOperatorTypes[op] = mappings.ToDictionary(m => m.Other, m => m.Result);

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
                        thisType is not null &&
                        thisType.CanBeAssignedTo(otherType)
                        ));
        }

        public bool IsMorePreciseThan(TypeList other)
        {
            return Types.SequenceEqual(other.Types, EqualityComparer<TypeBase>.Create((thisType, otherType) =>
                        thisType?.DistanceFromBase > otherType?.DistanceFromBase
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
                if (Types[i] is null || other.Types[i] is null)
                {
                    return MorePreciseResult.False;
                }

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
            if (Types.Count == 0)
            {
                return "()";
            }

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

            StringBuilder s = new(Types[0]?.ToString());

            for (int i = 1; i < Types.Count; i++)
            {
                s.Append('_');
                s.Append(Types[i]?.ToString());
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
        private Dictionary<OperatorType, Dictionary<TypeBase, TypeBase>> compOperatorTypes;

        public ClassType ParentClassType { get; }

        // name to param type list to signature
        public Dictionary<string, Dictionary<TypeList, MethodSignature>> MethodSignatures { get; } = new();

        public Dictionary<string, (TypeBase, Position)> Fields { get; private set; }

        public TypeList ConstructorTypes { get; private set; }

        public override IReadOnlyDictionary<OperatorType, Dictionary<TypeBase, TypeBase>> CompatibleOperatorTypes => compOperatorTypes;

        private bool isChecked = false;

        // constructor for built ins (avoid defaulting to Object inheritance)
        public ClassType(
                ClassDefinition classDefinition
                ) : base(classDefinition.Name.Value)
        {
            DistanceFromBase = 0;

            VariableDeclarations = classDefinition.VariableDeclarations;
            MethodDefinitions = classDefinition.MethodDefinitions;

            Constructor = (Constructor)classDefinition.Constructor;
        }

        //This constructor is for built in types, as in StringBuiltIn variable
        public ClassType(
                ClassDefinition classDefinition,
                TypeBase parentClassType,
                Dictionary<OperatorType, Dictionary<TypeBase, TypeBase>> compatibleOperatorTypes
            )
            : this(classDefinition, parentClassType, compatibleOperatorTypes, null)
        { }

        public ClassType(
                ClassDefinition classDefinition,
                TypeBase parentClassType,
                Dictionary<OperatorType, Dictionary<TypeBase, TypeBase>> compatibleOperatorTypes,
                List<string> output
                )
                : base(classDefinition.Name.Value)
        {
            // default to inheriting from Object
            ParentClassType = TypeBase.ObjectBuiltIn;

            compOperatorTypes = compatibleOperatorTypes;

            if (parentClassType is not null && ValidateParentClass(classDefinition, parentClassType, output))
            {
                ParentClassType = parentClassType as ClassType;
                DistanceFromBase = parentClassType.DistanceFromBase + 1;
            }

            VariableDeclarations = classDefinition.VariableDeclarations;
            MethodDefinitions = classDefinition.MethodDefinitions;

            Constructor = (Constructor)classDefinition.Constructor;
        }

        internal void SetOperator(OperatorType op, params (TypeBase Other, TypeBase Result)[] mappings)
        => compOperatorTypes[op] = mappings.ToDictionary(m => m.Other, m => m.Result);

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
            isChecked = true;

            // populate parent class first
            if (ParentClassType is not null)
            {
                ParentClassType.PopulateWithTypeMap(typeMap, output);
            }

            InitializeFields(typeMap, output);

            InitializeConstructor(typeMap, output);

            InitializeLocalMethodSignatures(typeMap, output);
            CheckInheritedMethods(output);
        }

        private void InitializeConstructor(TypeMap typeMap, List<string> output)
        {
            ConstructorTypes = new TypeList(
                Constructor.Parameters.Select(
                    param => typeMap.GetType(((VariableDeclaration)param).Type, output)
                    ).ToImmutableList()
            );
        }

        private void CheckMatchingParentMethodSet(Dictionary<TypeList, MethodSignature> parentMethodTypeDict, Dictionary<TypeList, MethodSignature> localMethodTypeDict, List<string> output)
        {
            foreach ((TypeList localMethodTypeList, MethodSignature localMethodSignature) in localMethodTypeDict)
            {
                // override occurs when method param types match exactly
                // method overrides need to have covariant return type
                if (parentMethodTypeDict.TryGetValue(localMethodTypeList, out MethodSignature parentMethodSignature)
                    && !localMethodSignature.CanOverride(parentMethodSignature))
                {
                    output.Add(new TypeException($"Overriding method {localMethodSignature.Name}'s return type " +
                            $"{localMethodSignature.ReturnType} is not a subtype of the parent method's " +
                            $"return type {parentMethodSignature.ReturnType}", localMethodSignature.Position).ToString());
                }
            }
        }

        public bool TryGetMethodTypeDict(string methodName, out Dictionary<TypeList, MethodSignature> methodTypeDict)
        {
            if (!MethodSignatures.TryGetValue(methodName, out methodTypeDict))
            {
                if (ParentClassType is null)
                {
                    return false;
                }

                return ParentClassType.TryGetMethodTypeDict(methodName, out methodTypeDict);
            }

            return true;
        }

        private void CheckInheritedMethods(List<string> output)
        {
            if (ParentClassType is null) return;

            foreach ((string localMethodName, Dictionary<TypeList, MethodSignature> localMethodTypeDict) in MethodSignatures)
            {
                if (ParentClassType.TryGetMethodTypeDict(localMethodName, out Dictionary<TypeList, MethodSignature> parentMethodTypeDict))
                {
                    CheckMatchingParentMethodSet(parentMethodTypeDict, localMethodTypeDict, output);
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


                MethodSignature newMethodSignature = new(
                    methodDefinition.Name.Value,
                    paramTypes,
                    newMethodReturnType,
                    methodDefinition.Position
                );

                methodDefinition.Annotate(newMethodSignature);

                if (!MethodSignatures.TryGetValue(newMethodSignature.Name, out Dictionary<TypeList, MethodSignature> methodTypeDict))
                {
                    // completely new method name
                    MethodSignatures.Add(
                        newMethodSignature.Name,
                        new Dictionary<TypeList, MethodSignature> {
                            {
                                paramTypes,
                                newMethodSignature
                            }
                        }
                    );
                    continue;
                }

                // method overloading attempt here
                if (!methodTypeDict.TryAdd(newMethodSignature.ParamTypes, newMethodSignature))
                {
                    // exact param type match, cannot do this
                    output.Add(new TypeException($"Redeclaration of method {newMethodSignature}", newMethodSignature.Position).ToString());
                }
            }
        }

        private void InitializeFields(TypeMap typeMap, List<string> output)
        {
            Fields = ParentClassType is not null ? new(ParentClassType.Fields) : new();

            foreach (VariableDeclaration variableDeclaration in VariableDeclarations)
            {
                bool added = Fields.TryAdd(
                        variableDeclaration.Var.Value,
                        (typeMap.GetType(variableDeclaration.Type.Value, variableDeclaration.Type.Position, output), variableDeclaration.Type.Position)
                        );

                if (!added)
                {
                    output.Add(new TypeException($"Redeclaration of field {variableDeclaration.Var.Value}", variableDeclaration.Position).ToString());
                }
            }
        }


        public MethodSignature GetMatchingSignature(string queryMethodName, TypeList queryMethodArguments, Position position, List<string> output)
        {
            if(!TryGetMethodTypeDict(queryMethodName, out Dictionary<TypeList, MethodSignature> methodTypeDict))
            {
                output.Add(new TypeException($"Class {Name} does not contain a method {queryMethodName}", position).ToString());
                return null;
            }

            // check for exact match
            if (methodTypeDict.TryGetValue(queryMethodArguments, out MethodSignature exactSignatureMatch))
            {
                return exactSignatureMatch;
            }

            // get potential method signatures
            IEnumerable<KeyValuePair<TypeList, MethodSignature>> potentialMethodSignatureEntries = methodTypeDict.Where(d => queryMethodArguments.AreSubtypesOf(d.Key));

            MethodSignature mostPrecise = null;
            foreach (KeyValuePair<TypeList, MethodSignature> methodEntry in potentialMethodSignatureEntries)
            {
                TypeList entryParamTypeList = methodEntry.Key;
                MethodSignature entryMethodSignature = methodEntry.Value;

                // no way to check ambiguity yet
                if (mostPrecise is null)
                {
                    mostPrecise = entryMethodSignature;
                    continue;
                }

                mostPrecise = entryParamTypeList.IsMorePreciseThanNonAmbiguous(mostPrecise.ParamTypes) switch
                {
                    TypeList.MorePreciseResult.True => entryMethodSignature,
                    TypeList.MorePreciseResult.False => mostPrecise,
                    TypeList.MorePreciseResult.Ambigious => ReturnNullAndAddMessage(new TypeException(
                            $"Ambiguous method call with types {queryMethodArguments}\n" +
                            $"Given types do not distinctly match {entryMethodSignature} or {mostPrecise}"
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
                return;
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
                    new Dictionary<OperatorType, Dictionary<TypeBase, TypeBase>>(),
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
            if (scope.HasBreak)
            {
                output.Add(new TypeException("Unreachable code after break", node.Position).ToString());
            }

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

                    CheckAssignment(prog.Statements, [], output);
                    break;
                case BlockStatement blockStatement:

                    EnterScope(scope.ReturnType);

                    foreach (AST statement in blockStatement.Statements)
                    {
                        CheckTypeHelper(statement, output);
                    }

                    ExitScope();

                    break;
                case ClassDefinition classDefinition:
                    CheckClass(classDefinition, output);

                    break;
                case VariableDeclaration varDec:
                    if (Types.TypeDefined(varDec.Var.Value))
                    {
                        output.Add(new TypeException("Variable name cannot be a type", varDec.Var.Position).ToString());
                    }

                    scope.Define(varDec.Var.Value, Types.GetType(varDec.Type, output), varDec.Position, output);

                    break;
                case AssignmentStatement assignmentStatement:
                    TypeBase rightType = GetExpressionType(assignmentStatement.Val, output);

                    if (rightType is null)
                    {
                        output.Add(new TypeException("Assignment Error: Unable to determine type of right side", assignmentStatement.Position).ToString());
                        break;
                    }

                    scope.Assign(assignmentStatement.Var, rightType, assignmentStatement.Position, output);

                    break;
                case IfStatement ifStatement:
                    CheckIfStatement(ifStatement, output);
                    break;
                case WhileStatement whileStatement:
                    CheckWhileStatement(whileStatement, output);
                    break;
                case ReturnStatement returnStatement:
                    if (scope.ReturnType is null)
                    {
                        output.Add(new TypeException("Cannot return outside of a method", returnStatement.Position).ToString());
                        break;
                    }
                    
                    //did not pass in method name recursively
                    CheckMethodReturnType(scope.ReturnType, returnStatement, output);
                    break;
                case BreakStatement breakStatement:
                    if(scope.InLoop)
                    {
                        scope.HasBreak = true;
                    }
                    else
                    {
                        output.Add(new TypeException("Cannot break outside of a loop context", breakStatement.Position).ToString());
                    }

                    break;
                case ExpressionStatement expressionStatement:
                    if (expressionStatement.Expression is not MethodCallExpression)
                    {
                        output.Add(new TypeException("Only assignment, and method call expressions can be used as a statement", expressionStatement.Position).ToString());
                        break;
                    }

                    GetExpressionType(expressionStatement.Expression, output);

                    break;
                case MethodCallExpression methodCallExpression:
                    if (scope.Initializing)
                    {
                        output.Add("Cannot use `this.` method calls in class constructor");
                    }

                    GetExpressionType(methodCallExpression, output);

                    break;
                case null:
                    output.Add(new TypeException("Null node given", new Position(1, 1)).ToString());
                    break;
                default:
                    output.Add(new TypeException($"Type is not supported: {node.GetType()}", node.Position).ToString());
                    break;
            }
        }

        private void CheckIfStatement(IfStatement ifStatement, List<string> output)
        {
            // although not always technically a new scope, 
            // entering a scope here prevents single line
            // break statements as a body from marking the rest
            // of the code in this scope as unreachable

            // restriction of vardec as a single body statement
            // makes this scope acceptable
            EnterScope(scope.ReturnType);

            TypeBase ifGuard = GetExpressionType(ifStatement.Guard, output);

            CheckGuardType(ifStatement, ifGuard, ifStatement.Guard.Position, output);

            AST ifBody = ifStatement.IfBody;

            if (ifBody is VariableDeclaration variableDeclaration)
            {
                output.Add(new TypeException("Cannot have variable declaration inside single if statement body", variableDeclaration.Position).ToString());
            }

            CheckTypeHelper(ifBody, output);


            // if (false)
            if (NodeIsBoolLiteral(ifStatement.Guard, false))
            {
                // unreachable code
                output.Add(new TypeException("Unreachable code in if body", ifBody.Position).ToString());
            }


            if (ifStatement.ElseBody is AST elseBody)
            {
                CheckTypeHelper(elseBody, output);

                // if (true)
                if (NodeIsBoolLiteral(ifStatement.Guard, true))
                {
                    // else would be unreachable
                    output.Add(new TypeException("Unreachable code in else body", elseBody.Position).ToString());
                }
            }
            
            ExitScope();
        }

        private void CheckWhileStatement(WhileStatement whileStatement, List<string> output)
        {
            // see CheckIfStatement for scope reasoning
            EnterScope(scope.ReturnType, true);

            TypeBase whileGuard = GetExpressionType(whileStatement.Guard, output);

            CheckGuardType(whileStatement, whileGuard, whileStatement.Guard.Position, output);

            AST whileBody = whileStatement.Statement;

            // while(false)
            if (NodeIsBoolLiteral(whileStatement.Guard, false))
            {
                // unreachable
                output.Add(new TypeException("Unreachable code in while(false) body", whileBody.Position).ToString());
            }

            if (whileBody is VariableDeclaration variableDeclaration)
            {
                output.Add(new TypeException("Cannot have variable declaration inside single line loop body", variableDeclaration.Position).ToString());
            }

            CheckTypeHelper(whileBody, output);

            ExitScope();
        }

        private bool CheckGuardType(AST astExpression, TypeBase guardType, Position position, List<string> output)
        {
            if (guardType != TypeBase.BooleanPrimitive)
            {
                output.Add(new TypeException($"Invalid guard type for {astExpression}: {guardType}", position).ToString());
                return false;
            }

            return true;
        }

        private bool NodeIsBoolLiteral(AST node, bool valueToCheck)
        {
            return node switch
            {
                BooleanLiteral(bool value, _) when value == valueToCheck => true,
                _ => false
            };
        }

        private bool CheckIfCodePath(IfStatement ifStatement, List<string> output)
        {
            // if (true)
            if (NodeIsBoolLiteral(ifStatement.Guard, true))
            {
                // only check if body
                return CheckCodePath([ifStatement.IfBody], output);
            }

            // if (false)
            if (NodeIsBoolLiteral(ifStatement.Guard, false))
            {
                // will still check else body in case there are more errors
                return CheckElseCodePath(ifStatement.ElseBody, output);
            }


            bool ifPathResult = CheckCodePath([ifStatement.IfBody], output);
            bool elsePathResult = CheckElseCodePath(ifStatement.ElseBody, output);

            return ifPathResult && elsePathResult;

        }

        private bool CheckElseCodePath(AST elseBody, List<string> output)
        {
            List<AST> elseBodyStatements = elseBody != null ? [elseBody] : [];
            return CheckCodePath(elseBodyStatements, output);
        }

        private bool CheckWhileCodePath(WhileStatement whileStatement, List<string> output)
        {
            // while(true) always treated as return unless break
            if (NodeIsBoolLiteral(whileStatement.Guard, true))
            {
                // guaranteed to hit body, check if there is a break
                // if no break, then infinite loop or return in loop, and
                // we can consider either valid
                return !FindLoopBreak([whileStatement.Statement]);
            }

            // return false so that CheckCodePath is forced to check next statement
            return false;
        }

        private bool CheckCodePath(List<AST> statements, List<string> output)
        {
            if (statements.Count == 0)
            {
                return false;
            }

            AST statement = statements.First();

            bool statementReturns = statement switch
            {
                ReturnStatement => true,
                IfStatement ifStatement => CheckIfCodePath(ifStatement, output),
                WhileStatement whileStatement => CheckWhileCodePath(whileStatement, output),
                BlockStatement blockStatement => CheckCodePath(blockStatement.Statements, output),
                _ => false,
            };

            if(statementReturns && statements.Count > 1)
            {
                output.Add(new TypeException($"Unreachable code after return", statement.Position).ToString());
            } 
            else if(!statementReturns)
            {
                return CheckCodePath(statements.Slice(1, statements.Count - 1), output);
            }

            return statementReturns;
        }

        private bool FindBreakInIfStatement(IfStatement ifStatement)
        {
            // if (true)
            if (NodeIsBoolLiteral(ifStatement.Guard, true))
            {
                return FindLoopBreak([ifStatement.IfBody]);
            }

            // if (false)
            if (NodeIsBoolLiteral(ifStatement.Guard, false))
            {
                return FindBreakInElseBody(ifStatement.ElseBody);
            }


            bool ifPathResult = FindLoopBreak([ifStatement.IfBody]);
            bool elsePathResult = FindBreakInElseBody(ifStatement.ElseBody);

            return ifPathResult && elsePathResult;

        }

        private bool FindBreakInElseBody(AST elseBody)
        {
            List<AST> elseBodyStatements = elseBody != null ? [elseBody] : [];
            return FindLoopBreak(elseBodyStatements);
        }


        private bool FindLoopBreak(List<AST> statements)
        {
            if (statements.Count == 0)
            {
                return false;
            }

            AST statement = statements.First();

            bool found = statement switch
            {
                BreakStatement => true,
                IfStatement ifStatement => FindBreakInIfStatement(ifStatement),
                BlockStatement blockStatement => FindLoopBreak(blockStatement.Statements),

                _ => false,
            };

            return found || FindLoopBreak(statements.Slice(1, statements.Count - 1));
        }

        private HashSet<string> CheckIfAssignment(IfStatement ifStatement, HashSet<string> prevAssignSet, List<string> output, List<HashSet<string>> breakAssignmentSets = null)
        {
            CheckExpressionAssignment(ifStatement.Guard, prevAssignSet, output);

            HashSet<string> ifBodySet = CheckAssignment([ifStatement.IfBody], prevAssignSet, output, breakAssignmentSets);
            HashSet<string> elseBodySet = CheckElseAssignment(ifStatement.ElseBody, prevAssignSet, output, breakAssignmentSets);

            // if (true)
            if (NodeIsBoolLiteral(ifStatement.Guard, true))
            {
                return ifBodySet;
            }

            // if (false)
            if (NodeIsBoolLiteral(ifStatement.Guard, false))
            {
                return elseBodySet;
            }

            return ifBodySet.Intersect(elseBodySet).ToHashSet();
        }

        private HashSet<string> CheckElseAssignment(AST elseBody, HashSet<string> prevAssignSet, List<string> output, List<HashSet<string>> breakAssignmentSets = null)
        {
            return elseBody is null ? new() : CheckAssignment([elseBody], prevAssignSet, output, breakAssignmentSets);
        }

        private HashSet<string> CheckWhileAssignment(WhileStatement whileStatement, HashSet<string> prevAssignSet, List<string> output)
        {
            CheckExpressionAssignment(whileStatement.Guard, prevAssignSet, output);
            
            List<HashSet<string>> breakAssignmentSets = new();
            CheckAssignment([whileStatement.Statement], prevAssignSet, output, breakAssignmentSets);

            // while(true)
            if (NodeIsBoolLiteral(whileStatement.Guard, true) && breakAssignmentSets.Count > 0)
            {
                return breakAssignmentSets.Aggregate((acc, next) => acc.Intersect(next).ToHashSet());
            }

            // anything other than while(true) we can't determine if assignment outlives the while statement
            return new();
        }

        private HashSet<string> CheckAssignment(List<AST> statements, HashSet<string> prevAssignSet, List<string> output, List<HashSet<string>> breakAssignmentSets = null)
        {
            if (statements.Count == 0)
            {
                return prevAssignSet;
            }

            AST statement = statements.First();

            if (statement is BreakStatement && breakAssignmentSets is not null)
            {
                breakAssignmentSets.Add(prevAssignSet);
                return prevAssignSet;
            }

            HashSet<string> statementAssignSet;
            switch (statement)
            {
                case VariableDeclaration(_, IdentifiedNode varName, _):
                    statementAssignSet = new HashSet<string>([varName.Value]);
                    break;
                case AssignmentStatement(IdentifiedNode varName, AST val, _):
                    statementAssignSet = new HashSet<string>([varName.Value]);
                    CheckExpressionAssignment(val, prevAssignSet, output);
                    break;
                case IfStatement ifStatement:
                    statementAssignSet = CheckIfAssignment(ifStatement, prevAssignSet, output, breakAssignmentSets);
                    break;
                case WhileStatement whileStatement:
                    statementAssignSet = CheckWhileAssignment(whileStatement, prevAssignSet, output);
                    break;
                case BlockStatement(List<AST> blockStatements, _):
                    statementAssignSet = CheckAssignment(blockStatements, prevAssignSet, output, breakAssignmentSets);
                    break;
                case ReturnStatement(AST val, _):
                    statementAssignSet = new();
                    CheckExpressionAssignment(val, prevAssignSet, output);
                    break;
                case ExpressionStatement(AST expression, _):
                    statementAssignSet = new();
                    CheckExpressionAssignment(expression, prevAssignSet, output);
                    break;
                default:
                    statementAssignSet = new();
                    break;
            };


            if (statement is VariableDeclaration)
            {
                // remove var from assign set as it was just redeclared
                statementAssignSet = prevAssignSet.Except(statementAssignSet).ToHashSet();
            }
            else
            {
                statementAssignSet = prevAssignSet.Union(statementAssignSet).ToHashSet();
            }

            HashSet<string> nextStatementAssignSet = CheckAssignment(statements.Slice(1, statements.Count - 1), statementAssignSet, output, breakAssignmentSets);
            return statementAssignSet.Union(nextStatementAssignSet).ToHashSet();
        }

        private void CheckExpressionAssignment(AST expression, HashSet<string> assignSet, List<string> output)
        {
            switch (expression)
            {
                case IdentifiedNode identifiedNode:
                    if (!assignSet.Contains(identifiedNode.Value))
                    {
                        output.Add(new TypeException($"Cannot use unassigned variable {identifiedNode.Value} in an expression", identifiedNode.Position).ToString());
                    }
                    break;
                case BinaryExpression(AST left, _, AST right, _):
                    CheckExpressionAssignment(left, assignSet, output);
                    CheckExpressionAssignment(right, assignSet, output);
                    break;
                case MethodCallExpression(_, _, List<AST> arguments, _):
                    arguments.ForEach(arg => CheckExpressionAssignment(arg, assignSet, output));
                    break;
                case NewObjectExpression(_, List<AST> arguments, _):
                    arguments.ForEach(arg => CheckExpressionAssignment(arg, assignSet, output));
                    break;
            }
        }

        private void CheckClass(ClassDefinition classDefinition, List<string> output)
        {
            ClassType classType = Types.GetTypeAs<ClassType>(classDefinition.Name.Value, classDefinition.Name.Position, output);

            if (classType is null)
            {
                return;
            }

            // enter class scope
            EnterScope();

            // hacky way of defining the type of "this"
            scope.Define("this", classType, classDefinition.Position, output);

            // add fields to scope
            foreach ((string name, (TypeBase type, Position position)) in classType.Fields)
            {
                scope.DefineField(name, type, position, output);
            }

            Constructor constructor = (Constructor)classDefinition.Constructor;

            // includes inherited fields, methods need access to all
            HashSet<string> fieldAssignSet = new(classType.Fields.Keys);
            // constructor needs to initialize *local* class fields
            HashSet<string> localFieldAssignSet = new(
                                                    classDefinition.VariableDeclarations
                                                    .Select(vd => (VariableDeclaration)vd)
                                                    .Select(vd => vd.Var.Value)
                                                    );
            // class constructor can use inherited fields
            HashSet<string> inheritedFieldAssignSet = fieldAssignSet.Except(localFieldAssignSet).ToHashSet();

            CheckClassConstructor(constructor, classType, localFieldAssignSet, inheritedFieldAssignSet, output);


            foreach (MethodDefinition methodDefinition in classDefinition.MethodDefinitions)
            {
                CheckClassMethod(methodDefinition, fieldAssignSet, output);
            }

            // exit class scope
            ExitScope();
        }

        private void CheckClassMethod(MethodDefinition methodDefinition, HashSet<string> fieldAssignSet, List<string> output)
        {            
            BlockStatement body = methodDefinition.Body as BlockStatement;

            TypeBase methodReturnType = TypeBase.VoidPrimitive;
            if (methodDefinition.ReturnType is not null)
            {
                methodReturnType = Types.GetType(methodDefinition.ReturnType.Value, methodDefinition.ReturnType.Position, output);
            }

            EnterScope(methodReturnType);

            HashSet<string> paramAssignSet = DefineAndGetParamAssignSet(methodDefinition.Parameters, output);

            // validate types first
            for (int i = 0; i < body.Statements.Count; i++)
            {
                CheckTypeHelper(body.Statements[i], output);
            }

            HashSet<string> fullAssignSet = fieldAssignSet.Union(paramAssignSet).ToHashSet();
            CheckAssignment(body.Statements, fullAssignSet, output);

            // only check code path if return is not void
            if (methodReturnType != TypeBase.VoidPrimitive &&
                !CheckCodePath(body.Statements, output))
            {
                output.Add(new TypeException($"Method {methodDefinition.Name.Value} expects return value of type {methodReturnType} but got none", methodDefinition.Position).ToString());
            }

            ExitScope();
        }

        private void CheckMethodReturnType(TypeBase methodReturnType, ReturnStatement returnStatement, List<string> output)
        {
            TypeBase returnExpressionType = TypeBase.VoidPrimitive;
            if (returnStatement.Val is not null)
            {
                returnExpressionType = GetExpressionType(returnStatement.Val, output);
            }

            if (returnExpressionType is null)
            {
                output.Add(new TypeException($"Cannot return unknown expression type", returnStatement.Val.Position).ToString());
            }
            else if (methodReturnType == TypeBase.VoidPrimitive && returnStatement.Val is not null)
            {
                output.Add(new TypeException($"Attempting to explicitly return a void value. Try `return;` instead", returnStatement.Position).ToString());
            }
            else if (!returnExpressionType.CanBeAssignedTo(methodReturnType))
            {
                output.Add(new TypeException($"Return type {returnExpressionType} does not match method return type", returnStatement.Val.Position).ToString());
            }
        }

        private void CheckSuperCall(Constructor constructor, ClassType classType, List<string> output)
        {
            // insert empty super call if a super isnt provided and parent is Object
            List<AST> superArguments = constructor.SuperArguments is null && classType.ParentClassType == TypeBase.ObjectBuiltIn
                                       ? []
                                       : constructor.SuperArguments;

            if (superArguments is null)
            {
                output.Add(new TypeException($"Constructor for class {classType.Name} is missing super call", constructor.Position).ToString());
            }
            else
            {
                TypeList superCallTypes = GetExpressionTypeList(superArguments, output);
                if (!superCallTypes.AreSubtypesOf(classType.ParentClassType.ConstructorTypes))
                {
                    output.Add(new TypeException($"Super call arguments in class {classType.Name} are not compatible with parent class {classType.ParentClassType} constructor", constructor.Position).ToString());
                }
            }

        }

        private void CheckClassConstructor(Constructor constructor, ClassType classType, HashSet<string> localFieldAssignSet, HashSet<string> inheritedFieldAssignSet, List<string> output)
        {
            // enter constructor scope
            EnterScope(initializing:true);

            HashSet<string> paramAssignSet = DefineAndGetParamAssignSet(constructor.Parameters, output);

            // check super call
            if (classType.ParentClassType is not null)
            {
                CheckSuperCall(constructor, classType, output);
            }
            else if (constructor.SuperArguments is not null)
            {
                output.Add(new TypeException($"Class {classType} attempts to call a super constructor when it does not inherit any class", constructor.Position).ToString());
            }

            // type check statements
            foreach (AST statement in constructor.Statements)
            {
                CheckTypeHelper(statement, output);
            }

            // check assignment
            HashSet<string> initialAssignSet = paramAssignSet.Union(inheritedFieldAssignSet).ToHashSet();
            HashSet<string> constructorAssignSet = CheckAssignment(constructor.Statements, initialAssignSet, output);

            // class local field set should be subset of constructor assign set
            if (!localFieldAssignSet.IsSubsetOf(constructorAssignSet))
            {
                output.Add(new TypeException($"Constructor of class {classType.Name} does not initialize all local fields", constructor.Position).ToString());
            }

            // exit constructor scope
            ExitScope();
        }

        private HashSet<string> DefineAndGetParamAssignSet(List<AST> astVariableDeclarations, List<string> output)
        {
            HashSet<string> paramAssignSet = new();
            foreach (AST astVariableDeclaration in astVariableDeclarations)
            {
                VariableDeclaration variableDeclaration = (VariableDeclaration)astVariableDeclaration;

                string varName = variableDeclaration.Var.Value;

                scope.Define(varName, Types.GetType(variableDeclaration.Type, output), variableDeclaration.Position, output);
                paramAssignSet.Add(varName);
            }

            return paramAssignSet;
        }

        private void EnterScope(TypeBase returnType = null, bool inLoop = false, bool initializing = false)
        {
            // or's are in place to persist loop or initializing states when entering a new scope
            scope = new Scope(scope, returnType, inLoop || scope.InLoop, initializing || scope.Initializing);
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
                ThisExpression(Position position) => scope.LookUp("this", position, output)?.Type,
                PrintLnStatement printLnStatement => DerivePrintLnStatementType(printLnStatement, output),
                BinaryExpression binaryExpression => DeriveBinaryExpressionStmt(binaryExpression, output),
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

        private TypeBase DeriveBinaryExpressionStmt(BinaryExpression binaryExpression, List<string> output)
        {
            return binaryExpression.OperatorType switch
            {
                OperatorType.Add or
                OperatorType.Subtract or
                OperatorType.Multiply or
                OperatorType.Divide or
                OperatorType.LessThan or
                OperatorType.Equal or
                OperatorType.NotEqual => CheckAndGetResultTypeOfBinaryExpression(binaryExpression, output),                                            
                _ => throw new Exception("Something went horribly wrong...") //should never happen
            };
        }

        private TypeBase CheckAndGetResultTypeOfBinaryExpression(BinaryExpression binaryExpression, List<string> output)
        {
            TypeBase leftType = GetExpressionType(binaryExpression.Left, output);
            TypeBase rightType = GetExpressionType(binaryExpression.Right, output);

            if (leftType is null || rightType is null)
            {
                output.Add(new TypeException($"Can not derive {binaryExpression.OperatorType} type with unknown operand type", binaryExpression.Position).ToString());
                return null;
            }

            //if we are doing equality comparison check if they can be assigned otherwise continue
            if (binaryExpression.OperatorType == OperatorType.Equal 
                || binaryExpression.OperatorType == OperatorType.NotEqual)
            {
                if (!leftType.CanBeAssignedTo(rightType) && !rightType.CanBeAssignedTo(leftType))
                {
                    output.Add(new TypeException($"Can not {binaryExpression.OperatorType} with Type {leftType.Name} and Type {rightType.Name}", binaryExpression.Position).ToString());
                    return null;
                }

                return TypeBase.BooleanPrimitive;
            }

            //check if ops are compatible
            if (leftType.CompatibleOperatorTypes.TryGetValue(binaryExpression.OperatorType, out var leftMap)
                && leftMap.TryGetValue(rightType, out TypeBase resultType))
            {
                return resultType;
            }

            output.Add(new TypeException($"Can not {binaryExpression.OperatorType} with Type {leftType.Name} and Type {rightType.Name}", binaryExpression.Position).ToString());
            return null;
        }

        private TypeList GetExpressionTypeList(List<AST> nodes, List<string> output)
        {
            return new TypeList(nodes.Select(n => GetExpressionType(n, output)).ToImmutableList());
        }

        private TypeBase DerivePrintLnStatementType(PrintLnStatement printLnStatement, List<string> output)
        {
            TypeBase argType = GetExpressionType(printLnStatement.Argument, output);
            if (argType is null)
            {
                output.Add(new TypeException($"Cannot call println with an unknown typed argument", printLnStatement.Position).ToString());
            }

            return TypeBase.VoidPrimitive;
        }

        private TypeBase DeriveIdentifiedNodeExpressionType(IdentifiedNode identifiedNode, List<string> output)
        {
            VarInfo varInfo = scope.LookUp(identifiedNode.Value, identifiedNode.Position, output);
            if (varInfo is null)
            {
                output.Add(new TypeException($"Cannot determine type of undefined variable {identifiedNode.Value}", identifiedNode.Position).ToString());
                return null;
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
            Position targetPosition = methodCallExpression.Target.Position;

            switch (targetType)
            {
                case ClassType targetClassType:
                    MethodSignature matchingSignature = targetClassType.GetMatchingSignature(
                        methodCallExpression.Name,
                        GetExpressionTypeList(methodCallExpression.Arguments, output),
                        methodCallExpression.Position,
                        output
                    );

                    if (matchingSignature is null)
                    {
                        return null;
                    }

                    methodCallExpression.AnnotatedMethodName = matchingSignature.MethodName;

                    return matchingSignature.ReturnType;
                case PrimitiveType primitiveType:
                    output.Add(new TypeException($"Cannot call methods on primitive type {primitiveType}", targetPosition).ToString());
                    return null;
                default:
                    output.Add(new TypeException($"Cannot call methods on unknown expression {methodCallExpression.Target}", targetPosition).ToString());
                    return null;
            }
        }
    }
}
