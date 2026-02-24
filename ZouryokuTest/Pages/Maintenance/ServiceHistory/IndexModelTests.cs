using Microsoft.AspNetCore.Mvc;
using Model.Enums;
using Model.Model;
using Zouryoku.Models;
using Zouryoku.Pages.Maintenance.ServiceHistory;

namespace ZouryokuTest.Pages.Maintenance.ServiceHistory
{
    /// <summary>
    /// IndexModel (サービス稼働履歴一覧ページ) のユニットテスト
    /// </summary>
    [TestClass]
    public class IndexModelTests : BaseInMemoryDbContextTest
    {
        private IndexModel CreateModel()
        {
            var model = new IndexModel(db, GetLogger<IndexModel>(), options, fakeTimeProvider);
            model.PageContext = GetPageContext();
            model.TempData = GetTempData();
            return model;
        }

        // TODO Model変更対応でコメントアウト

        /// <summary>
        /// 正常: 検索条件なしで全データが取得されること
        /// </summary>
        //[TestMethod]
        //public async Task OnPostSearchAsync_Returns_All_Records_When_No_Filters()
        //{
        //    // Arrange
        //    var serviceExecute1 = new ServiceExecute
        //    {
        //        Id = 1,
        //        Used = true,
        //        RequestDatetime = DT("2024-01-01 09:00:00"),
        //        CompletedDatetime = DT("2024-01-01 10:00:00"),
        //        Type = ServiceClassification.連携プログラム稼働
        //    };
        //    var serviceExecute2 = new ServiceExecute
        //    {
        //        Id = 2,
        //        Used = true,
        //        RequestDatetime = DT("2024-01-02 09:00:00"),
        //        CompletedDatetime = DT("2024-01-02 10:00:00"),
        //        Type = ServiceClassification.過労運転防止
        //    };
            
        //    db.ServiceExecutes.Add(serviceExecute1);
        //    db.ServiceExecutes.Add(serviceExecute2);
            
        //    db.ServiceExecuteHistories.Add(new ServiceExecuteHistory
        //    {
        //        Id = 1,
        //        RequestDatetime = DT("2024-01-01 09:00:00"),
        //        ExecuteDatetime = DT("2024-01-01 09:30:00"),
        //        CompletedDatetime = DT("2024-01-01 10:00:00"),
        //        Status = ServiceStatus.実行済,
        //        Content = "Test1",
        //        ServiceExecuteId = 1
        //    });
            
        //    db.ServiceExecuteHistories.Add(new ServiceExecuteHistory
        //    {
        //        Id = 2,
        //        RequestDatetime = DT("2024-01-02 09:00:00"),
        //        ExecuteDatetime = DT("2024-01-02 09:30:00"),
        //        CompletedDatetime = DT("2024-01-02 10:00:00"),
        //        Status = ServiceStatus.実行済,
        //        Content = "Test2",
        //        ServiceExecuteId = 2
        //    });
            
        //    await db.SaveChangesAsync();

        //    var model = CreateModel();
        //    model.Condition = new SearchCondition();

        //    // Act
        //    var result = await model.OnPostSearchAsync();

        //    // Assert
        //    Assert.IsInstanceOfType<JsonResult>(result, "結果は JsonResult であるべき");
        //    var jsonResult = result as JsonResult;
        //    Assert.IsNotNull(jsonResult, "JsonResult が null です");
            
        //    var gridJson = jsonResult.Value as GridJson<SearchGridModel>;
        //    Assert.IsNotNull(gridJson, "GridJson が null です");
        //    Assert.HasCount(2, gridJson.Data.ToList(), "全データが取得されていません");
        //    Assert.AreEqual(2, gridJson.ItemsCount, "ItemsCount が一致しません");
        //}

        ///// <summary>
        ///// 正常: サービス名で絞り込まれること
        ///// </summary>
        //[TestMethod]
        //public async Task OnPostSearchAsync_Filters_By_ServiceType()
        //{
        //    // Arrange
        //    var serviceExecute1 = new ServiceExecute
        //    {
        //        Id = 1,
        //        Used = true,
        //        RequestDatetime = DT("2024-01-01 09:00:00"),
        //        CompletedDatetime = DT("2024-01-01 10:00:00"),
        //        Type = ServiceClassification.連携プログラム稼働
        //    };
        //    var serviceExecute2 = new ServiceExecute
        //    {
        //        Id = 2,
        //        Used = true,
        //        RequestDatetime = DT("2024-01-02 09:00:00"),
        //        CompletedDatetime = DT("2024-01-02 10:00:00"),
        //        Type = ServiceClassification.過労運転防止
        //    };
            
        //    db.ServiceExecutes.Add(serviceExecute1);
        //    db.ServiceExecutes.Add(serviceExecute2);
            
