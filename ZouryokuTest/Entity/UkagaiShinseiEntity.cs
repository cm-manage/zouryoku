using Model.Enums;
using Model.Model;

public static class UkagaiShinseiEntity
{
    public static UkagaiShinsei CreateUkagaiShinsei(
        long? id = 1,
        long? ukagaiHeaderId = 1,
        InquiryType? ukagaiSyubetsu = InquiryType.テレワーク)
    {
        return new UkagaiShinsei()
        {
            Id = id ?? 1,
            UkagaiHeaderId = ukagaiHeaderId ?? 1,
            UkagaiSyubetsu = ukagaiSyubetsu ?? InquiryType.テレワーク,
        };
    }
}
