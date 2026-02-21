using System;
using System.Collections.Generic;
using System.Linq;
using CommonLibrary.Extensions;
using Model.Enums;

namespace ZouryokuCommonLibrary.Utils
{
    public class ResponseModel
    {
        public string? Message { get; set; }
        public string? ErrorControl { get; set; }
        public bool IsError { get => !string.IsNullOrWhiteSpace(ErrorControl); set { } }
        public string? WarningControl { get; set; }
        public bool IsWarning { get => !string.IsNullOrWhiteSpace(WarningControl); set { } }

        public ResponseModel(string? message, string? errorControl = null, string? warningControl = null)
        {
            Message = message;
            ErrorControl = errorControl;
            WarningControl = warningControl;
        }
    }

    public abstract class ResponseMessage
    {
        public abstract ResponseStatus Status { get; }
        public string? Message { get; protected set; }
        public List<string> ErrorControls { get; protected set; } = [];
        public List<string> WarningControls { get; protected set; } = [];
    }

    public class SuccessMessage : ResponseMessage
    {
        public SuccessMessage(string? message)
        {
            Message = message;
        }

        public override ResponseStatus Status => ResponseStatus.正常;
    }

    public class ErrorMessage : ResponseMessage
    {
        public ErrorMessage(string message)
        {
            Message = message;
        }

        public ErrorMessage(List<ResponseModel> models)
        {
            Message = models.Select(x => x.Message).Aggregate((a, b) => a + Environment.NewLine + b);
            ErrorControls = models.Where(x => x.ErrorControl != null).Select(x => x.ErrorControl ?? string.Empty).ToList();
            WarningControls = models.Where(x => x.WarningControl != null).Select(x => x.WarningControl ?? string.Empty).ToList();
        }

        public override ResponseStatus Status => ResponseStatus.エラー;
    }

    public class WarningMessage : ResponseMessage
    {
        public WarningMessage(string message)
        {
            Message = message;
        }

        public WarningMessage(List<ResponseModel> models)
        {
            Message = models.Select(x => x.Message).Aggregate((a, b) => a + Environment.NewLine + b);
            ErrorControls = models.Where(x => x.ErrorControl != null).Select(x => x.ErrorControl ?? string.Empty).ToList();
            WarningControls = models.Where(x => x.WarningControl != null).Select(x => x.WarningControl ?? string.Empty).ToList();
        }

        public override ResponseStatus Status => ResponseStatus.警告;
    }

    public static class ResponseMesages
    {
        public static ResponseMessage SuccessMessage(string? message)
            => new SuccessMessage(message);

        public static ResponseMessage ErrorMessage(string message)
            => new ErrorMessage(message);

        public static ResponseMessage ErrorMessage(List<ResponseModel> models)
            => new ErrorMessage(models);

        public static ResponseMessage WarningMessage(string message)
            => new WarningMessage(message);

        public static ResponseMessage WarningMessage(List<ResponseModel> models)
            => new WarningMessage(models);
    }

    public static class Messages
    {
        public static string RequiredMessage(string what)
            => Const.ErrorRequired.Format(what);
        
        public static string ErrorMaxdigitsMessage(string what, int num)
            => Const.ErrorMaxDigits.Format(what, num);

        public static string ErrorUniqueMessage(string what, string? num)
            => Const.ErrorUnique.Format(what, num ?? string.Empty);
        
        public static string TargetErrorUniqueMessage(string target, string targetValue, string what, string? num)
            => Const.TargetErrorUnique.Format(target, targetValue, what, num ?? string.Empty);

        public static string ErrorUniqueMessageNotValue(string what, string num)
            => Const.ErrorUniqueNotValue.Format(what, num);

        public static string ErrorRegisterNotValue(string what, string num)
            => Const.ErrorRegister.Format(what, num);

        public static string NumberMessage(string what)
            => Const.ErrorNumber.Format(what);

        public static string NumberRangeEqualMoreThanMessage(string what, int num)
            => Const.ErrorNumberRangeMoreThanEqual.Format(what, num);

        public static string NumberRangeMoreThanMessage(string what, int num)
            => Const.ErrorNumberRangeMoreThan.Format(what, num);

        public static string NumberRangeEqualLessThanMessage(string what, int num)
            => Const.ErrorNumberRangeLessThanEqual.Format(what, num);

        public static string NumberRangeLessThanMessage(string what, int num)
            => Const.ErrorNumberRangeLessThan.Format(what, num);

        public static string ValueRangeEqualMoreThanMessage(string what, string value)
            => Const.ErrorNumberRangeMoreThanEqual.Format(what, value);

        public static string ValueRangeMoreThanMessage(string what, string value)
            => Const.ErrorNumberRangeMoreThan.Format(what, value);

        public static string ValueRangeEqualLessThanMessage(string what, string value)
            => Const.ErrorNumberRangeLessThanEqual.Format(what, value);

        public static string ValueRangeLessThanMessage(string what, string value)
            => Const.ErrorNumberRangeLessThan.Format(what, value);

        public static string DateTimeMoreThanMessage(string what, string cause)
            => Const.ErrorMoreThanDateTime.Format(what, cause);

        public static string DateTimeLessThanMessage(string what, string cause)
            => Const.ErrorLessThanDateTime.Format(what, cause);
        public static string DateTimeOutOfRangeMessage(string what, string cause, string value)
            => Const.ErrorOutOfRangeDateTime.Format(what, cause, value);

        public static string TimeOutOfRangeMessage(string what, string cause, string value)
            => Const.ErrorOutOfRangeTime.Format(what, cause, value);

        public static string TimeLessThanMessage(string what, string cause)
            => Const.ErrorLessThanTime.Format(what, cause);

        public static string NotFoundMessage(string what, long id)
            => Const.ErrorNotFound.Format(what, id);

        public static string NotExistsMessage(string what, long id)
            => Const.ErrorNotExists.Format(what, id);

        public static string NotExistsMessage(string what, string val)
            => Const.ErrorNotExists.Format(what, val);

        public static string LinkedErrorMessage(string what, string cause)
            => Const.ErrorLinked.Format(what, cause);

        public static string ConflictMessage(string what)
            => Const.ErrorConflict.Format(what);

        public static string ConflictResearchMessage(string what)
            => Const.ErrorConflictResearch.Format(what);

        public static string ConflictReloadMessage(string what)
            => Const.ErrorConflictReload.Format(what);

        public static string ReadMessage(string what)
            => Const.ErrorRead.Format(what);

        public static string ReadResearchMessage(string what)
            => Const.ErrorReadResearch.Format(what);

        public static string ReadReloadMessage(string what)
            => Const.ErrorReadResearch.Format(what);

        public static string ConflictDeleteMessage(string what)
            => ConflictMessage(what) + "一覧を再検索してください。";
    }
}