        //    db.ServiceExecuteHistories.Add(new ServiceExecuteHistory
        //    {
        //        Id = 1,
        //        RequestDatetime = DT("2024-01-01 09:00:00"),
        //        ExecuteDatetime = DT("2024-01-01 09:30:00"),
        //        CompletedDatetime = DT("2024-01-01 10:00:00"),
        //        Status = ServiceStatus.実行済,
        //        Content = "Test1",
        //        ServiceExecuteId = 1
        //    });
            
        //    db.ServiceExecuteHistories.Add(new ServiceExecuteHistory
        //    {
        //        Id = 2,
        //        RequestDatetime = DT("2024-01-02 09:00:00"),
        //        ExecuteDatetime = DT("2024-01-02 09:30:00"),
        //        CompletedDatetime = DT("2024-01-02 10:00:00"),
        //        Status = ServiceStatus.実行済,
        //        Content = "Test2",
        //        ServiceExecuteId = 2
        //    });
            
        //    await db.SaveChangesAsync();

        //    var model = CreateModel();
        //    model.Condition = new SearchCondition { ServiceType = ServiceClassification.連携プログラム稼働 };

        //    // Act
        //    var result = await model.OnPostSearchAsync();

        //    // Assert
        //    var gridJson = (result as JsonResult)?.Value as GridJson<SearchGridModel>;
        //    Assert.IsNotNull(gridJson, "GridJson が null です");
        //    var dataList = gridJson.Data.ToList();
        //    Assert.HasCount(1, dataList, "フィルタリングが正しく機能していません");
        //    Assert.AreEqual("連携プログラム稼働", dataList[0].ServiceType, "サービス種別が一致しません");
        //}

        ///// <summary>
        ///// 正常: 日付範囲で絞り込まれること
        ///// </summary>
        //[TestMethod]
        //public async Task OnPostSearchAsync_Filters_By_DateRange()
        //{
        //    // Arrange
        //    var serviceExecute1 = new ServiceExecute
        //    {
        //        Id = 1,
        //        Used = true,
        //        RequestDatetime = DT("2024-01-01 09:00:00"),
        //        CompletedDatetime = DT("2024-01-01 10:00:00"),
        //        Type = ServiceClassification.連携プログラム稼働
        //    };
        //    var serviceExecute2 = new ServiceExecute
        //    {
        //        Id = 2,
        //        Used = true,
        //        RequestDatetime = DT("2024-01-15 09:00:00"),
        //        CompletedDatetime = DT("2024-01-15 10:00:00"),
        //        Type = ServiceClassification.連携プログラム稼働
        //    };
            
        //    db.ServiceExecutes.Add(serviceExecute1);
        //    db.ServiceExecutes.Add(serviceExecute2);
            
        //    db.ServiceExecuteHistories.Add(new ServiceExecuteHistory
        //    {
        //        Id = 1,
        //        RequestDatetime = DT("2024-01-01 09:00:00"),
        //        ExecuteDatetime = DT("2024-01-01 09:30:00"),
        //        CompletedDatetime = DT("2024-01-01 10:00:00"),
        //        Status = ServiceStatus.実行済,
        //        Content = "Test1",
        //        ServiceExecuteId = 1
        //    });
            
        //    db.ServiceExecuteHistories.Add(new ServiceExecuteHistory
        //    {
        //        Id = 2,
        //        RequestDatetime = DT("2024-01-15 09:00:00"),
        //        ExecuteDatetime = DT("2024-01-15 09:30:00"),
        //        CompletedDatetime = DT("2024-01-15 10:00:00"),
        //        Status = ServiceStatus.実行済,
        //        Content = "Test2",
        //        ServiceExecuteId = 2
        //    });
            
        //    await db.SaveChangesAsync();

        //    var model = CreateModel();
        //    model.Condition = new SearchCondition
        //    {
        //        RequestDateFrom = DT("2024-01-05"),
        //        CompletedDateTo = DT("2024-01-20")
        //    };

        //    // Act
        //    var result = await model.OnPostSearchAsync();

        //    // Assert
        //    var gridJson = (result as JsonResult)?.Value as GridJson<SearchGridModel>;
        //    Assert.IsNotNull(gridJson, "GridJson が null です");
        //    var dataList = gridJson.Data.ToList();
        //    Assert.HasCount(1, dataList, "日付範囲フィルタが正しく機能していません");
        //}

