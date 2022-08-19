﻿using Phantasma.Tomb.AST.Declarations;
using Phantasma.Tomb.CodeGen;

using System;

namespace Phantasma.Tomb.AST.Expressions
{
    public class ArrayElementExpression : Expression
    {
        public VarDeclaration decl;
        public Expression indexExpression;

        public ArrayElementExpression(Scope parentScope, VarDeclaration declaration, Expression indexExpression) : base(parentScope)
        {
            this.decl = declaration;
            this.indexExpression = indexExpression;
        }

        public override string ToString()
        {
            return decl.ToString();
        }

        public override Register GenerateCode(CodeGenerator output)
        {
            if (decl.Register == null)
            {
                throw new CompilerException(this, $"var not initialized:" + decl.Name);
            }

            var dstReg = Compiler.Instance.AllocRegister(output, this);
            var idxReg = indexExpression.GenerateCode(output);

            output.AppendLine(this, $"GET {decl.Register} {dstReg} {idxReg}");

            Compiler.Instance.DeallocRegister(ref idxReg);

            return dstReg;
        }

        public override void Visit(Action<Node> callback)
        {
            callback(this);
            decl.Visit(callback);
            indexExpression.Visit(callback);
        }

        public override bool IsNodeUsed(Node node)
        {
            return (node == this) || node == decl;
        }

        public override VarType ResultType => decl.Type is ArrayVarType ? ((ArrayVarType)decl.Type).elementType : VarType.Find(VarKind.Unknown);
    }

}
