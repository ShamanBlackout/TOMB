using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Phantasma.Core.Cryptography;
using Phantasma.Core.Domain;
using Phantasma.Core.Utils;
using Phantasma.Tomb;
using Phantasma.Tomb.Compilers;

namespace Tests.Contracts;

public class MethodTests
{
    [Test]
    public void DuplicatedMethodNames()
    {
        var sourceCode =
            @"
contract test {
    public testme(x:number): number {
         return 5;
    }

    public testme(x:number): string {
         return ""zero"";
     }}";

        var parser = new TombLangCompiler();

        Assert.Catch<CompilerException>(() =>
        {
            var contract = parser.Process(sourceCode).First();
        });
    }

    [Test]
    public void TooManyArgs()
    {
        var sourceCode = @"
contract arrays {
    import Array;

	public mycall(x:number):number {
        return x+ 1;
    }

	public something():number {
		return this.mycall(2, 3); // extra argument here, should not compile		
	}	
}
";

        var parser = new TombLangCompiler();

        Assert.Catch<CompilerException>(() =>
        {
            var contract = parser.Process(sourceCode).First();
        });
    }
    
    [Test]
    public void TestLocalCallViaThis()
    {
        var sourceCode =
            @"
contract test {
    private sum(x:number, y:number) : number 
    { return x + y; } 

    public fetch(val:number) : number
    { 
        return this.sum(val, 1);
    }
}";

        var parser = new TombLangCompiler();
        var contract = parser.Process(sourceCode).First();

        var storage = new Dictionary<byte[], byte[]>(new ByteArrayComparer());

        TestVM vm;

        var keys = PhantasmaKeys.Generate();

        // call fetch
        var fetch = contract.abi.FindMethod("fetch");
        Assert.IsNotNull(fetch);

        vm = new TestVM(contract, storage, fetch);
        vm.Stack.Push(VMObject.FromObject(10));
        var state = vm.Execute();
        Assert.IsTrue(state == ExecutionState.Halt);
        var result = vm.Stack.Pop().AsNumber();

        Assert.IsTrue(result == 11);
    }
    
    [Test]
    public void TestContractCallViaCallMethod()
    {
        var sourceCode =
            @"
contract test {
    import Call;

    private sum(x:number, y:number) : number 
    { return x + y; } 

    public fetch(val:number) : number
    { 
        return Call.method<number>(sum, val, 1);
    }
}";

        var parser = new TombLangCompiler();
        var contract = parser.Process(sourceCode).First();

        var storage = new Dictionary<byte[], byte[]>(new ByteArrayComparer());

        TestVM vm;

        var keys = PhantasmaKeys.Generate();

        // call fetch
        var fetch = contract.abi.FindMethod("fetch");
        Assert.IsNotNull(fetch);

        vm = new TestVM(contract, storage, fetch);
        vm.Stack.Push(VMObject.FromObject(10));
        var state = vm.Execute();
        Assert.IsTrue(state == ExecutionState.Halt);
        var result = vm.Stack.Pop().AsNumber();

        Assert.IsTrue(result == 11);
    }
}