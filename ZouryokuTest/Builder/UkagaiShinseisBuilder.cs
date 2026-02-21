using Model.Enums;
using Model.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace ZouryokuTest.Builder
{
    internal class UkagaiShinseiBuilder
    {
        private long? _id;
        private long? _ukagaiHeaderId;
        private InquiryType _ukagai_syubetsu;

        public UkagaiShinseiBuilder WithId(long id)
        {
            this._id = id;
            return this;
        }

        public UkagaiShinseiBuilder WithUkagaiHeaderId(long ukagaiHeaderId)
        {
            this._ukagaiHeaderId = ukagaiHeaderId;
            return this;
        }

        public UkagaiShinseiBuilder WithUkagaiSyubetsu(InquiryType ukagai_syubetsu)
        {
            this._ukagai_syubetsu = ukagai_syubetsu;
            return this;
        }

        public UkagaiShinsei Build()
        {
            return new UkagaiShinsei()
            {
                Id = _id ?? 1,
                UkagaiHeaderId = _ukagaiHeaderId ?? 1,
                UkagaiSyubetsu = _ukagai_syubetsu,
            };
        }
    }
}
