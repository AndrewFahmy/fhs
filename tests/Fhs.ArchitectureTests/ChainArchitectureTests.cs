using FHS.Api.Extensions;
using FHS.Api.Links;
using FHS.Chain;
using Microsoft.Extensions.DependencyInjection;

namespace Fhs.ArchitectureTests;

public sealed class ChainArchitectureTests
{
    [Fact]
    public void Every_declared_chain_builds()
    {
        // Build() runs inside each static initializer, so discovery *is* the wiring check.
        // A mis-ordered chain surfaces here as TypeInitializationException wrapping
        // ChainWiringException, with the offending link named in the inner message.
        var chains = ChainCatalog.All();

        Assert.NotEmpty(chains);
    }

    [Fact]
    public void Nothing_follows_the_saving_link()
    {
        foreach (var chain in ChainCatalog.All())
        {
            var names = chain.Links.Select(l => l.Name).ToList();
            var position = names.IndexOf(nameof(SaveChanges));

            if (position < 0)
            {
                continue; // read-only chain — Rule 10 has nothing to say about it
            }

            Assert.True(
                position == names.Count - 1,
                $"""
                Rule 10: {chain.Name} runs {string.Join(" -> ", names[(position + 1)..])} after
                {nameof(SaveChanges)}. Anything that can fail after the commit produces a
                persisted write and an error response.
                """
            );
        }
    }

    [Fact]
    public void No_link_depends_on_another_link()
    {
        foreach (var link in ChainCatalog.LinkTypes())
        {
            var offenders = link.GetConstructors()
                .SelectMany(c => c.GetParameters())
                .Where(p => ChainCatalog.IsLinkContract(p.ParameterType))
                .Select(p => p.ParameterType.Name)
                .ToList();

            Assert.True(
                offenders.Count == 0,
                $"""
                Rule 5: '{link.Name}' takes {string.Join(", ", offenders)} as a dependency.
                Links never call other links — that reinstates the nested tracing this design removes.
                """
            );
        }
    }

    [Fact]
    public void Every_link_in_every_chain_is_registered()
    {
        var services = new ServiceCollection()
            .AddValidationByAssembly(ChainCatalog.ApiAssembly)
            .AddChain(ChainCatalog.ApiAssembly);

        var registered = services.Select(d => d.ServiceType).ToHashSet();

        foreach (var chain in ChainCatalog.All())
        {
            foreach (var link in chain.Links)
            {
                var registrationKey = link.Type.IsConstructedGenericType
                    ? link.Type.GetGenericTypeDefinition()
                    : link.Type;

                Assert.True(
                    registered.Contains(link.Type) || registered.Contains(registrationKey),
                    $"""
                    {chain.Name}: '{link.Name}' resolves to nothing. AddChain skips open generics,
                    so generic link needs an explicit registration.
                    """
                );
            }
        }
    }

    [Fact]
    public void Kernel_does_not_reference_the_api()
    {
        var referenced = typeof(ChainRunner).Assembly.GetReferencedAssemblies().Select(a => a.Name);

        Assert.DoesNotContain("FHS.Api", referenced);
    }
}