        ///// <summary>
        ///// 正常: ステータスで絞り込まれること
        ///// </summary>
        //[TestMethod]
        //public async Task OnPostSearchAsync_Filters_By_Status()
        //{
        //    // Arrange
        //    var serviceExecute1 = new ServiceExecute
        //    {
        //        Id = 1,
        //        Used = true,
        //        RequestDatetime = DT("2024-01-01 09:00:00"),
        //        CompletedDatetime = DT("2024-01-01 10:00:00"),
        //        Type = ServiceClassification.連携プログラム稼働
        //    };
        //    var serviceExecute2 = new ServiceExecute
        //    {
        //        Id = 2,
        //        Used = true,
        //        RequestDatetime = DT("2024-01-02 09:00:00"),
        //        CompletedDatetime = DT("2024-01-02 10:00:00"),
        //        Type = ServiceClassification.連携プログラム稼働
        //    };
            
        //    db.ServiceExecutes.Add(serviceExecute1);
        //    db.ServiceExecutes.Add(serviceExecute2);
            
        //    db.ServiceExecuteHistories.Add(new ServiceExecuteHistory
        //    {
        //        Id = 1,
        //        RequestDatetime = DT("2024-01-01 09:00:00"),
        //        ExecuteDatetime = DT("2024-01-01 09:30:00"),
        //        CompletedDatetime = DT("2024-01-01 10:00:00"),
        //        Status = ServiceStatus.実行済,
        //        Content = "Test1",
        //        ServiceExecuteId = 1
        //    });
            
        //    db.ServiceExecuteHistories.Add(new ServiceExecuteHistory
        //    {
        //        Id = 2,
        //        RequestDatetime = DT("2024-01-02 09:00:00"),
        //        ExecuteDatetime = DT("2024-01-02 09:30:00"),
        //        CompletedDatetime = DT("2024-01-02 10:00:00"),
        //        Status = ServiceStatus.エラー,
        //        Content = "Test2",
        //        ServiceExecuteId = 2
        //    });
            
        //    await db.SaveChangesAsync();

        //    var model = CreateModel();
        //    model.Condition = new SearchCondition { Status = ServiceStatus.実行済 };

        //    // Act
        //    var result = await model.OnPostSearchAsync();

        //    // Assert
        //    var gridJson = (result as JsonResult)?.Value as GridJson<SearchGridModel>;
        //    Assert.IsNotNull(gridJson, "GridJson が null です");
        //    var dataList = gridJson.Data.ToList();
        //    Assert.HasCount(1, dataList, "ステータスフィルタが正しく機能していません");
        //    Assert.AreEqual("実行済", dataList[0].Status, "ステータスが一致しません");
        //}

        ///// <summary>
        ///// 正常: 複数条件で絞り込まれること
        ///// </summary>
        //[TestMethod]
        //public async Task OnPostSearchAsync_Filters_By_Multiple_Conditions()
        //{
        //    // Arrange
        //    var serviceExecute1 = new ServiceExecute
        //    {
        //        Id = 1,
        //        Used = true,
        //        RequestDatetime = DT("2024-01-01 09:00:00"),
        //        CompletedDatetime = DT("2024-01-01 10:00:00"),
        //        Type = ServiceClassification.連携プログラム稼働
        //    };
        //    var serviceExecute2 = new ServiceExecute
        //    {
        //        Id = 2,
        //        Used = true,
        //        RequestDatetime = DT("2024-01-15 09:00:00"),
        //        CompletedDatetime = DT("2024-01-15 10:00:00"),
        //        Type = ServiceClassification.過労運転防止
        //    };
            
        //    db.ServiceExecutes.Add(serviceExecute1);
        //    db.ServiceExecutes.Add(serviceExecute2);
            
        //    db.ServiceExecuteHistories.Add(new ServiceExecuteHistory
        //    {
        //        Id = 1,
        //        RequestDatetime = DT("2024-01-01 09:00:00"),
        //        ExecuteDatetime = DT("2024-01-01 09:30:00"),
        //        CompletedDatetime = DT("2024-01-01 10:00:00"),
        //        Status = ServiceStatus.実行済,
        //        Content = "Test1",
        //        ServiceExecuteId = 1
        //    });
            
        //    db.ServiceExecuteHistories.Add(new ServiceExecuteHistory
        //    {
        //        Id = 2,
        //        RequestDatetime = DT("2024-01-15 09:00:00"),
        //        ExecuteDatetime = DT("2024-01-15 09:30:00"),
        //        CompletedDatetime = DT("2024-01-15 10:00:00"),
        //        Status = ServiceStatus.実行済,
        //        Content = "Test2",
        //        ServiceExecuteId = 2
        //    });
            
        //    await db.SaveChangesAsync();

        //    var model = CreateModel();
        //    model.Condition = new SearchCondition
        //    {
        //        ServiceType = ServiceClassification.過労運転防止,
        //        RequestDateFrom = DT("2024-01-10"),
        //        CompletedDateTo = DT("2024-01-20"),
        //        Status = ServiceStatus.実行済
        //    };

        //    // Act
        //    var result = await model.OnPostSearchAsync();

