using Model.Enums;
using Model.Model;

public static class NippouEntity
{
    public static Nippou CreateNippou(
        long? id = 1,
        long? syainId = 1,
        DateOnly? nippouYmd = null,
        short? youbi = 0,
        TimeOnly? syukkinHm1 = null,
        TimeOnly? taisyutsuHm1 = null,
        TimeOnly? syukkinHm2 = null,
        TimeOnly? taisyutsuHm2 = null,
        TimeOnly? syukkinHm3 = null,
        TimeOnly? taisyutsuHm3 = null,
        decimal? hJitsudou = 0,
        decimal? hZangyo = 0,
        decimal? hWarimashi = 0,
        decimal? hShinyaZangyo = 0,
        decimal? dJitsudou = 0,
        decimal? dZangyo = 0,
        decimal? dWarimashi = 0,
        decimal? dShinyaZangyo = 0,
        decimal? nJitsudou = 0,
        decimal? nShinya = 0,
        decimal? totalZangyo = 0,
        NippousCompanyCode? kaisyaCode = null,
        bool? isRendouZumi = false,
        DateOnly? rendouYmd = null,
        DailyReportStatusClassification? tourokuKubun = null,
        DateOnly? kakuteiYmd = null,
        long? syukkinKubunId1 = 0,
        long? syukkinKubunId2 = 0)
    {
        return new Nippou
        {
            Id = id ?? 1,
            SyainId = syainId ?? 1,
            NippouYmd = nippouYmd ?? new DateOnly(2026, 1, 1),
            Youbi = youbi ?? 0,
            SyukkinHm1 = syukkinHm1,
            TaisyutsuHm1 = taisyutsuHm1,
            SyukkinHm2 = syukkinHm2,
            TaisyutsuHm2 = taisyutsuHm2,
            SyukkinHm3 = syukkinHm3,
            TaisyutsuHm3 = taisyutsuHm3,
            HJitsudou = hJitsudou,
            HZangyo = hZangyo,
            HWarimashi = hWarimashi,
            HShinyaZangyo = hShinyaZangyo,
            DJitsudou = dJitsudou,
            DZangyo = dZangyo,
            DWarimashi = dWarimashi,
            DShinyaZangyo = dShinyaZangyo,
            NJitsudou = nJitsudou,
            NShinya = nShinya,
            TotalZangyo = totalZangyo,
            KaisyaCode = kaisyaCode ?? NippousCompanyCode.協和,
            IsRendouZumi = isRendouZumi ?? false,
            RendouYmd = rendouYmd,
            TourokuKubun = tourokuKubun ?? DailyReportStatusClassification.一時保存,
            KakuteiYmd = kakuteiYmd,
            SyukkinKubunId1 = syukkinKubunId1 ?? 0,
            SyukkinKubunId2 = syukkinKubunId2 ?? 0
        };
    }
}
