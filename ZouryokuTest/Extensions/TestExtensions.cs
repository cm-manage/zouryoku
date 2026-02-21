using Model.Enums;
using Model.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static LanguageExt.Prelude;

namespace ZouryokuTest.Extensions
{
    public static class TestExtensions
    {
        /// <summary>
        /// UT 用に Code プロパティをセットする
        /// </summary>
        public static void SetCodeForTest(this SyukkinKubun kubun, AttendanceClassification value)
        {
            kubun.CodeString = ((short)value).ToString("D2");
        }
    }
}
