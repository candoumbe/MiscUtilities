using System.Runtime.CompilerServices;
using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Fluent.Conditions;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnitV3;
using Assembly = System.Reflection.Assembly;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using Type = System.Type;


namespace Candoumbe.MiscUtilties.ArchitecturalTests;

public class GeneralArchitecturalTests
{

    private static readonly Assembly MiscUtiltiesAssembly = typeof(ArrayExtensions).Assembly;

    private static readonly Architecture Architecure = new ArchLoader().LoadAssemblies(MiscUtiltiesAssembly)
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
        .FollowCustomPredicate(memeber => memeber.Parameters.AtLeastOnce(), "is extension method");

    [Fact]
    public void ExtensionMethod_should_be_in_the_same_namespace_as_the_type_it_extends()
    {
        // Arrange
        IArchRule extensionMethodsShouldResideInSameNamespaceAsTypeThatItExtenssRule = MethodMembers()
            .That()
            .Are(ExtensionMethods)
            .Should()
            .FollowCustomCondition(ConditionResult (member) =>
                                   {
                                       Namespace currentNamespace = member.Namespace;
                                       IType firstParamter = member.Parameters.First();
                                       Namespace expectedNamespace =  firstParamter.Namespace;
                                       bool isInExpectedNamespace = member.Namespace.Equals(expectedNamespace);
                                       string extendedType = firstParamter.FullName;

                                       return new ConditionResult(member,
                                                                  isInExpectedNamespace,
                                                                  $@"should be in ""{expectedNamespace}"" namespace as the ""{extendedType}"" type it extends (currently in ""{currentNamespace.FullName}"")");
                                   },
                                   "resides in the same namespace as the type it extends");


        // Assert
        extensionMethodsShouldResideInSameNamespaceAsTypeThatItExtenssRule.Check(Architecure);

    }
}