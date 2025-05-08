using System.Linq;
using System.Text.Json;
using Mapster;
using atmnr_api.Entities;
using atmnr_api.Models;

namespace atmnr_api.Models.Mappings;

public class MapsterConfig
{
    public static void Register()
    {
        TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);
        // TypeAdapterConfig.GlobalSettings.Default.IgnoreNonMapped(true);
        // TypeAdapterConfig.GlobalSettings.Default.ShallowCopyForSameType(true);
        // TypeAdapterConfig<Guid?, Guid?>.NewConfig()
        //     .MapToTargetWith((src, dest) => src != null && src == Guid.Empty ? null : src);

        // TypeAdapterConfig<Payroll, PayrollInfo>.NewConfig()
        //     .TwoWays()
        //     .Map(dest => dest.Id, src => src.NbpayrollId)
        //     .Map(dest => dest.Status, src => src.Payrollstatus);
        // TypeAdapterConfig<PayrollDetail, PayrollDetailInfo>.NewConfig()
        //     .TwoWays()
        //     .Map(dest => dest.DetailId, src => src.NbpayrolldetailId);

        // TypeAdapterConfig<Payslip, PayslipInfo>.NewConfig()
        //     .TwoWays()
        //     .Map(dest => dest.Id, src => src.NbpayslipId)
        //     .Map(dest => dest.Status, src => src.Payslipstatus);
        // TypeAdapterConfig<PayslipDetail, PayslipDetailInfo>.NewConfig()
        //     .TwoWays()
        //     .Map(dest => dest.DetailId, src => src.NbpayslipdetailId);

        // TypeAdapterConfig<JournalRepeat, JournalRepeatInfo>.NewConfig()
        //     .TwoWays()
        //     .Map(dest => dest.NbjournalId, src => src.NbrepeatjrnlId);
        // TypeAdapterConfig<JournalRepeatDetail, JournalDetailInfo>.NewConfig()
        //     .TwoWays()
        //     .Map(dest => dest.NbjournaldetailId, src => src.NbrepeatjrnldetailId)
        //     .Map(dest => dest.NbjournalId, src => src.NbrepeatjrnlId);

        // TypeAdapterConfig<BillRepeat, BillRepeatInfo>.NewConfig()
        //     .TwoWays()
        //     .Map(dest => dest.NbbillId, src => src.NbrepeatbillId);
        // TypeAdapterConfig<BillRepeatDetail, BillDetailInfo>.NewConfig()
        //     .TwoWays()
        //     .Map(dest => dest.NbbillId, src => src.NbrepeatbillId)
        //     .Map(dest => dest.NbbilldetailId, src => src.NbrepeatbilldetailId);
        // TypeAdapterConfig<InvoiceRepeat, InvoiceRepeatInfo>.NewConfig()
        //     .TwoWays()
        //     .Map(dest => dest.NbinvoiceId, src => src.NbrepeatinvId);
        // TypeAdapterConfig<InvoiceRepeatDetail, InvoiceDetailInfo>.NewConfig()
        //     .TwoWays()
        //     .Map(dest => dest.NbinvoicedetailId, src => src.NbrepeatinvdetailId)
        //     .Map(dest => dest.NbinvoiceId, src => src.NbrepeatinvId);


    }
}