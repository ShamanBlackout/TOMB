﻿using Phantasma.Domain;
using Phantasma.Tomb.CodeGen;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Phantasma.Tomb.AST.Declarations
{
    public class LibraryDeclaration : Declaration
    {
        public Dictionary<string, MethodInterface> methods = new Dictionary<string, MethodInterface>();

        public LibraryDeclaration(Scope parentScope, string name) : base(parentScope, name)
        {
        }

        public void GenerateCode(CodeGenerator output)
        {
            // DO NOTHING
        }

        public MethodInterface AddMethod(string name, MethodImplementationType convention, VarKind returnKind, MethodParameter[] parameters, string alias = null)
        {
            return AddMethod(name, convention, VarType.Find(returnKind), parameters, alias);
        }

        public MethodInterface AddMethod(string name, MethodImplementationType convention, VarType returnType, MethodParameter[] parameters, string alias = null)
        {
            if (!returnType.IsWeird && Compiler.Instance != null)
            {
                var vmType = MethodInterface.ConvertType(returnType);

                if (!ValidationUtils.IsValidMethod(name, vmType))
                {
                    throw new CompilerException("invalid method name: " + name);
                }
            }

            var method = new MethodInterface(this, convention, name, true, MethodKind.Method, returnType, parameters, alias);
            methods[name] = method;

            return method;
        }

        public MethodInterface FindMethod(string name, bool required = true)
        {
            /*if (name != name.ToLower())
            {
                throw new CompilerException(parser, "invalid method name: " + name);
            }*/

            if (methods.ContainsKey(name))
            {
                return methods[name];
            }

            if (required)
            {
                throw new CompilerException("unknown method: " + name);
            }

            return null;
        }

        public override string ToString()
        {
            return $"library {Name}";
        }

        public override void Visit(Action<Node> callback)
        {
            callback(this);
        }

        public override bool IsNodeUsed(Node node)
        {
            return node == this;
        }

        public GenericLibraryDeclaration MakeGenericLib(string key, string name, IEnumerable<VarType> generics)
        {
            key = $"{this.Name}<{key}>";

            if (_genericCache.ContainsKey(key))
            {
                return _genericCache[key];
            }
            
            var result = new GenericLibraryDeclaration(this.ParentScope, name, generics);
            foreach (var method in this.methods.Values)
            {
                var newMethod = method.Clone(result);
                result.methods[method.Name] = newMethod;
            }

            _genericCache[key] = result;
            return result;
        }

        private Dictionary<string, GenericLibraryDeclaration> _genericCache = new Dictionary<string, GenericLibraryDeclaration>();

        public LibraryDeclaration PatchMap(MapDeclaration mapDecl)
        {
            var key = $"{mapDecl.KeyKind},{mapDecl.ValueKind}";
            return this.MakeGenericLib(key, this.Name, new[] { mapDecl.KeyKind, mapDecl.ValueKind });           
        }

        public LibraryDeclaration PatchList(ListDeclaration listDecl)
        {
            var key = listDecl.ValueKind.ToString();
            return this.MakeGenericLib(key, this.Name, new[] { listDecl.ValueKind });
        }

        public LibraryDeclaration PatchSet(SetDeclaration setDecl)
        {
            var key = setDecl.ValueKind.ToString();
            return this.MakeGenericLib(key, this.Name, new[] { setDecl.ValueKind });
        }
    }

    public class GenericLibraryDeclaration : LibraryDeclaration
    {
        public readonly VarType[] Generics;

        public GenericLibraryDeclaration(Scope parentScope, string name, IEnumerable<VarType> generics) : base(parentScope, name)
        {
            this.Generics = generics.ToArray();
        }
    }

}
