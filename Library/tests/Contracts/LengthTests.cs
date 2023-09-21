using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NUnit.Framework;
using Phantasma.Core.Cryptography;
using Phantasma.Core.Domain;
using Phantasma.Core.Domain.Execution.Enums;
using Phantasma.Core.Domain.VM;
using Phantasma.Core.Domain.VM.Enums;
using Phantasma.Core.Utils;
using Phantasma.Tomb.Compilers;

namespace TOMBLib.Tests.Contracts;

public class LengthTests
{
    [Test]
    public void DirectLength()
    {
        var sourceCode = new string[]
        {
            "contract test{",
            "public returnLength() : number	{",
            "local myStr = \"Hello\";",
            "return myStr.length();",
            "}}"
        };

        var parser = new TombLangCompiler();
        var contract = parser.Process(sourceCode).First();

        var storage = new Dictionary<byte[], byte[]>(new ByteArrayComparer());

        TestVM vm;

        var keys = PhantasmaKeys.Generate();

        // call returnType
        var returnType = contract.abi.FindMethod("returnLength");
        Assert.IsNotNull(returnType);

        vm = new TestVM(contract, storage, returnType);
        var result = vm.Execute();
        Assert.IsTrue(result == ExecutionState.Halt);

        Assert.IsTrue(storage.Count == 0);

        Assert.IsTrue(vm.Stack.Count == 1);

        var obj = vm.Stack.Pop();
        var vmObject = obj.AsNumber();
        BigInteger helloLength = "Hello".Length; 

        Assert.AreEqual(vmObject, helloLength);
    }
    
    [Test]
    public void TestLengthWithArgument()
    {
        var sourceCode =
            @"
contract test{         
    import Runtime;          
    public getLength(myStr:string):number {
        Runtime.expect(myStr.length() >= 5, ""hello world"");
        return myStr.length();
    }
}";

        var parser = new TombLangCompiler();
        var contract = parser.Process(sourceCode).First();

        var storage = new Dictionary<byte[], byte[]>(new ByteArrayComparer());

        TestVM vm;

        var method = contract.abi.FindMethod("getLength");
        Assert.IsNotNull(method);

        vm = new TestVM(contract, storage, method);
        vm.Stack.Push(VMObject.FromObject("Hello"));
        var result = vm.Execute();
        Assert.IsTrue(result == ExecutionState.Halt);

        Assert.IsTrue(vm.Stack.Count == 1);

        var obj = vm.Stack.Pop();
        var lengthObject = obj.AsNumber();
        BigInteger helloLength = "Hello".Length;
        Assert.AreEqual(lengthObject, helloLength);
    }
    
    [Test]
    public void TestLengthWithinIFStatement()
    {
        var sourceCode =
            @"
contract test{              
    public getLength(myStr:string):number {
        if ( myStr.length() >= 5 ) {
            return myStr.length();
        }
        return myStr.length() + 5;
    }
}";

        var parser = new TombLangCompiler();
        var contract = parser.Process(sourceCode).First();

        var storage = new Dictionary<byte[], byte[]>(new ByteArrayComparer());

        TestVM vm;

        // First test with Hello
        var method = contract.abi.FindMethod("getLength");
        Assert.IsNotNull(method);

        vm = new TestVM(contract, storage, method);
        vm.Stack.Push(VMObject.FromObject("Hello"));
        var result = vm.Execute();
        Assert.IsTrue(result == ExecutionState.Halt);

        Assert.IsTrue(vm.Stack.Count == 1);

        var obj = vm.Stack.Pop();
        var lengthObject = obj.AsNumber();
        BigInteger helloLength = "Hello".Length;
        Assert.AreEqual(lengthObject, helloLength);
        
        // 2nd test with hi
        vm = new TestVM(contract, storage, method);
        vm.Stack.Push(VMObject.FromObject("Hi"));
        result = vm.Execute();
        Assert.IsTrue(result == ExecutionState.Halt);

        Assert.IsTrue(vm.Stack.Count == 1);

        obj = vm.Stack.Pop();
        lengthObject = obj.AsNumber();
        BigInteger hiLength = "hi".Length;
        Assert.AreNotEqual(lengthObject, hiLength);
        Assert.AreEqual(lengthObject, hiLength + 5);
    }
}