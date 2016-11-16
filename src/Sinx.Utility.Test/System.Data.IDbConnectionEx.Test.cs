using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Sinx.Utility.Extension;
using Xunit;
using Dapper;

namespace Sinx.Utility.Test
{
	// ReSharper disable once InconsistentNaming
	public class IDbConnectionExTest
	{
		private static readonly IDbConnection Db = new SqlConnection("Data Source=neter.me;Initial Catalog=DbDotNetStudio;User Id=sa;Password=Xiong2015");

		/// <summary>
		/// 测试能否根据条件查到值
		/// </summary>
		[Fact]
		public void GetAsync_ShouldReturnSpecifiedEntity()
		{
			// 没有条件, 查询所有
			var models = Db.GetAsync(new TblUser()).Result.ToList();
			Assert.NotEmpty(models);
			Assert.Equal(models.Count, Db.ExecuteScalar<int>($"SELECT Count(*) FROM {nameof(TblUser)}"));
			// 单个条件 - 条件不成立 - 查询空集
			var models0 = Db.GetAsync(new TblUser { CreateTime = DateTime.Now }).Result;	// TODO Expression Explain Func Develop eg: m => m.CreateTime > DateTime.Now
			Assert.Empty(models0);
			// 单个条件 - 条件是默认值 - 查询出指定集合
			var models1 = Db.GetAsync(new TblUser(), nameof(TblUser.IsDelete)).Result.ToList();
			Assert.NotEmpty(models1);
			Assert.All(models1, m => m.IsDelete = default(bool));
			Assert.Equal(models1.Count, Db.ExecuteScalar<int>($"SELECT Count(*) FROM {nameof(TblUser)} AS T WHERE T.IsDelete = 0"));
			var models2 = Db.GetAsync(new TblUser(), 0, 2).Result.ToList();
			var models2Temp = Db.Query<TblUser>("SELECT TOP 2 * FROM dbo.TblUser AS T ORDER BY T.Id").ToList();
			Assert.Equal(models2.Count, models2Temp.Count);
			Assert.True(models2.All(m => models2Temp.Any(o => o.Id == m.Id)));
		}


		private class TblUser
		{
			public int Id { get; set; }
			public string Name { get; set; }
			public string Email { get; set; }
			public bool IsDelete { get; set; }
			// ReSharper disable once UnusedAutoPropertyAccessor.Local
			public DateTime CreateTime { get; set; }
		}
	}
}
