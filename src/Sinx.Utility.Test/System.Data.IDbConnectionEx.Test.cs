using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Sinx.Utility.Extension;
using Xunit;

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
			var models0 = Db.GetAsync(new TblUser { CreateTime = DateTime.Now }).Result;	// TODO Expression Explain Func Develop eg: m => m.CreateTime > DateTime.Now
			Assert.Empty(models0);
			var models1 = Db.GetAsync(new TblUser(), nameof(TblUser.IsDelete)).Result.ToList();
			Assert.NotEmpty(models1);
			Assert.All(models1, m => m.IsDelete = default(bool));
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
