using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace ZouryokuCommonLibrary.Attributes
{
    /// <summary>
    /// データ更新時に空文字の時、登録データがnullに自動変換されるため、自動変換をOFFにする
    /// </summary>
    public class CustomMetadataProvider : IMetadataDetailsProvider, IDisplayMetadataProvider
    {
        public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
        {
            if (context.Key.MetadataKind == ModelMetadataKind.Property)
            {

                context.DisplayMetadata.ConvertEmptyStringToNull = false;
            }
        }
    }
}
