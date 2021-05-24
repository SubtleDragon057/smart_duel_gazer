using System.Threading.Tasks;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.Texture;
using AssemblyCSharp.Assets.Code.Core.Storage.Interface.Texture;
using AssemblyCSharp.Assets.Code.Core.YGOProDeck.Interface;
using Zenject;

namespace AssemblyCSharp.Assets.Code.Core.DataManager.Impl.Texture
{
    public class TextureDataManager : ITextureDataManager
    {
        private readonly IYGOProDeckApiProvider _ygoProDeckApiProvider;
        private readonly ITextureStorageProvider _textureStorageProvider;

        [Inject]
        public TextureDataManager(
            IYGOProDeckApiProvider ygoProDeckApiProvider,
            ITextureStorageProvider textureStorageProvider)
        {
            _ygoProDeckApiProvider = ygoProDeckApiProvider;
            _textureStorageProvider = textureStorageProvider;
        }

        public async Task<UnityEngine.Texture> GetCardImage(string cardId)
        {
            var image = _textureStorageProvider.GetTexture(cardId);
            if (image != null)
            {
                return image;
            }

            image = await _ygoProDeckApiProvider.GetCardImage(cardId);
            _textureStorageProvider.SaveTexture(cardId, image);
            return image;
        }
    }
}