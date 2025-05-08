using atmnr_api.Authorization;
namespace atmnr_api.Services;

public partial class ServiceHelper
{
    public static partial void AddServiceSys(IServiceCollection services);
    public static partial void AddServiceSys(IServiceCollection services)
    {
        services.AddScoped<IAuthReposity, AuthReposity>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IAddressService, AddressService>();
        services.AddScoped<IApartmentService, ApartmentService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IHomeService, HomeService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ICustomerJourneyService, CustomerJourneyService>();
        services.AddScoped<IHistoryService, HistoryService>();
        services.AddScoped<IReportService, ReportService>();
    }
}
