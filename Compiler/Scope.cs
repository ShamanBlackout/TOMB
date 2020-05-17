﻿using System;
using System.Collections.Generic;

namespace Phantasma.Tomb.Compiler
{
    public sealed class Scope
    {
        public readonly Scope Parent;
        public readonly Contract Root;
        public readonly string Name;

        public int Level
        {
            get
            {
                if (Parent != null)
                {
                    return Parent.Level + 1;
                }

                return 0;
            }
        }

        public Scope(Scope parent, string name, MethodParameter[] parameters)
        {
            this.Parent = parent;
            this.Root = parent.Root;
            this.Name = name;

            foreach (var entry in parameters)
            {
                var varDecl = new VarDeclaration(this, entry.Name, entry.Kind, VarStorage.Argument);
                this.AddVariable( varDecl);
            }
        }

        public Scope(Scope parent, string name): this(parent, name, new MethodParameter[0])
        {
        }

        public Scope(Contract contract)
        {
            this.Parent = null;
            this.Root = contract;
            this.Name = contract.Name;
        }

        public override string ToString()
        {
            if (Parent != null)
            {
                return Parent.ToString() + "=>" + Name;
            }

            return Name;
        }

        public readonly Dictionary<string, VarDeclaration> Variables = new Dictionary<string, VarDeclaration>();
        public readonly Dictionary<string, ConstDeclaration> Constants = new Dictionary<string, ConstDeclaration>();
        public readonly List<MethodInterface> Methods = new List<MethodInterface>();

        public void AddVariable(VarDeclaration decl)
        {
            if (Variables.ContainsKey(decl.Name))
            {
                throw new CompilerException("duplicated declaration: " + decl.Name);
            }

            if (decl.Name == decl.Name.ToUpper())
            {
                throw new CompilerException("invalid variable name: " + decl.Name);
            }

            Variables[decl.Name] = decl;
        }

        public void AddConstant(ConstDeclaration decl)
        {
            if (Constants.ContainsKey(decl.Name))
            {
                throw new CompilerException("duplicated declaration: " + decl.Name);
            }

            if (decl.Name != decl.Name.ToUpper())
            {
                throw new CompilerException("invalid constant name: " + decl.Name);
            }

            Constants[decl.Name] = decl;
        }

        public VarDeclaration FindVariable(string name, bool required = true)
        {
            if (Variables.ContainsKey(name))
            {
                return Variables[name];
            }

            if (Parent != null)
            {
                return Parent.FindVariable(name, required);
            }

            if (required)
            {
                throw new CompilerException("variable not declared: " + name);
            }

            return null;
        }

        public ConstDeclaration FindConstant( string name, bool required = true)
        {
            if (Constants.ContainsKey(name))
            {
                return Constants[name];
            }

            if (Parent != null)
            {
                return Parent.FindConstant(name, required);
            }

            if (required)
            {
                throw new CompilerException("constant not declared: " + name);
            }

            return null;
        }

        private Scope previousScope;

        public void Enter(CodeGenerator output)
        {
            previousScope = CodeGenerator.currentScope;
            CodeGenerator.currentScope = this;

            Console.WriteLine("entering " + this.Name);

            /*foreach (var variable in this.Variables.Values)
            {
                variable.Register = Parser.Instance.AllocRegister(output, variable, variable.Name);
            }*/
        }

        public void Leave(CodeGenerator output)
        {
            Console.WriteLine("leaving " + this.Name);

            foreach (var variable in this.Variables.Values)
            {
                if (variable.Storage == VarStorage.Global)
                {
                    continue;
                }

                if (variable.Register == null)
                {
                    throw new CompilerException("unused variable: " + variable.Name);
                }

                Parser.Instance.DeallocRegister(variable.Register);
                variable.Register = null;
            }

            CodeGenerator.currentScope = previousScope;
        }
    }
}