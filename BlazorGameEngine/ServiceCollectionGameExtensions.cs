using BlazorGameEngine.Abstractions;
using BlazorGameEngine.Assets;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Xml.Linq;

namespace BlazorGameEngine
{
    public static class ServiceCollectionGameExtensions
    {
        public static IServiceCollection AddSprite(this IServiceCollection services, string name, SpriteAsset sprite)
        {
            var instance = sprite with { };
            services.AddKeyedScoped(name, (_, _) => instance);
            services.AddScoped(_ => instance);
            services.AddScoped<IGameAsset>(_ => instance);
            return services;
        }
        public static IServiceCollection AddSprite(this IServiceCollection services, SpriteAsset sprite)
        {
            var instance = sprite with { };
            services.AddScoped(_ => instance);
            services.AddScoped<IGameAsset>(_ => instance);
            return services;
        }
        public static IServiceCollection AddSprite<TSprite>(this IServiceCollection services) where TSprite : SpriteAsset
        {
            services.AddScoped<TSprite>();
            services.AddScoped<SpriteAsset>(sp => sp.GetRequiredService<TSprite>());
            services.AddScoped<IGameAsset>(sp => sp.GetRequiredService<TSprite>());
            return services;
        }
        public static IServiceCollection AddSprite(this IServiceCollection services, Type spriteType)
        {
            services.AddScoped(spriteType);
            services.AddScoped(sp => (SpriteAsset)sp.GetRequiredService(spriteType));
            services.AddScoped(sp => (IGameAsset)sp.GetRequiredService(spriteType));
            return services;
        }
        public static IServiceCollection AddSprites(this IServiceCollection services, Assembly assembly)
        {
            var spriteTypes = assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(SpriteAsset)) && !t.IsAbstract);
            foreach (var spriteType in spriteTypes)
                services.AddSprite(spriteType);
            return services;
        }

        public static IServiceCollection AddImage(this IServiceCollection services, string name, ImageAsset image)
        {
            var instance = image with { };
            services.AddKeyedScoped(name, (_, _) => instance);
            services.AddScoped(_ => instance);
            services.AddScoped<IGameAsset>(_ => instance);
            return services;
        }
        public static IServiceCollection AddImage(this IServiceCollection services, ImageAsset image)
        {
            var instance = image with { };
            services.AddScoped(_ => instance);
            services.AddScoped<IGameAsset>(_ => instance);
            return services;
        }
        public static IServiceCollection AddImage<TImage>(this IServiceCollection services) where TImage : ImageAsset
        {
            services.AddScoped<TImage>();
            services.AddScoped<ImageAsset>(sp => sp.GetRequiredService<TImage>());
            services.AddScoped<IGameAsset>(sp => sp.GetRequiredService<TImage>());
            return services;
        }
        public static IServiceCollection AddImage(this IServiceCollection services, Type imageType)
        {
            services.AddScoped(imageType);
            services.AddScoped(sp => (ImageAsset)sp.GetRequiredService(imageType));
            services.AddScoped(sp => (IGameAsset)sp.GetRequiredService(imageType));
            return services;
        }
        public static IServiceCollection AddImages(this IServiceCollection services, Assembly assembly)
        {
            var imageTypes = assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(ImageAsset)) && !t.IsAbstract);
            foreach (var imageType in imageTypes)
                services.AddImage(imageType);
            return services;
        }

        public static IServiceCollection AddGame<TGame>(this IServiceCollection services) where TGame : Game
        {
            services.AddScoped<TGame>();
            services.AddScoped<Game>(sp => sp.GetRequiredService<TGame>());
            return services;
        }

        public static IServiceCollection AddGameResources(this IServiceCollection services, Assembly assembly)
        {
            services.AddSprites(assembly);
            services.AddImages(assembly);
            return services;
        }
    }
}
