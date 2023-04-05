﻿using System;
using System.Collections.Generic;
using Phantasma.Tomb.CodeGen;
using Phantasma.Tomb.AST.Expressions;
using Phantasma.Tomb.AST.Declarations;
using Phantasma.Core.Domain;

namespace Phantasma.Tomb.AST
{
    public class MethodParameter: Node
    {
        public string Name { get; private set; }
        public VarType Type { get; internal set; }

        public Func<CodeGenerator, Scope, Expression, Register> Callback;

        public MethodParameter(string name, VarType type)
        {
            Name = name;
            Type = type;
        }

        public MethodParameter(string name, VarKind kind) : this(name, VarType.Find(kind))
        {
        }

        public override string ToString()
        {
            return $"{Name}:{Type}";
        }

        public override void Visit(Action<Node> callback)
        {
            callback(this);
        }

        public override bool IsNodeUsed(Node node)
        {
            return node == this;
        }

        public void SetParameterCallback(Func<CodeGenerator, Scope, Expression, Register> callback)
        {
            this.Callback = callback;
        }
    }

    public enum MethodImplementationType
    {
        ExtCall,
        ContractCall,
        LocalCall,
        Custom
    }

    public class MethodInterface
    {
        public string Name;
        public LibraryDeclaration Library;
        public MethodKind Kind;
        public VarType ReturnType;
        public MethodParameter[] Parameters;
        public bool IsPublic;
        public string Alias;
        public string Contract;
        public MethodImplementationType Implementation;
        public Func<CodeGenerator, Scope, MethodCallExpression, Register> PreCallback;
        public Func<CodeGenerator, Scope, MethodCallExpression, Register, Register> PostCallback;

        public int StartAsmLine;
        public int EndAsmLine;

        public bool IsMulti; // if true, method will emit multiple return values in the vm result stack
        public bool IsBuiltin;

        public MethodInterface(LibraryDeclaration library, MethodImplementationType implementation, string name, bool isPublic, MethodKind kind, VarType returnType, MethodParameter[] parameters, string alias = null, bool isMulti = false, bool isBuiltin = false) 
        {
            this.Name = name;
            this.Library = library;
            this.Implementation = implementation;
            this.Kind = kind;
            this.IsPublic = isPublic; 
            this.ReturnType = returnType;
            this.Parameters = parameters;
            this.IsMulti = isMulti;
            this.IsBuiltin = isBuiltin;

            this.PreCallback = null;
            this.PostCallback = null;
            
            this.Contract = this.Library.Name;

            if (alias != null)
            {
                this.Alias = alias;
            }
            else
            {
                this.Alias = $"{char.ToUpper(this.Name[0])}{this.Name.Substring(1)}";
                if (implementation == MethodImplementationType.ExtCall)
                {
                    this.Alias = this.Library.Name + '.' + this.Alias;
                }
            }            
        }

        public MethodInterface Clone(LibraryDeclaration targetLibrary)
        {
            var method = this;
            var parameters = new List<MethodParameter>();

            foreach (var parameter in method.Parameters)
            {
                var entry = new MethodParameter(parameter.Name, parameter.Type);
                entry.Callback = parameter.Callback;
                parameters.Add(entry);
            }

            var newMethod = new MethodInterface(targetLibrary, method.Implementation, method.Name, method.IsPublic, method.Kind, method.ReturnType, parameters.ToArray(), method.Alias, method.IsMulti, method.IsBuiltin);
            newMethod.Contract = method.Contract;
            newMethod.PreCallback = method.PreCallback;
            newMethod.PostCallback = method.PostCallback;

            return newMethod;
        }

        public override string ToString()
        {
            return $"method {Name}:{ReturnType}";
        }

        public MethodInterface SetContract(string contract)
        {
            this.Contract = contract;
            return this;
        }

        public MethodInterface SetAlias(string alias)
        {
            this.Alias = alias;
            return this;
        }

        public MethodInterface SetPreCallback(Func<CodeGenerator, Scope, MethodCallExpression, Register> callback)
        {
            this.PreCallback = callback;
            return this;
        }

        public MethodInterface SetPostCallback(Func<CodeGenerator, Scope, MethodCallExpression, Register, Register> callback)
        {
            this.PostCallback = callback;
            return this;
        }

        public MethodInterface SetParameterCallback(string name, Func<CodeGenerator, Scope, Expression, Register> callback)
        {
            foreach (var parameter in Parameters)
            {
                if (parameter.Name == name)
                {
                    parameter.SetParameterCallback(callback);
                    return this;
                }
            }

            throw new Exception($"method {this.Library.Name}.{this.Name} contains no parameter called {name}");
        }

        public static VMType ConvertType(VarType type)
        {
            return ConvertType(type.Kind);
        }

        public static VMType ConvertType(VarKind kind)
        {
            switch (kind)
            {
                case VarKind.Address:
                    return VMType.Object;

                case VarKind.Bytes:
                case VarKind.Hash:
                    return VMType.Bytes;

                case VarKind.Bool:
                    return VMType.Bool;

                case VarKind.Enum:
                case VarKind.Type:
                    return VMType.Enum;

                case VarKind.Method:
                case VarKind.Number:
                case VarKind.Decimal:
                case VarKind.Task:
                    return VMType.Number;

                case VarKind.String:
                    return VMType.String;

                case VarKind.Timestamp:
                    return VMType.Timestamp;

                case VarKind.None:
                    return VMType.None;

                case VarKind.Struct:
                case VarKind.Array:
                    return VMType.Struct;

                case VarKind.Module:
                    return VMType.Bytes;

                default:
                    throw new System.Exception("Not a valid ABI return type: " + kind);
            }
        }

    }
}