        //    // Assert
        //    var gridJson = (result as JsonResult)?.Value as GridJson<SearchGridModel>;
        //    Assert.IsNotNull(gridJson, "GridJson が null です");
        //    var dataList = gridJson.Data.ToList();
        //    Assert.HasCount(1, dataList, "複数条件フィルタが正しく機能していません");
        //    Assert.AreEqual("過労運転防止", dataList[0].ServiceType, "サービス種別が一致しません");
        //}

        ///// <summary>
        ///// 正常: 条件に合うデータがない場合、空のリストが返されること
        ///// </summary>
        //[TestMethod]
        //public async Task OnPostSearchAsync_Returns_Empty_List_When_No_Match()
        //{
        //    // Arrange
        //    var serviceExecute1 = new ServiceExecute
        //    {
        //        Id = 1,
        //        Used = true,
        //        RequestDatetime = DT("2024-01-01 09:00:00"),
        //        CompletedDatetime = DT("2024-01-01 10:00:00"),
        //        Type = ServiceClassification.連携プログラム稼働
        //    };
            
        //    db.ServiceExecutes.Add(serviceExecute1);
            
        //    db.ServiceExecuteHistories.Add(new ServiceExecuteHistory
        //    {
        //        Id = 1,
        //        RequestDatetime = DT("2024-01-01 09:00:00"),
        //        ExecuteDatetime = DT("2024-01-01 09:30:00"),
        //        CompletedDatetime = DT("2024-01-01 10:00:00"),
        //        Status = ServiceStatus.実行済,
        //        Content = "Test1",
        //        ServiceExecuteId = 1
        //    });
            
        //    await db.SaveChangesAsync();

        //    var model = CreateModel();
        //    model.Condition = new SearchCondition { ServiceType = ServiceClassification.有給未取得アラート };

        //    // Act
        //    var result = await model.OnPostSearchAsync();

        //    // Assert
        //    var gridJson = (result as JsonResult)?.Value as GridJson<SearchGridModel>;
        //    Assert.IsNotNull(gridJson, "GridJson が null です");
        //    var dataList = gridJson.Data.ToList();
        //    Assert.IsEmpty(dataList, "空のリストが返されるべき");
        //    Assert.AreEqual(0, gridJson.ItemsCount, "ItemsCount が 0 であるべき");
        //}

        ///// <summary>
        ///// 正常: 完了日時の降順でソートされること
        ///// </summary>
        //[TestMethod]
        //public async Task OnPostSearchAsync_Returns_Sorted_By_CompletedDatetime_Descending()
        //{
        //    // Arrange
        //    var serviceExecute1 = new ServiceExecute
        //    {
        //        Id = 1,
        //        Used = true,
        //        RequestDatetime = DT("2024-01-01 09:00:00"),
        //        CompletedDatetime = DT("2024-01-01 10:00:00"),
        //        Type = ServiceClassification.連携プログラム稼働
        //    };
        //    var serviceExecute2 = new ServiceExecute
        //    {
        //        Id = 2,
        //        Used = true,
        //        RequestDatetime = DT("2024-01-02 09:00:00"),
        //        CompletedDatetime = DT("2024-01-02 10:00:00"),
        //        Type = ServiceClassification.連携プログラム稼働
        //    };
            
        //    db.ServiceExecutes.Add(serviceExecute1);
        //    db.ServiceExecutes.Add(serviceExecute2);
            
        //    db.ServiceExecuteHistories.Add(new ServiceExecuteHistory
        //    {
        //        Id = 1,
        //        RequestDatetime = DT("2024-01-01 09:00:00"),
        //        ExecuteDatetime = DT("2024-01-01 09:30:00"),
        //        CompletedDatetime = DT("2024-01-01 10:00:00"),
        //        Status = ServiceStatus.実行済,
        //        Content = "Test1",
        //        ServiceExecuteId = 1
        //    });
            
        //    db.ServiceExecuteHistories.Add(new ServiceExecuteHistory
        //    {
        //        Id = 2,
        //        RequestDatetime = DT("2024-01-02 09:00:00"),
        //        ExecuteDatetime = DT("2024-01-02 09:30:00"),
        //        CompletedDatetime = DT("2024-01-02 10:00:00"),
        //        Status = ServiceStatus.実行済,
        //        Content = "Test2",
        //        ServiceExecuteId = 2
        //    });
            
        //    await db.SaveChangesAsync();

        //    var model = CreateModel();
        //    model.Condition = new SearchCondition();

        //    // Act
        //    var result = await model.OnPostSearchAsync();

        //    // Assert
        //    var gridJson = (result as JsonResult)?.Value as GridJson<SearchGridModel>;
        //    Assert.IsNotNull(gridJson, "GridJson が null です");
        //    var dataList = gridJson.Data.ToList();
        //    Assert.HasCount(2, dataList, "全データが取得されていません");
            
