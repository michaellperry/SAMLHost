namespace SAMLHost;

public static class ApplicationBuilderExtensions
{
    public static void BindOptions<T>(this WebApplicationBuilder builder, string section) where T : class, new()
    {
        T options = new T();
        builder.Configuration.Bind(section, options);
        builder.Services.AddSingleton(options);
    }
}
