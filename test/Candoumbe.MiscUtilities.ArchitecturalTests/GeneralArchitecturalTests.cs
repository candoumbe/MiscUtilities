using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Fluent.Conditions;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnitV3;
using Xunit.Categories;
using Assembly = System.Reflection.Assembly;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Candoumbe.MiscUtilities.ArchitecturalTests;

[UnitTest]
public class GeneralArchitecturalTests
{
    private static readonly Assembly s_miscUtilitiesAssembly = typeof(ArrayExtensions).Assembly;

    private static readonly Architecture s_architecture = new ArchLoader().LoadAssemblies(s_miscUtilitiesAssembly)
        .Build();

    private static IObjectProvider<MethodMember> ExtensionMethods => MethodMembers()
        .That()
        .AreStatic()
        .And()
        .ArePublic()
        .And()
        .AreNoConstructors()
        .And()
        .HaveAnyAttributes(typeof(ExtensionAttribute))
        .And()
        .FollowCustomPredicate(member => member.Parameters.AtLeastOnce(), "is extension method");

    private static IObjectProvider<Class> StaticClassesWithExtensionMethods => Classes()
        .That()
        .AreAbstract()
        .And()
        .FollowCustomPredicate(clazz => ExtensionMethods.GetObjects(s_architecture)
                                   .Select(extensionMethod => extensionMethod.DeclaringType.FullName)
                                   .Contains(clazz.FullName),
                               "is an wrapper of at least one extension method");

    [Fact]
    public void Extension_method_should_be_in_the_same_namespace_as_the_type_it_extends()
    {
        // Arrange
        IArchRule extensionMethodsShouldResideInSameNamespaceAsTypeThatItExtendsRule = MethodMembers()
            .That()
            .Are(ExtensionMethods)
            .Should()
            .FollowCustomCondition(ConditionResult (member) =>
                                   {
                                       Namespace currentNamespace = member.Namespace;
                                       IType firstParameter = member.Parameters.First();
                                       Namespace expectedNamespace =  firstParameter.Namespace;
                                       bool isInExpectedNamespace = member.Namespace.Equals(expectedNamespace);
                                       string extendedType = firstParameter.Name;

                                       return new ConditionResult(member,
                                                                  isInExpectedNamespace,
                                                                  $@"should be in ""{expectedNamespace}"" namespace as the ""{extendedType}"" type it extends (currently in ""{currentNamespace.FullName}"")");
                                   },
                                   "resides in the same namespace as the type it extends") ;

        // Assert
        extensionMethodsShouldResideInSameNamespaceAsTypeThatItExtendsRule.Check(s_architecture);
    }
}