        //    // 完了日時の降順であることを確認
        //    Assert.IsGreaterThanOrEqualTo(
        //        0,
        //        dataList[0].DisplayCompletedDate.CompareTo(dataList[1].DisplayCompletedDate), "完了日時の降順でソートされていません"
        //    );
        //}

        ///// <summary>
        ///// エラーケース: 存在しないServiceExecuteに紐付くServiceExecuteHistoryが存在する場合
        ///// （外部キー制約で実際には発生しないが、データベース設計の検証のためのテスト）
        ///// </summary>
        //[TestMethod]
        //public async Task OnPostSearchAsync_Handles_Orphaned_ServiceExecuteHistory()
        //{
        //    // Arrange
        //    var serviceExecute1 = new ServiceExecute
        //    {
        //        Id = 1,
        //        Used = true,
        //        RequestDatetime = DT("2024-01-01 09:00:00"),
        //        CompletedDatetime = DT("2024-01-01 10:00:00"),
        //        Type = ServiceClassification.連携プログラム稼働
        //    };
            
        //    db.ServiceExecutes.Add(serviceExecute1);
            
        //    // 正常なHistory
        //    db.ServiceExecuteHistories.Add(new ServiceExecuteHistory
        //    {
        //        Id = 1,
        //        RequestDatetime = DT("2024-01-01 09:00:00"),
        //        ExecuteDatetime = DT("2024-01-01 09:30:00"),
        //        CompletedDatetime = DT("2024-01-01 10:00:00"),
        //        Status = ServiceStatus.実行済,
        //        Content = "Test1",
        //        ServiceExecuteId = 1
        //    });
            
        //    await db.SaveChangesAsync();

        //    var model = CreateModel();
        //    model.Condition = new SearchCondition();

        //    // Act
        //    var result = await model.OnPostSearchAsync();

        //    // Assert
        //    var gridJson = (result as JsonResult)?.Value as GridJson<SearchGridModel>;
        //    Assert.IsNotNull(gridJson, "GridJson が null です");
        //    var dataList = gridJson.Data.ToList();
        //    Assert.HasCount(1, dataList, "正常なデータが取得されるべき");
        //}

        ///// <summary>
        ///// 境界条件: 日付範囲フィルタリングの境界条件テストを修正
        ///// 同じ日付の翌日を終了日として指定した場合（日付範囲指定の正しい使用法）
        ///// </summary>
        //[TestMethod]
        //public async Task OnPostSearchAsync_Filters_With_Date_Range_Inclusive()
        //{
        //    // Arrange
        //    var serviceExecute1 = new ServiceExecute
        //    {
        //        Id = 1,
        //        Used = true,
        //        RequestDatetime = DT("2024-01-10 09:00:00"),
        //        CompletedDatetime = DT("2024-01-10 10:00:00"),
        //        Type = ServiceClassification.連携プログラム稼働
        //    };
        //    var serviceExecute2 = new ServiceExecute
        //    {
        //        Id = 2,
        //        Used = true,
        //        RequestDatetime = DT("2024-01-11 09:00:00"),
        //        CompletedDatetime = DT("2024-01-11 10:00:00"),
        //        Type = ServiceClassification.連携プログラム稼働
        //    };
        //    var serviceExecute3 = new ServiceExecute
        //    {
        //        Id = 3,
        //        Used = true,
        //        RequestDatetime = DT("2024-01-12 09:00:00"),
        //        CompletedDatetime = DT("2024-01-12 10:00:00"),
        //        Type = ServiceClassification.連携プログラム稼働
        //    };
            
        //    db.ServiceExecutes.Add(serviceExecute1);
        //    db.ServiceExecutes.Add(serviceExecute2);
        //    db.ServiceExecutes.Add(serviceExecute3);
            
        //    db.ServiceExecuteHistories.Add(new ServiceExecuteHistory
        //    {
        //        Id = 1,
        //        RequestDatetime = DT("2024-01-10 09:00:00"),
        //        ExecuteDatetime = DT("2024-01-10 09:30:00"),
        //        CompletedDatetime = DT("2024-01-10 10:00:00"),
        //        Status = ServiceStatus.実行済,
        //        Content = "Test1",
        //        ServiceExecuteId = 1
        //    });
            
        //    db.ServiceExecuteHistories.Add(new ServiceExecuteHistory
        //    {
        //        Id = 2,
        //        RequestDatetime = DT("2024-01-11 09:00:00"),
        //        ExecuteDatetime = DT("2024-01-11 09:30:00"),
        //        CompletedDatetime = DT("2024-01-11 10:00:00"),
        //        Status = ServiceStatus.実行済,
        //        Content = "Test2",
        //        ServiceExecuteId = 2
        //    });
            
