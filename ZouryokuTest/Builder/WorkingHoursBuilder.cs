using Model.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace ZouryokuTest.Builder
{
    internal class WorkingHoursBuilder
    {
        private long? _id;
        private long? _syainId;
        private DateOnly? _hiduke;
        private decimal? _syukkinLatitude;
        private decimal? _syukkinLongitude;
        private decimal? _taikinLatitude;
        private decimal? _taikinLongitude;
        private DateTime? _syukkinTime;
        private DateTime? _taikinTime;
        private bool? _edited;
        private bool? _deleted;
        private long? _editSyainId;
        private long? _ukagaiHeaderId;

        public WorkingHoursBuilder WithId(long id)
        {
            this._id = id;
            return this;
        }

        public WorkingHoursBuilder WithSyainId(long syainId)
        {
            this._syainId = syainId;
            return this;
        }

        public WorkingHoursBuilder WithHiduke(DateOnly hiduke)
        {
            this._hiduke = hiduke;
            return this;
        }

        public WorkingHoursBuilder WithSyukkinLatitude(decimal syukkinLatitude)
        {
            this._syukkinLatitude = syukkinLatitude;
            return this;
        }

        public WorkingHoursBuilder WithSyukkinLongitude(decimal syukkinLongitude)
        {
            this._syukkinLongitude = syukkinLongitude;
            return this;
        }

        public WorkingHoursBuilder WithTaikinLatitude(decimal taikinLatitude)
        {
            this._taikinLatitude = taikinLatitude;
            return this;
        }

        public WorkingHoursBuilder WithTaikinLongitude(decimal taikinLongitude)
        {
            this._taikinLongitude = taikinLongitude;
            return this;
        }

        public WorkingHoursBuilder WithSyukkinTime(DateTime syukkinTime)
        {
            this._syukkinTime = syukkinTime;
            return this;
        }

        public WorkingHoursBuilder WithTaikinTime(DateTime taikinTime)
        {
            this._taikinTime = taikinTime;
            return this;
        }

        public WorkingHoursBuilder WithEdited(bool edited)
        {
            this._edited = edited;
            return this;
        }

        public WorkingHoursBuilder WithDeleted(bool deleted)
        {
            this._deleted = deleted;
            return this;
        }

        public WorkingHoursBuilder WithEditSyainId(long editSyainId)
        {
            this._editSyainId = editSyainId;
            return this;
        }

        public WorkingHoursBuilder WithUkagaiHeaderId(long ukagaiHeaderId)
        {
            this._ukagaiHeaderId = ukagaiHeaderId;
            return this;
        }
        public WorkingHour Build()
        {
            return new WorkingHour()
            {
                Id = _id ?? 1,
                SyainId = _syainId ?? 1,
                Hiduke = _hiduke ?? DateOnly.FromDateTime(new DateTime(2026, 1, 1)),
                SyukkinLatitude = _syukkinLatitude ?? 0,
                SyukkinLongitude = _syukkinLongitude ?? 0,
                TaikinLatitude = _taikinLatitude ?? 0,
                TaikinLongitude = _taikinLongitude ?? 0,
                SyukkinTime = _syukkinTime,
                TaikinTime = _taikinTime,
                Edited = _edited ?? false,
                Deleted = _deleted ?? false,
                EditSyainId = _editSyainId,
                UkagaiHeaderId = _ukagaiHeaderId
            };
        }
    }

}
