using NUnit.Framework;
using Phantasma.Tomb.AST;
using Phantasma.Tomb.AST.Declarations;
using Phantasma.Tomb.CodeGen;
using Phantasma.Tomb.Lexers;

namespace Tests.AST.Declaration;

public class ConstDeclarationTest
{
    [SetUp]
    public void Setup()
    {
        TombLangLexer lexer = new TombLangLexer();
    }

    [Test]
    public void ConstDeclaration_Constructor_SetsProperties()
    {
        // Arrange
        var module = new Contract("myself", ModuleKind.Contract);
        var parentScope = new Scope(module);
        var name = "MyConst";
        var type = VarType.Find(VarKind.Number);
        var value = "42";

        // Act
        var constDeclaration = new ConstDeclaration(parentScope, name, type, value);

        // Assert
        Assert.AreEqual(parentScope, constDeclaration.ParentScope);
        Assert.AreEqual(name, constDeclaration.Name);
        Assert.AreEqual(type, constDeclaration.Type);
        Assert.AreEqual(value, constDeclaration.Value);
    }

    [Test]
    public void ConstDeclaration_ToString_ReturnsCorrectString()
    {
        // Arrange
        var module = new Contract("myself", ModuleKind.Contract);
        var parentScope = new Scope(module);
        var name = "MyConst";
        var type = VarType.Find(VarKind.Number);
        var value = "42";
        var constDeclaration = new ConstDeclaration(parentScope, name, type, value);

        // Act
        var result = constDeclaration.ToString();

        // Assert
        Assert.AreEqual("const MyConst:Number", result);
    }

    [Test]
    public void ConstDeclaration_Visit_ExecutesCallback()
    {
        // Arrange
        var module = new Contract("myself", ModuleKind.Contract);
        var parentScope = new Scope(module);
        var name = "MyConst";
        var type = VarType.Find(VarKind.Number);
        var value = "42";
        var constDeclaration = new ConstDeclaration(parentScope, name, type, value);
        var visited = false;

        // Act
        constDeclaration.Visit(node => visited = true);

        // Assert
        Assert.True(visited);
    }

    [Test]
    public void ConstDeclaration_IsNodeUsed_ReturnsTrue()
    {
        // Arrange
        var module = new Contract("myself", ModuleKind.Contract);
        var parentScope = new Scope(module);
        var name = "MyConst";
        var type = VarType.Find(VarKind.Number);
        var value = "42";
        var constDeclaration = new ConstDeclaration(parentScope, name, type, value);

        // Act
        var result = constDeclaration.IsNodeUsed(constDeclaration);

        // Assert
        Assert.True(result);
    }

    [Test]
    public void ConstDeclaration_IsNodeUsed_ReturnsFalse()
    {
        // Arrange
        var module = new Contract("myself", ModuleKind.Contract);
        var parentScope = new Scope(module);
        var name = "MyConst";
        var type = VarType.Find(VarKind.Number);
        var value = "42";
        var constDeclaration = new ConstDeclaration(parentScope, name, type, value);
        var otherNode = new ConstDeclaration(parentScope, "OtherConst", VarType.Find(VarKind.Decimal, 2), "3.12");

        // Act
        var result = constDeclaration.IsNodeUsed(otherNode);

        // Assert
        Assert.False(result);
    }
}