        //    db.ServiceExecuteHistories.Add(new ServiceExecuteHistory
        //    {
        //        Id = 3,
        //        RequestDatetime = DT("2024-01-12 09:00:00"),
        //        ExecuteDatetime = DT("2024-01-12 09:30:00"),
        //        CompletedDatetime = DT("2024-01-12 10:00:00"),
        //        Status = ServiceStatus.実行済,
        //        Content = "Test3",
        //        ServiceExecuteId = 3
        //    });
            
        //    await db.SaveChangesAsync();

        //    var model = CreateModel();
        //    // 2024-01-10～2024-01-11のデータを取得
        //    // （実装では CompletedDatetime <= val でチェックするため、
        //    // val を翌日の00:00:00に設定すると、その日付内のすべてのデータが取得される）
        //    model.Condition = new SearchCondition
        //    {
        //        RequestDateFrom = DT("2024-01-10"),
        //        CompletedDateTo = DT("2024-01-12")
        //    };

        //    // Act
        //    var result = await model.OnPostSearchAsync();

        //    // Assert
        //    var gridJson = (result as JsonResult)?.Value as GridJson<SearchGridModel>;
        //    Assert.IsNotNull(gridJson, "GridJson が null です");
        //    var dataList = gridJson.Data.ToList();
        //    Assert.HasCount(2, dataList, "2024-01-10と2024-01-11のデータが取得されるべき");
        //}

        ///// <summary>
        ///// 境界条件: Contentがnullの場合、正しく表示されること
        ///// </summary>
        //[TestMethod]
        //public async Task OnPostSearchAsync_Handles_Null_Content()
        //{
        //    // Arrange
        //    var serviceExecute1 = new ServiceExecute
        //    {
        //        Id = 1,
        //        Used = true,
        //        RequestDatetime = DT("2024-01-01 09:00:00"),
        //        CompletedDatetime = DT("2024-01-01 10:00:00"),
        //        Type = ServiceClassification.連携プログラム稼働
        //    };
            
        //    db.ServiceExecutes.Add(serviceExecute1);
            
        //    // Contentがnull
        //    db.ServiceExecuteHistories.Add(new ServiceExecuteHistory
        //    {
        //        Id = 1,
        //        RequestDatetime = DT("2024-01-01 09:00:00"),
        //        ExecuteDatetime = DT("2024-01-01 09:30:00"),
        //        CompletedDatetime = DT("2024-01-01 10:00:00"),
        //        Status = ServiceStatus.実行済,
        //        Content = null,
        //        ServiceExecuteId = 1
        //    });
            
        //    await db.SaveChangesAsync();

        //    var model = CreateModel();
        //    model.Condition = new SearchCondition();

        //    // Act
        //    var result = await model.OnPostSearchAsync();

        //    // Assert
        //    var gridJson = (result as JsonResult)?.Value as GridJson<SearchGridModel>;
        //    Assert.IsNotNull(gridJson, "GridJson が null です");
        //    var dataList = gridJson.Data.ToList();
        //    Assert.HasCount(1, dataList, "データが取得されるべき");
        //    Assert.IsNull(dataList[0].Content, "Contentはnullであるべき");
        //}

        ///// <summary>
        ///// 大量データ: 大量のレコードが存在する場合のパフォーマンステスト
        ///// </summary>
        //[TestMethod]
        //public async Task OnPostSearchAsync_Handles_Large_Dataset()
        //{
        //    // Arrange
        //    const int RecordCount = 1000;
            
        //    // 大量のServiceExecuteを作成
        //    for (int i = 1; i <= RecordCount; i++)
        //    {
        //        var serviceExecute = new ServiceExecute
        //        {
        //            Id = i,
        //            Used = true,
        //            RequestDatetime = DT("2024-01-01 09:00:00").AddMinutes(i),
        //            CompletedDatetime = DT("2024-01-01 09:30:00").AddMinutes(i),
        //            Type = i % 2 == 0 ? ServiceClassification.連携プログラム稼働 : ServiceClassification.過労運転防止
        //        };
        //        db.ServiceExecutes.Add(serviceExecute);
                
        //        // 大量のServiceExecuteHistoryを作成
        //        db.ServiceExecuteHistories.Add(new ServiceExecuteHistory
        //        {
        //            Id = i,
        //            RequestDatetime = DT("2024-01-01 09:00:00").AddMinutes(i),
        //            ExecuteDatetime = DT("2024-01-01 09:15:00").AddMinutes(i),
        //            CompletedDatetime = DT("2024-01-01 09:30:00").AddMinutes(i),
        //            Status = i % 3 == 0 ? ServiceStatus.エラー : ServiceStatus.実行済,
        //            Content = $"Test{i}",
        //            ServiceExecuteId = i
        //        });
        //    }
            
        //    await db.SaveChangesAsync();

        //    var model = CreateModel();
        //    model.Condition = new SearchCondition();

