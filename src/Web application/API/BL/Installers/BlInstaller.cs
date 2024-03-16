using BL.Facades.Interfaces;
using Common.Installers;
using Microsoft.Extensions.DependencyInjection;

namespace BL.Installers;

public class BlInstaller : AbstractInstaller
{
    public override void Install(IServiceCollection serviceCollection) =>
        serviceCollection.Scan(selector =>
            selector.FromAssemblyOf<BlInstaller>()
                .AddClasses(classes => classes.AssignableTo(typeof(IFacade<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());
}
