using Model.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZouryokuTest.Builder
{
    /// <summary>
    /// 出勤区分用のBuilder
    /// </summary>
    internal class SyukkinKubunBuilder
    {
        private long _id;
        private string _code = string.Empty;
        private string _name = string.Empty;
        private string _nameRyaku = string.Empty;
        private bool _isSyukkin;
        private bool _isVacation;
        private bool _isHoliday;
        private bool _isNeedKubun1;
        private bool _isNeedKubun2;

        public SyukkinKubunBuilder WithId(long id)
        {
            _id = id;
            return this;
        }

        public SyukkinKubunBuilder WithCode(string code)
        {
            _code = code;
            return this;
        }

        public SyukkinKubunBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public SyukkinKubunBuilder WithNameRyaku(string nameRyaku)
        {
            _nameRyaku = nameRyaku;
            return this;
        }

        public SyukkinKubunBuilder WithIsSyukkin(bool isSyukkin)
        {
            _isSyukkin = isSyukkin;
            return this;
        }

        public SyukkinKubunBuilder WithIsVacation(bool isVacation)
        {
            _isVacation = isVacation;
            return this;
        }

        public SyukkinKubunBuilder WithIsHoliday(bool isHoliday)
        {
            _isHoliday = isHoliday;
            return this;
        }

        public SyukkinKubunBuilder WithIsNeedKubun1(bool isNeedKubun1)
        {
            _isNeedKubun1 = isNeedKubun1;
            return this;
        }

        public SyukkinKubunBuilder WithIsNeedKubun2(bool isNeedKubun2)
        {
            _isNeedKubun2 = isNeedKubun2;
            return this;
        }

        public SyukkinKubun Build()
        {
            return new SyukkinKubun
            {
                Id = _id,
                CodeString = _code,
                Name = _name,
                NameRyaku = _nameRyaku,
                IsSyukkin = _isSyukkin,
                IsVacation = _isVacation,
                IsHoliday = _isHoliday,
                IsNeedKubun1 = _isNeedKubun1,
                IsNeedKubun2 = _isNeedKubun2,
            };
        }
    }
}
