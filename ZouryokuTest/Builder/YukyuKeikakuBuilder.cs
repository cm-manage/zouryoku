using Model.Enums;
using Model.Model;

namespace ZouryokuTest.Builder
{
    /// <summary>
    /// 計画有給休暇のビルダー
    /// </summary>
    internal class YukyuKeikakuBuilder
    {
        private long? _id;
        private LeavePlanStatus? _status;
        private long? _yukyuNendoId;
        private long? _syainBaseId;
        private ICollection<YukyuKeikakuMeisai>? _yukyuKeikakuMeisais;

        public YukyuKeikakuBuilder WithId(long id)
        {
            this._id = id;
            return this;
        }

        public YukyuKeikakuBuilder WithStatus(LeavePlanStatus status)
        {
            this._status = status;
            return this;
        }

        public YukyuKeikakuBuilder WithYukyuNendoId(long yukyuNendoId)
        {
            this._yukyuNendoId = yukyuNendoId;
            return this;
        }

        public YukyuKeikakuBuilder WithSyainBaseId(long syainBaseId)
        {
            this._syainBaseId = syainBaseId;
            return this;
        }

        public YukyuKeikakuBuilder WithYukyuKeikakuMeisais(params ICollection<YukyuKeikakuMeisai> yukyuKeikakuMeisais)
        {
            this._yukyuKeikakuMeisais = yukyuKeikakuMeisais;
            return this;
        }

        public YukyuKeikaku Build()
        {
            var entity = new YukyuKeikaku()
            {
                Id = _id ?? 0,
                Status = _status ?? LeavePlanStatus.未申請,
                YukyuNendoId = _yukyuNendoId ?? 0,
                SyainBaseId = _syainBaseId ?? 0
            };
            if (_yukyuKeikakuMeisais != null)
            {
                entity.YukyuKeikakuMeisais = _yukyuKeikakuMeisais;
            }
            return entity;
        }
    }
}