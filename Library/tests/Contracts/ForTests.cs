using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Phantasma.Core.Domain;
using Phantasma.Core.Domain.Execution.Enums;
using Phantasma.Core.Utils;
using Phantasma.Tomb;
using Phantasma.Tomb.Compilers;

namespace TOMBLib.Tests.Contracts;

public class ForTests
{
    [Test]
    public void ForLoop()
    {
        var sourceCode =
            @"
contract test {
    public countStuff():number {
        local x:number = 0;
        for (local i=0; i<9; i+=1)
        {
            x+=2;
        }
        return x;
    }
}";

        var parser = new TombLangCompiler();
        var contract = parser.Process(sourceCode).First();

        var storage = new Dictionary<byte[], byte[]>(new ByteArrayComparer());

        var countStuff = contract.abi.FindMethod("countStuff");
        Assert.IsNotNull(countStuff);

        var vm = new TestVM(contract, storage, countStuff);
        var result = vm.Execute();
        Assert.IsTrue(result == ExecutionState.Halt);

        Assert.IsTrue(vm.Stack.Count == 1);
        var val = vm.Stack.Pop().AsNumber();
        Assert.IsTrue(val == 18);
    }
    
    [Test]
    public void ForLoopOutsideVariable()
    {
        var sourceCode =
            @"
contract test {
    public countStuff():number {
        local x:number = 0;
        for (local y = 0; y<10; y++)
        {
            // do nothing here.
        }
        x = y;
        return x;
    }
}";

        var parser = new TombLangCompiler();
        var contract = parser.Process(sourceCode).First();

        var storage = new Dictionary<byte[], byte[]>(new ByteArrayComparer());

        var countStuff = contract.abi.FindMethod("countStuff");
        Assert.IsNotNull(countStuff);

        var vm = new TestVM(contract, storage, countStuff);
        var result = vm.Execute();
        Assert.IsTrue(result == ExecutionState.Halt);

        Assert.IsTrue(vm.Stack.Count == 1);
        var val = vm.Stack.Pop().AsNumber();
        Assert.IsTrue(val == 10);
    }
    
    [Test]
    public void ForLoopInsideVariables()
    {
        var sourceCode =
            @"
contract test {
    public countStuff():number {
        local x:number = 0;
        for (local y = 0; y<10; y++)
        {
            local z = y;
        }
        x = z;
        return x;
    }
}";

        var parser = new TombLangCompiler();
        Assert.Catch<CompilerException>(() =>
        {
            var contract = parser.Process(sourceCode).First();
        });
    }
}