        //    // Act
        //    var startTime = DateTime.Now;
        //    var result = await model.OnPostSearchAsync();
        //    var executionTime = DateTime.Now - startTime;

        //    // Assert
        //    var gridJson = (result as JsonResult)?.Value as GridJson<SearchGridModel>;
        //    Assert.IsNotNull(gridJson, "GridJson が null です");
        //    Assert.AreEqual(RecordCount, gridJson.ItemsCount, "全レコードが取得されるべき");
        //    // 性能確認: 1000件のデータ取得が1秒以内に完了すること
        //    Assert.IsLessThan(2, executionTime.TotalSeconds, $"パフォーマンスが低い: {executionTime.TotalSeconds}秒");
        //}

        ///// <summary>
        ///// 境界条件: 大量データの中から特定の条件で絞り込む場合
        ///// </summary>
        //[TestMethod]
        //public async Task OnPostSearchAsync_Filters_Large_Dataset_By_Status()
        //{
        //    // Arrange
        //    const int RecordCount = 500;
            
        //    for (int i = 1; i <= RecordCount; i++)
        //    {
        //        var serviceExecute = new ServiceExecute
        //        {
        //            Id = i,
        //            Used = true,
        //            RequestDatetime = DT("2024-01-01 09:00:00").AddMinutes(i),
        //            CompletedDatetime = DT("2024-01-01 09:30:00").AddMinutes(i),
        //            Type = ServiceClassification.連携プログラム稼働
        //        };
        //        db.ServiceExecutes.Add(serviceExecute);
                
        //        db.ServiceExecuteHistories.Add(new ServiceExecuteHistory
        //        {
        //            Id = i,
        //            RequestDatetime = DT("2024-01-01 09:00:00").AddMinutes(i),
        //            ExecuteDatetime = DT("2024-01-01 09:15:00").AddMinutes(i),
        //            CompletedDatetime = DT("2024-01-01 09:30:00").AddMinutes(i),
        //            Status = i % 2 == 0 ? ServiceStatus.実行済 : ServiceStatus.エラー,
        //            Content = $"Test{i}",
        //            ServiceExecuteId = i
        //        });
        //    }
            
        //    await db.SaveChangesAsync();

        //    var model = CreateModel();
        //    model.Condition = new SearchCondition { Status = ServiceStatus.実行済 };

        //    // Act
        //    var result = await model.OnPostSearchAsync();

        //    // Assert
        //    var gridJson = (result as JsonResult)?.Value as GridJson<SearchGridModel>;
        //    Assert.IsNotNull(gridJson, "GridJson が null です");
        //    var dataList = gridJson.Data.ToList();
        //    // 偶数件数が取得されるべき（500/2 = 250）
        //    Assert.AreEqual(RecordCount / 2, gridJson.ItemsCount, "フィルタリングされたレコード数が正しくないです");
        //    // すべてのレコードがステータス「実行済」であることを確認
        //    Assert.IsTrue(dataList.All(x => x.Status == "実行済"), "すべてのレコードがステータス「実行済」であるべき");
        //}

        ///// <summary>
        ///// 境界条件: 空のデータベースで検索した場合
        ///// </summary>
        //[TestMethod]
        //public async Task OnPostSearchAsync_Returns_Empty_When_Database_Is_Empty()
        //{
        //    // Arrange
        //    var model = CreateModel();
        //    model.Condition = new SearchCondition();

        //    // Act
        //    var result = await model.OnPostSearchAsync();

        //    // Assert
        //    var gridJson = (result as JsonResult)?.Value as GridJson<SearchGridModel>;
        //    Assert.IsNotNull(gridJson, "GridJson が null です");
        //    var dataList = gridJson.Data.ToList();
        //    Assert.IsEmpty(dataList, "空のリストが返されるべき");
        //    Assert.AreEqual(0, gridJson.ItemsCount, "ItemsCount が 0 であるべき");
        //}

        ///// <summary>
        ///// 境界条件: RequestDateFromのみ指定した場合
        ///// </summary>
        //[TestMethod]
        //public async Task OnPostSearchAsync_Filters_By_RequestDateFrom_Only()
        //{
        //    // Arrange
        //    var serviceExecute1 = new ServiceExecute
        //    {
        //        Id = 1,
        //        Used = true,
        //        RequestDatetime = DT("2024-01-01 09:00:00"),
        //        CompletedDatetime = DT("2024-01-01 10:00:00"),
        //        Type = ServiceClassification.連携プログラム稼働
        //    };
        //    var serviceExecute2 = new ServiceExecute
        //    {
        //        Id = 2,
        //        Used = true,
        //        RequestDatetime = DT("2024-01-15 09:00:00"),
        //        CompletedDatetime = DT("2024-01-15 10:00:00"),
        //        Type = ServiceClassification.連携プログラム稼働
        //    };
            
