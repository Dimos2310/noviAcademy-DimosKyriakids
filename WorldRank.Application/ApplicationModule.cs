using Autofac;
using MediatR;
using MediatR.Extensions.Autofac.DependencyInjection;
using MediatR.Extensions.Autofac.DependencyInjection.Builder;
using WorldRank.Application.Decorators;

namespace WorldRank.Application;

public class ApplicationModule : Module
{
	protected override void Load(ContainerBuilder builder)
	{
		var configuration = MediatRConfigurationBuilder
			.Create(ThisAssembly)
			.WithAllOpenGenericHandlerTypesRegistered()
			.Build();

		builder.RegisterMediatR(configuration);

		builder.RegisterGenericDecorator(typeof(LoggingDecorator<,>), typeof(IRequestHandler<,>));
	}
}
