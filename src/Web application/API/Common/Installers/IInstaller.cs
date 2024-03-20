using Common.Config;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Installers;

public interface IInstaller
{
    void Install(IServiceCollection serviceCollection);
    void Install(IServiceCollection serviceCollection, DbConfig config);
}