        //    db.ServiceExecutes.Add(serviceExecute1);
        //    db.ServiceExecutes.Add(serviceExecute2);
            
        //    db.ServiceExecuteHistories.Add(new ServiceExecuteHistory
        //    {
        //        Id = 1,
        //        RequestDatetime = DT("2024-01-01 09:00:00"),
        //        ExecuteDatetime = DT("2024-01-01 09:30:00"),
        //        CompletedDatetime = DT("2024-01-01 10:00:00"),
        //        Status = ServiceStatus.実行済,
        //        Content = "Test1",
        //        ServiceExecuteId = 1
        //    });
            
        //    db.ServiceExecuteHistories.Add(new ServiceExecuteHistory
        //    {
        //        Id = 2,
        //        RequestDatetime = DT("2024-01-15 09:00:00"),
        //        ExecuteDatetime = DT("2024-01-15 09:30:00"),
        //        CompletedDatetime = DT("2024-01-15 10:00:00"),
        //        Status = ServiceStatus.実行済,
        //        Content = "Test2",
        //        ServiceExecuteId = 2
        //    });
            
        //    await db.SaveChangesAsync();

        //    var model = CreateModel();
        //    // RequestDateFromのみ指定
        //    model.Condition = new SearchCondition { RequestDateFrom = DT("2024-01-10") };

        //    // Act
        //    var result = await model.OnPostSearchAsync();

        //    // Assert
        //    var gridJson = (result as JsonResult)?.Value as GridJson<SearchGridModel>;
        //    Assert.IsNotNull(gridJson, "GridJson が null です");
        //    var dataList = gridJson.Data.ToList();
        //    Assert.HasCount(1, dataList, "2024-01-10以降のデータのみ取得されるべき");
        //    Assert.AreEqual("2024/01/15", dataList[0].DisplayCompletedDate.Split(" ")[0], "2024-01-15のデータが取得されるべき");
        //}

        ///// <summary>
        ///// 境界条件: CompletedDateToのみ指定した場合
        ///// </summary>
        //[TestMethod]
        //public async Task OnPostSearchAsync_Filters_By_CompletedDateTo_Only()
        //{
        //    // Arrange
        //    var serviceExecute1 = new ServiceExecute
        //    {
        //        Id = 1,
        //        Used = true,
        //        RequestDatetime = DT("2024-01-01 09:00:00"),
        //        CompletedDatetime = DT("2024-01-01 10:00:00"),
        //        Type = ServiceClassification.連携プログラム稼働
        //    };
        //    var serviceExecute2 = new ServiceExecute
        //    {
        //        Id = 2,
        //        Used = true,
        //        RequestDatetime = DT("2024-01-15 09:00:00"),
        //        CompletedDatetime = DT("2024-01-15 10:00:00"),
        //        Type = ServiceClassification.連携プログラム稼働
        //    };
            
        //    db.ServiceExecutes.Add(serviceExecute1);
        //    db.ServiceExecutes.Add(serviceExecute2);
            
        //    db.ServiceExecuteHistories.Add(new ServiceExecuteHistory
        //    {
        //        Id = 1,
        //        RequestDatetime = DT("2024-01-01 09:00:00"),
        //        ExecuteDatetime = DT("2024-01-01 09:30:00"),
        //        CompletedDatetime = DT("2024-01-01 10:00:00"),
        //        Status = ServiceStatus.実行済,
        //        Content = "Test1",
        //        ServiceExecuteId = 1
        //    });
            
        //    db.ServiceExecuteHistories.Add(new ServiceExecuteHistory
        //    {
        //        Id = 2,
        //        RequestDatetime = DT("2024-01-15 09:00:00"),
        //        ExecuteDatetime = DT("2024-01-15 09:30:00"),
        //        CompletedDatetime = DT("2024-01-15 10:00:00"),
        //        Status = ServiceStatus.実行済,
        //        Content = "Test2",
        //        ServiceExecuteId = 2
        //    });
            
        //    await db.SaveChangesAsync();

        //    var model = CreateModel();
        //    // CompletedDateToのみ指定
        //    model.Condition = new SearchCondition { CompletedDateTo = DT("2024-01-10") };

        //    // Act
        //    var result = await model.OnPostSearchAsync();

        //    // Assert
        //    var gridJson = (result as JsonResult)?.Value as GridJson<SearchGridModel>;
        //    Assert.IsNotNull(gridJson, "GridJson が null です");
        //    var dataList = gridJson.Data.ToList();
        //    Assert.HasCount(1, dataList, "2024-01-10以前のデータのみ取得されるべき");
        //    Assert.AreEqual("2024/01/01", dataList[0].DisplayCompletedDate.Split(" ")[0], "2024-01-01のデータが取得されるべき");
        //}
    }
}
